using System.Collections.Generic;
using System.Linq;
using Cards;
using Cards.Actions;
using Cards.CardEvents;
using Entities;
using Entities.Enemies;
using NUnit.Framework;
using UnityEditor;
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

        [Test]
        public void CycleConditionProvidesOneBasedOutputsForConfiguredCount()
        {
            CycleCondition cycle = Track(ScriptableObject.CreateInstance<CycleCondition>());
            SetCycleOptionCount(cycle, 4);

            CollectionAssert.AreEqual(
                new[] { "1", "2", "3", "4" },
                cycle.GetOutputNames());
        }

        [Test]
        public void PlanCycleAdvancesAcrossTurnContextsAndWraps()
        {
            EnemyBrainData brain = CreateBrain();
            CycleCondition cycle = Track(ScriptableObject.CreateInstance<CycleCondition>());
            SetCycleOptionCount(cycle, 3);

            EnemyBrainNodeData plan = CreateNode("plan", EnemyBrainNodeType.Start);
            EnemyBrainNodeData cycleNode = CreateNode("cycle", EnemyBrainNodeType.Condition);
            cycleNode.condition = cycle;
            brain.startNodeGuid = plan.guid;
            brain.nodes.Add(plan);
            brain.nodes.Add(cycleNode);
            brain.connections.Add(CreateConnection(
                plan,
                cycleNode,
                EnemyBrainData.DefaultPlanOutputName));

            RecordingEnemyBrainRule[] rules = new RecordingEnemyBrainRule[3];
            for (int i = 0; i < rules.Length; i++)
            {
                rules[i] = Track(ScriptableObject.CreateInstance<RecordingEnemyBrainRule>());
                EnemyBrainNodeData ruleNode = CreateNode($"rule-{i + 1}", EnemyBrainNodeType.Rule);
                ruleNode.rule = rules[i];
                brain.nodes.Add(ruleNode);
                brain.connections.Add(CreateConnection(cycleNode, ruleNode, (i + 1).ToString()));
            }

            Dictionary<string, int> cycleIndices = new Dictionary<string, int>();
            for (int i = 0; i < 4; i++)
            {
                EnemyTurnContext context = new EnemyTurnContext(
                    null,
                    null,
                    0,
                    conditionCycleIndices: cycleIndices);
                Assert.That(brain.Plan(context), Is.True);
            }

            Assert.That(rules[0].CallCount, Is.EqualTo(2));
            Assert.That(rules[1].CallCount, Is.EqualTo(1));
            Assert.That(rules[2].CallCount, Is.EqualTo(1));
        }

        [Test]
        public void CommentNodePreservesTextAndStopsGameplayTraversal()
        {
            EnemyBrainData brain = CreateBrain();
            RecordingEnemyBrainRule rule = Track(
                ScriptableObject.CreateInstance<RecordingEnemyBrainRule>());

            EnemyBrainNodeData plan = CreateNode("plan", EnemyBrainNodeType.Start);
            EnemyBrainNodeData comment = CreateNode("comment", EnemyBrainNodeType.Comment);
            EnemyBrainNodeData ruleNode = CreateNode("rule", EnemyBrainNodeType.Rule);
            comment.comment = "Remember why this branch exists.";
            ruleNode.rule = rule;

            brain.startNodeGuid = plan.guid;
            brain.nodes.AddRange(new[] { plan, comment, ruleNode });
            brain.connections.Add(CreateConnection(
                plan,
                comment,
                EnemyBrainData.DefaultPlanOutputName));
            brain.connections.Add(CreateConnection(
                comment,
                ruleNode,
                EnemyBrainData.RuleOutputName));

            Assert.That(
                brain.Plan(new EnemyTurnContext(null, null, 0)),
                Is.False);
            Assert.That(rule.CallCount, Is.Zero);
            Assert.That(comment.comment, Is.EqualTo("Remember why this branch exists."));
            Assert.That(comment.title, Is.EqualTo("Comment"));
        }

        [Test]
        public void EnemyBrainGraphPositionsSnapToVisibleGridSpacing()
        {
            Assert.That(
                EnemyBrainGraphView.SnapPosition(new Vector2(34f, 46f)),
                Is.EqualTo(new Vector2(40f, 40f)));
            Assert.That(
                EnemyBrainGraphView.SnapPosition(new Vector2(-14f, -26f)),
                Is.EqualTo(new Vector2(-20f, -20f)));
        }

        [Test]
        public void ShieldFixedEntityActionKeepsSelectedTarget()
        {
            GameObject sourceObject = Track(new GameObject("shield-source"));
            GameObject targetObject = Track(new GameObject("shield-target"));
            NonPlayerEntity source = sourceObject.AddComponent<NonPlayerEntity>();
            NonPlayerEntity target = targetObject.AddComponent<NonPlayerEntity>();
            source.initialHealth = 10;
            source._health = 10;
            target.initialHealth = 10;
            target._health = 10;

            ShieldFixedEntityAction action = new ShieldFixedEntityAction(
                1,
                "basic",
                source,
                target,
                7);

            List<AbstractCardEvent> events = action.Activate((CardMonobehaviour)null);

            Assert.That(events, Has.Count.EqualTo(1));
            Assert.That(events[0], Is.TypeOf<ShieldCardEvent>());
            ShieldCardEvent shieldEvent = (ShieldCardEvent)events[0];
            Assert.That(shieldEvent.target, Is.SameAs(target));
            Assert.That(shieldEvent.amount, Is.EqualTo(7));
        }

        [Test]
        public void DirectionalAttackActionCarriesSelectedHitVfxIntoEventAndCopy()
        {
            const string selectedVfx = "BloodExplosionSpiky";
            DirectionalAttackAction action = new DirectionalAttackAction(
                1,
                "basic",
                null,
                "ne",
                2,
                7,
                selectedVfx);

            List<AbstractCardEvent> events = action.Activate((CardMonobehaviour)null, previewMode: true);

            Assert.That(action, Is.InstanceOf<AttackAction>());
            Assert.That(events, Has.Count.EqualTo(1));
            Assert.That(events[0], Is.TypeOf<AttackCardEvent>());
            AttackCardEvent attackEvent = (AttackCardEvent)events[0];
            Assert.That(attackEvent.usePosition, Is.False);
            Assert.That(attackEvent.direction, Is.EqualTo("ne"));
            Assert.That(attackEvent.distance, Is.EqualTo(2));
            Assert.That(attackEvent.amount, Is.EqualTo(7));
            Assert.That(attackEvent.manual, Is.True);
            Assert.That(attackEvent.hitFxKey, Is.EqualTo(selectedVfx));
            Assert.That(attackEvent.Copy().hitFxKey, Is.EqualTo(selectedVfx));
        }

        [Test]
        public void DirectionalAttackActionCardContextStillCreatesDirectionalEvent()
        {
            DirectionalAttackAction action = new DirectionalAttackAction(
                1,
                "basic",
                null,
                "sw",
                3,
                9);
            CardPlayContext context = new CardPlayContext(
                null,
                new Card(false),
                null,
                TargetSelection.Empty(),
                null,
                previewMode: true);

            List<AbstractCardEvent> events = action.Activate(context);

            Assert.That(events, Has.Count.EqualTo(1));
            AttackCardEvent attackEvent = events[0] as AttackCardEvent;
            Assert.That(attackEvent, Is.Not.Null);
            Assert.That(attackEvent.usePosition, Is.False);
            Assert.That(attackEvent.direction, Is.EqualTo("sw"));
            Assert.That(attackEvent.distance, Is.EqualTo(3));
            Assert.That(attackEvent.amount, Is.EqualTo(9));
            Assert.That(attackEvent.manual, Is.True);
        }

        [Test]
        public void AttackActionCarriesOptionalHitVfxIntoEvent()
        {
            const string selectedVfx = "EnergyExplosionYellow";
            AttackAction action = new AttackAction(1, "basic", null, 6, selectedVfx);
            Card card = new Card(false);
            TargetSelection targets = new TargetSelection(
                Cards.CardList.TargetDefinition.None,
                targetPositions: new[] { new Vector2Int(2, 3) });
            CardPlayContext context = new CardPlayContext(
                null,
                card,
                null,
                targets,
                null,
                previewMode: true);

            List<AbstractCardEvent> events = action.Activate(context);

            Assert.That(events, Has.Count.EqualTo(1));
            Assert.That(events[0], Is.TypeOf<AttackCardEvent>());
            Assert.That(((AttackCardEvent)events[0]).hitFxKey, Is.EqualTo(selectedVfx));
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

        private static void SetCycleOptionCount(CycleCondition cycle, int optionCount)
        {
            SerializedObject serializedCycle = new SerializedObject(cycle);
            serializedCycle.FindProperty("optionCount").intValue = optionCount;
            serializedCycle.ApplyModifiedPropertiesWithoutUndo();
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
