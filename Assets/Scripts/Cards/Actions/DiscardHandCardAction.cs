using System.Collections.Generic;
using Entities;
using Cards.CardEvents;
using UnityEngine;

namespace Cards.Actions
{
    public class DiscardHandCardAction : AbstractAction
    {
        public DiscardHandCardAction(int baseCost, string color, AbstractEntity entity) : base(baseCost, color, entity)
        {
            
        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            return new List<AbstractCardEvent> { new DiscardHandCardEvent() };
        }


        public override List<RectTransform> UpdateGraphic(GameObject diagram, GameObject tilePrefab,
            GameObject arrowPrefab)
        {
            return new List<RectTransform>();
        }

        public override string GetText()
        {
            return "Discard Cards in Hand";
        }

        public override string ToString()
        {
            return "Draw Card ";
        }
    } }