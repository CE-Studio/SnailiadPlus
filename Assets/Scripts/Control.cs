using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Control
{
    public static KeyCode[] inputs = new KeyCode[]
    {
        KeyCode.LeftArrow,  //  0 - Left 1
        KeyCode.RightArrow, //  1 - Right 1
        KeyCode.UpArrow,    //  2 - Up 1
        KeyCode.DownArrow,  //  3 - Down 1
        KeyCode.Z,          //  4 - Jump 1
        KeyCode.X,          //  5 - Shoot 1
        KeyCode.C,          //  6 - Strafe 1
        KeyCode.C,          //  7 - Speak 1
        KeyCode.A,          //  8 - Left 2
        KeyCode.D,          //  9 - Right 2
        KeyCode.W,          // 10 - Up 2
        KeyCode.S,          // 11 - Down 2
        KeyCode.K,          // 12 - Jump 2
        KeyCode.J,          // 13 - Shoot 2
        KeyCode.H,          // 14 - Strafe 2
        KeyCode.H,          // 15 - Speak 2
        KeyCode.Alpha1,     // 16 - Weapon 1
        KeyCode.Alpha2,     // 17 - Weapon 2
        KeyCode.Alpha3,     // 18 - Weapon 3
        KeyCode.Equals,     // 19 - Next weapon
        KeyCode.Minus,      // 20 - Previous weapon
        KeyCode.M,          // 21 - Map
        KeyCode.Escape      // 22 - Pause
    };

    public static KeyCode[] defaultInputs = new KeyCode[]
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
        KeyCode.Escape
    };

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
        switch (player)
        {
            default:
                return Input.GetKeyDown(inputs[0]) || Input.GetKeyDown(inputs[8]);
            case 1:
                return Input.GetKeyDown(inputs[0]);
            case 2:
                return Input.GetKeyDown(inputs[8]);
        }
    }

    public static bool LeftHold(int player = 0)
    {
        switch (player)
        {
            default:
                return Input.GetKey(inputs[0]) || Input.GetKey(inputs[8]);
            case 1:
                return Input.GetKey(inputs[0]);
            case 2:
                return Input.GetKey(inputs[8]);
        }
    }

    public static bool RightPress(int player = 0)
    {
        switch (player)
        {
            default:
                return Input.GetKeyDown(inputs[1]) || Input.GetKeyDown(inputs[9]);
            case 1:
                return Input.GetKeyDown(inputs[1]);
            case 2:
                return Input.GetKeyDown(inputs[9]);
        }
    }

    public static bool RightHold(int player = 0)
    {
        switch (player)
        {
            default:
                return Input.GetKey(inputs[1]) || Input.GetKey(inputs[9]);
            case 1:
                return Input.GetKey(inputs[1]);
            case 2:
                return Input.GetKey(inputs[9]);
        }
    }

    public static bool UpPress(int player = 0)
    {
        switch (player)
        {
            default:
                return Input.GetKeyDown(inputs[2]) || Input.GetKeyDown(inputs[10]);
            case 1:
                return Input.GetKeyDown(inputs[2]);
            case 2:
                return Input.GetKeyDown(inputs[10]);
        }
    }

    public static bool UpHold(int player = 0)
    {
        switch (player)
        {
            default:
                return Input.GetKey(inputs[2]) || Input.GetKey(inputs[10]);
            case 1:
                return Input.GetKey(inputs[2]);
            case 2:
                return Input.GetKey(inputs[10]);
        }
    }

    public static bool DownPress(int player = 0)
    {
        switch (player)
        {
            default:
                return Input.GetKeyDown(inputs[3]) || Input.GetKeyDown(inputs[11]);
            case 1:
                return Input.GetKeyDown(inputs[3]);
            case 2:
                return Input.GetKeyDown(inputs[11]);
        }
    }

    public static bool DownHold(int player = 0)
    {
        switch (player)
        {
            default:
                return Input.GetKey(inputs[3]) || Input.GetKey(inputs[11]);
            case 1:
                return Input.GetKey(inputs[3]);
            case 2:
                return Input.GetKey(inputs[11]);
        }
    }

    public static bool JumpPress(int player = 0)
    {
        switch (player)
        {
            default:
                return Input.GetKeyDown(inputs[4]) || Input.GetKeyDown(inputs[12]);
            case 1:
                return Input.GetKeyDown(inputs[4]);
            case 2:
                return Input.GetKeyDown(inputs[12]);
        }
    }

    public static bool JumpHold(int player = 0)
    {
        switch (player)
        {
            default:
                return Input.GetKey(inputs[4]) || Input.GetKey(inputs[12]);
            case 1:
                return Input.GetKey(inputs[4]);
            case 2:
                return Input.GetKey(inputs[12]);
        }
    }

    public static bool ShootPress(int player = 0)
    {
        switch (player)
        {
            default:
                return Input.GetKeyDown(inputs[5]) || Input.GetKeyDown(inputs[13]);
            case 1:
                return Input.GetKeyDown(inputs[5]);
            case 2:
                return Input.GetKeyDown(inputs[13]);
        }
    }

    public static bool ShootHold(int player = 0)
    {
        switch (player)
        {
            default:
                return Input.GetKey(inputs[5]) || Input.GetKey(inputs[13]);
            case 1:
                return Input.GetKey(inputs[5]);
            case 2:
                return Input.GetKey(inputs[13]);
        }
    }

    public static bool StrafePress(int player = 0)
    {
        switch (player)
        {
            default:
                return Input.GetKeyDown(inputs[6]) || Input.GetKeyDown(inputs[14]);
            case 1:
                return Input.GetKeyDown(inputs[6]);
            case 2:
                return Input.GetKeyDown(inputs[14]);
        }
    }

    public static bool StrafeHold(int player = 0)
    {
        switch (player)
        {
            default:
                return Input.GetKey(inputs[6]) || Input.GetKey(inputs[14]);
            case 1:
                return Input.GetKey(inputs[6]);
            case 2:
                return Input.GetKey(inputs[14]);
        }
    }

    public static bool SpeakPress(int player = 0)
    {
        switch (player)
        {
            default:
                return Input.GetKeyDown(inputs[7]) || Input.GetKeyDown(inputs[15]);
            case 1:
                return Input.GetKeyDown(inputs[7]);
            case 2:
                return Input.GetKeyDown(inputs[15]);
        }
    }

    public static bool SpeakHold(int player = 0)
    {
        switch (player)
        {
            default:
                return Input.GetKey(inputs[7]) || Input.GetKey(inputs[15]);
            case 1:
                return Input.GetKey(inputs[7]);
            case 2:
                return Input.GetKey(inputs[15]);
        }
    }

    public static bool Weapon1()
    {
        return Input.GetKeyDown(inputs[16]);
    }

    public static bool Weapon2()
    {
        return Input.GetKeyDown(inputs[17]);
    }

    public static bool Weapon3()
    {
        return Input.GetKeyDown(inputs[18]);
    }

    public static bool NextWeapon()
    {
        return Input.GetKeyDown(inputs[19]);
    }

    public static bool PreviousWeapon()
    {
        return Input.GetKeyDown(inputs[20]);
    }

    public static bool Map()
    {
        return Input.GetKeyDown(inputs[21]);
    }

    public static bool Pause()
    {
        return Input.GetKeyDown(inputs[22]);
    }

    public static string ParseKeyName(int keyID)
    {
        return ParseKeyName(inputs[keyID]);
    }

    public static string ParseKeyName(KeyCode key, bool shortForm = false)
    {
        switch (key)
        {
            default:
                return key.ToString();
            case KeyCode.Backspace:
                return shortForm ? "BKSP" : "Backspace";
            case KeyCode.Delete:
                return shortForm ? "DEL" : "Delete";
            case KeyCode.Return:
                return shortForm ? "RTRN" : "Return";
            case KeyCode.Escape:
                return shortForm ? "ESC" : "Escape";
            case KeyCode.Keypad0:
                return shortForm ? "KP0" : "Keypad 0";
            case KeyCode.Keypad1:
                return shortForm ? "KP1" : "Keypad 1";
            case KeyCode.Keypad2:
                return shortForm ? "KP2" : "Keypad 2";
            case KeyCode.Keypad3:
                return shortForm ? "KP3" : "Keypad 3";
            case KeyCode.Keypad4:
                return shortForm ? "KP4" : "Keypad 4";
            case KeyCode.Keypad5:
                return shortForm ? "KP5" : "Keypad 5";
            case KeyCode.Keypad6:
                return shortForm ? "KP6" : "Keypad 6";
            case KeyCode.Keypad7:
                return shortForm ? "KP7" : "Keypad 7";
            case KeyCode.Keypad8:
                return shortForm ? "KP8" : "Keypad 8";
            case KeyCode.Keypad9:
                return shortForm ? "KP9" : "Keypad 9";
            case KeyCode.KeypadPeriod:
                return shortForm ? "KP." : "Keypad .";
            case KeyCode.KeypadDivide:
                return shortForm ? "KP/" : "Keypad /";
            case KeyCode.KeypadMultiply:
                return shortForm ? "KP*" : "Keypad *";
            case KeyCode.KeypadMinus:
                return shortForm ? "KP-" : "Keypad -";
            case KeyCode.KeypadPlus:
                return shortForm ? "KP+" : "Keypad +";
            case KeyCode.KeypadEnter:
                return shortForm ? "KP RTRN" : "Keypad enter";
            case KeyCode.KeypadEquals:
                return shortForm ? "KP=" : "Keypad =";
            case KeyCode.UpArrow:
                return "Up";
            case KeyCode.DownArrow:
                return "Down";
            case KeyCode.RightArrow:
                return "Right";
            case KeyCode.LeftArrow:
                return "Left";
            case KeyCode.PageUp:
                return shortForm ? "PGUP" : "Page up";
            case KeyCode.PageDown:
                return shortForm ? "PGDN" : "Page down";
            case KeyCode.Alpha0:
                return "0";
            case KeyCode.Alpha1:
                return "1";
            case KeyCode.Alpha2:
                return "2";
            case KeyCode.Alpha3:
                return "3";
            case KeyCode.Alpha4:
                return "4";
            case KeyCode.Alpha5:
                return "5";
            case KeyCode.Alpha6:
                return "6";
            case KeyCode.Alpha7:
                return "7";
            case KeyCode.Alpha8:
                return "8";
            case KeyCode.Alpha9:
                return "9";
            case KeyCode.Quote:
                return "\"";
            case KeyCode.Comma:
                return ",";
            case KeyCode.Minus:
                return "-";
            case KeyCode.Period:
                return ".";
            case KeyCode.Slash:
                return "/";
            case KeyCode.Semicolon:
                return ";";
            case KeyCode.Equals:
                return "=";
            case KeyCode.LeftBracket:
                return "[";
            case KeyCode.RightBracket:
                return "]";
            case KeyCode.Backslash:
                return "\\";
            case KeyCode.BackQuote:
                return "`";
            case KeyCode.LeftShift:
                return shortForm ? "LSHFT" : "Left shift";
            case KeyCode.RightShift:
                return shortForm ? "RSHFT" : "Right shift";
            case KeyCode.LeftControl:
                return shortForm ? "LCTRL" : "Left ctrl";
            case KeyCode.RightControl:
                return shortForm ? "RCTRL" : "Right ctrl";
            case KeyCode.LeftAlt:
                return shortForm ? "LALT" : "Left alt";
            case KeyCode.RightAlt:
                return shortForm ? "RALT" : "Right alt";
            case KeyCode.LeftWindows:
                return shortForm ? "LWNDW" : "Left windows";
            case KeyCode.RightWindows:
                return shortForm ? "RWNDW" : "Right windows";
            case KeyCode.LeftCommand:
                return shortForm ? "LCMD" : "Left cmnd";
            case KeyCode.RightCommand:
                return shortForm ? "RCMD" : "Right cmnd";
            case KeyCode.Mouse0:
                return shortForm ? "LMB" : "Left mouse";
            case KeyCode.Mouse1:
                return shortForm ? "RMB" : "Right mouse";
            case KeyCode.Mouse2:
                return shortForm ? "MMB" : "Middle mouse";
        }
    }

    public static string LeftRightTranslation(string input)
    {
        string keyNameProper = "";
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
        switch (keyNameProper)
        {
            case "Shi":
                return keyNameFinal + "Shift";
            case "Con":
                return keyNameFinal + "Ctrl";
            case "Alt":
                return keyNameFinal + "Alt";
            case "Com":
                return keyNameFinal + "Cmnd";
            case "App":
                return keyNameFinal + "Apple";
            case "Win":
                return keyNameFinal + "Wndw";
        }
        return "Unknown key";
    }
}
