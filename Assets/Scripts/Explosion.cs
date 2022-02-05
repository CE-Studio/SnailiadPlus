using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public bool isActive = false;
    public Animator anim;

    public void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void Update()
    {
        if (PlayState.gameState == "Game")
            anim.speed = 1;
        else
            anim.speed = 0;
    }

    public void ResetExpl()
    {
        transform.position = Vector2.zero;
        GetComponent<Animator>().Play("Explosion blank", 0, 0);
        isActive = false;
    }
}
