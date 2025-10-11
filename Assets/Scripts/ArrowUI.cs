using UnityEngine;

[ExecuteAlways]
public class ArrowUI : MonoBehaviour
{
    public RectTransform tail;
    public RectTransform head;
    
    private float previousTailHeight = -1f;

    void Update()
    {
        float currentHeight = tail.rect.height;

        if (!Mathf.Approximately(previousTailHeight, currentHeight))
        {
            UpdateArrow();
            previousTailHeight = currentHeight;
        }
    }

    public void UpdateArrow()
    {
        tail.localPosition = new Vector3(tail.localPosition.x, tail.rect.height / 2);
        head.localPosition = new Vector3(head.localPosition.x, tail.rect.height);
    }

    
}
