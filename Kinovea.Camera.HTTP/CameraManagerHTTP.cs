#region License
/*
Copyright © Joan Charmant 2013.
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
using System.Linq;
using Kinovea.Services;

namespace Kinovea.Camera.HTTP
{
    /// <summary>
    /// Class to discover and manage cameras connected through HTTP.
    /// </summary>
    public class CameraManagerHTTP : CameraManager
    {
        #region Properties
        public override bool Enabled
        {
            get { return true; }
        }
        public override string CameraType 
        { 
            get { return "0F8CF704-97FC-11E2-9919-09C611C84021";}
        }
        public override string CameraTypeFriendlyName 
        { 
            get { return "IP Camera"; }
        }
        public override bool HasConnectionWizard
        {
            get { return true;}
        }
        #endregion
    
        #region Members
        private Dictionary<string, CameraSummary> cache = new Dictionary<string, CameraSummary>();
        private List<SnapshotRetriever> snapshotting = new List<SnapshotRetriever>();
        private Bitmap defaultIcon;
        private string defaultAlias = "IP Camera";
        private string defaultName = "IP Camera";
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public CameraManagerHTTP()
        {
            defaultIcon = IconLibrary.GetIcon("network");
        }

        public override bool SanityCheck()
        {
            return true;
        }
        
        public override List<CameraSummary> DiscoverCameras(IEnumerable<CameraBlurb> blurbs)
        {
            List<CameraSummary> summaries = new List<CameraSummary>();
            
            foreach(CameraBlurb blurb in blurbs)
            {
                if(blurb.CameraType != CameraType)
                    continue;
                    
                string alias = blurb.Alias;
                string identifier = blurb.Identifier;
                Bitmap icon = blurb.Icon ?? defaultIcon;
                Rectangle displayRectangle = blurb.DisplayRectangle;
                CaptureAspectRatio aspectRatio = CaptureAspectRatio.Auto;
                if(!string.IsNullOrEmpty(blurb.AspectRatio))
                    aspectRatio = (CaptureAspectRatio)Enum.Parse(typeof(CaptureAspectRatio), blurb.AspectRatio);
                ImageRotation rotation = ImageRotation.Rotate0;
                if (!string.IsNullOrEmpty(blurb.Rotation))
                    rotation = (ImageRotation)Enum.Parse(typeof(ImageRotation), blurb.Rotation);
                bool mirror = blurb.Mirror;
                object specific = SpecificInfoDeserialize(blurb.Specific);

                CameraSummary summary = new CameraSummary(alias, defaultName, identifier, icon, displayRectangle, aspectRatio, rotation, mirror, specific, this);
                summaries.Add(summary);
            }
            
            return summaries;
        }

        public override void ForgetCamera(CameraSummary summary)
        {
        }
        
        public override void StartThumbnail(CameraSummary summary)
        {
            SnapshotRetriever snapper = snapshotting.FirstOrDefault(s => s.Identifier == summary.Identifier);
            if (snapper != null)
                return;

            snapper = new SnapshotRetriever(this, summary);
            snapper.CameraThumbnailProduced += SnapshotRetriever_CameraThumbnailProduced;
            snapshotting.Add(snapper);
            snapper.Start();
        }

        public override void StopAllThumbnails()
        {
            for (int i = snapshotting.Count - 1; i >= 0; i--)
            {
                SnapshotRetriever snapper = snapshotting[i];
                snapper.Cancel();
                snapper.Thread.Join(500);
                if (snapper.Thread.IsAlive)
                    snapper.Thread.Abort();

                snapper.CameraThumbnailProduced -= SnapshotRetriever_CameraThumbnailProduced;
                snapshotting.RemoveAt(i);
            }
        }

        public override CameraBlurb BlurbFromSummary(CameraSummary summary)
        {
            string specific = SpecificInfoSerialize(summary);
            CameraBlurb blurb = new CameraBlurb(CameraType, summary.Identifier, summary.Alias, summary.Icon, summary.DisplayRectangle, summary.AspectRatio.ToString(), summary.Rotation.ToString(), summary.Mirror, specific);
            return blurb;
        }
        
        public override ICaptureSource CreateCaptureSource(CameraSummary summary)
        {
            FrameGrabber grabber = new FrameGrabber(summary, this);
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
                    summary.UpdateSpecific(form.Specific);
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
            string result = string.Format("{0}", summary.Alias);
            return result;
        }
        
        public override Control GetConnectionWizard()
        {
            if(!HasConnectionWizard)
                return null;
            
            ConnectionWizard control = new ConnectionWizard(this);
            return control;
        }
        
        public CameraSummary GetDefaultCameraSummary(string id)
        {
            return new CameraSummary(defaultAlias, defaultName, id, defaultIcon, this);
        }
        
        private void SnapshotRetriever_CameraThumbnailProduced(object sender, CameraThumbnailProducedEventArgs e)
        {
            Invoke((Action) delegate { ProcessThumbnail(sender, e); });
        }

        private void ProcessThumbnail(object sender, CameraThumbnailProducedEventArgs e)
        {
            SnapshotRetriever snapper = sender as SnapshotRetriever;
            if (snapper == null)
                return;

            log.DebugFormat("Received thumbnail event for {0}. Cancelled: {1}.", snapper.Alias, e.Cancelled);
            snapper.CameraThumbnailProduced -= SnapshotRetriever_CameraThumbnailProduced;
            if (snapshotting.Contains(snapper))
                snapshotting.Remove(snapper);

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
                info.User = ReadXML(doc, "/HTTP/User");
                info.Password = ReadXML(doc, "/HTTP/Password");
                info.Host = ReadXML(doc, "/HTTP/Host");
                info.Port = ReadXML(doc, "/HTTP/Port");
                info.Path = ReadXML(doc, "/HTTP/Path");
                info.Format = ReadXML(doc, "/HTTP/Format");
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
            XmlElement xmlRoot = doc.CreateElement("HTTP");
            
            AppendXML(doc, xmlRoot, "User", info.User);
            AppendXML(doc, xmlRoot, "Password", info.Password);
            AppendXML(doc, xmlRoot, "Host", info.Host);
            AppendXML(doc, xmlRoot, "Port", info.Port);
            AppendXML(doc, xmlRoot, "Path", info.Path);
            AppendXML(doc, xmlRoot, "Format", info.Format);

            doc.AppendChild(xmlRoot);
            return doc.OuterXml;
        }
        
        private void AppendXML(XmlDocument doc, XmlElement parent, string elementName, string elementValue)
        {
            XmlElement xml = doc.CreateElement(elementName);
            xml.InnerText = elementValue;
            parent.AppendChild(xml);
        }
        
        private string ReadXML(XmlDocument doc, string xpath)
        {
            string result = "";
            XmlNode xml = doc.SelectSingleNode(xpath);
            if(xml != null)
                result = xml.InnerText;
                
            return result;
        }
    }
}