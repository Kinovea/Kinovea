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
        public event EventHandler<TimeEventArgs> Selected;
        public event EventHandler<EventArgs<Guid>> Updated;
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
            btnColor.BackColor = this.BackColor;
            rtbComment.BackColor = this.BackColor;
            btnSidebar.BackColor = this.BackColor;
            btnColor.FlatAppearance.MouseDownBackColor = this.BackColor;
            btnColor.FlatAppearance.MouseOverBackColor = this.BackColor;
            
            btnColor.Paint += BtnColor_Paint;
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
            
            // The font size is stored in the rich text format string itself.
            // Get rid of all formatting.
            rtbComment.Rtf = keyframe.Comments;
            string text = rtbComment.Text;
            rtbComment.Text = text;

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
            btnSidebar.BackColor = isSelected ? keyframe.Color : this.BackColor;
            rtbComment.BackColor = isSelected ? Color.White : this.BackColor;
            pnlComment.BackColor = rtbComment.BackColor;
        }
        #endregion

        private void BtnColor_Paint(object sender, PaintEventArgs e)
        {
            if (keyframe == null)
                return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = new Rectangle(1, 1, btnColor.Width - 2, btnColor.Height - 2);
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
                RaiseUpdated();
                AfterColorChange();
            }
            picker.Dispose();
        }

        private void tbName_TextChanged(object sender, EventArgs e)
        {
            if (keyframe == null || manualUpdate)
                return;

            if (string.IsNullOrEmpty(tbName.Text.Trim()))
            {
                keyframe.Title = "";
                manualUpdate = true;
                tbName.Text = keyframe.Title;
                manualUpdate = false;
            }
            else
            {
                keyframe.Title = tbName.Text;
            }

            RaiseUpdated();
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
            RaiseUpdated();
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
            RaiseSelected();
        }

        private void KeyframeCommentBox_Click(object sender, EventArgs e)
        {
            RaiseSelected();
        }

        private void RaiseUpdated()
        {
            Updated?.Invoke(this, new EventArgs<Guid>(keyframe.Id));
        }

        private void RaiseSelected()
        {
            Selected?.Invoke(this, new TimeEventArgs(keyframe.Position));
        }
    }
}
