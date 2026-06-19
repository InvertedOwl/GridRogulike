using System.Collections.Generic;
using Cards.CardEvents;
using Entities;
using Grid;
using StateManager;
using UnityEngine;

namespace Cards.Actions
{
    public class PushNearbyEnemiesAwayAction : AbstractAction
    {
        private const int PushDistance = 1;

        public int Radius;

        public PushNearbyEnemiesAwayAction(int baseCost, string color, AbstractEntity entity, int radius)
            : base(baseCost, color, entity)
        {
            Radius = radius;
        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            List<AbstractCardEvent> cardEvents = new List<AbstractCardEvent>();

            if (entity == null ||
                GameStateManager.Instance == null ||
                !GameStateManager.Instance.IsCurrent<PlayingState>())
            {
                return cardEvents;
            }

            Dictionary<Vector2Int, int> distanceMap =
                HexGridManager.Instance.CalculateDistanceMap(entity.positionRowCol, new List<Vector2Int>());

            PlayingState playingState = GameStateManager.Instance.GetCurrent<PlayingState>();
            foreach (AbstractEntity target in playingState.GetEntities())
            {
                if (!playingState.IsPlayerAttackTarget(target))
                {
                    continue;
                }

                if (!distanceMap.TryGetValue(target.positionRowCol, out int distance))
                    continue;

                if (distance > 0 && distance <= Radius)
                    cardEvents.Add(new PushEntityAwayCardEvent(target, PushDistance));
            }

            return cardEvents;
        }

        public override List<AbstractCardEvent> Activate(CardPlayContext context)
        {
            List<AbstractCardEvent> cardEvents = new List<AbstractCardEvent>();

            if (context?.SourceEntity == null ||
                context.PlayingState == null ||
                HexGridManager.Instance == null)
            {
                return cardEvents;
            }

            Dictionary<Vector2Int, int> distanceMap =
                HexGridManager.Instance.CalculateDistanceMap(context.SourceEntity.positionRowCol, new List<Vector2Int>());

            foreach (AbstractEntity target in context.PlayingState.GetEntities())
            {
                if (!context.PlayingState.IsPlayerAttackTarget(target))
                    continue;

                if (!distanceMap.TryGetValue(target.positionRowCol, out int distance))
                    continue;

                if (distance > 0 && distance <= Radius)
                    cardEvents.Add(new PushEntityAwayCardEvent(target, PushDistance));
            }

            return cardEvents;
        }

        public override List<RectTransform> UpdateGraphic(GameObject diagram, GameObject tilePrefab, GameObject arrowPrefab)
        {
            return new List<RectTransform>();
        }

        public override string GetText()
        {
            return "Push enemies within " + Radius + " tile" + (Radius == 1 ? "" : "s") + " away";
        }

        public override string ToString()
        {
            return "Push Enemies Within " + Radius + " Away";
        }
    }
}
