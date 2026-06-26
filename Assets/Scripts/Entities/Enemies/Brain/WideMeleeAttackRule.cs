using UnityEngine;

namespace Entities.Enemies
{
    [CreateAssetMenu(fileName = "WideMeleeAttackRule", menuName = "Game/Enemy Brain/Rules/Attack/Wide Melee Attack")]
    public class WideMeleeAttackRule : EnemyBrainAttackRule
    {
        [SerializeField] private EnemyBrainTargetSelector targetSelector = EnemyBrainTargetSelector.Player;
        [SerializeField] private int damage = 10;
        [SerializeField] private int range = 1;
        [SerializeField, Min(1)] private int width = 3;
        [SerializeField, Min(1)] private int height = 1;
        [SerializeField] private int baseCost = 1;
        [SerializeField] private string color = "basic";

        public override bool TryPlan(EnemyTurnContext context)
        {
            if (!TrySelectAttackTarget(context, targetSelector, out AbstractEntity target) ||
                !TryFindDirectAttackLine(context, target, range, out _, out _) ||
                !TryFindClosestAttackLine(context, target, range, out string direction, out int distance))
            {
                return false;
            }

            bool added = false;
            foreach (int attackDistance in GetCenteredDistances(distance, height))
            {
                foreach (string attackDirection in GetCenteredDirections(direction, width))
                {
                    added |= TryAddAttackIfValid(context, attackDirection, attackDistance, damage, baseCost, color);
                }
            }

            return added;
        }
    }
}
