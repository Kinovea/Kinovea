using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Kinovea.ScreenManager;

namespace Kinovea.Root
{
    public partial class FormPatterns : Form
    {
        public FormPatterns()
        {
            InitializeComponent();
            InitList();
        }

        private void InitList()
        {
            Dictionary<FilePatternContexts, string> symbols = FilePatternSymbols.Symbols;
            foreach (KeyValuePair<FilePatternContexts, string> pair in symbols)
            {
                string description = pair.Key.ToString();
                string symbol = pair.Value;
                ListViewItem item = new ListViewItem(new string[] { description, symbol });
                item.Tag = pair.Key;
                lvSymbols.Items.Add(item);
            }
        }
    }
}
