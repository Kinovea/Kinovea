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
    /// Form to select a context variable to insert in a pattern.
    /// </summary>
    public partial class FormInsertVariable : Form
    {
        public string SelectedVariable
        {
            get
            {
                if (lvSymbols.SelectedItems.Count == 0)
                    return null;

                return (string)lvSymbols.SelectedItems[0].Tag;
            }
        }

        public FormInsertVariable()
        {
            InitializeComponent();

            this.Text = "Insert context variable";

            InitList();
        }

        private void InitList()
        {
            // Add built-in variables.
            DateTime now = DateTime.Now;
            AddItem("Date (ISO 8601)",  string.Format("{0:yyyy-MM-dd}", now),   "date");
            AddItem("Date (ISO 8601)",  string.Format("{0:yyyyMMdd}", now),     "dateb");
            AddItem("Year",             string.Format("{0:yyyy}", now),         "year");
            AddItem("Month",            string.Format("{0:MM}", now),           "month");
            AddItem("Day",              string.Format("{0:dd}", now),           "day");

            // TODO: Add custom variables.

            
            lvSymbols.Items[0].Selected = true;
        }

        private void AddItem(string desc, string example, string var)
        {
            ListViewItem item = new ListViewItem(new string[] { desc, example, var });
            item.Tag = var;
            lvSymbols.Items.Add(item);
        }
    }
}
