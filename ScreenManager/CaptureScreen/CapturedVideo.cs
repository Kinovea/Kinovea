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
using System.Drawing;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// CapturedVideo represent a recently captured file.
	/// It keeps the thumbnail, and path...
	/// It is used to display the recently captured videos as launchable thumbs.
	/// </summary>
	public class CapturedVideo
	{
		#region Properties
		public Bitmap Thumbnail
        {
            get { return m_Thumbnail; }
        }
		public string Filepath
		{
			get { return m_Filepath; }
		}
		#endregion
        
		#region Members
		private Bitmap m_Thumbnail;
		private string m_Filepath;
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		#endregion
        
		public CapturedVideo(string _Filepath, Bitmap _image)
		{
			m_Filepath = _Filepath;
			if(_image != null) 
			{
				m_Thumbnail = new Bitmap(_image, 100, 75);
			}
			else
			{
				m_Thumbnail = new Bitmap(100, 75);
				log.Error("Cannot create captured video thumbnail.");
			}
			
		}
	}
}
