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
using System.Windows.Forms;

using AForge.Video.DirectShow;
using Kinovea.Services;

namespace Kinovea.Camera.DirectShow
{
    /// <summary>
    /// Class to discover and manage cameras connected through DirectShow.
    /// </summary>
    public class CameraManagerDirectShow : CameraManager
    {
        #region Properties
        public override string CameraType 
        { 
            get { return "4602B70E-8FDD-47FF-B012-7C38BB2A16B9";}
        }
        #endregion
    
        #region Members
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private Dictionary<string, CameraSummary> cache = new Dictionary<string, CameraSummary>();
        private Bitmap defaultIcon;
        #endregion
        
        public CameraManagerDirectShow()
        {
            defaultIcon = IconLibrary.GetIcon("webcam");
        }
        
        public override List<CameraSummary> DiscoverCameras(IEnumerable<CameraBlurb> blurbs)
        {
            // DirectShow has active discovery. We just ask for the list of cameras connected to the PC.
            List<CameraSummary> summaries = new List<CameraSummary>();
            List<CameraSummary> found = new List<CameraSummary>();
            
            FilterInfoCollection cameras = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            
            foreach(FilterInfo camera in cameras)
            {
                // For now consider that the moniker string is like a serial number.
                // Apparently this is only true for certain models.
                // Check if we should extract the serial number part so that we don't change id when changing USB port.
                string identifier = camera.MonikerString;
                
                string alias = camera.Name;
                Bitmap icon = null;
                bool cached = cache.ContainsKey(identifier);
                
                if(blurbs != null)
                {
                    foreach(CameraBlurb blurb in blurbs)
                    {
                        if(blurb.Identifier != identifier)
                            continue;
                            
                        alias = blurb.Alias;
                        icon = blurb.Icon ?? defaultIcon;
                        break;
                    }
                }
                
                if(icon == null)
                    icon = defaultIcon;
                
                CameraSummary summary = new CameraSummary(alias, camera.Name, identifier, icon, null, this);
                summaries.Add(summary);
                
                if(cached)
                    found.Add(cache[identifier]);
                    
                if(!cached)
                {
                    cache.Add(identifier, summary);
                    found.Add(summary);
                }
            }
            
            // TODO: do we need to do all this. Just replace the cache with the current list.
            
            List<CameraSummary> lost = new List<CameraSummary>();
            foreach(CameraSummary summary in cache.Values)
            {
                if(!found.Contains(summary))
                   lost.Add(summary);
            }
            
            foreach(CameraSummary summary in lost)
                cache.Remove(summary.Identifier);

            return summaries;
        }
        
        public override void GetSingleImage(CameraSummary summary)
        {
            // TODO: Retrieve moniker from identifier.
            string moniker = summary.Identifier;
            
            // Spawn a thread to get a snapshot.
            SnapshotRetriever retriever = new SnapshotRetriever(summary, moniker);
            retriever.CameraImageReceived += SnapshotRetriever_CameraImageReceived;
            ThreadPool.QueueUserWorkItem(retriever.Run);
        }
        
        public override CameraBlurb BlurbFromSummary(CameraSummary summary)
        {
            CameraBlurb blurb = new CameraBlurb(CameraType, summary.Identifier, summary.Alias, summary.Icon);
            // TODO: Add information specific to DirectShow plug-in. (In the form of an XML node ?)
            return blurb;
        }
        
        public override IFrameGrabber Connect(CameraSummary summary)
        {
            // TODO: Retrieve moniker from identifier.
            string moniker = summary.Identifier;
            
            FrameGrabber grabber = new FrameGrabber(summary, moniker);
            return grabber;
        }
        
        public override bool Configure(CameraSummary summary)
        {
            bool needsReconnection = false;
            FormConfiguration form = new FormConfiguration(summary);
            if(form.ShowDialog() == DialogResult.OK)
            {
                if(form.AliasChanged)
                    summary.UpdateAlias(form.Alias, form.PickedIcon);
                
                if(form.SpecificChanged)
                {
                    SpecificInfo info = new SpecificInfo();
                    info.SelectedCapability = form.Capability;
                    summary.UpdateSpecific(info);

                    needsReconnection = true;
                }
                
                CameraTypeManager.UpdatedCameraSummary(summary);
            }
            
            form.Dispose();
            return needsReconnection;
        }
        
        public override string GetSummaryAsText(CameraSummary summary)
        {
            string result = "";
            string alias = summary.Alias;
            
            SpecificInfo info = summary.Specific as SpecificInfo;
            if(info != null && info.SelectedCapability != null)
            {
                Size size = info.SelectedCapability.FrameSize;
                float fps = (float)info.SelectedCapability.FrameRate;
                result = string.Format("{0} - {1}×{2} @ {3}fps", alias, size.Width, size.Height, fps);
            }
            else
            {
                result = string.Format("{0}", alias);
            }
            
            return result;
        }
        
        private void SnapshotRetriever_CameraImageReceived(object sender, CameraImageReceivedEventArgs e)
        {
            SnapshotRetriever retriever = sender as SnapshotRetriever;
            if(retriever != null)
                retriever.CameraImageReceived -= SnapshotRetriever_CameraImageReceived;
                
            OnCameraImageReceived(e);
        }
    }
}