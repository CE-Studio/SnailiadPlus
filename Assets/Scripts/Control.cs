using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

    public static readonly KeyCode[] defaultKeyboardInputs = new KeyCode[]
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

    public enum ControllerBinds
    {
        LStickU,
        LStickD,
        LStickL,
        LStickR,
        LStickClick,
        RStickU,
        RStickD,
        RStickL,
        RStickR,
        RStickClick,
        FaceU,
        FaceD,
        FaceL,
        FaceR,
        DPadU,
        DPadD,
        DPadL,
        DPadR,
        LBumper,
        LTrigger,
        RBumper,
        RTrigger,
        Start,
        Select
    }

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

    public static ControllerBinds[] controllerInputs = new ControllerBinds[] {};

    public static readonly ControllerBinds[] defaultControllerInputs = new ControllerBinds[]
    {
        ControllerBinds.LStickL,
        ControllerBinds.LStickR,
        ControllerBinds.LStickU,
        ControllerBinds.LStickD,
        ControllerBinds.FaceD,
        ControllerBinds.FaceL,
        ControllerBinds.FaceR,
        ControllerBinds.FaceU,
        ControllerBinds.RStickL,
        ControllerBinds.RStickR,
        ControllerBinds.RStickU,
        ControllerBinds.RStickD,
        ControllerBinds.RTrigger,
        ControllerBinds.FaceL,
        ControllerBinds.LTrigger,
        ControllerBinds.FaceU,
        ControllerBinds.DPadL,
        ControllerBinds.DPadU,
        ControllerBinds.DPadR,
        ControllerBinds.RBumper,
        ControllerBinds.LBumper,
        ControllerBinds.Select,
        ControllerBinds.Start,
        ControllerBinds.FaceR
    };

    public static int[] conFrames = new int[defaultControllerInputs.Length];

    public static bool[] virtualKey = new bool[defaultKeyboardInputs.Length];
    public static bool[] virtualCon = new bool[defaultControllerInputs.Length];

    public static bool[] conPressed = new bool[defaultControllerInputs.Length];

    public const float STICK_DEADZONE = 0.375f;

    public static bool lastInputIsCon = false;

    public static ControllerInput conInput;

    public static int AxisX(int player = 0, bool overrideParalyze = false, bool ignoreVirtual = false)
    {
        return 0 + (RightHold(player, overrideParalyze, ignoreVirtual) ? 1 : 0) - (LeftHold(player, overrideParalyze, ignoreVirtual) ? 1 : 0);
    }
    public static int AxisY(int player = 0, bool overrideParalyze = false, bool ignoreVirtual = false)
    {
        return 0 + (UpHold(player, overrideParalyze, ignoreVirtual) ? 1 : 0) - (DownHold(player, overrideParalyze, ignoreVirtual) ? 1 : 0);
    }

    public static Vector2 Aim(bool overrideParalyze = false, bool ignoreVirtual = false)
    {
        return new Vector2(0 + (RightAim(overrideParalyze, ignoreVirtual) ? 1 : 0) - (LeftAim(overrideParalyze, ignoreVirtual) ? 1 : 0),
            0 + (UpAim(overrideParalyze, ignoreVirtual) ? 1 : 0) - (DownAim(overrideParalyze, ignoreVirtual) ? 1 : 0));
    }

    public static bool LeftPress(int player = 0, bool overrideParalyze = false, bool ignoreVirtual = false)
    {
        if (CheckButton(Controller.Left, true, overrideParalyze, ignoreVirtual))
            return true;
        return player switch
        {
            1 => CheckKey(Keyboard.Left1, true, overrideParalyze, ignoreVirtual),
            2 => CheckKey(Keyboard.Left2, true, overrideParalyze, ignoreVirtual),
            _ => CheckKey(Keyboard.Left1, true, overrideParalyze, ignoreVirtual) || CheckKey(Keyboard.Left2, true, overrideParalyze, ignoreVirtual)
        };
    }

    public static bool LeftHold(int player = 0, bool overrideParalyze = false, bool ignoreVirtual = false)
    {
        if (CheckButton(Controller.Left, false, overrideParalyze, ignoreVirtual))
            return true;
        return player switch
        {
            1 => CheckKey(Keyboard.Left1, false, overrideParalyze, ignoreVirtual),
            2 => CheckKey(Keyboard.Left2, false, overrideParalyze, ignoreVirtual),
            _ => CheckKey(Keyboard.Left1, false, overrideParalyze, ignoreVirtual) || CheckKey(Keyboard.Left2, overrideParalyze, ignoreVirtual)
        };
    }

    public static bool LeftAim(bool overrideParalyze = false, bool ignoreVirtual = false)
    {
        return CheckButton(Controller.AimL, false, overrideParalyze, ignoreVirtual);
    }

    public static bool RightPress(int player = 0, bool overrideParalyze = false, bool ignoreVirtual = false)
    {
        if (CheckButton(Controller.Right, true, overrideParalyze, ignoreVirtual))
            return true;
        return player switch
        {
            1 => CheckKey(Keyboard.Right1, true, overrideParalyze, ignoreVirtual),
            2 => CheckKey(Keyboard.Right2, true, overrideParalyze, ignoreVirtual),
            _ => CheckKey(Keyboard.Right1, true, overrideParalyze, ignoreVirtual) || CheckKey(Keyboard.Right2, true, overrideParalyze, ignoreVirtual)
        };
    }

    public static bool RightHold(int player = 0, bool overrideParalyze = false, bool ignoreVirtual = false)
    {
        if (CheckButton(Controller.Right, false, overrideParalyze, ignoreVirtual))
            return true;
        return player switch
        {
            1 => CheckKey(Keyboard.Right1, false, overrideParalyze, ignoreVirtual),
            2 => CheckKey(Keyboard.Right2, false, overrideParalyze, ignoreVirtual),
            _ => CheckKey(Keyboard.Right1, false, overrideParalyze, ignoreVirtual) || CheckKey(Keyboard.Right2, overrideParalyze, ignoreVirtual)
        };
    }

    public static bool RightAim(bool overrideParalyze = false, bool ignoreVirtual = false)
    {
        return CheckButton(Controller.AimR, false, overrideParalyze, ignoreVirtual);
    }

    public static bool UpPress(int player = 0, bool overrideParalyze = false, bool ignoreVirtual = false)
    {
        if (CheckButton(Controller.Up, true, overrideParalyze, ignoreVirtual))
            return true;
        return player switch
        {
            1 => CheckKey(Keyboard.Up1, true, overrideParalyze, ignoreVirtual),
            2 => CheckKey(Keyboard.Up2, true, overrideParalyze, ignoreVirtual),
            _ => CheckKey(Keyboard.Up1, true, overrideParalyze, ignoreVirtual) || CheckKey(Keyboard.Up2, true, overrideParalyze, ignoreVirtual)
        };
    }

    public static bool UpHold(int player = 0, bool overrideParalyze = false, bool ignoreVirtual = false)
    {
        if (CheckButton(Controller.Up, false, overrideParalyze, ignoreVirtual))
            return true;
        return player switch
        {
            1 => CheckKey(Keyboard.Up1, false, overrideParalyze, ignoreVirtual),
            2 => CheckKey(Keyboard.Up2, false, overrideParalyze, ignoreVirtual),
            _ => CheckKey(Keyboard.Up1, false, overrideParalyze, ignoreVirtual) || CheckKey(Keyboard.Up2, overrideParalyze, ignoreVirtual)
        };
    }

    public static bool UpAim(bool overrideParalyze = false, bool ignoreVirtual = false)
    {
        return CheckButton(Controller.AimU, false, overrideParalyze, ignoreVirtual);
    }

    public static bool DownPress(int player = 0, bool overrideParalyze = false, bool ignoreVirtual = false)
    {
        if (CheckButton(Controller.Down, true, overrideParalyze, ignoreVirtual))
            return true;
        return player switch
        {
            1 => CheckKey(Keyboard.Down1, true, overrideParalyze, ignoreVirtual),
            2 => CheckKey(Keyboard.Down2, true, overrideParalyze, ignoreVirtual),
            _ => CheckKey(Keyboard.Down1, true, overrideParalyze, ignoreVirtual) || CheckKey(Keyboard.Down2, true, overrideParalyze, ignoreVirtual)
        };
    }

    public static bool DownHold(int player = 0, bool overrideParalyze = false, bool ignoreVirtual = false)
    {
        if (CheckButton(Controller.Down, false, overrideParalyze, ignoreVirtual))
            return true;
        return player switch
        {
            1 => CheckKey(Keyboard.Down1, false, overrideParalyze, ignoreVirtual),
            2 => CheckKey(Keyboard.Down2, false, overrideParalyze, ignoreVirtual),
            _ => CheckKey(Keyboard.Down1, false, overrideParalyze, ignoreVirtual) || CheckKey(Keyboard.Down2, overrideParalyze, ignoreVirtual)
        };
    }

    public static bool DownAim(bool overrideParalyze = false, bool ignoreVirtual = false)
    {
        return CheckButton(Controller.AimD, false, overrideParalyze, ignoreVirtual);
    }

    public static bool JumpPress(int player = 0, bool overrideParalyze = false, bool ignoreVirtual = false)
    {
        return player switch
        {
            1 => CheckKey(Keyboard.Jump1, true, overrideParalyze, ignoreVirtual) || CheckButton(Controller.Jump1, true, overrideParalyze, ignoreVirtual),
            2 => CheckKey(Keyboard.Jump2, true, overrideParalyze, ignoreVirtual) || CheckButton(Controller.Jump2, true, overrideParalyze, ignoreVirtual),
            _ => CheckKey(Keyboard.Jump1, true, overrideParalyze, ignoreVirtual) || CheckButton(Controller.Jump1, true, overrideParalyze, ignoreVirtual)
                || CheckKey(Keyboard.Jump2, true, overrideParalyze, ignoreVirtual) || CheckButton(Controller.Jump2, true, overrideParalyze, ignoreVirtual)
        };
    }

    public static bool JumpHold(int player = 0, bool overrideParalyze = false, bool ignoreVirtual = false)
    {
        return player switch
        {
            1 => CheckKey(Keyboard.Jump1, false, overrideParalyze, ignoreVirtual) || CheckButton(Controller.Jump1, false, overrideParalyze, ignoreVirtual),
            2 => CheckKey(Keyboard.Jump2, false, overrideParalyze, ignoreVirtual) || CheckButton(Controller.Jump2, false, overrideParalyze, ignoreVirtual),
            _ => CheckKey(Keyboard.Jump1, false, overrideParalyze, ignoreVirtual) || CheckButton(Controller.Jump1, false, overrideParalyze, ignoreVirtual)
                || CheckKey(Keyboard.Jump2, false, overrideParalyze, ignoreVirtual) || CheckButton(Controller.Jump2, false, overrideParalyze, ignoreVirtual)
        };
    }

    public static bool ShootPress(int player = 0, bool overrideParalyze = false, bool ignoreVirtual = false)
    {
        return player switch
        {
            1 => CheckKey(Keyboard.Shoot1, true, overrideParalyze, ignoreVirtual) || CheckButton(Controller.Shoot1, true, overrideParalyze, ignoreVirtual),
            2 => CheckKey(Keyboard.Shoot2, true, overrideParalyze, ignoreVirtual) || CheckButton(Controller.Shoot2, true, overrideParalyze, ignoreVirtual),
            _ => CheckKey(Keyboard.Shoot1, true, overrideParalyze, ignoreVirtual) || CheckButton(Controller.Shoot1, true, overrideParalyze, ignoreVirtual)
                || CheckKey(Keyboard.Shoot2, true, overrideParalyze, ignoreVirtual) || CheckButton(Controller.Shoot2, true, overrideParalyze, ignoreVirtual)
        };
    }

    public static bool ShootHold(int player = 0, bool overrideParalyze = false, bool ignoreVirtual = false)
    {
        return player switch
        {
            1 => CheckKey(Keyboard.Shoot1, false, overrideParalyze, ignoreVirtual) || CheckButton(Controller.Shoot1, false, overrideParalyze, ignoreVirtual),
            2 => CheckKey(Keyboard.Shoot2, false, overrideParalyze, ignoreVirtual) || CheckButton(Controller.Shoot2, false, overrideParalyze, ignoreVirtual),
            _ => CheckKey(Keyboard.Shoot1, false, overrideParalyze, ignoreVirtual) || CheckButton(Controller.Shoot1, false, overrideParalyze, ignoreVirtual)
                || CheckKey(Keyboard.Shoot2, false, overrideParalyze, ignoreVirtual) || CheckButton(Controller.Shoot2, false, overrideParalyze, ignoreVirtual)
        };
    }

    public static bool StrafePress(int player = 0, bool overrideParalyze = false, bool ignoreVirtual = false)
    {
        return player switch
        {
            1 => CheckKey(Keyboard.Strafe1, true, overrideParalyze, ignoreVirtual) || CheckButton(Controller.Strafe1, true, overrideParalyze, ignoreVirtual),
            2 => CheckKey(Keyboard.Strafe2, true, overrideParalyze, ignoreVirtual) || CheckButton(Controller.Strafe2, true, overrideParalyze, ignoreVirtual),
            _ => CheckKey(Keyboard.Strafe1, true, overrideParalyze, ignoreVirtual) || CheckButton(Controller.Strafe1, true, overrideParalyze, ignoreVirtual)
                || CheckKey(Keyboard.Strafe2, true, overrideParalyze, ignoreVirtual) || CheckButton(Controller.Strafe2, true, overrideParalyze, ignoreVirtual)
        };
    }

    public static bool StrafeHold(int player = 0, bool overrideParalyze = false, bool ignoreVirtual = false)
    {
        return player switch
        {
            1 => CheckKey(Keyboard.Strafe1, false, overrideParalyze, ignoreVirtual) || CheckButton(Controller.Strafe1, false, overrideParalyze, ignoreVirtual),
            2 => CheckKey(Keyboard.Strafe2, false, overrideParalyze, ignoreVirtual) || CheckButton(Controller.Strafe2, false, overrideParalyze, ignoreVirtual),
            _ => CheckKey(Keyboard.Strafe1, false, overrideParalyze, ignoreVirtual) || CheckButton(Controller.Strafe1, false, overrideParalyze, ignoreVirtual)
                || CheckKey(Keyboard.Strafe2, false, overrideParalyze, ignoreVirtual) || CheckButton(Controller.Strafe2, false, overrideParalyze, ignoreVirtual)
        };
    }

    public static bool SpeakPress(int player = 0, bool overrideParalyze = false, bool ignoreVirtual = false)
    {
        return player switch
        {
            1 => CheckKey(Keyboard.Speak1, true, overrideParalyze, ignoreVirtual) || CheckButton(Controller.Speak1, true, overrideParalyze, ignoreVirtual),
            2 => CheckKey(Keyboard.Speak2, true, overrideParalyze, ignoreVirtual) || CheckButton(Controller.Speak2, true, overrideParalyze, ignoreVirtual),
            _ => CheckKey(Keyboard.Speak1, true, overrideParalyze, ignoreVirtual) || CheckButton(Controller.Speak1, true, overrideParalyze, ignoreVirtual)
                || CheckKey(Keyboard.Speak2, true, overrideParalyze, ignoreVirtual) || CheckButton(Controller.Speak2, true, overrideParalyze, ignoreVirtual)
        };
    }

    public static bool SpeakHold(int player = 0, bool overrideParalyze = false, bool ignoreVirtual = false)
    {
        return player switch
        {
            1 => CheckKey(Keyboard.Speak1, false, overrideParalyze, ignoreVirtual) || CheckButton(Controller.Speak1, false, overrideParalyze, ignoreVirtual),
            2 => CheckKey(Keyboard.Speak2, false, overrideParalyze, ignoreVirtual) || CheckButton(Controller.Speak2, false, overrideParalyze, ignoreVirtual),
            _ => CheckKey(Keyboard.Speak1, false, overrideParalyze, ignoreVirtual) || CheckButton(Controller.Speak1, false, overrideParalyze, ignoreVirtual)
                || CheckKey(Keyboard.Speak2, false, overrideParalyze, ignoreVirtual) || CheckButton(Controller.Speak2, false, overrideParalyze, ignoreVirtual)
        };
    }

    public static bool Weapon1(bool overrideParalyze = false, bool ignoreVirtual = false)
    {
        return CheckKey(Keyboard.Weapon1, true, overrideParalyze, ignoreVirtual) || CheckButton(Controller.Weapon1, true, overrideParalyze, ignoreVirtual);
    }

    public static bool Weapon2(bool overrideParalyze = false, bool ignoreVirtual = false)
    {
        return CheckKey(Keyboard.Weapon2, true, overrideParalyze, ignoreVirtual) || CheckButton(Controller.Weapon2, true, overrideParalyze, ignoreVirtual);
    }

    public static bool Weapon3(bool overrideParalyze = false, bool ignoreVirtual = false)
    {
        return CheckKey(Keyboard.Weapon3, true, overrideParalyze, ignoreVirtual) || CheckButton(Controller.Weapon3, true, overrideParalyze, ignoreVirtual);
    }

    public static bool NextWeapon(bool overrideParalyze = false, bool ignoreVirtual = false)
    {
        return CheckKey(Keyboard.NextWeapon, true, overrideParalyze, ignoreVirtual) || CheckButton(Controller.NextWeapon, true, overrideParalyze, ignoreVirtual);
    }

    public static bool PreviousWeapon(bool overrideParalyze = false, bool ignoreVirtual = false)
    {
        return CheckKey(Keyboard.PrevWeapon, true, overrideParalyze, ignoreVirtual) || CheckButton(Controller.PrevWeapon, true, overrideParalyze, ignoreVirtual);
    }

    public static bool Map(bool overrideParalyze = false, bool ignoreVirtual = false)
    {
        return CheckKey(Keyboard.Map, false, overrideParalyze, ignoreVirtual) || CheckButton(Controller.Map, false, overrideParalyze, ignoreVirtual);
    }

    public static bool Pause(bool overrideParalyze = false, bool ignoreVirtual = false)
    {
        return CheckKey(Keyboard.Pause, true, overrideParalyze, ignoreVirtual) || CheckButton(Controller.Pause, true, overrideParalyze, ignoreVirtual);
    }

    public static bool Generic(KeyCode key, bool overrideParalyze = false)
    {
        if (!overrideParalyze && PlayState.paralyzed)
            return false;
        return Input.GetKeyDown(key);
    }

    public static bool AnyInputDown()
    {
        return Input.anyKey;
    }

    public static void SetVirtual(Keyboard input, bool state)
    {
        virtualKey[(int)input] = state;
    }

    public static void SetVirtual(Controller input, bool state)
    {
        virtualCon[(int)input] = state;
    }

    public static void ClearVirtual(bool clearKey, bool clearCon)
    {
        if (clearKey)
        {
            for (int i = 0; i < virtualKey.Length; i++)
                virtualKey[i] = false;
        }
        if (clearCon)
        {
            for (int i = 0; i < virtualCon.Length; i++)
                virtualCon[i] = false;
        }
    }

    public static bool CheckKey(Keyboard input, bool pressed = false, bool overrideParalyze = false, bool ignoreVirtual = false)
    {
        int index = (int)input;
        bool output = false;
        if (!(PlayState.paralyzed && !overrideParalyze))
            output = (pressed ? Input.GetKeyDown(PlayState.generalData.keyboardInputs[index]) :
                Input.GetKey(PlayState.generalData.keyboardInputs[index])) || (!ignoreVirtual && virtualKey[index]);
        else
            return output;
        if (!ignoreVirtual)
            output = output || virtualKey[(int)input];
        if (output)
            lastInputIsCon = false;
        return output;
    }

    public static bool CheckButton(Controller input, bool pressed = false, bool overrideParalyze = false, bool ignoreVirtual = false)
    {
        if (!PlayState.IsControllerConnected())
            return false;
        int index = (int)input;
        bool output = false;
        if (!(PlayState.paralyzed && !overrideParalyze))
        {
            output = !ignoreVirtual && virtualCon[index];
            if (pressed)
                output = conFrames[index] == 1 || output;
            else
                output = conFrames[index] > 0 || output;
        }
        else if (!ignoreVirtual)
            output = virtualCon[index];
        if (output)
            lastInputIsCon = true;
        return output;
    }

    public static IEnumerator HandleController()
    {
        while (true)
        {
            if (PlayState.IsControllerConnected())
            {
                for (int i = 0; i < controllerInputs.Length; i++)
                {
                    InputAction bind = controllerInputs[i] switch
                    {
                        ControllerBinds.LStickU => conInput.Controller.StickL,
                        ControllerBinds.LStickD => conInput.Controller.StickL,
                        ControllerBinds.LStickL => conInput.Controller.StickL,
                        ControllerBinds.LStickR => conInput.Controller.StickL,
                        ControllerBinds.LStickClick => conInput.Controller.StickLClick,
                        ControllerBinds.RStickU => conInput.Controller.StickR,
                        ControllerBinds.RStickD => conInput.Controller.StickR,
                        ControllerBinds.RStickL => conInput.Controller.StickR,
                        ControllerBinds.RStickR => conInput.Controller.StickR,
                        ControllerBinds.RStickClick => conInput.Controller.StickRClick,
                        ControllerBinds.FaceU => conInput.Controller.FaceU,
                        ControllerBinds.FaceD => conInput.Controller.FaceD,
                        ControllerBinds.FaceL => conInput.Controller.FaceL,
                        ControllerBinds.FaceR => conInput.Controller.FaceR,
                        ControllerBinds.DPadU => conInput.Controller.DPadU,
                        ControllerBinds.DPadD => conInput.Controller.DPadD,
                        ControllerBinds.DPadL => conInput.Controller.DPadL,
                        ControllerBinds.DPadR => conInput.Controller.DPadR,
                        ControllerBinds.LBumper => conInput.Controller.BumperL,
                        ControllerBinds.LTrigger => conInput.Controller.TriggerL,
                        ControllerBinds.RBumper => conInput.Controller.BumperR,
                        ControllerBinds.RTrigger => conInput.Controller.TriggerR,
                        ControllerBinds.Start => conInput.Controller.Start,
                        ControllerBinds.Select => conInput.Controller.Select,
                        _ => conInput.Controller.FaceD,
                    };
                    int thisState = controllerInputs[i] switch
                    {
                        ControllerBinds.LStickU or ControllerBinds.RStickU => bind.ReadValue<Vector2>().y > STICK_DEADZONE ? 1 : -1,
                        ControllerBinds.LStickD or ControllerBinds.RStickD => bind.ReadValue<Vector2>().y < -STICK_DEADZONE ? 1 : -1,
                        ControllerBinds.LStickL or ControllerBinds.RStickL => bind.ReadValue<Vector2>().x < -STICK_DEADZONE ? 1 : -1,
                        ControllerBinds.LStickR or ControllerBinds.RStickR => bind.ReadValue<Vector2>().x > STICK_DEADZONE ? 1 : -1,
                        _ => bind.WasPerformedThisFrame() ? 1 : (bind.WasReleasedThisFrame() ? -1 : 0)
                    };
                    if (conFrames[i] > 0)
                    {
                        if (thisState == -1)
                            conFrames[i] = 0;
                        else
                            conFrames[i]++;
                    }
                    else
                    {
                        if (thisState == 1)
                            conFrames[i]++;
                    }
                }
            }
            yield return new WaitForEndOfFrame();
        }
    }

    public static string ParseKeyName(int keyID, bool shortForm = false)
    {
        return ParseKeyName(PlayState.generalData.keyboardInputs[keyID], shortForm);
    }
    public static string ParseKeyName(Keyboard keyID, bool shortForm = false)
    {
        return ParseKeyName(PlayState.generalData.keyboardInputs[(int)keyID], shortForm);
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
        return ParseButtonName(PlayState.generalData.controllerInputs[buttonID], shortForm);
    }
    public static string ParseButtonName(Controller buttonID, bool shortForm = false)
    {
        return ParseButtonName(PlayState.generalData.controllerInputs[(int)buttonID], shortForm);
    }
    public static string ParseButtonName(ControllerBinds button, bool shortForm = false)
    {
        return button switch
        {
            ControllerBinds.LStickR => shortForm ? "(L)>" : "L stick right",
            ControllerBinds.LStickL => shortForm ? "(L)<" : "L stick left",
            ControllerBinds.LStickD => shortForm ? "(L)V" : "L stick down",
            ControllerBinds.LStickU => shortForm ? "(L)^" : "L stick up",
            ControllerBinds.RStickR => shortForm ? "(R)>" : "R stick right",
            ControllerBinds.RStickL => shortForm ? "(R)<" : "R stick left",
            ControllerBinds.RStickD => shortForm ? "(R)V" : "R stick down",
            ControllerBinds.RStickU => shortForm ? "(R)^" : "R stick up",
            ControllerBinds.FaceD => PlayState.generalData.controllerFaceType switch { 1 => "B", 2 => "X", 3 => "O", _ => "A" },
            ControllerBinds.FaceR => PlayState.generalData.controllerFaceType switch { 1 => "A", 2 => shortForm ? "CIR" : "Circle", 3 => "A", _ => "B" },
            ControllerBinds.FaceL => PlayState.generalData.controllerFaceType switch { 1 => "Y", 2 => shortForm ? "SQR" : "Square", 3 => "U", _ => "X" },
            ControllerBinds.FaceU => PlayState.generalData.controllerFaceType switch { 1 => "X", 2 => shortForm ? "TRI" : "Triangle", 3 => "Y", _ => "Y" },
            ControllerBinds.LBumper => PlayState.generalData.controllerFaceType switch { 1 => "L", _ => "L1" },
            ControllerBinds.RBumper => PlayState.generalData.controllerFaceType switch { 1 => "R", _ => "R1" },
            ControllerBinds.LTrigger => PlayState.generalData.controllerFaceType switch { 1 => "ZL", _ => "L2" },
            ControllerBinds.RTrigger => PlayState.generalData.controllerFaceType switch { 1 => "ZR", _ => "R2" },
            ControllerBinds.Select => PlayState.generalData.controllerFaceType switch { 0 => "View", 1 => "-", _ => shortForm ? "SEL" : "Select" },
            ControllerBinds.Start => PlayState.generalData.controllerFaceType switch { 0 => "Menu", 1 => "+", _ => shortForm ? "ST" : "Start" },
            ControllerBinds.LStickClick => PlayState.generalData.controllerFaceType switch { 1 => shortForm ? "LB" : "L Stick Click", _ => "L3" },
            ControllerBinds.RStickClick => PlayState.generalData.controllerFaceType switch { 1 => shortForm ? "RB" : "R Stick Click", _ => "R3" },
            ControllerBinds.DPadU => shortForm ? "+^" : "+Up",
            ControllerBinds.DPadD => shortForm ? "+V" : "+Down",
            ControllerBinds.DPadL => shortForm ? "+<" : "+Left",
            ControllerBinds.DPadR => shortForm ? "+>" : "+Right",
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
