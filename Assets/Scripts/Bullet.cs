using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public string bulletType;
    public int direction;
    public bool isActive;
    private Vector2 firePos;
    private float lifeTimer;
    private float velocity;

    public SpriteRenderer sprite;
    public Animator anim;
    
    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        sprite.enabled = false;
        anim = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        if (isActive)
        {
            lifeTimer += Time.fixedDeltaTime;
            switch (bulletType)
            {

            }
        }
    }

    public void Shoot(string type, int dir)
    {
        isActive = true;
        firePos = transform.position;
        bulletType = type;
        direction = dir;
        switch (type)
        {
            case "Rainbow Wave":
                anim.SetInteger("bulletType", 0);
                velocity = 0;
                break;
            default:
                anim.SetInteger("bulletType", 0);
                velocity = 0;
                break;
        }
        switch (dir)
        {
            case 1:
                anim.SetInteger("direction", 2);
                sprite.flipX = true;
                break;
            case 2:
                anim.SetInteger("direction", 1);
                break;
            case 3:
                anim.SetInteger("direction", 2);
                break;
            case 4:
                anim.SetInteger("direction", 0);
                sprite.flipX = true;
                break;
            case 5:
                anim.SetInteger("direction", 0);
                break;
            case 6:
                anim.SetInteger("direction", 2);
                sprite.flipX = true;
                sprite.flipY = true;
                break;
            case 7:
                anim.SetInteger("direction", 1);
                sprite.flipY = true;
                break;
            case 8:
                anim.SetInteger("direction", 2);
                sprite.flipY = true;
                break;
        }
    }

    private void MoveNW()
    {

    }
}
