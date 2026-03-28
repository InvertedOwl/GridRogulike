using System;
using UnityEngine;

public class SetGravityFromAnimationSpeed : MonoBehaviour
{
    private Rigidbody2D rb2d;

    public void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        rb2d.gravityScale = (GameplayNavSettings.speed + 1) * 2f;
    }
}
