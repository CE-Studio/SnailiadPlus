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
    public List<string> listKeys = new List<string>();
    public bool stopAtBlankFrame = false;
    public bool blankOnNonLoopEnd = false;
    
    private float animTimer = 0;
    private float timerMax = 0;
    private int currentFrame = 0;
    private bool smallBlank = false;

    private SpriteRenderer sprite;

    void Awake()
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
                        if (currentFrame != currentAnim.frames.Length)
                        {
                            sprite.sprite = currentAnim.frames[currentFrame] == -1 ? PlayState.BlankTexture(smallBlank) :
                                PlayState.GetSprite(currentAnim.spriteName, currentAnim.frames[currentFrame]);
                            animTimer += timerMax;
                        }
                        else
                        {
                            if (currentAnim.loop)
                            {
                                currentFrame = currentAnim.loopStartFrame;
                                sprite.sprite = currentAnim.frames[currentFrame] == -1 ? PlayState.BlankTexture(smallBlank) :
                                    PlayState.GetSprite(currentAnim.spriteName, currentAnim.frames[currentFrame]);
                                animTimer += timerMax;
                            }
                            else
                                Stop();
                        }
                        //Debug.Log(currentFrame + "/" + (currentAnim.frames.Length - 1));
                        if (isPlaying && currentAnim.frames[currentFrame] == -1 && stopAtBlankFrame)
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
            Debug.LogWarning("Animation \"" + animName + "\" does not exist! (Did you misspell it, or reference the wrong prefix?)");
        else
        {
            animList.Add(newAnim.name, newAnim);
            listKeys.Add(newAnim.name);
        }
    }

    public void Play(string animName, bool useSmallBlank)
    {
        Play(animName, 1, useSmallBlank);
    }
    public void Play(string animName, float newSpeed = 1, bool useSmallBlank = false)
    {
        smallBlank = useSmallBlank;
        if (animList.ContainsKey(animName))
        {
            currentAnim = animList[animName];
            currentAnimName = animName;

            timerMax = 1 / currentAnim.framerate;
            animTimer = timerMax;
            currentFrame = currentAnim.randomizeStartFrame ? Random.Range(0, currentAnim.frames.Length) : 0;
            sprite.sprite = currentAnim.frames[currentFrame] == -1 ? PlayState.BlankTexture(smallBlank) :
                PlayState.GetSprite(currentAnim.spriteName, currentAnim.frames[currentFrame]);
            speed = newSpeed;
            isPlaying = true;
        }
        else
            Debug.LogWarning("Animation \"" + animName + "\" is not present in this module's animation list.");
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
            sprite.sprite = PlayState.BlankTexture(smallBlank);
    }

    public void ResetToStart()
    {
        if (currentAnimName != "")
        {
            currentFrame = 0;
            animTimer = timerMax;
            sprite.sprite = PlayState.GetSprite(currentAnimName, currentAnim.frames[0]);
        }
    }

    public void ReloadList()
    {
        Dictionary<string, PlayState.AnimationData> newDict = new Dictionary<string, PlayState.AnimationData>();
        for (int i = 0; i < animList.Count; i++)
        {
            PlayState.AnimationData newAnim = PlayState.GetAnim(listKeys[i]);
            if (newAnim.name != "NoAnim")
                newDict.Add(listKeys[i], PlayState.GetAnim(listKeys[i]));
            else
                newDict.Add(listKeys[i], animList[listKeys[i]]);
        }
        animList = newDict;
    }

    public void SetSpeed(float newSpeed = 1)
    {
        speed = newSpeed;
    }

    public void PrintAllAnims()
    {
        string output = "";
        foreach (PlayState.AnimationData animData in PlayState.animationLibrary)
            output += "" + animData.name + ", ";
        Debug.Log(output);
    }

    public int GetCurrentFrame()
    {
        return currentFrame;
    }

    public void SetBlankSize(bool set)
    {
        smallBlank = set;
    }
}
