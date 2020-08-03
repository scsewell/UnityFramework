using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;

using UnityEngine;

namespace Framework.EditorTools
{
    /// <summary>
    /// A utility window used to select a class type.
    /// </summary>
    public class TypeSearchWindow : EditorWindow
    {
        private const float WINDOW_WIDTH = 700;
        private const float WINDOW_HEIGHT = 300;
        private static readonly string SEARCH_FIELD_NAME = $"{nameof(TypeSearchWindow)}SearchField";

        private static bool m_focusSearchBox = false;

        private Vector2 m_size;
        private Action<Type> m_onComplete = null;

        private Type[] m_allTypes = null;
        private string[] m_allTypeNamesLower = null;

        private Type[] m_searchedTypes = null;
        private string[] m_searchedTypeNames = null;

        private string m_search = string.Empty;
        private Vector2 m_scrollPos = Vector2.zero;

        /// <summary>
        /// Draws a type search selection dropdown.
        /// </summary>
        /// <param name="typeProp">A string property to hold the full type name.</param>
        /// <param name="filter">A filter which controls which types can be selected.</param>
        /// <param name="onComplete">An action taken when a new type is selected.</param>
        public static void DrawDropdown(SerializedProperty typeProp, Func<Type, bool> filter = null, Action onComplete = null)
        {
            var rect = EditorGUILayout.GetControlRect();

            using (var property = new EditorGUI.PropertyScope(rect, new GUIContent(typeProp.displayName), typeProp))
            {
                rect = EditorGUI.PrefixLabel(rect, property.content);

                EditorGUI.showMixedValue = typeProp.hasMultipleDifferentValues;

                // draw the dropdown which opens the selection window
                var currentType = Type.GetType(typeProp.stringValue);
                var displayValue = currentType == null ? string.Empty : currentType.Name;

                if (EditorGUI.DropdownButton(rect, new GUIContent(displayValue), FocusType.Keyboard))
                {
                    PickClass(rect, filter, (type) =>
                    {
                        typeProp.stringValue = type?.AssemblyQualifiedName ?? string.Empty;
                        typeProp.serializedObject.ApplyModifiedProperties();
                        onComplete?.Invoke();
                    });

                    // focus the search field
                    m_focusSearchBox = true;
                }
            }
        }

        /// <summary>
        /// Draws a window used to pick a type.
        /// </summary>
        /// <param name="dropdown">The rect to position the window under.</param>
        /// <param name="filter">A filter which controls which types can be selected.</param>
        /// <param name="onComplete">The action taken once a type has been selected.</param>
        public static void PickClass(Rect dropdown, Func<Type, bool> filter, Action<Type> onComplete)
        {
            var window = CreateInstance<TypeSearchWindow>();

            // size the window to appropriately fit the content type
            var size = new Vector2(WINDOW_WIDTH, WINDOW_HEIGHT);

            // draw the window below the control that initiated the selection
            dropdown.position = GUIUtility.GUIToScreenPoint(dropdown.position);
            window.ShowAsDropDown(dropdown, size);

            window.m_size = size;
            window.m_onComplete = onComplete;

            // load all the relevant type information
            window.FindAllTypes(filter);
            window.SearchTypes();
        }

        private void OnGUI()
        {
            DrawWindowOutline();

            using (new GUILayout.HorizontalScope())
            {
                DrawSearchBox();
                DrawNoneButton();
            }

            DrawTypeBox();
        }

        private void DrawWindowOutline()
        {
            const float outlineWidth = 1.0f;
            var rects = new Rect[]
            {
                new Rect(0, 0, outlineWidth, m_size.y),
                new Rect(0, 0, m_size.x, outlineWidth),
                new Rect(0, m_size.y - outlineWidth, m_size.x, outlineWidth),
                new Rect(m_size.x - outlineWidth, 0, outlineWidth, m_size.y),
            };

            GUI.color = new Color(0.5f, 0.5f, 0.5f);
            Array.ForEach(rects, r => GUI.DrawTexture(r, EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill));
            GUI.color = Color.white;
        }

        private void DrawSearchBox()
        {
            EditorGUILayout.LabelField("Search", GUILayout.Width(50f));

            // get the search text, updaing the shown types if changed
            using (var change = new EditorGUI.ChangeCheckScope())
            {
                GUI.SetNextControlName(SEARCH_FIELD_NAME);
                m_search = EditorGUILayout.TextField(m_search);

                if (change.changed)
                {
                    SearchTypes();
                }
            }

            // focus the search field when requested
            if (m_focusSearchBox)
            {
                GUI.FocusControl(SEARCH_FIELD_NAME);
                EditorGUI.FocusTextInControl(SEARCH_FIELD_NAME);
                m_focusSearchBox = false;
            }
        }

        private void DrawNoneButton()
        {
            if (GUILayout.Button(new GUIContent("None", "Clear the value."), GUILayout.Width(60f)))
            {
                Complete(null);
            }
        }

        private void DrawTypeBox()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            using (var scroll = new GUILayout.ScrollViewScope(m_scrollPos))
            {
                for (var i = 0; i < m_searchedTypes.Length; i++)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Select", EditorStyles.miniButton, GUILayout.MaxWidth(60f)))
                        {
                            Complete(m_searchedTypes[i]);
                        }
                        GUILayout.Label(m_searchedTypeNames[i]);
                    }
                }

                m_scrollPos = scroll.scrollPosition;
            }
        }

        private void Complete(Type type)
        {
            m_onComplete?.Invoke(type);
            Close();
        }

        private void FindAllTypes(Func<Type, bool> filter)
        {
            // sort the types and cache their full names
            m_allTypes = AppDomain.CurrentDomain.GetAssemblies().AsParallel()
                .SelectMany(a => a.GetTypes())
                .Where(t => filter(t))
                .OrderBy(t => t.FullName)
                .ToArray();

            m_allTypeNamesLower = m_allTypes
                .Select(t => t.FullName.ToLowerInvariant())
                .ToArray();
        }

        private void SearchTypes()
        {
            var searchValue = m_search.ToLowerInvariant();
            var searchTypes = new List<Type>();

            for (var i = 0; i < m_allTypeNamesLower.Length; i++)
            {
                if (m_allTypeNamesLower[i].Contains(searchValue))
                {
                    searchTypes.Add(m_allTypes[i]);
                }
            }

            m_searchedTypes = searchTypes.ToArray();
            m_searchedTypeNames = m_searchedTypes.Select(t => t.FullName).ToArray();
        }
    }
}
