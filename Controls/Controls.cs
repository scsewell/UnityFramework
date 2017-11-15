using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Framework.SettingManagement;

namespace Framework.InputManagement
{
    /// <summary>
    /// Stores and maintains game controls.
    /// </summary>
    public class Controls
    {
        private List<BufferedButton> m_buttons;
        private List<BufferedAxis> m_axes;

        private Dictionary<string, BufferedButton> m_nameToButton;
        public Dictionary<string, BufferedButton> NameToButton
        {
            get { return m_nameToButton; }
        }

        private Dictionary<string, BufferedAxis> m_nameToAxis;
        public Dictionary<string, BufferedAxis> NameToAxis
        {
            get { return m_nameToAxis; }
        }

        private Settings m_settings;
        public Settings Settings
        {
            get { return m_settings; }
        }

        private bool m_isMuted = false;
        public bool IsMuted
        {
            get { return m_isMuted; }
            set
            {
                if (m_isMuted != value)
                {
                    m_isMuted = value;
                    if (m_isMuted)
                    {
                        m_buttons.ForEach(s => { if (s.CanBeMuted) { s.ClearBuffers(); } });
                        m_axes.ForEach(s => { if (s.CanBeMuted) { s.ClearBuffers(); } });
                    }
                }
            }
        }

        private bool m_firstUpdate = true;
        private bool m_firstFixedFrame = true;

        public Controls()
        {
            m_buttons = new List<BufferedButton>();
            m_axes = new List<BufferedAxis>();
            m_nameToButton = new Dictionary<string, BufferedButton>();
            m_nameToAxis = new Dictionary<string, BufferedAxis>();

            m_settings = new Settings();
        }
        
        public void LateFixedUpdate()
        {
            if (!m_firstUpdate)
            {
                m_firstFixedFrame = false;
            }
            m_firstUpdate = true;
        }

        /*
         * Needs to run at the start of every Update frame to buffer new inputs.
         */
        public void EarlyUpdate()
        {
            if (m_firstUpdate)
            {
                m_firstUpdate = false;
                // Needs to run at the end of every FixedUpdate cycle to handle the input buffers.
                m_buttons.ForEach(s => s.RemoveOldBuffers());
                m_axes.ForEach(s => s.RemoveOldBuffers());
                m_firstFixedFrame = true;
            }

            m_buttons.ForEach(s => s.BufferInput(m_isMuted));
            m_axes.ForEach(s => s.BufferInput(m_isMuted));
        }

        /*
         * Clears the current controls and replaces them with the default set.
         */
        public void UseDefaults()
        {
            m_buttons.ForEach(s => s.UseDefaults());
            m_axes.ForEach(s => s.UseDefaults());
            m_settings.UseDefaults();
        }

        public void AddButton(string name, string displayName, bool canRebind, bool canBeMuted, params ISource<bool>[] defaultBindings)
        {
            BufferedButton button = new BufferedButton(displayName, canRebind, canBeMuted, defaultBindings);
            m_buttons.Add(button);
            m_nameToButton.Add(name, button);
        }

        public void AddAxis(string name, string displayName, bool canRebind, bool canBeMuted, float exponent, params ISource<float>[] defaultBindings)
        {
            BufferedAxis axis = new BufferedAxis(displayName, canRebind, canBeMuted, exponent, defaultBindings);
            m_axes.Add(axis);
            m_nameToAxis.Add(name, axis);
        }

        /*
         * Returns true if any of the relevant keyboard or joystick buttons are held down.
         */
        public bool IsDown(string button)
        {
            BufferedButton bufferedButton = m_nameToButton[button];
            return !(m_isMuted && bufferedButton.CanBeMuted) && bufferedButton.IsDown();
        }

        /*
         * Returns true if a relevant keyboard or joystick key was pressed since the last appropriate update.
         */
        public bool JustDown(string button)
        {
            BufferedButton bufferedButton = m_nameToButton[button];
            bool isFixed = (Time.deltaTime == Time.fixedDeltaTime);
            return !(m_isMuted && bufferedButton.CanBeMuted) && (isFixed ? m_firstFixedFrame && bufferedButton.JustDown() : bufferedButton.VisualJustDown());
        }

        /*
         * Returns true if a relevant keyboard or joystick key was released since the last appropriate update.
         */
        public bool JustUp(string button)
        {
            BufferedButton bufferedButton = m_nameToButton[button];
            bool isFixed = (Time.deltaTime == Time.fixedDeltaTime);
            return !(m_isMuted && bufferedButton.CanBeMuted) && (isFixed ? m_firstFixedFrame && bufferedButton.JustUp() : bufferedButton.VisualJustUp());
        }

        /*
         * Returns the average value of an axis from all Update frames since the last FixedUpdate.
         */
        public float AverageValue(string axis)
        {
            BufferedAxis bufferedAxis = m_nameToAxis[axis];
            return (m_isMuted && bufferedAxis.CanBeMuted) ? 0 : bufferedAxis.GetValue(true);
        }

        /*
         * Returns the cumulative value of an axis from all Update frames since the last FixedUpdate.
         */
        public float CumulativeValue(string axis)
        {
            BufferedAxis bufferedAxis = m_nameToAxis[axis];
            return (m_isMuted && bufferedAxis.CanBeMuted) ? 0 : bufferedAxis.GetValue(false);
        }


        public enum RebindState
        {
            None,
            Button,
            Axis,
            KeyAxis,
            ButtonAxis
        }

        private RebindState m_rebindState = RebindState.None;
        public RebindState rebindState
        {
            get { return m_rebindState; }
        }

        private BufferedButton m_rebindingButton = null;
        private BufferedAxis m_rebindingAxis = null;
        private List<KeyCode> m_rebindingPreviousKeys = new List<KeyCode>();
        private List<GamepadButton> m_rebindingPreviousButtons = new List<GamepadButton>();
        private KeyCode m_rebindingAxisKey;
        private GamepadButton m_rebindingAxisButton;
        private Action m_onRebindComplete = null;
        

        public void AddBinding(IInputSource source, Action onRebindComplete)
        {
            if (m_rebindState == RebindState.None)
            {
                if (source is BufferedButton)
                {
                    m_rebindState = RebindState.Button;
                    m_rebindingButton = (BufferedButton)source;
                }
                else if (source is BufferedAxis)
                {
                    m_rebindState = RebindState.Axis;
                    m_rebindingAxis = (BufferedAxis)source;
                }

                m_onRebindComplete = onRebindComplete;
                m_rebindingPreviousKeys = FindActiveKeys(false);
                m_rebindingPreviousButtons = FindActiveButtons(false);
            }
        }

        /*
         * When rebinding detects any appropriate inputs.
         */
        public void UpdateRebinding()
        {
            if (m_rebindState != RebindState.None)
            {
                if (m_rebindState == RebindState.Axis)
                {
                    ISource<float> source = GetAxisSource();
                    if (source != null)
                    {
                        m_rebindState = RebindState.None;
                        m_rebindingAxis.AddSource(source);
                        m_onRebindComplete();
                    }
                    else
                    {
                        List<KeyCode> activeKeys = FindActiveKeys(true);
                        List<GamepadButton> activeButtons = FindActiveButtons(true);
                        if (activeButtons.Count > 0)
                        {
                            m_rebindState = RebindState.ButtonAxis;
                            m_rebindingAxisButton = activeButtons.First();
                        }
                        else if (activeKeys.Count > 0)
                        {
                            m_rebindState = RebindState.KeyAxis;
                            m_rebindingAxisKey = activeKeys.First();
                        }
                    }
                }
                else if (m_rebindState == RebindState.Button)
                {
                    ISource<bool> source = GetButtonSource();
                    if (source != null)
                    {
                        m_rebindState = RebindState.None;
                        m_rebindingButton.AddSource(source);
                        m_onRebindComplete();
                    }
                }
                else if (m_rebindState == RebindState.ButtonAxis)
                {
                    List<GamepadButton> activeButtons = FindActiveButtons(true);
                    if (activeButtons.Count > 0)
                    {
                        m_rebindState = RebindState.None;
                        m_rebindingAxis.AddSource(new JoystickButtonAxis(activeButtons.First(), m_rebindingAxisButton));
                        m_onRebindComplete();
                    }
                }
                else if (m_rebindState == RebindState.KeyAxis)
                {
                    List<KeyCode> activeKeys = FindActiveKeys(true);
                    if (activeKeys.Count > 0)
                    {
                        m_rebindState = RebindState.None;
                        m_rebindingAxis.AddSource(new KeyAxis(activeKeys.First(), m_rebindingAxisKey));
                        m_onRebindComplete();
                    }
                }

                m_rebindingPreviousKeys = FindActiveKeys(false);
                m_rebindingPreviousButtons = FindActiveButtons(false);
            }
        }

        private ISource<bool> GetButtonSource()
        {
            List<GamepadButton> activeButtons = FindActiveButtons(true);
            if (activeButtons.Count > 0)
            {
                return new JoystickButton(activeButtons.First());
            }
            List<KeyCode> activeKeys = FindActiveKeys(true);
            if (activeKeys.Count > 0)
            {
                return new KeyButton(activeKeys.First());
            }
            return null;
        }

        private ISource<float> GetAxisSource()
        {
            foreach (GamepadAxis axis in Enum.GetValues(typeof(GamepadAxis)))
            {
                if (JoystickAxis.GetAxisValue(axis) > 0.5f)
                {
                    return new JoystickAxis(axis);
                }
            }
            foreach (MouseAxis.Axis axis in Enum.GetValues(typeof(MouseAxis.Axis)))
            {
                if (MouseAxis.GetAxisValue(axis) > 0.5f)
                {
                    return new MouseAxis(axis);
                }
            }
            return null;
        }

        private List<KeyCode> FindActiveKeys(bool ignorePrevious)
        {
            return Enum.GetValues(typeof(KeyCode)).Cast<KeyCode>().Where(
                button => KeyButton.GetButtonValue(button) && (!ignorePrevious || !m_rebindingPreviousKeys.Contains(button))
                ).ToList();
        }

        private List<GamepadButton> FindActiveButtons(bool ignorePrevious)
        {
            return Enum.GetValues(typeof(GamepadButton)).Cast<GamepadButton>().Where(
                button => JoystickButton.GetButtonValue(button) && (!ignorePrevious || !m_rebindingPreviousButtons.Contains(button))
                ).ToList();
        }

        public SerializableControls Serialize()
        {
            return new SerializableControls().Serialize(this);
        }

        public bool Deserialize(SerializableControls serializableControls)
        {
            return serializableControls.Deserialize(this);
        }
    }
}