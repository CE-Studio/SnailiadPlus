using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Control
{
    public enum Keyboard
    {
        Left1,
        Right1,
        Up1,
        Down1,
        Jump1,
        Shoot1,
        Strafe1,
        Speak1,
        Left2,
        Right2,
        Up2,
        Down2,
        Jump2,
        Shoot2,
        Strafe2,
        Speak2,
        Weapon1,
        Weapon2,
        Weapon3,
        NextWeapon,
        PrevWeapon,
        Map,
        Pause,
        Return
    };

    public static KeyCode[] keyboardInputs = new KeyCode[] { };

    public static KeyCode[] defaultKeyboardInputs = new KeyCode[]
    {
        KeyCode.LeftArrow,
        KeyCode.RightArrow,
        KeyCode.UpArrow,
        KeyCode.DownArrow,
        KeyCode.Z,
        KeyCode.X,
        KeyCode.C,
        KeyCode.C,
        KeyCode.A,
        KeyCode.D,
        KeyCode.W,
        KeyCode.S,
        KeyCode.K,
        KeyCode.J,
        KeyCode.H,
        KeyCode.H,
        KeyCode.Alpha1,
        KeyCode.Alpha2,
        KeyCode.Alpha3,
        KeyCode.Equals,
        KeyCode.Minus,
        KeyCode.M,
        KeyCode.Escape,
        KeyCode.Escape
    };

    public enum Controller
    {
        Left,
        Right,
        Up,
        Down,
        Jump1,
        Shoot1,
        Strafe1,
        Speak1,
        AimL,
        AimR,
        AimU,
        AimD,
        Jump2,
        Shoot2,
        Strafe2,
        Speak2,
        Weapon1,
        Weapon2,
        Weapon3,
        NextWeapon,
        PrevWeapon,
        Map,
        Pause,
        Return
    };

    public static KeyCode[] controllerInputs = new KeyCode[] {};

    public static KeyCode[] defaultControllerInputs = new KeyCode[]
    {
        KeyCode.Keypad0,
        KeyCode.Alpha0,
        KeyCode.Keypad1,
        KeyCode.Alpha1,
        KeyCode.JoystickButton0,
        KeyCode.JoystickButton2,
        KeyCode.JoystickButton7,
        KeyCode.JoystickButton1,
        KeyCode.Keypad2,
        KeyCode.Alpha2,
        KeyCode.Keypad3,
        KeyCode.Alpha3,
        KeyCode.JoystickButton6,
        KeyCode.JoystickButton2,
        KeyCode.JoystickButton7,
        KeyCode.JoystickButton3,
        KeyCode.JoystickButton14,
        KeyCode.JoystickButton12,
        KeyCode.JoystickButton15,
        KeyCode.JoystickButton5,
        KeyCode.JoystickButton4,
        KeyCode.JoystickButton13,
        KeyCode.JoystickButton9,
        KeyCode.JoystickButton8
    };

    public static bool[] virtualKey = new bool[defaultKeyboardInputs.Length];
    public static bool[] virtualCon = new bool[defaultControllerInputs.Length];

    public static bool[] conPressed = new bool[defaultControllerInputs.Length];

    public const float STICK_DEADZONE = 0.25f;

    public static int AxisX(int player = 0)
    {
        return 0 + (RightHold(player) ? 1 : 0) - (LeftHold(player) ? 1 : 0);
    }
    public static int AxisY(int player = 0)
    {
        return 0 + (UpHold(player) ? 1 : 0) - (DownHold(player) ? 1 : 0);
    }

    public static bool LeftPress(int player = 0)
    {
        if (CheckButton(Controller.Left, true))
            return true;
        return player switch
        {
            1 => CheckKey(Keyboard.Left1, true),
            2 => CheckKey(Keyboard.Left2, true),
            _ => CheckKey(Keyboard.Left1, true) || CheckKey(Keyboard.Left2, true)
        };
    }

    public static bool LeftHold(int player = 0)
    {
        if (CheckButton(Controller.Left))
            return true;
        return player switch
        {
            1 => CheckKey(Keyboard.Left1),
            2 => CheckKey(Keyboard.Left2),
            _ => CheckKey(Keyboard.Left1) || CheckKey(Keyboard.Left2)
        };
    }

    public static bool LeftAim()
    {
        return CheckButton(Controller.AimL);
    }

    public static bool RightPress(int player = 0)
    {
        if (CheckButton(Controller.Right, true))
            return true;
        return player switch
        {
            1 => CheckKey(Keyboard.Right1, true),
            2 => CheckKey(Keyboard.Right2, true),
            _ => CheckKey(Keyboard.Right1, true) || CheckKey(Keyboard.Right2, true)
        };
    }

    public static bool RightHold(int player = 0)
    {
        if (CheckButton(Controller.Right))
            return true;
        return player switch
        {
            1 => CheckKey(Keyboard.Right1),
            2 => CheckKey(Keyboard.Right2),
            _ => CheckKey(Keyboard.Right1) || CheckKey(Keyboard.Right2)
        };
    }

    public static bool RightAim()
    {
        return CheckButton(Controller.AimR);
    }

    public static bool UpPress(int player = 0)
    {
        if (CheckButton(Controller.Up, true))
            return true;
        return player switch
        {
            1 => CheckKey(Keyboard.Up1, true),
            2 => CheckKey(Keyboard.Up2, true),
            _ => CheckKey(Keyboard.Up1, true) || CheckKey(Keyboard.Up2, true)
        };
    }

    public static bool UpHold(int player = 0)
    {
        if (CheckButton(Controller.Up))
            return true;
        return player switch
        {
            1 => CheckKey(Keyboard.Up1),
            2 => CheckKey(Keyboard.Up2),
            _ => CheckKey(Keyboard.Up1) || CheckKey(Keyboard.Up2)
        };
    }

    public static bool UpAim()
    {
        return CheckButton(Controller.AimU);
    }

    public static bool DownPress(int player = 0)
    {
        if (CheckButton(Controller.Down, true))
            return true;
        return player switch
        {
            1 => CheckKey(Keyboard.Down1, true),
            2 => CheckKey(Keyboard.Down2, true),
            _ => CheckKey(Keyboard.Down1, true) || CheckKey(Keyboard.Down2, true)
        };
    }

    public static bool DownHold(int player = 0)
    {
        if (CheckButton(Controller.Down))
            return true;
        return player switch
        {
            1 => CheckKey(Keyboard.Down1),
            2 => CheckKey(Keyboard.Down2),
            _ => CheckKey(Keyboard.Down1) || CheckKey(Keyboard.Down2)
        };
    }

    public static bool DownAim()
    {
        return CheckButton(Controller.AimD);
    }

    public static bool JumpPress(int player = 0)
    {
        return player switch
        {
            1 => CheckKey(Keyboard.Jump1, true) || CheckButton(Controller.Jump1, true),
            2 => CheckKey(Keyboard.Jump2, true) || CheckButton(Controller.Jump2, true),
            _ => CheckKey(Keyboard.Jump1, true) || CheckButton(Controller.Jump1, true) || CheckKey(Keyboard.Jump2, true) || CheckButton(Controller.Jump2, true)
        };
    }

    public static bool JumpHold(int player = 0)
    {
        return player switch
        {
            1 => CheckKey(Keyboard.Jump1) || CheckButton(Controller.Jump1),
            2 => CheckKey(Keyboard.Jump2) || CheckButton(Controller.Jump2),
            _ => CheckKey(Keyboard.Jump1) || CheckButton(Controller.Jump1) || CheckKey(Keyboard.Jump2) || CheckButton(Controller.Jump2)
        };
    }

    public static bool ShootPress(int player = 0)
    {
        return player switch
        {
            1 => CheckKey(Keyboard.Shoot1, true) || CheckButton(Controller.Shoot1, true),
            2 => CheckKey(Keyboard.Shoot2, true) || CheckButton(Controller.Shoot2, true),
            _ => CheckKey(Keyboard.Shoot1, true) || CheckButton(Controller.Shoot1, true) || CheckKey(Keyboard.Shoot2, true) || CheckButton(Controller.Shoot2, true)
        };
    }

    public static bool ShootHold(int player = 0)
    {
        return player switch
        {
            1 => CheckKey(Keyboard.Shoot1) || CheckButton(Controller.Shoot1),
            2 => CheckKey(Keyboard.Shoot2) || CheckButton(Controller.Shoot2),
            _ => CheckKey(Keyboard.Shoot1) || CheckButton(Controller.Shoot1) || CheckKey(Keyboard.Shoot2) || CheckButton(Controller.Shoot2)
        };
    }

    public static bool StrafePress(int player = 0)
    {
        return player switch
        {
            1 => CheckKey(Keyboard.Strafe1, true) || CheckButton(Controller.Strafe1, true),
            2 => CheckKey(Keyboard.Strafe2, true) || CheckButton(Controller.Strafe2, true),
            _ => CheckKey(Keyboard.Strafe1, true) || CheckButton(Controller.Strafe1, true) || CheckKey(Keyboard.Strafe2, true) || CheckButton(Controller.Strafe2, true)
        };
    }

    public static bool StrafeHold(int player = 0)
    {
        return player switch
        {
            1 => CheckKey(Keyboard.Strafe1) || CheckButton(Controller.Strafe1),
            2 => CheckKey(Keyboard.Strafe2) || CheckButton(Controller.Strafe2),
            _ => CheckKey(Keyboard.Strafe1) || CheckButton(Controller.Strafe1) || CheckKey(Keyboard.Strafe2) || CheckButton(Controller.Strafe2)
        };
    }

    public static bool SpeakPress(int player = 0)
    {
        return player switch
        {
            1 => CheckKey(Keyboard.Speak1, true) || CheckButton(Controller.Speak1, true),
            2 => CheckKey(Keyboard.Speak2, true) || CheckButton(Controller.Speak2, true),
            _ => CheckKey(Keyboard.Speak1, true) || CheckButton(Controller.Speak1, true) || CheckKey(Keyboard.Speak2, true) || CheckButton(Controller.Speak2, true)
        };
    }

    public static bool SpeakHold(int player = 0)
    {
        return player switch
        {
            1 => CheckKey(Keyboard.Speak1) || CheckButton(Controller.Speak1),
            2 => CheckKey(Keyboard.Speak2) || CheckButton(Controller.Speak2),
            _ => CheckKey(Keyboard.Speak1) || CheckButton(Controller.Speak1) || CheckKey(Keyboard.Speak2) || CheckButton(Controller.Speak2)
        };
    }

    public static bool Weapon1()
    {
        return CheckKey(Keyboard.Weapon1, true) || CheckButton(Controller.Weapon1, true);
    }

    public static bool Weapon2()
    {
        return CheckKey(Keyboard.Weapon2, true) || CheckButton(Controller.Weapon2, true);
    }

    public static bool Weapon3()
    {
        return CheckKey(Keyboard.Weapon3, true) || CheckButton(Controller.Weapon3, true);
    }

    public static bool NextWeapon()
    {
        return CheckKey(Keyboard.NextWeapon, true) || CheckButton(Controller.NextWeapon, true);
    }

    public static bool PreviousWeapon()
    {
        return CheckKey(Keyboard.PrevWeapon, true) || CheckButton(Controller.PrevWeapon, true);
    }

    public static bool Map()
    {
        return CheckKey(Keyboard.Map) || CheckButton(Controller.Map);
    }

    public static bool Pause()
    {
        return CheckKey(Keyboard.Pause, true) || CheckButton(Controller.Pause, true);
    }

    public static bool Generic(KeyCode key)
    {
        return Input.GetKeyDown(key);
    }

    public static bool CheckKey(Keyboard input, bool pressed = false)
    {
        int index = (int)input;
        return (pressed ? Input.GetKeyDown(keyboardInputs[index]) : Input.GetKey(keyboardInputs[index])) || virtualKey[index];
    }

    public static bool CheckButton(Controller input, bool pressed = false)
    {
        if (!PlayState.IsControllerConnected())
            return false;
        int index = (int)input;
        string inputName = controllerInputs[index].ToString();
        bool inputDown;
        if (inputName.Contains("Alpha") || inputName.Contains("Keypad"))
        {
            char ID = inputName[inputName.Length - 1];
            bool positive = inputName.Contains("Alpha");
            float stickValue = ID switch {
                '0' => Input.GetAxis("LStickX"),
                '1' => Input.GetAxis("LStickY"),
                '2' => Input.GetAxis("RStickX"),
                _ => Input.GetAxis("RStickY")
            };
            inputDown = (positive ? (stickValue > STICK_DEADZONE) : (stickValue < -STICK_DEADZONE)) || virtualCon[index];
        }
        else
            inputDown = Input.GetKey(controllerInputs[index]);
        if (inputDown)
        {
            if (conPressed[index] && !pressed)
                return true;
            else if (!conPressed[index])
            {
                conPressed[index] = true;
                return true;
            }
            else
                return virtualCon[index];
        }
        conPressed[index] = false;
        return virtualCon[index];
    }

    public static string ParseKeyName(int keyID, bool shortForm = false)
    {
        return ParseKeyName(keyboardInputs[keyID], shortForm);
    }
    public static string ParseKeyName(Keyboard keyID, bool shortForm = false)
    {
        return ParseKeyName(keyboardInputs[(int)keyID], shortForm);
    }
    public static string ParseKeyName(KeyCode key, bool shortForm = false)
    {
        return key switch
        {
            KeyCode.Backspace => shortForm ? "BKSP" : "Backspace",
            KeyCode.Delete => shortForm ? "DEL" : "Delete",
            KeyCode.Return => shortForm ? "RTRN" : "Return",
            KeyCode.Escape => shortForm ? "ESC" : "Escape",
            KeyCode.Keypad0 => shortForm ? "KP0" : "Keypad 0",
            KeyCode.Keypad1 => shortForm ? "KP1" : "Keypad 1",
            KeyCode.Keypad2 => shortForm ? "KP2" : "Keypad 2",
            KeyCode.Keypad3 => shortForm ? "KP3" : "Keypad 3",
            KeyCode.Keypad4 => shortForm ? "KP4" : "Keypad 4",
            KeyCode.Keypad5 => shortForm ? "KP5" : "Keypad 5",
            KeyCode.Keypad6 => shortForm ? "KP6" : "Keypad 6",
            KeyCode.Keypad7 => shortForm ? "KP7" : "Keypad 7",
            KeyCode.Keypad8 => shortForm ? "KP8" : "Keypad 8",
            KeyCode.Keypad9 => shortForm ? "KP9" : "Keypad 9",
            KeyCode.KeypadPeriod => shortForm ? "KP." : "Keypad .",
            KeyCode.KeypadDivide => shortForm ? "KP/" : "Keypad /",
            KeyCode.KeypadMultiply => shortForm ? "KP*" : "Keypad *",
            KeyCode.KeypadMinus => shortForm ? "KP-" : "Keypad -",
            KeyCode.KeypadPlus => shortForm ? "KP+" : "Keypad +",
            KeyCode.KeypadEnter => shortForm ? "KP RTRN" : "Keypad enter",
            KeyCode.KeypadEquals => shortForm ? "KP=" : "Keypad =",
            KeyCode.UpArrow => "Up",
            KeyCode.DownArrow => "Down",
            KeyCode.RightArrow => "Right",
            KeyCode.LeftArrow => "Left",
            KeyCode.PageUp => shortForm ? "PGUP" : "Page up",
            KeyCode.PageDown => shortForm ? "PGDN" : "Page down",
            KeyCode.Alpha0 => "0",
            KeyCode.Alpha1 => "1",
            KeyCode.Alpha2 => "2",
            KeyCode.Alpha3 => "3",
            KeyCode.Alpha4 => "4",
            KeyCode.Alpha5 => "5",
            KeyCode.Alpha6 => "6",
            KeyCode.Alpha7 => "7",
            KeyCode.Alpha8 => "8",
            KeyCode.Alpha9 => "9",
            KeyCode.Quote => "\"",
            KeyCode.Comma => ",",
            KeyCode.Minus => "-",
            KeyCode.Period => ".",
            KeyCode.Slash => "/",
            KeyCode.Semicolon => ";",
            KeyCode.Equals => "=",
            KeyCode.LeftBracket => "[",
            KeyCode.RightBracket => "]",
            KeyCode.Backslash => "\\",
            KeyCode.BackQuote => "`",
            KeyCode.LeftShift => shortForm ? "LSHFT" : "Left shift",
            KeyCode.RightShift => shortForm ? "RSHFT" : "Right shift",
            KeyCode.LeftControl => shortForm ? "LCTRL" : "Left ctrl",
            KeyCode.RightControl => shortForm ? "RCTRL" : "Right ctrl",
            KeyCode.LeftAlt => shortForm ? "LALT" : "Left alt",
            KeyCode.RightAlt => shortForm ? "RALT" : "Right alt",
            KeyCode.LeftWindows => shortForm ? "LWNDW" : "Left windows",
            KeyCode.RightWindows => shortForm ? "RWNDW" : "Right windows",
            KeyCode.LeftCommand => shortForm ? "LCMD" : "Left cmnd",
            KeyCode.RightCommand => shortForm ? "RCMD" : "Right cmnd",
            KeyCode.Mouse0 => shortForm ? "LMB" : "Left mouse",
            KeyCode.Mouse1 => shortForm ? "RMB" : "Right mouse",
            KeyCode.Mouse2 => shortForm ? "MMB" : "Middle mouse",
            _ => key.ToString(),
        };
    }

    public static string ParseButtonName(int buttonID, bool shortForm = false)
    {
        return ParseButtonName(controllerInputs[buttonID], shortForm);
    }
    public static string ParseButtonName(Controller buttonID, bool shortForm = false)
    {
        return ParseButtonName(controllerInputs[(int)buttonID], shortForm);
    }
    public static string ParseButtonName(KeyCode button, bool shortForm = false)
    {
        return button switch
        {
            KeyCode.Alpha0 => shortForm ? "L +x" : "L stick right",
            KeyCode.Keypad0 => shortForm ? "L -x" : "L stick left",
            KeyCode.Alpha1 => shortForm ? "L +y" : "L stick down",
            KeyCode.Keypad1 => shortForm ? "L -y" : "L stick up",
            KeyCode.Alpha2 => shortForm ? "R +x" : "R stick right",
            KeyCode.Keypad2 => shortForm ? "R -x" : "R stick left",
            KeyCode.Alpha3 => shortForm ? "R +y" : "R stick down",
            KeyCode.Keypad3 => shortForm ? "R -y" : "R stick up",
            KeyCode.JoystickButton0 => PlayState.gameOptions[17] switch { 1 => "B", 2 => "X", 3 => "O", _ => "A" },
            KeyCode.JoystickButton1 => PlayState.gameOptions[17] switch { 1 => "A", 2 => (shortForm ? "CIR" : "Circle"), 3 => "A", _ => "B" },
            KeyCode.JoystickButton2 => PlayState.gameOptions[17] switch { 1 => "Y", 2 => (shortForm ? "SQR" : "Square"), 3 => "U", _ => "X" },
            KeyCode.JoystickButton3 => PlayState.gameOptions[17] switch { 1 => "X", 2 => (shortForm ? "TRI" : "Triangle"), 3 => "Y", _ => "Y" },
            KeyCode.JoystickButton4 => PlayState.gameOptions[17] switch { 1 => "L", _ => "L1" },
            KeyCode.JoystickButton5 => PlayState.gameOptions[17] switch { 1 => "R", _ => "R1" },
            KeyCode.JoystickButton6 => PlayState.gameOptions[17] switch { 1 => "ZL", _ => "L2" },
            KeyCode.JoystickButton7 => PlayState.gameOptions[17] switch { 1 => "ZR", _ => "R2" },
            KeyCode.JoystickButton8 => PlayState.gameOptions[17] switch { 0 => "View", 1 => "-", _ => (shortForm ? "SEL" : "Select") },
            KeyCode.JoystickButton9 => PlayState.gameOptions[17] switch { 0 => "Menu", 1 => "+", _ => (shortForm ? "ST" : "Start") },
            KeyCode.JoystickButton10 => PlayState.gameOptions[17] switch { 1 => shortForm ? "LB" : "L Stick Click", _ => "L3" },
            KeyCode.JoystickButton11 => PlayState.gameOptions[17] switch { 1 => shortForm ? "RB" : "R Stick Click", _ => "R3" },
            KeyCode.JoystickButton12 => "Up",
            KeyCode.JoystickButton13 => "Down",
            KeyCode.JoystickButton14 => "Left",
            KeyCode.JoystickButton15 => "Right",
            KeyCode.JoystickButton16 => "Home",
            _ => button.ToString()
        };
    }

    public static string LeftRightTranslation(string input)
    {
        string keyNameProper;
        string keyNameFinal = "";
        char firstChar = input[0];
        if (firstChar == 'L')
        {
            keyNameFinal += "Left ";
            keyNameProper = input.Substring(4, 6);
        }
        else
        {
            keyNameFinal += "Right ";
            keyNameProper = input.Substring(5, 7);
        }
        return keyNameProper switch
        {
            "Shi" => keyNameFinal + "Shift",
            "Con" => keyNameFinal + "Ctrl",
            "Alt" => keyNameFinal + "Alt",
            "Com" => keyNameFinal + "Cmnd",
            "App" => keyNameFinal + "Apple",
            "Win" => keyNameFinal + "Wndw",
            _ => "Unknown key",
        };
    }
}
