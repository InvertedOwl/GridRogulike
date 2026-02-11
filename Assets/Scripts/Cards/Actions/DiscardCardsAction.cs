using System.Collections.Generic;
using Entities;
using Cards.CardEvents;
using UnityEngine;

namespace Cards.Actions
{
    public class DiscardCardsAction : AbstractAction
    {
        private int _numCards;
        public DiscardCardsAction(int baseCost, string color, AbstractEntity entity, int numCards) : base(baseCost, color, entity)
        {
            _numCards = numCards;
        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            List<AbstractCardEvent> cardEvents = new List<AbstractCardEvent>();
            for (int i = 0; i < _numCards; i++)
            {
                cardEvents.Add(new DiscardCardEvent());
            }

            return cardEvents;
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