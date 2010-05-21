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
	/// Provides pointers to head and nth frame.
	/// Handles buffer rotation.
	/// 
	/// todo: turn into a proper circular buffer.
	/// </summary>
	public class FrameBuffer
	{
		#region Properties
		public int BufferCapacity
		{
			get { return m_iBufferCapacity; }
		}
		public int BufferCount
		{
			get { return m_Bitmaps.Count; }
		}
		#endregion
				
		#region Members
		private List<Bitmap> m_Bitmaps = new List<Bitmap>();
		private int m_iBufferCapacity = 125;
		#endregion

		#region Public methods
		public void PushFrame(Bitmap _bmp)
		{
			// A frame is received and must be stored.
			// We only store the reference at that point.
			
			// rotate buffer: remove last frame.
			if(m_Bitmaps.Count >= m_iBufferCapacity)
			{
				m_Bitmaps[m_Bitmaps.Count-1].Dispose();
				m_Bitmaps.RemoveAt(m_Bitmaps.Count - 1);
			}
			
			m_Bitmaps.Insert(0, _bmp);			
		}
		public Bitmap ReadFrameAt(int _iIndex)
		{
			// A frame from the buffer is asked. The caller needs a stable image, 
			// not just a reference to something that might be written at any time.
			
			Bitmap frame = null;
			if(_iIndex >= 0 && _iIndex < m_Bitmaps.Count)
			{
				frame = AForge.Imaging.Image.Clone(m_Bitmaps[_iIndex]);
			}
			return frame;
		}
		public void Dispose()
		{
			// Release all non managed resources.
			for(int i=m_Bitmaps.Count-1;i>=0;i--)
			{
				m_Bitmaps[i].Dispose();
				m_Bitmaps.RemoveAt(i);
			}
		}
		#endregion
		
	}
}
