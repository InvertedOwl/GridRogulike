using System.Collections.Generic;
using Entities;
using Grid;
using Cards.CardEvents;
using UnityEngine;
using Util;

namespace Cards.Actions
{
    public class GainMoneyForCardAction : AbstractAction
    {
        public GainMoneyForCardAction(int baseCost, string color, AbstractEntity entity) : base(baseCost, color, entity)
        {

        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            // Subtract the current card being activated
            return new List<AbstractCardEvent> { new GainMoneyCardEvent(BattleStats.CardsPlayedThisTurn - 1) };
        }

        public override string GetText()
        {
            return "Gain $1 for every card played this turn $cardturn$";
        }
        
        public override List<RectTransform> UpdateGraphic(GameObject diagram, GameObject tilePrefab,
            GameObject arrowPrefab)
        {
            return new List<RectTransform>();
        }
        
        public override string ToString()
        {
            return "Money";
        }
        
    }
}