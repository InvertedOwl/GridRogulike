using UnityEngine;

namespace Entities.Enemies
{
    public class UtilityOnlyWhenAloneRule : EnemyBrainRule
    {
        [SerializeField] private EnemyBrainRule innerRule;

        public override bool TryPlan(EnemyTurnContext context)
        {
            if (innerRule == null || innerRule == this || !IsOnlyLivingEnemy(context))
                return false;

            return innerRule.TryPlanIfConditionsPass(context);
        }
    }
}
