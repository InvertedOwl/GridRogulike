using System;
using Types.CardEvents;

namespace Types.CardModifiers
{
    public class DoNothingCardModifier : AbstractCardModifier
    {
        public DoNothingCardModifier()
        {
            this.ModifierText = "No Effect";
        }
        
        public override AbstractCardEvent Modify(AbstractCardEvent cardEvent)
        {
            return cardEvent;
        }
    }
}