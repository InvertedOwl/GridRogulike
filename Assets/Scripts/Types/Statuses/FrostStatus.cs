using System.Collections.Generic;
using Cards.CardEvents;

namespace Types.Statuses
{
    public class FrostStatus : AbstractStatus
    {
        public FrostStatus(int amount)
        {
            this.Amount = amount;
        }
        
        public override List<AbstractCardEvent> Modify(List<AbstractCardEvent> cardEvent)
        {
            return cardEvent;
        }

        public override void OnEndTurn()
        {
        }
    }
}