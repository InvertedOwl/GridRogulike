using UnityEngine;

namespace Entities.Enemies
{
    [CreateAssetMenu(fileName = "IsAloneCondition", menuName = "Game/Enemy Brain/Conditions/Is Alone")]
    public class IsAloneCondition : EnemyBrainCondition
    {
        protected override bool Evaluate(EnemyTurnContext context)
        {
            return EnemyBrainTargeting.CountLivingEnemies(context) <= 1;
        }
    }
}
