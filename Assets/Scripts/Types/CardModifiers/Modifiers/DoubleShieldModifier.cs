using System;
using System.Collections.Generic;
using Cards.CardEvents;
using StateManager;

namespace Types.CardModifiers.Modifiers
{
    public class DoubleShieldModifier : AbstractCardModifier
    {
        public DoubleShieldModifier()
        {
            this.ModifierText = "Double Shield";
        }
        
        public override List<AbstractCardEvent> Modify(List<AbstractCardEvent> cardEvent)
        {

            foreach (var cardEvent2 in cardEvent)
            {
                if (cardEvent2 is ShieldCardEvent shieldCardEvent)
                {
                    cardEvent.Add(shieldCardEvent);
                }
            }
            
            return cardEvent;
        }
    }
}