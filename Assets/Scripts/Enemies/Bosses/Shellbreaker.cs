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
    private List<GameObject> hands = new List<GameObject>();
    private List<AnimationModule> handAnims = new List<AnimationModule>();
    private List<BoxCollider2D> handBoxes = new List<BoxCollider2D>();
    private List<float> handTheta = new List<float>();
    private List<float> handThetaSpeed = new List<float>();
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


    void Awake()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        if (PlayState.IsBossAlive(0))
        {
            SpawnBoss(Mathf.FloorToInt(450 * (PlayState.currentDifficulty == 2 ? 1 : (PlayState.currentCharacter == "Sluggy" ? 0.66f : 0.88f))),
                2, 3, true, 0);
            StartCoroutine(RunIntro());
            eyes = transform.GetChild(0).gameObject;
            PlayState.player.transform.position = new Vector2(PlayState.player.transform.position.x + (2.25f * (PlayState.playerScript.facingLeft ? -1 : 1)),
                PlayState.player.transform.position.y);
            
            handCount *= (PlayState.currentDifficulty == 2 ? 4 : 1) * ((PlayState.currentCharacter == "Sluggy" || PlayState.currentCharacter == "Leechy") ? 2 : 1);
            shotMax = (PlayState.currentDifficulty == 2 ? 6 : 1) * ((PlayState.currentCharacter == "Sluggy" || PlayState.currentCharacter == "Leechy") ? 10 : 1);
            if (PlayState.currentDifficulty == 2)
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
            anim.Add("Boss_shellbreaker_normal1");
            anim.Add("Boss_shellbreaker_normal2");
            anim.Add("Boss_shellbreaker_normal3");
            anim.Add("Boss_shellbreaker_fire1");
            anim.Add("Boss_shellbreaker_fire2");
            anim.Add("Boss_shellbreaker_fire3");
            eyeAnim.Add("Boss_shellbreaker_eyes_normal1");
            eyeAnim.Add("Boss_shellbreaker_eyes_normal2");
            eyeAnim.Add("Boss_shellbreaker_eyes_normal3");
            eyeAnim.Add("Boss_shellbreaker_eyes_fire1");
            eyeAnim.Add("Boss_shellbreaker_eyes_fire2");
            eyeAnim.Add("Boss_shellbreaker_eyes_fire3");
            foreach(AnimationModule handAnim in handAnims)
            {
                handAnim.Add("Boss_shellbreaker_hand_1");
                handAnim.Add("Boss_shellbreaker_hand_2");
                handAnim.Add("Boss_shellbreaker_hand_3");
            }

            PlayAnim("normal");
        }
        else
            Destroy(gameObject);
    }

    void Update()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        lifeState = health <= maxHealth * 0.3333f ? 3 : (health <= maxHealth * 0.6667f ? 2 : 1);
        shotMax = lifeState == 3 ? 41 : (lifeState == 2 ? 14 : 5);
        turboMultiplier = lifeState == 3 ? 0.1f : (lifeState == 2 ? 0.28f : 0.6f);
        if (anim.currentAnimName[anim.currentAnimName.Length - 1].ToString() != lifeState.ToString())
        {
            PlayAnim(isFiring ? "fire" : "normal");
            if (isFiring)
                stateTimeout = shotDelay * turboMultiplier;
        }

        lifetime += Time.deltaTime * (PlayState.currentDifficulty == 2 ? 1.2f : 1);
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
                if (PlayState.ShootEnemyBullet(transform.position, EnemyBullet.BulletType.boomBlue, angle, weaponSpeed))
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
        PlayState.QueueAchievementPopup("fo4");
        base.Kill();
    }
}
