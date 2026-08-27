using System.Collections.Generic;
using Cards.CardEvents;
using Entities;
using UnityEngine;

namespace Cards.Actions
{
    public class ShieldFixedEntityAction : AbstractAction
    {
        public AbstractEntity target;
        public int Amount { get; }

        public ShieldFixedEntityAction(
            int baseCost,
            string color,
            AbstractEntity entity,
            AbstractEntity target,
            int amount) : base(baseCost, color, entity)
        {
            this.target = target;
            Amount = amount;
        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            if (target == null || target.Health <= 0)
                return new List<AbstractCardEvent>();

            return new List<AbstractCardEvent>
            {
                new ShieldCardEvent(Amount, target)
            };
        }

        public override string GetText()
        {
            return $"Give {Amount} <shield> to target";
        }

        public override string GetText(CardActionPreview preview)
        {
            int finalAmount = preview.GetTotalFinalValue(CardPreviewKeys.Shield, Amount);
            return $"Give {preview.FormatValue("<shield>", Amount, finalAmount)} to target";
        }

        public override string ToSimpleText()
        {
            return $"{Amount} <sprite name=shield>";
        }

        public override List<RectTransform> UpdateGraphic(
            GameObject diagram,
            GameObject tilePrefab,
            GameObject arrowPrefab)
        {
            return new List<RectTransform>();
        }

        public override string ToString()
        {
            return $"Shield target for {Amount}";
        }
    }
}
