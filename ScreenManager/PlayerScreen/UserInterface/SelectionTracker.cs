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

namespace Videa.ScreenManager
{
    public partial class SelectionTracker : UserControl
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
            }
        }
        [Category("Behavior"), Browsable(true)]
        public long SelStart
        {
            get
            {
                return m_iSelStart;
            }
            set
            {
                if (value < m_iMinimum) value = m_iMinimum;
                m_iSelStart = value;

                // -> Déplacer le handler.
                if (m_iMaximum - m_iMinimum > 0)
                {
                    HandlerLeft.Left = BumperLeft.Width + Rescale(m_iSelStart - m_iMinimum, m_iMaximum - m_iMinimum, m_iMaxWidth);
                    StretchSelection();
                }

                if (SelectionChanged != null) { SelectionChanged(this, EventArgs.Empty); }
            }
        }
        [Category("Behavior"), Browsable(true)]
        public long SelEnd
        {
            get
            {
                return m_iSelEnd;
            }
            set
            {
                if (value < m_iSelStart) value = m_iSelStart;
                if (value > m_iMaximum) value = m_iMaximum;

                m_iSelEnd = value;

                // -> Déplacer le handler.
                if (m_iMaximum - m_iMinimum > 0)
                {
                    HandlerRight.Left = BumperLeft.Width + HandlerLeft.Width + Rescale(m_iSelEnd - m_iMinimum, m_iMaximum - m_iMinimum, m_iMaxWidth);
                    StretchSelection();
                }

                if (SelectionChanged != null) { SelectionChanged(this, EventArgs.Empty); }
            }
        }
        [Category("Behavior"), Browsable(true)]
        public bool SelLocked
        {
            get
            {
                return m_bSelLocked;
            }
            set
            {
                m_bSelLocked = value;
            }
        }
        [Category("Behavior"), Browsable(false)]
        public long SelPos
        {
            get 
            {
                // Only for debugging purposes !
                return m_iSelPos;
            }
            set
            {
                if (value < m_iSelStart) value = m_iSelStart;
                if (value > m_iSelEnd) value = m_iSelEnd;

                m_iSelPos = value;

                // Redessiner la tête de lecture.
                SelectedZone.Invalidate();

            }
        }
        public long SelTarget
        {
            get { return m_iSelTarget;}
        }
        [Category("Misc"), Browsable(true)]
        public string ToolTip
        {
            set
            {
                toolTips.SetToolTip(SelectedZone, value);
                toolTips.SetToolTip(this, value);
            }
        }


        #endregion

        #region Members
        private long m_iMinimum = 2;
        private long m_iMaximum = 86;

        private long m_iSelStart = 2;
        private long m_iSelEnd = 86;
        private int m_iMaxWidth = 0;        // Taille maximale réelle de la selection en pixels.

        private bool m_bSelLocked = false;

        private long m_iSelPos = 0;
        private long m_iSelTarget = 0;
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region EventDelegates
        // Déclarations de Types
        public delegate void SelectionChangingHandler(object sender, EventArgs e);
        public delegate void SelectionChangedHandler(object sender, EventArgs e);
        public delegate void TargetAcquiredHandler(object sender, EventArgs e);

        // Déclarations des évènements
        [Category("Action"), Browsable(true)]
        public event SelectionChangingHandler SelectionChanging;
        [Category("Action"), Browsable(true)]
        public event SelectionChangedHandler SelectionChanged;
        [Category("Action"), Browsable(true)]
        public event TargetAcquiredHandler TargetAcquired;
        #endregion

        #region Ctor
        public SelectionTracker()
        {
            InitializeComponent();
            m_iMaxWidth = this.Width - BumperLeft.Width - BumperRight.Width - HandlerLeft.Width - HandlerRight.Width; 
        }
        #endregion

        #region Manipulation du contrôle
        
        private void HandlerLeft_MouseMove(object sender, MouseEventArgs e)
        {
            if ((e.Button == MouseButtons.Left) && (!m_bSelLocked))
            {
                //---------------------------------------------------------------
                // Déplacer le handler
                // GlobalMouseX correspond à la coordonnée dans le contrôle
                //---------------------------------------------------------------
                int GlobalMouseX = e.X + HandlerLeft.Left;
                int OldHandlerLeft = HandlerLeft.Left;

                // Empécher d'aller trop loin à droite et à gauche
                if (((GlobalMouseX + (HandlerLeft.Width / 2)) <= HandlerRight.Left) &&
                     (GlobalMouseX - (HandlerLeft.Width / 2)  >= BumperLeft.Width))
                {
                    HandlerLeft.Left = GlobalMouseX - (HandlerLeft.Width / 2);
                    StretchSelection();

                    UpdateValues();
                }
            }
        }
        private void HandlerRight_MouseMove(object sender, MouseEventArgs e)
        {

            if ((e.Button == MouseButtons.Left) && (!m_bSelLocked))
            {
                // Déplacer le handler
                int GlobalMouseX = e.X + HandlerRight.Left;
                int OldHandlerLeft = HandlerRight.Left;

                // Empécher d'aller trop loin à droite et à gauche
                if ((GlobalMouseX + (HandlerRight.Width / 2) <= (this.Width - BumperRight.Width)) &&
                    (GlobalMouseX - (HandlerRight.Width / 2)) >= (HandlerLeft.Left + HandlerLeft.Width))
                {
                    HandlerRight.Left = GlobalMouseX - (HandlerRight.Width / 2);
                    StretchSelection();

                    UpdateValues();
                }
            }
        
        }
        private void HandlerLeft_MouseUp(object sender, MouseEventArgs e)
        {
            if ((e.Button == MouseButtons.Left) && (!m_bSelLocked))
            {
                if (SelectionChanged != null) { SelectionChanged(this, EventArgs.Empty); }
            }
        }
        private void HandlerRight_MouseUp(object sender, MouseEventArgs e)
        {
            if ((e.Button == MouseButtons.Left) && (!m_bSelLocked))
            {
                if (SelectionChanged != null) { SelectionChanged(this, EventArgs.Empty); }
            }
        }
        
        private void SelectionTracker_Resize(object sender, EventArgs e)
        {
        	m_iMaxWidth = this.Width - BumperLeft.Width - BumperRight.Width - HandlerLeft.Width - HandlerRight.Width; 
            UpdateSelectedZone();
        }
        
        public void UpdateSelectedZone()
        {   
            //------------------------------------------------
            // Redessine la selection en fonction des données.
            // (Au chargement ou sur Resize)
            //------------------------------------------------

            // -> Déplacer le handler.
            if (m_iMaximum - m_iMinimum > 0)
            {
                HandlerLeft.Left = BumperLeft.Width + Rescale(m_iSelStart - m_iMinimum, m_iMaximum - m_iMinimum, m_iMaxWidth);
                HandlerRight.Left = BumperLeft.Width + HandlerLeft.Width + Rescale(m_iSelEnd - m_iMinimum, m_iMaximum - m_iMinimum, m_iMaxWidth);

                StretchSelection();
            }
        }
        
        private void SelectionTracker_DoubleClick(object sender, EventArgs e)
        {
            //--------------------------------------------------------------
            // Double clic dans le fond : Déplacer le handler le plus proche
            //--------------------------------------------------------------

            if (!m_bSelLocked)
            {
                Point MouseCoords = this.PointToClient(Cursor.Position);

                if ((MouseCoords.X > BumperLeft.Width + (HandlerLeft.Width / 2)) &&
                    (MouseCoords.X < this.Width - BumperRight.Width - (HandlerRight.Width / 2)))
                {

                    if (MouseCoords.X < HandlerLeft.Left)
                    {
                        HandlerLeft.Left = MouseCoords.X - (HandlerLeft.Width / 2);
                    }
                    else
                    {
                        HandlerRight.Left = MouseCoords.X - (HandlerRight.Width / 2);
                    }

                    StretchSelection();
                    UpdateValues();
                    if (SelectionChanged != null) { SelectionChanged(this, EventArgs.Empty); }
                }
            }
        }
        private void SelectedZone_DoubleClick(object sender, EventArgs e)
        {
            //if (!m_bSelLocked)
            //{
            //    //-------------------------------------
            //    // Déplacer le handler le plus proche
            //    // On sait qu'on est à l'intérieur.
            //    //-------------------------------------
            //    Point MouseCoords = this.PointToClient(Cursor.Position);

            //    int HandlerLeftDistance = (int)Math.Abs(HandlerLeft.Left + HandlerLeft.Width - MouseCoords.X);
            //    int HandlerRightDistance = (int)Math.Abs(HandlerRight.Left - MouseCoords.X);

            //    if (HandlerLeftDistance < HandlerRightDistance)
            //    {
            //        HandlerLeft.Left = MouseCoords.X - (HandlerLeft.Width / 2);
            //        StretchSelection();
            //    }
            //    else
            //    {
            //        HandlerRight.Left = MouseCoords.X - (HandlerRight.Width / 2);
            //        StretchSelection();
            //    }

            //    UpdateValues();
            //    if (SelectionChanged != null) { SelectionChanged(this, EventArgs.Empty); }
            //}
        }
        private void SelectedZone_MouseClick(object sender, MouseEventArgs e)
        {
            // Target selection.

            if (e.Button == MouseButtons.Left)
            {
                m_iSelTarget = Rescale(SelectedZone.Left + e.X - BumperLeft.Width - HandlerLeft.Width, m_iMaxWidth, m_iMaximum - m_iMinimum);
                if (TargetAcquired != null) { TargetAcquired(this, EventArgs.Empty); }
            }
        }

        private void BumperLeft_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if ((e.Button == MouseButtons.Left) && (!m_bSelLocked))
            {
                // Ramener le handler de gauche au début.
                HandlerLeft.Left = BumperLeft.Left + BumperLeft.Width;
                StretchSelection();

                UpdateValues();
                if (SelectionChanged != null) { SelectionChanged(this, EventArgs.Empty); }
            }
        }
        private void BumperRight_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if ((e.Button == MouseButtons.Left) && (!m_bSelLocked))
            {
                // Ramener le handler de doite à la fin.
                HandlerRight.Left = BumperRight.Left - HandlerRight.Width;
                StretchSelection();

                UpdateValues();
                if (SelectionChanged != null) { SelectionChanged(this, EventArgs.Empty); }
            }
        }
        private void EndOfTrackLeft_DoubleClick(object sender, EventArgs e)
        {
            if (!m_bSelLocked)
            {
                // Ramener le handler de gauche au début.
                HandlerLeft.Left = BumperLeft.Left + BumperLeft.Width;
                StretchSelection();

                UpdateValues();
                if (SelectionChanged != null) { SelectionChanged(this, EventArgs.Empty); }
            }
        }
        private void EndOfTrackRight_DoubleClick(object sender, EventArgs e)
        {
            if (!m_bSelLocked)
            {
                // Ramener le handler de doite à la fin.
                HandlerRight.Left = BumperRight.Left - HandlerRight.Width;
                StretchSelection();

                UpdateValues();
                if (SelectionChanged != null) { SelectionChanged(this, EventArgs.Empty); }
            }
        }

        private void StretchSelection()
        {
            SelectedZone.Left = HandlerLeft.Left + HandlerLeft.Width;
            SelectedZone.Width = HandlerRight.Left - SelectedZone.Left;
        }
        
        #endregion

        #region Binding UI to Data

        private void UpdateValues()
        {
            if (m_iSelEnd - m_iSelStart >= 0)
            {
                m_iSelStart = Rescale(SelectedZone.Left - BumperLeft.Width - HandlerLeft.Width, m_iMaxWidth, m_iMaximum - m_iMinimum) + m_iMinimum;
                m_iSelEnd = Rescale(SelectedZone.Left - BumperLeft.Width - HandlerLeft.Width + SelectedZone.Width, m_iMaxWidth, m_iMaximum - m_iMinimum) + m_iMinimum;
            
                // Forcer quand même à rester à l'intérieur des bornes fixées au chargement du contrôle.
                // Certaines vidéos ont des frames inaccessibles au premier abord qui se révèlent ensuite...
                // On les ignore car cela perturbe grandement le système de numération des frames.

                if (m_iSelStart < m_iMinimum)
                    m_iSelStart = m_iMinimum;

                if (m_iSelEnd > m_iMaximum)
                    m_iSelEnd = m_iMaximum;

                if (SelectionChanging != null) { SelectionChanging(this, EventArgs.Empty); }
            }
        }
        private int Rescale(long _iOldValue, long _iOldMax, long _iNewMax)
        {
            // Rescale : Pixels -> Values
            return (int)(Math.Round((double)((double)_iOldValue * (double)_iNewMax) / (double)_iOldMax));
        }

        #endregion

        private void SelectedZone_Paint(object sender, PaintEventArgs e)
        {
            // Redessiner la tête de lecture.
            if ((m_iSelEnd - m_iSelStart > 0) && (m_iSelPos >= m_iSelStart) && (m_iSelPos <= m_iSelEnd))
            {
                int head = Rescale(m_iSelPos - m_iSelStart, m_iSelEnd - m_iSelStart, SelectedZone.Width);

                if (head == SelectedZone.Width) { head = SelectedZone.Width - 1; }

                e.Graphics.DrawLine(Pens.Black, head, 4, head, SelectedZone.Height - 6);
            }
        }

        

        

     

    }
}
