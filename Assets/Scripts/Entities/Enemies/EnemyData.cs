using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Entities.Enemies
{
    public class EnemyData : MonoBehaviour
    {
        [SerializeField]
        public List<EnemyEntry> enemies;

        public EnemyEntry GetRandomEnemy(float difficulty, EnemyType enemyType)
        {
            var validEnemies = enemies
                .Where(e => difficulty >= e.DifficultyMin && difficulty <= e.DifficultyMax).Where(e => e.EnemyType == enemyType)
                .ToList();

            if (validEnemies.Count == 0)
            {
                Debug.LogError($"No enemies found for difficulty {difficulty}");
                return null;
            }

            return validEnemies[Random.Range(0, validEnemies.Count)];
        }
    }
}