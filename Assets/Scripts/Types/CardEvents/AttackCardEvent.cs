using Entities;
using Grid;
using StateManager;

namespace Types.CardEvents
{
    public class AttackCardEvent: AbstractCardEvent
    {
        public int distance;
        public string direction;
        public int amount;
        // TODO: Add Elemental Damage, and effects

        

        public AttackCardEvent(int distance, string direction, int amount)
        {
            this.distance = distance;
            this.direction = direction;
            this.amount = amount;
        }

        
        public override void Activate(AbstractEntity entity)
        {
            if (GameStateManager.Instance.GetCurrent<PlayingState>() is { } playing)
            {
                playing.DamageEntities(HexGridManager.MoveHex(entity.positionRowCol, this.direction, distance), amount);
            }
            
        }
    }
}