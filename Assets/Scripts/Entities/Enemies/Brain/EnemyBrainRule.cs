using UnityEngine;

namespace Entities.Enemies
{
    public enum EnemyBrainRuleFlow
    {
        Continue,
        StopPhase,
        StopBrain
    }

    public abstract class EnemyBrainRule : ScriptableObject
    {
        [SerializeField]
        [Tooltip("What to do after this rule successfully adds one or more planned actions.")]
        private EnemyBrainRuleFlow afterSuccessfulPlan = EnemyBrainRuleFlow.Continue;

        public EnemyBrainRuleFlow AfterSuccessfulPlan => afterSuccessfulPlan;

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
