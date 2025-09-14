using Entities;

namespace Types.CardEvents
{
    public abstract class AbstractCardEvent
    {
        public abstract void Activate(AbstractEntity entity);
    }
}