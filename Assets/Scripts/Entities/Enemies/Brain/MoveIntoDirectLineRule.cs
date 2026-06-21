using UnityEngine;

namespace Entities.Enemies
{
    [CreateAssetMenu(fileName = "MoveIntoDirectLineRule", menuName = "Game/Enemy Brain/Rules/Move/Move Into Direct Line")]
    public class MoveIntoDirectLineRule : EnemyBrainMoveRule
    {
        [SerializeField] private EnemyBrainTargetSelector targetSelector = EnemyBrainTargetSelector.Player;
        [SerializeField] private int attackRange = 4;
        [SerializeField] private int preferredDistance = 3;
        [SerializeField] private int maxMoves = 3;

        public override bool TryPlan(EnemyTurnContext context)
        {
            return TryMoveIntoDirectLine(context, targetSelector, attackRange, preferredDistance, maxMoves);
        }
    }
}
