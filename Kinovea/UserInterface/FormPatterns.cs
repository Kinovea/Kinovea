using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Kinovea.ScreenManager;
using Kinovea.Root.Languages;

namespace Kinovea.Root
{
    public partial class FormPatterns : Form
    {
        public FormPatterns()
        {
            InitializeComponent();

            this.Text = "   " + RootLang.dlgPreferences_Capture_ContextVariables;

            InitList();
        }

        private void InitList()
        {
            colContext.Text = RootLang.dlgPreferences_Capture_Context;
            colPattern.Text = RootLang.dlgPreferences_Capture_Macro;

            Dictionary<FilePatternContexts, string> symbols = FilePatternSymbols.Symbols;
            foreach (KeyValuePair<FilePatternContexts, string> pair in symbols)
            {
                string refName = "dlgPreferences_Capture_Pattern" + pair.Key.ToString();
                string description = RootLang.ResourceManager.GetString(refName);
                string symbol = pair.Value;
                ListViewItem item = new ListViewItem(new string[] { description, symbol });
                item.Tag = pair.Key;
                lvSymbols.Items.Add(item);
            }
        }
    }
}
