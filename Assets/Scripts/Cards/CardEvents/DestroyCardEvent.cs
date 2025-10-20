using Entities;
using StateManager;

namespace Cards.CardEvents
{
    public class DestroyCardEvent : AbstractCardEvent
    {
        private string cardId;
        
        public DestroyCardEvent(string cardId)
        {
            this.cardId = cardId;
        }

        
        public override void Activate(AbstractEntity entity)
        {
            Deck.Instance.DestroyCard(cardId);
        }
    }
}