using UnityEditor;
using UnityEngine;
using static Content;

public partial class ContentEditor : EditorWindow
{
    private static SliceMenu SliceMenuWindow;

    static void OpenSliceMenu(Addon addon, Bundle bundle)
    {
        SliceMenuWindow = (SliceMenu)GetWindow(typeof(SliceMenu), true);
        SliceMenuWindow.Show();
        SliceMenuWindow.titleContent = new GUIContent(
            $"{nameof(SliceMenu)} - {addon.Name} - {bundle.name}"
        );
        SliceMenuWindow.maxSize = new Vector2(600, 150);
        SliceMenuWindow.minSize = new Vector2(200, 150);
        SliceMenuWindow.SetAddon(addon);
        SliceMenuWindow.SetBundle(bundle);
    }
}

public class SliceMenu : EditorWindow
{
    Addon CurrentAddon;
    Bundle CurrentBundle;

    public void SetAddon(Addon addon) => CurrentAddon = addon;

    public void SetBundle(Bundle bundle) => CurrentBundle = bundle;

    int parts = 1;

    void OnGUI()
    {
        if (CurrentAddon != null && CurrentBundle != null)
        {
            EditorGUILayout.HelpBox(
                "How many parts create from current bundle?",
                MessageType.Info,
                true
            );
            parts = EditorGUILayout.IntField(parts);

            if (GUILayout.Button("Slice"))
            {
                CurrentAddon.SliceBundle(CurrentBundle.name, parts);
                SaveToAsset();
                Close();
            }

            if (GUILayout.Button("Maximum Slice"))
            {
                CurrentAddon.SliceBundle(CurrentBundle.name, CurrentBundle.AssetsCount());
                SaveToAsset();
                Close();
            }
        }
        else
        {
            Close();
        }
    }
}
