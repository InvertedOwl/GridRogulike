using Entities;
using StateManager;

namespace Cards.CardEvents
{
    public class DiscardCardEvent : AbstractCardEvent
    {
        
        public DiscardCardEvent()
        {
        }

        
        public override void Activate(AbstractEntity entity)
        {
            if (entity.entityType == EntityType.Player)
            {
                Deck.Instance.DiscardRandomFromHand();
            }
                
        }
    }
}