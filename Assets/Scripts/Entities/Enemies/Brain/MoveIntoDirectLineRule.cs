using UnityEngine;

namespace Entities.Enemies
{
    [CreateAssetMenu(fileName = "MoveIntoDirectLineRule", menuName = "Game/Enemy Brain/Rules/Move/Move Into Direct Line")]
    public class MoveIntoDirectLineRule : EnemyBrainRule
    {
        [SerializeField] private EnemyBrainTargetSelector targetSelector = EnemyBrainTargetSelector.Player;
        [SerializeField] private int attackRange = 4;
        [SerializeField] private int preferredDistance = 3;
        [SerializeField] private int maxMoves = 3;

        public override bool TryPlan(EnemyTurnContext context)
        {
            return TrySelectTarget(context, targetSelector, out AbstractEntity target) &&
                   EnemyBrainMovement.TryMoveIntoDirectLine(
                       context,
                       target,
                       attackRange,
                       preferredDistance,
                       maxMoves);
        }
    }
}
