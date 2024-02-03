using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public partial class ContentEditor : EditorWindow
{
    private static ContentEditor Window;

    [MenuItem("Content/Open", priority = 2056)]
    static void ShowWindow()
    {
        Window = (ContentEditor)GetWindow(typeof(ContentEditor), false);
        Window.Show();
        Window.titleContent = new GUIContent("Content");
        Window.minSize = new Vector2(640, 480);
    }

    Content content;

    void OnEnable()
    {
        content = Content.Get();
        Window = this;
    }

    void OnDisable()
    {
        AddonEditorWindow?.Close();
        SliceMenuWindow?.Close();
    }

    void OnFocus()
    {
        m_AssetsTree?.Reload();
    }

    ContentTreeView m_AssetsTree;
    TreeViewState m_AssetsTreeState;
    SearchField m_AssetsSearchField;
    MultiColumnHeaderState cachedAssetsColumns;

    private void OnGUI()
    {
        if (content)
        {
            titleContent = new GUIContent($"Content | Addons: {content.AddonsCount()}");
        }

        int border = 4;
        int topPadding = 12;
        int searchHeight = 20;
        Rect rect = new Rect(0, searchHeight + topPadding, position.width, position.height);

        {
            var searchRect = new Rect(border, topPadding, rect.width - border * 2, searchHeight);
            var remainTop = topPadding + searchHeight + border;
            var remainingRect = new Rect(
                border,
                topPadding + searchHeight + border,
                rect.width - border * 2,
                rect.height - remainTop - border
            );

            if (m_AssetsSearchField == null)
            {
                m_AssetsSearchField = new SearchField();
                m_AssetsSearchField.SetFocus();
                m_AssetsSearchField.downOrUpArrowKeyPressed += () =>
                {
                    m_AssetsTree.SetFocus();
                };
            }

            if (m_AssetsTree == null)
            {
                if (m_AssetsTreeState == null)
                {
                    m_AssetsTreeState = new TreeViewState();
                }

                if (cachedAssetsColumns == null)
                    cachedAssetsColumns = ContentTreeView.GetColumns();

                m_AssetsTree = new ContentTreeView(m_AssetsTreeState, cachedAssetsColumns);
                m_AssetsTree.Reload();
            }
            else
            {
                m_AssetsTree.searchString = m_AssetsSearchField.OnGUI(
                    searchRect,
                    m_AssetsTree.searchString
                );
                m_AssetsTree.OnGUI(rect);
            }
        }
    }
}
