namespace Framework.Settings
{
    /// <summary>
    /// How a setting should appear in the settings menu.
    /// </summary>
    public enum SettingDisplayMode
    {
        /// <summary>
        /// User can manually type a value.
        /// </summary>
        TextBox,
        /// <summary>
        /// User selects a value from a dropdown list.
        /// </summary>
        Dropdown,
        /// <summary>
        /// User scrolls though possible values.
        /// </summary>
        Spinner,
        /// <summary>
        /// User toggles a value.
        /// </summary>
        Toggle,
        /// <summary>
        /// User moves a slider to select a value.
        /// </summary>
        Slider,
    }
}
