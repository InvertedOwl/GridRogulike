using Cards.Actions;
using Grid;
using UnityEngine;

namespace Entities.Enemies
{
    [CreateAssetMenu(fileName = "WideMeleeAttackRule", menuName = "Game/Enemy Brain/Rules/Attack/Wide Melee Attack")]
    public class WideMeleeAttackRule : EnemyBrainRule
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

            bool added = context.AddAction(
                new AttackAction(baseCost, color, context.Self, direction, distance, damage)
            );

            if (HexGridManager.neighborDirections.TryGetValue(direction, out var neighborDirections))
            {
                foreach (string neighborDirection in neighborDirections)
                {
                    added |= context.AddAction(
                        new AttackAction(baseCost, color, context.Self, neighborDirection, distance, damage)
                    );
                }
            }

            return added;
        }
    }
}
