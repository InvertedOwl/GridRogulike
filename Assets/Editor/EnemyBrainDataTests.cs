using System.Collections.Generic;
using System.Linq;
using Entities.Enemies;
using NUnit.Framework;
using UnityEngine;

namespace GridRoguelike.EditorTools.Tests
{
    public class EnemyBrainDataTests
    {
        private readonly List<Object> createdObjects = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (Object createdObject in createdObjects)
            {
                if (createdObject != null)
                    Object.DestroyImmediate(createdObject);
            }

            createdObjects.Clear();
        }

        [Test]
        public void EnsurePlanNodesCreatesStablePlanAndPrePlanRoots()
        {
            EnemyBrainData brain = CreateBrain();

            brain.EnsurePlanNodes();
            string planGuid = brain.startNodeGuid;
            string prePlanGuid = brain.prePlanStartNodeGuid;

            brain.EnsurePlanNodes();

            Assert.That(brain.startNodeGuid, Is.EqualTo(planGuid));
            Assert.That(brain.prePlanStartNodeGuid, Is.EqualTo(prePlanGuid));
            Assert.That(
                brain.nodes.Count(node => node.type == EnemyBrainNodeType.Start),
                Is.EqualTo(1));
            Assert.That(
                brain.nodes.Count(node => node.type == EnemyBrainNodeType.PrePlanStart),
                Is.EqualTo(1));
        }

        [Test]
        public void EnsurePlanNodesMigratesLegacyOutAndRemovesEquivalentDuplicate()
        {
            EnemyBrainData brain = CreateBrain();
            EnemyBrainNodeData plan = CreateNode("plan", EnemyBrainNodeType.Start);
            EnemyBrainNodeData target = CreateNode("target", EnemyBrainNodeType.Condition);
            brain.startNodeGuid = plan.guid;
            brain.nodes.Add(plan);
            brain.nodes.Add(target);
            brain.connections.Add(CreateConnection(plan, target, EnemyBrainData.RuleOutputName));
            brain.connections.Add(CreateConnection(
                plan,
                target,
                EnemyBrainData.GetIntentOutputName(EnemyBrainIntent.None)));

            brain.EnsurePlanNodes();

            List<EnemyBrainConnectionData> migratedConnections = brain.connections
                .Where(connection =>
                    connection.fromNodeGuid == plan.guid && connection.toNodeGuid == target.guid)
                .ToList();
            Assert.That(migratedConnections, Has.Count.EqualTo(1));
            Assert.That(
                migratedConnections[0].outputName,
                Is.EqualTo(EnemyBrainData.GetIntentOutputName(EnemyBrainIntent.None)));
        }

        [Test]
        public void PrePlanUsesConditionBranchAndDoesNotCreateConcreteActions()
        {
            EnemyBrainData brain = CreateBrain();
            FixedEnemyBrainCondition condition = Track(
                ScriptableObject.CreateInstance<FixedEnemyBrainCondition>());

            EnemyBrainNodeData plan = CreateNode("plan", EnemyBrainNodeType.Start);
            EnemyBrainNodeData prePlan = CreateNode("pre-plan", EnemyBrainNodeType.PrePlanStart);
            EnemyBrainNodeData conditionNode = CreateNode("condition", EnemyBrainNodeType.Condition);
            EnemyBrainNodeData attacking = CreateNode("attacking", EnemyBrainNodeType.PrePlan);
            EnemyBrainNodeData blocking = CreateNode("blocking", EnemyBrainNodeType.PrePlan);
            conditionNode.condition = condition;
            attacking.prePlanIntent = EnemyBrainIntent.Attacking;
            blocking.prePlanIntent = EnemyBrainIntent.Blocking;

            brain.startNodeGuid = plan.guid;
            brain.prePlanStartNodeGuid = prePlan.guid;
            brain.nodes.AddRange(new[] { plan, prePlan, conditionNode, attacking, blocking });
            brain.connections.Add(CreateConnection(
                prePlan,
                conditionNode,
                EnemyBrainData.RuleOutputName));
            brain.connections.Add(CreateConnection(
                conditionNode,
                attacking,
                EnemyBrainData.ConditionTrueOutputName));
            brain.connections.Add(CreateConnection(
                conditionNode,
                blocking,
                EnemyBrainData.ConditionFalseOutputName));

            EnemyTurnContext context = new EnemyTurnContext(null, null, 0);
            condition.Result = true;
            Assert.That(brain.PrePlan(context, out EnemyBrainIntent trueIntent), Is.True);
            Assert.That(trueIntent, Is.EqualTo(EnemyBrainIntent.Attacking));
            Assert.That(context.PlannedActions, Is.Empty);

            condition.Result = false;
            Assert.That(brain.PrePlan(context, out EnemyBrainIntent falseIntent), Is.True);
            Assert.That(falseIntent, Is.EqualTo(EnemyBrainIntent.Blocking));
            Assert.That(context.PlannedActions, Is.Empty);
        }

        [Test]
        public void PlanTraversesOnlyTheSelectedIntentOutput()
        {
            EnemyBrainData brain = CreateBrain();
            RecordingEnemyBrainRule attackingRule = Track(
                ScriptableObject.CreateInstance<RecordingEnemyBrainRule>());
            RecordingEnemyBrainRule movingRule = Track(
                ScriptableObject.CreateInstance<RecordingEnemyBrainRule>());

            EnemyBrainNodeData plan = CreateNode("plan", EnemyBrainNodeType.Start);
            EnemyBrainNodeData attacking = CreateNode("attack-rule", EnemyBrainNodeType.Rule);
            EnemyBrainNodeData moving = CreateNode("move-rule", EnemyBrainNodeType.Rule);
            attacking.rule = attackingRule;
            moving.rule = movingRule;

            brain.startNodeGuid = plan.guid;
            brain.nodes.AddRange(new[] { plan, attacking, moving });
            brain.connections.Add(CreateConnection(
                plan,
                attacking,
                EnemyBrainData.GetIntentOutputName(EnemyBrainIntent.Attacking)));
            brain.connections.Add(CreateConnection(
                plan,
                moving,
                EnemyBrainData.GetIntentOutputName(EnemyBrainIntent.Moving)));

            Assert.That(
                brain.Plan(new EnemyTurnContext(null, null, 0), EnemyBrainIntent.Attacking),
                Is.True);
            Assert.That(attackingRule.CallCount, Is.EqualTo(1));
            Assert.That(movingRule.CallCount, Is.Zero);

            Assert.That(
                brain.Plan(new EnemyTurnContext(null, null, 0), EnemyBrainIntent.Moving),
                Is.True);
            Assert.That(attackingRule.CallCount, Is.EqualTo(1));
            Assert.That(movingRule.CallCount, Is.EqualTo(1));

            Assert.That(
                brain.Plan(new EnemyTurnContext(null, null, 0), EnemyBrainIntent.Blocking),
                Is.False);
            Assert.That(attackingRule.CallCount, Is.EqualTo(1));
            Assert.That(movingRule.CallCount, Is.EqualTo(1));
        }

        private EnemyBrainData CreateBrain()
        {
            EnemyBrainData brain = Track(ScriptableObject.CreateInstance<EnemyBrainData>());
            brain.startNodeGuid = null;
            brain.prePlanStartNodeGuid = null;
            brain.nodes = new List<EnemyBrainNodeData>();
            brain.connections = new List<EnemyBrainConnectionData>();
            return brain;
        }

        private T Track<T>(T createdObject) where T : Object
        {
            createdObjects.Add(createdObject);
            return createdObject;
        }

        private static EnemyBrainNodeData CreateNode(string guid, EnemyBrainNodeType type)
        {
            return new EnemyBrainNodeData
            {
                guid = guid,
                type = type
            };
        }

        private static EnemyBrainConnectionData CreateConnection(
            EnemyBrainNodeData from,
            EnemyBrainNodeData to,
            string outputName)
        {
            return new EnemyBrainConnectionData
            {
                fromNodeGuid = from.guid,
                toNodeGuid = to.guid,
                outputName = outputName
            };
        }
    }

    public class FixedEnemyBrainCondition : EnemyBrainCondition
    {
        public bool Result { get; set; }

        protected override bool Evaluate(EnemyTurnContext context)
        {
            return Result;
        }
    }

    public class RecordingEnemyBrainRule : EnemyBrainRule
    {
        public int CallCount { get; private set; }

        public override bool TryPlan(EnemyTurnContext context)
        {
            CallCount++;
            return true;
        }
    }
}
