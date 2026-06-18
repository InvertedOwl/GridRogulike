using System.Collections.Generic;
using System.Linq;
using Entities.Enemies;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "EncountersDatabase", menuName = "Game/Encounters Database")]
    public class EncounterDatabase : ScriptableObject
    {
        [SerializeField] private List<EncounterData> encounters = new List<EncounterData>();

        public EncounterData GetRandomEncounter(float difficulty, EnemyType enemyType, RandomState random)
        {
            List<EncounterData> validEncounters = encounters
                .Where(e => e != null &&
                            difficulty >= e.DifficultyMin &&
                            difficulty <= e.DifficultyMax &&
                            e.EncounterType == enemyType)
                .ToList();

            if (validEncounters.Count == 0)
            {
                Debug.LogError($"No encounter found for difficulty {difficulty} or encounter type {enemyType}");
                return null;
            }

            if (random == null)
                random = RunInfo.NewRandom("encounters");

            return validEncounters[random.Next(0, validEncounters.Count)];
        }
    }
}
