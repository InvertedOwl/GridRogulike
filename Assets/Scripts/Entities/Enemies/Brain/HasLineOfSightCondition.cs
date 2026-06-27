using UnityEngine;

namespace Entities.Enemies
{
    [CreateAssetMenu(fileName = "HasLineOfSightCondition", menuName = "Game/Enemy Brain/Conditions/Has Line Of Sight")]
    public class HasLineOfSightCondition : EnemyBrainCondition
    {
        [SerializeField] private EnemyBrainTargetSelector targetSelector = EnemyBrainTargetSelector.Player;
        [SerializeField] private int distance = 4;

        protected override bool Evaluate(EnemyTurnContext context)
        {
            if (context == null)
                return false;

            if (!TrySelectTarget(context, targetSelector, out AbstractEntity target))
                return false;

            return EnemyBrainLine.TryFindDirectLine(
                context,
                context.SimulatedPosition,
                context.GetEntityPosition(target),
                Mathf.Max(1, distance),
                out _,
                out _);
        }
    }
}
