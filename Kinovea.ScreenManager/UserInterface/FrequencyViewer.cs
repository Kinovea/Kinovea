/*
Copyright © Joan Charmant 2008.
jcharmant@gmail.com 
 
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
using System.Drawing;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    public partial class FrequencyViewer : UserControl
    {
        // The values here are completely uncorrelated with the real values.

        #region Properties
        public int HorizontalLines
        {
            get { return m_iHorizontalLines; }
            set
            {
                if (value < 1) value = 1;
                m_iHorizontalLines = value;
                Invalidate();
            }
        }
        public int Total
        {
            get { return m_iTotal; }
            set
            {
                if (value < m_iInterval) value = m_iInterval;
                m_iTotal = value;
                Invalidate();
            }
        }
        public int Interval
        {
            get { return m_iInterval; }
            set
            {
                if (value < 1) value = 1;
                m_iInterval = value;
                Invalidate();
            }
        }
        #endregion

        #region Members
        private int m_iHorizontalLines;
        private int m_iTotal;
        private int m_iInterval;
        #endregion

        public FrequencyViewer()
        {
            InitializeComponent();

            this.SetStyle( ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);

            m_iHorizontalLines = 5;
            m_iTotal = 10000;
            m_iInterval = 1000;
        }
        private void FrequencyViewer_Paint(object sender, PaintEventArgs e)
        {
            //-------------------
            // Drawing the lines.
            //-------------------

            // 1. Horizontal lines
            for (int i = 0; i < m_iHorizontalLines; i++)
            {
                e.Graphics.DrawLine(Pens.Gray, 0, (this.Height / m_iHorizontalLines) * i, this.Width, (this.Height / m_iHorizontalLines) * i);
            }
            e.Graphics.DrawLine(Pens.Gray, 0, this.Height - 1, this.Width, this.Height - 1);

            // 2. Vertical lines (the real visual information)
            for (int i = 0; i < m_iTotal / m_iInterval; i++)
            {
                int iAbscisse = i * m_iInterval;
                int iLineX = (int)(((double)iAbscisse * (double)this.Width) / (double)m_iTotal);
                e.Graphics.DrawLine(Pens.Gray, iLineX, 0, iLineX, this.Height);
            }
            e.Graphics.DrawLine(Pens.Gray, this.Width - 1, 0, this.Width - 1, this.Height);
        }
    }
}
