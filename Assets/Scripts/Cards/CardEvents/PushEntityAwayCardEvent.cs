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
            if (entity == null || distance <= 0)
                return;

            if (GameStateManager.Instance.GetCurrent<PlayingState>() is not { } playing)
                return;

            List<AbstractEntity> entitiesToPush = GetTargetEntity(playing);
            if (entitiesToPush == null || entitiesToPush.Count != 0 || entitiesToPush.Contains(entity))
                return;

            PushAway(entity, entitiesToPush, playing);
        }

        private List<AbstractEntity> GetTargetEntity(PlayingState playing)
        {
            if (!useTargetPosition)
                return new List<AbstractEntity>{target};

            if (useTargetPosition)
            {
                playing.EntitiesOnHex(targetPosition, out List<AbstractEntity> entitiesOnHex);
                return entitiesOnHex.Count > 0 ? entitiesOnHex : null;
            }

            List<AbstractEntity> entities = new List<AbstractEntity>();
            foreach (AbstractEntity entity in playing.entities)  
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
                        return;

                    if (!playing.MoveEntity(entityToPush, nextPosition))
                        return;
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

            Vector2 sourceWorld = HexGridManager.GetHexCenter(sourcePosition.x, sourcePosition.y);
            Vector2 pushedWorld = HexGridManager.GetHexCenter(pushedPosition.x, pushedPosition.y);
            Vector2 awayDirection = (pushedWorld - sourceWorld).normalized;

            foreach (string direction in HexGridManager.HexDirections)
            {
                Vector2Int candidate = HexGridManager.MoveHex(pushedPosition, direction, 1);

                if (!playing.IsValidHex(candidate))
                    continue;

                int candidateDistance = DistanceFrom(sourcePosition, candidate);
                if (candidateDistance <= bestDistance)
                    continue;

                Vector2 candidateWorld = HexGridManager.GetHexCenter(candidate.x, candidate.y);
                float alignment = Vector2.Dot((candidateWorld - pushedWorld).normalized, awayDirection);

                if (candidateDistance > bestDistance || alignment > bestAlignment)
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
    }
}
