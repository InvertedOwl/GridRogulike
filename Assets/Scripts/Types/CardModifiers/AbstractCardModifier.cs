using System.Collections.Generic;
using Cards.CardEvents;

namespace Types.CardModifiers
{
    public abstract class AbstractCardModifier
    {
        public string ModifierText;
        public Rarity Rarity;
        public abstract List<AbstractCardEvent> Modify(List<AbstractCardEvent> cardEvent);
    }
}