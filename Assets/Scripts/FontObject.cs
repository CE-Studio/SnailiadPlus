using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FontObject : MonoBehaviour
{
    public SpriteRenderer sprite;
    public Texture2D palette;
    public int size;
    public int ID;
    public string animType = "None";
    public float animTimer = 0;
    public Vector2 originPos;
    
    void Update()
    {
        if (animType != "None")
        {
            switch (animType)
            {
                case "Wave":
                    transform.localPosition = new Vector2(originPos.x, originPos.y + Mathf.Sin(animTimer * 8) * 0.125f);
                    break;
                case "Shake":
                    transform.localPosition = new Vector2(originPos.x + Random.Range(-0.046875f, 0.046875f), originPos.y + Random.Range(-0.046875f, 0.046875f));
                    break;
            }
            animTimer += Time.deltaTime;
        }
    }

    public void Create(char character)
    {
        Create(character, 0, "None", new Vector2(3, 12));
    }
    public void Create(char character, int type)
    {
        Create(character, type, "None", new Vector2(3, 12));
    }
    public void Create(char character, Vector2 colorID)
    {
        Create(character, 0, "None", colorID);
    }
    public void Create(char character, int type, Vector2 colorID)
    {
        Create(character, type, "None", colorID);
    }
    public void Create(char character, string newAnimType)
    {
        Create(character, 0, newAnimType, new Vector2(3, 12));
    }
    public void Create(char character, int type, string newAnimType)
    {
        Create(character, type, newAnimType, new Vector2(3, 12));
    }
    public void Create(char character, int type, string newAnimType, Vector2 colorID)
    {
        sprite = GetComponent<SpriteRenderer>();
        SetChar(character, type);
        sprite.color = palette.GetPixel((int)colorID.x, (int)colorID.y);
        animType = newAnimType;
        animTimer = 0;
        originPos = transform.localPosition;
    }

    public void SetSize(float size)
    {
        transform.localScale = new Vector2(size, size);
    }

    public void SetChar(char character, int type)
    {
        if (character == ' ')
        {
            sprite.sprite = PlayState.BlankTexture();
            ID = 94;
        }
        else
        {
            var newID = character switch
            {
                '!' => 0,
                '\"' => 1,
                '#' => 2,
                '$' => 3,
                '%' => 4,
                '¢' => 5,
                '\'' => 6,
                '(' => 7,
                ')' => 8,
                '*' => 9,
                '+' => 10,
                ',' => 11,
                '-' => 12,
                '.' => 13,
                '/' => 14,
                '0' => 15,
                '1' => 16,
                '2' => 17,
                '3' => 18,
                '4' => 19,
                '5' => 20,
                '6' => 21,
                '7' => 22,
                '8' => 23,
                '9' => 24,
                ':' => 25,
                ';' => 26,
                '<' => 27,
                '=' => 28,
                '>' => 29,
                '@' => 31,
                'A' => 32,
                'B' => 33,
                'C' => 34,
                'D' => 35,
                'E' => 36,
                'F' => 37,
                'G' => 38,
                'H' => 39,
                'I' => 40,
                'J' => 41,
                'K' => 42,
                'L' => 43,
                'M' => 44,
                'N' => 45,
                'O' => 46,
                'P' => 47,
                'Q' => 48,
                'R' => 49,
                'S' => 50,
                'T' => 51,
                'U' => 52,
                'V' => 53,
                'W' => 54,
                'X' => 55,
                'Y' => 56,
                'Z' => 57,
                '[' => 58,
                '\\' => 59,
                ']' => 60,
                '^' => 61,
                '_' => 62,
                '`' => 63,
                'a' => 64,
                'b' => 65,
                'c' => 66,
                'd' => 67,
                'e' => 68,
                'f' => 69,
                'g' => 70,
                'h' => 71,
                'i' => 72,
                'j' => 73,
                'k' => 74,
                'l' => 75,
                'm' => 76,
                'n' => 77,
                'o' => 78,
                'p' => 79,
                'q' => 80,
                'r' => 81,
                's' => 82,
                't' => 83,
                'u' => 84,
                'v' => 85,
                'w' => 86,
                'x' => 87,
                'y' => 88,
                'z' => 89,
                '{' => 90,
                '|' => 91,
                '}' => 92,
                '~' => 93,
                _ => 30,
            };
            ID = newID;
            newID += 94 * type;
            sprite.sprite = PlayState.GetSprite("UI/FontSprites", newID);
        }
    }
}
