using Cards.Actions;
using UnityEngine;

namespace Entities.Enemies
{
    [CreateAssetMenu(fileName = "ShieldSelfRule", menuName = "Game/Enemy Brain/Rules/Utility/Shield Self")]
    public class ShieldSelfRule : EnemyBrainRule
    {
        [SerializeField] private int shieldAmount = 5;
        [SerializeField] private int baseCost = 1;
        [SerializeField] private string color = "basic";

        public override bool TryPlan(EnemyTurnContext context)
        {
            return context != null &&
                   context.AddAction(new ShieldAction(baseCost, color, context.Self, shieldAmount));
        }
    }
}
