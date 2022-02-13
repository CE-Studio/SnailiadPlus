using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleLetter : MonoBehaviour
{
    public Sprite[] letters = new Sprite[26];
    public SpriteRenderer sprite;
    public Animator anim;
    public Vector2 localFinalPos;
    public bool readyToAnimate = false;
    private float animTimer = -2.5f;

    private const float X_SCALE = 80;

    void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
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
                PlayAnim("Normal");
            }
        }
    }

    public void PlayAnim(string animName)
    {
        anim.Play(animName, 0, 0);
    }

    public void SetLetter(char letter)
    {
        switch (char.ToLower(letter))
        {
            case 'a':
                sprite.sprite = letters[0];
                break;
            case 'b':
                sprite.sprite = letters[1];
                break;
            case 'c':
                sprite.sprite = letters[2];
                break;
            case 'd':
                sprite.sprite = letters[3];
                break;
            case 'e':
                sprite.sprite = letters[4];
                break;
            case 'f':
                sprite.sprite = letters[5];
                break;
            case 'g':
                sprite.sprite = letters[6];
                break;
            case 'h':
                sprite.sprite = letters[7];
                break;
            case 'i':
                sprite.sprite = letters[8];
                break;
            case 'j':
                sprite.sprite = letters[9];
                break;
            case 'k':
                sprite.sprite = letters[10];
                break;
            case 'l':
                sprite.sprite = letters[11];
                break;
            case 'm':
                sprite.sprite = letters[12];
                break;
            case 'n':
                sprite.sprite = letters[13];
                break;
            case 'o':
                sprite.sprite = letters[14];
                break;
            case 'p':
                sprite.sprite = letters[15];
                break;
            case 'q':
                sprite.sprite = letters[16];
                break;
            case 'r':
                sprite.sprite = letters[17];
                break;
            case 's':
                sprite.sprite = letters[18];
                break;
            case 't':
                sprite.sprite = letters[19];
                break;
            case 'u':
                sprite.sprite = letters[20];
                break;
            case 'v':
                sprite.sprite = letters[21];
                break;
            case 'w':
                sprite.sprite = letters[22];
                break;
            case 'x':
                sprite.sprite = letters[23];
                break;
            case 'y':
                sprite.sprite = letters[24];
                break;
            case 'z':
                sprite.sprite = letters[25];
                break;
            case ' ':
                sprite.enabled = false;
                break;
        }
    }

    public void SetColor(int colorID)
    {
        switch (colorID)
        {
            case 0:
                sprite.color = new Color32(255, 219, 117, 255);
                break;
            case 1:
                sprite.color = new Color32(88, 216, 88, 255);
                break;
            case 2:
                sprite.color = new Color32(104, 72, 252, 255);
                break;
            case 3:
                sprite.color = new Color32(176, 240, 216, 255);
                break;
            case 4:
                sprite.color = new Color32(200, 192, 192, 255);
                break;
            case 5:
                sprite.color = new Color32(252, 184, 252, 255);
                break;
            case 6:
                sprite.color = new Color32(252, 56, 0, 255);
                break;
            case 7:
                sprite.color = new Color32(156, 120, 252, 255);
                break;
        }
    }
}
