using Entities;

namespace Cards.CardEvents
{
    public abstract class AbstractCardEvent
    {
        public abstract void Activate(AbstractEntity entity);
    }
}