using System.Collections.Generic;
using Entities;
using Cards.CardEvents;
using UnityEngine;

namespace Cards.Actions
{
    public class PushAllEnemiesAwayAction : AbstractAction
    {
        public int _amount;
        public PushAllEnemiesAwayAction(int baseCost, string color, AbstractEntity entity, int amount) : base(baseCost, color, entity)
        {
            this._amount = amount;
        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            return new List<AbstractCardEvent> { new PushEntityAwayCardEvent(_amount) };
        }

        public override List<RectTransform> UpdateGraphic(GameObject diagram, GameObject tilePrefab,
            GameObject arrowPrefab)
        {
            return new List<RectTransform>();
        }

        public override string GetText()
        {
            return "Push all enemies away " + _amount + " tile" + (_amount == 1 ? "" : "s");
        }

        public override string GetText(CardActionPreview preview)
        {
            int finalAmount = preview.GetTotalFinalValue(CardPreviewKeys.Distance, _amount);
            return "Push all enemies away " + preview.FormatValue("", _amount, finalAmount) + " tile" + (finalAmount == 1 ? "" : "s");
        }

        public override string ToString()
        {
            return "Push All Enemies Away " + _amount;
        }
    }
}
