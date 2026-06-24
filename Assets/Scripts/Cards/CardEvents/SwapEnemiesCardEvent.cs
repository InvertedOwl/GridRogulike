using Entities;

namespace Cards.CardEvents
{
    public class SwapEnemiesCardEvent : SwapEntitiesCardEvent
    {
        public SwapEnemiesCardEvent(AbstractEntity firstEnemy, AbstractEntity secondEnemy)
            : base(firstEnemy, secondEnemy)
        {
        }

        protected override bool CanSwap(AbstractEntity firstEntity, AbstractEntity secondEntity)
        {
            return base.CanSwap(firstEntity, secondEntity) &&
                   firstEntity.entityType == EntityType.Enemy &&
                   secondEntity.entityType == EntityType.Enemy;
        }
    }
}
