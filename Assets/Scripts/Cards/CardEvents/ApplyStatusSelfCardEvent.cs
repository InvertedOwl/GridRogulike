using System.Collections.Generic;
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

        public override Dictionary<string, PreviewValue> GetPreviewValues()
        {
            return new Dictionary<string, PreviewValue>
            {
                [CardPreviewKeys.StatusAmount] = PreviewValue.Int(status?.Amount ?? 0),
                [CardPreviewKeys.StatusName] = PreviewValue.Text(status?.GetType().Name ?? "")
            };
        }

        public override void Activate(AbstractEntity entity)
        {
            if (GameStateManager.Instance.GetCurrent<PlayingState>() is { } playing)
            {
                entity.ApplyStatus(status);
                StatusApplicationFx.TryPlay(status, entity);
            }

        }
    }
}
