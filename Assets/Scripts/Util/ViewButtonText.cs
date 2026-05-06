using TMPro;
using UnityEngine;

public class ViewButtonText : MonoBehaviour
{
    public TextMeshProUGUI text;
    void Update()
    {
        if (text == null)
            return;

        if (Deck.Instance == null)
        {
            text.text = "View Deck";
            return;
        }

        text.text = "View Deck (" + Deck.Instance.Cards.Count.ToString() + ")";
    }
}
