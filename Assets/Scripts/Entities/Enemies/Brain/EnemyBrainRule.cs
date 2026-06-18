using UnityEngine;

namespace Entities.Enemies
{
    public abstract class EnemyBrainRule : ScriptableObject
    {
        public abstract bool TryPlan(EnemyTurnContext context);

        protected bool TrySelectTarget(
            EnemyTurnContext context,
            EnemyBrainTargetSelector selector,
            out AbstractEntity target)
        {
            return EnemyBrainTargeting.TrySelectTarget(context, selector, out target);
        }

        protected bool IsOnlyLivingEnemy(EnemyTurnContext context)
        {
            return EnemyBrainTargeting.CountLivingEnemies(context) <= 1;
        }
    }
}
