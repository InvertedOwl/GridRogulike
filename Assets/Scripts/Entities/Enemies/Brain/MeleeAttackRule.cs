using Cards.Actions;
using UnityEngine;

namespace Entities.Enemies
{
    [CreateAssetMenu(fileName = "MeleeAttackRule", menuName = "Game/Enemy Brain/Rules/Attack/Melee Attack")]
    public class MeleeAttackRule : EnemyBrainRule
    {
        [SerializeField] private EnemyBrainTargetSelector targetSelector = EnemyBrainTargetSelector.Player;
        [SerializeField] private int damage = 10;
        [SerializeField] private int range = 1;
        [SerializeField] private int baseCost = 1;
        [SerializeField] private string color = "basic";

        public override bool TryPlan(EnemyTurnContext context)
        {
            if (!TrySelectTarget(context, targetSelector, out AbstractEntity target))
                return false;

            if (!EnemyBrainLine.TryFindDirectLine(
                    context,
                    context.SimulatedPosition,
                    context.GetEntityPosition(target),
                    Mathf.Max(1, range),
                    out string direction,
                    out int distance))
            {
                return false;
            }

            return context.AddAction(
                new AttackAction(baseCost, color, context.Self, direction, distance, damage)
            );
        }
    }
}
