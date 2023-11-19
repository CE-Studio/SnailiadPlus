using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StompyEye : Enemy
{
    private Stompy boss;

    private Enemy pupil;
    private AnimationModule pupilAnim;
    private Enemy eyelid;
    private AnimationModule eyelidAnim;

    private const float CLUSTER_TIMEOUT = 7.2f;
    private const float SHOT_TIMEOUT = 0.6f;
    private const int SHOT_NUM = 2;
    private const float CLOSE_DELAY = 0.3f;

    private bool isOpen = true;
    private float blinkTimeout = 4f;
    private float openTimeout = 0f;
    private float closeTimeout = 0f;
    private bool willClose = false;
    public bool isLeft = false;
    private float clusterTimeout = CLUSTER_TIMEOUT;
    private float shotTimeout = SHOT_TIMEOUT;
    private int shots;
    private bool shooting = false;
    public bool shouldAttack;

    private void Awake()
    {
        Spawn(50000, 0, 0, true);
        boss = transform.parent.GetComponent<Stompy>();
        pupil = transform.Find("Pupil").GetComponent<Enemy>();
        pupil.Spawn(50000, 0, 0, true);
        eyelid = transform.Find("Eyelid").GetComponent<Enemy>();
        eyelid.Spawn(50000, 0, 0, true);
        pupilAnim = transform.Find("Pupil").GetComponent<AnimationModule>();
        eyelidAnim = transform.Find("Eyelid").GetComponent<AnimationModule>();
    }

    public override void LateUpdate()
    {
        if (PlayState.gameState != PlayState.GameState.game && !PlayState.playerScript.inDeathCutscene)
            return;

        if (!stunInvulnerability && PlayState.OnScreen(transform.position, col) && !invulnerable)
        {
            List<Bullet> bulletsToDespawn = new();
            bool killFlag = false;
            int maxDamage = 0;
            foreach (Bullet bullet in intersectingBullets)
            {
                if (!immunities.Contains(bullet.bulletType) && bullet.damage - defense > 0)
                {
                    int thisDamage = Mathf.FloorToInt((bullet.damage - defense) *
                        (weaknesses.Contains(bullet.bulletType) ? 2 : 1) * (resistances.Contains(bullet.bulletType) ? 0.5f : 1));
                    if (thisDamage > maxDamage)
                        maxDamage = thisDamage;
                }
                else
                {
                    if (!PlayState.armorPingPlayedThisFrame && makeSoundOnPing)
                    {
                        PlayState.armorPingPlayedThisFrame = true;
                        PlayState.PlaySound("Ping");
                    }
                    pingPlayer -= 1;
                }
                if (!letsPermeatingShotsBy || bullet.bulletType == 1)
                    bulletsToDespawn.Add(bullet);
            }
            if (maxDamage > 0 && isOpen)
            {
                if (boss.health - maxDamage <= 0)
                    killFlag = true;
                else
                    boss.Damage(maxDamage, isLeft);
                if (!willClose)
                {
                    willClose = true;
                    closeTimeout = CLOSE_DELAY;
                }
            }
            while (bulletsToDespawn.Count > 0)
            {
                intersectingBullets.RemoveAt(0);
                bulletsToDespawn[0].GetComponent<Bullet>().Despawn(true);
                bulletsToDespawn.RemoveAt(0);
            }
            if (killFlag)
                boss.Kill();
        }

        if (intersectingBullets.Count == 0)
        {
            pingPlayer = 0;
        }
    }

    private void Shoot(float angle)
    {
        float angleX = -Mathf.Cos(angle);
        float angleY = Mathf.Sin(angle);
        PlayState.ShootEnemyBullet(this, pupil.transform.position, EnemyBullet.BulletType.donutLinear, new float[] { 2.5f, angleX, angleY });
    }

    private void Update()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        float playerDir = Mathf.Atan2(PlayState.player.transform.position.y - (transform.position.y - 1.25f),
            PlayState.player.transform.position.x - transform.position.x);
        pupil.transform.position = new Vector2(transform.position.x + Mathf.Cos(playerDir) * 1.25f, transform.position.y + Mathf.Sin(playerDir) * 0.625f);

        if (shouldAttack)
        {
            clusterTimeout -= Time.deltaTime;
            if (clusterTimeout < 0)
            {
                clusterTimeout = CLUSTER_TIMEOUT;
                shotTimeout = SHOT_TIMEOUT;
                shots = SHOT_NUM;
                shooting = true;
            }
            if (shooting)
            {
                shotTimeout -= Time.deltaTime;
                if (shotTimeout < 0)
                {
                    shotTimeout = SHOT_TIMEOUT;
                    --shots;
                    if (shots == 0)
                        shooting = false;
                    float fireAngle;
                    if (isLeft)
                        fireAngle = -Mathf.PI / SHOT_NUM * shots;
                    else
                        fireAngle = -Mathf.PI / SHOT_NUM * (SHOT_NUM - shots);
                    Shoot(fireAngle);
                }
            }
        }

        if (isOpen)
        {
            blinkTimeout -= Time.deltaTime;
            if (blinkTimeout < 0)
            {
                blinkTimeout = Random.Range(0f, 1f) * 8f + 1f;
                eyelidAnim.Play("Boss_stompy_eyelid" + (isLeft ? "L" : "R") + boss.attackMode.ToString() + "_blink");
            }
            if (willClose)
            {
                closeTimeout -= Time.deltaTime;
                if (closeTimeout < 0)
                {
                    willClose = false;
                    isOpen = false;
                    eyelidAnim.Play("Boss_stompy_eyelid" + (isLeft ? "L" : "R") + boss.attackMode.ToString() + "_close");
                    openTimeout = 0.8f;
                }
            }
        }
        else
        {
            openTimeout -= Time.deltaTime;
            if (openTimeout < 0)
            {
                isOpen = true;
                eyelidAnim.Play("Boss_stompy_eyelid" + (isLeft ? "L" : "R") + boss.attackMode.ToString() + "_open");
            }
        }
    }

    public override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerBullet"))
            intersectingBullets.Add(collision.GetComponent<Bullet>());
    }

    public void StartFlash()
    {
        StartCoroutine(Flash());
    }

    public override IEnumerator Flash(bool playSound = true)
    {
        mask.enabled = true;
        pupil.mask.enabled = true;
        eyelid.mask.enabled = true;
        stunInvulnerability = true;
        if (playSound)
            PlayState.PlaySound("Explode" + Random.Range(1, 5));
        yield return new WaitForFixedUpdate();
        mask.enabled = false;
        pupil.mask.enabled = false;
        eyelid.mask.enabled = false;
        yield return new WaitForFixedUpdate();
        stunInvulnerability = false;
    }

    public void SetSolid(bool state)
    {
        if (col == null)
            col = GetComponent<Collider2D>();
        col.enabled = state;
    }
}
