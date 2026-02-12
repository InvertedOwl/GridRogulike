using System.Collections.Generic;
using Entities;
using Grid;
using Cards.CardEvents;
using UnityEngine;

namespace Cards.Actions
{
    public class GainEnergyAction : AbstractAction
    {
        private int _amount;
        public int Amount { get { return _amount; } set { _amount = value; } }
        public GainEnergyAction(int baseCost, string color, AbstractEntity entity, int amount) : base(baseCost, color, entity)
        {
            this._amount = amount;

        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            return new List<AbstractCardEvent> { new GainEnergyCardEvent(_amount) };
        }

        public override string GetText()
        {
            return _amount + "<energy>";
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