using UnityEngine;

[ExecuteAlways]
public class ArrowUI : MonoBehaviour
{
    public RectTransform tail;
    public RectTransform head;
    
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            UpdateArrow();
        }
    }
    
    void Update()
    {
        UpdateArrow();   
    }

    public void UpdateArrow()
    {
        tail.localPosition = new Vector3(tail.localPosition.x, tail.rect.height / 2);
        head.localPosition = new Vector3(head.localPosition.x, tail.rect.height);
    }
    
}
