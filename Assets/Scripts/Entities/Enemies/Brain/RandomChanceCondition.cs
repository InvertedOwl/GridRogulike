using UnityEngine;

namespace Entities.Enemies
{
    [CreateAssetMenu(fileName = "RandomChanceCondition", menuName = "Game/Enemy Brain/Conditions/Random Chance")]
    public class RandomChanceCondition : EnemyBrainCondition
    {
        [SerializeField]
        [Range(0, 100)]
        private int percentChance = 50;

        protected override bool Evaluate(EnemyTurnContext context)
        {
            RandomState random = context?.PlanningRandom;
            if (random == null)
                return false;

            int clampedChance = Mathf.Clamp(percentChance, 0, 100);
            int roll = random.Next(0, 100);
            return roll < clampedChance;
        }
    }
}
