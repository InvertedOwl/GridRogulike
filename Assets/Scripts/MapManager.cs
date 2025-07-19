using UnityEngine;

public class MapManager : MonoBehaviour
{   private static MapManager instance;
    public static MapManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<MapManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("MapInformation");
                    instance = go.AddComponent<MapManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    private int rewardMoney;
    private float difficultyLevel;
    private bool isBossFight;
    private int enemyCount;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public int RewardMoney
    {
        get { return rewardMoney; }
        set
        {
            rewardMoney = value;
            Debug.Log($"Map Reward Money set to: {rewardMoney}");
        }
    }

    public float DifficultyLevel
    {
        get { return difficultyLevel; }
        set
        {
            difficultyLevel = Mathf.Clamp(value, 1f, 10f);
            Debug.Log($"Map Difficulty Level set to: {difficultyLevel}");
        }
    }

    public bool IsBossFight
    {
        get { return isBossFight; }
        set
        {
            isBossFight = value;
            Debug.Log($"Map Is Boss Fight set to: {isBossFight}");
        }
    }

    public int EnemyCount
    {
        get { return enemyCount; }
        set
        {
            enemyCount = Mathf.Clamp(value, 1, 10);
            Debug.Log($"Map Enemy Count set to: {enemyCount}");
        }
    }
}
