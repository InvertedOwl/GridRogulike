using UnityEngine;

namespace Entities.Enemies
{
    [CreateAssetMenu(fileName = "SelfLowHealthCondition", menuName = "Game/Enemy Brain/Conditions/Self Low Health")]
    public class SelfLowHealthCondition : EnemyBrainCondition
    {
        [SerializeField] private bool usePercent = true;
        [SerializeField] private float percentThreshold = 0.5f;
        [SerializeField] private float flatThreshold = 10f;

        protected override bool Evaluate(EnemyTurnContext context)
        {
            if (context?.Self == null)
                return false;

            if (!usePercent)
                return context.Self.Health <= flatThreshold;

            if (context.Self.initialHealth <= 0f)
                return false;

            return context.Self.Health / context.Self.initialHealth <= Mathf.Clamp01(percentThreshold);
        }
    }
}
