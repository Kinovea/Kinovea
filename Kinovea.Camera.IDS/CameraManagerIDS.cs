#region License
/*
Copyright © Joan Charmant 2017.
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
using Kinovea.Services;
using Kinovea.Video;

namespace Kinovea.Camera.IDS
{
    /// <summary>
    /// Class to discover and manage IDS cameras (uEye API).
    /// </summary>
    public class CameraManagerIDS : CameraManager
    {
        #region Properties
        public override bool Enabled
        {
            get { return true; }
        }
        public override string CameraType 
        { 
            get { return "E43F59FE-E02D-4E73-8B6B-ACDBAE102044";}
        }
        public override string CameraTypeFriendlyName 
        { 
            get { return "IDS uEye"; }
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
        private Dictionary<string, long> deviceIds = new Dictionary<string, long>();
        #endregion
        
        public CameraManagerIDS()
        {
            defaultIcon = IconLibrary.GetIcon("ids");
        }
        
        public override bool SanityCheck()
        {
            bool result = false;
            try
            {
                Version apiVersion;
                uEye.Info.System.GetApiVersion(out apiVersion);
                Version wrapperVersion;
                uEye.Info.System.GetNetVersion(out wrapperVersion);
                log.DebugFormat("IDS uEye API version: {0}, .NET wrapper version: {1}.", apiVersion.ToString(), wrapperVersion.ToString());
                result = true;
            }
            catch
            {
                log.DebugFormat("IDS uEye Camera subsystem not available.");
            }

            return result;
        }
        
        public override List<CameraSummary> DiscoverCameras(IEnumerable<CameraBlurb> blurbs)
        {
            List<CameraSummary> summaries = new List<CameraSummary>();

            List<CameraSummary> found = new List<CameraSummary>();
            uEye.Types.CameraInformation[] devices;
            uEye.Info.Camera.GetCameraList(out devices);

            foreach (uEye.Types.CameraInformation device in devices)
            {
                string identifier = device.SerialNumber;
                bool cached = cache.ContainsKey(identifier);

                if(cached)
                {
                    deviceIds[identifier] = device.DeviceID;
                    summaries.Add(cache[identifier]);
                    found.Add(cache[identifier]);
                    continue;
                }
                
                string alias = device.Model;
                Bitmap icon = null;
                SpecificInfo specific = new SpecificInfo();
                Rectangle displayRectangle = Rectangle.Empty;
                CaptureAspectRatio aspectRatio = CaptureAspectRatio.Auto;
                ImageRotation rotation = ImageRotation.Rotate0;
                bool mirror = false;
                deviceIds[identifier] = device.DeviceID;
                
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
                        mirror = blurb.Mirror;
                        specific = SpecificInfoDeserialize(blurb.Specific);
                        break;
                    }
                }

                icon = icon ?? defaultIcon;
            
                CameraSummary summary = new CameraSummary(alias, device.Model, identifier, icon, displayRectangle, aspectRatio, rotation, mirror, specific, this);

                summaries.Add(summary);
                found.Add(summary);
                cache.Add(identifier, summary);

                //log.DebugFormat("IDS uEye device enumeration: {0} (id:{1}).", summary.Alias, identifier);
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

            ProfileHelper.Delete(summary.Identifier);
        }

        public override void GetSingleImage(CameraSummary summary)
        {
            if(snapshotting.IndexOf(summary.Identifier) >= 0)
                return;
            
            // Spawn a thread to get a snapshot.
            SnapshotRetriever retriever = new SnapshotRetriever(summary, deviceIds[summary.Identifier]);
            retriever.CameraThumbnailProduced += SnapshotRetriever_CameraThumbnailProduced;
            snapshotting.Add(summary.Identifier);
            ThreadPool.QueueUserWorkItem(retriever.Run);
        }
        
        public override CameraBlurb BlurbFromSummary(CameraSummary summary)
        {
            string specific = SpecificInfoSerialize(summary);
            CameraBlurb blurb = new CameraBlurb(CameraType, summary.Identifier, summary.Alias, summary.Icon, summary.DisplayRectangle, summary.AspectRatio.ToString(), summary.Rotation.ToString(), summary.Mirror, specific);
            return blurb;
        }
        
        public override ICaptureSource CreateCaptureSource(CameraSummary summary)
        {
            FrameGrabber grabber = new FrameGrabber(summary, deviceIds[summary.Identifier]);
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
                    info.StreamFormat = form.SelectedStreamFormat.Value;
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
                    info.CameraProperties.ContainsKey("width") &&
                    info.CameraProperties.ContainsKey("height") &&
                    info.CameraProperties.ContainsKey("framerate"))
                {
                    int format = info.StreamFormat;
                    int width = int.Parse(info.CameraProperties["width"].CurrentValue, CultureInfo.InvariantCulture);
                    int height = int.Parse(info.CameraProperties["height"].CurrentValue, CultureInfo.InvariantCulture);
                    double framerate = double.Parse(info.CameraProperties["framerate"].CurrentValue, CultureInfo.InvariantCulture);

                    uEye.Defines.ColorMode colorMode = (uEye.Defines.ColorMode)format;
                    
                    result = string.Format("{0} - {1}×{2} @ {3:0.##} fps ({4}).", alias, width, height, framerate, colorMode.ToString());
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
            XmlElement xmlRoot = doc.CreateElement("IDS");

            doc.AppendChild(xmlRoot);
            
            return doc.OuterXml;
        }
    }
}