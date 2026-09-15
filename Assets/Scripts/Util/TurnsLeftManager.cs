using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Util;

public class TurnsLeftManager : MonoBehaviour
{
    
    [SerializeField] private TextMeshProUGUI turnsLeftText;
    [SerializeField] private Image turnsLeftBG;
    [SerializeField] private Image turnsLeftBG2;
    [SerializeField] private Color turnsLeftColorGood;
    [SerializeField] private Color turnsLeftColorWarning;
    
    public int turnsLeft = 5;
    
    void Start()
    {
            
    }

    void Update()
    {
        
    }

    public void UpdateTurnsLeftVisuals()
    {
        turnsLeftText.text = turnsLeft.ToString();
        if (turnsLeft <= 3)
        {
            turnsLeftBG.color = turnsLeftColorWarning;
            turnsLeftBG2.color = turnsLeftColorWarning;
        }
        else
        {
            turnsLeftBG.color = turnsLeftColorGood;
            turnsLeftBG2.color = turnsLeftColorGood;
        }

        if (turnsLeft <= 1)
        {
            turnsLeftBG.GetComponent<PulseLightness>().Play();
            turnsLeftBG2.GetComponent<PulseLightness>().Play();
        }
        else
        {
            turnsLeftBG2.GetComponent<PulseLightness>().Stop();
            turnsLeftBG.GetComponent<PulseLightness>().Stop();
        }
    }
}
