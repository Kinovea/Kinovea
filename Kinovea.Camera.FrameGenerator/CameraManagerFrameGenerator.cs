#region License
/*
Copyright © Joan Charmant 2014.
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

using Kinovea.Camera;
using Kinovea.Services;

namespace Kinovea.Camera.FrameGenerator
{
    /// <summary>
    /// Non-functionnal.
    /// Needs to use the new capture pipeline where the frame grabber is expected to output a frame buffer directly,
    /// rather than a complete bitmap.
    /// </summary>
    public class CameraManagerFrameGenerator : CameraManager
    {
        #region Properties
        public override bool Enabled
        {
            get { return false; }
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
            defaultIcon = IconLibrary.GetIcon("dashboard");
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
                object specific = SpecificInfoDeserialize(blurb.Specific);

                CameraSummary summary = new CameraSummary(alias, defaultName, identifier, icon, displayRectangle, aspectRatio, specific, this);
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

            SnapshotRetriever retriever = new SnapshotRetriever(this, summary);
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
                    SpecificInfo info = new SpecificInfo();
                    info.SelectedFrameRate = form.Framerate;
                    info.SelectedFrameSize = form.FrameSize;
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
            string result = string.Format("{0}", summary.Alias);
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
            return new CameraSummary(defaultAlias, defaultName, id, defaultIcon, Rectangle.Empty, CaptureAspectRatio.Auto, null, this);
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

                int frameRate = 0;
                XmlNode xmlFrameRate = doc.SelectSingleNode("/FrameGenerator/SelectedFrameRate");
                if (xmlFrameRate != null)
                {
                    string strFrameRate = xmlFrameRate.InnerText;
                    frameRate = int.Parse(strFrameRate, CultureInfo.InvariantCulture);
                }
                info.SelectedFrameRate = frameRate;

                Size frameSize = Size.Empty;
                XmlNode xmlFrameSize = doc.SelectSingleNode("/FrameGenerator/SelectedFrameSize");
                if (xmlFrameSize != null)
                {
                    string strFrameSize = xmlFrameSize.InnerText;
                    frameSize = XmlHelper.ParseSize(strFrameSize);
                }
                info.SelectedFrameSize = frameSize;
            }
            catch (Exception e)
            {
                log.ErrorFormat(e.Message);
            }

            return info;
        }

        private string SpecificInfoSerialize(CameraSummary summary)
        {
            SpecificInfo info = summary.Specific as SpecificInfo;
            if (info == null)
                return null;

            XmlDocument doc = new XmlDocument();
            XmlElement xmlRoot = doc.CreateElement("FrameGenerator");

            XmlElement xmlFrameRate = doc.CreateElement("SelectedFrameRate");
            string framerate = string.Format("{0}", info.SelectedFrameRate);
            xmlFrameRate.InnerText = framerate;
            xmlRoot.AppendChild(xmlFrameRate);

            XmlElement xmlFrameSize = doc.CreateElement("SelectedFrameSize");
            string frameSize = string.Format("{0};{1}", info.SelectedFrameSize.Width, info.SelectedFrameSize.Height);
            xmlFrameSize.InnerText = frameSize;
            xmlRoot.AppendChild(xmlFrameSize);

            doc.AppendChild(xmlRoot);

            return doc.OuterXml;
        }
    }
}