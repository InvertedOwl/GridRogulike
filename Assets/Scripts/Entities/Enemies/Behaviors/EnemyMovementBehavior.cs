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
        protected static readonly string[] HexDirections = HexGridManager.HexDirections;

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

        protected virtual int PreferredTargetDistance => Mathf.Max(0, stopMovingWhenWithinDistance);

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
            PlanMovementTowardsTarget(state, target);
        }

        protected virtual void PlanMovementTowardsTarget(PlayingState state, AbstractEntity target)
        {
            if (target == null)
            {
                return;
            }

            Dictionary<Vector2Int, int> distanceMap =
                self.CalculateDistanceMap(target.positionRowCol, state, target);

            Vector2Int currentPosition = self.positionRowCol;

            for (int step = 0; step < maxMovesPerTurn; step++)
            {
                if (IsAtPreferredTargetDistance(currentPosition, distanceMap))
                {
                    break;
                }

                MoveAction bestMove = GetBestMove(currentPosition, distanceMap);

                if (bestMove == null)
                {
                    break;
                }

                currentPosition = HexGridManager.MoveHex(currentPosition, bestMove.Direction, 1);
                self.plannedAction.Add(bestMove);
            }
        }

        protected virtual MoveAction GetBestMove(
            Vector2Int currentPosition,
            Dictionary<Vector2Int, int> distanceMap
        )
        {
            PlayingState state = GameStateManager.Instance.GetCurrent<PlayingState>();
            MoveAction bestMove = null;
            int bestScore = int.MaxValue;
            int bestDistance = int.MaxValue;
            int currentScore = GetPreferredDistanceScore(currentPosition, distanceMap);

            foreach (string direction in HexDirections)
            {
                Vector2Int candidateHex = HexGridManager.MoveHex(currentPosition, direction, 1);

                if (!IsMoveDestinationOpen(state, candidateHex))
                {
                    continue;
                }

                if (!distanceMap.ContainsKey(candidateHex))
                {
                    continue;
                }

                int candidateDistance = distanceMap[candidateHex];

                if (candidateDistance == -1)
                {
                    continue;
                }

                int candidateScore = GetPreferredDistanceScore(candidateDistance);
                if (candidateScore >= currentScore)
                {
                    continue;
                }

                if (candidateScore < bestScore ||
                    (candidateScore == bestScore && candidateDistance < bestDistance))
                {
                    bestScore = candidateScore;
                    bestDistance = candidateDistance;
                    bestMove = new MoveAction(1, "basic", self, direction, 1);
                }
            }

            return bestMove;
        }

        protected bool IsCloserThanPreferredTargetDistance(
            Vector2Int currentPosition,
            Dictionary<Vector2Int, int> distanceMap
        )
        {
            return distanceMap.TryGetValue(currentPosition, out int distance) &&
                   distance >= 0 &&
                   distance < PreferredTargetDistance;
        }

        protected bool IsAtPreferredTargetDistance(
            Vector2Int currentPosition,
            Dictionary<Vector2Int, int> distanceMap
        )
        {
            return distanceMap.TryGetValue(currentPosition, out int distance) &&
                   distance == PreferredTargetDistance;
        }

        private int GetPreferredDistanceScore(Vector2Int position, Dictionary<Vector2Int, int> distanceMap)
        {
            if (!distanceMap.TryGetValue(position, out int distance) || distance < 0)
            {
                return int.MaxValue;
            }

            return GetPreferredDistanceScore(distance);
        }

        private int GetPreferredDistanceScore(int distance)
        {
            return Mathf.Abs(distance - PreferredTargetDistance);
        }

        private bool IsMoveDestinationOpen(PlayingState state, Vector2Int position)
        {
            if (HexGridManager.Instance == null ||
                !HexGridManager.Instance.BoardDictionary.ContainsKey(position))
            {
                return false;
            }

            if (state == null)
            {
                return true;
            }

            foreach (AbstractEntity entity in state.GetEntities())
            {
                if (entity == null || entity == self || entity.Health <= 0)
                {
                    continue;
                }

                if (entity.positionRowCol == position)
                {
                    return false;
                }
            }

            return true;
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
