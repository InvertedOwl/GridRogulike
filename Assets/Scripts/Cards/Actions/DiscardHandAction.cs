using System.Collections.Generic;
using Cards.CardEvents;
using Entities;
using UnityEngine;

namespace Cards.Actions
{
    public class DiscardHandAction : AbstractAction
    {
        public DiscardHandAction(int baseCost, string color, AbstractEntity entity) : base(baseCost, color, entity)
        {
        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            return new List<AbstractCardEvent> { new DiscardHandCardEvent() };
        }

        public override List<RectTransform> UpdateGraphic(GameObject diagram, GameObject tilePrefab, GameObject arrowPrefab)
        {
            return new List<RectTransform>();
        }

        public override string GetText()
        {
            return "Discard your hand";
        }

        public override string ToString()
        {
            return "Discard Hand";
        }
    }
}
