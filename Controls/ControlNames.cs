using UnityEngine;
using System.Linq;

namespace Framework.InputManagement
{
    public class ControlNames
    {
        public static string GetName(GamepadButton button)
        {
            switch (button)
            {
                case GamepadButton.A:               return "A";
                case GamepadButton.B:               return "B";
                case GamepadButton.X:               return "X";
                case GamepadButton.Y:               return "Y";
                case GamepadButton.RShoulder:       return "R Bumper";
                case GamepadButton.LShoulder:       return "L Bumper";
                case GamepadButton.Back:            return "Back";
                case GamepadButton.Start:           return "Start";
                case GamepadButton.LStick:          return "L Stick";
                case GamepadButton.RStick:          return "R Stick";

                case GamepadButton.LTrigger:        return "L Trigger";
                case GamepadButton.RTrigger:        return "R Trigger";
                case GamepadButton.DpadUp:          return "Dpad Up";
                case GamepadButton.DpadDown:        return "Dpad Down";
                case GamepadButton.DpadLeft:        return "Dpad Left";
                case GamepadButton.DpadRight:       return "Dpad Right";
                case GamepadButton.LStickUp:        return "L Stick Up";
                case GamepadButton.LStickDown:      return "L Stick Down";
                case GamepadButton.LStickLeft:      return "L Stick Left";
                case GamepadButton.LStickRight:     return "L Stick Right";
                case GamepadButton.RStickUp:        return "R Stick Up";
                case GamepadButton.RStickDown:      return "R Stick Down";
                case GamepadButton.RStickLeft:      return "R Stick Left";
                case GamepadButton.RStickRight:     return "R Stick Right";
            }
            return null;
        }

        public static string GetName(GamepadAxis axis)
        {
            switch (axis)
            {
                case GamepadAxis.LStickX:           return "L Stick Horizontal";
                case GamepadAxis.LStickY:           return "L Stick Vertical";
                case GamepadAxis.RStickX:           return "R Stick Horizontal";
                case GamepadAxis.RStickY:           return "R Stick Vertical";
                case GamepadAxis.DpadX:             return "Dpad Horizontal";
                case GamepadAxis.DpadY:             return "Dpad Vertical";
                case GamepadAxis.Triggers:          return "Triggers";
            }
            return null;
        }

        public static string GetName(MouseAxis.Axis axis)
        {
            switch (axis)
            {
                case MouseAxis.Axis.ScrollWheel:    return "ScrollWheel";
                case MouseAxis.Axis.MouseX:         return "Mouse Horizontal";
                case MouseAxis.Axis.MouseY:         return "Mouse Vertical";
            }
            return null;
        }

        public static string GetName(KeyCode keyCode)
        {
            switch (keyCode)
            {
                case KeyCode.AltGr:         return "Alt Gr";
                case KeyCode.Ampersand:     return "&";
                case KeyCode.Asterisk:      return "*";
                case KeyCode.At:            return "@";
                case KeyCode.BackQuote:     return "`";
                case KeyCode.Backslash:     return "\\";
                case KeyCode.Caret:         return "^";
                case KeyCode.Colon:         return ":";
                case KeyCode.Comma:         return ",";
                case KeyCode.Dollar:        return "$";
                case KeyCode.DoubleQuote:   return "\"";
                case KeyCode.Equals:        return "=";
                case KeyCode.Exclaim:       return "!";
                case KeyCode.Greater:       return ">";
                case KeyCode.Hash:          return "#";
                case KeyCode.LeftAlt:       return "Left Alt";
                case KeyCode.LeftBracket:   return "[";
                case KeyCode.LeftCommand:   return "Left Cmd";
                case KeyCode.LeftControl:   return "Left Ctrl";
                case KeyCode.LeftParen:     return "(";
                case KeyCode.LeftShift:     return "Left Shift";
                case KeyCode.LeftWindows:   return "Left Windows";
                case KeyCode.Less:          return "<";
                case KeyCode.Minus:         return "-";
                case KeyCode.Period:        return ".";
                case KeyCode.Plus:          return "+";
                case KeyCode.Question:      return "?";
                case KeyCode.Quote:         return "'";
                case KeyCode.RightAlt:      return "Right Alt";
                case KeyCode.RightBracket:  return "]";
                case KeyCode.RightCommand:  return "Right Cmd";
                case KeyCode.RightControl:  return "Right Ctrl";
                case KeyCode.RightParen:    return ")";
                case KeyCode.RightShift:    return "Right Shift";
                case KeyCode.RightWindows:  return "Right Windows";
                case KeyCode.Semicolon:     return ";";
                case KeyCode.Slash:         return "/";
                case KeyCode.Underscore:    return "_";
                    
                case KeyCode.UpArrow:       return "Up";
                case KeyCode.DownArrow:     return "Down";
                case KeyCode.LeftArrow:     return "Left";
                case KeyCode.RightArrow:    return "Right";

                case KeyCode.Mouse0:        return "Left Mouse";
                case KeyCode.Mouse1:        return "Right Mouse";
                case KeyCode.Mouse2:        return "Middle Mouse";

                case KeyCode.Alpha0:
                case KeyCode.Alpha1:
                case KeyCode.Alpha2:
                case KeyCode.Alpha3:
                case KeyCode.Alpha4:
                case KeyCode.Alpha5:
                case KeyCode.Alpha6:
                case KeyCode.Alpha7:
                case KeyCode.Alpha8:
                case KeyCode.Alpha9:        return keyCode.ToString().Replace("Alpha","");
                    
                case KeyCode.KeypadDivide:  return "Num /";
                case KeyCode.KeypadEnter:   return "Num Enter";
                case KeyCode.KeypadEquals:  return "Num =";
                case KeyCode.KeypadMinus:   return "Num -";
                case KeyCode.KeypadMultiply:return "Num *";
                case KeyCode.KeypadPeriod:  return "Num .";
                case KeyCode.KeypadPlus:    return "Num +";
                
                case KeyCode.Keypad0:
                case KeyCode.Keypad1:
                case KeyCode.Keypad2:
                case KeyCode.Keypad3:
                case KeyCode.Keypad4:
                case KeyCode.Keypad5:
                case KeyCode.Keypad6:
                case KeyCode.Keypad7:
                case KeyCode.Keypad8:
                case KeyCode.Keypad9:       return keyCode.ToString().Replace("Keypad", "Num ");
            }
            // by default the keycode with spaces inserted before capital letters
            string s = keyCode.ToString();
            return string.Join(string.Empty,
                    s.Select((x, i) => (
                         char.IsUpper(x) && i > 0 &&
                         (char.IsLower(s[i - 1]) || (i < s.Count() - 1 && char.IsLower(s[i + 1])))
                    ) ? " " + x : x.ToString()).ToArray());
        }
    }
}