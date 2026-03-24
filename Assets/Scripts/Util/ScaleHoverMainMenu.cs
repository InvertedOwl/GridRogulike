using UnityEngine;

public class ScaleHoverMainMenu : MonoBehaviour
{
    [SerializeField] private float hoverScaleMultiplier = 0.8f;

    void Update()
    {
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        Collider2D[] hits = Physics2D.OverlapPointAll(mouseWorldPos);

        foreach (Collider2D hit in hits)
        {
            Transform t = hit.transform;
            t.GetChild(0).localScale = hoverScaleMultiplier * Vector3.one;
            t.GetChild(1).localScale = (hoverScaleMultiplier * 0.8f) * Vector3.one;
        }
    }
}