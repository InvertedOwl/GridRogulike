using System.Collections;
using System.Collections.Generic;
using Cards.Actions;
using Grid;
using StateManager;
using UnityEngine;

namespace Entities.Enemies
{
    public class MoveTowardsPlayerEnemy : AbstractEntityBehavior
    {
        protected static readonly string[] HexDirections = { "n", "ne", "nw", "s", "se", "sw" };

        public enum TargetSearchMode
        {
            MoveTowardsClosestFriendly,
            MoveTowardsClosestEnemy
        }
        

        [Header("Targeting")]
        public TargetSearchMode targetSearchMode = TargetSearchMode.MoveTowardsClosestFriendly;

        [Header("Movement")]
        public int maxMovesPerTurn = 3;
        public int stopMovingWhenWithinDistance = 1;

        public override List<AbstractAction> NextTurn()
        {
            if (self.plannedAction == null)
            {
                self.plannedAction = new List<AbstractAction>();
            }

            self.plannedAction.Clear();
            PlanMovementTowardsTarget();

            return self.plannedAction;
        }

        protected virtual void PlanMovementTowardsTarget()
        {
            PlayingState state = GameStateManager.Instance.GetCurrent<PlayingState>();
            AbstractEntity target = GetClosestTarget(state);

            if (target == null)
            {
                return;
            }

            Dictionary<Vector2Int, int> distanceMap =
                self.CalculateDistanceMap(target.positionRowCol, state, target);

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

        protected virtual AbstractEntity GetClosestTarget(PlayingState state)
        {
            AbstractEntity closestTarget = null;
            int closestDistance = int.MaxValue;

            foreach (AbstractEntity entity in state.entities)
            {
                if (entity == null || entity == self)
                {
                    continue;
                }

                if (!IsValidTarget(entity))
                {
                    continue;
                }

                Dictionary<Vector2Int, int> distanceMap =
                    self.CalculateDistanceMap(entity.positionRowCol, state, entity);

                if (!distanceMap.TryGetValue(self.positionRowCol, out int distance))
                {
                    continue;
                }

                if (distance == -1)
                {
                    continue;
                }

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = entity;
                }
            }

            return closestTarget;
        }

        protected virtual bool IsValidTarget(AbstractEntity entity)
        {
            switch (targetSearchMode)
            {
                case TargetSearchMode.MoveTowardsClosestFriendly:
                    return entity.entityType == EntityType.Player ||
                           entity.entityType == EntityType.Friendly;

                case TargetSearchMode.MoveTowardsClosestEnemy:
                    return entity.entityType == EntityType.Enemy;

                default:
                    return false;
            }
        }

        protected bool IsTargetNearby(int distance = 1)
        {
            PlayingState state = GameStateManager.Instance.GetCurrent<PlayingState>();
            AbstractEntity target = GetClosestTarget(state);

            if (target == null)
            {
                return false;
            }

            Dictionary<Vector2Int, int> distanceMap =
                self.CalculateDistanceMap(target.positionRowCol, state, target);

            return distanceMap.TryGetValue(self.positionRowCol, out int dist) && dist <= distance;
        }
    }
}