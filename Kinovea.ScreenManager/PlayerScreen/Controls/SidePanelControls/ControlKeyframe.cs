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
using Kinovea.ScreenManager.Properties;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// This controls holds the keyframe name, color, timecode and comment.
    /// It is used in the side panel.
    /// 
    /// Undo/redo mechanics:
    /// After any change or re-init, capture the new state to a global memento.
    /// When making a new change, push the memento containing the previous state to the history stack.
    /// </summary>
    public partial class ControlKeyframe : UserControl
    {
        #region Events
        /// <summary>
        /// Asks the main timeline to move to the time of this keyframe.
        /// </summary>
        public event EventHandler<TimeEventArgs> Selected;

        /// <summary>
        /// Tells the main timeline that this keyframe was updated.
        /// </summary>
        public event EventHandler<EventArgs<Guid>> Updated;

        /// <summary>
        /// Delete this keyframe.
        /// </summary>
        public event EventHandler<EventArgs<Guid>> DeletionAsked;
        
        #endregion

        #region Properties
        public Keyframe Keyframe
        {
            get { return keyframe; }
        }

        /// <summary>
        /// Returns true if any text editor is being edited.
        /// This must be consulted before triggering a shortcut that would conflict with text input.
        /// </summary>
        public bool Editing
        {
            get { return editingName || editingComment; }
        }
        #endregion

        #region Members
        private Metadata metadata;
        private Keyframe keyframe;
        private bool editingName;
        private bool editingComment;
        private bool manualUpdate;
        private bool isSelected;
        private Pen penBorder = Pens.Silver;
        private HistoryMementoModifyKeyframe memento;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public ControlKeyframe()
        {
            InitializeComponent();
            this.BackColor = Color.WhiteSmoke;
            HomogenizeBackColor();
            pbThumbnail.SizeMode = PictureBoxSizeMode.CenterImage;
            pbThumbnail.BackColor = Color.FromArgb(42, 42, 42);

            this.Paint += KeyframeCommentBox_Paint;
            btnColor.Paint += BtnColor_Paint;
            rtbComment.MouseWheel += RtbComment_MouseWheel;

            AfterColorChange();
        }
        #endregion

        #region Public methods

        /// <summary>
        /// Set the keyframe this control is wrapping.
        /// </summary>
        public void SetKeyframe(Metadata metadata, Keyframe keyframe)
        {
            this.metadata = metadata;
            this.keyframe = keyframe;
            if (keyframe == null)
                return;

            UpdateContent();
            CaptureCurrentState();
        }

        /// <summary>
        /// Update the highlight status based on the current timestamp.
        /// </summary>
        public void UpdateHighlight(long timestamp)
        {
            if (keyframe == null)
                return;

            isSelected = keyframe.Timestamp == timestamp;

            AfterColorChange();
            this.BackColor = isSelected ? Color.WhiteSmoke : Color.White;
            HomogenizeBackColor();
            rtbComment.BackColor = isSelected ? Color.White : this.BackColor;
            pnlComment.BackColor = rtbComment.BackColor;
        }

        /// <summary>
        /// Update the timecode after a change in time calibration.
        /// </summary>
        public void UpdateTimecode()
        {
            if (keyframe == null)
                return;

            lblTimecode.Text = keyframe.TimeCode;
        }

        /// <summary>
        /// Update title, color or comments after an external change to the underlying keyframe.
        /// </summary>
        public void UpdateContent()
        {
            if (keyframe == null)
                return;

            manualUpdate = true;

            tbName.Text = keyframe.Name;
            AfterNameChange();
            lblTimecode.Text = keyframe.TimeCode;

            // The font size is stored in the rich text format string itself.
            // Get rid of all formatting.
            string text = TextHelper.GetText(keyframe.Comments);
            rtbComment.Text = text;
            btnComments.Visible = rtbComment.TextLength == 0;
            UpdateTextHeight();

            AfterColorChange();
            pbThumbnail.Image = keyframe.Thumbnail;

            manualUpdate = false;

            // Capture the new state as the baseline, even if this is coming from undo.
            CaptureCurrentState();
        }
        #endregion

        private void HomogenizeBackColor()
        {
            rtbComment.BackColor = this.BackColor;
            tbName.BackColor = this.BackColor;
            btnColor.BackColor = this.BackColor;
            btnColor.FlatAppearance.MouseDownBackColor = this.BackColor;
            btnColor.FlatAppearance.MouseOverBackColor = this.BackColor;
        }

        private void RtbComment_MouseWheel(object sender, MouseEventArgs e)
        {
            ((HandledMouseEventArgs)e).Handled = true;
            this.OnMouseWheel(e);
        }

        private void KeyframeCommentBox_Paint(object sender, PaintEventArgs e)
        {
            Rectangle rect = new Rectangle(0, 0, this.ClientRectangle.Width - 1, this.ClientRectangle.Height - 1);
            e.Graphics.DrawRectangle(penBorder, rect);
        }

        private void btnColor_Click(object sender, EventArgs e)
        {
            if (keyframe == null || manualUpdate)
                return;

            // We should have a memento ready at this point.

            FormColorPicker picker = new FormColorPicker(keyframe.Color);
            FormsHelper.Locate(picker);
            if (picker.ShowDialog() == DialogResult.OK)
            {
                if (picker.PickedColor != keyframe.Color)
                {
                    keyframe.Color = picker.PickedColor;
                    AfterStateChanged();
                }
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
                // We can't allow an empty string so fall back to the timecode.
                keyframe.Name = "";
                manualUpdate = true;
                tbName.Text = keyframe.Name;
                manualUpdate = false;
            }
            else
            {
                keyframe.Name = tbName.Text;
            }

            RaiseUpdated();
            AfterNameChange();
            AfterStateChanged();
        }

        private void rtbComment_TextChanged(object sender, EventArgs e)
        {
            UpdateTextHeight();

            if (keyframe == null || manualUpdate)
                return;

            keyframe.Comments = rtbComment.Rtf;
            btnComments.Visible = rtbComment.TextLength == 0;
            RaiseUpdated();
            AfterStateChanged();
        }

        private void AfterNameChange()
        {
            Size size = TextRenderer.MeasureText(tbName.Text, tbName.Font);
            tbName.Width = size.Width;
            tbName.Height = size.Height;
        }

        /// <summary>
        /// The main keyframe color was changed.
        /// </summary>
        private void AfterColorChange()
        {
            if (keyframe == null)
                return;

            btnColor.Invalidate();
            Color mainColor = isSelected ? keyframe.Color : this.BackColor;
            btnSidebar.BackColor = mainColor;
            btnSidebar.FlatAppearance.MouseDownBackColor = mainColor;
            btnSidebar.FlatAppearance.MouseOverBackColor = mainColor;
            
            btnTopbar.BackColor = mainColor;
            btnTopbar.FlatAppearance.MouseDownBackColor = mainColor;
            btnTopbar.FlatAppearance.MouseOverBackColor = mainColor;
            
            btnRightBar.BackColor = mainColor;
            btnRightBar.FlatAppearance.MouseDownBackColor = mainColor;
            btnRightBar.FlatAppearance.MouseOverBackColor = mainColor;

            btnBottomBar.BackColor = mainColor;
            btnBottomBar.FlatAppearance.MouseDownBackColor = mainColor;
            btnBottomBar.FlatAppearance.MouseOverBackColor = mainColor;
        }

        /// <summary>
        /// Push the previously saved memento to the history stack, and capture the new current state.
        /// This should be called after making any undoable change to the data.
        /// </summary>
        private void AfterStateChanged()
        {
            metadata.HistoryStack.PushNewCommand(memento);
            CaptureCurrentState();
        }

        /// <summary>
        /// Capture the current state to a memento.
        /// This may be pushed to the history stack later if we change state again.
        /// This should be called when the data is initialized or changed from the outside.
        /// </summary>
        private void CaptureCurrentState()
        {
            memento = new HistoryMementoModifyKeyframe(metadata, keyframe.Id);
        }

        private void UpdateTextHeight()
        {
            // Manually update the textbox height and manually grow the containers.
            // Other approaches tried:
            // - Having the container controls on autosize. Broke during init.
            // - Listening to ContentsResized event. Doesn't work with wordwrap.
            // - Using GetPreferredSize. 
            //
            // The setup for this is to have wordwrap, no scrollbars (None), anchors top-left-right.
            // Then coming here we manually compute the height of the control, and make sure it fits
            // the content. Then grow the containers.

            // Grow richtextbox.
            const int padding = 3;
            int numLines = rtbComment.GetLineFromCharIndex(rtbComment.TextLength) + 1;
            int border = rtbComment.Height - rtbComment.ClientSize.Height;
            rtbComment.Height = (rtbComment.Font.Height + 3) * numLines + border;

            // Grow containers.
            int paddingBottom = 6;
            pnlComment.Height = rtbComment.Top + rtbComment.Height + rtbComment.Margin.Bottom + padding;
            this.Height = pnlComment.Top + pnlComment.Height + pnlComment.Margin.Bottom + padding + paddingBottom;
        }

        private void BtnColor_Paint(object sender, PaintEventArgs e)
        {
            if (keyframe == null)
                return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = new Rectangle(1, 1, btnColor.Width - 2, btnColor.Height - 2);
            using (SolidBrush brush = new SolidBrush(keyframe.Color))
                e.Graphics.FillEllipse(brush, rect);
        }
        private void tbName_Enter(object sender, EventArgs e)
        {
            editingName = true;
        }

        private void tbName_Leave(object sender, EventArgs e)
        {
            editingName = false;
        }

        private void rtbComment_Enter(object sender, EventArgs e)
        {
            editingComment = true;

            btnComments.Visible = false;
        }

        private void rtbComment_Leave(object sender, EventArgs e)
        {
            editingComment = false;
            btnComments.Visible = rtbComment.TextLength == 0;
        }

        private void KeyframeCommentBox_Enter(object sender, EventArgs e)
        {
            RaiseSelected();
        }

        private void KeyframeCommentBox_Click(object sender, EventArgs e)
        {
            RaiseSelected();
        }

        private void pbThumbnail_Click(object sender, EventArgs e)
        {
            // Take the focus out of the text boxes. Removes the annoying flashing caret.
            pbThumbnail.Focus();
            RaiseSelected();
        }

        private void RaiseUpdated()
        {
            Updated?.Invoke(this, new EventArgs<Guid>(keyframe.Id));
        }

        private void RaiseSelected()
        {
            Selected?.Invoke(this, new TimeEventArgs(keyframe.Timestamp));
        }

        private void KeyframeCommentBox_Resize(object sender, EventArgs e)
        {
            if (manualUpdate)
                return;

            manualUpdate = true;

            UpdateTextHeight();

            manualUpdate = false;
        }

        private void btnClose_MouseEnter(object sender, EventArgs e)
        {
            btnClose.BackgroundImage = Resources.close_square_red;
        }

        private void btnClose_MouseLeave(object sender, EventArgs e)
        {
            btnClose.BackgroundImage = Resources.close_square;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            // Delete the keyframe.
            DeletionAsked?.Invoke(this, new EventArgs<Guid>(keyframe.Id));
        }
    }
}
