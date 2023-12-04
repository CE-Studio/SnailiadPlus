using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shellbreaker : Boss
{
    private GameObject eyes;
    private AnimationModule eyeAnim;

    private float lifetime = 0;
    private int lifeState = 1;
    private bool isFiring = false;

    private int handCount = 3;
    public GameObject hand;
    private List<GameObject> hands = new();
    private List<AnimationModule> handAnims = new();
    private List<BoxCollider2D> handBoxes = new();
    private List<float> handTheta = new();
    private List<float> handThetaSpeed = new();
    private float handRadius;
    private float currentRadiusMultiplier = 1;
    private float targetRadiusMultiplier = 1;

    private int shotMax = 5;
    private float shotDelay = 0.8f;
    private int firingPattern = 0;
    private float patternDelay = 3f;
    private float weaponSpeed = 16.875f;
    private float turboMultiplier = 0.6f;
    private float stateTimeout = 0f;
    private int currentShotCount = 0;
    private bool hasBeenShocked = false;
    private Vector2 shockDir = Vector2.zero;

    void Awake()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        if (PlayState.IsBossAlive(0))
        {
            SpawnBoss(Mathf.FloorToInt(450 * (PlayState.currentProfile.difficulty == 2 ? 1 : 0.88f)),
                2, 3, true, 0, true);
            StartCoroutine(RunIntro());
            PlayState.playerScript.CorrectGravity(true);
            eyes = transform.GetChild(0).gameObject;
            
            handCount *= (PlayState.currentProfile.difficulty == 2 ? 4 : 1) * ((PlayState.currentProfile.character == "Sluggy" ||
                PlayState.currentProfile.character == "Leechy") ? 2 : 1);
            shotMax = (PlayState.currentProfile.difficulty == 2 ? 6 : 1) * ((PlayState.currentProfile.character == "Sluggy" ||
                PlayState.currentProfile.character == "Leechy") ? 10 : 1);
            if (PlayState.currentProfile.difficulty == 2)
            {
                patternDelay = 4;
                weaponSpeed = 18.125f;
            }
            stateTimeout = patternDelay;

            hand = Resources.Load<GameObject>("Objects/Enemies/Bosses/Shellbreaker Hand");
            
            for (int i = 0; i < handCount; i++)
            {
                GameObject newHand = Instantiate(hand, origin, Quaternion.identity, transform);
                hands.Add(newHand);
                handAnims.Add(newHand.GetComponent<AnimationModule>());
                handBoxes.Add(newHand.GetComponent<BoxCollider2D>());
                handTheta.Add(2f * Mathf.PI / (float)handCount);
                handThetaSpeed.Add(2.5f + i * 0.75f);
            }

            eyeAnim = eyes.GetComponent<AnimationModule>();
            for (int i = 1; i <= 3; i++)
            {
                foreach (string type in new string[] { "normal", "fire", "shockedN", "shockedE", "shockedS", "shockedW" })
                    anim.Add(string.Format("Boss_shellbreaker_{0}{1}", type, i));
                foreach (string type in new string[] { "normal", "fire" })
                    eyeAnim.Add(string.Format("Boss_shellbreaker_eyes_{0}{1}", type, i));
            }
            foreach(AnimationModule handAnim in handAnims)
            {
                handAnim.Add("Boss_shellbreaker_hand_1");
                handAnim.Add("Boss_shellbreaker_hand_2");
                handAnim.Add("Boss_shellbreaker_hand_3");
            }

            PlayAnim("normal");

            PlayState.globalFunctions.CreateLightMask(20, transform);
        }
        else
            Destroy(gameObject);
    }

    void Update()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        if (hasBeenShocked)
        {
            if (PlayState.playerScript.gravShockState == 2)
                transform.position = (Vector2)PlayState.player.transform.position + (0.8f * shockDir);
            else
                Kill();
            return;
        }

        lifeState = health <= maxHealth * 0.3333f ? 3 : (health <= maxHealth * 0.6667f ? 2 : 1);
        shotMax = lifeState == 3 ? 41 : (lifeState == 2 ? 14 : 5);
        turboMultiplier = lifeState == 3 ? 0.1f : (lifeState == 2 ? 0.28f : 0.6f);
        if (anim.currentAnimName[anim.currentAnimName.Length - 1].ToString() != lifeState.ToString())
        {
            PlayAnim(isFiring ? "fire" : "normal");
            if (isFiring)
                stateTimeout = shotDelay * turboMultiplier;
        }

        lifetime += Time.deltaTime * (PlayState.currentProfile.difficulty == 2 ? 1.2f : 1);
        float modifier = Mathf.Sin(3f / 7f * lifetime);
        transform.localPosition = new Vector2(origin.x + 9 * Mathf.Cos(lifetime) * modifier, origin.y - 7 * Mathf.Sin(lifetime) * modifier);

        handRadius = Mathf.Clamp(1.75f * 5.625f * Mathf.Sin(Mathf.Sin(lifetime * 5f / 3f)), 3.125f, Mathf.Infinity) * 16;
        handRadius *= currentRadiusMultiplier;
        currentRadiusMultiplier = currentRadiusMultiplier * 0.9f + targetRadiusMultiplier * 0.1f;
        for (int i = 0; i < handCount; i++)
        {
            handTheta[i] += handThetaSpeed[i] * Time.deltaTime * (1f + Mathf.Sin(lifetime * 5f / 4f)) * 1.2f;
            hands[i].transform.localPosition = new Vector2(-Mathf.Sin(handTheta[i]) * (handRadius * 0.0625f),
                -Mathf.Cos(handTheta[i]) * (handRadius * 0.0625f));
            handBoxes[i].enabled = targetRadiusMultiplier != 0;
        }

        eyes.transform.localPosition = (PlayState.player.transform.position - transform.position).normalized * 0.125f;

        stateTimeout -= Time.deltaTime;
        if (stateTimeout < 0)
        {
            if (!isFiring)
            {
                targetRadiusMultiplier = 0;
                isFiring = true;
            }
            if (isFiring)
            {
                stateTimeout = shotDelay * turboMultiplier;
                PlayAnim("fire");
                float angle = Vector2.SignedAngle(Vector2.up, (PlayState.player.transform.position - transform.position).normalized);
                angle = firingPattern switch
                {
                    1 => angle + (Mathf.PI / shotMax - Mathf.PI / 2f) * currentShotCount / 12f * Mathf.Rad2Deg,
                    2 => angle + Mathf.PI * Mathf.Rad2Deg,
                    3 => angle - (Mathf.PI / shotMax - Mathf.PI / 2f) * currentShotCount / 12f * Mathf.Rad2Deg,
                    _ => angle
                };
                if (PlayState.ShootEnemyBullet(this, transform.position, EnemyBullet.BulletType.boomBlue, angle, weaponSpeed))
                    currentShotCount++;
            }
            if (currentShotCount >= shotMax)
            {
                isFiring = false;
                firingPattern = (firingPattern + 1) % 4;
                currentShotCount = 0;
                targetRadiusMultiplier = 1;
                stateTimeout = patternDelay;
                PlayAnim("normal");
            }
        }
    }

    public override void LateUpdate()
    {
        if ((PlayState.gameState != PlayState.GameState.game && !PlayState.playerScript.inDeathCutscene) || hasBeenShocked)
            return;

        if (intersectingPlayer && !PlayState.playerScript.stunned && canDamage)
        {
            bool canHit = true;
            if ((elementType.ToLower() == "ice" && PlayState.CheckShellLevel(1) && PlayState.currentProfile.difficulty != 2) ||
                (elementType.ToLower() == "fire" && PlayState.CheckShellLevel(3)))
                canHit = false;
            if (canHit)
                PlayState.playerScript.HitFor(attack, this);
        }

        if (!stunInvulnerability && PlayState.OnScreen(transform.position, col) && !invulnerable)
        {
            List<Bullet> bulletsToDespawn = new();
            List<EnemyBullet> enemyBulletsToDespawn = new();
            bool killFlag = false;
            int maxDamage = parryDamage;
            foreach (Bullet bullet in intersectingBullets)
            {
                int thisDamage = bullet.damage;
                if (bullet.bulletType == 7 || bullet.bulletType == 8)
                {
                    hasBeenShocked = true;
                    for (int i = 0; i < hands.Count; i++)
                        hands[i].GetComponent<Enemy>().Kill();
                    shockDir = PlayState.playerScript.gravityDir switch
                    {
                        Player.Dirs.Floor => Vector2.down,
                        Player.Dirs.WallL => Vector2.left,
                        Player.Dirs.WallR => Vector2.right,
                        _ => Vector2.up
                    };
                    PlayAnim("shocked");
                    return;
                }    
                if (!immunities.Contains(bullet.bulletType) && bullet.damage - defense > 0)
                {
                    thisDamage = Mathf.FloorToInt((bullet.damage - defense) *
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
                if (!letsPermeatingShotsBy || bullet.bulletType == 1 || !bullet.isActive)
                    bulletsToDespawn.Add(bullet);
            }
            foreach (EnemyBullet bullet in intersectingEnemyBullets)
            {
                if (bullet.hasBeenParried)
                {
                    if (bullet.damage - defense > 0)
                    {
                        int thisDamage = Mathf.FloorToInt(bullet.damage - defense);
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
                    if (!letsPermeatingShotsBy || bullet.bulletType == EnemyBullet.BulletType.pea || !bullet.isActive)
                        enemyBulletsToDespawn.Add(bullet);
                }
            }
            if (maxDamage > 0)
            {
                health -= maxDamage;
                if (health <= 0)
                    killFlag = true;
                else
                    StartCoroutine(Flash());
            }
            parryDamage = 0;
            while (bulletsToDespawn.Count > 0)
            {
                intersectingBullets.RemoveAt(0);
                bulletsToDespawn[0].Despawn(true);
                bulletsToDespawn.RemoveAt(0);
            }
            while (enemyBulletsToDespawn.Count > 0)
            {
                intersectingEnemyBullets.RemoveAt(0);
                enemyBulletsToDespawn[0].Despawn();
                enemyBulletsToDespawn.RemoveAt(0);
            }
            if (killFlag)
                Kill();
        }

        if (intersectingBullets.Count == 0)
        {
            pingPlayer = 0;
        }

        mask.transform.localScale = new Vector2(sprite.flipX ? -1 : 1, sprite.flipY ? -1 : 1);

        if (introTimer >= introTimestamps[4])
            barMask.transform.localPosition = new Vector2(
                Mathf.Floor(Mathf.Lerp(barPointLeft, barPointRight, Mathf.InverseLerp(0, maxHealth, health)) * 16) * 0.0625f,
                barMask.transform.localPosition.y);
    }

    private void PlayAnim(string newAnim)
    {
        if (newAnim == "shocked")
        {
            string dir = PlayState.playerScript.gravityDir switch
            {
                Player.Dirs.Floor => "S",
                Player.Dirs.WallL => "W",
                Player.Dirs.WallR => "E",
                _ => "N"
            };
            anim.Play("Boss_shellbreaker_" + newAnim + dir + lifeState);
            eyes.SetActive(false);
            return;
        }
        anim.Play("Boss_shellbreaker_" + newAnim + lifeState);
        eyeAnim.Play("Boss_shellbreaker_eyes_" + newAnim + lifeState);
        if (handAnims[0].currentAnimName == "" || handAnims[0].currentAnimName[handAnims[0].currentAnimName.Length - 1].ToString() != lifeState.ToString())
        {
            foreach (AnimationModule handAnim in handAnims)
                handAnim.Play("Boss_shellbreaker_hand_" + lifeState);
        }
    }

    public override void Kill()
    {
        PlayState.QueueAchievementPopup(AchievementPanel.Achievements.BeatShellbreaker);
        base.Kill();
    }
}
