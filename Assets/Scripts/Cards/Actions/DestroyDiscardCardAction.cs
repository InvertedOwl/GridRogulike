using System.Collections.Generic;
using Entities;
using Grid;
using Cards.CardEvents;
using UnityEngine;

namespace Cards.Actions
{
    public class DestroyDiscardCardAction : AbstractAction
    {
        public DestroyDiscardCardAction(int baseCost, string color, AbstractEntity entity) : base(baseCost, color, entity)
        {
        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            return new List<AbstractCardEvent> { new DestroyCardEvent(Deck.Instance.Discard[_actionRandom.Next(0, Deck.Instance.Discard.Count)].Card.UniqueId) };
        }

        public override string GetText()
        {
            return "Permanently destroy a random card in discard";
        }
        
        public override List<RectTransform> UpdateGraphic(GameObject diagram, GameObject tilePrefab,
            GameObject arrowPrefab)
        {
            return new List<RectTransform>();
        }
        
        public override string ToString()
        {
            return "Destroy Random Card in Discard";
        }
        
    }
}