using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleLetter : MonoBehaviour
{
    public SpriteRenderer sprite;
    public AnimationModule anim;
    public AnimationModule colorAnim;
    public Vector2 localFinalPos;
    public bool intro = true;
    public char letter = ' ';
    private float animTimer = -2.5f;
    private float startOffset = 0;

    private const float X_SCALE = 80;

    private readonly List<char> acceptedLetters = new List<char>
    {
        'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '+'
    };

    void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<AnimationModule>();
        anim.pauseOnMenu = false;
        colorAnim = transform.GetChild(0).GetComponent<AnimationModule>();
        colorAnim.updateSprite = false;
        colorAnim.pauseOnMenu = false;
        colorAnim.Add("Title_letterFlash_intro");
        colorAnim.Add("Title_letterFlash_hold");
    }

    public void Create(char newLetter, Vector2 finalPos, float animOffset)
    {
        SetLetter(newLetter);
        localFinalPos = finalPos;
        startOffset = animOffset;
    }

    void Update()
    {
        if (startOffset == 0 && letter != '+' && intro)
        {
            if (!colorAnim.isPlaying)
            {
                sprite.enabled = true;
                colorAnim.Play("Title_letterFlash_intro");
            }
            if (animTimer < 0)
            {
                transform.localPosition = new Vector2(localFinalPos.x + (-Mathf.Sin(-animTimer * Mathf.PI) * animTimer * X_SCALE) * 0.0625f, localFinalPos.y);
                animTimer += Time.deltaTime;
            }
            else
            {
                transform.localPosition = localFinalPos;
                intro = false;
                colorAnim.Play("Title_letterFlash_hold");
            }
        }
        else if (startOffset == 0 && letter == '+')
        {
            if (!anim.isPlaying)
            {
                sprite.enabled = true;
                anim.Play("Title_plus");
            }
            transform.localPosition = localFinalPos;
        }
        else if (startOffset != 0)
        {
            sprite.enabled = false;
            startOffset = Mathf.Clamp(startOffset - Time.deltaTime, 0, Mathf.Infinity);
        }

        if (colorAnim.isPlaying)
        {
            int currentColorInt = colorAnim.GetCurrentFrameValue();
            string currentColorString = (currentColorInt < 1000 ? "0" : "") + (currentColorInt < 100 ? "0" : "") + (currentColorInt < 10 ? "0" : "") + currentColorInt;
            sprite.color = PlayState.GetColor(currentColorString);
        }

        if (!(PlayState.gameState == PlayState.GameState.menu || PlayState.gameState == PlayState.GameState.pause))
            Destroy(gameObject);
    }

    public void SetLetter(char newLetter)
    {
        letter = newLetter.ToString().ToLower().ToCharArray()[0];
        if (!acceptedLetters.Contains(letter))
            sprite.sprite = PlayState.BlankTexture();
        else
        {
            string animName;
            if (letter == '+')
                animName = "Title_plus";
            else
                animName = "Title_letter_" + letter.ToString().ToUpper();
            anim.Add(animName);
            if (animName != "Title_plus")
                anim.Play(animName);
        }
    }
}
