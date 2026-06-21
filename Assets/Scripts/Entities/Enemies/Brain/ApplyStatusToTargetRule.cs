using Cards.Actions;
using UnityEngine;

namespace Entities.Enemies
{
    [CreateAssetMenu(fileName = "ApplyStatusToTargetRule", menuName = "Game/Enemy Brain/Rules/Utility/Apply Status To Target")]
    public class ApplyStatusToTargetRule : EnemyBrainUtilityRule
    {
        [SerializeField] private EnemyBrainTargetSelector targetSelector = EnemyBrainTargetSelector.ClosestAlly;
        [SerializeField] private int range = 1;
        [SerializeField] private StatusApplicationType statusType = StatusApplicationType.Buffed;
        [SerializeField] private int statusAmount = 3;
        [SerializeField] private int baseCost = 1;
        [SerializeField] private string color = "basic";

        public override bool TryPlan(EnemyTurnContext context)
        {
            if (!TrySelectUtilityTarget(context, targetSelector, out AbstractEntity target))
                return false;

            if (!IsTargetInRange(context, target, range, out _))
                return false;

            return TryAddStatusToTarget(context, target, statusType, statusAmount, baseCost, color);
        }
    }
}
