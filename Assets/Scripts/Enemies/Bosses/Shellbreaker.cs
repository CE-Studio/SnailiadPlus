using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shellbreaker : Boss
{
    private GameObject eyes;
    private AnimationModule eyeAnim;

    private float lifetime = 0;
    private int lifeState = 1;

    private const int HAND_COUNT = 3;
    public GameObject hand;
    private List<GameObject> hands;
    private List<AnimationModule> handAnims;
    private float handTheta;

    void Awake()
    {
        if (PlayState.IsBossAlive(0))
        {
            SpawnBoss(450, 2, 3, true, new Vector2(2.95f, 2.95f), 0);
            StartCoroutine(RunIntro());
            eyes = transform.GetChild(0).gameObject;
            PlayState.player.transform.position = new Vector2(PlayState.player.transform.position.x + (2.25f * (PlayState.playerScript.facingLeft ? -1 : 1)),
                PlayState.player.transform.position.y);
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

            PlayAnim("normal");
        }
        else
            Destroy(gameObject);
    }

    void Update()
    {
        if (PlayState.gameState != "Game")
            return;

        lifeState = health <= maxHealth * 0.3333f ? 3 : (health <= maxHealth * 0.6667f ? 2 : 1);
        if (anim.currentAnimName[anim.currentAnimName.Length - 1].ToString() != lifeState.ToString())
            PlayAnim("normal");

        lifetime += Time.deltaTime;
        float modifier = Mathf.Sin(3f / 7f * lifetime);
        transform.localPosition = new Vector2(origin.x + 9 * Mathf.Cos(lifetime) * modifier, origin.y - 7 * Mathf.Sin(lifetime) * modifier);

        eyes.transform.localPosition = (PlayState.player.transform.position - transform.position).normalized * 0.125f;
    }

    private void PlayAnim(string newAnim)
    {
        anim.Play("Boss_shellbreaker_" + newAnim + lifeState);
        eyeAnim.Play("Boss_shellbreaker_eyes_" + newAnim + lifeState);
    }
}
