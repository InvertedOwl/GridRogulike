using UnityEngine;

public class BGTile : MonoBehaviour
{
    public void SetColor(Color color)
    {
        Color newColor = color;
        newColor.a = 1;
        transform.GetChild(0).GetComponent<SpriteRenderer>().color = newColor;
        Color darkColor = color * 0.8f;
        darkColor.a = 1;
        transform.GetChild(1).GetComponent<SpriteRenderer>().color = darkColor;
    }
}
