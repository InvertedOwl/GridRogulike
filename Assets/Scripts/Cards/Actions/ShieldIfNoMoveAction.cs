using System.Collections.Generic;
using Entities;
using Grid;
using Cards.CardEvents;
using UnityEngine;
using Util;

namespace Cards.Actions
{
    public class ShieldIfNoMoveAction : AbstractAction
    {
        public int _amount;
        public int Amount { get { return _amount; } set { _amount = value; } }
        public ShieldIfNoMoveAction(int baseCost, string color, AbstractEntity entity, int _amount) : base(baseCost, color, entity)
        {
            this._amount = _amount;

        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            if (BattleStats.TilesMovedThisBattle == 0)
            {
                return new List<AbstractCardEvent> { new ShieldCardEvent(_amount) };
            }
            else
            {
                return new List<AbstractCardEvent>();
            }
        }

        public override string GetText()
        {
            return "Gain " + Amount + " <shield> if you have not moved this combat.";
        }

        public override string GetText(CardActionPreview preview)
        {
            int finalAmount = preview.GetTotalFinalValue(CardPreviewKeys.Shield, Amount);
            return "Gain " + preview.FormatValue("<shield>", Amount, finalAmount) + " if you have not moved this combat.";
        }

        public override string ToSimpleText()
        {
            return Amount + " <sprite name=shield>";
        }

        public override List<RectTransform> UpdateGraphic(GameObject diagram, GameObject tilePrefab,
            GameObject arrowPrefab)
        {
            return new List<RectTransform>();
        }

        public override string ToString()
        {
            return "Shield " + this._amount;
        }

    }
}
