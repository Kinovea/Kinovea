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
        private TreeNode favoritesRoot;
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
            
            tv.FullRowSelect = true;    // Changes the width of the highlight.
            tv.HotTracking = true;      // Node under the mouse is highlighted.
            tv.HideSelection = false;   // Highlight selected item even if the selection is programmatic.

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

        public void Build(IEnumerable<BrowserLocation> favorites)
        {
            treeView.Nodes.Clear();
            AddComputerRoot();
            AddFavoritesRoot(favorites);
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

        private void AddFavoritesRoot(IEnumerable<BrowserLocation> favorites)
        {
            string text = "Favorites";
            favoritesRoot = AddRoot(text, favorites, false);
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
                // The "computer" root gets the "My Computer" icon and the favorites gets a generic folder icon.
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
                
                if (!forDrives)
                {
                    root.Expand();
                }
            }
            finally
            {
                treeView.EndUpdate();
            }

            return root;
        }


        public void UpdateFavorites(IEnumerable<BrowserLocation> favorites)
        {
            if (favoritesRoot == null)
                return;

            TreeNode selectedNode = treeView.SelectedNode;

            // Remember if the selected node is somewhere under the favorites root.
            bool selectionWasUnderFavorites = selectedNode != null && IsDescendantOf(selectedNode, favoritesRoot);
            BrowserLocation selectedLocation = selectionWasUnderFavorites ? selectedNode.Tag as BrowserLocation : null;

            // Create the nodes at once before modifying the tree.
            TreeNode[] nodes = favorites.Select(s => CreateNode(s, false)).ToArray();

            suppressSelectionEvent = true;
            treeView.BeginUpdate();
            try
            {
                favoritesRoot.Nodes.Clear();
                favoritesRoot.Nodes.AddRange(nodes);
                
                // Always expand the favorites root.
                favoritesRoot.Expand();

                // Reselect the selected path.
                // This may move to a different part of the tree.
                if (selectionWasUnderFavorites && selectedLocation != null)
                {
                    TryReveal(selectedLocation);
                }
            }
            finally
            {
                treeView.EndUpdate();
            }

            suppressSelectionEvent = false;
        }

        /// <summary>
        /// Make a TreeNode from a browser location.
        /// Does not inspect the sub-folders.
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

            TreeNode node = new TreeNode(name);
            node.Tag = location;
            node.ImageIndex = iconIndex;
            node.SelectedImageIndex = iconIndexSelected;

            // Don't inspect the sub-folders while constructing the node.
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

            if (!suppressSelectionEvent)
            {
                LocationSelected?.Invoke(this, new EventArgs<BrowserLocation>(location));
            
                // Expand as soon as selected.
                e.Node.Expand();
            }
        }
        #endregion

        #region Selection and expansion
        /// <summary>
        /// Expands the passed node to the given location. 
        /// Build-expand children, scrolls it into view, and expand it.
        /// Returns true if the node was found and selected.
        /// This will find a path even it's a sub-folder of a favorite.
        /// </summary>
        private bool ExpandToPath(TreeNode startNode, BrowserLocation location)
        {
            if (startNode == null || location == null || !location.IsFileSystem)
                return false;

            string targetPath = NormalizePath(location.Path);
            TreeNode currentNode = startNode;

            while (currentNode != null)
            {
                BrowserLocation currentLocation = currentNode.Tag as BrowserLocation;

                if (currentLocation != null && currentLocation.IsFileSystem)
                {
                    string currentPath = NormalizePath(currentLocation.Path);

                    if (string.Equals(currentPath, targetPath, StringComparison.OrdinalIgnoreCase))
                    {
                        treeView.SelectedNode = currentNode;
                        currentNode.EnsureVisible();
                        return true;
                    }

                    // Bail out if the target cannot exist below this node.
                    if (!IsDescendantOf(targetPath, currentPath))
                    {
                        return false;
                    }
                }

                // At this point we know the target is a descendant of the node.
                // Expand to look for the best child.
                // Expand() is synchronous, so the children are available when this call returns.
                currentNode.Expand();

                TreeNode nextNode = FindBestChildAncestor(currentNode, targetPath);

                if (nextNode == null)
                    return false;

                currentNode = nextNode;
            }

            return false;
        }

        /// <summary>
        /// Look for the location anywhere in the tree, building and expanding as needed.
        /// This version looks by priority:
        /// - currently selected node
        /// - in favorites, possibly as a sub-folder of a favorite.
        /// - in drives.
        /// </summary>
        public bool TryReveal(BrowserLocation location, bool suppress = false)
        {
            if (location == null || !location.IsFileSystem)
                return false;

            if (IsSelectedNode(location))
                return true;

            if (suppress)
                suppressSelectionEvent = true;

            bool found = ExpandToPath(favoritesRoot, location);
            suppressSelectionEvent = false;
            
            if (found)
            {
                return true;
            }

            if (suppress)
                suppressSelectionEvent = true;
            found = ExpandToPath(drivesRoot, location);
            suppressSelectionEvent = false;

            return found;
        }

        /// <summary>
        /// Look for the location anywhere under the favorites root and select it.
        /// </summary>
        public bool TryRevealInFavorites(BrowserLocation location)
        {
            if (location == null || !location.IsFileSystem)
                return false;

            bool found = ExpandToPath(favoritesRoot, location);
            
            return found;
        }

        /// <summary>
        /// Returns true if the passed location is the same as the currently selected node.
        /// </summary>
        private bool IsSelectedNode(BrowserLocation location)
        {
            TreeNode selectedNode = treeView.SelectedNode;
            
            if (selectedNode == null)
                return false;
            
            BrowserLocation selectedLocation = selectedNode.Tag as BrowserLocation;
            if (selectedLocation == null)
                return false;
            
            return PathsEqual(selectedLocation.Path, location.Path);
        }

        /// <summary>
        /// Find the child of a node that is an ancestor to the path.
        /// </summary>
        private TreeNode FindBestChildAncestor(TreeNode parent, string targetPath)
        {
            TreeNode bestMatch = null;
            int bestMatchLength = -1;

            foreach (TreeNode child in parent.Nodes)
            {
                BrowserLocation childLocation = child.Tag as BrowserLocation;
                if (childLocation == null || !childLocation.IsFileSystem)
                {
                    continue;
                }

                string childPath = NormalizePath(childLocation.Path);
                if (!IsDescendantOf(targetPath, childPath))
                {
                    continue;
                }

                if (childPath.Length > bestMatchLength)
                {
                    bestMatch = child;
                    bestMatchLength = childPath.Length;
                }
            }

            return bestMatch;
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
        /// Walk the tree upwards to see if the candidate is an ancestor of the node.
        /// </summary>
        private static bool IsDescendantOf(TreeNode node, TreeNode candidate)
        {
            for (TreeNode current = node; current != null; current = current.Parent)
            {
                if (ReferenceEquals(current, candidate))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true if the path is a descendant of the candidate.
        /// </summary>
        private static bool IsDescendantOf(string path, string candidate)
        {
            if (string.Equals(path, candidate, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (!path.StartsWith(candidate, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // At this point we have the following cases:
            // full path: C:\Videos\Foo
            // candidate: C:\           good.
            // candidate: C:\Videos     good.
            // candidate: C:\Video      wrong.

            char lastChar = candidate[candidate.Length - 1];
            bool candidateIsDrive = lastChar == Path.DirectorySeparatorChar || lastChar == Path.AltDirectorySeparatorChar;
            if (candidateIsDrive)
            {
                return true;
            }

            // Ensure we don't match C:\Video.
            // The next character after the matching prefix must be a directory separator.
            char nextChar = path[candidate.Length];
            bool isGood = nextChar == Path.DirectorySeparatorChar || nextChar == Path.AltDirectorySeparatorChar;
            return isGood;
        }

        private static bool PathsEqual(string left, string right)
        {
            if (left == null || right == null)
                return false;

            return string.Equals(NormalizePath(left), NormalizePath(right), StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizePath(string path)
        {
            string fullPath = Path.GetFullPath(path);
            fullPath = fullPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

            string rootPath = Path.GetPathRoot(fullPath);

            // Preserve the trailing separator for filesystem roots.
            if (string.Equals(fullPath, rootPath, StringComparison.OrdinalIgnoreCase))
            {
                return fullPath;
            }

            return fullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
    }
}