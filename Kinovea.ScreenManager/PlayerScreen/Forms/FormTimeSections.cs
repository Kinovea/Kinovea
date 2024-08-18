using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Kinovea.ScreenManager.Languages;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Dialog to configure time section names for the advanced stopwatch.
    /// 
    /// Undo/redo mechancis:
    /// - We build a memento of the state of the object before any changes.
    /// - Changes to the dialog impact the object immediately, this way we get feedback in the main screen.
    /// - On "Cancel" we revert back to this memento.
    /// - On "OK" we push the memento to the history stack to enable "undo" of all the changes
    /// that happened during the dialog.
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
            this.Text = ScreenManagerLang.mnuTimeSections;

            Metadata metadata = drawing.ParentMetadata;
            List<ChronoSection> sections = drawing.Sections;

            // Build the text for the time values.
            long cumulativeTimestamps = 0;
            for (int i = 0; i < sections.Count; i++)
            {
                ChronoSection section = sections[i];
                section.IsCurrent = (i == currentIndex);

                VideoSection timeSection = section.Section;
                bool openEnded = timeSection.End == long.MaxValue;
                section.Start = metadata.TimeCodeBuilder(timeSection.Start, TimeType.Absolute, TimecodeFormat.Unknown, true);
                if (openEnded)
                {
                    section.Duration = "";
                    section.Cumul = "";
                    section.End = "";
                }
                else
                { 
                    long elapsedTimestamps = timeSection.End - timeSection.Start;
                    cumulativeTimestamps += elapsedTimestamps; 
                    section.Duration = metadata.TimeCodeBuilder(elapsedTimestamps, TimeType.Absolute, TimecodeFormat.Unknown, true);
                    section.Cumul = metadata.TimeCodeBuilder(cumulativeTimestamps, TimeType.Absolute, TimecodeFormat.Unknown, true);
                    section.End = metadata.TimeCodeBuilder(timeSection.End, TimeType.Absolute, TimecodeFormat.Unknown, true);
                }
            }

            // Configure columns.
            var colCurrent = new OLVColumn();
            var colName = new OLVColumn();
            var colStart = new OLVColumn();
            var colEnd = new OLVColumn();
            var colDuration = new OLVColumn();
            var colCumul = new OLVColumn();
            var colTag = new OLVColumn();

            // Name of properties used to get the data from the objects.
            colCurrent.AspectName = "IsCurrent";
            colName.AspectName = "Name";
            colStart.AspectName = "Start";
            colEnd.AspectName = "End";
            colDuration.AspectName = "Duration";
            colCumul.AspectName = "Cumul";
            colTag.AspectName = "Tag";

            colCurrent.Groupable = false;
            colName.Groupable = false;
            colTag.Groupable = false;

            colCurrent.Sortable = false;
            colName.Sortable = false;
            colTag.Sortable = false;
            
            colCurrent.IsEditable = false;
            
            SetTimeColumn(colStart);
            SetTimeColumn(colEnd);
            SetTimeColumn(colDuration);
            SetTimeColumn(colCumul);

            colCurrent.MinimumWidth = 20;
            colCurrent.MaximumWidth = 20;
            colName.MinimumWidth = 100;
            colTag.MinimumWidth = 50;

            colName.FillsFreeSpace = true;
            colName.FreeSpaceProportion = 2;
            colTag.FillsFreeSpace = true;
            colTag.FreeSpaceProportion = 1;

            // Displayed column name.
            colCurrent.Text = "";
            colName.Text = ScreenManagerLang.mnuMeasure_Name;
            colStart.Text = ScreenManagerLang.FormTimeSections_Start;
            colEnd.Text = ScreenManagerLang.FormTimeSections_End;
            colDuration.Text = ScreenManagerLang.FormTimeSections_Duration;
            colCumul.Text = ScreenManagerLang.FormTimeSections_Cumulative;
            colTag.Text = ScreenManagerLang.FormTimeSections_Tag;

            colName.TextAlign = HorizontalAlignment.Left;
            colTag.TextAlign = HorizontalAlignment.Left;

            colCurrent.AspectToStringConverter = v =>
            {
                bool active = (bool)v;
                return active ? "▶" : "";
            };

            olvSections.AllColumns.AddRange(new OLVColumn[] {
                colCurrent,
                colName,
                colTag,
                colStart,
                colEnd,
                colDuration,
                colCumul,
                });

            olvSections.Columns.AddRange(new ColumnHeader[] {
                colCurrent,
                colName,
                colTag,
                colStart,
                colEnd,
                colDuration,
                colCumul,
                });

            olvSections.SetObjects(sections);
        }

        private void SetTimeColumn(OLVColumn col)
        {
            col.Groupable = false;
            col.Sortable = false;
            col.IsEditable = false;
            col.TextAlign = HorizontalAlignment.Center;
            col.MinimumWidth = 75;
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
