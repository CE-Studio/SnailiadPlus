using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlPopup : MonoBehaviour
{
    private readonly List<KeyCode> keyboardIDs = new()
    {
        KeyCode.None, KeyCode.A, KeyCode.B, KeyCode.C, KeyCode.D, KeyCode.E, KeyCode.F, KeyCode.G, KeyCode.H, KeyCode.I, KeyCode.J, KeyCode.K, KeyCode.L,
        KeyCode.M, KeyCode.N, KeyCode.O, KeyCode.P, KeyCode.Q, KeyCode.R, KeyCode.S, KeyCode.T, KeyCode.U, KeyCode.V, KeyCode.W, KeyCode.X, KeyCode.Y,
        KeyCode.Z, KeyCode.BackQuote, KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7,
        KeyCode.Alpha8, KeyCode.Alpha9, KeyCode.Alpha0, KeyCode.Minus, KeyCode.Equals, KeyCode.Backspace, KeyCode.Tab, KeyCode.LeftBracket,
        KeyCode.RightBracket, KeyCode.Semicolon, KeyCode.Quote, KeyCode.Backslash, KeyCode.Slash, KeyCode.Comma, KeyCode.Period, KeyCode.DoubleQuote,
        KeyCode.Colon, KeyCode.LeftControl, KeyCode.LeftCommand, KeyCode.LeftWindows, KeyCode.LeftAlt, KeyCode.LeftShift, KeyCode.Return, KeyCode.Space,
        KeyCode.UpArrow, KeyCode.LeftArrow, KeyCode.DownArrow, KeyCode.RightArrow, KeyCode.Escape, KeyCode.F1, KeyCode.F2, KeyCode.F3, KeyCode.F4,
        KeyCode.F5, KeyCode.F6, KeyCode.F7, KeyCode.F8, KeyCode.F9, KeyCode.F10, KeyCode.F11, KeyCode.F12, KeyCode.Keypad0, KeyCode.Keypad1,
        KeyCode.Keypad2, KeyCode.Keypad3, KeyCode.Keypad4, KeyCode.Keypad5, KeyCode.Keypad6, KeyCode.Keypad7, KeyCode.Keypad8, KeyCode.Keypad9,
        KeyCode.KeypadPlus, KeyCode.KeypadMinus, KeyCode.KeypadMultiply, KeyCode.KeypadDivide, KeyCode.KeypadEnter, KeyCode.Delete
    };
    private readonly List<Control.Keyboard> keyboardControlIDs = new()
    {
        Control.Keyboard.Jump1, Control.Keyboard.Shoot1, Control.Keyboard.Up1, Control.Keyboard.Left1, Control.Keyboard.Down1, Control.Keyboard.Right1,
        Control.Keyboard.Speak1, Control.Keyboard.Jump2, Control.Keyboard.Shoot2, Control.Keyboard.Up2, Control.Keyboard.Left2, Control.Keyboard.Down2,
        Control.Keyboard.Right2, Control.Keyboard.Speak2
    };
    private readonly List<Control.Controller> controllerControlIDs = new()
    {
        Control.Controller.Jump1, Control.Controller.Shoot1, Control.Controller.Up, Control.Controller.Left, Control.Controller.Down,
        Control.Controller.Right, Control.Controller.Speak1, Control.Controller.Jump2, Control.Controller.Shoot2, Control.Controller.AimU,
        Control.Controller.AimL, Control.Controller.AimD, Control.Controller.AimR, Control.Controller.Speak2
    };

    private Sprite[] controlSprites;

    private SpriteRenderer sprite;
    private AnimationModule anim;
    private Transform keyParent;
    private List<SpriteRenderer> keyControls = new();
    private Transform conParent;
    private List<SpriteRenderer> conControls = new();

    private const float ACTIVE_TIME = 11f;
    private const float SCROLL_SPEED = 4f;

    private float activeTime = 0;
    private Vector2 origin;
    private Vector2 activeTarget = new(0, -5.5f);
    
    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<AnimationModule>();
        keyParent = GameObject.Find("Keyboard").transform;
        conParent = GameObject.Find("Controller").transform;
        for (int i = 0; i < keyParent.childCount; i++)
            keyControls.Add(keyParent.GetChild(i).GetComponent<SpriteRenderer>());
        for (int i = 0; i < conParent.childCount; i++)
            conControls.Add(conParent.GetChild(i).GetComponent<SpriteRenderer>());
        anim.Add("Controls_keyboard_base");
        anim.Add("Controls_keyboard_weapon");
        anim.Add("Controls_controller_base");
        anim.Add("Controls_controller_weapon");
        origin = transform.localPosition;
        controlSprites = Resources.LoadAll<Sprite>("Images/UI/ControlIcons");
    }

    void Update()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        if (activeTime > 0)
        {
            activeTime -= Time.deltaTime;
            transform.localPosition = Vector2.Lerp(transform.localPosition, activeTarget, SCROLL_SPEED * Time.deltaTime);
        }
        else
            transform.localPosition = Vector2.Lerp(transform.localPosition, origin, SCROLL_SPEED * Time.deltaTime);
    }

    public void RunPopup(bool showWeapon, bool isController)
    {
        if (isController)
        {
            anim.Play("Controls_controller_" + (showWeapon ? "weapon" : "base"));
            keyParent.gameObject.SetActive(false);
            conParent.gameObject.SetActive(true);
            for (int i = 0; i < conControls.Count; i++)
            {
                if (!showWeapon && new List<int> { 1, 8, 9, 10, 11, 12 }.Contains(i))
                    conControls[i].enabled = false;
                else
                {
                    conControls[i].enabled = true;
                    Control.Controller thisInput = controllerControlIDs[i];
                    int spriteID = Control.GetButtonSpriteIcon(PlayState.generalData.controllerInputs[(int)thisInput]);
                    conControls[i].sprite = controlSprites[spriteID];
                }
            }
        }
        else
        {
            anim.Play("Controls_keyboard_" + (showWeapon ? "weapon" : "base"));
            keyParent.gameObject.SetActive(true);
            conParent.gameObject.SetActive(false);
            for (int i = 0; i < keyControls.Count; i++)
            {
                if (!showWeapon && (i == 1 || i == 8))
                    keyControls[i].enabled = false;
                else
                {
                    keyControls[i].enabled = true;
                    Control.Keyboard thisInput = keyboardControlIDs[i];
                    int spriteID = Control.GetKeySpriteIcon(PlayState.generalData.keyboardInputs[(int)thisInput]);
                    keyControls[i].sprite = controlSprites[spriteID];
                }
            }
        }
        activeTime = ACTIVE_TIME;
    }
}
