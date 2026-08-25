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
                EnemyBrainData.DefaultPlanOutputName));

            brain.EnsurePlanNodes();

            List<EnemyBrainConnectionData> migratedConnections = brain.connections
                .Where(connection =>
                    connection.fromNodeGuid == plan.guid && connection.toNodeGuid == target.guid)
                .ToList();
            Assert.That(migratedConnections, Has.Count.EqualTo(1));
            Assert.That(
                migratedConnections[0].outputName,
                Is.EqualTo(EnemyBrainData.DefaultPlanOutputName));
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
            EnemyBrainNodeData charging = CreateNode("charging", EnemyBrainNodeType.PrePlan);
            EnemyBrainNodeData reacting = CreateNode("reacting", EnemyBrainNodeType.PrePlan);
            conditionNode.condition = condition;
            charging.prePlanOption = "Charge";
            reacting.prePlanOption = "React";

            brain.startNodeGuid = plan.guid;
            brain.prePlanStartNodeGuid = prePlan.guid;
            brain.nodes.AddRange(new[] { plan, prePlan, conditionNode, charging, reacting });
            brain.connections.Add(CreateConnection(
                prePlan,
                conditionNode,
                EnemyBrainData.RuleOutputName));
            brain.connections.Add(CreateConnection(
                conditionNode,
                charging,
                EnemyBrainData.ConditionTrueOutputName));
            brain.connections.Add(CreateConnection(
                conditionNode,
                reacting,
                EnemyBrainData.ConditionFalseOutputName));

            EnemyTurnContext context = new EnemyTurnContext(null, null, 0);
            condition.Result = true;
            Assert.That(brain.PrePlan(context, out string trueOption), Is.True);
            Assert.That(trueOption, Is.EqualTo("Charge"));
            Assert.That(context.PlannedActions, Is.Empty);

            condition.Result = false;
            Assert.That(brain.PrePlan(context, out string falseOption), Is.True);
            Assert.That(falseOption, Is.EqualTo("React"));
            Assert.That(context.PlannedActions, Is.Empty);
        }

        [Test]
        public void PlanTraversesOnlyTheSelectedPrePlanOutput()
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
                "Charge"));
            brain.connections.Add(CreateConnection(
                plan,
                moving,
                "React"));

            Assert.That(
                brain.Plan(new EnemyTurnContext(null, null, 0), "Charge"),
                Is.True);
            Assert.That(attackingRule.CallCount, Is.EqualTo(1));
            Assert.That(movingRule.CallCount, Is.Zero);

            Assert.That(
                brain.Plan(new EnemyTurnContext(null, null, 0), "React"),
                Is.True);
            Assert.That(attackingRule.CallCount, Is.EqualTo(1));
            Assert.That(movingRule.CallCount, Is.EqualTo(1));

            Assert.That(
                brain.Plan(new EnemyTurnContext(null, null, 0), "Wait"),
                Is.False);
            Assert.That(attackingRule.CallCount, Is.EqualTo(1));
            Assert.That(movingRule.CallCount, Is.EqualTo(1));
        }

        [Test]
        public void PrePlanOptionsCreatePlanOutputsAndRenameExistingConnection()
        {
            EnemyBrainData brain = CreateBrain();
            EnemyBrainNodeData plan = CreateNode("plan", EnemyBrainNodeType.Start);
            EnemyBrainNodeData charge = CreateNode("charge", EnemyBrainNodeType.PrePlan);
            EnemyBrainNodeData react = CreateNode("react", EnemyBrainNodeType.PrePlan);
            EnemyBrainNodeData target = CreateNode("target", EnemyBrainNodeType.Rule);
            charge.prePlanOption = "  Charge  ";
            react.prePlanOption = "React";

            brain.startNodeGuid = plan.guid;
            brain.nodes.AddRange(new[] { plan, charge, react, target });
            brain.connections.Add(CreateConnection(plan, target, "Charge"));

            CollectionAssert.AreEqual(
                new[] { EnemyBrainData.DefaultPlanOutputName, "Charge", "React" },
                brain.GetPlanOutputNames());

            brain.SetPrePlanOption(charge, "Wind Up");

            CollectionAssert.AreEqual(
                new[] { EnemyBrainData.DefaultPlanOutputName, "Wind Up", "React" },
                brain.GetPlanOutputNames());
            Assert.That(brain.connections[0].outputName, Is.EqualTo("Wind Up"));
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
