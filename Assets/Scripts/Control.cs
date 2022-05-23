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
        return player switch
        {
            1 => Input.GetKeyDown(inputs[0]),
            2 => Input.GetKeyDown(inputs[8]),
            _ => Input.GetKeyDown(inputs[0]) || Input.GetKeyDown(inputs[8]),
        };
    }

    public static bool LeftHold(int player = 0)
    {
        return player switch
        {
            1 => Input.GetKey(inputs[0]),
            2 => Input.GetKey(inputs[8]),
            _ => Input.GetKey(inputs[0]) || Input.GetKey(inputs[8]),
        };
    }

    public static bool RightPress(int player = 0)
    {
        return player switch
        {
            1 => Input.GetKeyDown(inputs[1]),
            2 => Input.GetKeyDown(inputs[9]),
            _ => Input.GetKeyDown(inputs[1]) || Input.GetKeyDown(inputs[9]),
        };
    }

    public static bool RightHold(int player = 0)
    {
        return player switch
        {
            1 => Input.GetKey(inputs[1]),
            2 => Input.GetKey(inputs[9]),
            _ => Input.GetKey(inputs[1]) || Input.GetKey(inputs[9]),
        };
    }

    public static bool UpPress(int player = 0)
    {
        return player switch
        {
            1 => Input.GetKeyDown(inputs[2]),
            2 => Input.GetKeyDown(inputs[10]),
            _ => Input.GetKeyDown(inputs[2]) || Input.GetKeyDown(inputs[10]),
        };
    }

    public static bool UpHold(int player = 0)
    {
        return player switch
        {
            1 => Input.GetKey(inputs[2]),
            2 => Input.GetKey(inputs[10]),
            _ => Input.GetKey(inputs[2]) || Input.GetKey(inputs[10]),
        };
    }

    public static bool DownPress(int player = 0)
    {
        return player switch
        {
            1 => Input.GetKeyDown(inputs[3]),
            2 => Input.GetKeyDown(inputs[11]),
            _ => Input.GetKeyDown(inputs[3]) || Input.GetKeyDown(inputs[11]),
        };
    }

    public static bool DownHold(int player = 0)
    {
        return player switch
        {
            1 => Input.GetKey(inputs[3]),
            2 => Input.GetKey(inputs[11]),
            _ => Input.GetKey(inputs[3]) || Input.GetKey(inputs[11]),
        };
    }

    public static bool JumpPress(int player = 0)
    {
        return player switch
        {
            1 => Input.GetKeyDown(inputs[4]),
            2 => Input.GetKeyDown(inputs[12]),
            _ => Input.GetKeyDown(inputs[4]) || Input.GetKeyDown(inputs[12]),
        };
    }

    public static bool JumpHold(int player = 0)
    {
        return player switch
        {
            1 => Input.GetKey(inputs[4]),
            2 => Input.GetKey(inputs[12]),
            _ => Input.GetKey(inputs[4]) || Input.GetKey(inputs[12]),
        };
    }

    public static bool ShootPress(int player = 0)
    {
        return player switch
        {
            1 => Input.GetKeyDown(inputs[5]),
            2 => Input.GetKeyDown(inputs[13]),
            _ => Input.GetKeyDown(inputs[5]) || Input.GetKeyDown(inputs[13]),
        };
    }

    public static bool ShootHold(int player = 0)
    {
        return player switch
        {
            1 => Input.GetKey(inputs[5]),
            2 => Input.GetKey(inputs[13]),
            _ => Input.GetKey(inputs[5]) || Input.GetKey(inputs[13]),
        };
    }

    public static bool StrafePress(int player = 0)
    {
        return player switch
        {
            1 => Input.GetKeyDown(inputs[6]),
            2 => Input.GetKeyDown(inputs[14]),
            _ => Input.GetKeyDown(inputs[6]) || Input.GetKeyDown(inputs[14]),
        };
    }

    public static bool StrafeHold(int player = 0)
    {
        return player switch
        {
            1 => Input.GetKey(inputs[6]),
            2 => Input.GetKey(inputs[14]),
            _ => Input.GetKey(inputs[6]) || Input.GetKey(inputs[14]),
        };
    }

    public static bool SpeakPress(int player = 0)
    {
        return player switch
        {
            1 => Input.GetKeyDown(inputs[7]),
            2 => Input.GetKeyDown(inputs[15]),
            _ => Input.GetKeyDown(inputs[7]) || Input.GetKeyDown(inputs[15]),
        };
    }

    public static bool SpeakHold(int player = 0)
    {
        return player switch
        {
            1 => Input.GetKey(inputs[7]),
            2 => Input.GetKey(inputs[15]),
            _ => Input.GetKey(inputs[7]) || Input.GetKey(inputs[15]),
        };
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

    public static bool Generic(KeyCode key)
    {
        return Input.GetKeyDown(key);
    }

    public static string ParseKeyName(int keyID)
    {
        return ParseKeyName(inputs[keyID]);
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
