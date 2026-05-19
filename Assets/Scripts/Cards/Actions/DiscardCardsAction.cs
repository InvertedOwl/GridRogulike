using System.Collections.Generic;
using Entities;
using Cards.CardEvents;
using UnityEngine;

namespace Cards.Actions
{
    public class DiscardCardsAction : AbstractAction
    {
        public int _numCards;
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
            return "Discard " + _numCards + " Random Card" + (_numCards == 1?"":"s") +  " in Hand";
        }

        public override string GetText(CardActionPreview preview)
        {
            int finalAmount = preview.GetTotalFinalValue(CardPreviewKeys.Discard, _numCards);
            return "Discard " + preview.FormatValue("", _numCards, finalAmount) + " Card" + (finalAmount == 1 ? "" : "s") + " in Hand";
        }

        public override string ToString()
        {
            return "Draw Card ";
        }
    } }
