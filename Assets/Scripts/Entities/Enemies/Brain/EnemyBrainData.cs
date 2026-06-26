using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Entities.Enemies
{
    [CreateAssetMenu(fileName = "EnemyBrainData", menuName = "Game/Enemy Brain/Brain Data")]
    public class EnemyBrainData : ScriptableObject
    {
        public const string PlanNodeTitle = "Plan";
        public const string RuleOutputName = "Out";
        public const string ConditionTrueOutputName = "True";
        public const string ConditionFalseOutputName = "False";

        public string startNodeGuid;
        public List<EnemyBrainNodeData> nodes = new();
        public List<EnemyBrainConnectionData> connections = new();

        private const int MaxTraversalSteps = 256;

        private void OnValidate()
        {
            EnsurePlanNode();
        }

        public EnemyBrainNodeData EnsurePlanNode()
        {
            nodes ??= new List<EnemyBrainNodeData>();
            connections ??= new List<EnemyBrainConnectionData>();
            nodes.RemoveAll(node => node == null);
            connections.RemoveAll(connection => connection == null);
            EnsureNodeIdsAndTitles();

            EnemyBrainNodeData planNode = null;
            if (!string.IsNullOrEmpty(startNodeGuid))
                planNode = nodes.FirstOrDefault(node => node.guid == startNodeGuid);

            planNode ??= nodes.FirstOrDefault(node => node.type == EnemyBrainNodeType.Start);

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

            planNode.type = EnemyBrainNodeType.Start;
            planNode.title = PlanNodeTitle;
            planNode.rule = null;
            planNode.condition = null;
            startNodeGuid = planNode.guid;
            RemoveExtraStartNodes(planNode);

            return planNode;
        }

        private void RemoveExtraStartNodes(EnemyBrainNodeData planNode)
        {
            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                EnemyBrainNodeData node = nodes[i];
                if (node == null || node == planNode || node.type != EnemyBrainNodeType.Start)
                    continue;

                connections.RemoveAll(connection =>
                    connection == null ||
                    connection.fromNodeGuid == node.guid ||
                    connection.toNodeGuid == node.guid);
                nodes.RemoveAt(i);
            }
        }

        private void EnsureNodeIdsAndTitles()
        {
            foreach (EnemyBrainNodeData node in nodes)
            {
                if (node == null)
                    continue;

                if (string.IsNullOrEmpty(node.guid))
                    node.guid = Guid.NewGuid().ToString();

                if (!string.IsNullOrEmpty(node.title))
                    continue;

                node.title = node.type switch
                {
                    EnemyBrainNodeType.Start => PlanNodeTitle,
                    EnemyBrainNodeType.Rule => node.rule != null ? node.rule.name : "Rule",
                    EnemyBrainNodeType.Condition => node.condition != null ? node.condition.name : "Condition",
                    _ => "Node"
                };
            }
        }

        public bool Plan(EnemyTurnContext context)
        {
            if (context == null)
                return false;

            EnsurePlanNode();
            EnemyBrainNodeData startNode = GetStartNode();
            if (startNode == null)
                return false;

            Dictionary<string, EnemyBrainNodeData> nodeLookup = new Dictionary<string, EnemyBrainNodeData>();
            foreach (EnemyBrainNodeData node in nodes)
            {
                if (node == null || string.IsNullOrEmpty(node.guid) || nodeLookup.ContainsKey(node.guid))
                    continue;

                nodeLookup[node.guid] = node;
            }

            return TraverseBreadthFirst(startNode, context, nodeLookup);
        }

        private EnemyBrainNodeData GetStartNode()
        {
            if (nodes == null || nodes.Count == 0)
                return null;

            if (!string.IsNullOrEmpty(startNodeGuid))
            {
                EnemyBrainNodeData node = nodes.FirstOrDefault(entry => entry.guid == startNodeGuid);
                if (node != null)
                    return node;
            }

            return nodes.FirstOrDefault(node => node.type == EnemyBrainNodeType.Start);
        }

        private bool TraverseBreadthFirst(
            EnemyBrainNodeData startNode,
            EnemyTurnContext context,
            Dictionary<string, EnemyBrainNodeData> nodeLookup)
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
                        EnqueueOutgoing(queue, node, nodeLookup, nextPath, RuleOutputName);
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
                            string outputName = node.condition.IsMet(context)
                                ? ConditionTrueOutputName
                                : ConditionFalseOutputName;
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
    }

    [Serializable]
    public class EnemyBrainConnectionData
    {
        public string fromNodeGuid;
        public string toNodeGuid;

        // Stores the branch name for condition outputs like "True" or "False".
        public string outputName;
    }

    public enum EnemyBrainNodeType
    {
        Start,
        Rule,
        Condition
    }
}
