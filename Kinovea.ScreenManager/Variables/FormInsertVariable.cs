using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Kinovea.ScreenManager.Languages;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Form to select a context variable to insert in a pattern.
    /// </summary>
    public partial class FormInsertVariable : Form
    {
        #region Private types
        private class ListViewVariable
        {
            public string Description { get; set; }
            public string Example { get; set; }
            public string Keyword { get; set; }

            public Color Color { get; set; }
        }
        #endregion

        public string SelectedVariable
        {
            get
            {
                return ((ListViewVariable)olvVariables.SelectedObject)?.Keyword;
            }
        }

        public FormInsertVariable(ContextVariableCategory categories, string path = null)
        {
            InitializeComponent();
            this.Text = "Insert context variable";
            btnOk.Text = "Insert";
            btnCancel.Text = ScreenManagerLang.Generic_Cancel;
            PrepareListView();
            InitList(categories, path);
        }

        /// <summary>
        /// Prepare the object list view control columns.
        /// </summary>
        private void PrepareListView()
        {
            var colDesc = new OLVColumn();
            colDesc.AspectName = "Description";
            colDesc.Groupable = false;
            colDesc.Sortable = false;
            colDesc.IsEditable = false;
            colDesc.MinimumWidth = 200;
            colDesc.TextAlign = HorizontalAlignment.Left;
            colDesc.Text = "Description";
            

            var colExample = new OLVColumn();
            colExample.AspectName = "Example";
            colExample.Groupable = false;
            colExample.Sortable = false;
            colExample.IsEditable = false;
            colExample.MinimumWidth = 100;
            colExample.FillsFreeSpace = true;
            colExample.TextAlign = HorizontalAlignment.Left;
            colExample.Text = "Example";

            var colKeyword = new OLVColumn();
            colKeyword.AspectName = "Keyword";
            colKeyword.Groupable = false;
            colKeyword.Sortable = false;
            colKeyword.IsEditable = false;
            colKeyword.MinimumWidth = 100;
            colKeyword.MaximumWidth = 100;
            colKeyword.TextAlign = HorizontalAlignment.Left;
            colKeyword.Text = "Keyword";

            olvVariables.AllColumns.AddRange(new OLVColumn[] {
                colDesc,
                colExample,
                colKeyword,
                });

            olvVariables.Columns.AddRange(new ColumnHeader[] {
                colDesc,
                colExample,
                colKeyword,
                });

            olvVariables.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            olvVariables.RowHeight = 18;
            olvVariables.FullRowSelect = true;
            olvVariables.GridLines = true;
        }

        private void olvVariables_FormatRow(object sender, FormatRowEventArgs e)
        {
            ListViewVariable v = (ListViewVariable)e.Model;
            e.Item.BackColor = v.Color;
        }

        private void InitList(ContextVariableCategory categories, string path = null)
        {
            olvVariables.Items.Clear();
           
            List<ListViewVariable> rows = new List<ListViewVariable>();
            DateTime now = DateTime.Now;

            // Check if flag is active in bitmask enum.
            if ((categories & ContextVariableCategory.Custom) == ContextVariableCategory.Custom)
            {
                Color varsColor = Color.AntiqueWhite;
                foreach (var vt in VariablesRepository.VariableTables)
                {
                    foreach (var vn in vt.Value.VariableNames)
                    {
                        string example = vt.Value.GetValue(vn);
                        rows.Add(MakeRow(vn, example, vn, varsColor));
                    }
                }
            }

            //--------------------------------------------
            // When changing the list here, the corresponding implementation needs to be adjusted.
            // This would happen in the various Build*Context() functions.
            // ex: DynamicPathResolver.BuildDateContext(), CaptureScreen.BuildCaptureContext(), etc.
            //--------------------------------------------

            if ((categories & ContextVariableCategory.Camera) == ContextVariableCategory.Camera)
            {
                Color cameraColor = Color.MistyRose;
                rows.Add(MakeRow("Camera alias", "webcam", "camalias", cameraColor));
                rows.Add(MakeRow("Camera frame rate", "100.00", "camfps", cameraColor));
                rows.Add(MakeRow("Received frame rate", "100.00", "recvfps", cameraColor));
            }

            if ((categories & ContextVariableCategory.PostRecordingCommand) == ContextVariableCategory.PostRecordingCommand)
            {
                if (path == null)
                    path = "D:\\temp\\videos\\video.mp4";

                Color commandColor = Color.MistyRose;
                rows.Add(MakeRow("Path to video file", path, "filepath", commandColor));
                rows.Add(MakeRow("Path to capture folder", Path.GetDirectoryName(path), "folderpath", commandColor));
                rows.Add(MakeRow("File name with extension", Path.GetFileName(path), "filename.ext", commandColor));
                rows.Add(MakeRow("File name without extension", Path.GetFileNameWithoutExtension(path), "filename", commandColor));
                rows.Add(MakeRow("File extension", Path.GetExtension(path).Substring(1), "ext", commandColor));
                rows.Add(MakeRow("Annotation file name", Path.GetFileNameWithoutExtension(path) + ".kva", "kva", commandColor));
            }

            if ((categories & ContextVariableCategory.Date) == ContextVariableCategory.Date)
            {
                // Date variables.
                Color dateColor = Color.LightCyan;
                rows.Add(MakeRow("Date (ISO 8601)", string.Format("{0:yyyy-MM-dd}", now), "date", dateColor));
                rows.Add(MakeRow("Date (ISO 8601)", string.Format("{0:yyyyMMdd}", now), "dateb", dateColor));
                rows.Add(MakeRow("Year", string.Format("{0:yyyy}", now), "year", dateColor));
                rows.Add(MakeRow("Month", string.Format("{0:MM}", now), "month", dateColor));
                rows.Add(MakeRow("Day", string.Format("{0:dd}", now), "day", dateColor));
            }

            if ((categories & ContextVariableCategory.Time) == ContextVariableCategory.Time)
            {
                // Time variables.
                Color timeColor = Color.Lavender;
                rows.Add(MakeRow("Time (ISO 8601)", string.Format("{0:HHmmss}", now), "time", timeColor));
                rows.Add(MakeRow("Hours", string.Format("{0:HH}", now), "hour", timeColor));
                rows.Add(MakeRow("Minutes", string.Format("{0:mm}", now), "minute", timeColor));
                rows.Add(MakeRow("Seconds", string.Format("{0:ss}", now), "second", timeColor));
                rows.Add(MakeRow("Milliseconds", string.Format("{0:fff}", now), "millisecond", timeColor));
            }

            olvVariables.SetObjects(rows);
            olvVariables.Refresh();
            olvVariables.RedrawItems(0, olvVariables.GetItemCount() - 1, true);
            olvVariables.CellPadding = new Rectangle(5, 3, 3, 3);
            // Start with nothing selected.

            // For some reason the FormatRow event doesn't fire until the mouse is over the row.
            for (int i = 0; i < rows.Count; i++)
            {
                var item = olvVariables.GetItem(i);
                item.BackColor = ((ListViewVariable)item.RowObject).Color;
            }

        }

        private ListViewVariable MakeRow(string desc, string ex, string var, Color color)
        {
            string quotedEx = string.Format("\"{0}\"", ex);
            return new ListViewVariable() { 
                Description = desc,
                Example = quotedEx,
                Keyword = var,
                Color = color
            };
        }

        private void olvVariables_DoubleClick(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
