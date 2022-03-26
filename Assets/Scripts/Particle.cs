using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle : MonoBehaviour
{
    public bool isActive = false;
    //public Animator anim;
    public AnimationModule anim;
    public SpriteRenderer sprite;
    public string type = "";
    public float[] vars = new float[] { 0, 0, 0, 0, 0 };
    private float[] internalVars = new float[] { 0, 0, 0, 0, 0 };
    public ParticleSpriteCollection sprites;

    private PlayState.AnimationController animCon;

    public void Start()
    {
        //anim = GetComponent<Animator>();
        anim = GetComponent<AnimationModule>();
        sprite = GetComponent<SpriteRenderer>();
        //anim.enabled = false;
        anim.blankOnNonLoopEnd = true;
        gameObject.SetActive(false);

        anim.Add("Bubble");
        anim.Add("Explosion_1");
        anim.Add("Explosion_2");
        anim.Add("Explosion_3");
        anim.Add("Explosion_4");
        anim.Add("Explosion_5");
        anim.Add("Explosion_6");
        anim.Add("Explosion_7");
        anim.Add("Explosion_8");
        anim.Add("Nom");
        anim.Add("Splash");
    }

    public void Update()
    {
        if (PlayState.gameState == "Game")
        {
            //anim.speed = 1;
            if (!anim.isPlaying)
                anim.Resume();
            if (gameObject.activeSelf)
            {
                switch (type)
                {
                    default:
                        break;
                    case "bubble":
                        if (vars[4] == 1)
                        {
                            if (internalVars[0] == 0 && internalVars[1] == 0)
                            {
                                internalVars[0] = Random.Range(-4f, 4f);
                                internalVars[1] = Random.Range(-12f, -3f) * Mathf.Abs(PlayState.playerScript.velocity.y);
                            }
                            transform.position = new Vector2(transform.position.x, transform.position.y + (internalVars[1] * Time.deltaTime));
                            vars[1] += internalVars[0] * Time.deltaTime;
                            internalVars[0] = Mathf.Lerp(internalVars[0], 0, 2f * Time.deltaTime);
                            internalVars[1] = Mathf.Lerp(internalVars[1], 0, 2f * Time.deltaTime);
                        }
                        vars[0] += Time.deltaTime;
                        transform.position = new Vector2(vars[1] + 2 * Mathf.Sin(vars[0] / 1.2f) * 0.0625f, transform.position.y + vars[3] * Time.deltaTime * 0.25f);
                        if (transform.position.y > vars[2])
                            ResetParticle();
                        break;
                }

                //if (animCon.currentAnim.framerate != 0)
                //{
                //    if (!(animCon.currentFrame == animCon.currentAnim.frames.Length - 1 && !animCon.currentAnim.loop))
                //    {
                //        animCon.animTimer -= Time.deltaTime;
                //        while (animCon.animTimer <= 0 && !(animCon.currentFrame == animCon.currentAnim.frames.Length - 1 && !animCon.currentAnim.loop))
                //        {
                //            animCon.currentFrame++;
                //            if (animCon.currentFrame != animCon.currentAnim.frames.Length - 1)
                //            {
                //                sprite.sprite = PlayState.GetSprite(animCon.animSpriteName, animCon.currentFrame);
                //                animCon.animTimer += animCon.timerResetVal;
                //            }
                //            else
                //            {
                //                if (animCon.currentAnim.loop)
                //                {
                //                    animCon.currentFrame = animCon.currentAnim.loopStartFrame;
                //                    sprite.sprite = PlayState.GetSprite(animCon.animSpriteName, animCon.currentFrame);
                //                    animCon.animTimer += animCon.timerResetVal;
                //                }
                //            }
                //        }
                //    }
                //    else
                //        ResetParticle();
                //}
                if (!anim.isPlaying && isActive && !(type == "bubble"))
                    ResetParticle();
            }
        }
        else
        {
            if (anim.isPlaying)
                anim.Pause();
        }
            //anim.speed = 0;
    }

    public void SetAnim(string animType)
    {
        isActive = true;
        switch (animType)
        {
            default:
                PlayState.AnimationData nullAnim = new PlayState.AnimationData();
                nullAnim.name = "NoAnim";
                nullAnim.framerate = 0;
                nullAnim.frames = new int[] { -1 };
                nullAnim.loop = false;
                nullAnim.loopStartFrame = 0;

                animCon.currentAnim = nullAnim;
                break;
            case "bubble":
                //animCon.currentAnim = PlayState.GetAnim("Bubble");
                //sprite.sprite = PlayState.GetSprite("Particles/Bubble", Random.Range(0, animCon.currentAnim.frames.Length));
                sprite.sprite = PlayState.GetSprite("Particles/Bubble", Random.Range(0, PlayState.GetAnim("Bubble").frames.Length));
                break;
            case "explosion":
                //animCon.currentAnim = PlayState.GetAnim("Explosion_" + vars[0]);
                //animCon.animSpriteName = "Particles/Explosion";
                anim.Play("Explosion_" + vars[0].ToString());
                break;
            case "nom":
                //animCon.currentAnim = PlayState.GetAnim("Nom");
                anim.Play("Nom");
                break;
            case "splash":
                //animCon.currentAnim = PlayState.GetAnim("Splash");
                //animCon.animSpriteName = "Particles/Splash";
                anim.Play("Splash");
                break;
        }
        if (animCon.currentAnim.framerate != 0)
        {
            animCon.currentFrame = 0;
            animCon.timerResetVal = animCon.currentAnim.name == "NoAnim" ? 0 : 1 / animCon.currentAnim.framerate;
            animCon.animTimer = animCon.timerResetVal;

            if (animCon.currentFrame != -1)
                sprite.sprite = PlayState.GetSprite(animCon.animSpriteName, 0);
            else
                sprite.sprite = PlayState.BlankTexture();
        }
    }

    public void PlaySound()
    {
        switch (type)
        {
            default:
                break;
            case "explosion":
                PlayState.PlaySound("Explode" + Random.Range(1, 5));
                break;
            case "splash":
                PlayState.PlaySound("Splash");
                break;
        }
    }

    public void ResetParticle()
    {
        isActive = false;
        transform.position = Vector2.zero;
        anim.Stop(true);
        sprite.sprite = sprites.blank;
        sprite.flipX = false;
        sprite.flipY = false;
        for (int i = 0; i < vars.Length; i++)
            vars[i] = 0;
        for (int i = 0; i < internalVars.Length; i++)
            internalVars[i] = 0;
        gameObject.SetActive(false);
    }
}
