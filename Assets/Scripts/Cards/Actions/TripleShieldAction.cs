using System.Collections.Generic;
using Cards.CardEvents;
using Entities;
using UnityEngine;

namespace Cards.Actions
{
    public class TripleShieldAction : AbstractAction
    {
        public TripleShieldAction(int baseCost, string color, AbstractEntity entity) : base(baseCost, color, entity)
        {
        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            return new List<AbstractCardEvent> { new ShieldCardEvent(CurrentShieldAmount() * 2) };
        }

        public override string GetText()
        {
            return "Triple your shield";
        }

        public override string GetText(CardActionPreview preview)
        {
            return "Triple your shield";
        }

        public override List<RectTransform> UpdateGraphic(GameObject diagram, GameObject tilePrefab,
            GameObject arrowPrefab)
        {
            return new List<RectTransform>();
        }

        public override string ToString()
        {
            return "Triple Shield";
        }

        private int CurrentShieldAmount()
        {
            AbstractEntity source = entity;
            if (source == null)
                return 0;

            return Mathf.FloorToInt(source.Shield);
        }
    }
}
