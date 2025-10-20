using System.Collections.Generic;
using Entities;
using Grid;
using Cards.CardEvents;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Cards.Actions
{
    public class DestroyHandCardAction : AbstractAction
    {
        public DestroyHandCardAction(int baseCost, string color, AbstractEntity entity) : base(baseCost, color, entity)
        {
        }

        public override List<AbstractCardEvent> Activate()
        {
            return new List<AbstractCardEvent> { new DestroyCardEvent(Deck.Instance.Hand[Random.Range(0, Deck.Instance.Hand.Count)].Card.UniqueId) };
        }

        public override void Hover()
        {

        }
        
        public override string GetText()
        {
            return "Permanently destroy a random card in hand";
        }
        
        public override List<RectTransform> UpdateGraphic(GameObject diagram, GameObject tilePrefab,
            GameObject arrowPrefab)
        {
            return new List<RectTransform>();
        }
        
        public override string ToString()
        {
            return "Destroy Random Card in Hand";
        }
        
    }
}