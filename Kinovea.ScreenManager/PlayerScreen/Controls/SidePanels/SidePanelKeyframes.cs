using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// This is the panel containing the keyframe comment boxes.
    /// </summary>
    public partial class SidePanelKeyframes : UserControl
    {
        #region Events
        public event EventHandler<TimeEventArgs> KeyframeSelected;
        public event EventHandler<EventArgs<Guid>> KeyframeUpdated;
        public event EventHandler<EventArgs<Guid>> KeyframeDeletionAsked;
        #endregion

        #region Properties
        public bool Editing
        {
            get { return kfcbs.Any(kfcb => kfcb.Value.Editing); }
        }
        #endregion

        #region Members
        private Metadata parentMetadata;
        private Dictionary<Guid, ControlKeyframe> kfcbs = new Dictionary<Guid, ControlKeyframe>();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public SidePanelKeyframes()
        {
            InitializeComponent();
        }

        #region Public methods
        /// <summary>
        /// Reset the list of keyframe comment boxes.
        /// </summary>
        public void Reset(Metadata metadata)
        {
            this.parentMetadata = metadata;
            ResetContent();
            log.DebugFormat("Side panel: ResetKeyframes");
        }

        public void Clear()
        {
            this.parentMetadata = null;
            ResetContent();
            log.DebugFormat("Side panel: Clear");
        }

        /// <summary>
        /// Update the timecode on all keyframes after a change in time calibration.
        /// </summary>
        public void UpdateTimecodes()
        {
            foreach (var kfcb in kfcbs)
                kfcb.Value.UpdateTimecode();
        }

        /// <summary>
        /// Highlight the keyframe corresponding to the passed time, unhighlight the others.
        /// </summary>
        public void HighlightKeyframe(long timestamp)
        {
            foreach (var kfcb in kfcbs)
            {
                kfcb.Value.UpdateHighlight(timestamp);

                if (kfcb.Value.Keyframe.Timestamp == timestamp)
                    flowKeyframes.ScrollControlIntoView(kfcb.Value);
            }
        }

        public void UpdateKeyframe(Guid id)
        {
            if (kfcbs.ContainsKey(id))
                kfcbs[id].UpdateContent();
        }
        #endregion


        private void ResetContent()
        {
            // Import the keyframe list from scratch.
            kfcbs.Clear();
            flowKeyframes.Controls.Clear();

            if (parentMetadata == null || parentMetadata.Count == 0)
                return;

            // The vertical margin between cards is defined in the KeyframeCommentBox control.
            foreach (var kf in parentMetadata.Keyframes)
            {
                ControlKeyframe kfb = new ControlKeyframe();
                kfb.SetKeyframe(parentMetadata, kf);
                kfb.Selected += (s, e) => KeyframeSelected?.Invoke(s, e);
                kfb.Updated += (s, e) => KeyframeUpdated?.Invoke(s, e);
                kfb.DeletionAsked += (s, e) => KeyframeDeletionAsked?.Invoke(s, e);
                
                kfcbs.Add(kf.Id, kfb);
                flowKeyframes.Controls.Add(kfb);
            }
        }

        private void flowKeyframes_Layout(object sender, LayoutEventArgs e)
        {
            foreach (var kfcb in kfcbs)
            {
                kfcb.Value.Width = flowKeyframes.ClientSize.Width - 10;
            }
        }
    }
}
