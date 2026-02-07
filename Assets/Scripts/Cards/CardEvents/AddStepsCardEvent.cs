using Entities;
using StateManager;

namespace Cards.CardEvents
{
    public class AddStepsCardEvent : AbstractCardEvent
    {
        private int _stepsToAdd = 1;
        
        public AddStepsCardEvent(int stepsToAdd = 1)
        {
            this._stepsToAdd = stepsToAdd;
        }

        
        public override void Activate(AbstractEntity entity)
        {
            if (GameStateManager.Instance.GetCurrent<PlayingState>() is { } playing && entity is Player)
            {
                RunInfo.Instance.CurrentSteps += _stepsToAdd;
            }
            
        }
    }
}