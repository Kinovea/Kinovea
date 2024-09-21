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

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Panel containing the style configuration for the active drawing.
    /// </summary>
    public partial class SidePanelDrawing : UserControl
    {
        #region Events
        public event EventHandler<DrawingEventArgs> DrawingModified;
        #endregion

        #region Properties
        public bool Editing
        {
            get { return styleConfigurator1.Editing; }
        }
        #endregion

        #region Members
        private AbstractDrawing drawing;
        private Metadata parentMetadata;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public SidePanelDrawing()
        {
            InitializeComponent();

            styleConfigurator1.DrawingModified += (s, e) => DrawingModified?.Invoke(s, e);
        }

        #region Public methods

        /// <summary>
        /// Set the metadata to listen to.
        /// </summary>
        public void SetMetadata(Metadata metadata)
        {
            Clear();
            this.parentMetadata = metadata;
            parentMetadata.DrawingSelected += Metadata_DrawingSelected;
            parentMetadata.DrawingDeleted += Metadata_DrawingDeleted;
        }

        /// <summary>
        /// Set the content to show the specified drawing.
        /// </summary>
        public void SetDrawing(AbstractDrawing drawing, Metadata metadata, Guid managerId, Guid drawingId)
        {
            this.drawing = drawing;
            styleConfigurator1.SetDrawing(drawing, metadata, managerId, drawingId);
        }

        /// <summary>
        /// Reset the content to an empty state.
        /// </summary>
        public void Clear()
        {
            if (parentMetadata == null)
                return;

            parentMetadata.DrawingSelected -= Metadata_DrawingSelected;
            parentMetadata.DrawingDeleted -= Metadata_DrawingDeleted;
            SetDrawing(null, null, Guid.Empty, Guid.Empty);
        }

        /// <summary>
        /// A drawing was just selected, update the content to show it.
        /// </summary>
        private void Metadata_DrawingSelected(object sender, DrawingEventArgs e)
        {
            if (e.Drawing == null)
            {
                SetDrawing(null, null, Guid.Empty, Guid.Empty);
            }
            else
            {
                SetDrawing(e.Drawing, parentMetadata, parentMetadata.HitDrawingOwner.Id, e.Drawing.Id);
            }
        }

        /// <summary>
        /// A drawing was just deleted.
        /// If it's the drawing that was displayed here clear the contents.
        /// </summary>
        private void Metadata_DrawingDeleted(object sender, EventArgs<Guid> e)
        {
            if (drawing.Id != e.Value)
                return;

            SetDrawing(null, null, Guid.Empty, Guid.Empty);
        }
        #endregion
    }
}
