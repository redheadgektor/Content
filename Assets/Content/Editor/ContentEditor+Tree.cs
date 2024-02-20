using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public partial class ContentEditor : EditorWindow
{
    public const float NotifyFadeOutTime = 3f;

    public sealed class AssetTreeViewItem : TreeViewItem
    {
        public Content.Asset asset;

        public long size { get; private set; } = -1;

        public void UpdateSize()
        {
            try
            {
                size = new System.IO.FileInfo(asset.path).Length;
            }
            catch
            {
                size = -1;
            }
        }

        public AssetTreeViewItem(int id, BundleTreeViewItem parent, Content.Asset asset)
            : base(id, 2, asset.name)
        {
            base.parent = parent;
            this.asset = asset;
            icon = AssetDatabase.GetCachedIcon(asset.path) as Texture2D;
            UpdateSize();
        }
    }

    public sealed class BundleTreeViewItem : TreeViewItem
    {
        public Content.Bundle bundle;
        private static Texture2D EmptyIcon;
        private static Texture2D FilledIcon;

        public long size { get; private set; } = 0;

        public void UpdateSize()
        {
            try
            {
                size = bundle.CalculateBundleSize();
            }
            catch { }
        }

        private static void CacheIcon()
        {
            if (!EmptyIcon)
            {
                EmptyIcon = EditorGUIUtility.IconContent("sv_icon_name6").image as Texture2D;
            }
            if (!FilledIcon)
            {
                FilledIcon = EditorGUIUtility.IconContent("sv_icon_name3").image as Texture2D;
            }
        }

        public void UpdateIcon()
        {
            CacheIcon();

            if (bundle.AssetsCount() > 0)
            {
                icon = FilledIcon;
            }
            else
            {
                icon = EmptyIcon;
            }
        }

        public BundleTreeViewItem(int id, AddonTreeViewItem parent, Content.Bundle bundle)
            : base(id, 1, bundle.name)
        {
            this.parent = parent;
            this.bundle = bundle;
            UpdateIcon();
            UpdateSize();
        }
    }

    public sealed class AddonTreeViewItem : TreeViewItem
    {
        public Content.Addon addon;
        private static Texture2D EmptyIcon;
        private static Texture2D FilledIcon;

        private static void CacheIcon()
        {
            if (!EmptyIcon)
            {
                EmptyIcon = EditorGUIUtility.IconContent("sv_icon_name6").image as Texture2D;
            }
            if (!FilledIcon)
            {
                FilledIcon = EditorGUIUtility.IconContent("sv_icon_name3").image as Texture2D;
            }
        }

        public void UpdateIcon()
        {
            CacheIcon();

            if (addon.BundlesCount() > 0)
            {
                icon = FilledIcon;
            }
            else
            {
                icon = EmptyIcon;
            }
        }

        public AddonTreeViewItem(int id, Content.Addon addon)
            : base(id, 0, addon.Name)
        {
            this.addon = addon;
            UpdateIcon();
        }
    }

    public class ContentTreeView : TreeView
    {
        public ContentTreeView(TreeViewState state, MultiColumnHeaderState headerState)
            : base(state, new MultiColumnHeader(headerState))
        {
            showAlternatingRowBackgrounds = true;
            showBorder = true;
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return true;
        }

        private bool Multiselected = false;
        private IList<int> MultiSelectedIDs;

        protected override void SelectionChanged(IList<int> ids)
        {
            MultiSelectedIDs = ids;
            Multiselected = MultiSelectedIDs.Count > 1;

            if (Multiselected)
            {
                SetFocus();
                return;
            }

            if (MultiSelectedIDs != null && MultiSelectedIDs.Count == 1)
            {
                var item = FindItem(MultiSelectedIDs[0], rootItem);
                if (item != null && item is AssetTreeViewItem)
                {
                    var asset_item = item as AssetTreeViewItem;
                    var asset = AssetDatabase.LoadMainAssetAtPath(asset_item.asset.path);

                    if (asset)
                    {
                        EditorGUIUtility.PingObject(asset);
                    }
                }

                SetFocus();
            }
        }

        protected override void DoubleClickedItem(int id)
        {
            var item = FindItem(id, rootItem);
            if (item != null && item is AssetTreeViewItem)
            {
                var asset_item = item as AssetTreeViewItem;
                var obj = AssetDatabase.LoadMainAssetAtPath(asset_item.asset.path);
                EditorGUIUtility.PingObject(obj);
            }
        }

        protected override TreeViewItem BuildRoot()
        {
            var Root = new TreeViewItem(-1, -1, "Root");

            if (Content.Get().AddonsCount() > 0)
            {
                for (var i = 0; i < Content.Get().AddonsCount(); i++)
                {
                    if (Content.Get().GetAddon(i, out var addon))
                    {
                        var addon_item_id = addon.GetHashCode();
                        var addon_item = new AddonTreeViewItem(addon_item_id, addon);
                        Root.AddChild(addon_item);

                        for (var j = 0; j < addon.BundlesCount(); j++)
                        {
                            if (addon.GetBundle(j, out var bundle))
                            {
                                var bundle_item_id = bundle.GetHashCode();
                                var bundle_item = new BundleTreeViewItem(
                                    bundle_item_id,
                                    addon_item,
                                    bundle
                                );
                                addon_item.AddChild(bundle_item);

                                for (var k = 0; k < bundle.AssetsCount(); k++)
                                {
                                    if (bundle.GetAsset(k, out var asset))
                                    {
                                        var asset_item_id = asset.GetHashCode();
                                        var asset_item = new AssetTreeViewItem(
                                            asset_item_id,
                                            bundle_item,
                                            asset
                                        );
                                        bundle_item.AddChild(asset_item);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                Root.AddChild(
                    new TreeViewItem()
                    {
                        id = 0,
                        depth = 0,
                        displayName = "No Addons!"
                    }
                );
            }

            return Root;
        }

        void CellGUIForAsset(Rect cellRect, AssetTreeViewItem item, int column, ref RowGUIArgs args)
        {
            switch (column)
            {
                case 0:

                    {
                        var iconRect = new Rect(
                            cellRect.x + 1 + 40,
                            cellRect.y + 1,
                            cellRect.height - 2,
                            cellRect.height - 2
                        );
                        if (item.icon != null)
                        {
                            GUI.DrawTexture(iconRect, item.icon, ScaleMode.ScaleToFit);
                        }
                        DefaultGUI.Label(
                            new Rect(
                                cellRect.x + iconRect.xMax + 1,
                                cellRect.y,
                                cellRect.width - iconRect.width,
                                cellRect.height
                            ),
                            item.displayName,
                            args.selected,
                            args.focused
                        );
                    }
                    break;
                case 1:
                    DefaultGUI.Label(cellRect, item.asset.path, args.selected, args.focused);
                    break;
                case 2:
                    DefaultGUI.Label(cellRect, item.asset.type, args.selected, args.focused);
                    break;
                case 3:
                    DefaultGUI.Label(cellRect, item.asset.base_type, args.selected, args.focused);
                    break;

                case 4:
                    var sz = item.size;
                    var sz_s = string.Empty;

                    if (sz == -1)
                    {
                        sz_s = "unknown";
                        DefaultGUI.Label(cellRect, sz_s, args.selected, args.focused);
                        break;
                    }

                    if (sz < 1024)
                    {
                        sz_s = $"{sz} b";
                    }

                    if (sz >= 1024)
                    {
                        sz_s = $"{sz / 1024} kb";
                    }

                    if (sz >= 1024 * 1024)
                    {
                        sz_s = $"{sz / 1024 / 1024} mb";
                    }

                    if (sz >= 1024 * 1024 * 1024)
                    {
                        sz_s = $"{sz / 1024 / 1024 / 1024} gb";
                    }

                    DefaultGUI.Label(cellRect, sz_s, args.selected, args.focused);
                    break;
            }
        }

        void CellGUIForBundle(
            Rect cellRect,
            BundleTreeViewItem item,
            int column,
            ref RowGUIArgs args
        )
        {
            switch (column)
            {
                case 0:

                    {
                        var iconRect = new Rect(
                            cellRect.x + 1 + 30,
                            cellRect.y + 1,
                            cellRect.height - 2,
                            cellRect.height - 2
                        );
                        if (item.icon != null)
                        {
                            GUI.DrawTexture(iconRect, item.icon, ScaleMode.ScaleToFit);
                        }
                        DefaultGUI.Label(
                            new Rect(
                                cellRect.x + iconRect.xMax + 1,
                                cellRect.y,
                                cellRect.width - iconRect.width,
                                cellRect.height
                            ),
                            item.displayName,
                            args.selected,
                            args.focused
                        );
                    }
                    break;

                case 4:
                    var sz = item.size;
                    var sz_s = string.Empty;

                    if (sz < 1024)
                    {
                        sz_s = $"{sz} B ({item.bundle.AssetsCount()})";
                    }

                    if (sz >= 1024)
                    {
                        sz_s = $"{sz / 1024} KB ({item.bundle.AssetsCount()})";
                    }

                    if (sz >= 1024 * 1024)
                    {
                        sz_s = $"{sz / 1024f / 1024f:f2} MB ({item.bundle.AssetsCount()})";
                    }

                    if (sz >= 1024 * 1024 * 1024)
                    {
                        sz_s = $"{sz / 1024f / 1024f / 1024f:f2} GB ({item.bundle.AssetsCount()})";
                    }

                    DefaultGUI.Label(cellRect, sz_s, args.selected, args.focused);
                    break;
            }
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            if (args.item is AddonTreeViewItem)
            {
                base.RowGUI(args);
            }
            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                if (args.item is AssetTreeViewItem)
                {
                    var rect = args.GetCellRect(i);
                    CellGUIForAsset(
                        rect,
                        args.item as AssetTreeViewItem,
                        args.GetColumn(i),
                        ref args
                    );
                }

                if (args.item is BundleTreeViewItem)
                {
                    var rect = args.GetCellRect(i);
                    CellGUIForBundle(
                        rect,
                        args.item as BundleTreeViewItem,
                        args.GetColumn(i),
                        ref args
                    );
                }
            }
        }

        protected override bool CanChangeExpandedState(TreeViewItem item)
        {
            if (item is AddonTreeViewItem)
            {
                var addon_item = item as AddonTreeViewItem;
                return addon_item != null
                    && addon_item.addon != null
                    && addon_item.addon.BundlesCount() > 0;
            }

            if (item is BundleTreeViewItem)
            {
                var bundle_item = item as BundleTreeViewItem;
                return bundle_item != null
                    && bundle_item.bundle != null
                    && bundle_item.bundle.AssetsCount() > 0;
            }

            return false;
        }

        public static MultiColumnHeaderState GetColumns()
        {
            var retVal = new MultiColumnHeaderState.Column[]
            {
                new MultiColumnHeaderState.Column(),
                new MultiColumnHeaderState.Column(),
                new MultiColumnHeaderState.Column(),
                new MultiColumnHeaderState.Column(),
                new MultiColumnHeaderState.Column()
            };
            retVal[0].headerContent = new GUIContent("Short name", "Short name of asset.");
            retVal[0].minWidth = 50;
            retVal[0].width = 300;
            retVal[0].maxWidth = 300;
            retVal[0].headerTextAlignment = TextAlignment.Left;
            retVal[0].canSort = true;
            retVal[0].autoResize = true;

            retVal[1].headerContent = new GUIContent("Path", "Full path to asset");
            retVal[1].minWidth = 50;
            retVal[1].width = 200;
            retVal[1].maxWidth = 500;
            retVal[1].headerTextAlignment = TextAlignment.Left;
            retVal[1].canSort = true;
            retVal[1].autoResize = true;

            retVal[2].headerContent = new GUIContent("Type", string.Empty);
            retVal[2].minWidth = 30;
            retVal[2].width = 100;
            retVal[2].maxWidth = 120;
            retVal[2].headerTextAlignment = TextAlignment.Left;
            retVal[2].canSort = true;
            retVal[2].autoResize = true;

            retVal[3].headerContent = new GUIContent("Base Type", string.Empty);
            retVal[3].minWidth = 30;
            retVal[3].width = 100;
            retVal[3].maxWidth = 120;
            retVal[3].headerTextAlignment = TextAlignment.Left;
            retVal[3].canSort = true;
            retVal[3].autoResize = true;

            retVal[4].headerContent = new GUIContent("Size", string.Empty);
            retVal[4].minWidth = 30;
            retVal[4].width = 100;
            retVal[4].maxWidth = 100;
            retVal[4].headerTextAlignment = TextAlignment.Left;
            retVal[4].canSort = true;
            retVal[4].autoResize = true;

            return new MultiColumnHeaderState(retVal);
        }

        /* Renaming */
        protected override bool CanRename(TreeViewItem item)
        {
            return item != null && item is AssetTreeViewItem
                || item is BundleTreeViewItem
                || item is AddonTreeViewItem;
        }

        protected override void RenameEnded(RenameEndedArgs args)
        {
            base.RenameEnded(args);

            if (args.newName.Length > 0 && args.newName != args.originalName)
            {
                var item = FindItem(args.itemID, rootItem);

                if (item is AssetTreeViewItem)
                {
                    if (!Content.Get().HasAsset(args.newName, out var asset))
                    {
                        var asset_item = item as AssetTreeViewItem;
                        asset_item.asset.name = args.newName;
                        args.acceptedRename = true;
                        Content.SaveToAsset();
                        Reload();
                    }
                    else
                    {
                        Window.ShowNotification(
                            new GUIContent($"Asset {args.newName} already exists"),
                            NotifyFadeOutTime
                        );
                        args.acceptedRename = false;
                    }
                }

                if (item is BundleTreeViewItem)
                {
                    var addon_item = item.parent as AddonTreeViewItem;

                    if (!addon_item.addon.HasBundle(args.newName, out var bundle))
                    {
                        var bundle_item = item as BundleTreeViewItem;
                        bundle_item.bundle.name = args.newName;
                        args.acceptedRename = true;
                        Content.SaveToAsset();
                        Reload();
                    }
                    else
                    {
                        Window.ShowNotification(
                            new GUIContent($"Bundle {args.newName} already exists in this addon!"),
                            NotifyFadeOutTime
                        );
                        args.acceptedRename = false;
                    }
                }

                if (item is AddonTreeViewItem)
                {
                    if (!Content.Get().HasAddon(args.newName, out var addon))
                    {
                        var addon_item = item as AddonTreeViewItem;
                        addon_item.addon.Name = args.newName;
                        args.acceptedRename = true;
                        Content.SaveToAsset();
                        Reload();
                    }
                    else
                    {
                        Window.ShowNotification(
                            new GUIContent($"Addon {args.newName} already exists"),
                            NotifyFadeOutTime
                        );
                        args.acceptedRename = false;
                    }
                }
            }
            else
            {
                args.acceptedRename = false;
            }
        }

        /* Context (Additional menu) */
        private bool m_ContextOnItem = false;

        protected override void ContextClicked()
        {
            if (m_ContextOnItem)
            {
                m_ContextOnItem = false;
                return;
            }

            GenericMenu menu = new GenericMenu();

            menu.AddItem(
                new GUIContent("New Addon"),
                false,
                delegate
                {
                    Content
                        .Get()
                        .AddAddon($"NewAddon_{Content.Get().AddonsCount()}", out var addon);

                    Content.SaveToAsset();
                    Reload();
                }
            );

            menu.ShowAsContext();
        }

        protected override void ContextClickedItem(int id)
        {
            m_ContextOnItem = true;

            var item = FindItem(id, rootItem);

            void RemoveAddon(TreeViewItem item)
            {
                if (item is AddonTreeViewItem)
                {
                    var addon_item = item as AddonTreeViewItem;
                    Content.Get().RemoveAddon(addon_item.addon.Name);
                }
            }

            void RemoveBundle(TreeViewItem item)
            {
                if (item is BundleTreeViewItem)
                {
                    var bundle_item = item as BundleTreeViewItem;

                    /* GET ADDON */
                    var addon_item = bundle_item.parent as AddonTreeViewItem;
                    addon_item.addon.RemoveBundle(bundle_item.bundle.name);
                }
            }

            void RemoveAsset(TreeViewItem item)
            {
                if (item is AssetTreeViewItem)
                {
                    var asset_item = item as AssetTreeViewItem;

                    /* GET BUNDLE */
                    var bundle_item = asset_item.parent as BundleTreeViewItem;
                    bundle_item.bundle.RemoveAsset(asset_item.asset.guid);
                }
            }

            void AddNewBundle(TreeViewItem item)
            {
                if (item is AddonTreeViewItem)
                {
                    var addon_item = item as AddonTreeViewItem;
                    addon_item.addon.AddBundle(
                        $"NewBundle_{addon_item.addon.BundlesCount()}.bundle",
                        out var blob
                    );
                }
            }

            GenericMenu menu = new GenericMenu();

            /* ADDON ITEM */
            if (item is AddonTreeViewItem)
            {
                menu.AddItem(
                    new GUIContent("Create/New Addon"),
                    false,
                    delegate
                    {
                        Content
                            .Get()
                            .AddAddon($"NewAddon_{Content.Get().AddonsCount()}", out var result);
                        Content.SaveToAsset();
                        Reload();
                    }
                );
                menu.AddItem(
                    new GUIContent("Create/Add Bundle"),
                    false,
                    delegate
                    {
                        AddNewBundle(item);
                        Content.SaveToAsset();
                        Reload();
                    }
                );

                menu.AddItem(
                    new GUIContent("Edit"),
                    false,
                    delegate
                    {
                        OpenAddonEditor((item as AddonTreeViewItem).addon);
                    }
                );

                menu.AddItem(
                        new GUIContent("Tool/Build"),
                        false,
                        delegate
                        {
                            Content
                                .Get()
                                .BuildAddon((item as AddonTreeViewItem).addon);
                        }
                    );
            }

            /* SHARED ITEM */
            menu.AddItem(
                new GUIContent("Rename"),
                false,
                delegate
                {
                    BeginRename(FindItem(id, rootItem));
                }
            );

            menu.AddItem(
                new GUIContent("Remove"),
                false,
                delegate
                {
                    if (!Multiselected)
                    {
                        RemoveAddon(item);
                        RemoveBundle(item);
                        RemoveAsset(item);
                    }
                    else
                    {
                        foreach (var i in MultiSelectedIDs)
                        {
                            var selected_item = FindItem(i, rootItem);
                            RemoveAddon(selected_item);
                            RemoveBundle(selected_item);
                            RemoveAsset(selected_item);
                        }
                    }
                    Content.SaveToAsset();
                    Reload();
                }
            );

            /* BUNDLE ITEM */
            if (item is BundleTreeViewItem)
            {
                var bundle_item = item as BundleTreeViewItem;
                var addon_item = bundle_item.parent as AddonTreeViewItem;

                if (Multiselected)
                {
                    var selected_bundles = new List<Content.Bundle>();

                    foreach (var b in MultiSelectedIDs)
                    {
                        var selected_item = FindItem(b, rootItem);
                        if (selected_item is BundleTreeViewItem)
                        {
                            var selected_bundle = selected_item as BundleTreeViewItem;

                            if (selected_bundle.parent == addon_item)
                            {
                                selected_bundles.Add(selected_bundle.bundle);
                            }
                        }
                    }

                    menu.AddItem(
                        new GUIContent("Tool/Build (Multiple)"),
                        false,
                        delegate
                        {
                            Content
                                .Get()
                                .BuildMultiple(addon_item.addon, selected_bundles.ToArray());
                        }
                    );
                }
                else
                {
                    menu.AddItem(
                        new GUIContent("Tool/Put in dependencies"),
                        false,
                        delegate
                        {
                            Content
                                .Get()
                                .PutInDependenciesOfBundle(
                                    bundle_item.bundle.name,
                                    addon_item.addon
                                );
                            Content.SaveToAsset();
                            Reload();
                        }
                    );

                    menu.AddItem(
                        new GUIContent("Tool/Build (Single)"),
                        false,
                        delegate
                        {
                            Content.Get().BuildSingle(addon_item.addon, bundle_item.bundle);
                        }
                    );
                }

                if (Multiselected)
                {
                    menu.AddItem(
                        new GUIContent("Tool/Split Bundles"),
                        false,
                        delegate
                        {
                            var first_bundle_selected = FindItem(MultiSelectedIDs[0], rootItem);
                            var bundles_selected = new List<string>();

                            if (first_bundle_selected is BundleTreeViewItem)
                            {
                                var first_bundle_item = first_bundle_selected as BundleTreeViewItem;
                                foreach (var i in MultiSelectedIDs)
                                {
                                    var selected_item = FindItem(i, rootItem);
                                    var bundle_item = selected_item as BundleTreeViewItem;

                                    if (selected_item is BundleTreeViewItem)
                                    {
                                        if (selected_item != first_bundle_selected)
                                        {
                                            bundles_selected.Add(bundle_item.bundle.name);
                                        }
                                    }
                                }
                                var splited = addon_item.addon.SplitBundle(
                                    bundles_selected.ToArray(),
                                    first_bundle_item.bundle.name
                                );
                                Window.ShowNotification(
                                    new GUIContent(
                                        $"{splited} bundles splited into {first_bundle_item.bundle.name}"
                                    ),
                                    NotifyFadeOutTime
                                );
                                Content.SaveToAsset();
                                Reload();
                            }
                        }
                    );
                }

                if (!Multiselected)
                {
                    menu.AddItem(
                        new GUIContent("Tool/Slice Bundle"),
                        false,
                        delegate
                        {
                            OpenSliceMenu(addon_item.addon, bundle_item.bundle);
                            Content.SaveToAsset();
                            Reload();
                        }
                    );
                }
                else
                {
                    menu.AddDisabledItem(new GUIContent("Tool/Slice Bundle"));
                }

                var compression = Content
                    .Get()
                    .GetCompressionType(addon_item.addon.Name, bundle_item.bundle.name);
                menu.AddItem(
                    new GUIContent("Compression/None"),
                    compression == CompressionType.None,
                    delegate
                    {
                        Content
                            .Get()
                            .SetCompressionType(
                                addon_item.addon.Name,
                                bundle_item.bundle.name,
                                CompressionType.None
                            );
                        Content.SaveToAsset();
                    }
                );
                menu.AddItem(
                    new GUIContent("Compression/LZMA - Max. compression, slow loading"),
                    compression == CompressionType.Lzma,
                    delegate
                    {
                        Content
                            .Get()
                            .SetCompressionType(
                                addon_item.addon.Name,
                                bundle_item.bundle.name,
                                CompressionType.Lzma
                            );
                        Content.SaveToAsset();
                    }
                );
                menu.AddItem(
                    new GUIContent("Compression/LZ4 - Chunk Based (Recommended!)"),
                    compression == CompressionType.Lz4,
                    delegate
                    {
                        Content
                            .Get()
                            .SetCompressionType(
                                addon_item.addon.Name,
                                bundle_item.bundle.name,
                                CompressionType.Lz4
                            );
                        Content.SaveToAsset();
                    }
                );
                menu.AddDisabledItem(new GUIContent("Compression/LZ4HC - Obsolete"));
            }

            /* ASSET ITEM */
            if (item is AssetTreeViewItem)
            {
                var bundle_item = item.parent as BundleTreeViewItem;
                var addon_item = bundle_item.parent as AddonTreeViewItem;

                if (bundle_item != null)
                {
                    var asset_item = item as AssetTreeViewItem;

                    menu.AddItem(
                        new GUIContent("Tool/Put in dependencies"),
                        false,
                        delegate
                        {
                            Content
                                .Get()
                                .PutInDependenciesOfAsset(asset_item.asset.guid, addon_item.addon);
                            Content.SaveToAsset();
                            Reload();
                        }
                    );
                }
            }

            menu.ShowAsContext();
        }

        /* Drag & Drop */

        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            args.draggedItemIDs = GetSelection();

            foreach (var id in args.draggedItemIDs)
            {
                var item = FindItem(id, rootItem);

                //restrict drag&drop addons
                if (item is AddonTreeViewItem)
                {
                    return false;
                }
            }

            return true;
        }

        private bool DragAndDropInWindow = false;

        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            DragAndDrop.PrepareStartDrag();
            var selectedItems = new List<AssetTreeViewItem>();
            for (var i = 0; i < args.draggedItemIDs.Count; i++)
            {
                var item = FindItem(args.draggedItemIDs[i], rootItem);

                if (item is AssetTreeViewItem)
                {
                    selectedItems.Add(item as AssetTreeViewItem);
                }
            }

            DragAndDrop.paths = selectedItems.Select(a => a.asset.guid).ToArray();
            DragAndDropInWindow = selectedItems.Count > 0;
            DragAndDrop.StartDrag("Content Drag&Drop");
        }

        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
            var paths = DragAndDrop.paths;

            //drag&drop from another window
            if (!DragAndDropInWindow)
            {
                if (args.performDrop)
                {
                    OutsideWindow_DropAsset(args, paths);
                }
            }
            //drag&drop inside this window (move assets/bundles)
            else
            {
                if (args.performDrop)
                {
                    InsideWindow_DropAsset(args, paths);
                }
            }
            Reload();
            return DragAndDropVisualMode.Move;
        }

        void OutsideWindow_DropAsset(DragAndDropArgs args, string[] paths)
        {
            if (args.parentItem is BundleTreeViewItem)
            {
                var bundle_item = args.parentItem as BundleTreeViewItem;

                foreach (var path in paths)
                {
                    var UnityObject = AssetDatabase.LoadMainAssetAtPath(path);
                    var status = bundle_item.bundle.AddAsset(UnityObject);

                    if (status == Content.Status.OK)
                    {
                        Window.ShowNotification(
                            new GUIContent(
                                $"{UnityObject.name} added to {bundle_item.bundle.name}"
                            ),
                            NotifyFadeOutTime
                        );
                    }
                    else
                    {
                        Window.ShowNotification(
                            new GUIContent(
                                $"{UnityObject.name} add failed! {Content.StatusToString(status)}"
                            ),
                            NotifyFadeOutTime
                        );
                    }
                }
                Content.SaveToAsset();
            }

            //if throw on asset (if miss) then throw it in bundle
            if (args.parentItem is AssetTreeViewItem)
            {
                if (args.parentItem.parent is BundleTreeViewItem)
                {
                    var bundle_item = args.parentItem.parent as BundleTreeViewItem;

                    foreach (var path in paths)
                    {
                        var UnityObject = AssetDatabase.LoadMainAssetAtPath(path);
                        var status = bundle_item.bundle.AddAsset(UnityObject);
                        if (status == Content.Status.OK)
                        {
                            Window.ShowNotification(
                                new GUIContent(
                                    $"{UnityObject.name} added to {bundle_item.bundle.name}"
                                ),
                                NotifyFadeOutTime
                            );
                        }
                        else
                        {
                            Window.ShowNotification(
                                new GUIContent(
                                    $"{UnityObject.name} add failed! {Content.StatusToString(status)}"
                                ),
                                NotifyFadeOutTime
                            );
                        }
                    }
                    Content.SaveToAsset();
                }
            }
        }

        void InsideWindow_DropAsset(DragAndDropArgs args, string[] paths)
        {
            //drop in exists bundle
            if (args.parentItem is BundleTreeViewItem)
            {
                var bundle_item = args.parentItem as BundleTreeViewItem;
                foreach (var guid in paths)
                {
                    if (
                        Content
                            .Get()
                            .HasAsset(
                                guid,
                                out var result_bundle,
                                out var result_asset,
                                Content.AssetSearchType.GUID
                            )
                    )
                    {
                        var status = Content.Get().MoveAsset(result_asset, bundle_item.bundle);
                        if (status == Content.Status.OK)
                        {
                            Window.ShowNotification(
                                new GUIContent(
                                    $"{result_asset.name} moved to {bundle_item.bundle.name}"
                                ),
                                NotifyFadeOutTime
                            );
                        }
                        else
                        {
                            Window.ShowNotification(
                                new GUIContent(
                                    $"{result_asset.name} moving failed! {Content.StatusToString(status)}"
                                ),
                                NotifyFadeOutTime
                            );
                        }
                    }
                }
                Content.SaveToAsset();
            }

            //if throw on asset (if miss) then throw it in bundle
            if (args.parentItem is AssetTreeViewItem)
            {
                var asset_item = args.parentItem as AssetTreeViewItem;

                if (asset_item.parent is BundleTreeViewItem)
                {
                    var bundle_item = asset_item.parent as BundleTreeViewItem;
                    foreach (var guid in paths)
                    {
                        if (
                            Content
                                .Get()
                                .HasAsset(
                                    guid,
                                    out var result_bundle,
                                    out var result_asset,
                                    Content.AssetSearchType.GUID
                                )
                        )
                        {
                            var status = Content.Get().MoveAsset(result_asset, bundle_item.bundle);
                            if (status == Content.Status.OK)
                            {
                                Window.ShowNotification(
                                    new GUIContent(
                                        $"{result_asset.name} moved to {bundle_item.bundle.name}"
                                    ),
                                    NotifyFadeOutTime
                                );
                            }
                            else
                            {
                                Window.ShowNotification(
                                    new GUIContent(
                                        $"{result_asset.name} moving failed! {Content.StatusToString(status)}"
                                    ),
                                    NotifyFadeOutTime
                                );
                            }
                        }
                    }
                    Content.SaveToAsset();
                }
            }
        }
    }
}

static class ContentEditorExtensions
{
    internal static IOrderedEnumerable<T> Order<T, TKey>(
        this IEnumerable<T> source,
        Func<T, TKey> selector,
        bool ascending
    )
    {
        if (ascending)
        {
            return source.OrderBy(selector);
        }
        else
        {
            return source.OrderByDescending(selector);
        }
    }

    internal static IOrderedEnumerable<T> ThenBy<T, TKey>(
        this IOrderedEnumerable<T> source,
        Func<T, TKey> selector,
        bool ascending
    )
    {
        if (ascending)
        {
            return source.ThenBy(selector);
        }
        else
        {
            return source.ThenByDescending(selector);
        }
    }
}
