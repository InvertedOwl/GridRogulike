using System;
using System.Collections;
using System.Collections.Generic;
using Serializer;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Util;
using Random = System.Random;

public class RunInfo : MonoBehaviour
{
    public static RunInfo Instance;
    public event Action<int, int> CurrentStepsChanged;
    
    public List<TextMeshProUGUI> energyText;
    public List<TextMeshProUGUI> moneyText;
    public List<TextMeshProUGUI> redrawText;
    public Button redrawButton;
    public List<TextMeshProUGUI> difficultyText;
    public List<TextMeshProUGUI> stepsText;
    public static string seed = GenerateSeed();
    public readonly int combineCost = 2;


    public int CurrentSteps
    {
        get => _currentSteps;
        set
        {
            int previousSteps = _currentSteps;
            _currentSteps = value;
            UpdateStepsText();

            if (previousSteps != _currentSteps)
            {
                CurrentStepsChanged?.Invoke(previousSteps, _currentSteps);
            }
        }
    }
    
    public int CurrentEnergy
    {
        get => _currentEnergy;
        set
        {
            _currentEnergy = value;
            UpdateEnergyText();
            if (Deck.Instance != null)
                Deck.Instance.UpdatePlayability();
        }
    }

    public int MaxEnergy
    {
        get => _maxEnergy;
        set
        {
            _maxEnergy = value;
            UpdateEnergyText();
        }
    }


    public int Money 
    { 
        get => _money;
        set
        {
            BattleStats.MoneyGainedThisBattle += value-_money;
            BattleStats.MoneyGainedThisTurn += value-_money;
            _money = value;
            UpdateMoneyText();
        } 
    }

    public int Difficulty
    {
        get => _difficulty;
        set
        {
            _difficulty = value;
            UpdateDifficultyText();
        }
    }

    private int _currentEnergy;
    private int _maxEnergy = DefaultMaxEnergy;
    private int _redraws;
    private int _money = 0;
    private int _difficulty = DefaultDifficulty; 
    private int _currentSteps = 0;
    
    private const int DefaultMaxEnergy = 3;
    private const int InitialEnergy = 3;
    private const int InitialMoney = 15;
    private const int DefaultDifficulty = 0;

    [SerializeField] public static Dictionary<int, RandomState> randoms = new Dictionary<int, RandomState>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticsOnLoad()
    {
        Instance = null;
        seed = GenerateSeed();
        randoms = new Dictionary<int, RandomState>();
    }

    public static void ResetRandoms()
    {
        randoms.Clear();
    }

    public static void ResetRunDefaults()
    {
        Instance = null;
        seed = GenerateSeed();
        ResetRandoms();
    }

    public static void SetSeed(string newSeed)
    {
        string normalizedSeed = string.IsNullOrWhiteSpace(newSeed) ? GenerateSeed() : newSeed;

        if (seed == normalizedSeed)
            return;

        seed = normalizedSeed;
        ResetRandoms();
    }

    private static string GenerateSeed()
    {
        return Guid.NewGuid().ToString();
    }

    public static int StableHash(string value)
    {
        unchecked
        {
            int hash = 17;
            foreach (char c in value ?? string.Empty)
            {
                hash = hash * 31 + c;
            }
            return hash;
        }
    }

    public static RandomState NewRandom(string key)
    {
        return NewRandom(StableHash(key));
    }

    public static RandomState ResetRandom(string key)
    {
        return ResetRandom(StableHash(key));
    }

    public static RandomState ResetRandom(int nudge)
    {
        RandomState random = NewRandom(nudge);
        random.calls = 0;
        random.RebuildRandom();
        return random;
    }
    
    public static RandomState NewRandom(int nudge)
    {
        if (randoms.ContainsKey(nudge))
        {
            return randoms[nudge];
        }

        RandomState random = new RandomState(unchecked(nudge + StableHash(seed)), 0);
        
        randoms[nudge] = random;
        return random;
    }
    
    
    
    // Initialize singleton and default values
    void Awake ()
    {
        Instance = this;
        CurrentEnergy = InitialEnergy;
        Money = DebugStats.Enabled ? DebugStats.StartingMoney : InitialMoney;
    }

    // Increase money by specified amount
    public void AddMoney(int amount)
    {
        Money += amount;
    }

    // Update energy display text elements
    private void UpdateEnergyText()
    {
        UpdateTextCollection(energyText, FormatEnergyText());
    }

    // Update money display text elements
    private void UpdateMoneyText()
    {
        
        UpdateTextCollection(moneyText, FormatMoneyText());
    }

    // Update steps display text elements
    private void UpdateStepsText()
    {
        
        UpdateTextCollection(stepsText, _currentSteps + "");
    }

    

    
    // Update difficulty display text elements
    private void UpdateDifficultyText()
    {
        UpdateTextCollection(difficultyText, FormatDifficultyText());
    }

    // Apply text to all elements in collection
    private void UpdateTextCollection(List<TextMeshProUGUI> textCollection, string text)
    {
        foreach (TextMeshProUGUI textElement in textCollection)
        {
            textElement.text = text;
            if (textElement.GetComponent<LerpPosition>())
            {
                StartCoroutine(UpdateTextPositionNextFrame(textElement.GetComponent<RectTransform>()));
            }
        }
    }

    IEnumerator UpdateTextPositionNextFrame(RectTransform rectTransform)
    {
        yield return new WaitForEndOfFrame();
        rectTransform.localPosition += new Vector3(0, 5, 0);
    }

    // Format current/max energy display
    private string FormatEnergyText()
    {
        return _currentEnergy + "/" + _maxEnergy;
    }

    // Format money amount with currency symbol
    private string FormatMoneyText()
    {
        return _money + "";
    }

    
    private string FormatDifficultyText()
    {
        return "Difficulty: " + _difficulty;
    }
    
    
    public RunInfoSaveData CaptureSaveData()
    {
        return new RunInfoSaveData
        {
            MaxEnergy = MaxEnergy,
            CurrentEnergy = CurrentEnergy,
            Money = Money,
            Difficulty = Difficulty,
            CurrentSteps = CurrentSteps,
            Seed = seed,
            randoms = CaptureUsedRandoms()
        };
    }

    private static Dictionary<int, RandomState> CaptureUsedRandoms()
    {
        Dictionary<int, RandomState> usedRandoms = new Dictionary<int, RandomState>();

        foreach (KeyValuePair<int, RandomState> randomEntry in randoms)
        {
            RandomState randomState = randomEntry.Value;
            if (randomState == null || randomState.calls <= 0)
                continue;

            usedRandoms[randomEntry.Key] = randomState.Clone();
        }

        return usedRandoms;
    }
    
    public void RestoreFromSaveData(RunInfoSaveData data)
    {
        if (data == null) return;

        MaxEnergy = data.MaxEnergy;
        CurrentEnergy = data.CurrentEnergy;
        Money = data.Money;
        Difficulty = data.Difficulty;
        CurrentSteps = data.CurrentSteps;

        seed = string.IsNullOrWhiteSpace(data.Seed) ? GenerateSeed() : data.Seed;
        Dictionary<int, RandomState> restoredRandoms = data.randoms ?? new Dictionary<int, RandomState>();

        List<int> existingKeys = new List<int>(randoms.Keys);
        HashSet<int> restoredKeys = new HashSet<int>();
        foreach (var kv in restoredRandoms)
        {
            if (kv.Value == null)
            {
                continue;
            }

            restoredKeys.Add(kv.Key);
            if (randoms.TryGetValue(kv.Key, out RandomState existingRandom))
            {
                existingRandom.CopyFrom(kv.Value);
            }
            else
            {
                randoms[kv.Key] = kv.Value.Clone();
            }
        }

        foreach (int key in existingKeys)
        {
            if (restoredKeys.Contains(key))
                continue;

            if (randoms.TryGetValue(key, out RandomState existingRandom))
            {
                existingRandom.CopyFrom(new RandomState(unchecked(key + StableHash(seed)), 0));
            }
        }
    }

}
