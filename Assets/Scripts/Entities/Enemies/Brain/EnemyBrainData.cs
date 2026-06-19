using System.Collections.Generic;
using UnityEngine;

namespace Entities.Enemies
{
    public struct EnemyBrainPlanResult
    {
        public bool PlannedAny { get; }
        public bool StopBrain { get; }

        public EnemyBrainPlanResult(bool plannedAny, bool stopBrain)
        {
            PlannedAny = plannedAny;
            StopBrain = stopBrain;
        }
    }

    [CreateAssetMenu(fileName = "EnemyBrainData", menuName = "Game/Enemy Brain/Brain Data")]
    public class EnemyBrainData : ScriptableObject
    {
        [SerializeField] private List<EnemyBrainRule> attackRules = new List<EnemyBrainRule>();
        [SerializeField] private List<EnemyBrainRule> moveRules = new List<EnemyBrainRule>();
        [SerializeField] private List<EnemyBrainRule> utilityRules = new List<EnemyBrainRule>();

        public bool TryPlanAttack(EnemyTurnContext context)
        {
            return PlanAttack(context).PlannedAny;
        }

        public bool TryPlanMove(EnemyTurnContext context)
        {
            return PlanMove(context).PlannedAny;
        }

        public bool TryPlanUtility(EnemyTurnContext context)
        {
            return PlanUtility(context).PlannedAny;
        }

        public EnemyBrainPlanResult PlanAttack(EnemyTurnContext context)
        {
            return PlanPhase(context, attackRules);
        }

        public EnemyBrainPlanResult PlanMove(EnemyTurnContext context)
        {
            return PlanPhase(context, moveRules);
        }

        public EnemyBrainPlanResult PlanUtility(EnemyTurnContext context)
        {
            return PlanPhase(context, utilityRules);
        }

        private EnemyBrainPlanResult PlanPhase(EnemyTurnContext context, List<EnemyBrainRule> rules)
        {
            if (context == null || rules == null)
                return new EnemyBrainPlanResult(false, false);

            bool plannedAny = false;

            foreach (EnemyBrainRule rule in rules)
            {
                if (rule == null)
                    continue;

                int actionCountBefore = context.PlannedActions.Count;
                rule.TryPlan(context);

                if (context.PlannedActions.Count <= actionCountBefore)
                    continue;

                plannedAny = true;

                if (rule.AfterSuccessfulPlan == EnemyBrainRuleFlow.StopBrain)
                    return new EnemyBrainPlanResult(true, true);

                if (rule.AfterSuccessfulPlan == EnemyBrainRuleFlow.StopPhase)
                    return new EnemyBrainPlanResult(true, false);
            }

            return new EnemyBrainPlanResult(plannedAny, false);
        }
    }
}
