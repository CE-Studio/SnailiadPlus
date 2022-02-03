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

    public static int AxisX()
    {
        return 0 + (RightHold() ? 1 : 0) - (LeftHold() ? 1 : 0);
    }
    public static int AxisY()
    {
        return 0 + (UpHold() ? 1 : 0) - (DownHold() ? 1 : 0);
    }

    public static bool LeftPress()
    {
        return Input.GetKeyDown(inputs[0]) || Input.GetKeyDown(inputs[8]);
    }

    public static bool LeftHold()
    {
        return Input.GetKey(inputs[0]) || Input.GetKey(inputs[8]);
    }

    public static bool RightPress()
    {
        return Input.GetKeyDown(inputs[1]) || Input.GetKeyDown(inputs[9]);
    }

    public static bool RightHold()
    {
        return Input.GetKey(inputs[1]) || Input.GetKey(inputs[9]);
    }

    public static bool UpPress()
    {
        return Input.GetKeyDown(inputs[2]) || Input.GetKeyDown(inputs[10]);
    }

    public static bool UpHold()
    {
        return Input.GetKey(inputs[2]) || Input.GetKey(inputs[10]);
    }

    public static bool DownPress()
    {
        return Input.GetKeyDown(inputs[3]) || Input.GetKeyDown(inputs[11]);
    }

    public static bool DownHold()
    {
        return Input.GetKey(inputs[3]) || Input.GetKey(inputs[11]);
    }

    public static bool JumpPress()
    {
        return Input.GetKeyDown(inputs[4]) || Input.GetKeyDown(inputs[12]);
    }

    public static bool JumpHold()
    {
        return Input.GetKey(inputs[4]) || Input.GetKey(inputs[12]);
    }

    public static bool ShootPress()
    {
        return Input.GetKeyDown(inputs[5]) || Input.GetKeyDown(inputs[13]);
    }

    public static bool ShootHold()
    {
        return Input.GetKey(inputs[5]) || Input.GetKey(inputs[13]);
    }

    public static bool StrafePress()
    {
        return Input.GetKeyDown(inputs[6]) || Input.GetKeyDown(inputs[14]);
    }

    public static bool StrafeHold()
    {
        return Input.GetKey(inputs[6]) || Input.GetKey(inputs[14]);
    }

    public static bool SpeakPress()
    {
        return Input.GetKeyDown(inputs[7]) || Input.GetKeyDown(inputs[15]);
    }

    public static bool SpeakHold()
    {
        return Input.GetKey(inputs[7]) || Input.GetKey(inputs[15]);
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

    public static string ParseKeyName(KeyCode key)
    {
        switch (key)
        {
            default:
                return key.ToString();
            case KeyCode.Keypad0:
                return "Keypad 0";
            case KeyCode.Keypad1:
                return "Keypad 1";
            case KeyCode.Keypad2:
                return "Keypad 2";
            case KeyCode.Keypad3:
                return "Keypad 3";
            case KeyCode.Keypad4:
                return "Keypad 4";
            case KeyCode.Keypad5:
                return "Keypad 5";
            case KeyCode.Keypad6:
                return "Keypad 6";
            case KeyCode.Keypad7:
                return "Keypad 7";
            case KeyCode.Keypad8:
                return "Keypad 8";
            case KeyCode.Keypad9:
                return "Keypad 9";
            case KeyCode.KeypadPeriod:
                return "Keypad .";
            case KeyCode.KeypadDivide:
                return "Keypad /";
            case KeyCode.KeypadMultiply:
                return "Keypad *";
            case KeyCode.KeypadMinus:
                return "Keypad -";
            case KeyCode.KeypadPlus:
                return "Keypad +";
            case KeyCode.KeypadEnter:
                return "Keypad enter";
            case KeyCode.KeypadEquals:
                return "Keypad =";
            case KeyCode.UpArrow:
                return "Up";
            case KeyCode.DownArrow:
                return "Down";
            case KeyCode.RightArrow:
                return "Right";
            case KeyCode.LeftArrow:
                return "Left";
            case KeyCode.PageUp:
                return "Page up";
            case KeyCode.PageDown:
                return "Page down";
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
                return "Left shift";
            case KeyCode.RightShift:
                return "Right shift";
            case KeyCode.LeftControl:
                return "Left ctrl";
            case KeyCode.RightControl:
                return "Right ctrl";
            case KeyCode.LeftAlt:
                return "Left alt";
            case KeyCode.RightAlt:
                return "Right alt";
            case KeyCode.LeftWindows:
                return "Left windows";
            case KeyCode.RightWindows:
                return "Right windows";
            case KeyCode.LeftCommand:
                return "Left cmnd";
            case KeyCode.RightCommand:
                return "Right cmnd";
            case KeyCode.Mouse0:
                return "Left mouse";
            case KeyCode.Mouse1:
                return "Right mouse";
            case KeyCode.Mouse2:
                return "Middle mouse";
        }
    }

    //public static string ParseKeyName(KeyCode key)
    //{
    //    string keyName = key.ToString();
    //    switch (key)
    //    {
    //        default:
    //            switch (keyName.Substring(0, 3))
    //            {
    //                case "Keyp":
    //                    if (char.IsDigit(keyName[keyName.Length - 1]))
    //                        return "Keypad " + keyName[keyName.Length - 1];
    //                    else
    //                    {
    //                        switch (keyName.Substring(6, 7))
    //                        {
    //                            case "Pe":
    //                                return "Keypad .";
    //                            case "Di":
    //                                return "Keypad /";
    //                            case "Mu":
    //                                return "Keypad *";
    //                            case "Mi":
    //                                return "Keypad -";
    //                            case "Pl":
    //                                return "Keypad +";
    //                            case "En":
    //                                return "Keypad enter";
    //                            case "Eq":
    //                                return "Keypad =";
    //                        }
    //                    }
    //                    break;
    //                case "Alph":
    //                    return keyName[keyName.Length - 1].ToString();
    //                case "Left":
    //                case "Righ":
    //                    return LeftRightTranslation(keyName);
    //            }
    //            break;
    //        case KeyCode.UpArrow:
    //            return "Up";
    //        case KeyCode.DownArrow:
    //            return "Down";
    //        case KeyCode.LeftArrow:
    //            return "Left";
    //        case KeyCode.RightArrow:
    //            return "Right";
    //        case KeyCode.PageUp:
    //            return "Page up";
    //        case KeyCode.PageDown:
    //            return "Page down";
    //        case KeyCode.Equals:
    //            return "=";
    //        case KeyCode.Minus:
    //            return "-";
    //        case KeyCode.Comma:
    //            return ",";
    //        case KeyCode.Period:
    //            return ".";
    //        case KeyCode.Slash:
    //            return "/";
    //        case KeyCode.Quote:
    //            return "\'";
    //        case KeyCode.Semicolon:
    //            return ";";
    //        case KeyCode.LeftBracket:
    //            return "[";
    //        case KeyCode.RightBracket:
    //            return "]";
    //        case KeyCode.Backslash:
    //            return "\\";
    //        case KeyCode.BackQuote:
    //            return "`";
    //        case KeyCode.Mouse0:
    //            return "Left mouse";
    //        case KeyCode.Mouse1:
    //            return "Right mouse";
    //        case KeyCode.Mouse2:
    //            return "Middle mouse";
    //    }
    //    return keyName;
    //}

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
