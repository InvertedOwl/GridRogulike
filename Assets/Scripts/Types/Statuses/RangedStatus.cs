using System.Collections.Generic;
using Cards.CardEvents;
using Entities;

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

        public static void ConsumeAfterAttack(AbstractEntity entity)
        {
            if (entity == null ||
                entity.entityType != EntityType.Player ||
                entity.statusManager == null)
            {
                return;
            }

            foreach (AbstractStatus status in entity.statusManager.statusList)
            {
                if (status is RangedStatus rangedStatus && rangedStatus.Amount > 0)
                {
                    rangedStatus.Amount = 0;
                    return;
                }
            }
        }
    }
}
