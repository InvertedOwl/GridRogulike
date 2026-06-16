using System.Collections.Generic;
using Entities;

namespace Cards.CardEvents
{
    public class IfPreviousEventDefeatedEnemyCardEvent : AbstractCardEvent
    {
        private readonly List<AbstractCardEvent> _events;

        public IfPreviousEventDefeatedEnemyCardEvent(List<AbstractCardEvent> events)
        {
            _events = events ?? new List<AbstractCardEvent>();
        }

        public override Dictionary<string, PreviewValue> GetPreviewValues()
        {
            Dictionary<string, PreviewValue> values = new Dictionary<string, PreviewValue>();

            foreach (AbstractCardEvent cardEvent in _events)
            {
                foreach (KeyValuePair<string, PreviewValue> kvp in cardEvent.GetPreviewValues())
                {
                    if (kvp.Value.Type != PreviewValueType.Int)
                    {
                        values[kvp.Key] = kvp.Value;
                        continue;
                    }

                    int currentValue = 0;
                    if (values.TryGetValue(kvp.Key, out PreviewValue existingValue))
                        existingValue.TryGetInt(out currentValue);

                    values[kvp.Key] = PreviewValue.Int(currentValue + kvp.Value.IntValue);
                }
            }

            return values;
        }

        public override void Activate(AbstractEntity entity)
        {
            ActivateWithResult(entity, new CardEventContext());
        }

        public override CardEventResult ActivateWithResult(AbstractEntity entity, CardEventContext context)
        {
            CardEventResult triggerResult = context?.PreviousResult;

            if (triggerResult?.DefeatedEnemy != true)
                return new CardEventResult(this);

            CardEventPipeline.ActivateResolved(_events, entity, context);
            return triggerResult;
        }
    }
}
