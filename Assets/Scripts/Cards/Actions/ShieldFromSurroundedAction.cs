using System.Collections.Generic;
using Entities;
using Grid;
using Cards.CardEvents;
using UnityEngine;

namespace Cards.Actions
{
    public class ShieldFromSurroundedAction : AbstractAction
    {
        private const int ShieldPerAdjacentEnemy = 4;

        public ShieldFromSurroundedAction(int baseCost, string color, AbstractEntity entity) : base(baseCost, color, entity)
        {
        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMonobehaviour)
        {
            return new List<AbstractCardEvent>();
        }

        public override List<AbstractCardEvent> Activate(CardPlayContext context)
        {
            int amount = CalculateShieldAmount(context);
            return new List<AbstractCardEvent> { new ShieldCardEvent(amount) };
        }

        public override string GetText()
        {
            return "Gain " + ShieldPerAdjacentEnemy + " <shield> for every adjacent enemy";
        }

        public override string GetText(CardActionPreview preview)
        {
            int amount = preview.GetTotalBaseValue(CardPreviewKeys.Shield, 0);
            int finalAmount = preview.GetTotalFinalValue(CardPreviewKeys.Shield, amount);
            return "Gain " + ShieldPerAdjacentEnemy + " <shield> for every adjacent enemy (" +
                   preview.FormatValue("", amount, finalAmount) +
                   ")";
        }

        public override string ToSimpleText()
        {
            return ShieldPerAdjacentEnemy + " <sprite name=shield>";
        }

        public override List<RectTransform> UpdateGraphic(GameObject diagram, GameObject tilePrefab,
            GameObject arrowPrefab)
        {
            return new List<RectTransform>();
        }

        public override string ToString()
        {
            return "Shield From Surrounded";
        }

        private int CalculateShieldAmount(CardPlayContext context)
        {
            if (context?.PlayingState == null ||
                !TryGetReferencePosition(context, out Vector2Int position))
            {
                return 0;
            }

            return context.PlayingState.GetAdjacentEnemies(position).Count * ShieldPerAdjacentEnemy;
        }

        private bool TryGetReferencePosition(CardPlayContext context, out Vector2Int position)
        {
            if (context?.Targets != null && context.Targets.TryGetFirstPosition(out position))
                return true;

            if (context?.SourceEntity != null)
            {
                position = context.SourceEntity.positionRowCol;
                return true;
            }

            position = default;
            return false;
        }
    }
}
