using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextObject : MonoBehaviour
{
    public Vector2 position;
    private float currentLength = 0;
    public int size = 2;
    private Font font;
    private readonly float[] lineSpacings = new float[] { 0.89f, 1f };
    private float animTimer = 0;
    private bool isConIcon = false;

    private const int FONT_SIZE = 40;
    private const float PIXEL = 0.0625f;
    private const float WAVE_SPEED = 8f;
    private const float WAVE_AMPLITUDE = 0.125f;
    private const float SHAKE_INTENSITY = 0.046875f;
    private const float CONTROL_ICON_X = 0.5625f;
    private const float CONTROL_ICON_Y = -0.6875f;

    public TextMesh thisText;
    private List<TextMesh> childText = new();
    public GameObject textObj;
    public SpriteRenderer controlIcon;

    public enum MoveEffects
    {
        none,
        wave,
        shake
    };
    private MoveEffects currentEffect = MoveEffects.none;

    void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        thisText = GetComponent<TextMesh>();
        Transform iconObj = transform.Find("Control Sprite");
        if (iconObj != null)
        {
            controlIcon = iconObj.GetComponent<SpriteRenderer>();
            controlIcon.enabled = false;
        }
        position = transform.localPosition;
        if (textObj == null)
            textObj = Resources.Load<GameObject>("Objects/Text Object (No Script)");
        font = thisText.font;
        childText.Clear();
        for (int i = 0; i < transform.childCount; i++)
            if (transform.GetChild(i).TryGetComponent(out TextMesh newText))
                childText.Add(newText);
        for (int i = 0; i < lineSpacings.Length; i++)
            if (lineSpacings[i] == thisText.lineSpacing)
                size = i + 1;
        currentLength = GetWidth();
    }

    void Update()
    {
        transform.localPosition = position;

        switch (currentEffect)
        {
            default:
            case MoveEffects.none:
                break;
            case MoveEffects.wave:
                transform.localPosition = new Vector2(position.x, position.y + Mathf.Sin(animTimer * WAVE_SPEED) * WAVE_AMPLITUDE);
                break;
            case MoveEffects.shake:
                transform.localPosition = new Vector2(position.x + Random.Range(-SHAKE_INTENSITY, SHAKE_INTENSITY),
                    position.y + Random.Range(-SHAKE_INTENSITY, SHAKE_INTENSITY));
                break;
        }
        animTimer += Time.deltaTime;

        transform.position = new Vector2(Mathf.Round(transform.position.x * 16) * 0.0625f, Mathf.Round(transform.position.y * 16) * 0.0625f);
        if (thisText.anchor == TextAnchor.UpperCenter && size == 1 && currentLength % 2 == 1)
            transform.position += PIXEL * 0.5f * Vector3.left;
    }

    public void SetText(string text)
    {
        thisText.text = text;
        foreach (TextMesh cText in childText)
            cText.text = text;
        currentLength = GetWidth();
    }

    public void SetSize(int newSize)
    {
        newSize = Mathf.Clamp(newSize, 1, 2);
        size = newSize;
        thisText.fontSize = FONT_SIZE * size;
        thisText.lineSpacing = lineSpacings[newSize - 1];
        for (int i = 0; i < childText.Count; i++)
        {
            childText[i].fontSize = FONT_SIZE * size;
            childText[i].lineSpacing = lineSpacings[newSize - 1];
        }

        currentLength = 0;
        foreach (char thisChar in thisText.text)
        {
            font.RequestCharactersInTexture(thisChar.ToString());
            font.GetCharacterInfo(thisChar, out CharacterInfo info);
            currentLength += info.advance;
        }
        currentLength = Mathf.RoundToInt((float)currentLength / 2f * (float)size);
    }

    public void SetColor(Color color)
    {
        thisText.color = color;
        foreach (TextMesh text in childText)
            text.color = new Color(0, 0, 0, color.a);
        controlIcon.color = color;
    }
    public void SetColor(Color32 color)
    {
        thisText.color = color;
        foreach (TextMesh text in childText)
            text.color = new Color32(0, 0, 0, color.a);
        controlIcon.color = color;
    }

    public void SetAlignment(string alignment)
    {
        switch (alignment)
        {
            default:
            case "left":
                thisText.anchor = TextAnchor.UpperLeft;
                thisText.alignment = TextAlignment.Left;
                foreach (TextMesh text in childText)
                {
                    text.anchor = TextAnchor.UpperLeft;
                    text.alignment = TextAlignment.Left;
                }
                controlIcon.transform.localPosition = new Vector2(CONTROL_ICON_X, CONTROL_ICON_Y);
                break;
            case "center":
                thisText.anchor = TextAnchor.UpperCenter;
                thisText.alignment = TextAlignment.Center;
                foreach (TextMesh text in childText)
                {
                    text.anchor = TextAnchor.UpperCenter;
                    text.alignment = TextAlignment.Center;
                }
                controlIcon.transform.localPosition = new Vector2(0, CONTROL_ICON_Y);
                break;
            case "right":
                thisText.anchor = TextAnchor.UpperRight;
                thisText.alignment = TextAlignment.Right;
                foreach (TextMesh text in childText)
                {
                    text.anchor = TextAnchor.UpperRight;
                    text.alignment = TextAlignment.Right;
                }
                controlIcon.transform.localPosition = new Vector2(-CONTROL_ICON_X, CONTROL_ICON_Y);
                break;
        }
    }

    public void SetMovement(MoveEffects effect)
    {
        currentEffect = effect;
    }

    public void SetIcon(int ID)
    {
        if (ID < 0)
            controlIcon.enabled = false;
        else
        {
            controlIcon.enabled = true;
            controlIcon.sprite = PlayState.GetSprite("UI/ControlIcons", ID);
        }
    }

    public string GetText()
    {
        return thisText.text;
    }

    public float GetWidth(bool convertToUnitFloat = false)
    {
        if (isConIcon)
            return convertToUnitFloat ? 1 : 16;
        if (thisText.text.Length == 0)
            return 0;

        font.RequestCharactersInTexture(thisText.text);
        List<int> lineWidths = new();
        int currentWidth = 0;
        for (int i = 0; i < thisText.text.Length; i++)
        {
            if (thisText.text[i] == '\n')
            {
                i++;
                lineWidths.Add(currentWidth);
                currentWidth = 0;
            }
            else
            {
                font.GetCharacterInfo(thisText.text[i], out CharacterInfo info);
                currentWidth += info.advance;
            }
        }
        if (currentWidth != 0)
            lineWidths.Add(currentWidth);
        int longestWidth = 0;
        for (int i = 0; i < lineWidths.Count; i++)
        {
            if (lineWidths[i] > longestWidth)
                longestWidth = lineWidths[i];
        }
        if (convertToUnitFloat)
        {
            float widthFloat = longestWidth * PlayState.FRAC_32 * size;
            return widthFloat;
        }
        return longestWidth * 0.5f * size;
    }

    public void ClearChildText()
    {
        for (int i = childText.Count - 1; i >= 0; i--)
        {
            GameObject textToDestroy = childText[i].gameObject;
            childText.RemoveAt(i);
            Destroy(textToDestroy);
        }
    }

    public void CreateNewChildText(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject newText = Instantiate(textObj, transform);
            for (int j = newText.transform.childCount - 1; j >= 0; j--)
                Destroy(newText.transform.GetChild(j).gameObject);
            newText.transform.localPosition = Vector2.zero;
            newText.GetComponent<TextObject>().enabled = false;
            TextMesh newMesh = newText.GetComponent<TextMesh>();
            newMesh.color = Color.black;
            newMesh.text = thisText.text;
            newMesh.offsetZ = -1;
            childText.Add(newMesh);
        }
    }

    public void CreateShadow()
    {
        ClearChildText();
        CreateNewChildText(1);
        childText[0].transform.position += new Vector3(PIXEL, -PIXEL, 0);
    }

    public void CreateOutline()
    {
        ClearChildText();
        CreateNewChildText(4);
        childText[0].transform.position += PIXEL * Vector3.up;
        childText[1].transform.position += PIXEL * Vector3.left;
        childText[2].transform.position += PIXEL * Vector3.down;
        childText[3].transform.position += PIXEL * Vector3.right;
    }

    public void CreateBoth()
    {
        ClearChildText();
        CreateNewChildText(5);
        childText[0].transform.position += new Vector3(PIXEL * 2, -PIXEL * 2, 0);
        childText[1].transform.position += PIXEL * Vector3.up;
        childText[2].transform.position += PIXEL * Vector3.left;
        childText[3].transform.position += PIXEL * Vector3.down;
        childText[4].transform.position += PIXEL * Vector3.right;
    }

    public void EditorClearChildText()
    {
        for (int i = childText.Count - 1; i >= 0; i--)
        {
            GameObject textToDestroy = childText[i].gameObject;
            childText.RemoveAt(i);
            DestroyImmediate(textToDestroy);
        }
    }

    public void EditorCreateNewChildText(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject newText = Instantiate(textObj, transform);
            for (int j = newText.transform.childCount - 1; j >= 0; j--)
                DestroyImmediate(newText.transform.GetChild(j).gameObject);
            newText.transform.localPosition = Vector2.zero;
            newText.GetComponent<TextObject>().enabled = false;
            TextMesh newMesh = newText.GetComponent<TextMesh>();
            newMesh.color = Color.black;
            newMesh.text = thisText.text;
            newMesh.offsetZ = -1;
            childText.Add(newMesh);
        }
    }

    public void EditorCreateShadow()
    {
        EditorClearChildText();
        EditorCreateNewChildText(1);
        childText[0].transform.position += new Vector3(PIXEL, -PIXEL, 0);
    }

    public void EditorCreateOutline()
    {
        EditorClearChildText();
        EditorCreateNewChildText(4);
        childText[0].transform.position += PIXEL * Vector3.up;
        childText[1].transform.position += PIXEL * Vector3.left;
        childText[2].transform.position += PIXEL * Vector3.down;
        childText[3].transform.position += PIXEL * Vector3.right;
    }

    public void EditorCreateBoth()
    {
        EditorClearChildText();
        EditorCreateNewChildText(5);
        childText[0].transform.position += new Vector3(PIXEL * 2, -PIXEL * 2, 0);
        childText[1].transform.position += PIXEL * Vector3.up;
        childText[2].transform.position += PIXEL * Vector3.left;
        childText[3].transform.position += PIXEL * Vector3.down;
        childText[4].transform.position += PIXEL * Vector3.right;
    }
}
