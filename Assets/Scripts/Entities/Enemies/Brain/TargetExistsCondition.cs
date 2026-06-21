using UnityEngine;

namespace Entities.Enemies
{
    [CreateAssetMenu(fileName = "TargetExistsCondition", menuName = "Game/Enemy Brain/Conditions/Target Exists")]
    public class TargetExistsCondition : EnemyBrainCondition
    {
        [SerializeField] private EnemyBrainTargetSelector targetSelector = EnemyBrainTargetSelector.Player;

        protected override bool Evaluate(EnemyTurnContext context)
        {
            return TrySelectTarget(context, targetSelector, out _);
        }
    }
}
