using System.Linq;

using UnityEngine;
using UnityEngine.UI;

using UnityEditor;
using UnityEditor.SceneManagement;

using TMPro;

namespace Framework.EditorTools
{
    public static class TextMeshProUpgrade
    {
        [MenuItem("Framework/Tools/UI/Text To TMP (Selected) %#t", false, 100)]
        private static void SelectionToTextMeshPro()
        {
            Convert(Selection.gameObjects.Select(go =>
                go.GetComponent<Text>()).Where(t => t != null)
            .ToArray());
        }

        [MenuItem("Framework/Tools/UI/Text To TMP (Selected With Children) #&t", false, 101)]
        private static void SelectionToTextMeshProRecursive()
        {
            Convert(Selection.gameObjects.SelectMany(go =>
                go.GetComponentsInChildren<Text>(true)).Where(t => t != null)
            .ToArray());
        }

        private static void Convert(Text[] texts)
        {
            Undo.SetCurrentGroupName("Convert Text To TMP");

            foreach (Text text in texts)
            {
                Undo.RegisterFullObjectHierarchyUndo(text, string.Empty);
                ConvertTextToTextMeshPro(text);
            }

            Undo.FlushUndoRecordObjects();
        }

        private static void ConvertTextToTextMeshPro(Text text)
        {
            GameObject go = text.gameObject;
            Vector2 sizeDelta = go.GetComponent<RectTransform>().sizeDelta;

            // extract all settings on the text component and remove the compoent
            TextSettings settings = TMPConverter.GetTextSettings(text);
            Object.DestroyImmediate(text, false);

            // add a new TMP component with all of the old settings
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            TMPConverter.ApplyTextSettings(tmp, settings);

            // remove outlines since they don't work with TMP
            Outline outline = go.GetComponent<Outline>();
            if (outline != null)
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
