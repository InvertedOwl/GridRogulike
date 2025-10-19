using System;
using System.Collections.Generic;
using Cards.CardEvents;

namespace Types.CardModifiers.Modifiers
{
    public class SpeedCardModifier : AbstractCardModifier
    {
        public SpeedCardModifier()
        {
            this.ModifierText = "All movement distance is doubled.";
        }
        
        public override List<AbstractCardEvent> Modify(List<AbstractCardEvent> cardEvent)
        {
            foreach (var cEvent in cardEvent)
            {
                if (cEvent is MoveCardEvent)
                {
                    ((MoveCardEvent)cEvent).distance *= 2;
                }
            }
            
            return cardEvent;
        }
    }
}