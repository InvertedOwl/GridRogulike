using System;
using System.Collections.Generic;
using Entities.Enemies;
using ScriptableObjects;
using StateManager;
using TMPro;
using UnityEngine;

namespace Map
{
    public class MapNode: MonoBehaviour
    {
        public List<MapNode> children = new List<MapNode>();
        public MapTarget target;
        public bool isOn;

        public GOList GoList;

        public int rewardMoney;
        public EncounterData encounterData;
        public EnemiesData enemiesData;
        
        
        public static RandomState guidRandom = RunInfo.NewRandom("mnguid".GetHashCode());
        public static string GenerateDeterministicId()
        {
            byte[] bytes = new byte[16];
            guidRandom.NextBytes(bytes);
            return new Guid(bytes).ToString();
        }
        
        private RandomState _mapNodeRandom = RunInfo.NewRandom(GenerateDeterministicId().GetHashCode());

        public void Start()
        {
            GameState mapState = GameStateManager.Instance.GetCurrent<MapState>();
            if (target == MapTarget.Enemy)
            {
                encounterData = mapState.GetComponent<EnemyData>().GetRandomEncounter(0, EnemyType.Normal, _mapNodeRandom);
            }
            
            if (target == MapTarget.HardEnemy)
            {
                encounterData = mapState.GetComponent<EnemyData>().GetRandomEncounter(0, EnemyType.Hard, _mapNodeRandom);
            }
            
            if (target == MapTarget.Boss)
            {
                encounterData = mapState.GetComponent<EnemyData>().GetRandomEncounter(0, EnemyType.Boss, _mapNodeRandom);
            }

            int numNormalEnemy = 0;
            int numHardEnemy = 0;
            int numBossEnemy = 0;

            if (encounterData != null)
            {
                foreach (string enemyEntry in encounterData.enemies)
                {
                    Debug.Log("Loading " + enemyEntry);

                    if (!enemiesData.Get(enemyEntry).HasValue)
                    {
                        Debug.Log("For some fucking reason this is null");
                        continue;
                    }
                    EnemiesData.EnemyEntry enemy = enemiesData.Get(enemyEntry).Value;

                    
                    if (enemy.enemyType == EnemyType.Normal)
                        numNormalEnemy += 1;
                    if (enemy.enemyType == EnemyType.Hard)
                        numHardEnemy += 1;
                    if (enemy.enemyType == EnemyType.Boss)
                        numBossEnemy += 1;
                }
            }

            rewardMoney = (numNormalEnemy * 3) + (numHardEnemy * 5) + (numBossEnemy * 7);
            
            if (GoList)
            {
                GoList.GetValue("Reward").GetComponent<TextMeshProUGUI>().text = "$" + rewardMoney;
                GoList.GetValue("EnemyNormalGO").SetActive(numNormalEnemy > 0);
                GoList.GetValue("EnemyNormalGO").GetComponentInChildren<TextMeshProUGUI>().text = "x" + numNormalEnemy;
                GoList.GetValue("EnemyHardGO").SetActive(numHardEnemy > 0);
                GoList.GetValue("EnemyHardGO").GetComponentInChildren<TextMeshProUGUI>().text = "x" + numHardEnemy;
                GoList.GetValue("EnemyBossGO").SetActive(numBossEnemy > 0);
                GoList.GetValue("EnemyBossGO").GetComponentInChildren<TextMeshProUGUI>().text = "x" + numBossEnemy;
            }
        }
    }
}