using UnityEngine;

namespace Entities.Enemies
{
    [CreateAssetMenu(fileName = "UtilityOnlyWhenAloneRule", menuName = "Game/Enemy Brain/Rules/Utility/Only When Alone")]
    public class UtilityOnlyWhenAloneRule : EnemyBrainRule
    {
        [SerializeField] private EnemyBrainRule innerRule;

        public override bool TryPlan(EnemyTurnContext context)
        {
            if (innerRule == null || innerRule == this || !IsOnlyLivingEnemy(context))
                return false;

            return innerRule.TryPlan(context);
        }
    }
}
