using System;
using System.Collections.Generic;
using Cards.CardEvents;

namespace Types.CardModifiers.Modifiers
{
    public class DrawCardModifier : AbstractCardModifier
    {
        public DrawCardModifier()
        {
            this.ModifierText = "Draw a card";
        }
        
        public override List<AbstractCardEvent> Modify(List<AbstractCardEvent> cardEvent)
        {
            cardEvent.Add(new DrawCardEvent(1));
            return cardEvent;
        }
    }
}
