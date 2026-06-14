using System.Collections.Generic;
using Cards.CardEvents;

namespace Types.Statuses
{
    public class FallStatus : AbstractStatus
    {
        private const int FallDamage = 999;

        public FallStatus(int amount)
        {
            Amount = amount;
        }

        public override List<AbstractCardEvent> Modify(List<AbstractCardEvent> cardEvent)
        {
            return cardEvent;
        }

        public override void OnEndTurn()
        {
            Amount -= 1;

            if (Amount <= 0)
                Entity?.Damage(FallDamage);
        }
    }
}
