using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RushShellbreaker : Boss
{
    private GameObject eyes;
    private AnimationModule eyeAnim;

    private float lifetime = 0;
    private int lifeState = 1;
    private bool isFiring = false;

    private int handCount = 12;
    public GameObject hand;
    private List<GameObject> hands = new();
    private List<AnimationModule> handAnims = new();
    private List<BoxCollider2D> handBoxes = new();
    private List<float> handTheta = new();
    private List<float> handThetaSpeed = new();
    private float handRadius;
    private float currentRadiusMultiplier = 1;
    private float targetRadiusMultiplier = 1;

    private int shotMax = 8;
    private float shotDelay = 0.3f;
    private int firingPattern = 0;
    private float patternDelay = 3f;
    private float weaponSpeed = 16.875f;
    private float turboMultiplier = 0.6f;
    private float stateTimeout = 0f;
    private int currentShotCount = 0;


    void Awake()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        if (PlayState.IsBossAlive(0))
        {
            SpawnBoss(10000, 2, 3, true, 20, 0, true);
            StartCoroutine(RunIntro());
            PlayState.playerScript.CorrectGravity(true);
            eyes = transform.GetChild(0).gameObject;

            handCount += PlayState.currentProfile.difficulty == 2 ? 24 : 0;
            stateTimeout = patternDelay;

            hand = Resources.Load<GameObject>("Objects/Enemies/Bosses/Super Shellbreaker Hand");

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
            anim.Add("RushBoss_shellbreaker_normal1");
            anim.Add("RushBoss_shellbreaker_normal2");
            anim.Add("RushBoss_shellbreaker_normal3");
            anim.Add("RushBoss_shellbreaker_fire1");
            anim.Add("RushBoss_shellbreaker_fire2");
            anim.Add("RushBoss_shellbreaker_fire3");
            eyeAnim.Add("RushBoss_shellbreaker_eyes_normal1");
            eyeAnim.Add("RushBoss_shellbreaker_eyes_normal2");
            eyeAnim.Add("RushBoss_shellbreaker_eyes_normal3");
            eyeAnim.Add("RushBoss_shellbreaker_eyes_fire1");
            eyeAnim.Add("RushBoss_shellbreaker_eyes_fire2");
            eyeAnim.Add("RushBoss_shellbreaker_eyes_fire3");
            foreach (AnimationModule handAnim in handAnims)
            {
                handAnim.Add("RushBoss_shellbreaker_hand_1");
                handAnim.Add("RushBoss_shellbreaker_hand_2");
                handAnim.Add("RushBoss_shellbreaker_hand_3");
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

        PlayState.activeRushData.ssbTime += Time.deltaTime;

        lifeState = health <= maxHealth * 0.3333f ? 3 : (health <= maxHealth * 0.6667f ? 2 : 1);
        shotMax = lifeState == 3 ? 44 : (lifeState == 2 ? 17 : 8);
        turboMultiplier = lifeState == 3 ? 0.19f : (lifeState == 2 ? 0.28f : 0.6f);
        if (anim.currentAnimName[anim.currentAnimName.Length - 1].ToString() != lifeState.ToString())
        {
            PlayAnim(isFiring ? "fire" : "normal");
            if (isFiring)
                stateTimeout = shotDelay * turboMultiplier;
        }

        lifetime += Time.deltaTime * 1.15f;
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
                    1 => angle + (Mathf.PI / shotMax - Mathf.PI / 2f) * currentShotCount / 8f * Mathf.Rad2Deg,
                    2 => angle + Mathf.PI * Mathf.Rad2Deg,
                    3 => angle - (Mathf.PI / shotMax - Mathf.PI / 2f) * currentShotCount / 8f * Mathf.Rad2Deg,
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

    private void PlayAnim(string newAnim)
    {
        anim.Play("RushBoss_shellbreaker_" + newAnim + lifeState);
        eyeAnim.Play("RushBoss_shellbreaker_eyes_" + newAnim + lifeState);
        if (handAnims[0].currentAnimName == "" || handAnims[0].currentAnimName[handAnims[0].currentAnimName.Length - 1].ToString() != lifeState.ToString())
        {
            foreach (AnimationModule handAnim in handAnims)
                handAnim.Play("RushBoss_shellbreaker_hand_" + lifeState);
        }
    }

    public override void Kill()
    {
        PlayState.QueueAchievementPopup(AchievementPanel.Achievements.BeatShellbreaker);
        base.Kill();
    }
}
