using System;
using UnityEngine;

public class ScaleWithSin : MonoBehaviour
{
    public float low;
    public float high;

    private Vector3 initScale;
    
    public void Start()
    {
        initScale = transform.localScale;
    }

    void Update()
    {
        float speed = 3f;
        float t = (Mathf.Sin(Time.time * speed) + 1f) * 0.5f;
        float currentScaleMult = Mathf.Lerp(low, high, t);
        
        transform.localScale = initScale * currentScaleMult;
    }
}
