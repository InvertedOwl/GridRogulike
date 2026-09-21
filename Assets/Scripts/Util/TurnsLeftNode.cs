using System;
using TMPro;
using UnityEngine;

public class TurnsLeftNode : MonoBehaviour
{
    [Header("Objects")] 
    [SerializeField] private EaseScale WhiteNodeElement;
    [SerializeField] private TextMeshProUGUI WhiteNodeText;


    public void SetUsed(bool used)
    {
        if (used)
        {
            WhiteNodeElement.SetScale(new Vector3(0, 0, 0));
        }
        else
        {
            WhiteNodeElement.SetScale(new Vector3(1, 1, 1));
        }

    }

    public void SetText(string text)
    {
        WhiteNodeElement.gameObject.SetActive(true);
        WhiteNodeText.text = text;
    }
}
