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
using DocumentFormat.OpenXml.ExtendedProperties;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// This controls holds the name and style configuration editors for the active drawing.
    /// It is used in the side panel.
    /// </summary>
    public partial class ControlDrawingName : UserControl
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
        private bool manualUpdate;
        private bool editing;
        //private List<AbstractStyleElement> elementList = new List<AbstractStyleElement>();
        //private Dictionary<AbstractStyleElement, Control> miniEditors = new Dictionary<AbstractStyleElement, Control>();
        //private Action invalidator;
        //private HistoryMementoModifyDrawing memento;
        private Pen penBorder = Pens.Silver;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public ControlDrawingName()
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
            //memento = null;
            //Clear();

            this.drawing = drawing;

            if (drawing == null || !(drawing is IDecorable))
            {
                //this.drawing = null;
                tbName.Text = "";
                tbName.Enabled = false;
                manualUpdate = false;
                return;
            }

            this.metadata = metadata;
            this.managerId = managerId;
            //this.drawingId = drawingId;
            
            //CaptureCurrentState();
            
            // Update content.
            tbName.Text = drawing.Name;
            tbName.Enabled = true;
            AfterNameChange();
            
            manualUpdate = false;
        }
        #endregion

        private void Control_Paint(object sender, PaintEventArgs e)
        {
            Rectangle rect = new Rectangle(0, 0, this.ClientRectangle.Width - 1, this.ClientRectangle.Height - 1);
            e.Graphics.DrawRectangle(penBorder, rect);
        }

        //private void Clear()
        //{
        //    //memento = null;
        //}
       
        /// <summary>
        /// The drawing name has been changed.
        /// This may be called as part of undo/redo mechanics.
        /// </summary>
        private void tbName_TextChanged(object sender, EventArgs e)
        {
            if (manualUpdate)
            {
                return;
            }

            if (drawing == null || metadata == null)
            {
                throw new InvalidProgramException();
            }

            //if (memento == null)
            //{
            //    throw new InvalidProgramException();
            //}

            // Don't allow empty name.
            if (string.IsNullOrEmpty(tbName.Text.Trim()))
            {
                manualUpdate = true;
                metadata.SetDrawingName(drawing);
                tbName.Text = drawing.Name;
                manualUpdate = false;
            }
            else
            {
                drawing.Name = tbName.Text;
            }

            //memento.UpdateCommandName(drawing.Name);
            AfterNameChange();
            AfterStateChanged(sender);
        }

        private void AfterNameChange()
        {
            Size size = TextRenderer.MeasureText(tbName.Text, tbName.Font);
            tbName.Width = size.Width;
            tbName.Height = size.Height;
        }

        
        /// <summary>
        /// Push the previously saved memento to the history stack, and capture the new current state.
        /// This should be called after making any undoable change to the data.
        /// Signal to the host that the drawing has been modified.
        /// </summary>
        private void AfterStateChanged(object sender)
        {
            //if (metadata == null || managerId == Guid.Empty || drawingId == Guid.Empty)
            //{
            //    throw new InvalidProgramException();
            //}

            //if (memento == null)
            //{
            //    throw new InvalidProgramException();
            //}


            DrawingModified?.Invoke(this, new DrawingEventArgs(drawing, managerId));

            // As part of undo mechanics this is called for every style element 
            // when importing from the saved KVA fragment and rebiding.
            // Detect this and ignore.
            //if (metadata.HistoryStack.IsPerformingUndoRedo)
            //{
            //    if (memento.IsSameState())
            //    {
            //        //log.Debug("Same state while performing undo/redo. Ignore.");
            //        return;
            //    }

            //    //log.Debug("Different state while performing undo/redo. Capture current state for later.");
            //    CaptureCurrentState();
            //    return;
            //}
            //else
            //{
            //    // Push the old state to the undo stack.
            //    metadata.HistoryStack.PushNewCommand(memento);

            //    // Immediately capture the new state to prepare for the next change.
            //    CaptureCurrentState();

            //    // Signal to the host that the drawing has been modified.
            //    // This is used to update the image, preset and cursor.
            //    DrawingModified?.Invoke(this, new DrawingEventArgs(drawing, managerId));

            //    // Make sure the mini editor has the right value, without retriggering the event.
            //    // This is used to handle external changes to individual style elements, like
            //    // changing the font size from the corner of the text label.
            //    manualUpdate = true;

            //    AbstractStyleElement elem = sender as AbstractStyleElement;
            //    if (elem == null)
            //    {
            //        manualUpdate = false;
            //        return;
            //    }

            //    elem.UpdateEditor(miniEditors[elem]);
            //    manualUpdate = false;
            //}
        }

        /// <summary>
        /// Capture the current state to a memento.
        /// This may be pushed to the history stack later if we change state again.
        /// This should be called when the data is initialized or changed from the outside.
        /// </summary>
        //private void CaptureCurrentState()
        //{
        //    if (metadata == null || managerId == Guid.Empty || drawingId == Guid.Empty)
        //    {
        //        throw new InvalidProgramException();
        //    }

        //    memento = new HistoryMementoModifyDrawing(metadata, managerId, drawingId, drawing.Name, SerializationFilter.Style);
        //}


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
