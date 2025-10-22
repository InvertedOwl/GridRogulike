using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Util;

public class RunInfo : MonoBehaviour
{
    public static RunInfo Instance;
    
    public int maxRedraws = 1;
    public List<TextMeshProUGUI> energyText;
    public List<TextMeshProUGUI> moneyText;
    public List<TextMeshProUGUI> redrawText;
    public List<TextMeshProUGUI> difficultyText;
    public string seed = "testing";
    public readonly int combineCost = 2;

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
    
    private const int DefaultMaxEnergy = 4;
    private const int InitialEnergy = 4;
    private const int InitialMoney = 4;
    private const int DefaultDifficulty = 0;
    
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
        }
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