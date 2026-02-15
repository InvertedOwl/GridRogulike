using Entities;
using StateManager;

namespace Cards.CardEvents
{
    public class DrawCardEvent : AbstractCardEvent
    {
        private int _cardsToDraw = 1;
        
        public DrawCardEvent(int cardsToDraw = 1)
        {
            this._cardsToDraw = cardsToDraw;
        }

        
        public override void Activate(AbstractEntity entity)
        {
            if (GameStateManager.Instance.GetCurrent<PlayingState>() is { } playing && entity.entityType == EntityType.Player)
            {
                Deck.Instance.FullDrawHand(_cardsToDraw);
            }
            
        }
    }
}