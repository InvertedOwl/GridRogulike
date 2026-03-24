using TMPro;
using UnityEngine;

public class ViewButtonText : MonoBehaviour
{
    public TextMeshProUGUI text;
    void Update()
    {
        text.text = "View Deck (" + Deck.Instance.Cards.Count.ToString() + ")";
    }
}
