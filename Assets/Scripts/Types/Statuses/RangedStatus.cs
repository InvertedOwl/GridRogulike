using System.Collections.Generic;
using Cards.CardEvents;

namespace Types.Statuses
{
    public class RangedStatus : AbstractStatus
    {
        public RangedStatus(int amount)
        {
            Amount = amount;
        }

        public override List<AbstractCardEvent> Modify(List<AbstractCardEvent> cardEvents)
        {
            foreach (AbstractCardEvent cardEvent in cardEvents)
            {
                if (cardEvent is AttackCardEvent attackCardEvent &&
                    !attackCardEvent.usePosition &&
                    attackCardEvent.distance > 0)
                {
                    attackCardEvent.distance += Amount;
                }
            }

            return cardEvents;
        }

        public override void OnEndTurn()
        {
        }
    }
}
