using Types.CardEvents;

namespace Types.CardModifiers
{
    public abstract class AbstractCardModifier
    {
        public string ModifierText;
        public abstract AbstractCardEvent Modify(AbstractCardEvent cardEvent);
    }
}