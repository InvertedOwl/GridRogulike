using System.Collections.Generic;
using Cards;
using Cards.CardList;
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

        public override TargetDefinition ModifyTargetDefinition(TargetDefinition targetDefinition, CardPlayContext context)
        {
            if (targetDefinition == null ||
                Amount <= 0 ||
                !targetDefinition.MaxRange.HasValue ||
                targetDefinition.TargetType is not (TargetType.AnyEnemy or TargetType.EveryEnemy))
            {
                return targetDefinition;
            }

            TargetDefinition modified = targetDefinition.Copy();
            modified.MaxRange = modified.MaxRange.Value + Amount;
            return modified;
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
