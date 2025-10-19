using System.Collections.Generic;
using Entities;
using Grid;
using Cards.CardEvents;
using UnityEngine;

namespace Cards.Actions
{
    public class ShieldAction : AbstractAction
    {
        private int _amount;
        public int Amount { get { return _amount; } set { _amount = value; } }
        public ShieldAction(int baseCost, string color, AbstractEntity entity, int _amount) : base(baseCost, color, entity)
        {
            this._amount = _amount;

        }

        public override List<AbstractCardEvent> Activate()
        {
            return new List<AbstractCardEvent> { new ShieldCardEvent(_amount) };
        }

        public override void Hover()
        {

        }
        
        public override string GetText()
        {
            return Amount.ToString();
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