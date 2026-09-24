using Kinovea.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Kinovea.FileBrowser
{
    public class BrowserTreeController : IDisposable
    {
        public event EventHandler<EventArgs<BrowserLocation>> LocationSelected;

        #region Members
        private bool suppressSelectionEvent;
        private bool showHiddenFolders;
        private readonly TreeView treeView;
        private TreeNode drivesRoot;
        private TreeNode shortcutsRoot;
        private static readonly Guid ComputerFolderId = new Guid("0AC0837C-BBF8-452A-850D-79D08E667CA7");
        private static readonly object DummyTag = new object();
        #endregion

        #region Construction/Destruction
        public BrowserTreeController(TreeView tv)
        {
            this.treeView = tv;

            ShellSystemImageList.Attach(tv);

            tv.BeforeExpand += TreeView_BeforeExpand;
            tv.AfterSelect += TreeView_AfterSelect;

            tv.AllowDrop = false;
            tv.ItemHeight = 20;
            tv.ShowLines = false;
            tv.FullRowSelect = true;
            tv.HotTracking = true;
            tv.Indent = 20;
            tv.KeyDown += (s, e) =>
            {
                // Disable the * key to expand all nodes.
                if (e.KeyCode == Keys.Multiply)
                    e.Handled = true;
            };
        }
        public void Dispose()
        {
            treeView.BeforeExpand -= TreeView_BeforeExpand;
            treeView.AfterSelect -= TreeView_AfterSelect;
        }
        #endregion

        #region Building the treeview nodes

        public void Build(IEnumerable<BrowserLocation> shortcuts)
        {
            treeView.Nodes.Clear();
            AddComputerRoot();
            AddShortcutsRoot(shortcuts);
        }

        private void AddComputerRoot()
        {
            string[] drives;

            try
            {
                // This includes mapped network drives.
                drives = Directory.GetLogicalDrives();
                Array.Sort(drives, StringComparer.OrdinalIgnoreCase);
            }
            catch
            {
                drives = new string[0];
            }

            List<BrowserLocation> driveLocations = drives.Select(d => new BrowserLocation(d)).ToList();

            string text = GetComputerDisplayName();
            drivesRoot = AddRoot(text, driveLocations, true);
        }

        private void AddShortcutsRoot(IEnumerable<BrowserLocation> shortcuts)
        {
            string text = "Shortcuts";
            shortcutsRoot = AddRoot(text, shortcuts, false);
        }

        /// <summary>
        /// Add a root and its immediate children, returns the root.
        /// </summary>
        private TreeNode AddRoot(string name, IEnumerable<BrowserLocation> locations, bool forDrives)
        {
            treeView.BeginUpdate();

            TreeNode root = null;

            try
            {
                // This is a purely visual root, not a filesystem path.
                // The "computer" root gets the "My Computer" icon and the shortcuts gets a generic folder icon.
                int fallbackIcon = ShellIconIndex.Get("dummy-folder", false, false);
                int icon = 0;
                int selectedIcon = 0;
                if (forDrives)
                {
                    icon = ShellIconIndex.GetStockIconIndex(NativeMethods.StockIconId.DesktopPc, fallbackIcon);
                    selectedIcon = icon;
                }
                else
                {
                    int fallbackIconOpen = ShellIconIndex.Get("dummy-folder", true, false);
                    icon = ShellIconIndex.GetStockIconIndex(NativeMethods.StockIconId.Folder, fallbackIcon);
                    selectedIcon = ShellIconIndex.GetStockIconIndex(NativeMethods.StockIconId.FolderOpen, fallbackIconOpen);
                }

                root = new TreeNode(name);
                root.Tag = null;
                root.ImageIndex = icon;
                root.SelectedImageIndex = selectedIcon;

                foreach (BrowserLocation location in locations)
                {
                    if (location.Type == BrowserLocationType.FileSystem)
                    {
                        string path = location.Path;
                        if (!string.IsNullOrWhiteSpace(path))
                        {
                            root.Nodes.Add(CreateNode(location, forDrives));
                        }
                    }
                    else
                    {
                        // Skip non-filesystem locations for now.
                    }
                }

                treeView.Nodes.Add(root);
                root.Expand();
            }
            finally
            {
                treeView.EndUpdate();
            }

            return root;
        }

        /// <summary>
        /// Add a new child to the root node if it doesn't already exist.
        /// This should only be used to add shortcuts.
        /// </summary>
        public void AddShortcut(BrowserLocation location)
        {
            if (location.Type != BrowserLocationType.FileSystem)
                return;

            string path = location.Path;
            if (string.IsNullOrWhiteSpace(path))
                return;

            // Look for the path in the root's children. If not found, add it.
            TreeNode foundNode = FindChildByPath(shortcutsRoot.Nodes, location);

            if (foundNode == null)
            {
                treeView.BeginUpdate();
                TreeNode newNode = CreateNode(location, false);
                shortcutsRoot.Nodes.Insert(0, newNode);
                treeView.EndUpdate();
            }
        }

        /// <summary>
        /// Remove a child of the shortcuts root if it exists.
        /// </summary>
        public void RemoveShortcut(BrowserLocation location)
        {
            if (location == null || location.Type != BrowserLocationType.FileSystem)
                return;

            // Look for the path in the root's children. If found, remove it.
            TreeNode foundNode = FindChildByPath(shortcutsRoot.Nodes, location);
            if (foundNode != null)
            {
                treeView.BeginUpdate();
                shortcutsRoot.Nodes.Remove(foundNode);
                treeView.EndUpdate();
            }
        }

        /// <summary>
        /// Make a TreeNode from a browser location.
        /// </summary>
        private TreeNode CreateNode(BrowserLocation location, bool isDrive)
        {
            bool isRecentFiles = location.Type == BrowserLocationType.RecentFiles;

            string name = isRecentFiles ? "Recent files" : GetDisplayName(location.Path, isDrive);

            int iconIndex = 0;
            int iconIndexSelected = 0;

            if (isRecentFiles)
            {
                iconIndex = ShellIconIndex.GetStockIconIndex(NativeMethods.StockIconId.Folder, 0);
                iconIndexSelected = iconIndex;
            }
            else if (isDrive)
            {
                string path = location.Path;
                iconIndex = GetDriveIconIndex(path);
                iconIndexSelected = iconIndex;
            }
            else
            {
                string path = location.Path;
                iconIndex = ShellIconIndex.Get(path, false, isDrive);
                iconIndexSelected = ShellIconIndex.Get(path, true, isDrive);
            }

            TreeNode node = new TreeNode(name)
            {
                Tag = location,
                ImageIndex = iconIndex,
                SelectedImageIndex = iconIndexSelected
            };

            // We don't inspect the sub-folders while constructing the node.
            // Add a dummy child to give it an expansion glyph.
            if (!isRecentFiles)
            {
                node.Nodes.Add(new TreeNode { Tag = DummyTag });
            }
            
            return node;
        }

        /// <summary>
        /// Build the children of a node if not done already.
        /// </summary>
        private void BuildChildren(TreeNode node)
        {
            BrowserLocation location = node.Tag as BrowserLocation;
            if (location == null)
                return;

            List<string> directoryPaths;
            if (!TryGetChildDirectories(location, out directoryPaths))
                return;

            HashSet<string> wantedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string path in directoryPaths)
            {
                wantedPaths.Add(NormalizePath(path));
            }

            // Remove the dummy node and directories that no longer exist.
            for (int i = node.Nodes.Count - 1; i >= 0; i--)
            {
                TreeNode child = node.Nodes[i];
                BrowserLocation childLocation = child.Tag as BrowserLocation;
                string childPath = childLocation?.Path;

                if (childLocation == null || childPath == null || !wantedPaths.Contains(NormalizePath(childPath)))
                {
                    node.Nodes.RemoveAt(i);
                }
            }

            // Index the nodes that remain.
            // Keeping these preserves their known children and expansion state.
            Dictionary<string, TreeNode> existingNodes = new Dictionary<string, TreeNode>(StringComparer.OrdinalIgnoreCase);
            foreach (TreeNode child in node.Nodes)
            {
                BrowserLocation childLocation = child.Tag as BrowserLocation;
                string childPath = childLocation?.Path;
                if (childPath != null)
                { 
                    existingNodes[NormalizePath(childPath)] = child;
                }
            }

            // directoryPaths is already sorted. Insert only newly discovered nodes.
            for (int i = 0; i < directoryPaths.Count; i++)
            {
                string directoryPath = directoryPaths[i];
                string key = NormalizePath(directoryPath);

                if (existingNodes.ContainsKey(key))
                    continue;

                BrowserLocation newLocation = new BrowserLocation(directoryPath);
                TreeNode newNode = CreateNode(newLocation, false);

                // Inserting at the corresponding position preserves alphabetical
                // ordering without moving the existing nodes.
                node.Nodes.Insert(i, newNode);
                existingNodes.Add(key, newNode);
            }

            return;
        }
        
        private static string GetDisplayName(string path, bool isDrive)
        {
            if (string.IsNullOrWhiteSpace(path))
                return string.Empty;

            string trimmed = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            if (isDrive)
                return trimmed;

            string name = Path.GetFileName(trimmed);
            return string.IsNullOrEmpty(name) ? trimmed : name;
        }

        /// <summary>
        /// Returns the display name of the virtual folder "Computer" (or "This PC").
        /// </summary>
        private static string GetComputerDisplayName()
        {
            const string fallback = "Computer";

            IntPtr pidl = IntPtr.Zero;

            try
            {
                // Get the PIDL of the Computer folder.
                Guid folderId = ComputerFolderId;
                int result = NativeMethods.SHGetKnownFolderIDList(ref folderId, 0, IntPtr.Zero, out pidl);
                if (result < 0 || pidl == IntPtr.Zero)
                    return fallback;

                // Get the display name.
                NativeMethods.SHFILEINFO info = new NativeMethods.SHFILEINFO();
                IntPtr shellResult = NativeMethods.SHGetFileInfo(
                    pidl,
                    0,
                    ref info,
                    (uint)Marshal.SizeOf(typeof(NativeMethods.SHFILEINFO)),
                    NativeMethods.SHGFI_PIDL | NativeMethods.SHGFI_DISPLAYNAME);

                if (shellResult == IntPtr.Zero || string.IsNullOrWhiteSpace(info.szDisplayName))
                {
                    return fallback;
                }

                return info.szDisplayName;
            }
            catch (DllNotFoundException)
            {
                return fallback;
            }
            catch (EntryPointNotFoundException)
            {
                return fallback;
            }
            finally
            {
                if (pidl != IntPtr.Zero)
                {
                    NativeMethods.ILFree(pidl);
                }
            }
        }

        /// <summary>
        /// Get the icon index for a drive.
        /// </summary>
        private int GetDriveIconIndex(string drivePath)
        {
            int genericIcon = ShellIconIndex.GetStockIconIndex(NativeMethods.StockIconId.DriveFixed, 0);

            if (!PreferencesManager.FileExplorerPreferences.UseDriveIcons)
                return genericIcon;

            return ShellIconIndex.Get(drivePath, false, true, genericIcon);
        }

        #endregion

        #region Event handlers
        private void TreeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            BuildChildren(e.Node);
        }

        private void TreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            BrowserLocation location = e.Node.Tag as BrowserLocation;
            if (location == null)
                return;

            // Always expand as soon as selected.
            e.Node.Expand();

            if (!suppressSelectionEvent)
            {
                LocationSelected?.Invoke(this, new EventArgs<BrowserLocation>(location));
            }
        }
        #endregion

        #region Selection and expansion
        /// <summary>
        /// Expands the drives sub-tree to the given folder. 
        /// Selects the target folder, scrolls it into view, and expand it.
        /// This is used to synchronize the drives tree view with the shortcuts.
        /// </summary>
        public bool ExpandToPath(BrowserLocation location, bool notifySelection = false)
        {
            if (location == null || location.Type != BrowserLocationType.FileSystem)
                return false;

            string path = location.Path;
            if (string.IsNullOrWhiteSpace(path))
                return false;

            string targetPath;
            string rootPath;

            try
            {
                targetPath = NormalizePath(path);
                rootPath = NormalizePath(Path.GetPathRoot(targetPath));
            }
            catch (Exception ex) when (
                ex is ArgumentException ||
                ex is NotSupportedException ||
                ex is PathTooLongException)
            {
                return false;
            }

            if (treeView.Nodes.Count == 0)
                return false;

            // Find C:\, D:\, mapped Z:\, etc.
            TreeNode currentNode = FindChildByPath(drivesRoot.Nodes, location);

            if (currentNode == null)
                return false;

            // Build a stack of path parts:
            //
            // C:\
            // C:\Users
            // C:\Users\Name
            // C:\Users\Name\Videos
            //
            Stack<string> remainingPaths = new Stack<string>();
            string currentPath = targetPath;

            while (!PathsEqual(currentPath, rootPath))
            {
                remainingPaths.Push(currentPath);

                DirectoryInfo parent;

                try
                {
                    parent = Directory.GetParent(currentPath);
                }
                catch (Exception ex) when (
                    ex is ArgumentException ||
                    ex is NotSupportedException ||
                    ex is PathTooLongException)
                {
                    return false;
                }

                if (parent == null)
                    return false;

                string parentPath = NormalizePath(parent.FullName);

                // Protection against malformed paths.
                if (PathsEqual(parentPath, currentPath))
                    return false;

                currentPath = parentPath;
            }

            bool previousSuppression = suppressSelectionEvent;
            suppressSelectionEvent = !notifySelection;

            // Walk down the tree, expanding and building children as we go.
            treeView.BeginUpdate();

            try
            {
                drivesRoot.Expand();

                while (remainingPaths.Count > 0)
                {
                    BuildChildren(currentNode);
                    currentNode.Expand();

                    string nextPath = remainingPaths.Pop();
                    BrowserLocation loc = new BrowserLocation(nextPath);
                    TreeNode nextNode = FindChildByPath(currentNode.Nodes, loc);

                    if (nextNode == null)
                        return false;

                    currentNode = nextNode;
                }

                // Also expand the target itself.
                BuildChildren(currentNode);
                currentNode.Expand();

                // Select and scroll into view.
                treeView.SelectedNode = currentNode;
                currentNode.EnsureVisible();

                return true;
            }
            finally
            {
                treeView.EndUpdate();
                suppressSelectionEvent = previousSuppression;
            }
        }

        /// <summary>
        /// Selects an immediate child of the root node.
        /// This is used to select a shortcut based on the current video path.
        /// </summary>
        public void SelectRootChild(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
                return;

            BrowserLocation location = new BrowserLocation(folderPath);
            TreeNode currentNode = FindChildByPath(shortcutsRoot.Nodes, location);
            if (currentNode != null)
            {
                treeView.SelectedNode = currentNode;
                currentNode.EnsureVisible();
            }
        }

        #endregion

        /// <summary>
        /// Get the sub-folders of the passed location.
        /// Returns true if the operation was successful even if the result is empty.
        /// </summary>
        private bool TryGetChildDirectories(BrowserLocation location, out List<string> result)
        {
            result = new List<string>();
                
            if (location.Type == BrowserLocationType.RecentFiles)
            {
                // Recent files doesn't have any sub-folders, only files. Return an empty list.
                return true;
            }

            if (string.IsNullOrWhiteSpace(location.Path))
            {
                return true;
            }

            try
            {
                string parentPath = location.Path;
                foreach (string childPath in Directory.GetDirectories(parentPath))
                {
                    try
                    {
                        string name = Path.GetFileName(childPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

                        if (name.StartsWith(".", StringComparison.Ordinal))
                            continue;

                        FileAttributes attributes = File.GetAttributes(childPath);

                        if (!showHiddenFolders && (attributes & FileAttributes.Hidden) != 0)
                        {
                            continue;
                        }

                        result.Add(childPath);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Skip this individual entry.
                    }
                    catch (IOException)
                    {
                        // It disappeared or its attributes are unavailable.
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Parent cannot be enumerated.
            }
            catch (DirectoryNotFoundException)
            {
                // Disconnected drive or deleted directory.
            }
            catch (IOException)
            {
                // Unavailable network/removable drive.
            }

            result.Sort((left, right) =>
                StringComparer.CurrentCultureIgnoreCase.Compare(
                    GetDisplayName(left, false), 
                    GetDisplayName(right, false)));

            return true;
        }


        /// <summary>
        /// Returns the child node matching the given path.
        /// </summary>
        private static TreeNode FindChildByPath(TreeNodeCollection nodes, BrowserLocation queryLocation)
        {
            if (queryLocation == null || queryLocation.Type != BrowserLocationType.FileSystem)
                return null;

            foreach (TreeNode node in nodes)
            {
                BrowserLocation location = node.Tag as BrowserLocation;
                string nodePath = location?.Path;
                string queryPath = queryLocation.Path;
                if (nodePath != null && PathsEqual(nodePath, queryPath))
                {
                    return node;
                }
            }

            return null;
        }

        private static bool PathsEqual(string left, string right)
        {
            if (left == null || right == null)
                return false;

            return string.Equals(
                NormalizePath(left).TrimEnd('\\', '/'),
                NormalizePath(right).TrimEnd('\\', '/'),
                StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizePath(string path)
        {
            string fullPath = Path.GetFullPath(path);
            string rootPath = Path.GetPathRoot(fullPath);

            // Preserve the trailing separator for filesystem roots.
            if (string.Equals(
                fullPath.TrimEnd('\\', '/'),
                rootPath.TrimEnd('\\', '/'),
                StringComparison.OrdinalIgnoreCase))
            {
                return rootPath;
            }

            return fullPath.TrimEnd('\\', '/');
        }
    }
}