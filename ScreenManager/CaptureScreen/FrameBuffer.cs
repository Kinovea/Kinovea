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
using System.Collections.Generic;
using System.Drawing;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// FrameBuffer - a buffer that holds the recent history of grabbed frames.
	/// Handles buffer rotation.
	/// </summary>
	public class FrameBuffer
	{
		#region Members
		private static readonly int m_iCapacity = 10;
		private int m_iHead;
		private int m_iTail;
		private Bitmap[] m_Buffer = new Bitmap[m_iCapacity];
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		#endregion

		#region Public methods
		public void Write(Bitmap _bmp)
		{
			// A frame is received and must be stored.
			//log.Debug(String.Format("Writing frame. tail:{0}, head:{1}", m_iTail, m_iHead));			
			if(m_Buffer[m_iTail] != null)
			{
				m_Buffer[m_iTail].Dispose();
				m_Buffer[m_iTail] = null;
			}
            
			m_Buffer[m_iTail] = AForge.Imaging.Image.Clone(_bmp);
			
			m_iTail++;
			if(m_iTail == m_iCapacity) m_iTail = 0;
		}
		public Bitmap Read()
		{
			// Read next frame. It had been properly cloned during write.
			// Don't move if underflow.
			Bitmap frame = m_Buffer[m_iHead];
			//log.Debug(String.Format("Reading frame. tail:{0}, head:{1}", m_iTail, m_iHead));
			if(frame != null)
			{
				m_iHead++;
				if(m_iHead == m_iCapacity) m_iHead = 0;
			}
			
			return frame;
		}
		public void Clear()
		{
			// Release all non managed resources.
			//log.Debug(String.Format("Clearing frame. tail:{0}, head:{1}", m_iTail, m_iHead));
			for(int i=0; i<m_Buffer.Length; i++)
			{
				if(m_Buffer[i] != null)
				{
					m_Buffer[i].Dispose();	
				}
			}
			
			m_iHead = 0;
			m_iTail = 0;
		}
		#endregion
		
	}
}
