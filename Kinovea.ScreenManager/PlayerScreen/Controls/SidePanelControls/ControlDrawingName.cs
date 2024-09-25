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
        /// Returns true if any text editor is being edited.
        /// This must be consulted before triggering a shortcut that would conflict with text input.
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
            

            this.drawing = drawing;

            if (drawing == null || !(drawing is IDecorable))
            {
                manualUpdate = true;
                tbName.Text = "";
                tbName.Enabled = false;
                manualUpdate = false;
                return;
            }

            this.metadata = metadata;
            this.managerId = managerId;

            // Update content.
            manualUpdate = true;
            tbName.Text = drawing.Name;
            tbName.Enabled = true;
            manualUpdate = false;

            AfterNameChange();
        }

        /// <summary>
        /// The name was updated from the outside.
        /// </summary>
        public void UpdateName()
        {
            if (drawing == null)
                throw new InvalidProgramException();
            
            manualUpdate = true;
            tbName.Text = drawing.Name;
            manualUpdate = false;

            AfterNameChange();
        }
        #endregion

        /// <summary>
        /// The drawing name has been changed from the text box.
        /// This may also be called as part of undo/redo mechanics.
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

            AfterNameChange();
            AfterStateChanged(sender);
        }

        /// <summary>
        /// The name was changed, make sure the textbox is sized accordingly.
        /// </summary>
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
            DrawingModified?.Invoke(this, new DrawingEventArgs(drawing, managerId));
        }

        private void tbName_Enter(object sender, EventArgs e)
        {
            editing = true;
        }

        private void tbName_Leave(object sender, EventArgs e)
        {
            editing = false;
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
