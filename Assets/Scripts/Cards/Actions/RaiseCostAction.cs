using System.Collections.Generic;
using Entities;
using Grid;
using Cards.CardEvents;
using UnityEngine;

namespace Cards.Actions
{
    public class RaiseCostAction : AbstractAction
    {
        public RaiseCostAction(int baseCost, string color, AbstractEntity entity) : base(baseCost, color, entity)
        {

        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            float cost = (cardMono.CostOverride > -1)
                ? cardMono.CostOverride + 1
                : cardMono.Card.Cost + 1;
            return new List<AbstractCardEvent> { new EditCardEvent(cardMono, cardMono.Card, false, cost) };
        }

        public override string GetText()
        {
            return "Raise cost of this card by 1";
        }

        public override string GetText(CardActionPreview preview)
        {
            if (preview.TryGetFirstModifiedValue(CardPreviewKeys.Cost, out PreviewValue value) &&
                value.TryGetFloat(out float cost))
            {
                return "Raise cost of this card by " + cost.ToString("0.#");
            }

            return GetText();
        }

        public override List<RectTransform> UpdateGraphic(GameObject diagram, GameObject tilePrefab,
            GameObject arrowPrefab)
        {
            return new List<RectTransform>();
        }

        public override string ToString()
        {
            return "raise cost";
        }

    }
}
