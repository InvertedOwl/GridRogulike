using System;
using System.Collections.Generic;
using Cards.CardEvents;

namespace Types.CardModifiers.Modifiers
{
    public class DoNothingCardModifier : AbstractCardModifier
    {
        public DoNothingCardModifier()
        {
            this.ModifierText = "No Effect";
        }
        
        public override List<AbstractCardEvent> Modify(List<AbstractCardEvent> cardEvent)
        {
            return cardEvent;
        }
    }
}