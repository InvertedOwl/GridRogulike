using System;
using UnityEngine;

public class LerpHeight : MonoBehaviour
{
    public float speed = 1;
    public float targetHeight = 2.5f;
    public SpriteRenderer spriteRenderer;
    
    void Update()
    {
        spriteRenderer.size = new Vector2(spriteRenderer.size.x, Mathf.Lerp(spriteRenderer.size.y, targetHeight, speed * Time.deltaTime));
    }
}
