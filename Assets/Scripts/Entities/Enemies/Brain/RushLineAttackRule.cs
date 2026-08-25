using Grid;
using UnityEngine;

namespace Entities.Enemies
{
    [CreateAssetMenu(fileName = "RushLineAttackRule", menuName = "Game/Enemy Brain/Rules/Attack/Rush Line Attack")]
    public class RushLineAttackRule : EnemyBrainAttackRule
    {
        [SerializeField] private EnemyBrainTargetSelector targetSelector = EnemyBrainTargetSelector.Player;
        [SerializeField] private int damage = 10;
        [SerializeField] private int range = 4;
        [SerializeField] private int baseCost = 0;
        [SerializeField] private string color = "basic";

        public override bool TryPlan(EnemyTurnContext context)
        {
            if (!TryFindDirectAttackLine(
                    context,
                    targetSelector,
                    range,
                    out AbstractEntity target,
                    out string direction,
                    out _))
            {
                return false;
            }

            int attacksAdded = 0;
            int rushRange = ClampRange(range);

            for (int step = 0; step < rushRange; step++)
            {
                Vector2Int attackTile =
                    HexGridManager.MoveHex(context.SimulatedPosition, direction, 1);
                if (!context.IsBoardPosition(attackTile))
                    break;

                bool isTargetTile = attackTile == context.GetEntityPosition(target);
                if (context.MoveBudget <= 0 && !isTargetTile)
                    break;

                bool lineBlocked = context.IsAttackLineBlocked(attackTile);
                if (!TryAddAttack(context, direction, 1, damage, baseCost, color))
                    break;

                attacksAdded++;

                if (lineBlocked || !context.TryAddMove(direction))
                    break;
            }

            return attacksAdded > 0;
        }
    }
}
