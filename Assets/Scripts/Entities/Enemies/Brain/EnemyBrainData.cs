using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Entities.Enemies
{
    [CreateAssetMenu(fileName = "EnemyBrainData", menuName = "Game/Enemy Brain/Brain Data")]
    public class EnemyBrainData : ScriptableObject
    {
        public const string PlanNodeTitle = "Plan";
        public const string PrePlanNodeTitle = "Pre Plan";
        public const string PrePlanSelectorNodeTitle = "Preplan";
        public const string RuleOutputName = "Out";
        public const string ConditionTrueOutputName = "True";
        public const string ConditionFalseOutputName = "False";
        public const string DefaultPlanOutputName = "None";

        public string startNodeGuid;
        public string prePlanStartNodeGuid;
        public List<EnemyBrainNodeData> nodes = new();
        public List<EnemyBrainConnectionData> connections = new();

        private const int MaxTraversalSteps = 256;

        private void OnValidate()
        {
            EnsurePlanNodes();
        }

        // Kept as the public compatibility entry point for existing editor/runtime callers.
        public EnemyBrainNodeData EnsurePlanNode()
        {
            EnsurePlanNodes();
            return GetStartNode(startNodeGuid, EnemyBrainNodeType.Start);
        }

        public EnemyBrainNodeData EnsurePrePlanNode()
        {
            EnsurePlanNodes();
            return GetStartNode(prePlanStartNodeGuid, EnemyBrainNodeType.PrePlanStart);
        }

        public void EnsurePlanNodes()
        {
            nodes ??= new List<EnemyBrainNodeData>();
            connections ??= new List<EnemyBrainConnectionData>();
            nodes.RemoveAll(node => node == null);
            connections.RemoveAll(connection => connection == null);
            EnsureNodeIdsAndTitles();

            EnemyBrainNodeData planNode = GetStartNode(startNodeGuid, EnemyBrainNodeType.Start);
            if (planNode == null)
            {
                planNode = new EnemyBrainNodeData
                {
                    guid = Guid.NewGuid().ToString(),
                    title = PlanNodeTitle,
                    type = EnemyBrainNodeType.Start,
                    editorPosition = new Vector2(100f, 200f)
                };

                nodes.Insert(0, planNode);
            }

            NormalizeStartNode(planNode, EnemyBrainNodeType.Start, PlanNodeTitle);
            startNodeGuid = planNode.guid;
            RemoveExtraStartNodes(planNode, EnemyBrainNodeType.Start);

            EnemyBrainNodeData prePlanNode = GetStartNode(
                prePlanStartNodeGuid,
                EnemyBrainNodeType.PrePlanStart);
            if (prePlanNode == null)
            {
                prePlanNode = new EnemyBrainNodeData
                {
                    guid = Guid.NewGuid().ToString(),
                    title = PrePlanNodeTitle,
                    type = EnemyBrainNodeType.PrePlanStart,
                    editorPosition = planNode.editorPosition + new Vector2(0f, -180f)
                };

                nodes.Insert(0, prePlanNode);
            }

            NormalizeStartNode(prePlanNode, EnemyBrainNodeType.PrePlanStart, PrePlanNodeTitle);
            prePlanStartNodeGuid = prePlanNode.guid;
            RemoveExtraStartNodes(prePlanNode, EnemyBrainNodeType.PrePlanStart);
            MigrateLegacyPlanConnections(planNode);
            CanonicalizeImplicitOutputNames();
            RemoveDuplicateConnections();
        }

        private void NormalizeStartNode(
            EnemyBrainNodeData node,
            EnemyBrainNodeType type,
            string nodeTitle)
        {
            node.type = type;
            node.title = nodeTitle;
            node.rule = null;
            node.condition = null;
            node.ClearPrePlanOption();
        }

        private void RemoveExtraStartNodes(
            EnemyBrainNodeData retainedNode,
            EnemyBrainNodeType startType)
        {
            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                EnemyBrainNodeData node = nodes[i];
                if (node == null || node == retainedNode || node.type != startType)
                    continue;

                connections.RemoveAll(connection =>
                    connection == null ||
                    connection.fromNodeGuid == node.guid ||
                    connection.toNodeGuid == node.guid);
                nodes.RemoveAt(i);
            }
        }

        private void MigrateLegacyPlanConnections(EnemyBrainNodeData planNode)
        {
            HashSet<string> planOutputNames = new HashSet<string>(GetPlanOutputNames());
            foreach (EnemyBrainConnectionData connection in connections)
            {
                if (connection == null || connection.fromNodeGuid != planNode.guid)
                    continue;

                if (string.IsNullOrEmpty(connection.outputName) ||
                    connection.outputName == RuleOutputName &&
                    !planOutputNames.Contains(RuleOutputName))
                {
                    connection.outputName = DefaultPlanOutputName;
                }
            }
        }

        private void EnsureNodeIdsAndTitles()
        {
            HashSet<string> usedGuids = new HashSet<string>();
            foreach (EnemyBrainNodeData node in nodes)
            {
                if (node == null)
                    continue;

                if (string.IsNullOrEmpty(node.guid) || !usedGuids.Add(node.guid))
                {
                    do
                    {
                        node.guid = Guid.NewGuid().ToString();
                    } while (!usedGuids.Add(node.guid));
                }

                if (node.type == EnemyBrainNodeType.PrePlan)
                    node.MigrateLegacyPrePlanOption();
                else
                    node.ClearPrePlanOption();

                if (!string.IsNullOrEmpty(node.title))
                    continue;

                node.title = node.type switch
                {
                    EnemyBrainNodeType.Start => PlanNodeTitle,
                    EnemyBrainNodeType.Rule => node.rule != null ? node.rule.name : "Rule",
                    EnemyBrainNodeType.Condition => node.condition != null ? node.condition.name : "Condition",
                    EnemyBrainNodeType.PrePlanStart => PrePlanNodeTitle,
                    EnemyBrainNodeType.PrePlan => PrePlanSelectorNodeTitle,
                    EnemyBrainNodeType.Comment => "Comment",
                    _ => "Node"
                };
            }
        }

        private void RemoveDuplicateConnections()
        {
            HashSet<(string From, string To, string Output)> seenConnections =
                new HashSet<(string From, string To, string Output)>();

            connections.RemoveAll(connection =>
                connection == null ||
                !seenConnections.Add((
                    connection.fromNodeGuid,
                    connection.toNodeGuid,
                    connection.outputName)));
        }

        private void CanonicalizeImplicitOutputNames()
        {
            Dictionary<string, EnemyBrainNodeData> nodeLookup = BuildNodeLookup();
            foreach (EnemyBrainConnectionData connection in connections)
            {
                if (connection == null ||
                    !string.IsNullOrEmpty(connection.outputName) ||
                    !nodeLookup.TryGetValue(connection.fromNodeGuid, out EnemyBrainNodeData sourceNode))
                {
                    continue;
                }

                if (sourceNode.type == EnemyBrainNodeType.Rule ||
                    sourceNode.type == EnemyBrainNodeType.PrePlanStart)
                {
                    connection.outputName = RuleOutputName;
                }
            }
        }

        public bool PrePlan(EnemyTurnContext context, out string prePlanOption)
        {
            prePlanOption = string.Empty;
            if (context == null)
                return false;

            EnsurePlanNodes();
            EnemyBrainNodeData startNode = GetStartNode(
                prePlanStartNodeGuid,
                EnemyBrainNodeType.PrePlanStart);
            if (startNode == null)
                return false;

            Dictionary<string, EnemyBrainNodeData> nodeLookup = BuildNodeLookup();
            return TryTraversePrePlanBreadthFirst(startNode, context, nodeLookup, out prePlanOption);
        }

        public bool Plan(EnemyTurnContext context)
        {
            return Plan(context, string.Empty);
        }

        public bool Plan(EnemyTurnContext context, string prePlanOption)
        {
            if (context == null)
                return false;

            EnsurePlanNodes();
            EnemyBrainNodeData startNode = GetStartNode(startNodeGuid, EnemyBrainNodeType.Start);
            if (startNode == null)
                return false;

            Dictionary<string, EnemyBrainNodeData> nodeLookup = BuildNodeLookup();
            string outputName = GetPlanOutputName(prePlanOption);
            return TraversePlanBreadthFirst(startNode, context, nodeLookup, outputName);
        }

        public IReadOnlyList<string> GetPlanOutputNames()
        {
            List<string> outputNames = new List<string> { DefaultPlanOutputName };
            HashSet<string> seenOutputNames = new HashSet<string>(outputNames, StringComparer.Ordinal);

            if (nodes == null)
                return outputNames;

            foreach (EnemyBrainNodeData node in nodes)
            {
                if (node == null || node.type != EnemyBrainNodeType.PrePlan)
                    continue;

                string option = NormalizePrePlanOption(node.prePlanOption);
                if (!string.IsNullOrEmpty(option) && seenOutputNames.Add(option))
                    outputNames.Add(option);
            }

            return outputNames;
        }

        public void SetPrePlanOption(EnemyBrainNodeData node, string option)
        {
            if (node == null || node.type != EnemyBrainNodeType.PrePlan)
                return;

            string previousOption = NormalizePrePlanOption(node.prePlanOption);
            string normalizedOption = NormalizePrePlanOption(option);
            node.SetPrePlanOption(normalizedOption);

            if (previousOption == normalizedOption ||
                string.IsNullOrEmpty(previousOption) ||
                previousOption == DefaultPlanOutputName)
            {
                return;
            }

            bool previousOptionStillUsed = nodes != null && nodes.Any(otherNode =>
                otherNode != null &&
                otherNode != node &&
                otherNode.type == EnemyBrainNodeType.PrePlan &&
                NormalizePrePlanOption(otherNode.prePlanOption) == previousOption);
            if (previousOptionStillUsed || connections == null)
                return;

            EnemyBrainNodeData planNode = GetStartNode(startNodeGuid, EnemyBrainNodeType.Start);
            if (planNode == null)
                return;

            if (string.IsNullOrEmpty(normalizedOption))
            {
                connections.RemoveAll(connection =>
                    connection != null &&
                    connection.fromNodeGuid == planNode.guid &&
                    connection.outputName == previousOption);
                return;
            }

            foreach (EnemyBrainConnectionData connection in connections)
            {
                if (connection != null &&
                    connection.fromNodeGuid == planNode.guid &&
                    connection.outputName == previousOption)
                {
                    connection.outputName = normalizedOption;
                }
            }

            RemoveDuplicateConnections();
        }

        public static string GetPlanOutputName(string prePlanOption)
        {
            string normalizedOption = NormalizePrePlanOption(prePlanOption);
            return string.IsNullOrEmpty(normalizedOption)
                ? DefaultPlanOutputName
                : normalizedOption;
        }

        public static string NormalizePrePlanOption(string prePlanOption)
        {
            return prePlanOption?.Trim() ?? string.Empty;
        }

        private Dictionary<string, EnemyBrainNodeData> BuildNodeLookup()
        {
            Dictionary<string, EnemyBrainNodeData> nodeLookup =
                new Dictionary<string, EnemyBrainNodeData>();

            foreach (EnemyBrainNodeData node in nodes)
            {
                if (node == null || string.IsNullOrEmpty(node.guid) || nodeLookup.ContainsKey(node.guid))
                    continue;

                nodeLookup[node.guid] = node;
            }

            return nodeLookup;
        }

        private EnemyBrainNodeData GetStartNode(string nodeGuid, EnemyBrainNodeType startType)
        {
            if (nodes == null || nodes.Count == 0)
                return null;

            if (!string.IsNullOrEmpty(nodeGuid))
            {
                EnemyBrainNodeData node = nodes.FirstOrDefault(entry =>
                    entry != null && entry.guid == nodeGuid && entry.type == startType);
                if (node != null)
                    return node;
            }

            return nodes.FirstOrDefault(node => node != null && node.type == startType);
        }

        private bool TryTraversePrePlanBreadthFirst(
            EnemyBrainNodeData startNode,
            EnemyTurnContext context,
            Dictionary<string, EnemyBrainNodeData> nodeLookup,
            out string prePlanOption)
        {
            prePlanOption = string.Empty;
            if (startNode == null || string.IsNullOrEmpty(startNode.guid))
                return false;

            int steps = 0;
            Queue<EnemyBrainTraversalFrame> queue = new Queue<EnemyBrainTraversalFrame>();
            queue.Enqueue(new EnemyBrainTraversalFrame(startNode, new HashSet<string>()));

            while (queue.Count > 0 && steps < MaxTraversalSteps)
            {
                EnemyBrainTraversalFrame frame = queue.Dequeue();
                EnemyBrainNodeData node = frame.Node;
                if (node == null || string.IsNullOrEmpty(node.guid) || frame.ActivePath.Contains(node.guid))
                    continue;

                steps++;
                HashSet<string> nextPath = new HashSet<string>(frame.ActivePath)
                {
                    node.guid
                };

                switch (node.type)
                {
                    case EnemyBrainNodeType.PrePlanStart:
                        EnqueueOutgoing(queue, node, nodeLookup, nextPath, RuleOutputName);
                        break;

                    case EnemyBrainNodeType.Condition:
                        if (node.condition != null)
                        {
                            string outputName = node.condition.SelectOutput(context, node.guid);
                            EnqueueOutgoing(queue, node, nodeLookup, nextPath, outputName);
                        }
                        break;

                    case EnemyBrainNodeType.PrePlan:
                        prePlanOption = NormalizePrePlanOption(node.prePlanOption);
                        return true;
                }
            }

            return false;
        }

        private bool TraversePlanBreadthFirst(
            EnemyBrainNodeData startNode,
            EnemyTurnContext context,
            Dictionary<string, EnemyBrainNodeData> nodeLookup,
            string startOutputName)
        {
            if (startNode == null || string.IsNullOrEmpty(startNode.guid))
                return false;

            int steps = 0;
            bool plannedAny = false;
            Queue<EnemyBrainTraversalFrame> queue = new Queue<EnemyBrainTraversalFrame>();
            queue.Enqueue(new EnemyBrainTraversalFrame(startNode, new HashSet<string>()));

            while (queue.Count > 0 && steps < MaxTraversalSteps)
            {
                EnemyBrainTraversalFrame frame = queue.Dequeue();
                EnemyBrainNodeData node = frame.Node;
                if (node == null || string.IsNullOrEmpty(node.guid) || frame.ActivePath.Contains(node.guid))
                    continue;

                steps++;
                HashSet<string> nextPath = new HashSet<string>(frame.ActivePath)
                {
                    node.guid
                };

                switch (node.type)
                {
                    case EnemyBrainNodeType.Start:
                        EnqueueOutgoing(queue, node, nodeLookup, nextPath, startOutputName);
                        break;

                    case EnemyBrainNodeType.Rule:
                        if (node.rule != null)
                        {
                            int revisionBefore = context.PlannedActionRevision;
                            bool rulePlanned = node.rule.TryPlan(context) ||
                                               context.PlannedActionRevision != revisionBefore;
                            if (rulePlanned)
                            {
                                plannedAny = true;
                                EnqueueOutgoing(queue, node, nodeLookup, nextPath, RuleOutputName);
                            }
                        }
                        break;

                    case EnemyBrainNodeType.Condition:
                        if (node.condition != null)
                        {
                            string outputName = node.condition.SelectOutput(context, node.guid);
                            EnqueueOutgoing(queue, node, nodeLookup, nextPath, outputName);
                        }
                        break;
                }
            }

            return plannedAny;
        }

        private void EnqueueOutgoing(
            Queue<EnemyBrainTraversalFrame> queue,
            EnemyBrainNodeData node,
            Dictionary<string, EnemyBrainNodeData> nodeLookup,
            HashSet<string> activePath,
            string outputName)
        {
            if (connections == null)
                return;

            foreach (EnemyBrainConnectionData connection in connections)
            {
                if (connection == null ||
                    connection.fromNodeGuid != node.guid ||
                    !OutputMatches(connection.outputName, outputName) ||
                    string.IsNullOrEmpty(connection.toNodeGuid) ||
                    !nodeLookup.TryGetValue(connection.toNodeGuid, out EnemyBrainNodeData nextNode))
                {
                    continue;
                }

                queue.Enqueue(new EnemyBrainTraversalFrame(nextNode, activePath));
            }
        }

        private bool OutputMatches(string connectionOutputName, string requestedOutputName)
        {
            if (string.IsNullOrEmpty(connectionOutputName))
                return string.IsNullOrEmpty(requestedOutputName) || requestedOutputName == RuleOutputName;

            return connectionOutputName == requestedOutputName;
        }

        private readonly struct EnemyBrainTraversalFrame
        {
            public EnemyBrainNodeData Node { get; }
            public HashSet<string> ActivePath { get; }

            public EnemyBrainTraversalFrame(EnemyBrainNodeData node, HashSet<string> activePath)
            {
                Node = node;
                ActivePath = activePath;
            }
        }
    }

    [Serializable]
    public class EnemyBrainNodeData
    {
        public string guid;
        public string title;
        public EnemyBrainNodeType type;
        public Vector2 editorPosition;

        public EnemyBrainRule rule;
        public EnemyBrainCondition condition;

        public string prePlanOption;
        [TextArea] public string comment;

        [FormerlySerializedAs("prePlanIntent")]
        [SerializeField, HideInInspector]
        private int legacyPrePlanIntent;

        internal void MigrateLegacyPrePlanOption()
        {
            prePlanOption = EnemyBrainData.NormalizePrePlanOption(prePlanOption);
            if (!string.IsNullOrEmpty(prePlanOption))
            {
                legacyPrePlanIntent = 0;
                return;
            }

            prePlanOption = legacyPrePlanIntent switch
            {
                1 => "Attacking",
                2 => "Moving",
                3 => "Blocking",
                _ => string.Empty
            };
            legacyPrePlanIntent = 0;
        }

        internal void SetPrePlanOption(string option)
        {
            prePlanOption = EnemyBrainData.NormalizePrePlanOption(option);
            legacyPrePlanIntent = 0;
        }

        internal void ClearPrePlanOption()
        {
            prePlanOption = string.Empty;
            legacyPrePlanIntent = 0;
        }
    }

    [Serializable]
    public class EnemyBrainConnectionData
    {
        public string fromNodeGuid;
        public string toNodeGuid;

        // Stores the selected branch name (condition, rule, or preplan output).
        public string outputName;
    }

    public enum EnemyBrainNodeType
    {
        // Existing values are serialized numerically in brain assets. Append new values only.
        Start = 0,
        Rule = 1,
        Condition = 2,
        PrePlanStart = 3,
        PrePlan = 4,
        Comment = 5
    }

}
