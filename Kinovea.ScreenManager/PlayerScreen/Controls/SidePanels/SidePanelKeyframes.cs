using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
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
        /// Update the list of keyframes.
        /// </summary>
        public void OrganizeContent(Metadata metadata)
        {
            this.parentMetadata = metadata;
            OrganizeContent();
        }

        /// <summary>
        /// Hide all keyframes.
        /// </summary>
        public void Clear()
        {
            this.parentMetadata = null;
            OrganizeContent();
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


        private void OrganizeContent()
        {
            //-----------------------------------------------------------
            // Recycle the existing controls as much as possible.
            // Just change the keyframe they are pointing to.
            // Also we don't delete controls, it's costly to recreate, just hide them.
            //-----------------------------------------------------------

            Stopwatch sw = Stopwatch.StartNew();

            flowKeyframes.SuspendLayout();

            if (parentMetadata == null)
            {
                // Hide all the controls.
                kfcbs.Clear();
                for (int i = 0; i < flowKeyframes.Controls.Count; ++i)
                {
                    flowKeyframes.Controls[i].Visible = false;
                }

                flowKeyframes.ResumeLayout();
                return;
            }

            var keyframes = parentMetadata.Keyframes;
            for (int i = 0; i < keyframes.Count; i++)
            {
                // Add a control if needed.
                if (flowKeyframes.Controls.Count < i + 1)
                {
                    ControlKeyframe kfb = new ControlKeyframe();
                    kfb.SetKeyframe(parentMetadata, keyframes[i]);
                    kfb.Selected += (s, e) => KeyframeSelected?.Invoke(s, e);
                    kfb.Updated += (s, e) => KeyframeUpdated?.Invoke(s, e);
                    kfb.DeletionAsked += (s, e) => KeyframeDeletionAsked?.Invoke(s, e);

                    // https://learn.microsoft.com/en-us/dotnet/desktop/winforms/controls/how-to-anchor-and-dock-child-controls-in-a-flowlayoutpanel-control
                    // General rule for anchoring and docking in the FlowLayoutPanel control:
                    // for vertical flow directions, the FlowLayoutPanel control calculates the width
                    // of an implied column from the widest child control in the column.
                    // All other controls in this column with Anchor or Dock properties are aligned or stretched
                    // to fit this implied column.
                    if (i == 0)
                        kfb.Width = flowKeyframes.Width - 10;
                    else
                        kfb.Dock = DockStyle.Fill;
                    
                    flowKeyframes.Controls.Add(kfb);
                    continue;
                }

                // Replace the keyframe in that control if needed.
                var oldBox = flowKeyframes.Controls[i] as ControlKeyframe;
                if (oldBox.Keyframe.Id != keyframes[i].Id)
                {
                    oldBox.SetKeyframe(parentMetadata, keyframes[i]);
                    oldBox.Visible = true;
                }
            }

            // Hide leftover controls.
            for (int i = keyframes.Count; i < flowKeyframes.Controls.Count; i++)
            {
                var oldBox = flowKeyframes.Controls[i] as ControlKeyframe;
                oldBox.Visible = false;
            }

            log.DebugFormat("Organized {0} keyframes in {1} ms.", keyframes.Count, sw.ElapsedMilliseconds);

            // Keep our dict in sync.
            kfcbs.Clear();
            for (int i = 0; i < flowKeyframes.Controls.Count; ++i)
            {
                var kfcb = flowKeyframes.Controls[i] as ControlKeyframe;
                if (kfcb.Visible)
                {
                    kfcbs.Add(kfcb.Keyframe.Id, kfcb);
                }
            }

            flowKeyframes.ResumeLayout();
        }

        private void flowKeyframes_Layout(object sender, LayoutEventArgs e)
        {
            // Simulate anchor left|right for the first, all the others will follow.
            if (flowKeyframes.Controls.Count > 0)
            {
                flowKeyframes.Controls[0].Width = flowKeyframes.ClientSize.Width - 10;
            }
        }
    }
}
