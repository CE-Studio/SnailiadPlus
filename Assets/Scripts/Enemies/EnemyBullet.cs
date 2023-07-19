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
    private float radiusVelocity;
    private float thetaVelocity;
    private float thetaOffset;
    private float travelAngle;
    private float turnSpeed;
    private bool despawnOffscreen;

    private bool[] playerBulletsDestroyedByMe = new bool[6];
    private bool[] playerBulletsThatDestroyMe = new bool[6];
    private int bulletInteraction = 0;
    // 0 = always destroy
    // 1 = destroy parallel
    // 2 = destroy parallel (wider range)
    // 3 = destroy perpendicular
    // 4 = destroy perpendicular (wider range)

    public SpriteRenderer sprite;
    public AnimationModule anim;
    public BoxCollider2D box;

    public enum BulletType { pea, bigPea, boomBlue, boomRed, laser, donutLinear, donutRotary, donutHybrid, spikeball, shadowWave, gigaWave }
    
    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<AnimationModule>();
        box = GetComponent<BoxCollider2D>();

        anim.Add("Bullet_enemy_peashooter");
        foreach (string dir in PlayState.DIRS_COMPASS)
        {
            anim.Add("Bullet_enemy_gigaPea_" + dir);
            anim.Add("Bullet_enemy_boomerang1_" + dir);
            anim.Add("Bullet_enemy_boomerang2_" + dir);
            anim.Add("Bullet_enemy_donut_linear_" + dir);
            anim.Add("Bullet_enemy_spikeball_" + dir);
            anim.Add("Bullet_enemy_shadowWave_" + dir);
        }
        foreach (string dir in PlayState.DIRS_CARDINAL)
        {
            anim.Add("Bullet_enemy_gigaWave_" + dir);
        }
        anim.Add("Bullet_enemy_donut_rotary_CW");
        anim.Add("Bullet_enemy_donut_rotary_CCW");
        anim.Add("Bullet_enemy_donut_hybrid_CW");
        anim.Add("Bullet_enemy_donut_hybrid_CCW");
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
                case BulletType.bigPea:
                    transform.position = new Vector2(transform.position.x + (direction.x * speed * Time.fixedDeltaTime),
                        transform.position.y + (direction.y * speed * Time.fixedDeltaTime));
                    break;
                case BulletType.boomBlue:
                    transform.position = new Vector2(transform.position.x + (direction.x * speed * Time.fixedDeltaTime),
                        transform.position.y + (direction.y * speed * Time.fixedDeltaTime));
                    speed -= initialSpeed * 1.5f * Time.fixedDeltaTime;
                    break;
                case BulletType.boomRed:
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
                        origin.x + radiusVelocity * lifeTimer * Mathf.Cos(lifeTimer * thetaVelocity + thetaOffset),
                        origin.y - radiusVelocity * lifeTimer * Mathf.Sin(lifeTimer * thetaVelocity + thetaOffset)
                        );
                    break;
                case BulletType.donutHybrid:
                    float currentAngle = Mathf.Atan2(PlayState.player.transform.position.y - origin.y, PlayState.player.transform.position.x - origin.x);
                    float newAngle = currentAngle - travelAngle;
                    while (newAngle > Mathf.PI)
                        newAngle -= PlayState.TAU;
                    while (newAngle < -Mathf.PI)
                        newAngle += PlayState.TAU;
                    if (newAngle > 0)
                        travelAngle -= Mathf.PI * Time.fixedDeltaTime * turnSpeed;
                    else if (newAngle < 0)
                        travelAngle += Mathf.PI * Time.fixedDeltaTime * turnSpeed;
                    direction = new Vector2(-Mathf.Cos(travelAngle), -Mathf.Sin(travelAngle));

                    speed += 9.375f * Time.fixedDeltaTime;
                    origin += new Vector2(direction.x * speed * Time.fixedDeltaTime, direction.y * speed * Time.fixedDeltaTime);
                    transform.position = new Vector2(
                        origin.x + radiusVelocity * lifeTimer * Mathf.Cos(lifeTimer * Mathf.PI * thetaVelocity + thetaOffset),
                        origin.y - radiusVelocity * lifeTimer * Mathf.Sin(lifeTimer * Mathf.PI * thetaVelocity + thetaOffset)
                        );
                    break;
                case BulletType.spikeball:
                    transform.position = new Vector2(transform.position.x + (direction.x * speed * Time.fixedDeltaTime),
                        transform.position.y + (direction.y * speed * Time.fixedDeltaTime));
                    break;
                case BulletType.shadowWave:
                    transform.position = new Vector2(transform.position.x + (direction.x * speed),
                        transform.position.y + (direction.y * speed));
                    speed += initialSpeed * 18f * Time.fixedDeltaTime;
                    break;
                case BulletType.gigaWave:
                    transform.position = new Vector2(transform.position.x + (direction.x * speed),
                        transform.position.y + (direction.y * speed));
                    speed += initialSpeed * 18f * Time.fixedDeltaTime;
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
                despawnOffscreen = true;
                if (playSound)
                    soundID = "ShotPeashooter";
                break;
            case BulletType.bigPea:
                anim.Play("Bullet_enemy_gigaPea_" + VectorToCompass(new Vector2(dirVelVars[1], dirVelVars[2])));
                damage = 6;
                maxLifetime = 3.6f;
                box.size = new Vector2(1.45f, 1.45f);
                speed = dirVelVars[0];
                direction = new Vector2(dirVelVars[1], dirVelVars[2]);
                despawnOffscreen = true;
                if (playSound)
                    soundID = "ShotPeashooterDev";
                break;
            case BulletType.boomBlue:
                anim.Play("Bullet_enemy_boomerang1_" + VectorToCompass(new Vector2(dirVelVars[1], dirVelVars[2])));
                damage = 2;
                maxLifetime = 4f;
                box.size = new Vector2(1.45f, 1.45f);
                speed = dirVelVars[0];
                direction = new Vector2(dirVelVars[1], dirVelVars[2]);
                despawnOffscreen = false;
                if (playSound)
                    soundID = "ShotBoomerangDev";
                break;
            case BulletType.boomRed:
                anim.Play("Bullet_enemy_boomerang2_" + VectorToCompass(new Vector2(dirVelVars[1], dirVelVars[2])));
                damage = 2;
                maxLifetime = 4f;
                box.size = new Vector2(1.45f, 1.45f);
                speed = dirVelVars[0];
                direction = new Vector2(dirVelVars[1], dirVelVars[2]);
                despawnOffscreen = false;
                if (playSound)
                    soundID = "ShotBoomerangDev";
                SetDestroyableLevels("111111", false);
                SetDestroyableLevels("111111", true);
                bulletInteraction = 0;
                break;
            case BulletType.laser:
                anim.Play("Bullet_enemy_laser_" + (dirVelVars[1] == -1 ? "left" : "right"));
                damage = 3;
                maxLifetime = 3f;
                box.size = new Vector2(1.45f, 0.25f);
                speed = dirVelVars[0];
                direction = new Vector2(dirVelVars[1], dirVelVars[2]);
                despawnOffscreen = true;
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
                radiusVelocity = dirVelVars[0];
                thetaVelocity = dirVelVars[1];
                thetaOffset = dirVelVars[2];
                despawnOffscreen = true;
                if (playSound)
                    soundID = "ShotEnemyDonut";
                break;
            case BulletType.donutHybrid:
                anim.Play("Bullet_enemy_donut_hybrid_" + (dirVelVars[4] > 0 ? "CW" : "CCW"));
                damage = 2;
                maxLifetime = 3f;
                box.size = new Vector2(1.45f, 1.45f);
                speed = dirVelVars[0];
                travelAngle = dirVelVars[1];
                turnSpeed = dirVelVars[2];
                radiusVelocity = dirVelVars[3];
                thetaVelocity = dirVelVars[4];
                thetaOffset = dirVelVars[5];
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
            case BulletType.shadowWave:
                anim.Play("Bullet_enemy_shadowWave_" + VectorToCompass(new Vector2(dirVelVars[1], dirVelVars[2])));
                damage = 16;
                maxLifetime = 2f;
                box.size = new Vector2(2.4f, 2.4f);
                speed = dirVelVars[0];
                direction = new Vector2(dirVelVars[1], dirVelVars[2]);
                despawnOffscreen = true;
                if (playSound)
                    soundID = "ShotRainbowDev";
                SetDestroyableLevels("111111", false);
                SetDestroyableLevels("111111", true);
                bulletInteraction = 0;
                break;
            case BulletType.gigaWave:
                anim.Play("Bullet_enemy_gigaWave_" + VectorToCardinal(new Vector2(dirVelVars[1], dirVelVars[2])));
                damage = 12;
                maxLifetime = 2f;
                box.size = new Vector2(2f, 5.9f);
                speed = dirVelVars[0];
                direction = new Vector2(dirVelVars[1], dirVelVars[2]);
                despawnOffscreen = true;
                if (playSound)
                    soundID = "ShotEnemyGigaWave";
                SetDestroyableLevels("111111", true);
                bulletInteraction = 2;
                break;
        }
        initialSpeed = speed;

        if (!PlayState.OnScreen(transform.position, box))
            Despawn();
        else if (playSound)
            PlayState.PlaySound(soundID);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerBullet"))
        {
            Bullet bullet = collision.GetComponent<Bullet>();
            bool destroyFlag = false;
            float angleBetween = Vector2.Angle(direction, bullet.Vector2Direction());
            while (angleBetween > 180f)
                angleBetween -= 180f;
            switch (bulletInteraction)
            {
                case 0:
                    destroyFlag = true;
                    break;
                case 1:
                    if (angleBetween <= 22.5f || angleBetween >= 157.5f)
                        destroyFlag = true;
                    break;
                case 2:
                    if (angleBetween <= 67.5f || angleBetween >= 112.5f)
                        destroyFlag = true;
                    break;
                case 3:
                    if (angleBetween >= 67.5f && angleBetween <= 112.5f)
                        destroyFlag = true;
                    break;
                case 4:
                    if (angleBetween >= 22.5f && angleBetween <= 157.5f)
                        destroyFlag = true;
                    break;
                default:
                    return;
            }
            if (destroyFlag)
            {
                int bulletType = bullet.bulletType - 1;
                if (playerBulletsDestroyedByMe[bulletType])
                    bullet.Despawn(true);
                if (playerBulletsThatDestroyMe[bulletType])
                    Despawn();
            }
        }
        else if ((collision.CompareTag("PlayerCollide") || collision.CompareTag("BreakableBlock")) && bulletType == 0)
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
            SetDestroyableLevels("000000", false);
            SetDestroyableLevels("000000", true);
            bulletInteraction = 0;
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

    private string VectorToCardinal(Vector2 dir)
    {
        float angle = Vector2.SignedAngle(Vector2.up, dir);
        while (angle < 0)
            angle += 360;
        while (angle > 360)
            angle -= 360;
        if (angle > 315f)
            return "up";
        else if (angle > 225f)
            return "right";
        else if (angle > 135f)
            return "down";
        else if (angle > 45f)
            return "left";
        else
            return "up";
    }

    private void SetDestroyableLevels(string data, bool setOffensiveArray)
    {
        int maxIndex = playerBulletsDestroyedByMe.Length > data.Length ? data.Length : playerBulletsDestroyedByMe.Length;
        for (int i = 0; i < maxIndex; i++)
            (setOffensiveArray ? playerBulletsDestroyedByMe : playerBulletsThatDestroyMe)[i] = data[i] switch
            {
                '0' => false,
                '1' => true,
                _ => (setOffensiveArray ? playerBulletsDestroyedByMe : playerBulletsThatDestroyMe)[i]
            };
    }
}
