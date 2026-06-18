using System.Collections.Generic;
using UnityEngine;

namespace Entities.Enemies
{
    [CreateAssetMenu(fileName = "EnemyBrainData", menuName = "Game/Enemy Brain/Brain Data")]
    public class EnemyBrainData : ScriptableObject
    {
        [SerializeField] private List<EnemyBrainRule> attackRules = new List<EnemyBrainRule>();
        [SerializeField] private List<EnemyBrainRule> moveRules = new List<EnemyBrainRule>();
        [SerializeField] private List<EnemyBrainRule> utilityRules = new List<EnemyBrainRule>();

        public bool TryPlanAttack(EnemyTurnContext context)
        {
            return TryPlanPhase(context, attackRules);
        }

        public bool TryPlanMove(EnemyTurnContext context)
        {
            return TryPlanPhase(context, moveRules);
        }

        public bool TryPlanUtility(EnemyTurnContext context)
        {
            return TryPlanPhase(context, utilityRules);
        }

        private bool TryPlanPhase(EnemyTurnContext context, List<EnemyBrainRule> rules)
        {
            if (context == null || rules == null)
                return false;

            foreach (EnemyBrainRule rule in rules)
            {
                if (rule == null)
                    continue;

                int actionCountBefore = context.PlannedActions.Count;
                if (rule.TryPlan(context) && context.PlannedActions.Count > actionCountBefore)
                    return true;
            }

            return false;
        }
    }
}
