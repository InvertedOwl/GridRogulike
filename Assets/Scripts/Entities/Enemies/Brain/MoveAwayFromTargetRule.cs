using UnityEngine;

namespace Entities.Enemies
{
    [CreateAssetMenu(fileName = "MoveAwayFromTargetRule", menuName = "Game/Enemy Brain/Rules/Move/Move Away From Target")]
    public class MoveAwayFromTargetRule : EnemyBrainMoveRule
    {
        [SerializeField] private EnemyBrainTargetSelector targetSelector = EnemyBrainTargetSelector.Player;
        [SerializeField] private int minimumDistance = 2;
        [SerializeField] private int maxMoves = 3;

        public override bool TryPlan(EnemyTurnContext context)
        {
            return TryMoveAwayFromTarget(context, targetSelector, minimumDistance, maxMoves);
        }
    }
}
