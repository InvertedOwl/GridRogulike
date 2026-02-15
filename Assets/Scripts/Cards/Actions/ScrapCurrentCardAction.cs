using System.Collections.Generic;
using Entities;
using Cards.CardEvents;
using UnityEngine;

namespace Cards.Actions
{
    public class ScrapCurrentCardAction : AbstractAction
    {
        public ScrapCurrentCardAction(int baseCost, string color, AbstractEntity entity, bool visible = true) : base(baseCost, color, entity, visible) { }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            return new List<AbstractCardEvent> { new ScrapCardEvent(cardMono.Card.UniqueId) };
        }

        public override string GetText()
        {
            return "Scrap this card.";
        }
        
        public override List<RectTransform> UpdateGraphic(GameObject diagram, GameObject tilePrefab,
            GameObject arrowPrefab)
        {
            return new List<RectTransform>();
        }
        
        public override string ToString()
        {
            return "Scrap this card.";
        }
        
    }
}