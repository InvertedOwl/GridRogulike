using UnityEngine;

namespace Entities.Enemies
{
    [CreateAssetMenu(fileName = "ShieldTargetRule", menuName = "Game/Enemy Brain/Rules/Utility/Shield Target")]
    public class ShieldTargetRule : EnemyBrainUtilityRule
    {
        [SerializeField] private EnemyBrainTargetSelector targetSelector = EnemyBrainTargetSelector.ClosestAlly;
        [SerializeField, Min(0)] private int range = 1;
        [SerializeField, Min(0)] private int shieldAmount = 5;
        [SerializeField] private int baseCost = 1;
        [SerializeField] private string color = "basic";

        public override bool TryPlan(EnemyTurnContext context)
        {
            if (!TrySelectUtilityTarget(context, targetSelector, out AbstractEntity target))
                return false;

            if (!IsTargetInRange(context, target, range, out _))
                return false;

            return TryAddShieldToTarget(context, target, shieldAmount, baseCost, color);
        }
    }
}
