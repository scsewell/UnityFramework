using System.Collections.Generic;
using System.Linq;

namespace Framework.InputManagement
{
    /// <summary>
    /// Stores all the keyboard and joystick keys that are relevant to a specific in game command.
    /// </summary>
    public class BufferedButton : BufferedSource<bool>
    {
        public BufferedButton(string displayName, bool canRebind, bool canBeMuted, ISource<bool>[] defaultSources) : base(displayName, canRebind, canBeMuted, defaultSources) { }
        
        /*
         * Returns true if any relevant keys are down this frame.
         */
        public bool IsDown()
        {
            foreach (List<bool> source in GetRelevantInput(false))
            {
                for (int i = 0; i < source.Count; i++)
                {
                    if (source[i])
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /*
         * Returns true if a relevant keyboard or joystick key was pressed since the last FixedUpdate.
         */
        public bool JustDown()
        {
            foreach (List<bool> source in GetRelevantInput(true))
            {
                for (int i = 0; i < source.Count - 1; i++)
                {
                    if (source[i + 1] && !source[i])
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /*
         * Returns true if a relevant keyboard or joystick key was released since the last FixedUpdate.
         */
        public bool JustUp()
        {
            foreach (List<bool> source in GetRelevantInput(true))
            {
                for (int i = 0; i < source.Count - 1; i++)
                {
                    if (!source[i + 1] && source[i])
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /*
         * Returns true if any relevant keys are down this frame.
         */
        public bool VisualIsDown()
        {
            foreach (List<bool> source in GetRelevantInput(false))
            {
                if (source.Count > 0 && source.Last())
                {
                    return true;
                }
            }
            return false;
        }

        /*
         * Returns true if a relevant keyboard or joystick key was pressed this frame.
         */
        public bool VisualJustDown()
        {
            foreach (List<bool> source in GetRelevantInput(true))
            {
                if (source.Count > 1 && source[source.Count - 1] && !source[source.Count - 2])
                {
                    return true;
                }
            }
            return false;
        }

        /*
         * Returns true if a relevant keyboard or joystick key was released this frame.
         */
        public bool VisualJustUp()
        {
            foreach (List<bool> source in GetRelevantInput(true))
            {
                if (source.Count > 1 && !source[source.Count - 1] && source[source.Count - 2])
                {
                    return true;
                }
            }
            return false;
        }
    }
}