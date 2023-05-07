using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public BulletType bulletType;
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

    public enum BulletType { pea, boomBlue, boomRed, laser, donutLinear, donutRotary, spikeball }
    
    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<AnimationModule>();
        box = GetComponent<BoxCollider2D>();

        anim.Add("Bullet_enemy_peashooter");
        foreach (string dir in PlayState.DIRS_COMPASS)
        {
            anim.Add("Bullet_enemy_boomerang1_" + dir);
            anim.Add("Bullet_enemy_donut_linear_" + dir);
            anim.Add("Bullet_enemy_spikeball_" + dir);
        }
        anim.Add("Bullet_enemy_donut_rotary_CW");
        anim.Add("Bullet_enemy_donut_rotary_CCW");
        anim.Add("Bullet_enemy_laser_left");
        anim.Add("Bullet_enemy_laser_right");

        sprite.enabled = false;
        box.enabled = false;
    }

    void FixedUpdate()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        if (isActive)
        {
            lifeTimer += Time.fixedDeltaTime;
            switch (bulletType)
            {
                case BulletType.pea:
                    transform.position = new Vector2(transform.position.x + (direction.x * speed * Time.fixedDeltaTime),
                        transform.position.y + (direction.y * speed * Time.fixedDeltaTime));
                    break;
                case BulletType.boomBlue:
                    transform.position = new Vector2(transform.position.x + (direction.x * speed * Time.fixedDeltaTime),
                        transform.position.y + (direction.y * speed * Time.fixedDeltaTime));
                    speed -= initialSpeed * 1.5f * Time.fixedDeltaTime;
                    break;
                case BulletType.laser:
                    transform.position = new Vector2(transform.position.x + (direction.x * speed * Time.fixedDeltaTime),
                        transform.position.y + (direction.y * speed * Time.fixedDeltaTime));
                    break;
                case BulletType.donutLinear:
                    transform.position = new Vector2(transform.position.x + (direction.x * speed * Time.fixedDeltaTime),
                        transform.position.y + (direction.y * speed * Time.fixedDeltaTime));
                    break;
                case BulletType.donutRotary:
                    transform.position = new Vector2(
                        origin.x + radius_velocity * lifeTimer * Mathf.Cos(lifeTimer * theta_velocity + theta_offset),
                        origin.y - radius_velocity * lifeTimer * Mathf.Sin(lifeTimer * theta_velocity + theta_offset)
                        );
                    break;
                case BulletType.spikeball:
                    transform.position = new Vector2(transform.position.x + (direction.x * speed * Time.fixedDeltaTime),
                        transform.position.y + (direction.y * speed * Time.fixedDeltaTime));
                    break;
            }
            if (lifeTimer > maxLifetime || (despawnOffscreen && !PlayState.OnScreen(transform.position, box)))
                Despawn();
        }
    }

    public void Shoot(Vector2 newOrigin, BulletType type, float[] dirVelVars, bool playSound = true)
    {
        sprite.enabled = true;
        box.enabled = true;
        isActive = true;

        origin = newOrigin;
        transform.position = newOrigin;

        bulletType = type;
        initialSpeed = speed;
        string soundID = "";
        switch (type)
        {
            case BulletType.pea:
                anim.Play("Bullet_enemy_peashooter");
                damage = 2;
                maxLifetime = 3.6f;
                box.size = new Vector2(0.25f, 0.25f);
                speed = dirVelVars[0];
                direction = new Vector2(dirVelVars[1], dirVelVars[2]);
                despawnOffscreen = false;
                if (playSound)
                    soundID = "ShotPeashooter";
                break;
            case BulletType.boomBlue:
                anim.Play("Bullet_enemy_boomerang1_" + VectorToCompass(new Vector2(dirVelVars[1], dirVelVars[2])));
                damage = 2;
                maxLifetime = 4f;
                box.size = new Vector2(1.5f, 1.5f);
                speed = dirVelVars[0];
                direction = new Vector2(dirVelVars[1], dirVelVars[2]);
                despawnOffscreen = false;
                if (playSound)
                    soundID = "ShotBoomerangDev";
                break;
            case BulletType.laser:
                anim.Play("Bullet_enemy_laser_" + (dirVelVars[1] == -1 ? "left" : "right"));
                damage = 3;
                maxLifetime = 3f;
                box.size = new Vector2(1.45f, 0.25f);
                speed = dirVelVars[0];
                direction = new Vector2(dirVelVars[1], dirVelVars[2]);
                if (playSound)
                    soundID = "ShotEnemyLaser";
                break;
            case BulletType.donutLinear:
                anim.Play("Bullet_enemy_donut_linear_" + VectorToCompass(new Vector2(dirVelVars[1], dirVelVars[2])));
                damage = 2;
                maxLifetime = 3.8f;
                box.size = new Vector2(1.45f, 1.45f);
                speed = dirVelVars[0];
                direction = new Vector2(dirVelVars[1], dirVelVars[2]);
                despawnOffscreen = true;
                if (playSound)
                    soundID = "ShotEnemyDonut";
                break;
            case BulletType.donutRotary:
                anim.Play("Bullet_enemy_donut_rotary_" + (dirVelVars[1] > 0 ? "CW" : "CCW"));
                damage = 2;
                maxLifetime = 3f;
                box.size = new Vector2(1.45f, 1.45f);
                radius_velocity = dirVelVars[0];
                theta_velocity = dirVelVars[1];
                theta_offset = dirVelVars[2];
                despawnOffscreen = true;
                if (playSound)
                    soundID = "ShotEnemyDonut";
                break;
            case BulletType.spikeball:
                anim.Play("Bullet_enemy_spikeball_" + VectorToCompass(new Vector2(dirVelVars[1], dirVelVars[2])));
                damage = 2;
                maxLifetime = 1.6f;
                box.size = new Vector2(1.7f, 1.7f);
                speed = dirVelVars[0];
                direction = new Vector2(dirVelVars[1], dirVelVars[2]);
                despawnOffscreen = true;
                if (playSound)
                    soundID = "Cannon";
                break;
        }

        if (!PlayState.OnScreen(transform.position, box))
            Despawn();
        else if (playSound)
            PlayState.PlaySound(soundID);
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
