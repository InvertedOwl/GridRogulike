using System;
using UnityEngine;
using Random = System.Random;

public class InitVelocity : MonoBehaviour
{
    public int startAngle = 90;
    public int endAngle = 180;
    public float forceStrength = 5;
    
    public void Start()
    {
        Random random = new Random();
        float radianStartAngle = startAngle * Mathf.Deg2Rad;
        float radianEndAngle = endAngle * Mathf.Deg2Rad;
        
        
        float yoverx = Mathf.Tan((float) random.NextDouble() * (radianEndAngle - radianStartAngle) + startAngle);
        
        GetComponent<Rigidbody2D>().AddForce(new Vector2(Mathf.Sign(yoverx), Mathf.Abs(yoverx)).normalized * forceStrength, ForceMode2D.Impulse);
    }
}
