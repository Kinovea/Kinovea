using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Kinovea.Services;

namespace Kinovea.ExpTreeLib2
{
    /// <summary>
    /// This class is currently an empty stub matching the API of the existing ExpTree class.
    /// </summary>
    public partial class ExpTree2 : UserControl
    {
        #region Events
        /// <summary>
        /// Event raised before a node is visually expanded but after it has been filled in.
        /// </summary>
        public event EventHandler<TreeViewEventArgs> TreeViewBeforeExpand;
        public event EventHandler<EventArgs<CShItem2>> ExpTreeNodeSelected;
        #endregion

        #region Properties
        public string RootDisplayName { get; set; }
        public StartDir StartUpDirectory { get; set; }
        public CShItem2 SelectedItem { get; set; }
        public bool ShowHiddenFolders { get; set; }
        public bool ShowRootLines { get; set; }
        public bool ShortcutsMode { get; set; }
        #endregion

        public ExpTree2()
        {
            InitializeComponent();
        }

        public void ExpandANode(CShItem2 current)
        {
            
        }

        public void SelectNode(string lastOpenedDirectory)
        {
            
        }

        public void SetShortcuts(ArrayList arrayList)
        {
            
        }

        public bool IsOnSelectedItem(Point location)
        {
            return false;
        }
    }
}
