using System.Collections.Generic;
using System.Linq;
using Cards.CardStatuses;
using Entities;
using UnityEngine;

namespace Cards.CardEvents
{
    public class AddCardStatusToRandomHandCardEvent : AbstractCardEvent
    {
        private readonly string _statusKey;
        private readonly RandomState _random;
        private readonly bool _includeUsedCards;

        public AddCardStatusToRandomHandCardEvent(
            string statusKey,
            RandomState random = null,
            bool includeUsedCards = false)
        {
            _statusKey = statusKey;
            _random = random;
            _includeUsedCards = includeUsedCards;
        }

        public override Dictionary<string, PreviewValue> GetPreviewValues()
        {
            return new Dictionary<string, PreviewValue>
            {
                [CardPreviewKeys.CardCount] = PreviewValue.Int(1),
                [CardPreviewKeys.StatusName] = PreviewValue.Text(_statusKey ?? "")
            };
        }

        public override void Activate(AbstractEntity entity)
        {
            if (entity == null || entity.entityType != EntityType.Player || Deck.Instance == null)
                return;

            if (string.IsNullOrWhiteSpace(_statusKey))
                return;

            List<CardMonobehaviour> validCards = Deck.Instance.Hand
                .Where(card => card != null &&
                               !card.onlyDisplay &&
                               !card.played &&
                               (_includeUsedCards || !card.used))
                .ToList();

            if (validCards.Count == 0)
                return;

            RandomState random = _random ?? RunInfo.NewRandom("addcardstatustorandomhandcard");
            CardMonobehaviour targetCard = validCards[random.Next(0, validCards.Count)];

            if (targetCard.cardStatusDatabase == null)
            {
                Debug.LogWarning("Cannot add card status because the target card has no CardStatusDatabase.");
                return;
            }

            CardStatusDatabase.CardStatus cardStatus = targetCard.cardStatusDatabase.Get(_statusKey);
            if (cardStatus == null)
            {
                Debug.LogWarning("Card status not found: " + _statusKey);
                return;
            }

            targetCard.SetCardStatus(cardStatus);
            Deck.Instance.MarkPlayabilityDirty();
        }
    }
}
