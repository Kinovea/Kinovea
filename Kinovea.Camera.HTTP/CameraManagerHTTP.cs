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

using Kinovea.Camera;
using Kinovea.Services;
using Kinovea.Video;

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
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private Dictionary<string, CameraSummary> cache = new Dictionary<string, CameraSummary>();
        private List<string> snapshotting = new List<string>();
        private Bitmap defaultIcon;
        private string defaultAlias = "IP Camera";
        private string defaultName = "IP Camera";
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
        
        public override void GetSingleImage(CameraSummary summary)
        {
            if(snapshotting.IndexOf(summary.Identifier) >= 0)
                return;
            
            log.DebugFormat("Retrieve single image.");
            
            SnapshotRetriever retriever = new SnapshotRetriever(this, summary);
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
        
        public string BuildURL(SpecificInfo specific)
        {
            string url = "";
            if(string.IsNullOrEmpty(specific.User) && string.IsNullOrEmpty(specific.Password))
            {
                if(string.IsNullOrEmpty(specific.Port) || specific.Port == "80")
                    url = string.Format("http://{0}{1}", specific.Host, specific.Path);
                else
                    url = string.Format("http://{0}:{1}{2}", specific.Host, specific.Port, specific.Path);
            }
            else
            {
                if(string.IsNullOrEmpty(specific.Port) || specific.Port == "80")
                    url = string.Format("http://{0}:{1}@{2}{3}", specific.User, specific.Password, specific.Host, specific.Path);
                else
                    url = string.Format("http://{0}:{1}@{2}:{3}{4}", specific.User, specific.Password, specific.Host, specific.Port, specific.Path);
            }
            
            return url;
        }
        
        private void SnapshotRetriever_CameraThumbnailProduced(object sender, CameraThumbnailProducedEventArgs e)
        {
            SnapshotRetriever retriever = sender as SnapshotRetriever;
            if(retriever != null)
            {
                retriever.CameraThumbnailProduced-= SnapshotRetriever_CameraThumbnailProduced;
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