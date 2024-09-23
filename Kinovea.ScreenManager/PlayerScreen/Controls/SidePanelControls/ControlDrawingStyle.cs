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
    public partial class ControlDrawingStyle : UserControl
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
        //private Guid drawingId;
        private bool manualUpdate;
        private bool editing;
        private List<AbstractStyleElement> elementList = new List<AbstractStyleElement>();
        private Dictionary<AbstractStyleElement, Control> miniEditors = new Dictionary<AbstractStyleElement, Control>();
        private Pen penBorder = Pens.Silver;
        //private Action invalidator;
        //private HistoryMementoModifyDrawing memento;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public ControlDrawingStyle()
        {
            InitializeComponent();
            this.Paint += Control_Paint;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Set the drawing this control is managing.
        /// </summary>
        public void SetDrawing(AbstractDrawing drawing, Metadata metadata, Guid managerId, Guid drawingId)
        {
            manualUpdate = true;
            
            // TODO: check if same drawing?
            if (this.drawing != null && drawing != null && this.drawing.Id == drawing.Id)
            {
                log.DebugFormat("Set drawing of the same drawing.");
            }

            Clear();

            this.drawing = drawing;
            this.metadata = metadata;
            this.managerId = managerId;

            if (drawing == null || !(drawing is IDecorable))
            {
                manualUpdate = false;
                return;
            }

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
            pnlConfig.Controls.Clear();
            miniEditors.Clear();
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
            pnlConfig.Controls.Clear();
            miniEditors.Clear();

            Size editorSize = new Size(60, 20);

            // Initialize the horizontal layout with a minimal value, 
            // it will be fixed later if some of the entries have long text.
            //int minimalWidth = btnOK.Width + btnCancel.Width + 10;
            //int editorsLeft = minimalWidth - 20 - editorSize.Width;
            int editorsLeft = 10;
            int lastEditorBottom = 0;

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
                int editorLeftMargin = 50;
                if (lbl.Left + labelSize.Width + editorLeftMargin > editorsLeft)
                    editorsLeft = (int)(lbl.Left + labelSize.Width + editorLeftMargin);

                Control miniEditor = styleElement.GetEditor();
                miniEditor.Size = editorSize;
                miniEditor.Location = new Point(editorsLeft, btn.Top);
                miniEditor.Enter += (s, e) => { editing = true; };
                miniEditor.Leave += (s, e) => { editing = false; };

                lastEditorBottom = miniEditor.Bottom;

                pnlConfig.Controls.Add(btn);
                pnlConfig.Controls.Add(lbl);
                pnlConfig.Controls.Add(miniEditor);
                miniEditors.Add(styleElement, miniEditor);
            }

            // Recheck all mini editors for the left positionning.
            foreach (Control c in pnlConfig.Controls)
            {
                if (!(c is Label) && !(c is Button))
                {
                    if (c.Left < editorsLeft)
                        c.Left = editorsLeft;
                }
            }

            pnlConfig.Height = lastEditorBottom + 20;
            this.Height = this.Height - this.ClientRectangle.Height + pnlConfig.Height + 10;
        }

        /// <summary>
        /// A style element has been changed either here or externally.
        /// This may be called as part of undo/redo mechanics.
        /// </summary>
        private void element_ValueChanged(object sender, EventArgs<string> e)
        {
            if (drawing == null)
            {
                throw new InvalidProgramException();
            }
            
            if (manualUpdate)
            {
                return;
            }

            log.Debug(string.Format("Style element changed: {0}", e.Value));

            // Signal to the parent panel.
            // This will decide to push the memento to the history stack or not
            // and propagate the change to the player screen.
            DrawingModified?.Invoke(this, new DrawingEventArgs(drawing, managerId));

            // Make sure the mini editor has the right value, without retriggering the event.
            // This is used to handle external changes to individual style elements, like
            // changing the font size from the corner of the text label.
            if (!metadata.HistoryStack.IsPerformingUndoRedo)
            {
                manualUpdate = true;

                AbstractStyleElement elem = sender as AbstractStyleElement;
                if (elem == null)
                {
                    manualUpdate = false;
                    return;
                }

                elem.UpdateEditor(miniEditors[elem]);
                manualUpdate = false;
            }
        }

        /// <summary>
        /// Force focus on click to make it easier to unfocus any text fields.
        /// </summary>
        private void pnlConfig_Click(object sender, EventArgs e)
        {
            pnlConfig.Focus();
        }

        /// <summary>
        /// Custom outline color.
        /// </summary>
        private void Control_Paint(object sender, PaintEventArgs e)
        {
            Rectangle rect = new Rectangle(0, 0, this.ClientRectangle.Width - 1, this.ClientRectangle.Height - 1);
            e.Graphics.DrawRectangle(penBorder, rect);
        }
    }
}
