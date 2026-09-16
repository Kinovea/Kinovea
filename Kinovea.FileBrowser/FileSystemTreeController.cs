using Kinovea.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Kinovea.FileBrowser
{
    public class FileSystemTreeController : IDisposable
    {
        public event Action<string> SelectedPathChanged;

        #region Members
        private bool suppressSelectionEvent;
        private bool showHiddenFolders;
        private bool isDrives;
        private readonly TreeView treeView;
        private static readonly object DummyTag = new object();
        #endregion

        #region Construction/Destruction
        public FileSystemTreeController(TreeView tv)
        {
            this.treeView = tv;

            ShellSystemImageList.Attach(tv);

            tv.BeforeExpand += TreeView_BeforeExpand;
            tv.AfterSelect += TreeView_AfterSelect;

            tv.AllowDrop = false;
            tv.BorderStyle = BorderStyle.None;
            tv.ItemHeight = 20;
            tv.ShowLines = false;
            tv.ShowPlusMinus = true; // Can't get the chevron.
            tv.FullRowSelect = true;
            tv.HotTracking = false; // underline on hover.
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

        /// <summary>
        /// Build the treeview with the logical drives of the computer.
        /// </summary>
        public void BuildComputer()
        {
            isDrives = true;
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

            string rootText = "Computer";
            BuildRoot(rootText, drives, true);

        }

        /// <summary>
        /// Build the treeview with the user's favorite folders.
        /// </summary>
        public void BuildFavorites(IEnumerable<string> paths)
        {
            isDrives = false;
            string rootText = "Favorites";
            BuildRoot(rootText, paths, false);
        }

        /// <summary>
        /// Add the root and its immediate children.
        /// </summary>
        private void BuildRoot(string name, IEnumerable<string> paths, bool forDrives)
        {
            treeView.BeginUpdate();

            try
            {
                treeView.Nodes.Clear();

                // This is a purely visual root, no filesystem path.
                // The "explorer" tab gets the "Computer" icon and the shortcuts gets a generic folder icon.
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

                TreeNode root = new TreeNode(name)
                {
                    Tag = null,
                    ImageIndex = icon,
                    SelectedImageIndex = selectedIcon
                };

                foreach (string path in paths)
                {
                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        root.Nodes.Add(CreatePathNode(path, forDrives));
                    }
                }

                treeView.Nodes.Add(root);
                root.Expand();
            }
            finally
            {
                treeView.EndUpdate();
            }
        }

        /// <summary>
        /// Add a new child to the root node if it doesn't already exist.
        /// This should only be used to add shortcuts.
        /// </summary>
        public void AddRootChild(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
                return;

            // Look for the path in the root's children. If not found, add it.
            TreeNode rootNode = treeView.Nodes[0];
            TreeNode foundNode = FindChildByPath(rootNode.Nodes, folderPath);

            if (foundNode == null)
            {
                treeView.BeginUpdate();
                TreeNode newNode = CreatePathNode(folderPath, false);
                rootNode.Nodes.Insert(0, newNode);
                treeView.EndUpdate();
            }
        }

        /// <summary>
        /// Remove a child from the root node if it exists.
        /// This should only be used to remove the virtual shortcut.
        /// </summary>
        public void RemoveRootChild(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
                return;

            // Look for the path in the root's children. If found, remove it.
            TreeNode rootNode = treeView.Nodes[0];
            TreeNode foundNode = FindChildByPath(rootNode.Nodes, folderPath);
            if (foundNode != null)
            {
                treeView.BeginUpdate();
                rootNode.Nodes.Remove(foundNode);
                treeView.EndUpdate();
            }
        }

        /// <summary>
        /// Make one TreeNode from a file system path.
        /// </summary>
        private TreeNode CreatePathNode(string path, bool isDrive)
        {
            string name = GetDisplayName(path, isDrive);

            int iconIndex = 0;
            int iconIndexSelected = 0;

            if (isDrive)
            {
                iconIndex = GetDriveIconIndex(path);
                iconIndexSelected = iconIndex;
            }
            else
            {
                iconIndex = ShellIconIndex.Get(path, false, isDrive);
                iconIndexSelected = ShellIconIndex.Get(path, true, isDrive);
            }
            
            TreeNode node = new TreeNode(name)
            {
                Tag = path,
                ImageIndex = iconIndex,
                SelectedImageIndex = iconIndexSelected
            };

            // We don't inspect the directory while constructing the node.
            // Add a dummy child to give it an expansion glyph.
            node.Nodes.Add(new TreeNode { Tag = DummyTag });
            return node;
        }

        /// <summary>
        /// Build the children of a node if not done already.
        /// </summary>
        private bool BuildChildren(TreeNode node)
        {
            string parentPath = node.Tag as string;
            if (parentPath == null)
                return false;

            List<string> directoryPaths;

            if (!TryGetChildDirectories(parentPath, out directoryPaths))
                return false;

            HashSet<string> wantedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string path in directoryPaths)
            {
                wantedPaths.Add(NormalizePath(path));
            }

            // Remove the dummy node and directories that no longer exist.
            for (int i = node.Nodes.Count - 1; i >= 0; i--)
            {
                TreeNode child = node.Nodes[i];
                string childPath = child.Tag as string;

                if (childPath == null || !wantedPaths.Contains(NormalizePath(childPath)))
                {
                    node.Nodes.RemoveAt(i);
                }
            }

            // Index the nodes that remain.
            // Keeping these preserves their known children and expansion state.
            Dictionary<string, TreeNode> existingNodes = new Dictionary<string, TreeNode>(StringComparer.OrdinalIgnoreCase);
            foreach (TreeNode child in node.Nodes)
            {
                string childPath = child.Tag as string;

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

                TreeNode newNode = CreatePathNode(directoryPath, false);

                // Inserting at the corresponding position preserves alphabetical
                // ordering without moving the existing nodes.
                node.Nodes.Insert(i, newNode);
                existingNodes.Add(key, newNode);
            }

            return true;
        }
        
        private static string GetDisplayName(string path, bool isDrive)
        {
            string trimmed = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            if (isDrive)
                return trimmed;

            string name = Path.GetFileName(trimmed);
            return string.IsNullOrEmpty(name) ? trimmed : name;
        }

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
            string path = e.Node.Tag as string;
            if (path == null)
                return;

            // Always expand as soon as selected.
            e.Node.Expand();

            if (!suppressSelectionEvent)
            {
                SelectedPathChanged?.Invoke(path);
            }
        }

        #endregion

        #region Selection and expansion
        /// <summary>
        /// Expands the drives tree to the given folder. 
        /// Selects the target folder, scrolls it into view, and expand it.
        /// This is used to synchronize the drives tree view with the shortcuts.
        /// </summary>
        public bool ExpandToPath(string folderPath, bool notifySelection = false)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
                return false;

            if (!isDrives)
                return false;

            string targetPath;
            string rootPath;

            try
            {
                targetPath = NormalizePath(folderPath);
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

            TreeNode rootNode = treeView.Nodes[0];

            // Find C:\, D:\, mapped Z:\, etc.
            TreeNode currentNode = FindChildByPath(rootNode.Nodes, rootPath);

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
                rootNode.Expand();

                while (remainingPaths.Count > 0)
                {
                    BuildChildren(currentNode);
                    currentNode.Expand();

                    string nextPath = remainingPaths.Pop();

                    TreeNode nextNode = FindChildByPath(currentNode.Nodes, nextPath);

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

            TreeNode rootNode = treeView.Nodes[0];
            TreeNode currentNode = FindChildByPath(rootNode.Nodes, folderPath);

            if (currentNode != null)
            {
                treeView.SelectedNode = currentNode;
                currentNode.EnsureVisible();
            }
        }

        #endregion

        private bool TryGetChildDirectories(string parentPath, out List<string> result)
        {
            result = new List<string>();

            try
            {
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
                StringComparer.CurrentCultureIgnoreCase.Compare(GetDisplayName(left, false), GetDisplayName(right, false)));

            return true;
        }


        /// <summary>
        /// Returns the child node matching the given path.
        /// </summary>
        private static TreeNode FindChildByPath(TreeNodeCollection nodes, string path)
        {
            foreach (TreeNode node in nodes)
            {
                string nodePath = node.Tag as string;
                if (nodePath != null && PathsEqual(nodePath, path))
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