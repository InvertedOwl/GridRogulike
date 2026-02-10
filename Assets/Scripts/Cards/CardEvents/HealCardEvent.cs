using Entities;
using StateManager;

namespace Cards.CardEvents
{
    public class HealCardEvent : AbstractCardEvent
    {
        private int _heal;
        
        public HealCardEvent(int heal)
        {
            this._heal = heal;
        }

        
        public override void Activate(AbstractEntity entity)
        {
            if (GameStateManager.Instance.GetCurrent<PlayingState>() is { } playing && entity is Player)
            {
                entity.Health += this._heal;
            }
            
        }
    }
}