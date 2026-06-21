using Cards.Actions;
using UnityEngine;

namespace Entities.Enemies
{
    [CreateAssetMenu(fileName = "ApplyStatusAttackRule", menuName = "Game/Enemy Brain/Rules/Attack/Apply Status Attack")]
    public class ApplyStatusAttackRule : EnemyBrainAttackRule
    {
        [SerializeField] private EnemyBrainTargetSelector targetSelector = EnemyBrainTargetSelector.Player;
        [SerializeField] private int damage = 10;
        [SerializeField] private int range = 1;
        [SerializeField] private StatusApplicationType statusType = StatusApplicationType.Poison;
        [SerializeField] private int statusAmount = 1;
        [SerializeField] private int baseCost = 1;
        [SerializeField] private string color = "basic";

        public override bool TryPlan(EnemyTurnContext context)
        {
            return TryFindDirectAttackLine(
                       context,
                       targetSelector,
                       range,
                       out _,
                       out string direction,
                       out int distance) &&
                   TryAddStatusAttack(
                       context,
                       direction,
                       distance,
                       damage,
                       statusType,
                       statusAmount,
                       baseCost,
                       color);
        }
    }
}
