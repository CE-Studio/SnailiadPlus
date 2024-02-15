using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon2 : Enemy
{
    private const float AIM_TIMEOUT = 0.25f;
    private const float AIM_TIMEOUT_MOD = 1f;
    private const float SHOT_TIMEOUT = 2.5f;
    private const float SHOT_TIMEOUT_MOD = 0.6f;
    private const float SHOT_SPEED = 140f;

    private float aimTimeout;
    private float shotTimeout;
    private Vector2 velocity;
    private string baseDir;
    private bool isAlive = true;

    private GameObject baseObj;
    private AnimationModule baseAnim;

    public void Awake()
    {
        Spawn(650, 0, 0, true, 3);
        canDamage = false;
        baseObj = transform.GetChild(0).gameObject;
        baseAnim = baseObj.GetComponent<AnimationModule>();
        for (int i = 0; i < PlayState.DIRS_COMPASS.Length; i++)
            anim.Add("Enemy_cannon2_" + PlayState.DIRS_COMPASS[i]);
        for (int i = 0; i < PlayState.DIRS_SURFACE.Length; i++)
        {
            baseAnim.Add("Enemy_cannon2_base_" + PlayState.DIRS_SURFACE[i]);
            baseAnim.Add("Enemy_cannon2_baseDestroyed_" + PlayState.DIRS_SURFACE[i]);
        }
        aimTimeout = transform.position.x / 38.2f % 0.25f;
        shotTimeout = (SHOT_TIMEOUT + transform.position.x / 96f) % 2f;
    }

    public void PlayAnim(string dir, bool targetBase = false)
    {
        if (targetBase)
        {
            baseAnim.Play("Enemy_cannon2_base_" + dir);
            baseDir = dir;
        }
        else
            anim.Play("Enemy_cannon2_" + dir);
    }

    private void AimCannon()
    {
        float output = Mathf.Atan2(PlayState.player.transform.position.y - transform.position.y, PlayState.player.transform.position.x - transform.position.x);
        output = Mathf.RoundToInt(output * 4f / Mathf.PI) / 4f * Mathf.PI;
        velocity = new Vector2(Mathf.Cos(output), Mathf.Sin(output)) * SHOT_SPEED;
        string dirStr = (velocity.y > 0.1f ? "N" : (velocity.y < -0.1f ? "S" : "")) + (velocity.x > 0.1f ? "E" : (velocity.x < -0.1f ? "W" : ""));
        PlayAnim(dirStr);
    }

    private void Shoot()
    {
        Vector2 normVel = velocity.normalized;
        PlayState.ShootEnemyBullet(this, transform.position, EnemyBullet.BulletType.spikeball, new float[] { SHOT_SPEED * 0.0625f, normVel.x, normVel.y });
    }

    public void Update()
    {
        if (PlayState.gameState != PlayState.GameState.game || !isAlive)
            return;

        aimTimeout -= Time.deltaTime * AIM_TIMEOUT_MOD;
        if (aimTimeout < 0)
        {
            aimTimeout = AIM_TIMEOUT;
            AimCannon();
        }
        if (PlayState.OnScreen(transform.position, col))
        {
            if (!PlayState.paralyzed)
                shotTimeout -= Time.deltaTime * SHOT_TIMEOUT_MOD;
            if (shotTimeout < 0)
            {
                shotTimeout = SHOT_TIMEOUT;
                Shoot();
            }
        }
    }

    public override void Kill()
    {
        PlayState.PlaySound("EnemyKilled1");
        for (int i = Random.Range(1, 4); i > 0; i--)
            PlayState.RequestParticle(new Vector2(Random.Range(transform.position.x - 0.5f, transform.position.x + 0.5f),
                Random.Range(transform.position.y - 0.5f, transform.position.y + 0.5f)), "explosion", new float[] { 2 });
        sprite.enabled = false;
        col.enabled = false;
        isAlive = false;
        baseAnim.Play("Enemy_cannon2_baseDestroyed_" + baseDir);
        SpawnHealthOrbs();
    }
}
