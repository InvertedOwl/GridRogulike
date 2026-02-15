using System.Collections.Generic;
using Entities;
using Grid;
using Cards.CardEvents;
using StateManager;
using UnityEngine;

namespace Cards.Actions
{
    public class DoubleShieldAction : AbstractAction
    {
        public DoubleShieldAction(int baseCost, string color, AbstractEntity entity) : base(baseCost, color, entity)
        {
        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            
            return new List<AbstractCardEvent> { new ShieldCardEvent((int) GameStateManager.Instance.GetCurrent<PlayingState>().player.Shield) };
        }

        public override string GetText()
        {
            return "Double Current <s1hield>";
        }
        
        public override List<RectTransform> UpdateGraphic(GameObject diagram, GameObject tilePrefab,
            GameObject arrowPrefab)
        {
            return new List<RectTransform>();
        }
        
        public override string ToString()
        {
            return "Double Shield";
        }
        
    }
}