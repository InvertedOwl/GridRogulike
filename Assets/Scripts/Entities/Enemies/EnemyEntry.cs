using System;
using UnityEngine;

namespace Entities.Enemies
{
    [Serializable]
    public class EnemyEntry
    {
        public GameObject enemyPrefab;
        public GameObject enemyIconPrefab;
        public string enemyName;
        public float DifficultyMin; 
        public float DifficultyMax;
        public EnemyType EnemyType;
    }
}