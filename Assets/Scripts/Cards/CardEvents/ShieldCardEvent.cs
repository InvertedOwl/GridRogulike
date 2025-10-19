using Entities;
using Grid;
using StateManager;

namespace Cards.CardEvents
{
    public class ShieldCardEvent: AbstractCardEvent
    {
        public int amount;
        

        public ShieldCardEvent(int amount)
        {
            this.amount = amount;
        }

        
        public override void Activate(AbstractEntity entity)
        {
            if (GameStateManager.Instance.GetCurrent<PlayingState>() is { } playing)
            {
                playing.player.shield += amount;
            }
            
        }
    }
}