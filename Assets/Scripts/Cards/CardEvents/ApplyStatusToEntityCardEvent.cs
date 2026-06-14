using System.Collections.Generic;
using Entities;
using Types.Statuses;

namespace Cards.CardEvents
{
    public class ApplyStatusToEntityCardEvent : AbstractCardEvent
    {
        public AbstractEntity target;
        public AbstractStatus status;

        public ApplyStatusToEntityCardEvent(AbstractEntity target, AbstractStatus status)
        {
            this.target = target;
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
            if (target == null || target.Health <= 0)
                return;

            target.ApplyStatus(status);
            StatusApplicationFx.TryPlay(status, target);
        }
    }
}
