using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleLetter : MonoBehaviour
{
    public SpriteRenderer sprite;
    public AnimationModule anim;
    public AnimationModule colorAnim;
    public Vector2 localFinalPos;
    public bool readyToAnimate = false;
    public char letter = ' ';
    private float animTimer = -2.5f;

    private const float X_SCALE = 80;

    void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<AnimationModule>();
        colorAnim = transform.GetChild(0).GetComponent<AnimationModule>();
        colorAnim.updateSprite = false;
        colorAnim.Add("Title_letterFlash_intro");
        colorAnim.Add("Title_letterFlash_hold");
        colorAnim.Play("Title_letterFlash_intro");
    }

    void Update()
    {
        if (readyToAnimate)
        {
            if (animTimer < 0)
            {
                transform.localPosition = new Vector2(localFinalPos.x + (-Mathf.Sin(-animTimer * Mathf.PI) * animTimer * X_SCALE) * 0.0625f, localFinalPos.y);
                animTimer += Time.deltaTime;
            }
            else
            {
                transform.localPosition = localFinalPos;
                readyToAnimate = false;
                PlayAnim(true, "Title_letterFlash_hold");
            }
        }
        int currentColorInt = colorAnim.GetCurrentFrameValue();
        string currentColorString = (currentColorInt < 1000 ? "0" : "") + (currentColorInt < 100 ? "0" : "") + (currentColorInt < 10 ? "0" : "") + currentColorInt;
        sprite.color = PlayState.GetColor(currentColorString);
    }

    public void PlayAnim(bool mode, string animName = "")
    {
        if (mode) // Color
        {
            colorAnim.Play(animName);
        }
        else // Letter
        {
            string newAnimName = "Title_letter_" + letter;
            anim.Add(newAnimName);
            anim.Play(newAnimName);
        }
    }

    public void SetLetter(char newLetter)
    {
        letter = newLetter;
        if (letter == ' ')
            sprite.sprite = PlayState.BlankTexture();
        else
        {
            string animName = "Title_letter_" + letter.ToString().ToUpper();
            anim.Add(animName);
            anim.Play(animName);
        }
    }
}
