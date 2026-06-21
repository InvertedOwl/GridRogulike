using UnityEngine;

namespace Entities.Enemies
{
    [CreateAssetMenu(fileName = "TargetDistanceCondition", menuName = "Game/Enemy Brain/Conditions/Target Distance")]
    public class TargetDistanceCondition : EnemyBrainCondition
    {
        [SerializeField] private EnemyBrainTargetSelector targetSelector = EnemyBrainTargetSelector.Player;
        [SerializeField] private int minDistance = 0;
        [SerializeField] private int maxDistance = 1;

        protected override bool Evaluate(EnemyTurnContext context)
        {
            if (!TrySelectTarget(context, targetSelector, out AbstractEntity target))
                return false;

            if (!context.TryGetDistanceTo(target, out int distance))
                return false;

            int min = Mathf.Max(0, minDistance);
            int max = Mathf.Max(min, maxDistance);
            return distance >= min && distance <= max;
        }
    }
}
