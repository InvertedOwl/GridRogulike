using System;
using Types.CardEvents;

namespace Types.CardModifiers
{
    public class DoNothingCardModifier : AbstractCardModifier
    {
        public DoNothingCardModifier()
        {
            this.ModifierText = "Do Nothing";
        }
        
        public override AbstractCardEvent Modify(AbstractCardEvent cardEvent)
        {
            return cardEvent;
        }
    }
}