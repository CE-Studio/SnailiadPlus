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
    private float initialSpeed;
    private float radius_velocity;
    private float theta_velocity;
    private float theta_offset;
    private bool despawnOffscreen;

    public SpriteRenderer sprite;
    public AnimationModule anim;
    public BoxCollider2D box;

    private readonly string[] compassDirs = new string[] { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };
    
    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<AnimationModule>();
        box = GetComponent<BoxCollider2D>();

        anim.Add("Bullet_enemy_peashooter");
        foreach (string dir in compassDirs)
            anim.Add("Bullet_enemy_boomerang1_" + dir);
        anim.Add("Bullet_enemy_donut_rotary");

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
                case 0: // Pea
                    transform.position = new Vector2(transform.position.x + (direction.x * speed * Time.fixedDeltaTime),
                        transform.position.y + (direction.y * speed * Time.fixedDeltaTime));
                    break;
                case 1: // Devastator Boomerang (blue)
                    transform.position = new Vector2(transform.position.x + (direction.x * speed * Time.fixedDeltaTime),
                        transform.position.y + (direction.y * speed * Time.fixedDeltaTime));
                    speed -= initialSpeed * 1.5f * Time.fixedDeltaTime;
                    break;
                case 2: // Rotary donut
                    transform.position = new Vector2(
                        origin.x + radius_velocity * lifeTimer * Mathf.Cos(lifeTimer * theta_velocity + theta_offset),
                        origin.y - radius_velocity * lifeTimer * Mathf.Sin(lifeTimer * theta_velocity + theta_offset)
                        );
                    break;
            }
            if ((!despawnOffscreen && lifeTimer > maxLifetime && !PlayState.OnScreen(transform.position, box)) || (despawnOffscreen && !PlayState.OnScreen(transform.position, box)))
                Despawn();
        }
    }

    public void Shoot(Vector2 newOrigin, int type, float[] dirVelVars, bool playSound = true)
    {
        sprite.enabled = true;
        box.enabled = true;
        isActive = true;

        origin = newOrigin;
        transform.position = newOrigin;

        bulletType = type;
        initialSpeed = speed;
        switch (type)
        {
            case 0:
                anim.Play("Bullet_enemy_peashooter");
                damage = 2;
                maxLifetime = 3.6f;
                box.size = new Vector2(0.25f, 0.25f);
                speed = dirVelVars[0];
                direction = new Vector2(dirVelVars[1], dirVelVars[2]);
                despawnOffscreen = false;
                if (playSound)
                    PlayState.PlaySound("ShotPeashooter");
                break;
            case 1:
                anim.Play("Bullet_enemy_boomerang1_" + VectorToCompass(new Vector2(dirVelVars[1], dirVelVars[2])));
                damage = 2;
                maxLifetime = 4f;
                box.size = new Vector2(1.5f, 1.5f);
                speed = dirVelVars[0];
                direction = new Vector2(dirVelVars[1], dirVelVars[2]);
                despawnOffscreen = false;
                if (playSound)
                    PlayState.PlaySound("ShotBoomerangDev");
                break;
            case 2:
                anim.Play("Bullet_enemy_donut_rotary");
                damage = 2;
                maxLifetime = 3f;
                box.size = new Vector2(1.45f, 1.45f);
                radius_velocity = dirVelVars[0];
                theta_velocity = dirVelVars[1];
                theta_offset = dirVelVars[2];
                despawnOffscreen = true;
                if (playSound)
                    PlayState.PlaySound("ShotEnemyDonut");
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((collision.CompareTag("PlayerCollide") || collision.CompareTag("BreakableBlock")) && bulletType == 0)
            Despawn();
        else if (collision.CompareTag("Player") && !PlayState.playerScript.stunned)
            PlayState.playerScript.HitFor(damage);
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

    private string VectorToCompass(Vector2 dir)
    {
        float angle = Vector2.SignedAngle(Vector2.up, dir);
        while (angle < 0)
            angle += 360;
        while (angle > 360)
            angle -= 360;
        if (angle > 337.5f)
            return "N";
        else if (angle > 292.5f)
            return "NE";
        else if (angle > 247.5f)
            return "E";
        else if (angle > 202.5f)
            return "SE";
        else if (angle > 157.5f)
            return "S";
        else if (angle > 112.5f)
            return "SW";
        else if (angle > 67.5f)
            return "W";
        else if (angle > 22.5f)
            return "NW";
        else
            return "N";
    }
}
