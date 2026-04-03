using System.Collections.Generic;
using Cards.Actions;
using Grid;
using StateManager;
using UnityEngine;

namespace Entities.Enemies
{
    public abstract class MoveTowardsPlayerEnemy : AbstractEntityBehavior
    {
        protected static readonly string[] HexDirections = { "n", "ne", "nw", "s", "se", "sw" };

        [Header("Movement")]
        public int maxMovesPerTurn = 3;
        public int stopMovingWhenWithinDistance = 1;

        protected virtual void PlanMovementTowardsPlayer()
        {
            PlayingState state = GameStateManager.Instance.GetCurrent<PlayingState>();
            AbstractEntity player = state.player;

            Dictionary<Vector2Int, int> distanceMap =
                self.CalculateDistanceMap(player.positionRowCol, state, player);

            Vector2Int currentPosition = self.positionRowCol;

            for (int step = 0; step < maxMovesPerTurn; step++)
            {
                MoveAction bestMove = GetBestMove(currentPosition, distanceMap);

                if (bestMove == null)
                {
                    break;
                }

                currentPosition = HexGridManager.MoveHex(currentPosition, bestMove.Direction, 1);
                self.plannedAction.Add(bestMove);

                if (distanceMap.ContainsKey(currentPosition) &&
                    distanceMap[currentPosition] <= stopMovingWhenWithinDistance)
                {
                    break;
                }
            }
        }

        protected virtual MoveAction GetBestMove(
            Vector2Int currentPosition,
            Dictionary<Vector2Int, int> distanceMap
        )
        {
            MoveAction bestMove = null;
            int bestDistance = int.MaxValue;

            foreach (string direction in HexDirections)
            {
                Vector2Int candidateHex = HexGridManager.MoveHex(currentPosition, direction, 1);

                if (!distanceMap.ContainsKey(candidateHex))
                {
                    continue;
                }

                int candidateDistance = distanceMap[candidateHex];

                if (candidateDistance == -1)
                {
                    continue;
                }

                if (candidateDistance < bestDistance)
                {
                    bestDistance = candidateDistance;
                    bestMove = new MoveAction(1, "basic", self, direction, 1);
                }
            }

            return bestMove;
        }
    }
}
