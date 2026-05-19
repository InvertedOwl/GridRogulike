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
    public class FrostSelfAction: AbstractAction
    {
        public int frostAmount;

        public FrostSelfAction(int baseCost, string color, AbstractEntity entity, int frostAmount) : base(baseCost, color, entity)
        {
            this.frostAmount = frostAmount;
        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            return new List<AbstractCardEvent> { new ApplyStatusSelfCardEvent(new FrostStatus(frostAmount)) };
        }

        public override List<RectTransform> UpdateGraphic(GameObject diagram, GameObject tilePrefab, GameObject arrowPrefab)
        {
            return new List<RectTransform> { };
        }

        public override string GetText()
        {
            return  "Apply " + frostAmount + " <frost> to self";
        }

        public override string GetText(CardActionPreview preview)
        {
            int finalAmount = preview.GetTotalFinalValue(CardPreviewKeys.StatusAmount, frostAmount);
            return "Apply " + preview.FormatValue("<frost>", frostAmount, finalAmount) + " to self";
        }

        public override string ToString()
        {
            return "Frost " + this.frostAmount;
        }
    }
}
