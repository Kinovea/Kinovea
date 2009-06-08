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
	/// <summary>
	/// Represent a key image as a thumbnail.
	/// Holds the key image id in the .Tag prop.
	/// </summary>
    public partial class ThumbBox : UserControl
    {
        #region EventDelegates
        // Déclarations de Types
        public delegate void CloseThumbHandler(object sender, EventArgs e);
        public delegate void ClickThumbHandler(object sender, EventArgs e);
        public delegate void ClickInfosHandler(object sender, EventArgs e);

        // Déclarations des évènements
        [Category("Action"), Browsable(true)]
        public event CloseThumbHandler CloseThumb;
        [Category("Action"), Browsable(true)]
        public event ClickThumbHandler ClickThumb;
        [Category("Action"), Browsable(true)]
        public event ClickInfosHandler ClickInfos;
        #endregion

        #region Properties
        public bool Selected
        {
			//get => unused.
			
        	set
        	{ 
        		if(value)
        		{
        			BackColor = Color.SteelBlue;
					//BackColor = Color.OliveDrab;
        		}
        		else
        		{
        			BackColor = Color.Black;
        		}
        	}
        }		
		public string TimeCode 
		{
			get { return m_TimeCode; }
			set { m_TimeCode = value; }
		}		
		public string Title 
		{
			get { return m_Title; }
			set 
			{ 
				m_Title = value; 
				lblTimecode.Text = m_Title;
			}
		}
		#endregion
        
		#region Members
		private string m_TimeCode;
		private string m_Title;
		#endregion
		
		#region Constructor
        public ThumbBox()
        {
            InitializeComponent();
            lblTimecode.Visible = true;
            BackColor = Color.Black;
            btnClose.Parent = pbThumbnail;
            btnComment.Parent = pbThumbnail;
        }
		#endregion
        
        #region Mouse Enter
        private void pbThumbnail_MouseEnter(object sender, EventArgs e)
        {
            ShowButtons();
            lblTimecode.Visible = true;
        }
        private void lblTimecode_MouseEnter(object sender, EventArgs e)
        {
            ShowButtons();
        }
        #endregion
        
        #region Mouse leave
        private void pbThumbnail_MouseLeave(object sender, EventArgs e)
        {
            CheckMouseLeave();
        }
        private void btnClose_MouseLeave(object sender, EventArgs e)
        {
            CheckMouseLeave();    
        }
        private void lblTimecode_MouseLeave(object sender, EventArgs e)
        {
            CheckMouseLeave();
        }
        private void ThumbBox_MouseLeave(object sender, EventArgs e)
        {
            CheckMouseLeave();
        }
		private void CheckMouseLeave()
        {
            // We hide the close button only if we left the whole control.
            Point clientMouse = PointToClient(Control.MousePosition);
            if(!pbThumbnail.ClientRectangle.Contains(clientMouse))
            {
                HideButtons();
            }
        }
		
		#endregion

		#region Buttons
        private void btnClose_Click(object sender, EventArgs e)
        {
            if (CloseThumb != null) CloseThumb(this, e);
        }
        private void pbThumbnail_Click(object sender, EventArgs e)
        {
            if (ClickThumb != null) ClickThumb(this, e);
        }
        private void btnComment_Click(object sender, EventArgs e)
        {
            if (ClickInfos != null) ClickInfos(this, e);
        }
        private void ShowButtons()
        {
            btnClose.Visible = true;
            btnComment.Visible = true;
        }
        private void HideButtons()
        {
            btnClose.Visible = false;
            btnComment.Visible = false;
        }
        #endregion
    }
}
