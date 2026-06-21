using UnityEngine;

namespace Entities.Enemies
{
    [CreateAssetMenu(fileName = "ShieldSelfRule", menuName = "Game/Enemy Brain/Rules/Utility/Shield Self")]
    public class ShieldSelfRule : EnemyBrainUtilityRule
    {
        [SerializeField] private int shieldAmount = 5;
        [SerializeField] private int baseCost = 1;
        [SerializeField] private string color = "basic";

        public override bool TryPlan(EnemyTurnContext context)
        {
            return TryAddShield(context, shieldAmount, baseCost, color);
        }
    }
}
