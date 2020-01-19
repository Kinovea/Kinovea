using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Kinovea.Services;
using Kinovea.Video;
using System.Windows.Forms;
using GxIAPINET;
using System.Xml;
using System.IO;
using System.Threading;

namespace Kinovea.Camera.Daheng
{
    public class CameraManagerDaheng : CameraManager
    {
        #region Properties
        public override bool Enabled
        {
            get { return true; }
        }

        public override string CameraType
        {
            get { return "E298083B-C0B7-40AF-9560-DA02B8C8753A"; }
        }
        public override string CameraTypeFriendlyName
        {
            get { return "Daheng"; }
        }
        public override bool HasConnectionWizard
        {
            get { return false; }
        }
        #endregion

        #region Members
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private Dictionary<string, CameraSummary> cache = new Dictionary<string, CameraSummary>();
        private List<string> snapshotting = new List<string>();
        private Bitmap defaultIcon;
        private int discoveryStep = 0;
        private int discoverySkip = 5;
        private IGXFactory igxFactory;
        private Dictionary<string, uint> deviceIndices = new Dictionary<string, uint>();
        #endregion

        public CameraManagerDaheng()
        {
            defaultIcon = IconLibrary.GetIcon("webcam");
        }

        #region Public methods - CameraManager implementation
        public override bool SanityCheck()
        {
            bool result = false;
            try
            {
                igxFactory = IGXFactory.GetInstance();
                igxFactory.Init();
                result = true;
            }
            catch
            {
                log.DebugFormat("Daheng Camera subsystem not available.");
            }

            return result;
        }

        public override List<CameraSummary> DiscoverCameras(IEnumerable<CameraBlurb> blurbs)
        {
            List<CameraSummary> summaries = new List<CameraSummary>();
            List<CameraSummary> found = new List<CameraSummary>();

            List<IGXDeviceInfo> devices = new List<IGXDeviceInfo>();
            igxFactory.UpdateDeviceList(200, devices);

            foreach (IGXDeviceInfo device in devices)
            {
                string identifier = device.GetSN();
                bool cached = cache.ContainsKey(identifier);

                if (cached)
                {
                    // We've already seen this camera in the current Kinovea session.
                    //deviceIds[identifier] = device.GetDeviceID();
                    summaries.Add(cache[identifier]);
                    found.Add(cache[identifier]);
                    continue;
                }

                string alias = device.GetDisplayName();
                Bitmap icon = null;
                SpecificInfo specific = new SpecificInfo();
                Rectangle displayRectangle = Rectangle.Empty;
                CaptureAspectRatio aspectRatio = CaptureAspectRatio.Auto;
                ImageRotation rotation = ImageRotation.Rotate0;
                //deviceIndices[identifier] = device.GetDeviceID();

                if (blurbs != null)
                {
                    foreach (CameraBlurb blurb in blurbs)
                    {
                        if (blurb.CameraType != this.CameraType || blurb.Identifier != identifier)
                            continue;

                        // We know this camera from a previous Kinovea session, restore the user custom values.
                        alias = blurb.Alias;
                        icon = blurb.Icon ?? defaultIcon;
                        displayRectangle = blurb.DisplayRectangle;
                        if (!string.IsNullOrEmpty(blurb.AspectRatio))
                            aspectRatio = (CaptureAspectRatio)Enum.Parse(typeof(CaptureAspectRatio), blurb.AspectRatio);
                        if (!string.IsNullOrEmpty(blurb.Rotation))
                            rotation = (ImageRotation)Enum.Parse(typeof(ImageRotation), blurb.Rotation);
                        specific = SpecificInfoDeserialize(blurb.Specific);
                        break;
                    }
                }

                icon = icon ?? defaultIcon;

                CameraSummary summary = new CameraSummary(alias, device.GetDisplayName(), identifier, icon, displayRectangle, aspectRatio, rotation, specific, this);

                summaries.Add(summary);
                found.Add(summary);
                cache.Add(identifier, summary);
            }

            List<CameraSummary> lost = new List<CameraSummary>();
            foreach (CameraSummary summary in cache.Values)
            {
                if (!found.Contains(summary))
                    lost.Add(summary);
            }

            foreach (CameraSummary summary in lost)
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
            if (snapshotting.IndexOf(summary.Identifier) >= 0)
                return;

            // Spawn a thread to get a snapshot.
            SnapshotRetriever retriever = new SnapshotRetriever(summary, igxFactory);
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
            FrameGrabber grabber = new FrameGrabber(summary, igxFactory);
            return grabber;
        }

        public override bool Configure(CameraSummary summary)
        {
            return false;
            //throw new NotImplementedException();
        }

        public override string GetSummaryAsText(CameraSummary summary)
        {
            string result = "";
            string alias = summary.Alias;
            result = string.Format("{0}", alias);

            return result;
        }

        public override Control GetConnectionWizard()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Private methods
        private void SnapshotRetriever_CameraThumbnailProduced(object sender, CameraThumbnailProducedEventArgs e)
        {
            SnapshotRetriever retriever = sender as SnapshotRetriever;
            if (retriever != null)
            {
                retriever.CameraThumbnailProduced -= SnapshotRetriever_CameraThumbnailProduced;
                if (snapshotting != null && snapshotting.Count > 0)
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

                Dictionary<string, CameraProperty> cameraProperties = new Dictionary<string, CameraProperty>();

                XmlNodeList props = doc.SelectNodes("/Daheng/CameraProperties/CameraProperty");
                foreach (XmlNode node in props)
                {
                    XmlAttribute keyAttribute = node.Attributes["key"];
                    if (keyAttribute == null)
                        continue;

                    string key = keyAttribute.Value;
                    CameraProperty property = new CameraProperty();

                    string xpath = string.Format("/Daheng/CameraProperties/CameraProperty[@key='{0}']", key);
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

                //info.StreamFormat = streamFormat;
                //info.Bayer8Conversion = bayer8Conversion;
                info.CameraProperties = cameraProperties;
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
            XmlElement xmlRoot = doc.CreateElement("Daheng");

            //XmlElement xmlStreamFormat = doc.CreateElement("StreamFormat");
            //xmlStreamFormat.InnerText = info.StreamFormat;
            //xmlRoot.AppendChild(xmlStreamFormat);

            //XmlElement xmlBayer8Conversion = doc.CreateElement("Bayer8Conversion");
            //xmlBayer8Conversion.InnerText = info.Bayer8Conversion.ToString().ToLower();
            //xmlRoot.AppendChild(xmlBayer8Conversion);

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
        #endregion

    }
}
