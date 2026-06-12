using System.Collections.Generic;
using Entities;
using UnityEngine;

namespace Cards.CardEvents
{
    public class SeeCardsThenPickCardEvent : AbstractCardEvent
    {
        public int Amount { get; }

        public SeeCardsThenPickCardEvent(int amount)
        {
            Amount = amount;
        }

        public override Dictionary<string, PreviewValue> GetPreviewValues()
        {
            return new Dictionary<string, PreviewValue>
            {
                [CardPreviewKeys.Draw] = PreviewValue.Int(Amount)
            };
        }

        public override void Activate(AbstractEntity entity)
        {
            List<CardMonobehaviour> topCardMonos = Deck.Instance.Draw.GetRange(0, 3);
            List<Card> topCards = new List<Card>();
            foreach (CardMonobehaviour cardMono in topCardMonos)
            {
                topCards.Add(cardMono.Card);
            }
            
            DeckView.Instance.GetCardWhitelist((card) =>
            {
                Debug.Log("Chose card: " + card.CardName);
                Deck.Instance.DrawCard(card);
            }, topCards.ToArray());
        }
    }
}
