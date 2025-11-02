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
using Kinovea.ScreenManager.Languages;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// This is the panel containing the keyframe comment boxes.
    /// </summary>
    public partial class SidePanelKeyframes : UserControl
    {
        #region Events
        public event EventHandler KeyframeAddAsked;
        public event EventHandler KeyframeNextAsked;
        public event EventHandler KeyframePrevAsked;
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
        private bool filterOutZone = true;
        private Dictionary<Guid, ControlKeyframe> kfcbs = new Dictionary<Guid, ControlKeyframe>();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public SidePanelKeyframes()
        {
            InitializeComponent();

            toolTip1.SetToolTip(btnAddKeyframe, ScreenManagerLang.ToolTip_AddKeyframe);
            toolTip1.SetToolTip(btnPrev, Kinovea.ScreenManager.Languages.ScreenManagerLang.tooltip_PreviousKeyImage);
            toolTip1.SetToolTip(btnNext, Kinovea.ScreenManager.Languages.ScreenManagerLang.tooltip_NextKeyImage);
            toolTip1.SetToolTip(btnShowAll, Kinovea.ScreenManager.Languages.ScreenManagerLang.tooltip_ShowAllKeyImages);
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

        /// <summary>
        /// Update the control hosting this key frame to reflect changes that may have
        /// happened elsewhere.
        /// </summary>
        public void UpdateKeyframe(Guid id)
        {
            if (kfcbs.ContainsKey(id))
                kfcbs[id].UpdateContent();
        }

        /// <summary>
        /// Update the control hosting this key frame after the thumbnail image
        /// may have been updated or created.
        /// </summary>
        public void UpdateImage(Guid id)
        {
            if (kfcbs.ContainsKey(id))
                kfcbs[id].UpdateImage();
        }
        #endregion


        private void OrganizeContent()
        {
            //-----------------------------------------------------------
            // Recycle the existing controls as much as possible, just change the keyframe they are pointing to.
            // Also we don't delete controls as they are costly to recreate, just hide them.
            // Controls get disposed when we close the screen.
            //
            // Adding new controls is what takes the most time, it seems to go through a redraw/layout somehow.
            // Doing it while the whole side panel is closed is much faster.
            // Setting .Visible to false improves perfs a bit but it breaks the selection when deleting.
            // Sending WM_SETREDRAW improves perfs as well and doesn't break selection.
            // Stats on adding a new control with 125 controls already in place:
            // - With SuspendLayout/ResumeLayout: 150 ms.
            // - With WM_SETREDRAW, 65 ms.
            // - With the panel closed, 4 ms.
            // Calling SETREDRAW on this control or on the flowKeyframes doesn't make a difference.
            //-----------------------------------------------------------

            Stopwatch sw = Stopwatch.StartNew();

            flowKeyframes.SuspendLayout();
            SuspendDraw();

            kfcbs.Clear();
            
            if (parentMetadata == null)
            {
                // Hide all the controls.
                for (int i = 0; i < flowKeyframes.Controls.Count; ++i)
                {
                    flowKeyframes.Controls[i].Visible = false;
                }

                ResumeDraw();
                flowKeyframes.ResumeLayout();
                flowKeyframes.Refresh();
                return;
            }

            var keyframes = parentMetadata.Keyframes;
            int ctrlIndex = 0;
            for (int i = 0; i < keyframes.Count; i++)
            {
                var kf = keyframes[i];

                if (filterOutZone && kf.Timestamp < parentMetadata.SelectionStart)
                {
                    // Before zone.
                    continue;
                }

                if (filterOutZone && kf.Timestamp > parentMetadata.SelectionEnd)
                {
                    // After zone.
                    break;
                }

                ControlKeyframe kfb;
                
                // Add a control if needed.
                if (flowKeyframes.Controls.Count < ctrlIndex + 1)
                {
                    kfb = new ControlKeyframe();
                    kfb.SetKeyframe(parentMetadata, kf);
                    kfb.Selected += (s, e) => KeyframeSelected?.Invoke(s, e);
                    kfb.Updated += (s, e) => KeyframeUpdated?.Invoke(s, e);
                    kfb.DeletionAsked += (s, e) => KeyframeDeletionAsked?.Invoke(s, e);

                    // https://learn.microsoft.com/en-us/dotnet/desktop/winforms/controls/how-to-anchor-and-dock-child-controls-in-a-flowlayoutpanel-control
                    // General rule for anchoring and docking in the FlowLayoutPanel control:
                    // for vertical flow directions, the FlowLayoutPanel control calculates the width
                    // of an implied column from the widest child control in the column.
                    // All other controls in this column with Anchor or Dock properties are aligned or stretched
                    // to fit this implied column.
                    if (ctrlIndex == 0)
                        kfb.Width = flowKeyframes.Width - 10;
                    else
                        kfb.Dock = DockStyle.Fill;

                    flowKeyframes.Controls.Add(kfb);
                    kfcbs.Add(kf.Id, kfb);
                    ctrlIndex++;
                    continue;
                }

                // Replace the keyframe in that control if needed.
                kfb = flowKeyframes.Controls[ctrlIndex] as ControlKeyframe;
                if (kfb.Keyframe.Id != kf.Id)
                {
                    kfb.SetKeyframe(parentMetadata, kf);
                }

                kfb.Visible = true;
                kfcbs.Add(kf.Id, kfb);
                ctrlIndex++;
            }
            
            // Hide leftover controls.
            for (int i = kfcbs.Count; i < flowKeyframes.Controls.Count; i++)
            {
                var oldBox = flowKeyframes.Controls[i] as ControlKeyframe;
                oldBox.Visible = false;
            }

            log.DebugFormat("Organized keyframes: {0}/{1}/{2} in {3} ms.", 
                kfcbs.Count, keyframes.Count, flowKeyframes.Controls.Count, sw.ElapsedMilliseconds);

            ResumeDraw();
            flowKeyframes.ResumeLayout();
            flowKeyframes.Refresh();

            if (kfcbs.Count == keyframes.Count)
            {
                lblCount.Text = string.Format("{0}", keyframes.Count);
            }
            else
            {
                lblCount.Text = string.Format("{0}/{1}", kfcbs.Count, keyframes.Count);
            }

        }

        private void SuspendDraw()
        {
            NativeMethods.SendMessage(flowKeyframes.Handle, NativeMethods.WM_SETREDRAW, false, 0);
        }

        private void ResumeDraw()
        {
            NativeMethods.SendMessage(flowKeyframes.Handle, NativeMethods.WM_SETREDRAW, true, 0);
        }

        private void flowKeyframes_Layout(object sender, LayoutEventArgs e)
        {
            // Simulate anchor left|right for the first, all the others will follow.
            if (flowKeyframes.Controls.Count > 0)
            {
                flowKeyframes.Controls[0].Width = flowKeyframes.ClientSize.Width - 10;
            }
        }

        private void btnShowAll_Click(object sender, EventArgs e)
        {
            filterOutZone = !filterOutZone;
            btnShowAll.BackColor = filterOutZone ? Color.Transparent : Color.LightSteelBlue;
            btnShowAll.FlatAppearance.MouseOverBackColor = btnShowAll.BackColor;
            OrganizeContent();
        }

        private void btnAddKeyframe_Click(object sender, EventArgs e)
        {
            KeyframeAddAsked?.Invoke(this, EventArgs.Empty);
        }

        private void btnPrev_Click(object sender, EventArgs e)
        {
            KeyframePrevAsked?.Invoke(this, EventArgs.Empty);
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            KeyframeNextAsked?.Invoke(this, EventArgs.Empty);
        }
    }
}
