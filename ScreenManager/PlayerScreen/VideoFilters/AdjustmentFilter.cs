#region License
/*
Copyright © Joan Charmant 2011.
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
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

using AForge.Imaging;
using Kinovea.Video;

namespace Kinovea.ScreenManager
{
    /// <summary>
	/// All adjustment filters :
	/// - Input        : All images.
	/// - Output       : All images, same size.
	/// - Type         : Work on each frame separately - the filter must be applied in place.
	/// - Previewable  : Yes.
	/// </summary>
    public abstract class AdjustmentFilter : AbstractVideoFilter
	{
        public abstract ImageProcessor ImageProcessor { get ; }
        
		public override void Menu_OnClick(object sender, EventArgs e)
        {
		    if(ImageProcessor == null || FrameCache.Count < 1)
		        return;

		    using(Bitmap bmp = FrameCache.Representative.CloneDeep())
			using(formPreviewVideoFilter fpvf = new formPreviewVideoFilter(ImageProcessor(bmp), Name))
			{
                if (fpvf.ShowDialog() == DialogResult.OK)
                    StartProcessing();
			}
        }
		protected override void Process()
		{
			// Method called back from AbstractVideoFilter after a call to StartProcessing().
			if(ImageProcessor == null)
		        return;
			
			int i = 0;
			foreach(VideoFrame vf in FrameCache)
			{
			    ImageProcessor(vf.Image);
			    m_BackgroundWorker.ReportProgress(i++, FrameCache.Count);
			}
		}
	}
}
