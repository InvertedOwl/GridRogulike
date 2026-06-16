using System.Collections.Generic;
using Cards.CardEvents;
using Entities;

namespace Types.Statuses
{
    public class ShieldCarryoverStatus : AbstractStatus
    {
        public ShieldCarryoverStatus(int amount)
        {
            Amount = amount;
        }

        public override List<AbstractCardEvent> Modify(List<AbstractCardEvent> cardEvent)
        {
            return cardEvent;
        }

        public override bool PreservesShieldOnStartTurn(AbstractEntity entity)
        {
            return Amount > 0;
        }

        public override void OnShieldPreservedStartTurn()
        {
            Amount -= 1;
        }

        public override void OnEndTurn()
        {
        }
    }
}
