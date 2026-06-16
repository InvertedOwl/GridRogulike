using Entities;
using UnityEngine;

public class CampfireHealthbar : MonoBehaviour
{
    private HealthBarManager healthBarManager;
    
    void Start()
    {
        healthBarManager = GetComponent<HealthBarManager>();
    }
    void Update()
    {
        healthBarManager.SetValues(Player.Instance.Health, Player.Instance.initialHealth, 0);
    }
}
