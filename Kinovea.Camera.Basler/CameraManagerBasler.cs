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

using PylonC.NET;
using Kinovea.Services;

namespace Kinovea.Camera.Basler
{
    /// <summary>
    /// Class to discover and manage Basler cameras (Pylon API).
    /// </summary>
    public class CameraManagerBasler : CameraManager
    {
        #region Properties
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
            defaultIcon = IconLibrary.GetIcon("webcam");
        }
        
        public override bool SanityCheck()
        {
            bool result = false;
            try
            {
                // Trigger a P/Invoke to see if the correct native DLL is installed.
                Pylon.EnumerateDevices();
                result = true;
            }
            catch (Exception e)
            {
                log.DebugFormat("Basler Camera subsystem not available. {0}", e.ToString());
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
            
            // Takes about 250ms.
            uint count = Pylon.EnumerateDevices();
            
            for( uint i = 0; i < count; i++)
            {
                Device device = new Device(i);
                string identifier = device.SerialNumber;
                
                bool cached = cache.ContainsKey(identifier);
                if(cached)
                {
                    deviceIndices[identifier] = i;
                    summaries.Add(cache[identifier]);
                    found.Add(cache[identifier]);
                    continue;
                }
                
                string alias = device.Name;
                Bitmap icon = null;
                SpecificInfo specific = new SpecificInfo();
                Rectangle displayRectangle = Rectangle.Empty;
                CaptureAspectRatio aspectRatio = CaptureAspectRatio.Auto;
                deviceIndices[identifier] = i;
                
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
                        //specific = SpecificInfoDeserialize(blurb.Specific);
                        break;
                    }
                }

                icon = icon ?? defaultIcon;
            
                CameraSummary summary = new CameraSummary(alias, device.Name, identifier, icon, displayRectangle, aspectRatio, specific, this);

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
            CameraBlurb blurb = new CameraBlurb(CameraType, summary.Identifier, summary.Alias, summary.Icon, summary.DisplayRectangle, summary.AspectRatio.ToString(), specific);
            return blurb;
        }
        
        public override ICaptureSource Connect(CameraSummary summary)
        {
            FrameGrabber grabber = new FrameGrabber(summary, deviceIndices[summary.Identifier]);
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
                
                /*if(form.SpecificChanged)
                {
                    SpecificInfo info = new SpecificInfo();
                    info.SelectedFrameRate = form.Capability.FrameRate;
                    info.SelectedFrameSize = form.Capability.FrameSize;
                    summary.UpdateSpecific(info);
                    summary.UpdateDisplayRectangle(Rectangle.Empty);

                    needsReconnection = true;
                }*/
                
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
            /*if(info != null)
            {
                //Size size = info.SelectedFrameSize;
                //float fps = (float)info.SelectedFrameRate;
                //result = string.Format("{0} - {1}×{2} @ {3}fps", alias, size.Width, size.Height, fps);
            }
            else
            {*/
                result = string.Format("{0}", alias);
            //}
            
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
                snapshotting.Remove(retriever.Identifier);
            }
            
            OnCameraThumbnailProduced(e);
        }
        
        private SpecificInfo SpecificInfoDeserialize(string xml)
        {
            if(string.IsNullOrEmpty(xml))
                return null;
            
            SpecificInfo info = new SpecificInfo();
            
            try
            {
                /*XmlDocument doc = new XmlDocument();
                doc.Load(new StringReader(xml));

                info = new SpecificInfo();
                
                int frameRate = 0;
                XmlNode xmlFrameRate = doc.SelectSingleNode("/DirectShow/SelectedFrameRate");
                if(xmlFrameRate != null)
                {
                    string strFrameRate = xmlFrameRate.InnerText;
                    frameRate = int.Parse(strFrameRate, CultureInfo.InvariantCulture);
                }
                info.SelectedFrameRate = frameRate;
                
                Size frameSize = Size.Empty;
                XmlNode xmlFrameSize = doc.SelectSingleNode("/DirectShow/SelectedFrameSize");
                if(xmlFrameSize != null)
                {
                    string strFrameSize = xmlFrameSize.InnerText;
                    frameSize = XmlHelper.ParseSize(strFrameSize);
                }
                info.SelectedFrameSize = frameSize;*/
            }
            catch(Exception e)
            {
                log.ErrorFormat(e.Message);
            }
            
            return info;
        }
        
        private string SpecificInfoSerialize(CameraSummary summary)
        {
            /*SpecificInfo info = summary.Specific as SpecificInfo;
            if(info == null)
                return null;
                
            XmlDocument doc = new XmlDocument();
            XmlElement xmlRoot = doc.CreateElement("Basler");
            
            XmlElement xmlFrameRate = doc.CreateElement("SelectedFrameRate");
            string framerate = string.Format("{0}", info.SelectedFrameRate);
            xmlFrameRate.InnerText = framerate;
            xmlRoot.AppendChild(xmlFrameRate);
            
            XmlElement xmlFrameSize = doc.CreateElement("SelectedFrameSize");
            string frameSize = string.Format("{0};{1}", info.SelectedFrameSize.Width, info.SelectedFrameSize.Height);
            xmlFrameSize.InnerText = frameSize;
            xmlRoot.AppendChild(xmlFrameSize);
            
            doc.AppendChild(xmlRoot);
            
            return doc.OuterXml;*/
            return "";
        }
    }
}