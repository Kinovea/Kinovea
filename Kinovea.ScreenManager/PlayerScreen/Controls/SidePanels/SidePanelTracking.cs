using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Spreadsheet;
using Kinovea.Services;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Panel containing the name and style configuration for the active drawing.
    /// </summary>
    public partial class SidePanelTracking : UserControl
    {
        #region Events
        public event EventHandler<DrawingEventArgs> DrawingModified;
        #endregion

        #region Properties
        /// <summary>
        /// Whether the drawing is currently being edited.
        /// Any shortcut like space or delete should be ignored as they conflict with 
        /// writing text fields.
        /// </summary>
        public bool Editing
        {
            get { return controlDrawingTrackingSetup.Editing || controlDrawingName.Editing; }
        }
        #endregion

        #region Members
        private AbstractDrawing drawing;
        private Metadata metadata;
        private Guid managerId;
        private Guid drawingId;
        private Action invalidator;
        private HistoryMementoModifyDrawing memento;
        private IDrawingHostView hostView;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public SidePanelTracking(IDrawingHostView hostView)
        {
            this.hostView = hostView;

            InitializeComponent();
            controlDrawingName.DrawingModified += Control_DrawingModified;
            controlDrawingTrackingSetup.DrawingModified += Control_DrawingModified;
            controlDrawingTrackingSetup.SetHostView(hostView);
        }

        #region Public methods

        /// <summary>
        /// Set the metadata to listen to.
        /// </summary>
        public void SetMetadata(Metadata metadata)
        {
            ClearMetadata();
            this.metadata = metadata;
            metadata.DrawingSelected += Metadata_DrawingSelected;
            metadata.DrawingDeleted += Metadata_DrawingDeleted;
            metadata.KeyframeDeleted += Metadata_KeyframeDeleted;
        }

        /// <summary>
        /// Reset the content to an empty state.
        /// </summary>
        public void ClearMetadata()
        {
            if (metadata == null)
                return;

            metadata.DrawingSelected -= Metadata_DrawingSelected;
            metadata.DrawingDeleted -= Metadata_DrawingDeleted;
            metadata.KeyframeDeleted -= Metadata_KeyframeDeleted;
            metadata = null;

            SetDrawing(null, Guid.Empty, Guid.Empty);
        }

        /// <summary>
        /// Set the content to show the specified drawing.
        /// Pass null to reset to an empty state.
        /// </summary>
        public void SetDrawing(AbstractDrawing drawing, Guid managerId, Guid drawingId)
        {
            memento = null;
            this.drawing = drawing;
            this.managerId = managerId;
            this.drawingId = drawingId;

            if (drawing != null && (drawing is IDecorable))
                CaptureCurrentState();

            controlDrawingName.SetDrawing(drawing, metadata, managerId, drawingId);
            controlDrawingTrackingSetup.SetDrawing(drawing, metadata, managerId, drawingId);
        }

        /// <summary>
        /// The name was changed from outside. Make sure we reflect it.
        /// </summary>
        public void UpdateName()
        {
            controlDrawingName.UpdateName();
        }
        #endregion

        /// <summary>
        /// The drawing data was modified from one of the child controls.
        /// Push the prepared memento with the original state to the history stack, 
        /// and capture the new current state for later.
        /// Signal to the host that the drawing has been modified to update image.
        /// </summary>
        private void Control_DrawingModified(object sender, DrawingEventArgs e)
        {
            
            if (metadata == null || managerId == Guid.Empty || drawingId == Guid.Empty)
            {
                throw new InvalidProgramException();
            }

            if (memento == null)
            {
                throw new InvalidProgramException();
            }

            memento.UpdateCommandName(drawing.Name);

            // As part of undo mechanics this is called for every style element 
            // when importing from the saved KVA fragment and rebiding.
            // Detect this and ignore.
            if (metadata.HistoryStack.IsPerformingUndoRedo)
            {
                if (memento.IsSameState())
                {
                    //log.Debug("Same state while performing undo/redo. Ignore.");
                    return;
                }

                // Normal case of having made some changes to the object in the style
                // editor or elsewhere and undoing them.
                //log.Debug("Different state while performing undo/redo. Capture current state for later.");
                CaptureCurrentState();
                return;
            }
            else
            {
                // Push the old state to the undo stack.
                metadata.HistoryStack.PushNewCommand(memento);

                // Immediately capture the new state to prepare for the next change.
                // The change may come from outside like in the case of changing the font size 
                // from the corner of the label. We still need to prepare the memento with
                // the new state so that when we post the state later it is correct.
                CaptureCurrentState();

                // Signal to the host that the drawing has been modified.
                // This is used to update the image, preset and cursor.
                DrawingModified?.Invoke(this, new DrawingEventArgs(drawing, managerId));
            }
        }

        /// <summary>
        /// A drawing was selected in the player, update the content to show it.
        /// </summary>
        private void Metadata_DrawingSelected(object sender, DrawingEventArgs e)
        {
            if (e.Drawing == null)
            {
                SetDrawing(null, Guid.Empty, Guid.Empty);
            }
            else
            {
                SetDrawing(e.Drawing, metadata.HitDrawingOwner.Id, e.Drawing.Id);
            }
        }

        /// <summary>
        /// A drawing was deleted in the player.
        /// If it's the drawing we were handling clear the contents.
        /// </summary>
        private void Metadata_DrawingDeleted(object sender, EventArgs<Guid> e)
        {
            if (drawing == null)
                return;

            if (drawing.Id != e.Value)
                return;

            SetDrawing(null, Guid.Empty, Guid.Empty);
        }

        /// <summary>
        /// A keyframe was deleted.
        /// If it contained the drawing we were handling clear the contents.
        /// </summary>
        private void Metadata_KeyframeDeleted(object sender, KeyframeEventArgs e)
        {
            if (drawing == null)
                return;

            if (managerId != e.KeyframeId)
                return;

            SetDrawing(null, Guid.Empty, Guid.Empty);
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

            // FIXME: when this panel handles other parts of the drawing like visibility
            // we'll want to set a more general serialization filter.
            // At the time we capture the state we don't know what kind of change will be made.
            memento = new HistoryMementoModifyDrawing(metadata, managerId, drawingId, drawing.Name, SerializationFilter.Style);
        }
    }
}
