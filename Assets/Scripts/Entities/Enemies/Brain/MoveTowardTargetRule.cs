using UnityEngine;

namespace Entities.Enemies
{
    [CreateAssetMenu(fileName = "MoveTowardTargetRule", menuName = "Game/Enemy Brain/Rules/Move/Move Toward Target")]
    public class MoveTowardTargetRule : EnemyBrainRule
    {
        [SerializeField] private EnemyBrainTargetSelector targetSelector = EnemyBrainTargetSelector.Player;
        [SerializeField] private int stopDistance = 1;
        [SerializeField] private int maxMoves = 3;

        public override bool TryPlan(EnemyTurnContext context)
        {
            return TrySelectTarget(context, targetSelector, out AbstractEntity target) &&
                   EnemyBrainMovement.TryMoveTowardTarget(
                       context,
                       target,
                       stopDistance,
                       maxMoves);
        }
    }
}
