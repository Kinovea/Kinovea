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
    /// This controls holds the keyframe name, color, timecode and comment.
    /// It is used in the side panel.
    /// </summary>
    public partial class KeyframeCommentBox : UserControl
    {
        #region Events
        /// <summary>
        /// Asks the main timeline to move to the time of this keyframe.
        /// </summary>
        public event EventHandler<TimeEventArgs> SelectAsked;
        #endregion

        #region Properties
        public Keyframe Keyframe
        {
            get { return keyframe; }
        }
        public bool Editing
        {
            get { return editingName || editingComment; }
        }
        #endregion

        #region Members
        private bool editingName;
        private bool editingComment;
        private Keyframe keyframe;
        private bool manualUpdate;
        private bool isSelected;
        #endregion

        public KeyframeCommentBox()
        {
            InitializeComponent();
            btnColor.BackColor = Color.White;
            rtbComment.BackColor = Color.White;
            btnSidebar.BackColor = Color.White;

            btnColor.FlatAppearance.MouseDownBackColor = Color.White;
            btnColor.FlatAppearance.MouseOverBackColor = Color.White;
            btnColor.Paint += BtnColor_Paint;
            this.Paint += KeyframeCommentBox_Paint;
        }

        private void KeyframeCommentBox_Paint(object sender, PaintEventArgs e)
        {
            //btnColor.Invalidate();
        }

        private void KeyframeCommentBox_Invalidated(object sender, InvalidateEventArgs e)
        {
            //btnColor.Invalidate();
        }

        #region Public methods

        /// <summary>
        /// Set the keyframe this control is wrapping.
        /// </summary>
        public void SetKeyframe(Keyframe keyframe)
        {
            this.keyframe = keyframe;
            if (keyframe == null)
                return;
            manualUpdate = true;
            tbName.Text = keyframe.Title;
            AfterNameChange();
            lblTimecode.Text = string.Format("{0}", keyframe.TimeCode);
            rtbComment.Rtf = keyframe.Comments;
            manualUpdate = false;
        }

        /// <summary>
        /// Update the highlight status based on the current timestamp.
        /// </summary>
        /// <param name="timestamp"></param>
        public void UpdateHighlight(long timestamp)
        {
            if (keyframe == null)
                return;

            isSelected = keyframe.Position == timestamp;

            btnSidebar.BackColor = isSelected ? keyframe.Color : Color.White;
        }
        #endregion

        private void BtnColor_Paint(object sender, PaintEventArgs e)
        {
            if (keyframe == null)
                return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = e.ClipRectangle;
            rect.X += 1;
            rect.Y += 1;
            rect.Width -= 2;
            rect.Height -= 2;
            using (SolidBrush brush = new SolidBrush(keyframe.Color))
                e.Graphics.FillEllipse(brush, rect);
        }

        private void btnColor_Click(object sender, EventArgs e)
        {
            if (keyframe == null || manualUpdate)
                return;

            FormColorPicker picker = new FormColorPicker(keyframe.Color);
            FormsHelper.Locate(picker);
            if (picker.ShowDialog() == DialogResult.OK)
            {
                keyframe.Color = picker.PickedColor;
                AfterColorChange();
            }
            picker.Dispose();
        }

        private void tbName_TextChanged(object sender, EventArgs e)
        {
            if (keyframe == null || manualUpdate)
                return;

            keyframe.Title = tbName.Text;
            AfterNameChange();
        }

        private void AfterNameChange()
        {
            Size size = TextRenderer.MeasureText(tbName.Text, tbName.Font);
            tbName.Width = size.Width;
            tbName.Height = size.Height;
        }

        private void AfterColorChange()
        {
            btnSidebar.BackColor = isSelected ? keyframe.Color : Color.White;
        }

        private void rtbComment_TextChanged(object sender, EventArgs e)
        {
            if (keyframe == null || manualUpdate)
                return;

            keyframe.Comments = rtbComment.Rtf;
        }

        private void tbName_Leave(object sender, EventArgs e)
        {
            editingName = false;
        }

        private void tbName_Enter(object sender, EventArgs e)
        {
            editingName = true;
        }

        private void rtbComment_Enter(object sender, EventArgs e)
        {
            editingComment = true;
        }

        private void rtbComment_Leave(object sender, EventArgs e)
        {
            editingComment = false;
        }

        private void KeyframeCommentBox_Enter(object sender, EventArgs e)
        {
            SelectAsked?.Invoke(this, new TimeEventArgs(keyframe.Position));
        }

        private void KeyframeCommentBox_Click(object sender, EventArgs e)
        {
            SelectAsked?.Invoke(this, new TimeEventArgs(keyframe.Position));
        }
    }
}
