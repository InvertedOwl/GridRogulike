using UnityEngine;

namespace Entities.Enemies
{
    [CreateAssetMenu(fileName = "DirectLineAttackRule", menuName = "Game/Enemy Brain/Rules/Attack/Direct Line Attack")]
    public class DirectLineAttackRule : EnemyBrainRule
    {
        [SerializeField] private EnemyBrainTargetSelector targetSelector = EnemyBrainTargetSelector.Player;
        [SerializeField] private int damage = 10;
        [SerializeField] private int range = 4;
        [SerializeField] private int baseCost = 0;
        [SerializeField] private string color = "basic";

        public override bool TryPlan(EnemyTurnContext context)
        {
            if (!TrySelectTarget(context, targetSelector, out AbstractEntity target))
                return false;

            if (!EnemyBrainLine.TryFindDirectLine(
                    context,
                    context.SimulatedPosition,
                    context.GetEntityPosition(target),
                    Mathf.Max(1, range),
                    out string direction,
                    out _))
            {
                return false;
            }

            return EnemyBrainLine.AddLineAttackActions(
                context,
                direction,
                Mathf.Max(1, range),
                damage,
                baseCost,
                color) > 0;
        }
    }
}
