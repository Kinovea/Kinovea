using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Kinovea.ScreenManager.Languages;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Dialog to configure time section names for the advanced stopwatch.
    /// </summary>
    public partial class FormTimeSections : Form
    {
        private DrawingChronoMulti drawing;
        private HistoryMementoModifyDrawing memento;
        private int currentIndex = -1;
        private IDrawingHostView hostView;

        public FormTimeSections(DrawingChronoMulti drawing, int currentIndex, IDrawingHostView hostView)
        {
            this.drawing = drawing;
            this.currentIndex = currentIndex;
            this.hostView = hostView;

            memento = new HistoryMementoModifyDrawing(
                drawing.ParentMetadata, 
                drawing.ParentMetadata.ChronoManager.Id, 
                drawing.Id, 
                drawing.Name, 
                SerializationFilter.Core);

            InitializeComponent();
            Populate();
        }

        private void Populate()
        {
            this.Text = "Time sections";

            // Create a pair of textbox + label with timecodes.
            // Move the current section indicator.
            Metadata metadata = drawing.ParentMetadata;
            List<VideoSection> sections = drawing.VideoSections;
            List<string> names = drawing.SectionNames;
            List<string> tags = drawing.SectionTags;
            long cumulativeTimestamps = 0;

            int top = 40;
            int margin = 10;
            for (int i = 0; i < sections.Count; i++)
            {
                VideoSection section = sections[i];
                string name = names[i];
                string tag = tags[i];

                string end = "";
                string elapsed = "";
                string cumul = "";
                bool openEnded = section.End == long.MaxValue;
                
                string start = metadata.TimeCodeBuilder(section.Start, TimeType.Absolute, TimecodeFormat.Unknown, true);

                if (!openEnded)
                {
                    long elapsedTimestamps = section.End - section.Start;
                    cumulativeTimestamps += elapsedTimestamps; 
                    elapsed = metadata.TimeCodeBuilder(elapsedTimestamps, TimeType.Absolute, TimecodeFormat.Unknown, true);
                    cumul = metadata.TimeCodeBuilder(cumulativeTimestamps, TimeType.Absolute, TimecodeFormat.Unknown, true);
                    end = metadata.TimeCodeBuilder(section.End, TimeType.Absolute, TimecodeFormat.Unknown, true);
                }
                
                TextBox tbName = new TextBox();
                tbName.Location = new Point(65, top);
                tbName.Size = new Size(120, 20);
                tbName.Text = string.IsNullOrEmpty(name) ? (i + 1).ToString() : name;

                TextBox tbTag = new TextBox();
                tbTag.Location = new Point(tbName.Right + 10, top);
                tbTag.Size = new Size(80, 20);
                tbTag.Text = tag;

                Label lbl = new Label();
                lbl.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                lbl.Location = new Point(tbTag.Right + 10, top + 5);
                lbl.AutoSize = true;

                if (openEnded)
                    lbl.Text = string.Format("{0}", start);
                else
                    lbl.Text = string.Format("{0} -> {1} | {2} | {3}", start, end, elapsed, cumul);

                // Text box event handlers.
                int index = i;
                tbName.TextChanged += (s, e) => {
                    drawing.SectionNames[index] = tbName.Text;
                    if (hostView != null)
                        hostView.InvalidateFromMenu();
                };

                tbTag.TextChanged += (s, e) =>
                {
                    drawing.SectionTags[index] = tbTag.Text;
                    if (hostView != null)
                        hostView.InvalidateFromMenu();
                };

                grpConfig.Controls.Add(tbName);
                grpConfig.Controls.Add(lbl);
                grpConfig.Controls.Add(tbTag);

                if (currentIndex == i)
                    btnIndicator.Top = top - 2;

                top += lbl.Height + margin;
            }

            btnIndicator.Visible = currentIndex >= 0;

            grpConfig.Height = top + 10;
            btnOK.Top = grpConfig.Bottom + 5;
            btnCancel.Top = btnOK.Top;

            int borderTop = this.Height - this.ClientRectangle.Height;
            this.Height = borderTop + btnOK.Bottom + 10;

        }

        #region OK/Cancel/Exit
        private void btnCancel_Click(object sender, EventArgs e)
        {
            Cancel();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            // Commit the original state to the undo history stack.
            memento.UpdateCommandName(drawing.Name);
            drawing.ParentMetadata.HistoryStack.PushNewCommand(memento);
        }

        private void Cancel()
        {
            memento.PerformUndo();
        }
        #endregion

        private void FormTimeSections_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.DialogResult == DialogResult.OK)
                return;

            memento.PerformUndo();
        }
    }
}
