using System.Collections.Generic;
using Grid;
using UnityEngine;

namespace Entities.Enemies
{
    public static class EnemyBrainMovement
    {
        public static bool TryMoveToPreferredDistance(
            EnemyTurnContext context,
            AbstractEntity target,
            int preferredDistance,
            int maxMoves)
        {
            if (!CanMove(context, target))
                return false;

            int movesAdded = 0;
            int movesAllowed = GetMovesAllowed(context, maxMoves);
            preferredDistance = Mathf.Max(0, preferredDistance);
            Dictionary<Vector2Int, int> distanceMap = GetDistanceMap(context, target);

            for (int i = 0; i < movesAllowed; i++)
            {
                if (!TryGetDistance(distanceMap, context.SimulatedPosition, out int currentDistance))
                    break;

                int currentScore = Mathf.Abs(currentDistance - preferredDistance);
                if (currentScore == 0)
                    break;

                if (!TryFindBestPreferredDistanceMove(
                        context,
                        distanceMap,
                        preferredDistance,
                        currentDistance,
                        currentScore,
                        out string bestDirection))
                {
                    break;
                }

                if (!context.TryAddMove(bestDirection))
                    break;

                movesAdded++;
            }

            return movesAdded > 0;
        }

        public static bool TryMoveTowardTarget(
            EnemyTurnContext context,
            AbstractEntity target,
            int stopDistance,
            int maxMoves)
        {
            if (!CanMove(context, target))
                return false;

            int movesAdded = 0;
            int movesAllowed = GetMovesAllowed(context, maxMoves);
            stopDistance = Mathf.Max(0, stopDistance);
            Dictionary<Vector2Int, int> distanceMap = GetDistanceMap(context, target);

            for (int i = 0; i < movesAllowed; i++)
            {
                if (!TryGetDistance(distanceMap, context.SimulatedPosition, out int currentDistance) ||
                    currentDistance <= stopDistance)
                {
                    break;
                }

                if (!TryFindBestTowardMove(context, distanceMap, currentDistance, out string bestDirection))
                    break;

                if (!context.TryAddMove(bestDirection))
                    break;

                movesAdded++;
            }

            return movesAdded > 0;
        }

        public static bool TryMoveAwayFromTarget(
            EnemyTurnContext context,
            AbstractEntity target,
            int minimumDistance,
            int maxMoves)
        {
            if (!CanMove(context, target))
                return false;

            int movesAdded = 0;
            int movesAllowed = GetMovesAllowed(context, maxMoves);
            minimumDistance = Mathf.Max(0, minimumDistance);
            Dictionary<Vector2Int, int> distanceMap = GetDistanceMap(context, target);

            for (int i = 0; i < movesAllowed; i++)
            {
                if (!TryGetDistance(distanceMap, context.SimulatedPosition, out int currentDistance) ||
                    currentDistance >= minimumDistance)
                {
                    break;
                }

                if (!TryFindBestAwayMove(context, distanceMap, currentDistance, out string bestDirection))
                    break;

                if (!context.TryAddMove(bestDirection))
                    break;

                movesAdded++;
            }

            return movesAdded > 0;
        }

        public static bool TryMoveIntoDirectLine(
            EnemyTurnContext context,
            AbstractEntity target,
            int attackRange,
            int preferredDistance,
            int maxMoves)
        {
            if (!CanMove(context, target))
                return false;

            int movesAdded = 0;
            int movesAllowed = GetMovesAllowed(context, maxMoves);
            Dictionary<Vector2Int, int> lineupMap = BuildDirectLineDistanceMap(context, target, attackRange);
            Dictionary<Vector2Int, int> targetDistanceMap = GetDistanceMap(context, target);

            for (int i = 0; i < movesAllowed; i++)
            {
                if (EnemyBrainLine.TryFindDirectLine(
                        context,
                        context.SimulatedPosition,
                        context.GetEntityPosition(target),
                        attackRange,
                        out _,
                        out _))
                {
                    break;
                }

                if (!TryFindBestLineupMove(
                        context,
                        lineupMap,
                        targetDistanceMap,
                        preferredDistance,
                        out string bestDirection))
                {
                    break;
                }

                if (!context.TryAddMove(bestDirection))
                    break;

                movesAdded++;
            }

            return movesAdded > 0;
        }

        public static bool TryMoveAlongCurrentDistance(
            EnemyTurnContext context,
            AbstractEntity target,
            int maxMoves)
        {
            if (!CanMove(context, target))
                return false;

            int movesAdded = 0;
            int movesAllowed = GetMovesAllowed(context, maxMoves);
            Dictionary<Vector2Int, int> distanceMap = GetDistanceMap(context, target);

            for (int i = 0; i < movesAllowed; i++)
            {
                if (!TryGetDistance(distanceMap, context.SimulatedPosition, out int currentDistance))
                    break;

                if (!TryFindSameDistanceMove(context, distanceMap, currentDistance, out string bestDirection))
                    break;

                if (!context.TryAddMove(bestDirection))
                    break;

                movesAdded++;
            }

            return movesAdded > 0;
        }

        private static bool TryFindBestPreferredDistanceMove(
            EnemyTurnContext context,
            Dictionary<Vector2Int, int> distanceMap,
            int preferredDistance,
            int currentDistance,
            int currentScore,
            out string bestDirection)
        {
            bestDirection = "";
            int bestScore = int.MaxValue;
            List<string> bestDirections = new List<string>();

            foreach (string direction in HexGridManager.HexDirections)
            {
                Vector2Int candidate = HexGridManager.MoveHex(context.SimulatedPosition, direction, 1);
                if (!context.IsOpen(candidate) || !TryGetDistance(distanceMap, candidate, out int candidateDistance))
                    continue;

                int candidateScore = Mathf.Abs(candidateDistance - preferredDistance);
                if (candidateScore >= currentScore)
                    continue;

                if (candidateScore < bestScore)
                {
                    bestScore = candidateScore;
                    bestDirections.Clear();
                    bestDirections.Add(direction);
                }
                else if (candidateScore == bestScore)
                {
                    bestDirections.Add(direction);
                }
            }

            return TryPickRandomDirection(context, bestDirections, out bestDirection);
        }

        private static bool TryFindBestTowardMove(
            EnemyTurnContext context,
            Dictionary<Vector2Int, int> distanceMap,
            int currentDistance,
            out string bestDirection)
        {
            bestDirection = "";
            int bestDistance = currentDistance;
            List<string> bestDirections = new List<string>();

            foreach (string direction in HexGridManager.HexDirections)
            {
                Vector2Int candidate = HexGridManager.MoveHex(context.SimulatedPosition, direction, 1);
                if (!context.IsOpen(candidate) || !TryGetDistance(distanceMap, candidate, out int candidateDistance))
                    continue;

                if (candidateDistance < bestDistance)
                {
                    bestDistance = candidateDistance;
                    bestDirections.Clear();
                    bestDirections.Add(direction);
                }
                else if (candidateDistance == bestDistance)
                {
                    bestDirections.Add(direction);
                }
            }

            return TryPickRandomDirection(context, bestDirections, out bestDirection);
        }

        private static bool TryFindBestAwayMove(
            EnemyTurnContext context,
            Dictionary<Vector2Int, int> distanceMap,
            int currentDistance,
            out string bestDirection)
        {
            bestDirection = "";
            int bestDistance = currentDistance;
            List<string> bestDirections = new List<string>();

            foreach (string direction in HexGridManager.HexDirections)
            {
                Vector2Int candidate = HexGridManager.MoveHex(context.SimulatedPosition, direction, 1);
                if (!context.IsOpen(candidate) || !TryGetDistance(distanceMap, candidate, out int candidateDistance))
                    continue;

                if (candidateDistance > bestDistance)
                {
                    bestDistance = candidateDistance;
                    bestDirections.Clear();
                    bestDirections.Add(direction);
                }
                else if (candidateDistance == bestDistance)
                {
                    bestDirections.Add(direction);
                }
            }

            return TryPickRandomDirection(context, bestDirections, out bestDirection);
        }

        private static bool TryFindBestLineupMove(
            EnemyTurnContext context,
            Dictionary<Vector2Int, int> lineupMap,
            Dictionary<Vector2Int, int> targetDistanceMap,
            int preferredDistance,
            out string bestDirection)
        {
            bestDirection = "";
            if (!TryGetDistance(lineupMap, context.SimulatedPosition, out int currentLineupDistance))
                currentLineupDistance = int.MaxValue;

            int bestScore = int.MaxValue;
            List<string> bestDirections = new List<string>();

            foreach (string direction in HexGridManager.HexDirections)
            {
                Vector2Int candidate = HexGridManager.MoveHex(context.SimulatedPosition, direction, 1);
                if (!context.IsOpen(candidate) ||
                    !TryGetDistance(lineupMap, candidate, out int candidateLineupDistance) ||
                    candidateLineupDistance >= currentLineupDistance)
                {
                    continue;
                }

                targetDistanceMap.TryGetValue(candidate, out int candidateTargetDistance);
                int distancePenalty = Mathf.Abs(candidateTargetDistance - preferredDistance);
                int score = candidateLineupDistance * 100 + distancePenalty;

                if (score < bestScore)
                {
                    bestScore = score;
                    bestDirections.Clear();
                    bestDirections.Add(direction);
                }
                else if (score == bestScore)
                {
                    bestDirections.Add(direction);
                }
            }

            return TryPickRandomDirection(context, bestDirections, out bestDirection);
        }

        private static bool TryFindSameDistanceMove(
            EnemyTurnContext context,
            Dictionary<Vector2Int, int> distanceMap,
            int currentDistance,
            out string bestDirection)
        {
            bestDirection = "";
            List<string> sameDistanceDirections = new List<string>();

            foreach (string direction in HexGridManager.HexDirections)
            {
                Vector2Int candidate = HexGridManager.MoveHex(context.SimulatedPosition, direction, 1);
                if (!context.IsOpen(candidate) || !TryGetDistance(distanceMap, candidate, out int candidateDistance))
                    continue;

                if (candidateDistance == currentDistance)
                    sameDistanceDirections.Add(direction);
            }

            return TryPickRandomDirection(context, sameDistanceDirections, out bestDirection);
        }

        private static bool TryPickRandomDirection(
            EnemyTurnContext context,
            List<string> directions,
            out string direction)
        {
            direction = "";
            if (directions == null || directions.Count == 0)
                return false;

            int index = context.PlanningRandom.Next(0, directions.Count);
            direction = directions[index];
            return true;
        }

        private static Dictionary<Vector2Int, int> BuildDirectLineDistanceMap(
            EnemyTurnContext context,
            AbstractEntity target,
            int attackRange)
        {
            Dictionary<Vector2Int, int> combinedMap = new Dictionary<Vector2Int, int>();
            if (HexGridManager.Instance == null || target == null)
                return combinedMap;

            List<Vector2Int> blockers = context.GetBlockers();
            Vector2Int targetPosition = context.GetEntityPosition(target);

            foreach (Vector2Int boardPosition in HexGridManager.Instance.BoardDictionary.Keys)
            {
                if (boardPosition != context.SimulatedPosition && !context.IsOpen(boardPosition))
                    continue;

                if (!EnemyBrainLine.TryFindDirectLine(
                        context,
                        boardPosition,
                        targetPosition,
                        attackRange,
                        out _,
                        out _))
                {
                    continue;
                }

                Dictionary<Vector2Int, int> distanceMap =
                    HexGridManager.Instance.CalculateDistanceMap(boardPosition, blockers);

                foreach (KeyValuePair<Vector2Int, int> entry in distanceMap)
                {
                    if (entry.Value < 0)
                        continue;

                    if (!combinedMap.TryGetValue(entry.Key, out int currentDistance) ||
                        entry.Value < currentDistance)
                    {
                        combinedMap[entry.Key] = entry.Value;
                    }
                }
            }

            return combinedMap;
        }

        private static bool CanMove(EnemyTurnContext context, AbstractEntity target)
        {
            return context != null &&
                   context.Self != null &&
                   target != null &&
                   context.MoveBudget > 0 &&
                   HexGridManager.Instance != null;
        }

        private static int GetMovesAllowed(EnemyTurnContext context, int maxMoves)
        {
            int ruleLimit = maxMoves <= 0 ? context.MoveBudget : maxMoves;
            return Mathf.Min(context.MoveBudget, ruleLimit);
        }

        private static Dictionary<Vector2Int, int> GetDistanceMap(EnemyTurnContext context, AbstractEntity target)
        {
            if (HexGridManager.Instance == null || target == null)
                return new Dictionary<Vector2Int, int>();

            return HexGridManager.Instance.CalculateDistanceMap(
                context.GetEntityPosition(target),
                context.GetBlockers(target));
        }

        private static bool TryGetDistance(
            Dictionary<Vector2Int, int> distanceMap,
            Vector2Int position,
            out int distance)
        {
            return distanceMap.TryGetValue(position, out distance) && distance >= 0;
        }
    }
}
