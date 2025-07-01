using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RunInfo : MonoBehaviour
{
    public static RunInfo Instance;
    private int _currentEnergy;
    private int _maxEnergy = 4;
    private int _redraws;
    private int _money = 0;

    public int maxRedraws = 1;

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
            foreach (TextMeshProUGUI textMeshProUGUI in redrawText)
            {
                textMeshProUGUI.text = _redraws + "/" + maxRedraws;
            }
        }
    }

    public int Money 
    { 
        get => _money;
        set
        {
            _money = value;
            foreach (TextMeshProUGUI textMeshProUGUI in moneyText)
            {
                textMeshProUGUI.text = _money + "$";
            }
        } 
    }

    public List<TextMeshProUGUI> energyText;
    public List<TextMeshProUGUI> moneyText;
    public List<TextMeshProUGUI> redrawText;

    void Awake()
    {
        Instance = this;
        CurrentEnergy = 4;
        Money = 5;
    }

    private void UpdateEnergyText()
    {
        foreach (TextMeshProUGUI textMeshProUGUI in energyText)
        {
            textMeshProUGUI.text = _currentEnergy + "/" + _maxEnergy;
        }
    }

    public void AddMoney(int amount)
    {
        Money += amount;
    }
}