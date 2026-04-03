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
    
    public List<TextMeshProUGUI> energyText;
    public List<TextMeshProUGUI> moneyText;
    public List<TextMeshProUGUI> redrawText;
    public Button redrawButton;
    public List<TextMeshProUGUI> difficultyText;
    public List<TextMeshProUGUI> stepsText;
    public static string seed = new Random().Next().ToString();
    public readonly int combineCost = 2;


    public int CurrentSteps
    {
        get => _currentSteps;
        set
        {
            _currentSteps = value;
            UpdateStepsText();
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
    
    private const int DefaultMaxEnergy = 2;
    private const int InitialEnergy = 2;
    private const int InitialMoney = 15;
    private const int DefaultDifficulty = 0;

    [SerializeField] public static Dictionary<int, RandomState> randoms = new Dictionary<int, RandomState>();

    public static void ResetRandoms()
    {
        randoms.Clear();
    }
    
    public static RandomState NewRandom(int nudge)
    {
        if (randoms.ContainsKey(nudge))
        {
            return randoms[nudge];
        }

        RandomState random = new RandomState(nudge + seed.GetHashCode(), 0); 
        
        randoms[nudge] = random;
        return random;
    }
    
    
    
    // Initialize singleton and default values
    void Awake ()
    {
        Instance = this;
        CurrentEnergy = InitialEnergy;
        Money = InitialMoney;
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
            randoms = randoms
        };
    }
    
    public void RestoreFromSaveData(RunInfoSaveData data)
    {
        if (data == null) return;

        MaxEnergy = data.MaxEnergy;
        CurrentEnergy = data.CurrentEnergy;
        Money = data.Money;
        Difficulty = data.Difficulty;
        CurrentSteps = data.CurrentSteps;

        seed = data.Seed;
        randoms = data.randoms ?? new Dictionary<int, RandomState>();

        foreach (var kv in randoms)
        {
            kv.Value.RebuildRandom();
        }
    }

}