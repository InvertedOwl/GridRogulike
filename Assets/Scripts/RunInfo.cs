using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Util;
using Random = System.Random;

public class RunInfo : MonoBehaviour
{
    public static RunInfo Instance;
    
    public int maxRedraws = 0;
    public List<TextMeshProUGUI> energyText;
    public List<TextMeshProUGUI> moneyText;
    public List<TextMeshProUGUI> redrawText;
    public Button redrawButton;
    public List<TextMeshProUGUI> difficultyText;
    public List<TextMeshProUGUI> stepsText;
    public static string seed = new System.Random().Next(Int32.MaxValue) + ""; // TODO: Random Seed
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

    public int Redraws
    {
        get => _redraws;
        set
        {
            _redraws = value;
            UpdateRedrawText();
            if (value == 0)
            {
                redrawButton.interactable = false;
            }
            else
            {
                redrawButton.interactable = true;
            }
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
    
    private const int DefaultMaxEnergy = 4;
    private const int InitialEnergy = 4;
    private const int InitialMoney = 9999;
    private const int DefaultDifficulty = 0;
    

    public static Random NewRandom(int nudge)
    {
        return new Random(seed.GetHashCode() + nudge);
    }
    
    
    // Initialize singleton and default values
    void Awake()
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

    
    // Update redraw display text elements
    private void UpdateRedrawText()
    {
        UpdateTextCollection(redrawText, FormatRedrawText());
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

    // Format current/max redraws display
    private string FormatRedrawText()
    {
        return _redraws + "/" + maxRedraws;
    }
    
    private string FormatDifficultyText()
    {
        return "Difficulty: " + _difficulty;
    }

}