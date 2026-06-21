using System.Collections.Generic;
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

        [SerializeField]
        [Tooltip("All conditions must pass before this rule is allowed to plan.")]
        private List<EnemyBrainCondition> conditions = new List<EnemyBrainCondition>();

        public EnemyBrainRuleFlow AfterSuccessfulPlan => afterSuccessfulPlan;

        public abstract bool TryPlan(EnemyTurnContext context);

        public bool ConditionsPass(EnemyTurnContext context)
        {
            if (conditions == null)
                return true;

            foreach (EnemyBrainCondition condition in conditions)
            {
                if (condition == null)
                    continue;

                if (!condition.IsMet(context))
                    return false;
            }

            return true;
        }

        public bool TryPlanIfConditionsPass(EnemyTurnContext context)
        {
            return ConditionsPass(context) && TryPlan(context);
        }

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
