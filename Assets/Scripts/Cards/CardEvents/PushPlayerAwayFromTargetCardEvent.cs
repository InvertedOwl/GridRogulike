using System.Collections.Generic;
using Entities;
using Grid;
using StateManager;
using UnityEngine;

namespace Cards.CardEvents
{
    public class PushPlayerAwayFromTargetCardEvent : AbstractCardEvent
    {
        public AbstractEntity target;
        public int distance;

        public PushPlayerAwayFromTargetCardEvent(AbstractEntity target, int distance)
        {
            this.target = target;
            this.distance = distance;
        }

        public override Dictionary<string, PreviewValue> GetPreviewValues()
        {
            return new Dictionary<string, PreviewValue>
            {
                [CardPreviewKeys.Distance] = PreviewValue.Int(distance),
                [CardPreviewKeys.Direction] = PreviewValue.Text("away")
            };
        }

        public override void Activate(AbstractEntity entity)
        {
            if (entity == null ||
                entity.entityType != EntityType.Player ||
                target == null ||
                target.Health <= 0 ||
                distance <= 0)
            {
                return;
            }

            if (GameStateManager.Instance.GetCurrent<PlayingState>() is not { } playing)
                return;

            for (int step = 0; step < distance; step++)
            {
                if (!TryGetNextPushPosition(target.positionRowCol, entity.positionRowCol, playing, out Vector2Int nextPosition))
                    break;

                if (!playing.MoveEntity(entity, nextPosition))
                    break;
            }
        }

        private bool TryGetNextPushPosition(
            Vector2Int sourcePosition,
            Vector2Int pushedPosition,
            PlayingState playing,
            out Vector2Int nextPosition)
        {
            nextPosition = pushedPosition;
            int currentDistance = DistanceFrom(sourcePosition, pushedPosition);
            int bestDistance = currentDistance;
            float bestAlignment = float.NegativeInfinity;
            Vector3 awayDirection = GetCubeDelta(sourcePosition, pushedPosition);
            if (awayDirection == Vector3.zero)
                return false;

            foreach (string direction in HexGridManager.HexDirections)
            {
                Vector2Int candidate = HexGridManager.MoveHex(pushedPosition, direction, 1);

                if (!playing.IsValidHex(candidate))
                    continue;

                int candidateDistance = DistanceFrom(sourcePosition, candidate);
                if (candidateDistance <= currentDistance)
                    continue;

                Vector3 candidateDirection = GetCubeDelta(pushedPosition, candidate);
                float alignment = Vector3.Dot(candidateDirection.normalized, awayDirection.normalized);

                if (alignment > bestAlignment ||
                    (Mathf.Approximately(alignment, bestAlignment) && candidateDistance > bestDistance))
                {
                    bestDistance = candidateDistance;
                    bestAlignment = alignment;
                    nextPosition = candidate;
                }
            }

            return bestDistance > currentDistance;
        }

        private int DistanceFrom(Vector2Int start, Vector2Int targetPosition)
        {
            Dictionary<Vector2Int, int> distanceMap =
                HexGridManager.Instance.CalculateDistanceMap(start, new List<Vector2Int>());

            return distanceMap.TryGetValue(targetPosition, out int mappedDistance)
                ? mappedDistance
                : int.MaxValue;
        }

        private Vector3 GetCubeDelta(Vector2Int from, Vector2Int to)
        {
            return OffsetToCube(to) - OffsetToCube(from);
        }

        private Vector3 OffsetToCube(Vector2Int offset)
        {
            int rowParity = Mathf.Abs(offset.y % 2);
            int q = offset.x - ((offset.y - rowParity) / 2);
            int r = offset.y;
            int s = -q - r;

            return new Vector3(q, r, s);
        }
    }
}
