using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kitty : Enemy
{
    private const int MAX_SHOTS = 5;
    private const float WEAPON_SPEED = 12.5f;
    private readonly float[] hopTimeouts = new float[] { 0.7f, 0.8f, 0.6f, 0.7f, 0.8f, 0.6f, 0.7f, 0.8f, 0.6f };
    private readonly float[] hopHeight = new float[] { 1f, 1f, 1f, 1.2f, 1.3f, 1f, 1.2f, 1f, 0.9f };

    void Awake()
    {
        Spawn(100, 2, 1, true, new Vector2(1.95f, 0.95f));
    }

    void Update()
    {
        
    }
}
