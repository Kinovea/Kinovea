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
using System.Diagnostics;
using System.Drawing;

using Kinovea.Services;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// FrameBuffer - a buffer that holds the recent history of grabbed frames.
	/// This is an In-Memory Buffer
	/// Handles buffer rotation.
	/// Frames are inserted at the tail.
	/// Head represents the first frame not read yet.
	/// </summary>
	public class FrameBuffer
	{
		#region Properties
		public int Capacity
		{
			get { return m_iCapacity; }
		}
		public int FillPercentage
		{
			get { return (int)(((double)m_iFill / (double)m_iCapacity)*100);}
		}
		#endregion
		
		#region Members
		private int m_iCapacity = 1;
		private Size m_Size = new Size(640,480);
		private int m_iCaptureMemoryBuffer = 16;
		private Bitmap[] m_Buffer;
		private int m_iHead; // next spot to read from.
		private int m_iTail; // next spot to write to.
		private int m_iToRead; // number of spots that were written but not read yet.
		private int m_iFill; // number of spots that were written.
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		#endregion

		#region Constructor
		public FrameBuffer()
		{
			 m_Buffer = new Bitmap[m_iCapacity];
		}
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
            
			// Copy the image to its final place in the buffer.
			if(!_bmp.Size.Equals(m_Size))
			{
				// Copy and resize.
				m_Buffer[m_iTail] = new Bitmap(m_Size.Width, m_Size.Height);
				Graphics g = Graphics.FromImage(m_Buffer[m_iTail]);
	
				Rectangle rDst = new Rectangle(0, 0, m_Size.Width, m_Size.Height);
				RectangleF rSrc = new Rectangle(0, 0, _bmp.Width, _bmp.Height);
				g.DrawImage(_bmp, rDst, rSrc, GraphicsUnit.Pixel);
			}
			else
			{
				// simple copy.
				m_Buffer[m_iTail] = AForge.Imaging.Image.Clone(_bmp);
			}
			
			if(m_iFill < m_iCapacity) 
			    m_iFill++;
			m_iToRead++;
			m_iTail++;
			if(m_iTail == m_iCapacity) 
			    m_iTail = 0;
			
			//log.Debug(String.Format("Wrote frame. tail:{0}, head:{1}, count:{2}", m_iTail, m_iHead, m_iFill));
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
					m_iToRead--;
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
					m_Buffer[i].Dispose();	
			}
			
			m_iHead = 0;
			m_iTail = 0;
			m_iToRead = 0;
			m_iFill = 0;
		}
		public void UpdateFrameSize(Size _size)
		{
			// The buffer directly keep the images at the final display size.
			// This avoid an extra copy when the display size is not the decoding size. (force 16:9 for example).
			if(!m_Size.Equals(_size))
			{
				m_Size = _size;
				ResetBuffer();
			}
		}
		public void UpdateMemoryCapacity(bool _bShared)
		{
			// This is called when the memory cache size is changed in the preferences.
			PreferencesManager pm = PreferencesManager.Instance();
			int iAllocatedMemory = _bShared ? pm.CaptureMemoryBuffer / 2 : pm.CaptureMemoryBuffer;
			if(iAllocatedMemory != m_iCaptureMemoryBuffer)
			{
				log.DebugFormat("Changing memory capacity from {0} to {1}", m_iCaptureMemoryBuffer, iAllocatedMemory);
				m_iCaptureMemoryBuffer = iAllocatedMemory;
				ResetBuffer();
			}
		}
		#endregion
		
		private void ResetBuffer()
		{
			// Buffer capacity.
			log.Debug("Capture buffer reset");
			int bytesPerFrame = m_Size.Width * m_Size.Height * 3;
			int capacity = (int)(((double)m_iCaptureMemoryBuffer * 1048576) / (double)bytesPerFrame);
			m_iCapacity = capacity > 0 ? capacity : 1;
			
			// Reset the buffer.
			Clear();
			m_Buffer = new Bitmap[m_iCapacity];
		}
		
	}
}
