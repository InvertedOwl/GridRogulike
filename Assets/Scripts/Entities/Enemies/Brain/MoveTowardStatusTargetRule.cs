using UnityEngine;

namespace Entities.Enemies
{
    [CreateAssetMenu(fileName = "MoveTowardStatusTargetRule", menuName = "Game/Enemy Brain/Rules/Move/Move Toward Status Target")]
    public class MoveTowardStatusTargetRule : EnemyBrainMoveRule
    {
        [SerializeField] private EnemyBrainTargetSelector targetSelector = EnemyBrainTargetSelector.ClosestAlly;
        [SerializeField] private int statusRange = 1;
        [SerializeField] private int maxMoves = 3;

        public override bool TryPlan(EnemyTurnContext context)
        {
            return TryMoveTowardTarget(context, targetSelector, statusRange, maxMoves);
        }
    }
}
