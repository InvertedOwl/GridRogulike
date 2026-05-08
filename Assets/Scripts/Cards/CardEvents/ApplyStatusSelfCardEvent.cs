using Entities;
using Grid;
using StateManager;
using Types.Statuses;
using UnityEngine;

namespace Cards.CardEvents
{
    public class ApplyStatusSelfCardEvent: AbstractCardEvent
    {
        public AbstractStatus status;

        public ApplyStatusSelfCardEvent(AbstractStatus status)
        {

            this.status = status;
        }
        
        public override void Activate(AbstractEntity entity)
        {
            if (GameStateManager.Instance.GetCurrent<PlayingState>() is { } playing)
            {
                entity.ApplyStatus(status);
            }
            
        }
    }
}