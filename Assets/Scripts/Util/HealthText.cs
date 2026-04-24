using Entities;
using StateManager;
using TMPro;
using UnityEngine;

public class HealthText : MonoBehaviour
{
    private TextMeshProUGUI text;
    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();        
    }

    void Update()
    {
        if (GameStateManager.Instance.IsCurrent<PlayingState>() && Player.Instance != null)
        {
            text.text = Player.Instance.Health + "/" + Player.Instance.initialHealth;
        }
    }
}
