#region License
/*
Copyright © Joan Charmant 2009.
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
using Kinovea.VideoFiles;
using System.Drawing;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// FrameServerPlayer encapsulate the video file, meta data and everything 
	/// needed to render the frame and access file functions.
	/// PlayerScreenUserInterface is the View, FrameServerPlayer is the Model.
	/// </summary>
	public class FrameServerPlayer : AbstractFrameServer
	{
		#region Properties
		public VideoFile VideoFile
		{
			get { return m_VideoFile; }
			set { m_VideoFile = value; }
		}		
		public bool Loaded
		{
			get { return m_VideoFile.Loaded;}
		}
		#endregion
		
		#region Members
		private VideoFile m_VideoFile = new VideoFile();
		//private Metadata m_Metadata = new Metadata();
		#endregion

		#region Constructor
		public FrameServerPlayer()
		{
		}
		#endregion
		
		#region Public
		public LoadResult Load(string _FilePath)
		{
			return m_VideoFile.Load(_FilePath);
		}
		public void Unload()
		{
			// Prepare the FrameServer for a new video by resetting everything.
			m_VideoFile.Unload();
			//m_Metadata.Reset();
		}
		public override void Draw(Graphics _canvas)
		{
			// Draw the current image on canvas according to conf.
			// This is called back from screen paint method.
		}
		#endregion
	}
}
