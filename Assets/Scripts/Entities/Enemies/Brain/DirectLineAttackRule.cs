using UnityEngine;

namespace Entities.Enemies
{
    [CreateAssetMenu(fileName = "DirectLineAttackRule", menuName = "Game/Enemy Brain/Rules/Attack/Direct Line Attack")]
    public class DirectLineAttackRule : EnemyBrainAttackRule
    {
        [SerializeField] private EnemyBrainTargetSelector targetSelector = EnemyBrainTargetSelector.Player;
        [SerializeField] private int damage = 10;
        [SerializeField] private int range = 4;
        [SerializeField] private int baseCost = 0;
        [SerializeField] private string color = "basic";

        public override bool TryPlan(EnemyTurnContext context)
        {
            return TryFindDirectAttackLine(
                       context,
                       targetSelector,
                       range,
                       out _,
                       out string direction,
                       out _) &&
                   AddLineAttackActions(context, direction, range, damage, baseCost, color) > 0;
        }
    }
}
