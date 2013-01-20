#region License
/*
Copyright © Joan Charmant 2012.
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
using System.Threading;

using AForge.Video.DirectShow;

namespace Kinovea.Camera.DirectShow
{
    /// <summary>
    /// Class to discover and manage cameras connected through DirectShow.
    /// </summary>
    public class CameraManagerDirectShow : CameraManager
    {
        #region Members
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private Dictionary<string, CameraBlurb> blurbCache = new Dictionary<string, CameraBlurb>();
        #endregion
        
        public override List<CameraSummary> DiscoverCameras(List<CameraBlurb> previouslySeen)
        {
            // DirectShow has active discovery. We just ask for the list of cameras connected to the PC.
            List<CameraSummary> summaries = new List<CameraSummary>();
            
            FilterInfoCollection cameras = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            
            foreach(FilterInfo camera in cameras)
            {
                // For now consider that the moniker string is like a serial number.
                // Apparently this is only true for certain models.
                // Check if we should extract the serial number part so that we don't change id when changing USB port.
                string identifier = camera.MonikerString;
                
                if(blurbCache.ContainsKey(identifier))
                    continue;
                
                log.DebugFormat("DirectShow camera. Name:{0}, Moniker:{1}", camera.Name, camera.MonikerString);
                
                string alias = camera.Name;
                bool known = false;
                if(previouslySeen != null)
                {
                    foreach(CameraBlurb b in previouslySeen)
                    {
                        if(b.Identifier == identifier)
                            alias = b.Alias;
                    }
                }
                
                CameraBlurb blurb = new CameraBlurb("DirectShow", identifier, alias, null);
                blurbCache.Add(identifier, blurb);
                
                Bitmap defaultIcon = Camera.IconLibrary.GetIcon("webcam");
                CameraSummary summary = new CameraSummary(alias, identifier, defaultIcon, this);
                summaries.Add(summary);
                
                // Spawn a thread to get a snapshot.
                SnapshotRetriever retriever = new SnapshotRetriever(summary, camera.MonikerString);
                retriever.CameraImageReceived += SnapshotRetriever_CameraImageReceived;
                ThreadPool.QueueUserWorkItem(retriever.Run);
            }

            return summaries;
        }

        private void SnapshotRetriever_CameraImageReceived(object sender, CameraImageReceivedEventArgs e)
        {
            OnCameraImageReceived(e);
        }
        
        public override FrameGrabber Connect(string identifier)
        {
            throw new NotImplementedException();
        }
        
        public override void ExportCameraBlurb(string identifier)
        {
            // Save blurb to XML.
            //if(!blurbCache.ContainsKey(identifier))
            //{
                
            //}
            throw new NotImplementedException();
        }
    }
}