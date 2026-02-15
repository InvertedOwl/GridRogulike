using Entities;
using StateManager;

namespace Cards.CardEvents
{
    public class DiscardHandCardEvent : AbstractCardEvent
    {
        
        public DiscardHandCardEvent()
        {
        }

        
        public override void Activate(AbstractEntity entity)
        {
            if (entity.entityType == EntityType.Player)
            {
                Deck.Instance.DiscardHand();
            }
                
        }
    }
}