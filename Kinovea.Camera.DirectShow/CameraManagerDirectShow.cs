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
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

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
        public override string CameraTypeFriendlyName 
        { 
            get { return "DirectShow"; }
        }
        public override bool HasConnectionWizard
        {
            get { return false;}
        }
        #endregion
    
        #region Members
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private Dictionary<string, CameraSummary> cache = new Dictionary<string, CameraSummary>();
        private List<string> snapshotting = new List<string>();
        private Bitmap defaultIcon;
        private HashSet<string> bypass = new HashSet<string>();
        #endregion
        
        public CameraManagerDirectShow()
        {
            defaultIcon = IconLibrary.GetIcon("webcam");
            
            // Bypass DirectShow filters of cameras for which we have a dedicated plugin.
            bypass.Add("Basler GenICam Source");
            bypass.Add("FlyCapture2 Camera");
            //bypass.Add("Logitech HD Pro Webcam C920");
            //bypass.Add("Logitech Webcam C100");
            //bypass.Add("PS3Eye Camera");
            bypass.Add("uEye Capture Device 1");
        }

        public override bool SanityCheck()
        {
            return true;
        }
        
        public override List<CameraSummary> DiscoverCameras(IEnumerable<CameraBlurb> blurbs)
        {
            // DirectShow has active discovery. We just ask for the list of cameras connected to the PC.
            List<CameraSummary> summaries = new List<CameraSummary>();
            List<CameraSummary> found = new List<CameraSummary>();
            
            FilterInfoCollection cameras = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            
            foreach(FilterInfo camera in cameras)
            {
                if(bypass.Contains(camera.Name))
                    continue;

                // For now consider that the moniker string is like a serial number.
                // Cameras that don't have a serial number will appear to be new when changing USB port.
                string identifier = camera.MonikerString;
                bool cached = cache.ContainsKey(identifier);
                
                string alias = camera.Name;
                Bitmap icon = null;
                SpecificInfo specific = null;
                Rectangle displayRectangle = Rectangle.Empty;
                CaptureAspectRatio aspectRatio = CaptureAspectRatio.Auto;
                
                if(blurbs != null)
                {
                    foreach(CameraBlurb blurb in blurbs)
                    {
                        if(blurb.CameraType != this.CameraType || blurb.Identifier != identifier)
                            continue;
                            
                        alias = blurb.Alias;
                        icon = blurb.Icon ?? defaultIcon;
                        displayRectangle = blurb.DisplayRectangle;
                        if(!string.IsNullOrEmpty(blurb.AspectRatio))
                            aspectRatio = (CaptureAspectRatio)Enum.Parse(typeof(CaptureAspectRatio), blurb.AspectRatio);
                        specific = SpecificInfoDeserialize(blurb.Specific);
                        break;
                    }
                }
                
                if(icon == null)
                    icon = defaultIcon;
                
                CameraSummary summary = new CameraSummary(alias, camera.Name, identifier, icon, displayRectangle, aspectRatio, specific, this);
                summaries.Add(summary);
                
                if(cached)
                    found.Add(cache[identifier]);
                    
                if(!cached)
                {
                    cache.Add(identifier, summary);
                    found.Add(summary);
                    log.DebugFormat("DirectShow device enumeration: {0} (moniker:{1}).", summary.Alias, identifier);
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

        public override void ForgetCamera(CameraSummary summary)
        {
            if (cache.ContainsKey(summary.Identifier))
                cache.Remove(summary.Identifier);
        }
        
        public override void GetSingleImage(CameraSummary summary)
        {
            if(snapshotting.IndexOf(summary.Identifier) >= 0)
                return;
            
            // TODO: Retrieve moniker from identifier.
            string moniker = summary.Identifier;
            
            // Spawn a thread to get a snapshot.
            SnapshotRetriever retriever = new SnapshotRetriever(summary, moniker);
            retriever.CameraThumbnailProduced += SnapshotRetriever_CameraThumbnailProduced;
            snapshotting.Add(summary.Identifier);
            ThreadPool.QueueUserWorkItem(retriever.Run);
        }
        
        public override CameraBlurb BlurbFromSummary(CameraSummary summary)
        {
            string specific = SpecificInfoSerialize(summary);
            CameraBlurb blurb = new CameraBlurb(CameraType, summary.Identifier, summary.Alias, summary.Icon, summary.DisplayRectangle, summary.AspectRatio.ToString(), specific);
            return blurb;
        }
        
        public override ICaptureSource CreateCaptureSource(CameraSummary summary)
        {
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
                    info.MediaTypeIndex = form.SelectedMediaTypeIndex;
                    info.SelectedFramerate = form.SelectedFramerate;
                    
                    if (form.HasExposureControl)
                    {
                        info.HasExposureControl = true;
                        info.ManualExposure = form.ManualExposure;
                        info.ExposureValue = form.ExposureValue;
                        info.UseLogitechExposure = form.UseLogitechExposure;
                    }
                    
                    summary.UpdateSpecific(info);

                    summary.UpdateDisplayRectangle(Rectangle.Empty);
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
            if(info != null && info.MediaTypeIndex >= 0)
            {
                VideoCaptureDevice device = new VideoCaptureDevice(summary.Identifier);
                Dictionary<int, MediaType> mediaTypes = MediaTypeImporter.Import(device);
                if (mediaTypes.ContainsKey(info.MediaTypeIndex))
                {
                    Size size = mediaTypes[info.MediaTypeIndex].FrameSize;
                    float fps = (float)info.SelectedFramerate;
                    string compression = mediaTypes[info.MediaTypeIndex].Compression;
                    result = string.Format("{0} - {1}×{2} @ {3:0.###} fps ({4}).", alias, size.Width, size.Height, fps, compression);
                }
                else
                {
                    result = string.Format("{0}", alias);
                }
            }
            else
            {
                result = string.Format("{0}", alias);
            }
            
            return result;
        }
        
        public override Control GetConnectionWizard()
        {
            throw new NotImplementedException();
        }

        private void SnapshotRetriever_CameraThumbnailProduced(object sender, CameraThumbnailProducedEventArgs e)
        {
            SnapshotRetriever retriever = sender as SnapshotRetriever;
            if (retriever == null)
                return;

            retriever.CameraThumbnailProduced -= SnapshotRetriever_CameraThumbnailProduced;
            snapshotting.Remove(retriever.Identifier);

            if (e.Thumbnail != null && !e.HadError && !e.Cancelled)
                OnCameraThumbnailProduced(e);
        }
        
        private SpecificInfo SpecificInfoDeserialize(string xml)
        {
            if(string.IsNullOrEmpty(xml))
                return null;
            
            SpecificInfo info = null;
            
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(new StringReader(xml));

                info = new SpecificInfo();

                float selectedFramerate = -1;
                int index = -1;
                bool hasExposureControl = false;
                bool manualExposure = false;
                long exposureValue = 0;
                bool useLogitechExposure = false;

                XmlNode xmlSelectedFrameRate = doc.SelectSingleNode("/DirectShow/SelectedFramerate");
                if (xmlSelectedFrameRate != null)
                    selectedFramerate = float.Parse(xmlSelectedFrameRate.InnerText, CultureInfo.InvariantCulture);

                XmlNode xmlIndex = doc.SelectSingleNode("/DirectShow/MediaTypeIndex");
                if (xmlIndex != null)
                    index = int.Parse(xmlIndex.InnerText, CultureInfo.InvariantCulture);

                // Exposure
                XmlNode xmlHasExposureControl = doc.SelectSingleNode("/DirectShow/HasExposureControl");
                if (xmlHasExposureControl != null)
                    hasExposureControl = XmlHelper.ParseBoolean(xmlHasExposureControl.InnerText);

                XmlNode xmlManualExposure = doc.SelectSingleNode("/DirectShow/ManualExposure");
                if (xmlManualExposure != null)
                    manualExposure = XmlHelper.ParseBoolean(xmlManualExposure.InnerText);

                XmlNode xmlExposureValue = doc.SelectSingleNode("/DirectShow/ExposureValue");
                if (xmlExposureValue != null)
                    exposureValue = long.Parse(xmlExposureValue.InnerText, CultureInfo.InvariantCulture);

                XmlNode xmlUseLogitechExposure = doc.SelectSingleNode("/DirectShow/UseLogitechExposure");
                if (xmlUseLogitechExposure != null)
                    useLogitechExposure = XmlHelper.ParseBoolean(xmlUseLogitechExposure.InnerText);

                info.MediaTypeIndex = index;
                info.SelectedFramerate = selectedFramerate;
                info.HasExposureControl = hasExposureControl;
                info.ManualExposure = manualExposure;
                info.ExposureValue = exposureValue;
                info.UseLogitechExposure = useLogitechExposure;
            }
            catch(Exception e)
            {
                log.ErrorFormat(e.Message);
            }
            
            return info;
        }
        
        private string SpecificInfoSerialize(CameraSummary summary)
        {
            SpecificInfo info = summary.Specific as SpecificInfo;
            if(info == null)
                return null;
                
            XmlDocument doc = new XmlDocument();
            XmlElement xmlRoot = doc.CreateElement("DirectShow");

            if (info.MediaTypeIndex < 0)
            {
                doc.AppendChild(xmlRoot);
                return doc.OuterXml;
            }

            XmlElement xmlIndex = doc.CreateElement("MediaTypeIndex");
            xmlIndex.InnerText = string.Format("{0}", info.MediaTypeIndex);
            xmlRoot.AppendChild(xmlIndex);

            XmlElement xmlFramerate = doc.CreateElement("SelectedFramerate");
            string fps = info.SelectedFramerate.ToString("0.000", CultureInfo.InvariantCulture);
            xmlFramerate.InnerText = fps;
            xmlRoot.AppendChild(xmlFramerate);

            // Exposure
            XmlElement xmlHasExposureControl = doc.CreateElement("HasExposureControl");
            xmlHasExposureControl.InnerText = info.HasExposureControl.ToString().ToLower();
            xmlRoot.AppendChild(xmlHasExposureControl);

            XmlElement xmlManualExposure = doc.CreateElement("ManualExposure");
            xmlManualExposure.InnerText = info.ManualExposure.ToString().ToLower();
            xmlRoot.AppendChild(xmlManualExposure);

            XmlElement xmlExposureValue = doc.CreateElement("ExposureValue");
            xmlExposureValue.InnerText = string.Format("{0}", info.ExposureValue);
            xmlRoot.AppendChild(xmlExposureValue);
            
            XmlElement xmlUseLogitechExposure = doc.CreateElement("UseLogitechExposure");
            xmlUseLogitechExposure.InnerText = info.UseLogitechExposure.ToString().ToLower();
            xmlRoot.AppendChild(xmlUseLogitechExposure);

            doc.AppendChild(xmlRoot);
            
            return doc.OuterXml;
        }
    }
}