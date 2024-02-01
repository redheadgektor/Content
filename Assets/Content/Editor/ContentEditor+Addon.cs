using UnityEditor;
using UnityEngine;
using static Content;

public partial class ContentEditor : EditorWindow
{
    private static AddonEditor AddonEditorWindow;

    static void OpenAddonEditor(Addon addon)
    {
        AddonEditorWindow = (AddonEditor)GetWindow(typeof(AddonEditor), true);
        AddonEditorWindow.Show();
        AddonEditorWindow.titleContent = new GUIContent($"{nameof(AddonEditor)} - {addon.Name}");
        AddonEditorWindow.maxSize = new Vector2(600, 150);
        AddonEditorWindow.minSize = new Vector2(200, 150);
        AddonEditorWindow.SetAddon(addon);
    }
}

public class AddonEditor : EditorWindow
{
    Addon CurrentAddon;

    public void SetAddon(Addon addon) => CurrentAddon = addon;

    void OnDisable()
    {
        SaveToAsset();
    }

    void OnGUI()
    {
        if (CurrentAddon != null)
        {
            GUILayout.BeginHorizontal("box");
            GUILayout.Label("Addon name: ");
            CurrentAddon.Name = EditorGUILayout.TextArea(CurrentAddon.Name);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("box");
            GUILayout.Label("Description: ");
            CurrentAddon.description = EditorGUILayout.TextArea(CurrentAddon.description);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("box");
            GUILayout.Label("Author: ");
            CurrentAddon.author = EditorGUILayout.TextArea(CurrentAddon.author);
            GUILayout.EndHorizontal();
        }
        else
        {
            Close();
        }
    }
}
