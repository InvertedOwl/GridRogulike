using System.Collections.Generic;
using Entities;
using Grid;
using Cards.CardEvents;
using UnityEngine;

namespace Cards.Actions
{
    public class RemoveAllShieldAction : AbstractAction
    {
        public RemoveAllShieldAction(int baseCost, string color, AbstractEntity entity) : base(baseCost, color, entity)
        {
        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            return new List<AbstractCardEvent> { new RemoveAllShieldCardEvent()};
        }

        public override string GetText()
        {
            return "Remove all shield.";
        }

        public override string GetText(CardActionPreview preview)
        {
            return "Remove all shield.";
        }

        public override string ToSimpleText()
        {
            return "<shield> 0";
        }

        public override List<RectTransform> UpdateGraphic(GameObject diagram, GameObject tilePrefab,
            GameObject arrowPrefab)
        {
            return new List<RectTransform>();
        }

        public override string ToString()
        {
            return "Shield removed";
        }

    }
}
