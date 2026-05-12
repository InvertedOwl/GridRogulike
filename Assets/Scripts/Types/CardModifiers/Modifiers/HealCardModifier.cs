using System;
using System.Collections.Generic;
using Cards.CardEvents;

namespace Types.CardModifiers.Modifiers
{
    public class HealCardModifier : AbstractCardModifier
    {
        public HealCardModifier()
        {
            this.ModifierText = "Heal 5.";
        }
        
        public override List<AbstractCardEvent> Modify(List<AbstractCardEvent> cardEvent)
        {
            cardEvent.Add(new HealSelfCardEvent(5));
            return cardEvent;
        }
    }
}
