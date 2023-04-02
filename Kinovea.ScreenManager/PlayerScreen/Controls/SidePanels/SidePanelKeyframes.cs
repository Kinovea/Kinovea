using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// This is the panel containing the existing keyframe comment boxes.
    /// </summary>
    public partial class SidePanelKeyframes : UserControl
    {
        #region Events
        public event EventHandler<TimeEventArgs> Keyframe_SelectAsked;
        #endregion

        #region Properties
        public bool Editing
        {
            get { return kfcbs.Any(kfcb => kfcb.Value.Editing); }
        }
        #endregion


        #region Members
        private Metadata parentMetadata;
        private Dictionary<Guid, KeyframeCommentBox> kfcbs = new Dictionary<Guid, KeyframeCommentBox>();
        #endregion

        public SidePanelKeyframes()
        {
            InitializeComponent();
        }

        public void SetMetadata(Metadata metadata)
        {
            this.parentMetadata = metadata;
            ResetContent();
        }

        private void ResetContent()
        {
            // Import the metadata anew.
            // Create a control for each keyframe and add it to the panel.
            pnlKeyframes.Controls.Clear();
            kfcbs.Clear();

            if (parentMetadata == null || parentMetadata.Count == 0)
                return;

            int top = 0;
            int margin = 5;
            foreach (var kf in parentMetadata.Keyframes)
            {
                KeyframeCommentBox kfb = new KeyframeCommentBox();
                kfb.SetKeyframe(kf);
                kfb.Top = top;
                kfb.Width = this.Width;
                kfb.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                kfb.SelectAsked += KeyframeCommentBox_SelectAsked;

                kfcbs.Add(kf.Id, kfb);
                pnlKeyframes.Controls.Add(kfb);
                
                top += kfb.Height + margin;
            }

        }

        private void KeyframeCommentBox_SelectAsked(object sender, TimeEventArgs e)
        {
            Keyframe_SelectAsked?.Invoke(sender, e);
        }

        public void ResetKeyframes()
        {
            // The model has changed.
            // Add/removed keyframes, or just changed content ?
            ResetContent();
        }

        /// <summary>
        /// Highlight the keyframe corresponding to the passed time, unhighlight the others.
        /// </summary>
        public void HighlightKeyframe(long timestamp)
        {
            foreach (var kfb in kfcbs)
            {
                kfb.Value.UpdateHighlight(timestamp);
            }
        }
    }
}
