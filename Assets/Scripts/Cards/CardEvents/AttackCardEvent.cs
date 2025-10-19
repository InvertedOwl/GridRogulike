using Entities;
using Grid;
using StateManager;
using Types.Statuses;

namespace Cards.CardEvents
{
    public class AttackCardEvent: AbstractCardEvent
    {
        public int distance;
        public string direction;
        public int amount;
        public AbstractStatus status;

        public AttackCardEvent(int distance, string direction, int amount, AbstractStatus status = null)
        {
            this.distance = distance;
            this.direction = direction;
            this.amount = amount;
            this.status = status;
        }

        
        public override void Activate(AbstractEntity entity)
        {
            if (GameStateManager.Instance.GetCurrent<PlayingState>() is { } playing)
            {
                playing.DamageEntities(HexGridManager.MoveHex(entity.positionRowCol, this.direction, distance), amount, status);
            }
            
        }
    }
}