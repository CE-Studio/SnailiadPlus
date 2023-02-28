using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon1 : Enemy
{
    private const float AIM_TIMEOUT = 0.25f;
    private const float SHOT_TIMEOUT = 4f;
    private const float TIMEOUT_DEC_MOD = 0.6f;
    private const float SHOT_SPEED = 140f;

    private Vector2 delta;
    private float aimTimeout;
    private float shotTimeout;
    private Vector2 velocity;

    private GameObject baseObj;
    private AnimationModule baseAnim;

    public void Awake()
    {
        Spawn(2000, 0, 0, true);
        canDamage = false;
        baseObj = transform.GetChild(0).gameObject;
        baseAnim = baseObj.GetComponent<AnimationModule>();
        for (int i = 0; i < PlayState.DIRS_COMPASS.Length; i++)
            anim.Add("Enemy_cannon1_" + PlayState.DIRS_COMPASS[i]);
        for (int i = 0; i < PlayState.DIRS_SURFACE.Length; i++)
            baseAnim.Add("Enemy_cannon1_base_" + PlayState.DIRS_SURFACE[i]);
        aimTimeout = transform.position.x / 38.2f % 0.25f;
        shotTimeout = SHOT_TIMEOUT + transform.position.x / 96f % 6f;
    }

    public void PlayAnim(string dir, bool targetBase = false)
    {
        if (targetBase)
            baseAnim.Play("Enemy_cannon1_base_" + dir);
        else
            anim.Play("Enemy_cannon1_" + dir);
    }

    private void AimCannon()
    {
        float output = Mathf.Atan2(PlayState.player.transform.position.y - transform.position.y, PlayState.player.transform.position.x - transform.position.x);
        output = (int)((output + Mathf.PI / 8f) * 4f / Mathf.PI) / 4f * Mathf.PI;
        velocity = new Vector2(Mathf.Cos(output), Mathf.Sin(output)) * SHOT_SPEED;
        string dirStr = (velocity.y > 0.1f ? "N" : (velocity.y < -0.1f ? "S" : "")) + (velocity.x > 0.1f ? "E" : (velocity.x < -0.1f ? "W" : ""));
        PlayAnim(dirStr);
        Debug.Log("" + velocity.x + ", " + velocity.y + ", " + dirStr);
    }

    private void Shoot()
    {
        Vector2 normVel = velocity.normalized;
        PlayState.ShootEnemyBullet(transform.position, EnemyBullet.BulletType.spikeball, new float[] { SHOT_SPEED * 0.0625f, normVel.x, normVel.y });
    }

    public void Update()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        aimTimeout -= Time.deltaTime;
        if (aimTimeout < 0)
        {
            aimTimeout = AIM_TIMEOUT;
            AimCannon();
        }
        if (PlayState.OnScreen(transform.position, col))
        {
            shotTimeout -= Time.deltaTime * TIMEOUT_DEC_MOD;
            if (shotTimeout < 0)
            {
                shotTimeout = SHOT_TIMEOUT;
                Shoot();
            }
        }
    }
}
