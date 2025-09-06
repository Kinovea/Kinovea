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

        private bool captureFileName = false;


        public FormInsertVariable(bool captureFileName = false)
        {
            InitializeComponent();
            this.Text = "Insert context variable";
            this.captureFileName = captureFileName;

            InitList();
        }

        private void InitList()
        {
            // Custom variables.
            Color varsColor = Color.AntiqueWhite;
            foreach (var vt in VariablesRepository.VariableTables)
            {
                foreach (var vn in vt.Value.VariableNames)
                {
                    string example = vt.Value.GetValue(vn);
                    AddItem(vn, example, vn, varsColor);
                }
            }

            // Date variables.
            Color dateColor = Color.LightCyan;
            DateTime now = DateTime.Now;
            AddItem("Date (ISO 8601)",  string.Format("{0:yyyy-MM-dd}", now),   "date",     dateColor);
            AddItem("Date (ISO 8601)",  string.Format("{0:yyyyMMdd}", now),     "dateb",    dateColor);
            AddItem("Year",             string.Format("{0:yyyy}", now),         "year",     dateColor);
            AddItem("Month",            string.Format("{0:MM}", now),           "month",    dateColor);
            AddItem("Day",              string.Format("{0:dd}", now),           "day",      dateColor);


            if (captureFileName)
            {
                // Time variables.
                Color timeColor = Color.Lavender;
                AddItem("Time (ISO 8601)", string.Format("{0:HHmmss}", now), "time", timeColor);
                AddItem("Hours", string.Format("{0:HH}", now), "hour", timeColor);
                AddItem("Minutes", string.Format("{0:mm}", now), "minute", timeColor);
                AddItem("Seconds", string.Format("{0:ss}", now), "second", timeColor);
                AddItem("Milliseconds", string.Format("{0:fff}", now), "millisecond", timeColor);

                // Camera variables.
                Color cameraColor = Color.MistyRose;
                AddItem("Camera alias", "webcam", "camalias", cameraColor);
                AddItem("Camera frame rate", "100.00", "camfps", cameraColor);
                AddItem("Received frame rate", "100.00", "recvfps", cameraColor);
            }

            // Start with nothing selected.
        }

        private void AddItem(string desc, string example, string var, Color color)
        {
            string quotedEx = string.Format("\"{0}\"", example);
            ListViewItem item = new ListViewItem(new string[] { desc, quotedEx, var });
            item.Tag = var;
            item.BackColor = color;
            lvSymbols.Items.Add(item);
        }

        private void lvSymbols_DoubleClick(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
