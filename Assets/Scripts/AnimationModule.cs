using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class AnimationModule : MonoBehaviour
{
    public bool isPlaying = false;
    public string currentAnimName = "";
    public string lastAnimName = "";
    public PlayState.AnimationData currentAnim;
    public Dictionary<string, PlayState.AnimationData> animList = new();
    public List<string> listKeys = new();
    public bool stopAtBlankFrame = false;
    public bool blankOnNonLoopEnd = false;
    public bool updateSprite = true;
    public bool pauseOnMenu = true;
    public bool updateMask = false;
    public bool affectedByGlobalEntityColor = true;
    
    private float animTimer = 0;
    private float timerMax = 0;
    private int currentFrame = 0;
    private float speed = 1;
    private float lastNonZeroSpeed = 1;
    private bool smallBlank = false;

    private SpriteRenderer sprite;
    private List<SpriteMask> masks = new();

    void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (PlayState.gameState != PlayState.GameState.game && PlayState.gameState != PlayState.GameState.credits && pauseOnMenu)
            speed = 0;
        else
        {
            speed = lastNonZeroSpeed;
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
                                if (updateSprite)
                                {
                                    sprite.sprite = currentAnim.frames[currentFrame] == -1 ? PlayState.BlankTexture(smallBlank) :
                                        PlayState.GetSprite(currentAnim.spriteName, currentAnim.frames[currentFrame]);
                                }
                                animTimer += timerMax;
                            }
                            else
                            {
                                if (currentAnim.loop)
                                {
                                    currentFrame = currentAnim.loopStartFrame;
                                    if (updateSprite)
                                    {
                                        sprite.sprite = currentAnim.frames[currentFrame] == -1 ? PlayState.BlankTexture(smallBlank) :
                                            PlayState.GetSprite(currentAnim.spriteName, currentAnim.frames[currentFrame]);
                                    }
                                    animTimer += timerMax;
                                }
                                else
                                    Stop();
                            }
                            if (isPlaying && currentAnim.frames[currentFrame] == -1 && stopAtBlankFrame)
                                Stop();
                        }
                    }
                    else
                        Stop(blankOnNonLoopEnd);
                }
            }
            if (updateMask && masks.Count > 0)
            {
                foreach (SpriteMask mask in masks)
                    mask.sprite = sprite.sprite;
            }

            if (affectedByGlobalEntityColor)
            {
                Color newEntityColor = PlayState.entityColor;
                newEntityColor.a = sprite.color.a;
                sprite.color = newEntityColor;
            }
        }
    }

    public void Add(string animName)
    {
        PlayState.AnimationData newAnim = PlayState.GetAnim(animName);
        if (newAnim.name == "NoAnim")
            Debug.LogWarning("Animation \"" + animName + "\" does not exist! (Did you misspell it?)");
        else
        {
            animList.Add(newAnim.name, newAnim);
            listKeys.Add(newAnim.name);
        }
    }

    public void Play(string animName, bool useSmallBlank)
    {
        Play(animName, 1f, 0, useSmallBlank);
    }
    public void Play(string animName, float newSpeed = 1f, int transposeFrames = 0, bool useSmallBlank = false)
    {
        if (sprite == null)
            sprite = GetComponent<SpriteRenderer>();
        smallBlank = useSmallBlank;
        if (animList.ContainsKey(animName))
        {
            currentAnim = animList[animName];
            currentAnimName = animName;
            lastAnimName = animName;

            if (transposeFrames != 0)
            {
                int[] newFrames = (int[])currentAnim.frames.Clone();
                for (int i = 0; i < newFrames.Length; i++)
                    if (newFrames[i] != -1)
                        newFrames[i] += transposeFrames;
                currentAnim.frames = newFrames;
            }

            timerMax = 1 / currentAnim.framerate;
            animTimer = timerMax;
            currentFrame = currentAnim.randomizeStartFrame ? Random.Range(0, currentAnim.frames.Length) : 0;
            if (updateSprite)
            {
                sprite.sprite = currentAnim.frames[currentFrame] == -1 ? PlayState.BlankTexture(smallBlank) :
                    PlayState.GetSprite(currentAnim.spriteName, currentAnim.frames[currentFrame]);
                if (updateMask && masks.Count > 0)
                {
                    foreach (SpriteMask mask in masks)
                    {
                        mask.sprite = sprite.sprite;
                        mask.transform.localScale = new Vector2(sprite.flipX ? -1 : 1, sprite.flipY ? -1 : 1);
                    }
                }
            }
            speed = newSpeed;
            isPlaying = true;
        }
        else
            Debug.LogWarning("Animation \"" + animName + "\" is not present in this module's animation list.");
    }

    public void AddAndPlay(string animName, bool useSmallBlank)
    {
        AddAndPlay(animName, 1f, 0, useSmallBlank);
    }
    public void AddAndPlay(string animName, float newSpeed = 1f, int transposeFrames = 0, bool useSmallBlank = false)
    {
        Add(animName);
        Play(animName, newSpeed, transposeFrames, useSmallBlank);
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
        {
            sprite.sprite = PlayState.BlankTexture(smallBlank);
            for (int i = 0; i < masks.Count; i++)
                masks[i].sprite = PlayState.BlankTexture(smallBlank);
        }
    }

    public void ResetToStart()
    {
        if (currentAnimName != "")
        {
            currentFrame = 0;
            animTimer = timerMax;
            if (updateSprite)
                sprite.sprite = currentAnim.frames[currentFrame] == -1 ? PlayState.BlankTexture(smallBlank) :
                    PlayState.GetSprite(currentAnim.spriteName, currentAnim.frames[0]);
        }
    }

    public void ReloadList()
    {
        Dictionary<string, PlayState.AnimationData> newDict = new();
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

    public void ClearList()
    {
        Stop(true);
        animList = new();
    }

    public void SetSpeed(float newSpeed = 1)
    {
        speed = newSpeed;
        lastNonZeroSpeed = newSpeed;
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

    public int GetCurrentFrameValue()
    {
        return currentAnim.frames[currentFrame];
    }

    public void SetBlankSize(bool set)
    {
        smallBlank = set;
    }

    public void AddMask(SpriteMask newMask)
    {
        updateMask = true;
        masks.Add(newMask);
    }

    public SpriteRenderer GetSpriteRenderer()
    {
        return sprite;
    }
}
