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
using System.Collections.Generic;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;

using Kinovea.Services;
using Kinovea.VideoFiles;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// VideoFilterReverse.
	/// - Input			: All images.
	/// - Output		: All images, same size.
	/// - Operation 	: revert the order of the images.
	/// - Type 			: Work on all frames at once.
	/// - Previewable 	: No.
	/// </summary>
	public class VideoFilterReverse : AbstractVideoFilter
	{
		#region Properties
		public override ToolStripMenuItem Menu
		{
			get { return m_Menu; }
		}	
		public override List<DecompressedFrame> FrameList
        {
			set { m_FrameList = value; }
        }
		public override bool Experimental 
		{
			get { return true; }
		}
		#endregion
		
		#region Members
		private ToolStripMenuItem m_Menu;
		private List<DecompressedFrame> m_FrameList;
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		#endregion
		
		#region Constructor
		public VideoFilterReverse()
		{
			ResourceManager resManager = new ResourceManager("Kinovea.ScreenManager.Languages.ScreenManagerLang", Assembly.GetExecutingAssembly());
            
			// Menu
            m_Menu = new ToolStripMenuItem();
            m_Menu.Tag = new ItemResourceInfo(resManager, "VideoFilterReverse_FriendlyName");
            m_Menu.Text = ((ItemResourceInfo)m_Menu.Tag).resManager.GetString(((ItemResourceInfo)m_Menu.Tag).strText, Thread.CurrentThread.CurrentUICulture);
            m_Menu.Click += new EventHandler(Menu_OnClick);
            m_Menu.MergeAction = MergeAction.Append;
		}
		#endregion
		
		#region AbstractVideoFilter Implementation
		public override void Menu_OnClick(object sender, EventArgs e)
        {
			// Direct call to Process because we don't need progress bar support.
			Process();
        }
		protected override void Process()
		{
			List<DecompressedFrame> m_TempFrameList = new List<DecompressedFrame>();
			
			for(int i= m_FrameList.Count-1;i>=0;i--)
            {
				int iCurrent = m_FrameList.Count-1-i;
				
				DecompressedFrame df = new DecompressedFrame();
				df.BmpImage = m_FrameList[i].BmpImage;
				df.iTimeStamp = m_FrameList[iCurrent].iTimeStamp;
				
				m_TempFrameList.Add(df);
            }
			
			for(int i=0;i<m_FrameList.Count;i++)
			{
				m_FrameList[i] = m_TempFrameList[i];
			}
			
			ProcessingOver();
		}
		#endregion
	}
}

