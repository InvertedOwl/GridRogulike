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
    public class ShieldCarryoverStatusSelfAction: AbstractAction
    {
        public int shieldcarryoverAmount;

        public ShieldCarryoverStatusSelfAction(int baseCost, string color, AbstractEntity entity, int shieldcarryoverAmount) : base(baseCost, color, entity)
        {
            this.shieldcarryoverAmount = shieldcarryoverAmount;
        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            return new List<AbstractCardEvent> { new ApplyStatusSelfCardEvent(new ShieldCarryoverStatus(shieldcarryoverAmount)) };
        }

        public override List<RectTransform> UpdateGraphic(GameObject diagram, GameObject tilePrefab, GameObject arrowPrefab)
        {
            return new List<RectTransform> { };
        }

        public override string GetText()
        {
            return  "Shield carries over between rounds for " + shieldcarryoverAmount + " turns";
        }

        public override string GetText(CardActionPreview preview)
        {
            int finalAmount = preview.GetTotalFinalValue(CardPreviewKeys.StatusAmount, shieldcarryoverAmount);
            return "Shield carries over between rounds for " + preview.FormatValue("", shieldcarryoverAmount, finalAmount) + " turns";
        }

        public override string ToString()
        {
            return "Shield Carry Over " + this.shieldcarryoverAmount;
        }
    }
}
