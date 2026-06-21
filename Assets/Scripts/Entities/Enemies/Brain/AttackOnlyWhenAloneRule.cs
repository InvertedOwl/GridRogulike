using UnityEngine;

namespace Entities.Enemies
{
    public class AttackOnlyWhenAloneRule : EnemyBrainAttackRule
    {
        [SerializeField] private EnemyBrainTargetSelector targetSelector = EnemyBrainTargetSelector.Player;
        [SerializeField] private int damage = 10;
        [SerializeField] private int range = 1;
        [SerializeField] private int baseCost = 1;
        [SerializeField] private string color = "basic";

        public override bool TryPlan(EnemyTurnContext context)
        {
            if (!IsOnlyLivingEnemy(context))
                return false;

            return TryFindDirectAttackLine(
                       context,
                       targetSelector,
                       range,
                       out _,
                       out string direction,
                       out int distance) &&
                   TryAddAttack(context, direction, distance, damage, baseCost, color);
        }
    }
}
