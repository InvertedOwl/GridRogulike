using Entities;
using StateManager;

namespace Cards.CardEvents
{
    public class GainStepsCardEvent : AbstractCardEvent
    {
        private int _stepsToAdd = 1;
        
        public GainStepsCardEvent(int stepsToAdd = 1)
        {
            this._stepsToAdd = stepsToAdd;
        }

        
        public override void Activate(AbstractEntity entity)
        {
            if (GameStateManager.Instance.GetCurrent<PlayingState>() is { } playing && entity.entityType == EntityType.Player)
            {
                RunInfo.Instance.CurrentSteps += _stepsToAdd;
            }
            
        }
    }
}