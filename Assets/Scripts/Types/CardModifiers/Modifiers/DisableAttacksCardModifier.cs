using System;
using System.Collections.Generic;
using Cards.CardEvents;

namespace Types.CardModifiers.Modifiers
{
    public class DisableAttacksCardModifier : AbstractCardModifier
    {
        public DisableAttacksCardModifier()
        {
            this.ModifierText = "Disables all attacks.";
        }
        
        public override List<AbstractCardEvent> Modify(List<AbstractCardEvent> cardEvent)
        {
            cardEvent.RemoveAll(cEvent => cEvent is AttackCardEvent);
            
            return cardEvent;
        }
    }
}