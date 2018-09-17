#region License
/*
Copyright © Joan Charmant 2012.
jcharmant@gmail.com 
 
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
using System.Text.RegularExpressions;

namespace Kinovea.Camera.DirectShow
{
    /// <summary>
    /// Class to discover and manage cameras connected through DirectShow.
    /// </summary>
    public class CameraManagerDirectShow : CameraManager
    {
        #region Properties
        public override bool Enabled
        {
            get { return true; }
        }
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
        private HashSet<string> blacklist = new HashSet<string>();
        private Regex idsPattern = new Regex(@"^UI\d{3,4}");
        #endregion
        
        public CameraManagerDirectShow()
        {   
            blacklist.Add("Basler GenICam Source");
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
                if (IsBlackListed(camera.Name))
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
                        icon = blurb.Icon ?? SelectDefaultIcon(identifier);
                        displayRectangle = blurb.DisplayRectangle;
                        if(!string.IsNullOrEmpty(blurb.AspectRatio))
                            aspectRatio = (CaptureAspectRatio)Enum.Parse(typeof(CaptureAspectRatio), blurb.AspectRatio);
                        
                        specific = SpecificInfoDeserialize(blurb.Specific);
                        VendorHelper.IdentifyModel(identifier);
                        break;
                    }
                }
                
                if(icon == null)
                    icon = SelectDefaultIcon(identifier);
                
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
            FormsHelper.Locate(form);
            if(form.ShowDialog() == DialogResult.OK)
            {
                if(form.AliasChanged)
                    summary.UpdateAlias(form.Alias, form.PickedIcon);
                
                if(form.SpecificChanged)
                {
                    SpecificInfo info = new SpecificInfo();
                    info.MediaTypeIndex = form.SelectedMediaTypeIndex;
                    info.SelectedFramerate = form.SelectedFramerate;
                    info.CameraProperties = form.CameraProperties;

                    summary.UpdateSpecific(info);

                    summary.UpdateDisplayRectangle(Rectangle.Empty);
                    needsReconnection = form.NeedsReconnection;
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
                    result = string.Format("{0} - {1}×{2} @ {3:0.##} fps ({4}).", alias, size.Width, size.Height, fps, compression);
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
            if (retriever != null)
            {
                retriever.CameraThumbnailProduced -= SnapshotRetriever_CameraThumbnailProduced;
                snapshotting.Remove(retriever.Identifier);
            }

            OnCameraThumbnailProduced(e);
        }

        private Bitmap SelectDefaultIcon(string identifier)
        {
            if (identifier.Contains("usb#vid_046d"))
                return IconLibrary.GetIcon("logitech");
            else if (identifier.Contains("usb#vid_045e"))
                return IconLibrary.GetIcon("microsoft");
            else if (identifier.Contains("PS3Eye Camera"))
                return IconLibrary.GetIcon("playstation");
            else
                return IconLibrary.GetIcon("webcam");
        }

        private bool IsBlackListed(string name)
        {
            // This blacklist is used to bypass DirectShow handling of cameras for which we have a dedicated module.
            if (blacklist.Contains(name))
                return true;

            // IDS uEye.
            Match match = idsPattern.Match(name);
            if (match.Success)
            {
                // Add the camera to the blacklist to avoid running the regex on every device enumeration.
                blacklist.Add(name);
                return true;
            }

            return false;
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

                XmlNode xmlSelectedFrameRate = doc.SelectSingleNode("/DirectShow/SelectedFramerate");
                if (xmlSelectedFrameRate != null)
                    selectedFramerate = float.Parse(xmlSelectedFrameRate.InnerText, CultureInfo.InvariantCulture);

                XmlNode xmlIndex = doc.SelectSingleNode("/DirectShow/MediaTypeIndex");
                if (xmlIndex != null)
                    index = int.Parse(xmlIndex.InnerText, CultureInfo.InvariantCulture);

                Dictionary<string, CameraProperty> cameraProperties = new Dictionary<string, CameraProperty>();

                XmlNodeList props = doc.SelectNodes("/DirectShow/CameraProperties/CameraProperty2");
                foreach (XmlNode node in props)
                {
                    XmlAttribute keyAttribute = node.Attributes["key"];
                    if (keyAttribute == null)
                        continue;

                    string key = keyAttribute.Value;
                    CameraProperty property = new CameraProperty();

                    string xpath = string.Format("/DirectShow/CameraProperties/CameraProperty2[@key='{0}']", key);
                    XmlNode xmlPropertyValue = doc.SelectSingleNode(xpath + "/Value");
                    if (xmlPropertyValue != null)
                        property.CurrentValue = xmlPropertyValue.InnerText;
                    else
                        property.Supported = false;

                    XmlNode xmlPropertyAuto = doc.SelectSingleNode(xpath + "/Auto");
                    if (xmlPropertyAuto != null)
                        property.Automatic = XmlHelper.ParseBoolean(xmlPropertyAuto.InnerText);
                    else
                        property.Supported = false;

                    cameraProperties.Add(key, property);
                }

                info.MediaTypeIndex = index;
                info.SelectedFramerate = selectedFramerate;
                info.CameraProperties = cameraProperties;
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

            XmlElement xmlCameraProperties = doc.CreateElement("CameraProperties");

            foreach (KeyValuePair<string, CameraProperty> pair in info.CameraProperties)
            {
                XmlElement xmlCameraProperty2 = doc.CreateElement("CameraProperty2");
                XmlAttribute attr = doc.CreateAttribute("key");
                attr.Value = pair.Key;
                xmlCameraProperty2.Attributes.Append(attr);

                XmlElement xmlCameraProperty2Value = doc.CreateElement("Value");
                xmlCameraProperty2Value.InnerText = pair.Value.CurrentValue;
                xmlCameraProperty2.AppendChild(xmlCameraProperty2Value);

                XmlElement xmlCameraProperty2Auto = doc.CreateElement("Auto");
                xmlCameraProperty2Auto.InnerText = pair.Value.Automatic.ToString().ToLower();
                xmlCameraProperty2.AppendChild(xmlCameraProperty2Auto);

                xmlCameraProperties.AppendChild(xmlCameraProperty2);
            }

            xmlRoot.AppendChild(xmlCameraProperties);

            doc.AppendChild(xmlRoot);
            
            return doc.OuterXml;
        }
    }
}