using System.Collections.Generic;
using Cards.CardEvents;
using Entities;
using UnityEngine;

namespace Cards.Actions
{
    public class DamageSelfAction : AbstractAction
    {
        public int Amount;

        public DamageSelfAction(int baseCost, string color, AbstractEntity entity, int amount)
            : base(baseCost, color, entity)
        {
            Amount = amount;
        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            return new List<AbstractCardEvent> { new DamageSelfCardEvent(Amount) };
        }

        public override string GetText()
        {
            return "Take " + Amount + " <attack>";
        }

        public override string GetText(CardActionPreview preview)
        {
            int finalAmount = preview.GetTotalFinalValue(CardPreviewKeys.SelfDamage, Amount);
            return "Take " + preview.FormatValue("<attack>", Amount, finalAmount);
        }

        public override List<RectTransform> UpdateGraphic(GameObject diagram, GameObject tilePrefab, GameObject arrowPrefab)
        {
            return new List<RectTransform>();
        }

        public override string ToString()
        {
            return "Damage Self " + Amount;
        }
    }
}
