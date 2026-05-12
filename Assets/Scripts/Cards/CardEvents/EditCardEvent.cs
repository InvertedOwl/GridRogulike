using System.Collections.Generic;
using Cards.CardList;
using Entities;
using StateManager;

namespace Cards.CardEvents
{
    public class EditCardEvent : AbstractCardEvent
    {
        private CardMonobehaviour _cardMono;
        private Card _newCard;
        private bool _perma;
        private float _costOverride;
        
        public EditCardEvent(CardMonobehaviour cardMono, Card newCard, bool perma, float costOverride = -1f)
        {
            this._cardMono = cardMono;
            this._newCard = newCard;
            this._perma = perma;
            this._costOverride = costOverride;
        }

        public override Dictionary<string, PreviewValue> GetPreviewValues()
        {
            Dictionary<string, PreviewValue> values = new Dictionary<string, PreviewValue>
            {
                [CardPreviewKeys.CardId] = PreviewValue.Text(_newCard.UniqueId ?? "")
            };

            if (_costOverride > -1f)
                values[CardPreviewKeys.Cost] = PreviewValue.Float(_costOverride);

            return values;
        }
        
        public override void Activate(AbstractEntity entity)
        {
            if (!_perma && entity.entityType == EntityType.Player)
            {
                _cardMono.SetCard(_newCard, costOverride: _costOverride);
            }
        }
    }
}
