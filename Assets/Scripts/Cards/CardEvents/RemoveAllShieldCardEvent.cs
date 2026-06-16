using System.Collections.Generic;
using Entities;

namespace Cards.CardEvents
{
    public class RemoveAllShieldCardEvent : AbstractCardEvent
    {
        public override Dictionary<string, PreviewValue> GetPreviewValues()
        {
            return new Dictionary<string, PreviewValue>();
        }

        public override void Activate(AbstractEntity entity)
        {
            if (entity == null)
                return;

            entity.Shield = 0;
        }
    }
}
