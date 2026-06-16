using System.Collections.Generic;
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

        public override Dictionary<string, PreviewValue> GetPreviewValues()
        {
            return new Dictionary<string, PreviewValue>
            {
                [CardPreviewKeys.Steps] = PreviewValue.Int(_stepsToAdd)
            };
        }


        public override void Activate(AbstractEntity entity)
        {
            if (GameStateManager.Instance.GetCurrent<PlayingState>() is { } playing && entity.entityType == EntityType.Player)
            {
                if (playing.IsPlayerMovementBlocked)
                    return;

                RunInfo.Instance.CurrentSteps += _stepsToAdd;
            }

        }
    }
}
