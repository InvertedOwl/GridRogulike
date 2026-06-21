using Cards.Actions;
using UnityEngine;

namespace Entities.Enemies
{
    [CreateAssetMenu(fileName = "ApplyStatusToSelfRule", menuName = "Game/Enemy Brain/Rules/Utility/Apply Status To Self")]
    public class ApplyStatusToSelfRule : EnemyBrainUtilityRule
    {
        [SerializeField] private StatusApplicationType statusType = StatusApplicationType.Buffed;
        [SerializeField] private int statusAmount = 3;
        [SerializeField] private int baseCost = 1;
        [SerializeField] private string color = "basic";

        public override bool TryPlan(EnemyTurnContext context)
        {
            return TryAddStatusToSelf(context, statusType, statusAmount, baseCost, color);
        }
    }
}
