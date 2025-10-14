using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Map
{
    public class MapNode: MonoBehaviour
    {
        public List<MapNode> children = new List<MapNode>();
        public MapTarget target;
        public bool isOn;

        public GOList GoList;

        public int rewardMoney;
        public int numNormalEnemy;
        public int numHardEnemy;
        public int numBossEnemy;

        public void Start()
        {
            if (target == MapTarget.Enemy)
            {
                rewardMoney = Random.Range(0, 3) + 3;
                numNormalEnemy = Random.Range(0, 2) + 1;
                numHardEnemy = 0;
                numBossEnemy = 0;
                
                

            }
            
            if (target == MapTarget.HardEnemy)
            {
                rewardMoney = Random.Range(0, 3) + 5;
                numNormalEnemy = Random.Range(0, 3);
                numHardEnemy = 1;
                numBossEnemy = 0;
            }
            
            if (target == MapTarget.Boss)
            {
                rewardMoney = Random.Range(0, 3) + 8;
                numNormalEnemy = Random.Range(0, 3);
                numHardEnemy = Random.Range(0, 2);
                numBossEnemy = 1;
            }

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