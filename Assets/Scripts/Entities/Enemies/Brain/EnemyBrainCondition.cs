using System.Collections.Generic;
using UnityEngine;

namespace Entities.Enemies
{
    public abstract class EnemyBrainCondition : ScriptableObject
    {
        private static readonly IReadOnlyList<string> BooleanOutputNames = new[]
        {
            EnemyBrainData.ConditionTrueOutputName,
            EnemyBrainData.ConditionFalseOutputName
        };

        [SerializeField] private bool invert;

        public virtual IReadOnlyList<string> GetOutputNames()
        {
            return BooleanOutputNames;
        }

        public virtual string SelectOutput(EnemyTurnContext context, string nodeGuid)
        {
            return IsMet(context)
                ? EnemyBrainData.ConditionTrueOutputName
                : EnemyBrainData.ConditionFalseOutputName;
        }

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
