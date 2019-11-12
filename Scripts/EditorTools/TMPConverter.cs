using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

using TMPro;

namespace Framework.EditorTools
{
    /// <summary>
    /// Stores settings used for converting Text to TextMeshPro.
    /// </summary>
    public struct TextSettings
    {
        public bool enabled;
        public TMP_FontAsset font;
        public FontStyles fontStyle;
        public float fontSize;
        public float fontSizeMin;
        public float fontSizeMax;
        public float lineSpacing;
        public bool enableRichText;
        public bool enableAutoSizing;
        public TextAlignmentOptions alignment;
        public bool wordWrapping;
        public TextOverflowModes overflowMode;
        public string text;
        public Color color;
        public bool raycastTarget;
    }

    public static class TMPConverter
    {
        /// <summary>
        /// Gets the configuration of a UI text component.
        /// </summary>
        /// <param name="text">The text component to get the settings of.</param>
        public static TextSettings GetTextSettings(Text text)
        {
            return new TextSettings
            {
                enabled = text.enabled,
                text = text.text,
                font = GetFont(text.font, text.material),
                fontStyle = ConvertFontStyle(text.fontStyle),
                fontSize = text.fontSize,
                fontSizeMin = text.resizeTextMinSize,
                fontSizeMax = text.resizeTextMaxSize,
                lineSpacing = ConvertLineSpacing(text.lineSpacing),
                enableRichText = text.supportRichText,
                enableAutoSizing = text.resizeTextForBestFit,
                alignment = ConvertTextAnchor(text.alignment),
                wordWrapping = ConvertHorizontalWrapMode(text.horizontalOverflow),
                overflowMode = ConvertVerticalWrapMode(text.verticalOverflow),
                color = text.color,
                raycastTarget = text.raycastTarget,
            };
        }

        /// <summary>
        /// Applies a text component configuration to a text component.
        /// </summary>
        /// <param name="text">The Text Mesh Pro text to apply text configuration to./param>
        /// <param name="settings">The configuration to apply.</param>
        public static void ApplyTextSettings(TMP_Text text, TextSettings settings)
        {
            if (text != null)
            {
                text.enabled = settings.enabled;
                text.font = settings.font;
                text.fontStyle = settings.fontStyle;
                text.fontSize = settings.fontSize;
                text.fontSizeMin = settings.fontSizeMin;
                text.fontSizeMax = settings.fontSizeMax;
                text.lineSpacing = settings.lineSpacing;
                text.richText = settings.enableRichText;
                text.enableAutoSizing = settings.enableAutoSizing;
                text.alignment = settings.alignment;
                text.enableWordWrapping = settings.wordWrapping;
                text.overflowMode = settings.overflowMode;
                text.text = settings.text;
                text.color = settings.color;
                text.raycastTarget = settings.raycastTarget;
            }
        }

        public static TMP_FontAsset GetFont(Font font, Material material)
        {
#if UNITY_EDITOR
            // Mapping the fonts can only be done in the editor, and to be consistent
            // with behaviour when built don't return a result in play mode.
            if (Application.isPlaying)
            {
                return null;
            }

            // find all fonts with a matching name
            List<TMP_FontAsset> matchingFonts = new List<TMP_FontAsset>();

            foreach (string guid in AssetDatabase.FindAssets("t:TMP_FontAsset"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.IsNullOrEmpty(path))
                {
                    foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path))
                    {
                        TMP_FontAsset tmpFontAsset = asset as TMP_FontAsset;
                        if (tmpFontAsset != null && tmpFontAsset.name.ToLowerInvariant().Contains(font.name.ToLowerInvariant()))
                        {
                            matchingFonts.Add(tmpFontAsset);
                        }
                    }
                }
            }

            TMP_FontAsset tmpFont = matchingFonts.FirstOrDefault();

            // take any font with a matching name if the material settings can't be matched
            if (tmpFont == null)
            {
                Debug.LogWarning($"Couldn't find TextMeshPro font asset with matching material depth write settings as material \"{material.name}\" for font \"{font.name}\"!");
                tmpFont = matchingFonts.FirstOrDefault();
            }

            // use the default if no fonts share a name
            if (tmpFont == null)
            {
                Debug.LogWarning($"Couldn't find TextMeshPro font asset matching font \"{font.name}\"! A default will be used.");
                tmpFont = TMP_Settings.defaultFontAsset;
            }

            return tmpFont;
#else
            return null;
#endif
        }

        /// <summary>
        /// Converts a Unity text setting to a Text Mesh Pro equivalent.
        /// </summary>
        public static float ConvertLineSpacing(float spacing)
        {
            return 100f * (spacing - 1f);
        }

        /// <summary>
        /// Converts a Unity text setting to a Text Mesh Pro equivalent.
        /// </summary>
        public static bool ConvertHorizontalWrapMode(HorizontalWrapMode overflow)
        {
            return overflow == HorizontalWrapMode.Wrap;
        }

        /// <summary>
        /// Converts a Unity text setting to a Text Mesh Pro equivalent.
        /// </summary>
        public static TextOverflowModes ConvertVerticalWrapMode(VerticalWrapMode verticalOverflow)
        {
            return verticalOverflow == VerticalWrapMode.Truncate ? TextOverflowModes.Truncate : TextOverflowModes.Overflow;
        }

        /// <summary>
        /// Converts a Unity text setting to a Text Mesh Pro equivalent.
        /// </summary>
        public static FontStyles ConvertFontStyle(FontStyle fontStyle)
        {
            switch (fontStyle)
            {
                case FontStyle.Normal: return FontStyles.Normal;
                case FontStyle.Bold: return FontStyles.Bold;
                case FontStyle.Italic: return FontStyles.Italic;
                case FontStyle.BoldAndItalic: return FontStyles.Bold | FontStyles.Italic;
            }
            throw new NotImplementedException($"{nameof(ConvertFontStyle)} does not implement all required cases.");
        }

        /// <summary>
        /// Converts a Unity text setting to a Text Mesh Pro equivalent.
        /// </summary>
        public static TextAlignmentOptions ConvertTextAnchor(TextAnchor textAnchor)
        {
            switch (textAnchor)
            {
                case TextAnchor.UpperLeft: return TextAlignmentOptions.TopLeft;
                case TextAnchor.UpperCenter: return TextAlignmentOptions.Top;
                case TextAnchor.UpperRight: return TextAlignmentOptions.TopRight;
                case TextAnchor.MiddleLeft: return TextAlignmentOptions.Left;
                case TextAnchor.MiddleCenter: return TextAlignmentOptions.Center;
                case TextAnchor.MiddleRight: return TextAlignmentOptions.Right;
                case TextAnchor.LowerLeft: return TextAlignmentOptions.BottomLeft;
                case TextAnchor.LowerCenter: return TextAlignmentOptions.Bottom;
                case TextAnchor.LowerRight: return TextAlignmentOptions.BottomRight;
            }
            throw new NotImplementedException($"{nameof(ConvertTextAnchor)} does not implement all required cases.");
        }
    }
}
