using System.Collections.Generic;
using Cards.CardEvents;

namespace Types.Statuses
{
    public class PoisonStatus : AbstractStatus
    {
        public PoisonStatus(int amount)
        {
            this.Amount = amount;
        }
        
        public override List<AbstractCardEvent> Modify(List<AbstractCardEvent> cardEvent)
        {
            return cardEvent;
        }

        public override void OnEndTurn()
        {
            this.Entity.Damage(Amount, null);
            Amount -= 1;
        }
    }
}