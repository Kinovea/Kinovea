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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    public partial class FrameTracker : UserControl
    {

        #region Properties
        [Category("Behavior"), Browsable(true)]
        public long Minimum
        {
            get
            {
                return m_iMinimum;
            }
            set
            {
                m_iMinimum = value;

                if (m_iPosition < m_iMinimum) { m_iPosition = m_iMinimum; }

                // -> Déplacer le curseur.
                if (m_iMaximum - m_iMinimum > 0)
                {
                    NavCursor.Left = BumperLeft.Width + Rescale(m_iPosition - m_iMinimum, m_iMaximum - m_iMinimum, m_iMaxWidth);
                }
            }
        }
        [Category("Behavior"), Browsable(true)]
        public long Maximum
        {
            get
            {
                return m_iMaximum;
            }
            set
            {
                m_iMaximum = value;

                if (m_iPosition > m_iMaximum) { m_iPosition = m_iMaximum; }
                
                // -> Déplacer le curseur.
                if (m_iMaximum - m_iMinimum > 0)
                {
                    NavCursor.Left = BumperLeft.Width + Rescale(m_iPosition - m_iMinimum, m_iMaximum - m_iMinimum, m_iMaxWidth);
                }
            }
        }
        [Category("Behavior"), Browsable(true)]
        public long Position
        {
            get
            {
                return m_iPosition;
            }
            set
            {
                //-----------------------------------------------------------
                // /!\ Positionner manuellement la position ne déclenche pas
                // l'event en retour.
                // Ne fait que déplacer le curseur.
                //-----------------------------------------------------------
                if (value < m_iMinimum) value = m_iMinimum;
                if (value > m_iMaximum) value = m_iMaximum;

                m_iPosition  = value;

                // -> Déplacer le curseur.
                if (m_iMaximum - m_iMinimum > 0)
                {
                    NavCursor.Left = BumperLeft.Width + Rescale(m_iPosition - m_iMinimum, m_iMaximum - m_iMinimum, m_iMaxWidth);
                }
            }
        }
        [Category("Behavior"), Browsable(true)]
        public bool ReportOnMouseMove
        {
            get { return m_bReportOnMouseMove;  }
            set { m_bReportOnMouseMove = value; }
        }
        public long[] KeyframesTimestamps
        {
            set
            {
                if (m_KeyframesTimestamps == null)
                {
                    m_KeyframesTimestamps = new long[value.Length];
                }
                else if (m_KeyframesTimestamps.Length != value.Length)
                {
                    // dispose ?
                    m_KeyframesTimestamps = new long[value.Length];
                }

                for (int i = 0; i < value.Length; i++)
                {
                    m_KeyframesTimestamps[i] = value[i];
                }

                UpdateMarkers();
            }
        }
        #endregion

        #region Members
        private long m_iMinimum = 0;
        private long m_iPosition = 0;
        private long m_iMaximum = 0;
        private int m_iMaxWidth = 0;
        private bool m_bReportOnMouseMove = false;
        private long[] m_KeyframesTimestamps;
        private int[] m_KeyframesMarks;
       	private static readonly Pen m_PenKeyImageMark = new Pen(Color.FromArgb(128, Color.YellowGreen), 2);
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region EventDelegates
        // Déclarations de Types
        public delegate void PositionChangingHandler(object sender, long _iPosition);
        public delegate void PositionChangedHandler(object sender, long _iPosition);

        // Déclarations des évènements
        [Category("Action"), Browsable(true)]
        public event PositionChangingHandler PositionChanging;
        [Category("Action"), Browsable(true)]
        public event PositionChangedHandler PositionChanged;
        #endregion

        public FrameTracker()
        {
            InitializeComponent();
            m_iMaxWidth = this.Width - BumperLeft.Width - BumperRight.Width - NavCursor.Width;
        }

        #region Manipulation du contrôle
        private void NavCursor_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // Déplacer le curseur
                // GlobalMouseX correspond à la coordonnée dans le panelNavigation.
                int GlobalMouseX = e.X + NavCursor.Left;
                int OldCursorLeft = NavCursor.Left;

                // Empécher d'aller trop loin à droite et à gauche
                if ((GlobalMouseX < (this.Width - (NavCursor.Width / 2) - BumperRight.Width)) &&
                     (GlobalMouseX - (NavCursor.Width / 2) - BumperLeft.Width > 0))
                {
                    NavCursor.Left = GlobalMouseX - (NavCursor.Width / 2);

                    if (m_bReportOnMouseMove)
                    {
                        m_iPosition = m_iMinimum + Rescale(NavCursor.Left - BumperLeft.Width, m_iMaxWidth, m_iMaximum - m_iMinimum);
                        if (PositionChanging != null) { PositionChanging(this, m_iPosition); }
                    }
                }
            }
        }
        private void FrameTracker_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Point MouseCoords = this.PointToClient(Cursor.Position);
                if ((MouseCoords.X > BumperLeft.Width + (NavCursor.Width / 2)) &&
                    (MouseCoords.X < this.Width - BumperRight.Width - (NavCursor.Width / 2)))
                {
                    NavCursor.Left = MouseCoords.X - (NavCursor.Width / 2);
                    UpdateValue();
                }
            }
        }
        private void NavCursor_MouseUp(object sender, MouseEventArgs e)
        {
            UpdateValue();
        }
        #endregion

        #region Binding UI to Data
        private void FrameTracker_Resize(object sender, EventArgs e)
        {
            m_iMaxWidth = this.Width - BumperLeft.Width - BumperRight.Width - NavCursor.Width;
            UpdateNavCursor();
            UpdateMarkers();
        }
        private void UpdateValue()
        {
            m_iPosition = m_iMinimum + Rescale(NavCursor.Left - BumperLeft.Width, m_iMaxWidth, m_iMaximum - m_iMinimum);
            if (PositionChanged != null) { PositionChanged(this, m_iPosition); }
        }
        private int Rescale(long _iOldValue, long _iOldMax, long _iNewMax)
        {
            // Rescale : Pixels -> Values
            return (int)(Math.Round((double)((double)_iOldValue * (double)_iNewMax) / (double)_iOldMax));
        }
        private void UpdateNavCursor()
        {
            //------------------------------------------------
            // Redessine en fonction des données.
            // (Au chargement ou sur Resize)
            //------------------------------------------------
            if (m_iMaximum - m_iMinimum > 0)
            {
                NavCursor.Left = BumperLeft.Width + Rescale(m_iPosition - m_iMinimum, m_iMaximum - m_iMinimum, m_iMaxWidth);
            }
        }
        private void UpdateMarkers()
        {
            if (m_KeyframesTimestamps != null)
            {
                if (m_KeyframesMarks == null)
                {
                    m_KeyframesMarks = new int[m_KeyframesTimestamps.Length];
                }
                else if (m_KeyframesTimestamps.Length != m_KeyframesMarks.Length)
                {
                    // dispose ?
                    m_KeyframesMarks = new int[m_KeyframesTimestamps.Length];
                }

                // Assign new values.
                for (int i = 0; i < m_KeyframesTimestamps.Length; i++)
                {
                    m_KeyframesMarks[i] = (NavCursor.Width / 2) + BumperLeft.Width + Rescale(m_KeyframesTimestamps[i] - m_iMinimum, m_iMaximum - m_iMinimum, m_iMaxWidth);
                }
            }
        }
        #endregion

        private void FrameTracker_Paint(object sender, PaintEventArgs e)
        {
            if (m_KeyframesMarks != null)
            {
                foreach (int mark in m_KeyframesMarks)
                {
                    if (mark > 0)
                    {
                        e.Graphics.DrawLine(m_PenKeyImageMark, mark, 2, mark, this.Height - 4);
                    }
                }
            }
        }
    }
}
