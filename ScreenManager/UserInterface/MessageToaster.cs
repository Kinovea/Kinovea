#region License
/*
Copyright © Joan Charmant 2010.
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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// MessageToaster.
	/// A class to encapsulate the display of a message to be painted directly on a canvas,
	/// for a given duration.
	/// Used to indicate a change in the state of a screen for example. (Pause, Zoom factor).
	/// The same object is reused for various messages. Each screen should have its own instance.
	/// </summary>
	public class MessageToaster
	{
		#region Properties
		public bool Enabled
		{
			get { return m_bEnabled; }
		}
		#endregion
		
		#region Members
		private string m_Message;
		private Timer m_Timer = new Timer();
		private Font m_Font;
		private bool m_bEnabled;
		private Control m_CanvasHolder;
		private static readonly int m_DefaultDuration = 1000;
		private static readonly int m_DefaultFontSize = 24;
		private Brush m_ForeBrush = new SolidBrush(Color.FromArgb(255, Color.White));
		private Brush m_BackBrush = new SolidBrush(Color.FromArgb(128, Color.Black));
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		#endregion
		
		#region Constructor
		public MessageToaster(Control _canvasHolder)
		{
			m_CanvasHolder = _canvasHolder;
			m_Font = new Font("Arial", m_DefaultFontSize, FontStyle.Bold);
			m_Timer.Interval = m_DefaultDuration;
			m_Timer.Tick += new EventHandler(Timer_OnTick); 
		}
		#endregion
		
		#region Public Methods
		public void SetDuration(int _duration)
		{
			m_Timer.Interval = _duration;
		}
		public void Show(string _message)
		{
			log.Debug(String.Format("Toasting message: {0}", _message));
			m_Message = _message;
			m_bEnabled = true;
			StartStopTimer();
		}
		public void Draw(Graphics _canvas)
		{
			if(m_Message != "" && m_CanvasHolder != null)
			{
				SizeF bgSize = _canvas.MeasureString(m_Message, m_Font);
				bgSize = new SizeF(bgSize.Width, bgSize.Height + 3);
				Point pos = new Point((int)(m_CanvasHolder.Width - bgSize.Width)/2, (int)(m_CanvasHolder.Height - bgSize.Height)/2);
				
				// 1. Draw background.
				Rectangle bg = new Rectangle(pos.X - 5, pos.Y - 5, (int)bgSize.Width + 10, (int)bgSize.Height + 10);
	            int radius = (int)(m_Font.Size / 2);
	            
	            int diameter = radius * 2;
				GraphicsPath gp = new GraphicsPath();
	            gp.StartFigure();
	            gp.AddArc(bg.X, bg.Y, diameter, diameter, 180, 90);
            	gp.AddLine(bg.X + radius, bg.Y, bg.X + bg.Width - diameter, bg.Y);
	            gp.AddArc(bg.X + bg.Width - diameter, bg.Y, diameter, diameter, 270, 90);
	            gp.AddLine(bg.X + bg.Width, bg.Y + radius, bg.X + bg.Width, bg.Y + bg.Height - diameter);
	            gp.AddArc(bg.X + bg.Width - diameter, bg.Y + bg.Height - diameter, diameter, diameter, 0, 90);
	            gp.AddLine(bg.X + bg.Width - radius, bg.Y + bg.Height, bg.X + radius, bg.Y + bg.Height);
	            gp.AddArc(bg.X, bg.Y + bg.Height - diameter, diameter, diameter, 90, 90);
	            gp.AddLine(bg.X, bg.Y + bg.Height - radius, bg.X, bg.Y + radius);
	            gp.CloseFigure();
	            
	            _canvas.FillPath(m_BackBrush, gp);
	            _canvas.DrawString(m_Message, m_Font, m_ForeBrush, pos.X, pos.Y);
			}
		}
		#endregion
		
		#region Private methods
		private void Timer_OnTick(object sender, EventArgs e)
		{
			// Timer fired : Time to hide the message.
			m_bEnabled = false;
			StartStopTimer();
		}
		private void StartStopTimer()
		{
			if(m_bEnabled)
			{
				if(m_Timer.Enabled)
				{	
					m_Timer.Stop();
				}
				
				m_Timer.Start();
			}
			else
			{
				m_Timer.Stop();
				if(m_CanvasHolder != null)	m_CanvasHolder.Invalidate();
			}
		}
		#endregion
	}
}
