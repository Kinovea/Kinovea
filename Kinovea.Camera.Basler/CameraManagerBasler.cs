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
using PylonC.NET;
using Kinovea.Services;
using PylonC.NETSupportLibrary;
using Kinovea.Video;

namespace Kinovea.Camera.Basler
{
    /// <summary>
    /// Class to discover and manage Basler cameras (Pylon API).
    /// </summary>
    public class CameraManagerBasler : CameraManager
    {
        //---------------------------------------------------------------------------------------------------------
        // Note: The .NET framework will only delay-load the assembly as we don't need it in any public field.
        // This means the Pylon wrapper DLL will not be copied to the output directory.
        // Even if we use it here, since the Pylon installer added it to the GAC, it won't be copied to the output.
        // The copy is done explicitely in kinovea.targets.
        //---------------------------------------------------------------------------------------------------------

        #region Properties
        public override bool Enabled
        {
            get { return true; }
        }
        public override string CameraType 
        { 
            get { return "B7FE6FE2-A98C-11E2-97AA-7A3A79957A39";}
        }
        public override string CameraTypeFriendlyName 
        { 
            get { return "Basler"; }
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
        private int discoveryStep = 0;
        private int discoverySkip = 5;
        private Dictionary<string, uint> deviceIndices = new Dictionary<string, uint>();
        #endregion
        
        public CameraManagerBasler()
        {
            defaultIcon = IconLibrary.GetIcon("basler");
        }
        
        public override bool SanityCheck()
        {
            bool result = false;
            try
            {
                Pylon.Initialize();
                result = true;
            }
            catch
            {
                log.DebugFormat("Basler Camera subsystem not available.");
            }

            return result;
        }
        
        public override List<CameraSummary> DiscoverCameras(IEnumerable<CameraBlurb> blurbs)
        {
            List<CameraSummary> summaries = new List<CameraSummary>();
        
            // We don't do the discover step every time to avoid UI freeze since Enumerate takes some time.
            if(discoveryStep > 0)
            {
                discoveryStep = (discoveryStep + 1) % discoverySkip;
                foreach(CameraSummary summary in cache.Values)
                    summaries.Add(summary);
                
                return summaries;
            }

            discoveryStep = 1;
            List<CameraSummary> found = new List<CameraSummary>();
            
            List<DeviceEnumerator.Device> devices = DeviceEnumerator.EnumerateDevices();
            foreach (DeviceEnumerator.Device device in devices)
            {
                string identifier = device.FullName;
                
                bool cached = cache.ContainsKey(identifier);
                if(cached)
                {
                    deviceIndices[identifier] = device.Index;
                    summaries.Add(cache[identifier]);
                    found.Add(cache[identifier]);
                    continue;
                }

                string alias = device.Name;
                Bitmap icon = null;
                SpecificInfo specific = new SpecificInfo();
                Rectangle displayRectangle = Rectangle.Empty;
                CaptureAspectRatio aspectRatio = CaptureAspectRatio.Auto;
                ImageRotation rotation = ImageRotation.Rotate0;
                deviceIndices[identifier] = device.Index;

                if(blurbs != null)
                {
                    foreach(CameraBlurb blurb in blurbs)
                    {
                        if(blurb.CameraType != this.CameraType || blurb.Identifier != identifier)
                            continue;

                        // We already know this camera, restore the user custom values.
                        alias = blurb.Alias;
                        icon = blurb.Icon ?? defaultIcon;
                        displayRectangle = blurb.DisplayRectangle;
                        if(!string.IsNullOrEmpty(blurb.AspectRatio))
                            aspectRatio = (CaptureAspectRatio)Enum.Parse(typeof(CaptureAspectRatio), blurb.AspectRatio);
                        
                        if (!string.IsNullOrEmpty(blurb.Rotation))
                            rotation = (ImageRotation)Enum.Parse(typeof(ImageRotation), blurb.Rotation);
                        // Restore saved parameters.
                        specific = SpecificInfoDeserialize(blurb.Specific);
                        break;
                    }
                }

                icon = icon ?? defaultIcon;
            
                CameraSummary summary = new CameraSummary(alias, device.Name, identifier, icon, displayRectangle, aspectRatio, rotation, specific, this);

                summaries.Add(summary);
                found.Add(summary);
                cache.Add(identifier, summary);
            }
            
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
            if(cache.ContainsKey(summary.Identifier))
                cache.Remove(summary.Identifier);
        }

        public override void GetSingleImage(CameraSummary summary)
        {
            if(snapshotting.IndexOf(summary.Identifier) >= 0)
                return;
            
            // Spawn a thread to get a snapshot.
            SnapshotRetriever retriever = new SnapshotRetriever(summary, deviceIndices[summary.Identifier]);
            retriever.CameraThumbnailProduced += SnapshotRetriever_CameraThumbnailProduced;
            snapshotting.Add(summary.Identifier);
            ThreadPool.QueueUserWorkItem(retriever.Run);
        }
        
        public override CameraBlurb BlurbFromSummary(CameraSummary summary)
        {
            string specific = SpecificInfoSerialize(summary);
            CameraBlurb blurb = new CameraBlurb(CameraType, summary.Identifier, summary.Alias, summary.Icon, summary.DisplayRectangle, summary.AspectRatio.ToString(), summary.Rotation.ToString(), specific);
            return blurb;
        }
        
        public override ICaptureSource CreateCaptureSource(CameraSummary summary)
        {
            FrameGrabber grabber = new FrameGrabber(summary, deviceIndices[summary.Identifier]);
            return grabber;
        }

        public override bool Configure(CameraSummary summary)
        {
            throw new NotImplementedException();
        }

        public override bool Configure(CameraSummary summary, Action disconnect, Action connect)
        {
            bool needsReconnection = false;
            SpecificInfo info = summary.Specific as SpecificInfo;
            if (info == null)
                return false;

            FormConfiguration form = new FormConfiguration(summary, disconnect, connect);
            FormsHelper.Locate(form);
            if (form.ShowDialog() == DialogResult.OK)
            {
                if(form.AliasChanged)
                    summary.UpdateAlias(form.Alias, form.PickedIcon);
                
                if(form.SpecificChanged)
                {
                    info.StreamFormat = form.SelectedStreamFormat.Symbol;
                    info.Bayer8Conversion = form.Bayer8Conversion;
                    info.CameraProperties = form.CameraProperties;

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

            try
            {
                if (info != null &&
                    info.StreamFormat != null &&
                    info.CameraProperties.ContainsKey("width") &&
                    info.CameraProperties.ContainsKey("height") &&
                    info.CameraProperties.ContainsKey("framerate"))
                {
                    string format = info.StreamFormat;
                    int width = int.Parse(info.CameraProperties["width"].CurrentValue, CultureInfo.InvariantCulture);
                    int height = int.Parse(info.CameraProperties["height"].CurrentValue, CultureInfo.InvariantCulture);
                    double framerate = 0;

                    // The configured framerate is always between 0 and 100 000, but the actual resulting framerate can be obtained.
                    if (info.Handle != null && info.Handle.IsValid)
                        framerate = PylonHelper.GetResultingFramerate(info.Handle);
                    else
                        framerate = double.Parse(info.CameraProperties["framerate"].CurrentValue, CultureInfo.InvariantCulture);

                    result = string.Format("{0} - {1}×{2} @ {3:0.##} fps ({4}).", alias, width, height, framerate, format);
                }
                else
                {
                    result = string.Format("{0}", alias);
                }
            }
            catch
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
            if(retriever != null)
            {
                retriever.CameraThumbnailProduced -= SnapshotRetriever_CameraThumbnailProduced;
                if (snapshotting != null && snapshotting.Count > 0)
                    snapshotting.Remove(retriever.Identifier);
            }
            
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

                string streamFormat = "";
                XmlNode xmlStreamFormat = doc.SelectSingleNode("/Basler/StreamFormat");
                if (xmlStreamFormat != null)
                    streamFormat = xmlStreamFormat.InnerText;

                Bayer8Conversion bayer8Conversion = Bayer8Conversion.Color;
                XmlNode xmlBayer8Conversion = doc.SelectSingleNode("/Basler/Bayer8Conversion");
                if (xmlBayer8Conversion != null)
                    bayer8Conversion = (Bayer8Conversion)Enum.Parse(typeof(Bayer8Conversion), xmlBayer8Conversion.InnerText);
                
                Dictionary<string, CameraProperty> cameraProperties = new Dictionary<string, CameraProperty>();

                XmlNodeList props = doc.SelectNodes("/Basler/CameraProperties/CameraProperty");
                foreach (XmlNode node in props)
                {
                    XmlAttribute keyAttribute = node.Attributes["key"];
                    if (keyAttribute == null)
                        continue;

                    string key = keyAttribute.Value;
                    CameraProperty property = new CameraProperty();

                    string xpath = string.Format("/Basler/CameraProperties/CameraProperty[@key='{0}']", key);
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

                info.StreamFormat = streamFormat;
                info.Bayer8Conversion = bayer8Conversion;
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
            XmlElement xmlRoot = doc.CreateElement("Basler");

            XmlElement xmlStreamFormat = doc.CreateElement("StreamFormat");
            xmlStreamFormat.InnerText = info.StreamFormat;
            xmlRoot.AppendChild(xmlStreamFormat);

            XmlElement xmlBayer8Conversion = doc.CreateElement("Bayer8Conversion");
            xmlBayer8Conversion.InnerText = info.Bayer8Conversion.ToString();
            xmlRoot.AppendChild(xmlBayer8Conversion);
            
            XmlElement xmlCameraProperties = doc.CreateElement("CameraProperties");

            foreach (KeyValuePair<string, CameraProperty> pair in info.CameraProperties)
            {
                XmlElement xmlCameraProperty = doc.CreateElement("CameraProperty");
                XmlAttribute attr = doc.CreateAttribute("key");
                attr.Value = pair.Key;
                xmlCameraProperty.Attributes.Append(attr);

                XmlElement xmlCameraPropertyValue = doc.CreateElement("Value");
                xmlCameraPropertyValue.InnerText = pair.Value.CurrentValue;
                xmlCameraProperty.AppendChild(xmlCameraPropertyValue);

                XmlElement xmlCameraPropertyAuto = doc.CreateElement("Auto");
                xmlCameraPropertyAuto.InnerText = pair.Value.Automatic.ToString().ToLower();
                xmlCameraProperty.AppendChild(xmlCameraPropertyAuto);

                xmlCameraProperties.AppendChild(xmlCameraProperty);
            }

            xmlRoot.AppendChild(xmlCameraProperties);

            doc.AppendChild(xmlRoot);
            
            return doc.OuterXml;
        }
    }
}