using System.Collections.Generic;
using Cards;
using UnityEngine;
using UnityEngine.UI;

public class CardCombine : MonoBehaviour
{

    private List<Card> toCombine = new List<Card>();

    public Button confirmButton;
    
    void Start()
    {
        
    }

    void Update()
    {
        if (toCombine.Count < 2)
        {
            confirmButton.interactable = false;
        }
        else
        {
            confirmButton.interactable = true;
        }
    }
}
