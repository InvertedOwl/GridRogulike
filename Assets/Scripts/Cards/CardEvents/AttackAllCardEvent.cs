using Entities;
using Grid;
using StateManager;
using Types.Statuses;

namespace Cards.CardEvents
{
    public class AttackAllCardEvent: AbstractCardEvent
    {
        public int amount;
        public AbstractStatus status;

        public AttackAllCardEvent(int amount, AbstractStatus status = null)
        {
            this.amount = amount;
            this.status = status;
        }

        
        public override void Activate(AbstractEntity entity)
        {
            if (GameStateManager.Instance.GetCurrent<PlayingState>() is { } playing)
            {
                foreach (AbstractEntity enemy in playing.GetEntities())
                {
                    if (entity == enemy)
                        continue;
                    playing.DamageEntities(enemy.positionRowCol, amount, status);
                }
                
            }
            
        }
    }
}