using System.Collections.Generic;
using Entities;
using Grid;
using Cards.CardEvents;
using UnityEngine;
using Util;

namespace Cards.Actions
{
    public class GainShieldForMoneyAction : AbstractAction
    {
        public GainShieldForMoneyAction(int baseCost, string color, AbstractEntity entity) : base(baseCost, color, entity)
        {

        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            // Subtract the current card being activated
            return new List<AbstractCardEvent> { new ShieldCardEvent(RunInfo.Instance.Money) };
        }

        public override string GetText()
        {
            return "Gain <shield> equal to amount of money you have (" + RunInfo.Instance.Money + ")";
        }

        public override string GetText(CardActionPreview preview)
        {
            int amount = RunInfo.Instance.Money;
            int finalAmount = preview.GetTotalFinalValue(CardPreviewKeys.Shield, amount);
            return "Gain <shield> equal to amount of money you have (" + preview.FormatValue("", amount, finalAmount) + ")";
        }

        public override List<RectTransform> UpdateGraphic(GameObject diagram, GameObject tilePrefab,
            GameObject arrowPrefab)
        {
            return new List<RectTransform>();
        }

        public override string ToString()
        {
            return "Money";
        }

    }
}
