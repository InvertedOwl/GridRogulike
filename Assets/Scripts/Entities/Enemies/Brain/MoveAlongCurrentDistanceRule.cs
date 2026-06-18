using UnityEngine;

namespace Entities.Enemies
{
    [CreateAssetMenu(fileName = "MoveAlongCurrentDistanceRule", menuName = "Game/Enemy Brain/Rules/Move/Move Along Current Distance")]
    public class MoveAlongCurrentDistanceRule : EnemyBrainRule
    {
        [SerializeField] private EnemyBrainTargetSelector targetSelector = EnemyBrainTargetSelector.Player;
        [SerializeField] private int maxMoves = 1;

        public override bool TryPlan(EnemyTurnContext context)
        {
            return TrySelectTarget(context, targetSelector, out AbstractEntity target) &&
                   EnemyBrainMovement.TryMoveAlongCurrentDistance(context, target, maxMoves);
        }
    }
}
