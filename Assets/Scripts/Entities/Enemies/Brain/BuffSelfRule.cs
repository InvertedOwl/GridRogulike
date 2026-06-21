using UnityEngine;

namespace Entities.Enemies
{
    [CreateAssetMenu(fileName = "BuffSelfRule", menuName = "Game/Enemy Brain/Rules/Utility/Buff Self")]
    public class BuffSelfRule : EnemyBrainUtilityRule
    {
        [SerializeField] private int buffAmount = 3;
        [SerializeField] private int baseCost = 1;
        [SerializeField] private string color = "basic";

        public override bool TryPlan(EnemyTurnContext context)
        {
            return TryAddBuffSelf(context, buffAmount, baseCost, color);
        }
    }
}
