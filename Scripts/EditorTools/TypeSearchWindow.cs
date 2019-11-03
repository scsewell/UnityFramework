using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine;
using UnityEditor;

namespace Framework.EditorTools
{
    /// <summary>
    /// A utility window used to select a class type
    /// </summary>
    public class TypeSearchWindow : EditorWindow
    {
        private const float WINDOW_WIDTH = 700;
        private const float WINDOW_HEIGHT = 300;
        private static readonly string SEARCH_FIELD_NAME = $"{nameof(TypeSearchWindow)}SearchField";

        private static bool _focusSearchBox = false;

        private Vector2 _size;
        private Action<Type> _applyType = null;

        private Type[] _allTypes = null;
        private string[] _allTypeNamesLower = null;

        private Type[] _searchedTypes = null;
        private string[] _searchedTypeNames = null;

        private string _search = string.Empty;
        private Vector2 _scrollPos = Vector2.zero;

        /// <summary>
        /// Draws a type search selection dropdown.
        /// </summary>
        /// <param name="typeProp">A string property to hold the full type name.</param>
        /// <param name="filter">A filter which controls which types can be selected.</param>
        /// <param name="onComplete">An action taken when a new type is selected.</param>
        public static void DrawDropdown(SerializedProperty typeProp, Func<Type, bool> filter = null, Action onComplete = null)
        {
            Rect rect = EditorGUILayout.GetControlRect();
            GUIContent content = EditorGUI.BeginProperty(rect, new GUIContent(typeProp.displayName), typeProp);
            rect = EditorGUI.PrefixLabel(rect, content);

            EditorGUI.showMixedValue = typeProp.hasMultipleDifferentValues;

            // draw the dropdown which opens the selection window
            Type currentType = Type.GetType(typeProp.stringValue);
            string displayValue = currentType == null ? string.Empty : currentType.Name;

            if (EditorGUI.DropdownButton(rect, new GUIContent(displayValue), FocusType.Keyboard))
            {
                PickClass(rect, filter, (type) =>
                {
                    typeProp.stringValue = type?.AssemblyQualifiedName ?? string.Empty;
                    typeProp.serializedObject.ApplyModifiedProperties();
                    onComplete?.Invoke();
                });

                // focus the search field
                _focusSearchBox = true;
            }
        }

        public static void PickClass(Rect dropdown, Func<Type, bool> filter, Action<Type> applyType)
        {
            TypeSearchWindow window = CreateInstance<TypeSearchWindow>();

            // size the window to appropriately fit the content type
            Vector2 size = new Vector2(WINDOW_WIDTH, WINDOW_HEIGHT);

            // draw the window below the control that initiated the selection
            dropdown.position = GUIUtility.GUIToScreenPoint(dropdown.position);
            window.ShowAsDropDown(dropdown, size);

            window._size = size;
            window._applyType = applyType;

            // load all the relevant type information
            window.GetAllTypes(filter);
            window.GetSearchTypes();
        }

        private void OnGUI()
        {
            DrawWindowOutline();

            EditorGUILayout.BeginHorizontal();
            DrawSearchBox();
            DrawNoneButton();
            EditorGUILayout.EndHorizontal();

            DrawTypeBox();
        }

        private void DrawWindowOutline()
        {
            const float outlineWidth = 1.0f;
            Rect[] rects = new Rect[]
            {
                new Rect(0, 0, outlineWidth, _size.y),
                new Rect(0, 0, _size.x, outlineWidth),
                new Rect(0, _size.y - outlineWidth, _size.x, outlineWidth),
                new Rect(_size.x - outlineWidth, 0, outlineWidth, _size.y),
            };

            GUI.color = new Color(0.5f, 0.5f, 0.5f);
            Array.ForEach(rects, r => GUI.DrawTexture(r, EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill));
            GUI.color = Color.white;
        }

        private void DrawSearchBox()
        {
            EditorGUILayout.LabelField("Search", GUILayout.Width(50f));

            // get the search text, updaing the shown types if changed
            EditorGUI.BeginChangeCheck();

            GUI.SetNextControlName(SEARCH_FIELD_NAME);
            _search = EditorGUILayout.TextField(_search);

            if (EditorGUI.EndChangeCheck())
            {
                GetSearchTypes();
            }

            // focus the search field when requested
            if (_focusSearchBox)
            {
                GUI.FocusControl(SEARCH_FIELD_NAME);
                EditorGUI.FocusTextInControl(SEARCH_FIELD_NAME);
                _focusSearchBox = false;
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
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            _scrollPos = GUILayout.BeginScrollView(_scrollPos);

            for (int i = 0; i < _searchedTypes.Length; i++)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Select", EditorStyles.miniButton, GUILayout.MaxWidth(60f)))
                {
                    Complete(_searchedTypes[i]);
                }
                GUILayout.Label(_searchedTypeNames[i]);
                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void Complete(Type type)
        {
            _applyType?.Invoke(type);
            Close();
        }

        private void GetAllTypes(Func<Type, bool> filter)
        {
            List<Type> allTypes = new List<Type>();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (filter(type))
                    {
                        allTypes.Add(type);
                    }
                }
            }

            // sort the types and cache their full names
            _allTypes = allTypes.OrderBy(t => t.FullName).ToArray();
            _allTypeNamesLower = _allTypes.Select(t => t.FullName.ToLowerInvariant()).ToArray();
        }

        private void GetSearchTypes()
        {
            string searchValue = _search.ToLowerInvariant();

            List<Type> searchTypes = new List<Type>();

            for (int i = 0; i < _allTypeNamesLower.Length; i++)
            {
                if (_allTypeNamesLower[i].Contains(searchValue))
                {
                    searchTypes.Add(_allTypes[i]);
                }
            }

            _searchedTypes = searchTypes.ToArray();
            _searchedTypeNames = _searchedTypes.Select(t => t.FullName).ToArray();
        }
    }
}
