using Cards.Actions;
using UnityEngine;

namespace Entities.Enemies
{
    [CreateAssetMenu(fileName = "ApplyStatusToTargetRule", menuName = "Game/Enemy Brain/Rules/Utility/Apply Status To Target")]
    public class ApplyStatusToTargetRule : EnemyBrainRule
    {
        [SerializeField] private EnemyBrainTargetSelector targetSelector = EnemyBrainTargetSelector.ClosestAlly;
        [SerializeField] private int range = 1;
        [SerializeField] private StatusApplicationType statusType = StatusApplicationType.Buffed;
        [SerializeField] private int statusAmount = 3;
        [SerializeField] private int baseCost = 1;
        [SerializeField] private string color = "basic";

        public override bool TryPlan(EnemyTurnContext context)
        {
            if (!TrySelectTarget(context, targetSelector, out AbstractEntity target))
                return false;

            if (!context.TryGetDistanceTo(target, out int distance) || distance > range)
                return false;

            return context.AddAction(
                new ApplyStatusToEntityAction(baseCost, color, context.Self, target, statusType, statusAmount)
            );
        }
    }
}
