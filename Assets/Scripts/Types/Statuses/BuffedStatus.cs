using System;
using System.Collections.Generic;
using Cards.Actions;
using Cards.CardEvents;

namespace Types.Statuses
{
    public class BuffedStatus : AbstractStatus
    {
        public BuffedStatus(int amount)
        {
            this.Amount = amount;
        }
        
        public override List<AbstractCardEvent> Modify(List<AbstractCardEvent> cardEvents)
        {
            foreach (AbstractCardEvent cardEvent in cardEvents)
            {
                if (cardEvent is AttackCardEvent attackCardEvent)
                {
                    attackCardEvent.amount += Amount;
                }
            }
            
            return cardEvents;
        }

        public override void OnEndTurn()
        {
        }
    }
}