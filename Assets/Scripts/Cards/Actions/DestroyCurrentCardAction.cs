using System.Collections.Generic;
using Entities;
using Cards.CardEvents;
using UnityEngine;

namespace Cards.Actions
{
    public class DestroyCurrentCardAction : AbstractAction
    {
        public DestroyCurrentCardAction(int baseCost, string color, AbstractEntity entity) : base(baseCost, color, entity) { }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            return new List<AbstractCardEvent> { new DestroyCardEvent(cardMono.Card.UniqueId) };
        }

        public override string GetText()
        {
            return "Permanently destroy this card.";
        }
        
        public override List<RectTransform> UpdateGraphic(GameObject diagram, GameObject tilePrefab,
            GameObject arrowPrefab)
        {
            return new List<RectTransform>();
        }
        
        public override string ToString()
        {
            return "Permanently destroy this card.";
        }
        
    }
}