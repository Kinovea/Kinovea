#region License
/*
Copyright © Joan Charmant 2011.
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
#endregion
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Kinovea.Root
{
	/// <summary>
	/// PreferencePanelButtton.
	/// A simple "image + label" control to be used for preferences pages.
	/// </summary>
	public partial class PreferencePanelButtton : UserControl
	{
		#region Properties
		public IPreferencePanel PreferencePanel
		{
			get { return m_PreferencePanel; }
		}
		#endregion
		
		#region Members
		private bool m_bSelected;
		private IPreferencePanel m_PreferencePanel;
		private static readonly Font m_FontLabel = new Font("Arial", 8, FontStyle.Regular);
		#endregion
		
		#region Construction
		public PreferencePanelButtton(IPreferencePanel _PreferencePanel)
		{
			InitializeComponent();
			m_PreferencePanel = _PreferencePanel;
		}
		#endregion
		
		#region Public Methods
		public void SetSelected(bool _bSelected)
		{
			m_bSelected = _bSelected;
			this.BackColor = _bSelected ? Color.LightSteelBlue : Color.White;
		}
		#endregion
		
		#region Private Methods		
		private void preferencePanelButtton_Paint(object sender, PaintEventArgs e)
		{
			if(m_PreferencePanel.Icon != null)
			{
				Point iconStart = new Point((this.Width - m_PreferencePanel.Icon.Width) / 2, 10);
				e.Graphics.DrawImage(m_PreferencePanel.Icon, iconStart);
			}
			
			SizeF textSize = e.Graphics.MeasureString(m_PreferencePanel.Description, m_FontLabel);
			PointF textStart = new PointF(((float)this.Width - textSize.Width) / 2, 50.0F);
			e.Graphics.DrawString(m_PreferencePanel.Description, m_FontLabel, Brushes.Black, textStart);
		}		
		private void PreferencePanelButttonMouseEnter(object sender, EventArgs e)
		{
			if(!m_bSelected)
			{
				this.BackColor = Color.FromArgb(224,232,246);
			}
		}
		void PreferencePanelButttonMouseLeave(object sender, EventArgs e)
		{
			if(!m_bSelected)
			{
				this.BackColor = Color.White;	
			}
		}
		#endregion
	}
}
