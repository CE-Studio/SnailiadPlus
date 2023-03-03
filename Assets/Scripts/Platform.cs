using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
    public int size = 2;
    public int type = 0;
    public float speed;
    public float topSpeed = 0.5f;
    public Vector2 a = Vector2.zero;
    public Vector2 b = Vector2.zero;
    public Vector2 aRelative = Vector2.up * 3;
    public Vector2 bRelative = Vector2.down * 3;
    public float brakePercentage;
    
    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
