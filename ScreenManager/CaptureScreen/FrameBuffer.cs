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
		private Bitmap[] m_Buffer = new Bitmap[m_iCapacity];
		private int m_iHead; // next spot to read from.
		private int m_iTail; // next spot to write to.
		private int m_iFill; // number of spots that were written but not read yet.
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		#endregion

		#region Public methods
		public void Write(Bitmap _bmp)
		{
			// A frame is received and must be stored.
			if(m_Buffer[m_iTail] != null)
			{
				m_Buffer[m_iTail].Dispose();
				m_Buffer[m_iTail] = null;
			}
            
			m_Buffer[m_iTail] = AForge.Imaging.Image.Clone(_bmp);
			m_iFill++;
			m_iTail++;
			if(m_iTail == m_iCapacity) m_iTail = 0;
			
			log.Debug(String.Format("Wrote frame. tail:{0}, head:{1}, count:{2}", m_iTail, m_iHead, m_iFill));
		}
		public Bitmap Read()
		{
			// Read next frame. It had been properly cloned during write.
			// Don't move if underflow.
			Bitmap frame = null;
			
			if(m_iFill > 0)
			{
				frame = m_Buffer[m_iHead];
				
				//log.Debug(String.Format("Reading frame. tail:{0}, head:{1}", m_iTail, m_iHead));
				if(frame != null)
				{
					m_iFill--;
					m_iHead++;
					if(m_iHead == m_iCapacity) m_iHead = 0;
				}
			}
			
			log.Debug(String.Format("Read frame. tail:{0}, head:{1}, count:{2}", m_iTail, m_iHead, m_iFill));
			return frame;
		}
		public Bitmap ReadAt(int _index)
		{
			// Read frame at specified index.
			Bitmap frame = null;
			
			if(m_iFill > _index)
			{
				// Head is always the oldest frame that haven't been read yet.
				// What we want is a frame that is only old of _index frames.
				int spot = m_iTail - 1 - _index;
				if(spot < 0)
					spot = m_iCapacity + spot;
				
				frame = m_Buffer[spot];
				if(frame != null)
				{
					m_iFill--;
					m_iHead++;
					if(m_iHead == m_iCapacity) m_iHead = 0;
				}
			}
			else if(m_iFill > 0)
			{
				// There's not enough images in the buffer to reach that index.
				// Return last image from the buffer but don't touch the sentinels.
				frame = m_Buffer[m_iHead];
			}
			
			//log.Debug(String.Format("Read frame. tail:{0}, head:{1}, count:{2}", m_iTail, m_iHead, m_iFill));
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
