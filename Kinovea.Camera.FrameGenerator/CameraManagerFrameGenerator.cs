#region License
/*
Copyright © Joan Charmant 2014.
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

namespace Kinovea.Camera.FrameGenerator
{
    public class CameraManagerFrameGenerator : CameraManager
    {
        #region Properties
        public override bool Enabled
        {
            get { return true; }
        }
        public override string CameraType
        {
            get { return "904E2A6C-126D-45AF-BF08-6CE3925FF67E"; }
        }
        public override string CameraTypeFriendlyName
        {
            get { return "Camera simulator"; }
        }
        public override bool HasConnectionWizard
        {
            get { return true; }
        }
        #endregion

        #region Members
        private List<string> snapshotting = new List<string>();
        private Bitmap defaultIcon;
        private string defaultAlias = "Camera simulator";
        private string defaultName = "Camera simulator";
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public CameraManagerFrameGenerator()
        {
            defaultIcon = IconLibrary.GetIcon("robot");
        }

        public override bool SanityCheck()
        {
            return true;
        }

        public override List<CameraSummary> DiscoverCameras(IEnumerable<CameraBlurb> blurbs)
        {
            List<CameraSummary> summaries = new List<CameraSummary>();

            foreach (CameraBlurb blurb in blurbs)
            {
                if (blurb.CameraType != CameraType)
                    continue;

                string alias = blurb.Alias;
                string identifier = blurb.Identifier;
                Bitmap icon = blurb.Icon ?? defaultIcon;
                Rectangle displayRectangle = blurb.DisplayRectangle;
                CaptureAspectRatio aspectRatio = CaptureAspectRatio.Auto;
                if (!string.IsNullOrEmpty(blurb.AspectRatio))
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
            if (snapshotting.IndexOf(summary.Identifier) >= 0)
                return;

            // Spawn a thread to get a snapshot.
            SnapshotRetriever retriever = new SnapshotRetriever(summary);
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
            FrameGrabber grabber = new FrameGrabber(summary);
            return grabber;
        }

        public override bool Configure(CameraSummary summary)
        {
            bool needsReconnection = false;
            FormConfiguration form = new FormConfiguration(summary);
            if (form.ShowDialog() == DialogResult.OK)
            {
                if (form.AliasChanged)
                    summary.UpdateAlias(form.Alias, form.PickedIcon);

                if (form.SpecificChanged)
                {
                    SpecificInfo info = form.SpecificInfo;
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
            if (info != null)
                result = string.Format("{0} - {1}×{2} @ {3} fps ({4}).", alias, info.Width, info.Height, info.Framerate, info.ImageFormat);
            else
                result = string.Format("{0}", alias);

            return result;
        }

        public override Control GetConnectionWizard()
        {
            if (!HasConnectionWizard)
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
            SnapshotRetriever retriever = sender as SnapshotRetriever;
            if (retriever != null)
            {
                retriever.CameraThumbnailProduced -= SnapshotRetriever_CameraThumbnailProduced;
                snapshotting.Remove(retriever.Identifier);
            }

            OnCameraThumbnailProduced(e);
        }

        private SpecificInfo SpecificInfoDeserialize(string xml)
        {
            if (string.IsNullOrEmpty(xml))
                return null;

            SpecificInfo info = null;

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(new StringReader(xml));

                info = new SpecificInfo();
                
                XmlNode xmlImageFormat = doc.SelectSingleNode("/FrameGenerator/ImageFormat");
                if (xmlImageFormat != null)
                    info.ImageFormat = (ImageFormat)Enum.Parse(typeof(ImageFormat), xmlImageFormat.InnerText);
                
                info.Width = ParseInt(doc, "/FrameGenerator/Width", info.Width);
                info.Height = ParseInt(doc, "/FrameGenerator/Height", info.Height);
                info.Framerate = ParseInt(doc, "/FrameGenerator/Framerate", info.Framerate);

                if (info.ImageFormat == ImageFormat.JPEG)
                    info.Width -= (info.Width % 4);
            }
            catch (Exception e)
            {
                log.ErrorFormat(e.Message);
            }

            return info;
        }

        private int ParseInt(XmlDocument doc, string path, int defaultValue)
        {
            XmlNode xmlNode = doc.SelectSingleNode(path);
            if (xmlNode != null)
                return int.Parse(xmlNode.InnerText);
            else
                return defaultValue;
        }

        private string SpecificInfoSerialize(CameraSummary summary)
        {
            SpecificInfo info = summary.Specific as SpecificInfo;
            if (info == null)
                return null;

            XmlDocument doc = new XmlDocument();
            XmlElement xmlRoot = doc.CreateElement("FrameGenerator");

            XmlElement xmlImageFormat = doc.CreateElement("ImageFormat");
            xmlImageFormat.InnerText = info.ImageFormat.ToString();
            xmlRoot.AppendChild(xmlImageFormat);

            WriteInt(doc, xmlRoot, "Width", info.Width);
            WriteInt(doc, xmlRoot, "Height", info.Height);
            WriteInt(doc, xmlRoot, "Framerate", info.Framerate);

            doc.AppendChild(xmlRoot);
            return doc.OuterXml;
        }

        private void WriteInt(XmlDocument doc, XmlElement parent, string tag, int value)
        {
            XmlElement xmlNode = doc.CreateElement(tag);
            xmlNode.InnerText = value.ToString();
            parent.AppendChild(xmlNode);
        }
    }
}