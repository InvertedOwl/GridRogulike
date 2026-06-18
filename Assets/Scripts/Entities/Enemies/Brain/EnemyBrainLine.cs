using Cards.Actions;
using Grid;
using UnityEngine;

namespace Entities.Enemies
{
    public static class EnemyBrainLine
    {
        public static bool TryFindDirectLine(
            EnemyTurnContext context,
            Vector2Int origin,
            Vector2Int targetPosition,
            int range,
            out string direction,
            out int distance)
        {
            direction = "";
            distance = 0;

            if (context == null)
                return false;

            foreach (string possibleDirection in HexGridManager.HexDirections)
            {
                for (int currentDistance = 1; currentDistance <= range; currentDistance++)
                {
                    Vector2Int attackTile = HexGridManager.MoveHex(origin, possibleDirection, currentDistance);
                    if (!context.IsBoardPosition(attackTile))
                        break;

                    if (attackTile == targetPosition)
                    {
                        direction = possibleDirection;
                        distance = currentDistance;
                        return true;
                    }

                    if (context.IsOccupied(attackTile))
                        break;
                }
            }

            return false;
        }

        public static int AddLineAttackActions(
            EnemyTurnContext context,
            string direction,
            int range,
            int damage,
            int baseCost,
            string color)
        {
            if (context == null || string.IsNullOrEmpty(direction))
                return 0;

            int added = 0;
            for (int distance = 1; distance <= range; distance++)
            {
                Vector2Int attackTile = HexGridManager.MoveHex(context.SimulatedPosition, direction, distance);
                if (!context.IsBoardPosition(attackTile))
                    break;

                if (context.AddAction(new AttackAction(baseCost, color, context.Self, direction, distance, damage)))
                    added++;

                if (context.IsOccupied(attackTile))
                    break;
            }

            return added;
        }
    }
}
