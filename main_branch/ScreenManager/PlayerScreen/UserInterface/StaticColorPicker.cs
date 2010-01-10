using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    public partial class StaticColorPicker : UserControl
    {
        #region Properties
        public Color PickedColor
        {
            get { return m_PickedColor; }
        }
        #endregion

        #region Events
        public delegate void DelegateColorPicked(object sender, EventArgs e);
        [Category("Action"), Browsable(true)]
        public event DelegateColorPicked ColorPicked;

        public delegate void DelegateMouseLeft(object sender, EventArgs e);
        [Category("Mouse"), Browsable(true)]
        public event DelegateMouseLeft MouseLeft;
        #endregion

        #region Members
        private Color m_PickedColor;
        #endregion

        #region Constructor
        public StaticColorPicker()
        {
            m_PickedColor = Color.Black;

            InitializeComponent();
            
            // All the buttons share the same eventhandler.
            foreach (Control btn in this.Controls)
            {
                if (btn is Button)
                {
                    ((Button)btn).FlatAppearance.MouseOverBackColor = ((Button)btn).BackColor;
                    ((Button)btn).FlatAppearance.BorderColor = Color.White;

                    ((Button)btn).Click += new EventHandler(btn_Click);
                    ((Button)btn).MouseEnter += new EventHandler(btn_MouseEnter);
                    ((Button)btn).MouseLeave += new EventHandler(btn_MouseLeave);
                }
            }
        }
        #endregion

        private void btn_Click(object sender, EventArgs e)
        {
            if (sender is Button)
            {
                m_PickedColor = ((Button)sender).BackColor;
                if (ColorPicked != null)
                {
                    ColorPicked(this, EventArgs.Empty);
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
                
                // If we quit the whole control, we generate a global mouse leave.
                // (The panel will not fire it since there are buttons everywhere.)
                Point clientMouse = PointToClient(Control.MousePosition);
                //Rectangle rect = new Rectangle(this.ClientRectangle.X - 5, this.ClientRectangle.Y - 5, this.ClientRectangle.Width + 10, this.ClientRectangle.Height + 10);
                Rectangle rect = new Rectangle(this.ClientRectangle.X, this.ClientRectangle.Y, this.ClientRectangle.Width, this.ClientRectangle.Height);
                if (!rect.Contains(clientMouse))
                {
                    if (MouseLeft != null)
                    {
                        MouseLeft(this, EventArgs.Empty);
                    }
                }
            }
        }
    }
}
