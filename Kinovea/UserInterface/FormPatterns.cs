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
    /// <summary>
    /// Simple form showing the map of symbols to meaning for a given symbol map.
    /// </summary>
    public partial class FormPatterns : Form
    {
        public FormPatterns(Dictionary<PatternContext, string> symbols)
        {
            InitializeComponent();

            this.Text = "   " + RootLang.dlgPreferences_Capture_ContextVariables;

            InitList(symbols);
        }

        private void InitList(Dictionary<PatternContext, string> symbols)
        {
            colPattern.Text = "Variable";
            colContext.Text = "Value";

            foreach (KeyValuePair<PatternContext, string> pair in symbols)
            {
                string refName = "dlgPreferences_Capture_Pattern" + pair.Key.ToString();
                string description = RootLang.ResourceManager.GetString(refName);
                if (string.IsNullOrEmpty(description))
                    description = pair.Key.ToString();

                string symbol = pair.Value;
                ListViewItem item = new ListViewItem(new string[] { symbol, description});
                item.Tag = pair.Key;
                lvSymbols.Items.Add(item);
            }
        }
    }
}
