using Entities;
using StateManager;

namespace Types.CardEvents
{
    public class DrawCardEvent : AbstractCardEvent
    {
        public DrawCardEvent()
        {
        }

        
        public override void Activate(AbstractEntity entity)
        {
            if (GameStateManager.Instance.GetCurrent<PlayingState>() is { } playing)
            {
                Deck.Instance.FullDrawHand(1);
            }
            
        }
    }
}