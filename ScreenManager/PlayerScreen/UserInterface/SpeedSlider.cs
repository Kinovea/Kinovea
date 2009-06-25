/*
Copyright © Joan Charmant 2008.
joan.charmant@gmail.com 
 
This file is part of Kinovea.

Kinovea is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License version 2 
as published by the Free Software Foundation.

Kinovea is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Kinovea. If not, see http://www.gnu.org/licenses/.

*/


using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    public partial class SpeedSlider : UserControl
    {
        #region Properties
        [Category("Behavior"), Browsable(true)]
        public int Minimum
        {
            get
            {
                return m_iMinimum;
            }
            set
            {
                m_iMinimum = value;
            }
        }
        [Category("Behavior"), Browsable(true)]
        public int Maximum
        {
            get
            {
                return m_iMaximum;
            }
            set
            {
                m_iMaximum = value;
            }
        }
        [Category("Behavior"), Browsable(true)]
        public int Value
        {
            get
            {
                return m_iValue;
            }
            set
            {
                if (value < m_iMinimum) value = m_iMinimum;
                if (value > m_iMaximum) value = m_iMaximum;

                m_iValue = value;

                // -> Déplacer le curseur.
                if (m_iMaximum - m_iMinimum > 0)
                {
                    SuspendLayout();
                    btnCursor.Left = btnRail.Left + Rescale(m_iValue - m_iMinimum, m_iMaximum - m_iMinimum, btnRail.Width);
                    ResumeLayout();
                }

                if (ValueChanged != null)
                {
                    ValueChanged(this, EventArgs.Empty);
                }
            }
        }
        [Category("Misc"), Browsable(true)]
        public string ToolTip
        {
            set
            {
                toolTips.SetToolTip(btnCursor, value);
                toolTips.SetToolTip(btnRail, value);
            }
        }
        [Category("Behavior"), Browsable(true)]
        public int SmallChange
        {
            get
            {
                return m_iSmallChange;
            }
            set
            {
                m_iSmallChange = value;
            }
        }
        [Category("Behavior"), Browsable(true)]
        public int LargeChange
        {
            get
            {
                return m_iLargeChange;
            }
            set
            {
                m_iLargeChange = value;
            }
        }
        [Category("Behavior"), Browsable(true)]
        public int StickyValue
        {
            get
            {
                return m_iStickyValue;
            }
            set
            {
                m_iStickyValue = value;
            }
        }
        #endregion

        #region Members
        private int m_iMinimum = 0;
        private int m_iValue = 0;
        private int m_iMaximum = 0;
        private int m_iSmallChange = 1;
        private int m_iLargeChange = 5;
        private int m_iStickyValue = 0;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region EventDelegates
        // Déclarations de Types
        public delegate void ValueChangedHandler(object sender, EventArgs e);

        // Déclarations des évènements
        [Category("Action"), Browsable(true)]
        public event ValueChangedHandler ValueChanged;

        #endregion

        #region Ctor
        public SpeedSlider()
        {
            InitializeComponent();
            this.BackColor = Color.White;
        }
        #endregion

        #region Internal Events
        private void btnCursor_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int iTargetPosition = btnCursor.Left + e.X;
                if((iTargetPosition > btnRail.Left) && (iTargetPosition <= btnRail.Left + btnRail.Width))
                {
                    btnCursor.Left = iTargetPosition - (btnCursor.Width / 2);
                    SetNewValue();
                }
            }
        }
        private void btnRail_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                btnCursor.Left = btnRail.Left + e.X - (btnCursor.Width / 2);
                SetNewValue();
            }
        }
        private void btnRail_MouseMove(object sender, MouseEventArgs e)
        {
            if ((e.Button == MouseButtons.Left) && (e.X > 0) && (e.X <= btnRail.Width))
            {
                SuspendLayout();
                btnCursor.Left = btnRail.Left + e.X - (btnCursor.Width / 2);
                ResumeLayout();
                SetNewValue();
            }
        }
        private void btnDecrease_MouseClick(object sender, MouseEventArgs e)
        {
            // -> Déplacer le curseur.
            if (m_iMaximum - m_iMinimum > 0 && m_iValue > m_iMinimum + m_iLargeChange)
            {
                m_iValue -= m_iLargeChange;

                btnCursor.Left = btnRail.Left + Rescale(m_iValue - m_iMinimum, m_iMaximum - m_iMinimum, btnRail.Width);

                if (ValueChanged != null)
                {
                    ValueChanged(this, EventArgs.Empty);
                }
            }

        }
        private void btnIncrease_MouseClick(object sender, MouseEventArgs e)
        {
            // -> Déplacer le curseur.
            if (m_iMaximum - m_iMinimum > 0 && m_iValue <= m_iMaximum - m_iLargeChange)
            {
                m_iValue += m_iLargeChange;

                btnCursor.Left = btnRail.Left + Rescale(m_iValue - m_iMinimum, m_iMaximum - m_iMinimum, btnRail.Width);

                if (ValueChanged != null)
                {
                    ValueChanged(this, EventArgs.Empty);
                }
            }
        }
        #endregion
        
        private void SetNewValue()
        {
            m_iValue = Rescale(btnCursor.Left + (btnCursor.Width / 2) - btnRail.Left, btnRail.Width, (m_iMaximum - m_iMinimum));

            m_iValue += m_iMinimum;
            
            // Stickiness
            if (
                (m_iValue >= (m_iStickyValue - (2*m_iSmallChange))) && (m_iValue <= m_iStickyValue) ||
                (m_iValue <= (m_iStickyValue + (2*m_iSmallChange))) && (m_iValue >= m_iStickyValue)
                )
            {
                m_iValue = m_iStickyValue;
            }
            
            if (ValueChanged != null) 
            { 
                ValueChanged(this, EventArgs.Empty);
            }
        }
        private int Rescale(long _iValue, long _iOldMax, long _iNewMax)
        {
            // Rescale : Pixels -> Values
            return (int)(Math.Round((double)((double)_iValue * (double)_iNewMax) / (double)_iOldMax));
        }

        

        
    }
}
