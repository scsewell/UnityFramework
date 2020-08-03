using System.Linq;

using TMPro;

using UnityEditor;
using UnityEditor.SceneManagement;

using UnityEngine;
using UnityEngine.UI;

namespace Framework.EditorTools
{
    internal static class TMPUpgradeMenu
    {
        [MenuItem("Framework/Tools/UI/Text To TMP (Selected) %#t", false, 100)]
        private static void SelectionToTextMeshPro()
        {
            Convert(Selection.gameObjects
                .Select(go => go.GetComponent<Text>())
                .Where(t => t != null)
                .ToArray());
        }

        [MenuItem("Framework/Tools/UI/Text To TMP (Selected With Children) #&t", false, 101)]
        private static void SelectionToTextMeshProRecursive()
        {
            Convert(Selection.gameObjects
                .SelectMany(go => go.GetComponentsInChildren<Text>(true))
                .Where(t => t != null)
                .ToArray());
        }

        private static void Convert(Text[] texts)
        {
            Undo.SetCurrentGroupName("Convert Text To TMP");

            foreach (var text in texts)
            {
                Undo.RegisterFullObjectHierarchyUndo(text, string.Empty);
                ConvertTextToTextMeshPro(text);
            }

            Undo.FlushUndoRecordObjects();
        }

        private static void ConvertTextToTextMeshPro(Text text)
        {
            var go = text.gameObject;
            var sizeDelta = go.GetComponent<RectTransform>().sizeDelta;

            // extract all settings on the text component and remove the compoent
            var settings = TMPConverter.GetTextSettings(text);
            Object.DestroyImmediate(text, false);

            // add a new TMP component with all of the old settings
            var tmp = go.AddComponent<TextMeshProUGUI>();
            TMPConverter.ApplyTextSettings(tmp, settings);

            // remove outlines since they don't work with TMP
            if (go.TryGetComponent<Outline>(out var outline))
            {
                Undo.DestroyObjectImmediate(outline);
            }

            // the size gets changed by the TMP component for some silly reason, so fix it
            go.GetComponent<RectTransform>().sizeDelta = sizeDelta;

            // make sure we can remove the new component if undoing
            Undo.RegisterCreatedObjectUndo(tmp, string.Empty);

            // we modified the scene, so record that is was changed
            EditorSceneManager.MarkSceneDirty(go.scene);
        }
    }
}
