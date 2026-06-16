using System.Collections.Generic;
using Entities;
using Grid;
using StateManager;
using Cards.CardEvents;
using Types.Statuses;
using UnityEngine;
using UnityEngine.UI;
using Util;

namespace Cards.Actions
{
    public class RangedSelfAction: AbstractAction
    {
        public int rangedAmount;

        public RangedSelfAction(int baseCost, string color, AbstractEntity entity, int rangedAmount) : base(baseCost, color, entity)
        {
            this.rangedAmount = rangedAmount;
        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            return new List<AbstractCardEvent> { new ApplyStatusSelfCardEvent(new RangedStatus(rangedAmount)) };
        }

        public override List<RectTransform> UpdateGraphic(GameObject diagram, GameObject tilePrefab, GameObject arrowPrefab)
        {
            return new List<RectTransform> { };
        }

        public override string GetText()
        {
            return  "Your next attack has +" + rangedAmount + " range";
        }

        public override string GetText(CardActionPreview preview)
        {
            int finalAmount = preview.GetTotalFinalValue(CardPreviewKeys.StatusAmount, rangedAmount);
            return "Your next attack has +" + preview.FormatValue("", rangedAmount, finalAmount) + " range";
        }

        public override string ToString()
        {
            return "Ranged " + this.rangedAmount;
        }
    }
}
