using System.Collections.Generic;
using Entities;
using Grid;
using Cards.CardEvents;
using UnityEngine;
using Util;

namespace Cards.Actions
{
    public class FirstCardEnergyCardAction : AbstractAction
    {
        public int _amount;
        public int Amount { get { return _amount; } set { _amount = value; } }
        public FirstCardEnergyCardAction(int baseCost, string color, AbstractEntity entity, int amount) : base(baseCost, color, entity)
        {
            this._amount = amount;

        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            if (BattleStats.CardsPlayedThisTurn == 0)
            {
                return new List<AbstractCardEvent> { new GainEnergyCardEvent(_amount) };
            }
            else
            {
                return new List<AbstractCardEvent>();
            }
        }

        public override string GetText()
        {
            return "Gain " + _amount + " <energy> if this is the first card played this turn.";
        }

        public override string GetText(CardActionPreview preview)
        {
            int finalAmount = preview.GetTotalFinalValue(CardPreviewKeys.Energy, _amount);
            return "Gain " + preview.FormatValue("<energy>", _amount, finalAmount) +
                   " if this is the first card played this turn.";
        }

        public override List<RectTransform> UpdateGraphic(GameObject diagram, GameObject tilePrefab,
            GameObject arrowPrefab)
        {
            return new List<RectTransform>();
        }

        public override string ToString()
        {
            return "Energy " + this._amount;
        }

    }
}
