using Entities;
using Grid;
using Passives;
using StateManager;
using Types.Statuses;

namespace Cards.CardEvents
{
    public class SpawnPassiveEvent: AbstractCardEvent
    {
        private PassiveEntry _passive;

        public SpawnPassiveEvent(PassiveEntry passive)
        {
            this._passive = passive;
        }

        
        public override void Activate(AbstractEntity entity)
        {
            if (GameStateManager.Instance.GetCurrent<PlayingState>() is { } playing)
            {
                EnvironmentManager.instance.AddPassive(_passive);
            }
            
        }
    }
}