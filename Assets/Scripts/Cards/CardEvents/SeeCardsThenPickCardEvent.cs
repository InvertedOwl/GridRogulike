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
            if (Deck.Instance == null || DeckView.Instance == null)
                return;

            int cardsToShow = Mathf.Min(Amount, Deck.Instance.Draw.Count);
            if (cardsToShow <= 0)
                return;

            List<CardMonobehaviour> topCardMonos = Deck.Instance.Draw.GetRange(0, cardsToShow);
            List<Card> topCards = new List<Card>();
            foreach (CardMonobehaviour cardMono in topCardMonos)
            {
                topCards.Add(cardMono.Card);
            }
            
            DeckView.Instance.GetCardWhitelist((card) =>
            {
                if (!card.isReal)
                    return;

                Debug.Log("Chose card: " + card.CardName);
                Deck.Instance.DrawCard(card);
            }, topCards.ToArray());
        }
    }
}
