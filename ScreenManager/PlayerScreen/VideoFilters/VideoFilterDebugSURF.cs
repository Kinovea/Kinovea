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
using Kinovea.Services;
using Kinovea.VideoFiles;
using OpenSURF;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// VideoFilterDebugSURF.
	/// - Input			: All images.
	/// - Output		: All images, same size.
	/// - Operation 	: extract SURF features on each image, and paint them on.
	/// - Type 			: Work on each frame separately.
	/// - Previewable 	: No.
	/// </summary>
	public class VideoFilterDebugSURF : AbstractVideoFilter
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
		private int m_iOctaves = 1;
        private int m_iIntervals = 3;
        private int m_iInitSample = 1;
        private float m_fThreshold = 0.0001f;
        private int m_iInterpolationSteps = 10;
		private bool m_bUpright = false;
			
		private ToolStripMenuItem m_Menu;
		private List<DecompressedFrame> m_FrameList;
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		#endregion
		
		#region Constructor
		public VideoFilterDebugSURF()
		{
			ResourceManager resManager = new ResourceManager("Kinovea.ScreenManager.Languages.ScreenManagerLang", Assembly.GetExecutingAssembly());
            
			// Menu
            m_Menu = new ToolStripMenuItem();
            //m_Menu.Tag = new ItemResourceInfo(resManager, "VideoFilterReverse_FriendlyName");
            //m_Menu.Text = ((ItemResourceInfo)m_Menu.Tag).resManager.GetString(((ItemResourceInfo)m_Menu.Tag).strText, Thread.CurrentThread.CurrentUICulture);
            m_Menu.Text = "Feature extraction (SURF)";
            m_Menu.Click += new EventHandler(Menu_OnClick);
            m_Menu.MergeAction = MergeAction.Append;
		}
		#endregion
		
		#region AbstractVideoFilter Implementation
		public override void Menu_OnClick(object sender, EventArgs e)
        {
			StartProcessing();
        }
		protected override void Process()
		{
			// Method called back from AbstractVideoFilter after a call to StartProcessing().
			// Use StartProcessing() to get progress bar and threading support.
			List<DecompressedFrame> m_TempFrameList = new List<DecompressedFrame>();
			
			RefreshParameters();
			
			for(int i=0;i<m_FrameList.Count;i++)
            {
				m_FrameList[i].BmpImage = ProcessSingleImage(m_FrameList[i].BmpImage);
            	m_BackgroundWorker.ReportProgress(i, m_FrameList.Count);
            }
		}
		#endregion
		
		#region Private methods
		private void RefreshParameters()
		{
			PreferencesManager pm = PreferencesManager.Instance();
			pm.Import();
			
			m_iOctaves = pm.SurfOctaves;
	        m_iIntervals = pm.SurfIntervals;
	        m_iInitSample = pm.SurfInitSample;
	        m_fThreshold = pm.SurfThreshold;
	        m_iInterpolationSteps = pm.SurfInterpolationSteps;
			m_bUpright = pm.SurfUpright;
			
			log.Debug(String.Format("SURF params : Octaves:{0}, Intervals:{1}, Treshold:{2}", m_iOctaves, m_iIntervals, m_fThreshold));
		}
		private Bitmap ProcessSingleImage(Bitmap _src)
		{
			// Scale-space image of the search window.
			IplImage pIplImage = IplImage.LoadImage(_src);
            pIplImage = pIplImage.BuildIntegral(null);

            List<Ipoint> ipts = new List<Ipoint>();
            CFastHessian pCFastHessian = new CFastHessian(pIplImage, ref ipts, m_iOctaves, m_iIntervals, m_iInitSample, m_fThreshold, m_iInterpolationSteps);
            
            // Fill the scale-space image with actual data and finds the local extrema.
            pCFastHessian.getIpoints();
            
            // Fill the descriptor field, orientation and laplacian of the feature.
            Surf pSurf = new Surf(pIplImage, ipts);
            pSurf.getDescriptors(m_bUpright);
            
            log.Debug(String.Format("SURF, number of points found:{0}", ipts.Count));
           	
           	// Paint described points on image.
           	PaintSURFPoints(_src, ipts);
			
			return _src;
		}
		private void PaintSURFPoints(Bitmap _src, List<Ipoint> _ipts)
		{	
            Graphics pgd = null;
            Pen penPoint = new Pen(Color.Yellow, 2);
            try
            {
                pgd = Graphics.FromImage(_src);
                
                if (_ipts == null) 
                	return;

                foreach (Ipoint pIpoint in _ipts)
                {
                    if (pIpoint == null) continue;

                    int xd = (int)pIpoint.x;
                    int yd = (int)pIpoint.y;
                    float scale = pIpoint.scale;
                    float orientation = pIpoint.orientation;
                    float radius = ((9.0f / 1.2f) * scale) / 3.0f;

                    pgd.DrawEllipse(penPoint, xd - radius, yd - radius, 2 * radius, 2 * radius);

                    double dx = radius * Math.Cos(orientation);
                    double dy = radius * Math.Sin(orientation);
                    pgd.DrawLine(penPoint, new Point(xd, yd), new Point((int)(xd+dx),(int)(yd+dy)));
                }
            }
            finally
            {
                if (penPoint != null) penPoint.Dispose();
                if (pgd != null) pgd.Dispose();
            }
		}
		#endregion
	}
}


