using System.Collections.Generic;
using Entities;
using Grid;
using Cards.CardEvents;
using UnityEngine;

namespace Cards.Actions
{
    public class DelayedShieldAction : AbstractAction
    {
        public int _amount;
        public int Amount { get { return _amount; } set { _amount = value; } }
        public DelayedShieldAction(int baseCost, string color, AbstractEntity entity, int _amount) : base(baseCost, color, entity)
        {
            this._amount = _amount;

        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            entity.nextTurnActions.Add(new ShieldAction(0, "base", entity, _amount));
            
            return new List<AbstractCardEvent> {  };
        }

        public override string GetText()
        {
            return "Gain <shield>" + Amount + " on your next turn";
        }

        public override string ToSimpleText()
        {
            return "<sprite name=shield>" + Amount;
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