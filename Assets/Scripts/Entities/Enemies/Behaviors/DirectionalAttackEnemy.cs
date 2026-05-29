using System.Collections;
using System.Collections.Generic;
using Cards.Actions;
using Cards.CardEvents;
using Grid;
using StateManager;
using UnityEngine;
using Util;

namespace Entities.Enemies
{
    public class DirectionalAttackEnemy : MoveTowardsPlayerEnemy
    {
        public int DefaultDamage = 10;
        public int attackRange = 4;

        private string _direction = "e";

        // For assigning to things, need to tell controllers what enemies next turn is
        public override List<AbstractAction> NextTurn()
        {
            if (self.plannedAction == null)
            {
                self.plannedAction = new List<AbstractAction>();
            }

            self.plannedAction.Clear();

            PlayingState playingState = GameStateManager.Instance.GetCurrent<PlayingState>();
            AbstractEntity target = playingState.player;

            if (target == null || target.Health <= 0)
            {
                return self.plannedAction;
            }

            if (TryGetAttackDirection(self.positionRowCol, target.positionRowCol, out _direction))
            {
                PlanLineAttack(_direction);
                return self.plannedAction;
            }

            Dictionary<Vector2Int, int> targetDistanceMap =
                HexGridManager.Instance.CalculateDistanceMap(target.positionRowCol, new List<Vector2Int>());

            if (!targetDistanceMap.TryGetValue(self.positionRowCol, out int distanceToTarget) ||
                distanceToTarget < 0 ||
                distanceToTarget > attackRange)
            {
                PlanMovementTowardsAttackRange(playingState, target);
                return self.plannedAction;
            }

            PlanMovementTowardsLineup(playingState, target, targetDistanceMap, distanceToTarget);
            return self.plannedAction;
        }

        private void PlanLineAttack(string direction)
        {
            for (int i = 0; i < attackRange; i++)
            {
                self.plannedAction.Add(new AttackAction(0, "basic", self, direction, i + 1, DefaultDamage));
            }
        }

        private bool TryGetAttackDirection(Vector2Int attackPosition, Vector2Int targetPosition, out string direction)
        {
            foreach (string possibleDirection in HexGridManager.HexDirections)
            {
                for (int distance = 1; distance <= attackRange; distance++)
                {
                    if (HexGridManager.MoveHex(attackPosition, possibleDirection, distance) == targetPosition)
                    {
                        direction = possibleDirection;
                        return true;
                    }
                }
            }

            direction = "";
            return false;
        }

        private void PlanMovementTowardsAttackRange(PlayingState state, AbstractEntity target)
        {
            Dictionary<Vector2Int, int> movementDistanceMap =
                self.CalculateDistanceMap(target.positionRowCol, state, target);

            Vector2Int currentPosition = self.positionRowCol;

            for (int step = 0; step < maxMovesPerTurn; step++)
            {
                if (movementDistanceMap.TryGetValue(currentPosition, out int distance) &&
                    distance >= 0 &&
                    distance <= attackRange)
                {
                    break;
                }

                MoveAction bestMove = GetBestMove(currentPosition, movementDistanceMap);

                if (bestMove == null)
                {
                    break;
                }

                currentPosition = HexGridManager.MoveHex(currentPosition, bestMove.Direction, 1);
                self.plannedAction.Add(bestMove);
            }
        }

        private void PlanMovementTowardsLineup(
            PlayingState state,
            AbstractEntity target,
            Dictionary<Vector2Int, int> targetDistanceMap,
            int startingDistanceToTarget
        )
        {
            Dictionary<Vector2Int, int> lineupDistanceMap =
                CalculateLineupDistanceMap(state, target.positionRowCol);

            Vector2Int currentPosition = self.positionRowCol;

            for (int step = 0; step < maxMovesPerTurn; step++)
            {
                if (TryGetAttackDirection(currentPosition, target.positionRowCol, out _))
                {
                    break;
                }

                MoveAction bestMove = GetBestLineupMove(
                    currentPosition,
                    lineupDistanceMap,
                    targetDistanceMap,
                    startingDistanceToTarget
                );

                if (bestMove == null)
                {
                    break;
                }

                currentPosition = HexGridManager.MoveHex(currentPosition, bestMove.Direction, 1);
                self.plannedAction.Add(bestMove);
            }
        }

        private MoveAction GetBestLineupMove(
            Vector2Int currentPosition,
            Dictionary<Vector2Int, int> lineupDistanceMap,
            Dictionary<Vector2Int, int> targetDistanceMap,
            int preferredDistanceToTarget
        )
        {
            lineupDistanceMap.TryGetValue(currentPosition, out int currentLineupDistance);
            if (currentLineupDistance <= 0)
            {
                currentLineupDistance = int.MaxValue;
            }

            MoveAction bestMove = null;
            int bestScore = int.MaxValue;

            foreach (string direction in HexGridManager.HexDirections)
            {
                Vector2Int candidatePosition = HexGridManager.MoveHex(currentPosition, direction, 1);

                if (!lineupDistanceMap.TryGetValue(candidatePosition, out int candidateLineupDistance) ||
                    candidateLineupDistance < 0 ||
                    candidateLineupDistance >= currentLineupDistance)
                {
                    continue;
                }

                if (!targetDistanceMap.TryGetValue(candidatePosition, out int candidateDistanceToTarget) ||
                    candidateDistanceToTarget < 0 ||
                    candidateDistanceToTarget > attackRange)
                {
                    continue;
                }

                int lateralPenalty = Mathf.Abs(candidateDistanceToTarget - preferredDistanceToTarget);
                int closerPenalty = candidateDistanceToTarget < preferredDistanceToTarget ? 1 : 0;
                int score = candidateLineupDistance * 100 + lateralPenalty * 10 + closerPenalty;

                if (score < bestScore)
                {
                    bestScore = score;
                    bestMove = new MoveAction(1, "basic", self, direction, 1);
                }
            }

            return bestMove;
        }

        private Dictionary<Vector2Int, int> CalculateLineupDistanceMap(PlayingState state, Vector2Int targetPosition)
        {
            Dictionary<Vector2Int, int> combinedDistanceMap = new Dictionary<Vector2Int, int>();
            List<Vector2Int> blockers = GetMovementBlockers(state);

            foreach (Vector2Int boardPosition in HexGridManager.Instance.BoardDictionary.Keys)
            {
                if (IsHexOccupiedByOtherEntity(state, boardPosition))
                {
                    continue;
                }

                if (!TryGetAttackDirection(boardPosition, targetPosition, out _))
                {
                    continue;
                }

                Dictionary<Vector2Int, int> distanceMap =
                    HexGridManager.Instance.CalculateDistanceMap(boardPosition, blockers);

                foreach (KeyValuePair<Vector2Int, int> distanceEntry in distanceMap)
                {
                    if (distanceEntry.Value < 0)
                    {
                        continue;
                    }

                    if (!combinedDistanceMap.TryGetValue(distanceEntry.Key, out int currentDistance) ||
                        distanceEntry.Value < currentDistance)
                    {
                        combinedDistanceMap[distanceEntry.Key] = distanceEntry.Value;
                    }
                }
            }

            return combinedDistanceMap;
        }

        private List<Vector2Int> GetMovementBlockers(PlayingState state)
        {
            List<Vector2Int> blockers = new List<Vector2Int>();

            foreach (AbstractEntity entity in state.GetEntities())
            {
                if (entity == null || entity == self || entity.Health <= 0)
                {
                    continue;
                }

                blockers.Add(entity.positionRowCol);
            }

            return blockers;
        }

        private bool IsHexOccupiedByOtherEntity(PlayingState state, Vector2Int position)
        {
            foreach (AbstractEntity entity in state.GetEntities())
            {
                if (entity == null || entity == self || entity.Health <= 0)
                {
                    continue;
                }

                if (entity.positionRowCol == position)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
