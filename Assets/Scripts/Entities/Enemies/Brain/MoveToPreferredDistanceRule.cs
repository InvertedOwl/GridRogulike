using UnityEngine;

namespace Entities.Enemies
{
    [CreateAssetMenu(fileName = "MoveToPreferredDistanceRule", menuName = "Game/Enemy Brain/Rules/Move/Move To Preferred Distance")]
    public class MoveToPreferredDistanceRule : EnemyBrainMoveRule
    {
        [SerializeField] private EnemyBrainTargetSelector targetSelector = EnemyBrainTargetSelector.Player;
        [SerializeField] private int preferredDistance = 1;
        [SerializeField] private int maxMoves = 3;

        public override bool TryPlan(EnemyTurnContext context)
        {
            return TryMoveToPreferredDistance(context, targetSelector, preferredDistance, maxMoves);
        }
    }
}
