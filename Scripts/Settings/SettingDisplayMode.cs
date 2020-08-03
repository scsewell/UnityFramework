namespace Framework.Settings
{
    /// <summary>
    /// How a setting should appear in the settings menu.
    /// </summary>
    public enum SettingDisplayMode
    {
        /// <summary>
        /// The setting is not shown to the user.
        /// </summary>
        Hidden = 0,
        /// <summary>
        /// User can manually type a value.
        /// </summary>
        TextBox = 5,
        /// <summary>
        /// User selects a value from a dropdown list.
        /// </summary>
        Dropdown = 10,
        /// <summary>
        /// User scrolls though possible values.
        /// </summary>
        Spinner = 15,
        /// <summary>
        /// User moves a slider to select a value.
        /// </summary>
        Slider = 20,
        /// <summary>
        /// User toggles a value.
        /// </summary>
        Toggle = 25,
    }
}
