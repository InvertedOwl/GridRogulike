using UnityEngine;

namespace Entities.Enemies
{
    public abstract class EnemyBrainCondition : ScriptableObject
    {
        [SerializeField] private bool invert;

        public bool IsMet(EnemyTurnContext context)
        {
            bool result = Evaluate(context);
            return invert ? !result : result;
        }

        protected abstract bool Evaluate(EnemyTurnContext context);

        protected bool TrySelectTarget(
            EnemyTurnContext context,
            EnemyBrainTargetSelector selector,
            out AbstractEntity target)
        {
            return EnemyBrainTargeting.TrySelectTarget(context, selector, out target);
        }
    }
}
