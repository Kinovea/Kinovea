using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// Allow the user to choose a Line Style.
	/// Color will always be Black here. For a complete Line Style,
	/// The result must be combined with a color we got from elsewhere.
	/// </summary>
    public partial class StaticStylePicker : UserControl
    {
        #region Properties
        public LineStyle PickedStyle
        {
            get { return m_PickedStyle; }
        }
        /// <summary>
        /// ToolType is used to configure the Style Picker display.
        /// See ConfigureStyleButtons to see the possible values.
        /// </summary>
        public DrawingToolType ToolType
        {
            get { return m_ToolType; }
            set
            {
                m_ToolType = value;
                ConfigureStyleButtons();
            }
        }
        #endregion

        #region Events
        public delegate void DelegateStylePicked(object sender, EventArgs e);
        [Category("Action"), Browsable(true)]
        public event DelegateStylePicked StylePicked;

        public delegate void DelegateMouseLeft(object sender, EventArgs e);
        [Category("Mouse"), Browsable(true)]
        public event DelegateMouseLeft MouseLeft;
        #endregion

        #region Members
        private LineStyle m_PickedStyle;
        private DrawingToolType m_ToolType;
        private int m_iRows = 3;
        private int m_iCols = 4;
        private int m_iRowHeight = 25;
        private int m_iColWidth = 40;
        private int m_iMargin = 1;
        #endregion

        #region Constructor
        public StaticStylePicker() : this(DrawingToolType.Line2D)
        {
        }
        public StaticStylePicker(DrawingToolType _dtt)
        {
            InitializeComponent();
            m_ToolType = _dtt;
            ConfigureStyleButtons();
        }
        #endregion

        private void ConfigureStyleButtons()
        {
            m_PickedStyle = LineStyle.DefaultValue;

            this.SuspendLayout();
            switch(m_ToolType)
            {
                case DrawingToolType.Pencil:
                    ConfigureButtonsForBrushes();
                    break;
                case DrawingToolType.Cross2D:// /!\ This is actually used for Track Line.
                   	ConfigureButtonsForTrackLines();
                    break;
                case DrawingToolType.Line2D:
                default:
                    ConfigureButtonsForLines();
                    break;
            }
            this.ResumeLayout(true);
        }
        private void ConfigureButtonsForBrushes()
        {
            Controls.Clear();
            m_iRows = 3;
            m_iCols = 4;
            m_iRowHeight = 30;
            m_iColWidth = 30;

            // Possible values. (/!\ Must be as many as cells)
            int[] values = {  2,  3,  4, 5,
                              7,  9, 11, 13,
                             16, 19, 22, 25  };

            for (int i = 0; i < m_iRows; i++)
            {
                for (int j = 0; j < m_iCols; j++)
                {
                    Button btn = GetDefaultButton(m_iColWidth, m_iRowHeight);
                    btn.Location = new Point(m_iMargin + j * m_iColWidth, m_iMargin + i * m_iRowHeight);
                    
                    // Put the value into the Tag field so we can let them draw themselves.
					btn.Tag = new LineStyle(values[i * m_iCols + j], LineShape.Simple, Color.Black);
                    
                    this.Controls.Add(btn);
                    btn.BringToFront();
                }
            }

            // total size of the control
            this.Size = new Size(m_iCols * m_iColWidth + (m_iMargin * 2), m_iRows * m_iRowHeight + (m_iMargin * 2));
        }
        private void ConfigureButtonsForLines()
        {
            Controls.Clear();
            m_iRows = 3;
            m_iCols = 4;
            m_iRowHeight = 25;
            m_iColWidth = 40;

            // Possible values. (/!\ Must be as many as cells)
            int[] values = {  1, 3, 5, 7,
                              1, 3, 5, 7,
                              1, 3, 5, 7  };

            for (int i = 0; i < m_iRows; i++)
            {
                for (int j = 0; j < m_iCols; j++)
                {
                    Button btn = GetDefaultButton(m_iColWidth, m_iRowHeight);
                    btn.Location = new Point(m_iMargin + j * m_iColWidth, m_iMargin + i * m_iRowHeight);

                    int width = values[i * m_iCols + j];

                    // Specify Arrows...
                    switch(i)
                    {
                    	case 0:
                            btn.Tag = new LineStyle(width, LineShape.Simple, Color.Black);
                            break;
                        case 1:
                            btn.Tag = new LineStyle(width, LineShape.EndArrow, Color.Black);
                            break;
                        case 2:
                            btn.Tag = new LineStyle(width, LineShape.DoubleArrow, Color.Black);
                            break;
                    }

                    this.Controls.Add(btn);
                    btn.BringToFront();
                }
            }

            // total size of the control
            this.Size = new Size(m_iCols * m_iColWidth + (m_iMargin * 2), m_iRows * m_iRowHeight + (m_iMargin * 2));
        }
        private void ConfigureButtonsForTrackLines()
        {
            Controls.Clear();
            m_iRows = 3;
            m_iCols = 4;
            m_iRowHeight = 25;
            m_iColWidth = 50;

            // Possible values. (/!\ Must be as many as cells)
            int[] values = {  	1, 3, 5, 7,
                              	2, 3, 5, 7,
                              	2, 3, 5, 7 };

            for (int i = 0; i < m_iRows; i++)
            {
                for (int j = 0; j < m_iCols; j++)
                {
                    Button btn = GetDefaultButton(m_iColWidth, m_iRowHeight);
                    btn.Location = new Point(m_iMargin + j * m_iColWidth, m_iMargin + i * m_iRowHeight);

                    int width = values[i * m_iCols + j];

                    // Specify style
                    switch (i)
                    {
                        case 0:
                            btn.Tag = new LineStyle(width, LineShape.Simple, Color.Black);
                            break;
                        case 1:
                            btn.Tag = new LineStyle(width, LineShape.Dash, Color.Black);
                            break;
                        case 2:
                            btn.Tag = new LineStyle(width, LineShape.DashDot, Color.Black);
                            break;
                    }

                    this.Controls.Add(btn);
                    btn.BringToFront();
                }
            }

            // total size of the control
            this.Size = new Size(m_iCols * m_iColWidth + (m_iMargin * 2), m_iRows * m_iRowHeight + (m_iMargin * 2));
        }
        private Button GetDefaultButton(int _iColWidth, int _iRowHeight)
        {
            Button btn = new Button();

            // Common properties
            btn.BackColor = Color.White;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.BorderColor = Color.LightGray;
            btn.FlatAppearance.MouseDownBackColor = Color.Transparent;
            btn.FlatAppearance.MouseOverBackColor = btn.BackColor;
            btn.FlatStyle = FlatStyle.Flat;
            btn.Size = new Size(_iColWidth, _iRowHeight);
            btn.UseVisualStyleBackColor = false;
            btn.Visible = true;

            // Common Event Handlers
            btn.Click += new EventHandler(btn_Click);
            btn.MouseEnter += new EventHandler(btn_MouseEnter);
            btn.MouseLeave += new EventHandler(btn_MouseLeave);
            btn.Paint += new PaintEventHandler(btn_Paint);

            return btn;
        }

        #region Buttons common event handlers
        private void btn_Click(object sender, EventArgs e)
        {
            if (sender is Button)
            {
                m_PickedStyle = (LineStyle) ((Button)sender).Tag;
                if (StylePicked != null)
                {
                    StylePicked(this, EventArgs.Empty);
                }
            }
        }
        private void btn_MouseEnter(object sender, EventArgs e)
        {
            if (sender is Button)
            {
                ((Button)sender).FlatAppearance.BorderSize = 1;
            }
        }
        private void btn_MouseLeave(object sender, EventArgs e)
        {
            if (sender is Button)
            {
                ((Button)sender).FlatAppearance.BorderSize = 0;
            }
             
            // If we quit the whole control, we generate a global mouse leave.
            Point clientMouse = PointToClient(Control.MousePosition);
            Rectangle rect = new Rectangle(this.ClientRectangle.X, this.ClientRectangle.Y, this.ClientRectangle.Width, this.ClientRectangle.Height);
            if (!rect.Contains(clientMouse))
            {
                if (MouseLeft != null)
                {
                    MouseLeft(this, EventArgs.Empty);
                }
            }
        }
        private void btn_Paint(object sender, PaintEventArgs e)
        {
            // Ask each and every preconfigured style to draw itself on its button's canvas.
            Button btn = (Button)sender;
            LineStyle stl = (LineStyle)btn.Tag;
            stl.Draw(e.Graphics, m_ToolType == DrawingToolType.Pencil, Color.Black);
        }
        #endregion

        private void StaticStylePicker_MouseLeave(object sender, EventArgs e)
        {
            if (MouseLeft != null)
            {
                MouseLeft(this, EventArgs.Empty);
            }
        }
    }
}
