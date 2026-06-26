using UnityEngine;

namespace Entities.Enemies
{
    public class UtilityOnlyWhenAloneRule : EnemyBrainUtilityRule
    {
        [SerializeField] private EnemyBrainUtilityRule innerRule;

        public override bool TryPlan(EnemyTurnContext context)
        {
            if (innerRule == null || innerRule == this || !IsOnlyLivingEnemy(context))
                return false;

            return innerRule.TryPlan(context);
        }
    }
}
