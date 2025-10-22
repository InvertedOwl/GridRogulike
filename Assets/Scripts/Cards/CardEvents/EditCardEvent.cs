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

        
        public override void Activate(AbstractEntity entity)
        {
            if (!_perma)
            {
                _cardMono.SetCard(_newCard, costOverride: _costOverride);
            }
        }
    }
}