using System.Collections.Generic;
using Cards.CardEvents;
namespace Types.Statuses
{
    public class SleepyStatus : AbstractStatus
    {
        public SleepyStatus(int amount)
        {
            Amount = amount;
        }

        public override int ModifyDrawCount(int drawCount)
        {
            return drawCount - Amount;
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
