using System.Collections.Generic;
using Cards.CardEvents;
using Entities;
using Grid;
using StateManager;
using UnityEngine;

namespace Cards.Actions
{
    public class GainStepsForNearbyEnemiesAction : AbstractAction
    {
        public int Radius;

        public GainStepsForNearbyEnemiesAction(int baseCost, string color, AbstractEntity entity, int radius = 1)
            : base(baseCost, color, entity)
        {
            Radius = radius;
        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            int steps = CountNearbyEnemies();
            return steps > 0
                ? new List<AbstractCardEvent> { new GainStepsCardEvent(steps) }
                : new List<AbstractCardEvent>();
        }

        public override string GetText()
        {
            return "Gain 1 <move> for each nearby enemy";
        }

        public override string GetText(CardActionPreview preview)
        {
            int steps = CountNearbyEnemies();
            int finalSteps = preview.GetTotalFinalValue(CardPreviewKeys.Steps, steps);
            return "Gain " + preview.FormatValue("<move>", steps, finalSteps) + " for nearby enemies";
        }

        public override List<RectTransform> UpdateGraphic(GameObject diagram, GameObject tilePrefab, GameObject arrowPrefab)
        {
            return new List<RectTransform>();
        }

        public override string ToString()
        {
            return "Gain Steps For Nearby Enemies";
        }

        private int CountNearbyEnemies()
        {
            AbstractEntity source = entity;
            if (source == null ||
                GameStateManager.Instance == null ||
                !GameStateManager.Instance.IsCurrent<PlayingState>() ||
                HexGridManager.Instance == null)
            {
                return 0;
            }

            Dictionary<Vector2Int, int> distanceMap =
                HexGridManager.Instance.CalculateDistanceMap(source.positionRowCol, new List<Vector2Int>());

            int count = 0;
            PlayingState playingState = GameStateManager.Instance.GetCurrent<PlayingState>();
            foreach (AbstractEntity target in playingState.GetEntities())
            {
                if (target == null ||
                    target == source ||
                    target.entityType != EntityType.Enemy ||
                    target.Health <= 0)
                {
                    continue;
                }

                if (!distanceMap.TryGetValue(target.positionRowCol, out int distance))
                    continue;

                if (distance > 0 && distance <= Radius)
                    count += 1;
            }

            return count;
        }
    }
}
