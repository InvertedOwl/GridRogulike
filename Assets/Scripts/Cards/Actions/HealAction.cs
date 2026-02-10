using System.Collections.Generic;
using Entities;
using Grid;
using Cards.CardEvents;
using UnityEngine;

namespace Cards.Actions
{
    public class HealAction : AbstractAction
    {
        private int _heal;
        
        public HealAction(int baseCost, string color, AbstractEntity entity, int heal) : base(baseCost, color, entity)
        {
            this._heal = heal;
        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            return new List<AbstractCardEvent>
            {
                new HealCardEvent(_heal)
            };
        }

        public override string GetText()
        {
            return "Heal " + _heal.ToString();
        }
        
        public override List<RectTransform> UpdateGraphic(GameObject diagram, GameObject tilePrefab,
            GameObject arrowPrefab)
        {
            return new List<RectTransform>();
        }
        
        public override string ToString()
        {
            return "raise cost";
        }
        
    }
}