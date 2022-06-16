using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public int bulletType;
    public Vector2 origin;
    public Vector2 direction;
    public float speed;
    public bool isActive;
    private float lifeTimer;
    public int damage;
    public float maxLifetime;

    public SpriteRenderer sprite;
    public AnimationModule anim;
    public BoxCollider2D box;
    
    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<AnimationModule>();
        box = GetComponent<BoxCollider2D>();

        anim.Add("Bullet_enemy_peashooter");

        sprite.enabled = false;
        box.enabled = false;
    }

    void FixedUpdate()
    {
        if (PlayState.gameState != "Game")
            return;

        if (isActive)
        {
            lifeTimer += Time.fixedDeltaTime;
            switch (bulletType)
            {
                case 0:
                    transform.position = new Vector2(transform.position.x + (direction.x * speed * Time.fixedDeltaTime),
                        transform.position.y + (direction.y * speed * Time.fixedDeltaTime));
                    break;
            }
            if (lifeTimer > maxLifetime)
                Despawn();
        }
    }

    public void Shoot(Vector2 newOrigin, int type, Vector2 newDirection, float newSpeed)
    {
        sprite.enabled = true;
        box.enabled = true;
        isActive = true;

        origin = newOrigin;
        transform.position = newOrigin;
        speed = newSpeed;
        direction = newDirection;

        bulletType = type;
        switch (type)
        {
            case 0:
                anim.Play("Bullet_enemy_peashooter");
                damage = 2;
                maxLifetime = 3.6f;
                box.size = new Vector2(0.25f, 0.25f);
                PlayState.PlaySound("ShotPeashooter");
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerCollide") && bulletType == 0)
        {
            Despawn();
        }
        else if (collision.CompareTag("Player") && !PlayState.playerScript.stunned)
        {
            PlayState.playerScript.health = Mathf.RoundToInt(Mathf.Clamp(PlayState.playerScript.health - damage, 0, Mathf.Infinity));
            if (PlayState.playerScript.health <= 0)
                PlayState.playerScript.Die();
            else
                PlayState.playerScript.BecomeStunned();
        }
    }

    public void Despawn()
    {
        if (isActive)
        {
            isActive = false;
            sprite.enabled = false;
            box.enabled = false;
            lifeTimer = 0;
            transform.position = Vector2.zero;
        }
    }
}
