using System;
using System.Collections.Generic;
using System.Linq;
using StateManager;
using UnityEngine;

namespace Entities.Enemies
{
    [Serializable]
    public class EncounterData
    {
        public float DifficultyMin; 
        public float DifficultyMax;
        public List<string> enemies;
        public List<string> environmentPassives = new List<string>();
        public EnemyType EncounterType;
        public int PerlinNoiseSeed;
        public int TurnsLeft = 5;
        // Additional victory rewards; map-node gold is supplied by PlayingState separately.
        public List<EncounterResult> rewards = new();

        public EncounterData CreateRuntimeCopy(int perlinNoiseSeed)
        {
            return new EncounterData
            {
                DifficultyMin = DifficultyMin,
                DifficultyMax = DifficultyMax,
                enemies = enemies != null ? new List<string>(enemies) : new List<string>(),
                environmentPassives = environmentPassives != null
                    ? new List<string>(environmentPassives)
                    : new List<string>(),
                EncounterType = EncounterType,
                PerlinNoiseSeed = perlinNoiseSeed,
                TurnsLeft = TurnsLeft,
                rewards = rewards?.Select(result => result?.Copy()).ToList() ?? new List<EncounterResult>()
            };
        }
    }
    
    public class EnemyData : MonoBehaviour
    {
        [SerializeField]
        public List<EncounterData> enemies;

        public EncounterData GetRandomEncounter(float difficulty, EnemyType enemyType, RandomState random)
        {
            var validEncounter = enemies
                .Where(e => difficulty >= e.DifficultyMin && difficulty <= e.DifficultyMax).Where(e => e.EncounterType == enemyType)
                .ToList();

            if (validEncounter.Count == 0)
            {
                Debug.LogError($"No encounter found for difficulty {difficulty} or encounter type {enemyType}");
                return null;
            }

            return validEncounter[random.Next(0, validEncounter.Count)];
        }
    }
}
