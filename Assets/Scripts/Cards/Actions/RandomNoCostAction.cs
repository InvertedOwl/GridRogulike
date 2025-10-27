using System.Collections.Generic;
using Entities;
using Grid;
using Cards.CardEvents;
using UnityEngine;

namespace Cards.Actions
{
    public class RandomNoCostAction : AbstractAction
    {
        public RandomNoCostAction(int baseCost, string color, AbstractEntity entity) : base(baseCost, color, entity)
        {

        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            float cost = 0;
            CardMonobehaviour card = Deck.Instance.Hand[_actionRandom.Next(0, Deck.Instance.Hand.Count)];
            return new List<AbstractCardEvent> { new EditCardEvent(card, card.Card, false, cost) };
        }

        public override string GetText()
        {
            return "Temporarily set a random Card held in hand to 0 Cost.";
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