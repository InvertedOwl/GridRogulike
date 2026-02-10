using Entities;
using StateManager;

namespace Cards.CardEvents
{
    public class ScrapCardEvent : AbstractCardEvent
    {
        private string cardId;
        
        public ScrapCardEvent(string cardId)
        {
            this.cardId = cardId;
        }

        
        public override void Activate(AbstractEntity entity)
        {
            if (entity is Player)
                Deck.Instance.ScrapCard(cardId);
        }
    }
}