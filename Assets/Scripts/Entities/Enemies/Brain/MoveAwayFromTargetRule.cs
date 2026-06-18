using UnityEngine;

namespace Entities.Enemies
{
    [CreateAssetMenu(fileName = "MoveAwayFromTargetRule", menuName = "Game/Enemy Brain/Rules/Move/Move Away From Target")]
    public class MoveAwayFromTargetRule : EnemyBrainRule
    {
        [SerializeField] private EnemyBrainTargetSelector targetSelector = EnemyBrainTargetSelector.Player;
        [SerializeField] private int minimumDistance = 2;
        [SerializeField] private int maxMoves = 3;

        public override bool TryPlan(EnemyTurnContext context)
        {
            return TrySelectTarget(context, targetSelector, out AbstractEntity target) &&
                   EnemyBrainMovement.TryMoveAwayFromTarget(
                       context,
                       target,
                       minimumDistance,
                       maxMoves);
        }
    }
}
