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
using AForge.Imaging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using AForge.Imaging.Filters;
using Videa.Services;
using VideaPlayerServer;

namespace Videa.ScreenManager
{
	/// <summary>
	/// VideoFilterAutoLevels.
	/// - Input			: All images.
	/// - Output		: All images, same size.
	/// - Operation 	: Remap channels histograms. 
	/// - Type 			: Work on each frame separately.
	/// - Previewable 	: Yes.
	/// </summary>
	public class VideoFilterAutoLevels : AbstractVideoFilter
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
			get { return false; }
		}
		#endregion
		
		#region Members
		private ToolStripMenuItem m_Menu;
		private List<DecompressedFrame> m_FrameList;
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		#endregion
		
		#region Constructor
		public VideoFilterAutoLevels()
		{
			ResourceManager resManager = new ResourceManager("Videa.ScreenManager.Languages.ScreenManagerLang", Assembly.GetExecutingAssembly());
            
			// Menu
            m_Menu = new ToolStripMenuItem();
            m_Menu.Tag = new ItemResourceInfo(resManager, "VideoFilterAutoLevels_FriendlyName");
            m_Menu.Text = ((ItemResourceInfo)m_Menu.Tag).resManager.GetString(((ItemResourceInfo)m_Menu.Tag).strText, Thread.CurrentThread.CurrentUICulture);
            m_Menu.Click += new EventHandler(Menu_OnClick);
            m_Menu.MergeAction = MergeAction.Append;
		}
		#endregion
		
		#region AbstractVideoFilter Implementation
		public override void Menu_OnClick(object sender, EventArgs e)
        {
			// 1. Display preview dialog box.
			formPreviewVideoFilter fpvf = new formPreviewVideoFilter(GetPreviewImage(), m_Menu.Text);
            if (fpvf.ShowDialog() == DialogResult.OK)
            {
            	// 2. Process filter.
            	StartProcessing();
            }
            fpvf.Dispose();
        }
		protected override void Process()
		{
			// Method called back from AbstractVideoFilter after a call to StartProcessing().
			// Use StartProcessing() to get progress bar and threading support.
			
			for(int i=0;i<m_FrameList.Count;i++)
            {
				m_FrameList[i].BmpImage = ProcessSingleImage(m_FrameList[i].BmpImage);
            	m_BackgroundWorker.ReportProgress(i, m_FrameList.Count);
            }
		}
		#endregion
		
		#region Private methods
		private Bitmap GetPreviewImage()
		{
			// Deep clone an image then pass it to the filter.
			Bitmap bmp = AForge.Imaging.Image.Clone(m_FrameList[(m_FrameList.Count-1)/2].BmpImage);
			return ProcessSingleImage(bmp);
		}
		private Bitmap ProcessSingleImage(Bitmap _src)
		{
			ImageStatistics stats = new ImageStatistics(_src);
        	
			LevelsLinear levelsLinear = new LevelsLinear();
        	levelsLinear.InRed   = stats.Red.GetRange( 0.87 );
            levelsLinear.InGreen = stats.Green.GetRange( 0.87 );
            levelsLinear.InBlue  = stats.Blue.GetRange( 0.87 );
            
            levelsLinear.ApplyInPlace(_src);
			
			return _src;
		}
		#endregion
	}
}

