using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationModule : MonoBehaviour
{
    public bool isPlaying = false;
    public string currentAnimName = "";
    public PlayState.AnimationData currentAnim;
    public float speed = 1;
    public Dictionary<string, PlayState.AnimationData> animList = new Dictionary<string, PlayState.AnimationData>();
    public bool stopAtBlankFrame = false;
    public bool blankOnNonLoopEnd = false;
    
    private float animTimer = 0;
    private float timerMax = 0;
    private int currentFrame = 0;

    private SpriteRenderer sprite;

    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (isPlaying)
        {
            if (currentAnim.framerate != 0)
            {
                if (!(currentFrame == currentAnim.frames.Length - 1 && !currentAnim.loop))
                {
                    animTimer -= Time.deltaTime * speed;
                    while (animTimer <= 0 && !(currentFrame == currentAnim.frames.Length - 1 && !currentAnim.loop))
                    {
                        currentFrame++;
                        if (currentFrame != currentAnim.frames.Length - 1)
                        {
                            sprite.sprite = currentAnim.frames[currentFrame] == -1 ? PlayState.BlankTexture() :
                                PlayState.GetSprite(currentAnim.spriteName, currentAnim.frames[currentFrame]);
                            animTimer += timerMax;
                        }
                        else
                        {
                            if (currentAnim.loop)
                            {
                                currentFrame = currentAnim.loopStartFrame;
                                sprite.sprite = currentAnim.frames[currentFrame] == -1 ? PlayState.BlankTexture() :
                                    PlayState.GetSprite(currentAnim.spriteName, currentAnim.frames[currentFrame]);
                                animTimer += timerMax;
                            }
                        }
                        if (currentAnim.frames[currentFrame] == -1 && stopAtBlankFrame)
                            Stop();
                    }
                }
                else
                    Stop(blankOnNonLoopEnd);
            }
        }
    }

    public void Add(string animName)
    {
        PlayState.AnimationData newAnim = PlayState.GetAnim(animName);
        if (newAnim.name == "NoAnim")
            Debug.Log("Animation \"" + animName + "\" does not exist! (Did you misspell it, or reference the wrong prefix?)");
        else
            animList.Add(newAnim.name, newAnim);
    }

    public void Play(string animName, float newSpeed = 1)
    {
        if (animList.ContainsKey(animName))
        {
            currentAnim = animList[animName];
            currentAnimName = animName;

            timerMax = 1 / currentAnim.framerate;
            animTimer = timerMax;
            sprite.sprite = PlayState.GetSprite(currentAnim.spriteName, currentAnim.frames[0]);
            currentFrame = 0;
            speed = newSpeed;
            isPlaying = true;
        }
        else
            Debug.Log("Animation \"" + animName + "\" is not present in this module's animation list.");
    }

    public void Pause()
    {
        isPlaying = false;
    }

    public void Resume()
    {
        if (currentAnimName != "")
            isPlaying = true;
    }

    public void Stop(bool setBlank = false)
    {
        isPlaying = false;
        currentAnimName = "";
        if (setBlank)
            sprite.sprite = PlayState.BlankTexture();
    }
}
