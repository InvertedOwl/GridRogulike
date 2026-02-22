using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Entities.Enemies
{
    
    [Serializable]
    public class EnemyEntry
    {
        public GameObject enemyPrefab;
        public EnemyType enemyType;
        public GameObject enemyIconPrefab;
        public string enemyName;
    }
    
    [Serializable]
    public class EncounterData
    {
        public float DifficultyMin; 
        public float DifficultyMax;
        public List<EnemyEntry> enemies;
        public EnemyType EncounterType;
            
    }
    
    
    
    public class EnemyData : MonoBehaviour
    {
        [SerializeField]
        public List<EncounterData> enemies;

        public EncounterData GetRandomEncounter(float difficulty, EnemyType enemyType, System.Random random)
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