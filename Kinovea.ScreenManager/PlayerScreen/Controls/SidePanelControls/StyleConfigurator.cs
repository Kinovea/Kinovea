using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// This controls holds the name and style configuration editors for the active drawing.
    /// It is used in the side panel.
    /// </summary>
    public partial class StyleConfigurator : UserControl
    {
        #region Events
        public event EventHandler<DrawingEventArgs> DrawingModified;
        #endregion

        #region Properties
        /// <summary>
        /// Returns true if the name field or any mini editor is being edited.
        /// </summary>
        public bool Editing
        {
            get { return editing; }
        }
        #endregion

        #region Members
        private AbstractDrawing drawing;
        private Metadata metadata;
        private Guid managerId;
        private Guid drawingId;
        private bool manualUpdate;
        private bool editing;
        private List<AbstractStyleElement> elementList = new List<AbstractStyleElement>();
        private Action invalidator;
        private HistoryMementoModifyDrawing memento;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public StyleConfigurator()
        {
            InitializeComponent();
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Set the drawing this control is managing.
        /// </summary>
        public void SetDrawing(AbstractDrawing drawing, Metadata metadata, Guid managerId, Guid drawingId)
        {
            manualUpdate = true;
            memento = null;
            Clear();

            if (drawing == null || !(drawing is IDecorable))
            {
                this.drawing = null;
                tbName.Text = "";
                manualUpdate = false;
                return;
            }

            this.drawing = drawing;
            this.metadata = metadata;
            this.managerId = managerId;
            this.drawingId = drawingId;
            
            CaptureCurrentState();
            
            // Update content.
            tbName.Text = drawing.Name;
            SetupMiniEditors();
            
            manualUpdate = false;
        }
        #endregion

        
        private void Clear()
        {
            foreach (AbstractStyleElement element in elementList)
            {
                element.ValueChanged -= element_ValueChanged;
            }

            elementList.Clear();
            grpConfig.Controls.Clear();
            memento = null;
        }
        
        /// <summary>
        /// Set up the mini editors for the style elements.
        /// </summary>
        private void SetupMiniEditors()
        {
            // Dynamic layout:
            // Any number of mini editor lines. (must scale vertically)
            // High dpi vs normal dpi (scales vertically and horizontally)
            // Verbose languages (scales horizontally)
            // All the dynamic layout is confined to the grpConfig box, it is possible to add elements before it.

            // Clean up
            grpConfig.Controls.Clear();

            Size editorSize = new Size(60, 20);

            // Initialize the horizontal layout with a minimal value, 
            // it will be fixed later if some of the entries have long text.
            //int minimalWidth = btnOK.Width + btnCancel.Width + 10;
            //int editorsLeft = minimalWidth - 20 - editorSize.Width;
            int editorsLeft = 10;
            int lastEditorBottom = 10;

            IDecorable decorable = drawing as IDecorable;
            StyleElements styleElements = decorable.StyleElements;

            foreach (KeyValuePair<string, AbstractStyleElement> pair in styleElements.Elements)
            {
                AbstractStyleElement styleElement = pair.Value;
                if (styleElement is StyleElementToggle && (((StyleElementToggle)styleElement).IsHidden))
                    continue;

                elementList.Add(styleElement);
                styleElement.ValueChanged += element_ValueChanged;

                Button btn = new Button();
                btn.Image = styleElement.Icon;
                btn.Size = new Size(20, 20);
                btn.Location = new Point(10, lastEditorBottom + 15);
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
                btn.BackColor = Color.Transparent;

                Label lbl = new Label();
                lbl.Text = styleElement.DisplayName;
                lbl.AutoSize = true;
                lbl.Location = new Point(btn.Right + 10, lastEditorBottom + 20);

                SizeF labelSize = TextHelper.MeasureString(lbl.Text, lbl.Font);

                // dynamic horizontal layout for high dpi and verbose languages.
                if (lbl.Left + labelSize.Width + 25 > editorsLeft)
                    editorsLeft = (int)(lbl.Left + labelSize.Width + 25);

                Control miniEditor = styleElement.GetEditor();
                miniEditor.Size = editorSize;
                miniEditor.Location = new Point(editorsLeft, btn.Top);
                miniEditor.Enter += (s, e) => { editing = true; };
                miniEditor.Leave += (s, e) => { editing = false; };

                lastEditorBottom = miniEditor.Bottom;

                grpConfig.Controls.Add(btn);
                grpConfig.Controls.Add(lbl);
                grpConfig.Controls.Add(miniEditor);
            }

            // Recheck all mini editors for the left positionning.
            foreach (Control c in grpConfig.Controls)
            {
                if (!(c is Label) && !(c is Button))
                {
                    if (c.Left < editorsLeft)
                        c.Left = editorsLeft;
                }
            }
        }

        /// <summary>
        /// The drawing name has been changed.
        /// </summary>
        private void tbName_TextChanged(object sender, EventArgs e)
        {
            if (manualUpdate)
            {
                return;
            }

            if (drawing == null)
            {
                throw new InvalidProgramException();
            }

            if (memento == null)
            {
                throw new InvalidProgramException();
            }

            // Ignore empty drawing name.
            if (string.IsNullOrEmpty(tbName.Text))
            {
                return;
            }
            
            drawing.Name = tbName.Text;
            memento.UpdateCommandName(drawing.Name);

            AfterStateChanged();
        }

        /// <summary>
        /// A style element has been changed either here or outside.
        /// This may be called as part of undo/redo mechanics.
        /// </summary>
        private void element_ValueChanged(object sender, EventArgs<string> e)
        {
            if (manualUpdate)
            {
                return;
            }
            
            if (drawing == null)
            {
                throw new InvalidProgramException();
            }

            if (memento == null)
            {
                throw new InvalidProgramException();
            }

            log.Debug(string.Format("Style element changed: {0}", e.Value));
            AfterStateChanged();
        }

        /// <summary>
        /// Push the previously saved memento to the history stack, and capture the new current state.
        /// This should be called after making any undoable change to the data.
        /// Signal to the host that the drawing has been modified.
        /// </summary>
        private void AfterStateChanged()
        {
            if (metadata == null || managerId == Guid.Empty || drawingId == Guid.Empty)
            {
                throw new InvalidProgramException();
            }

            if (memento == null)
            {
                throw new InvalidProgramException();
            }

            // As part of undo mechanics this is called for every style element 
            // when importing from the saved KVA fragment and rebiding.
            // Detect this and ignore.
            if (metadata.HistoryStack.IsPerformingUndoRedo)
            {
                if (memento.IsSameState())
                {
                    log.Debug("same state while performing undo/redo. Ignore.");
                    return;
                }

                log.Debug("Different state while performing undo/redo. Capture current state for later.");
                CaptureCurrentState();
                return;
            }
            else
            {
                // Push the old state to the undo stack.
                metadata.HistoryStack.PushNewCommand(memento);

                // Immediately capture the new state to prepare for the next change.
                CaptureCurrentState();
            
                // Signal to the host that the drawing has been modified.
                // This is used to update the image, preset and cursor.
                DrawingModified?.Invoke(this, new DrawingEventArgs(drawing, managerId));
            }
        }

        /// <summary>
        /// Capture the current state to a memento.
        /// This may be pushed to the history stack later if we change state again.
        /// This should be called when the data is initialized or changed from the outside.
        /// </summary>
        private void CaptureCurrentState()
        {
            if (metadata == null || managerId == Guid.Empty || drawingId == Guid.Empty)
            {
                throw new InvalidProgramException();
            }

            memento = new HistoryMementoModifyDrawing(metadata, managerId, drawingId, drawing.Name, SerializationFilter.Style);
        }


        private void tbName_Enter(object sender, EventArgs e)
        {
            editing = true;
        }

        private void tbName_Leave(object sender, EventArgs e)
        {
            editing = false;
        }

    }
}
