using Entities;
using StateManager;

namespace Types.CardEvents
{
    public class MoveCardEvent : AbstractCardEvent
    {
        public int distance;
        public string direction;

        public MoveCardEvent(int distance, string direction)
        {
            this.distance = distance;
            this.direction = direction;
        }

        public override void Activate(AbstractEntity entity)
        {
            if (!entity) return;
            
            if (GameStateManager.Instance.GetCurrent<PlayingState>() is { } playing)
                playing.MoveEntity(entity, this.direction, distance);
            
        }
    }
}