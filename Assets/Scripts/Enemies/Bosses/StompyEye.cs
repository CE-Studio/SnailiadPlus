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
        if (!stunInvulnerability && PlayState.OnScreen(transform.position, col) && !invulnerable)
        {
            List<GameObject> bulletsToDespawn = new List<GameObject>();
            bool killFlag = false;
            foreach (GameObject bullet in intersectingBullets)
            {
                Bullet bulletScript = bullet.GetComponent<Bullet>();
                if (!immunities.Contains(bulletScript.bulletType) && bulletScript.damage - defense > 0)
                {
                    int damage = Mathf.FloorToInt((bulletScript.damage - defense) *
                        (weaknesses.Contains(bulletScript.bulletType) ? 2 : 1) * (resistances.Contains(bulletScript.bulletType) ? 0.5f : 1));
                    if (boss.health - damage <= 0)
                        killFlag = true;
                    else
                        boss.Damage(damage, isLeft);
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
                if (!letsPermeatingShotsBy || bulletScript.bulletType == 1)
                    bulletsToDespawn.Add(bullet);
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
        float mult = 40f;
        float angleX = -Mathf.Cos(angle) * mult;
        float angleY = -Mathf.Sin(angle) * mult;
        PlayState.ShootEnemyBullet(pupil.transform.position, EnemyBullet.BulletType.donutLinear, new float[] { mult, angleX, angleY });
    }

    private void Update()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        float playerDir = Mathf.Atan2(PlayState.player.transform.position.y - (transform.position.y - 1.25f),
            PlayState.player.transform.position.x - transform.position.x);
        pupil.transform.position = new Vector2(transform.position.x + Mathf.Cos(playerDir) * 1.25f, transform.position.y + Mathf.Sin(playerDir) * 0.625f);
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
        yield return new WaitForSeconds(0.0125f);
        mask.enabled = false;
        pupil.mask.enabled = false;
        eyelid.mask.enabled = false;
        yield return new WaitForSeconds(0.0125f);
        stunInvulnerability = false;
    }
}
