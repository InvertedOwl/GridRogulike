using UnityEngine;

namespace Entities.Enemies
{
    [CreateAssetMenu(fileName = "MoveTowardTargetRule", menuName = "Game/Enemy Brain/Rules/Move/Move Toward Target")]
    public class MoveTowardTargetRule : EnemyBrainMoveRule
    {
        [SerializeField] private EnemyBrainTargetSelector targetSelector = EnemyBrainTargetSelector.Player;
        [SerializeField] private int stopDistance = 1;
        [SerializeField] private int maxMoves = 3;

        public override bool TryPlan(EnemyTurnContext context)
        {
            return TryMoveTowardTarget(context, targetSelector, stopDistance, maxMoves);
        }
    }
}
