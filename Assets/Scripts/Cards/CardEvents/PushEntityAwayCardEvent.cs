using System.Collections.Generic;
using Entities;
using Grid;
using StateManager;
using UnityEngine;

namespace Cards.CardEvents
{
    public class PushEntityAwayCardEvent : AbstractCardEvent
    {
        public AbstractEntity target;
        public Vector2Int targetPosition;
        public int distance;
        public bool useTargetPosition;

        public PushEntityAwayCardEvent(AbstractEntity target, int distance)
        {
            this.target = target;
            this.distance = distance;
        }

        public PushEntityAwayCardEvent(Vector2Int targetPosition, int distance)
        {
            this.targetPosition = targetPosition;
            this.distance = distance;
            useTargetPosition = true;
        }
        
        public PushEntityAwayCardEvent(int distance)
        {
            this.distance = distance;
        }

        public PushEntityAwayCardEvent Copy()
        {
            PushEntityAwayCardEvent copy;
            if (useTargetPosition)
            {
                copy = new PushEntityAwayCardEvent(targetPosition, distance);
            }
            else if (target != null)
            {
                copy = new PushEntityAwayCardEvent(target, distance);
            }
            else
            {
                copy = new PushEntityAwayCardEvent(distance);
            }

            copy.PreviewSourceActionIndex = PreviewSourceActionIndex;
            return copy;
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
            if (entity == null || distance <= 0)
                return;

            if (GameStateManager.Instance.GetCurrent<PlayingState>() is not { } playing)
                return;

            List<AbstractEntity> entitiesToPush = GetTargetEntities(playing);
            if (entitiesToPush == null)
                return;

            entitiesToPush.RemoveAll(targetEntity =>
                targetEntity == null ||
                targetEntity == entity ||
                targetEntity.Health <= 0);

            if (entitiesToPush.Count == 0)
                return;

            PushAway(entity, entitiesToPush, playing);
        }

        private List<AbstractEntity> GetTargetEntities(PlayingState playing)
        {
            if (useTargetPosition)
            {
                playing.EntitiesOnHex(targetPosition, out List<AbstractEntity> entitiesOnHex);
                return entitiesOnHex;
            }

            if (target != null)
            {
                return new List<AbstractEntity> { target };
            }

            List<AbstractEntity> entities = new List<AbstractEntity>();
            foreach (AbstractEntity entity in playing.GetEntities())
            {
                if (entity.entityType == EntityType.Enemy)
                {
                    entities.Add(entity);
                }
            }

            return entities;
        }

        private void PushAway(AbstractEntity source, List<AbstractEntity> entitiesToPush, PlayingState playing)
        {
            foreach (AbstractEntity entityToPush in entitiesToPush)
            {
                for (int step = 0; step < distance; step++)
                {
                    if (!TryGetNextPushPosition(source.positionRowCol, entityToPush.positionRowCol, playing, out Vector2Int nextPosition))
                        break;

                    if (!playing.MoveEntity(entityToPush, nextPosition))
                        break;
                }
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

        private int DistanceFrom(Vector2Int start, Vector2Int target)
        {
            Dictionary<Vector2Int, int> distanceMap =
                HexGridManager.Instance.CalculateDistanceMap(start, new List<Vector2Int>());

            return distanceMap.TryGetValue(target, out int mappedDistance)
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
