using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Kinovea.Services;
using Kinovea.Video;
using System.Windows.Forms;
using BGAPI2;
using System.Xml;
using System.IO;
using System.Threading;
using System.Globalization;

namespace Kinovea.Camera.Baumer
{
    public class CameraManagerBaumer : CameraManager
    {
        #region Properties
        public override bool Enabled
        {
            get { return true; }
        }

        public override string CameraType
        {
            get { return "67cfc2e8-696b-4ed6-9caa-02baee8872bc"; }
        }
        public override string CameraTypeFriendlyName
        {
            get { return "Baumer"; }
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
        private BGAPI2.SystemList systemList;
        private Dictionary<string, BGAPI2.System> systems = new Dictionary<string, BGAPI2.System>();
        private Dictionary<string, uint> deviceIndices = new Dictionary<string, uint>();
        #endregion

        public CameraManagerBaumer()
        {
            defaultIcon = IconLibrary.GetIcon("webcam");
        }

        #region Public methods - CameraManager implementation
        public override bool SanityCheck()
        {
            bool result = false;
            try
            {
                systemList = SystemList.Instance;

                // Collect usable systems.
                // FIXME: This lists all systems and initializes them, including non Baumer GenAPI implementations.
                // This prevents other modules based on GenAPI from initializing properly.
                // We need a way to uninitialize the other systems without uninitializing the Baumer systems.
                systemList.Refresh();
                foreach (KeyValuePair<string, BGAPI2.System> systemPair in systemList)
                {
                    BGAPI2.System system = systemPair.Value;
                    if (!system.Vendor.Contains("Baumer"))
                    {
                        system.Close();
                        continue;
                    }

                    system.Open();
                    if (string.IsNullOrEmpty(system.Id))
                    {
                        system.Close();
                        continue;
                    }

                    systems.Add(systemPair.Key, system);
                }

                result = systems.Count > 0;
            }
            catch (Exception e)
            {
                log.DebugFormat("Baumer Camera subsystem not available.");
                log.ErrorFormat(e.Message);
            }

            return result;
        }

        public override List<CameraSummary> DiscoverCameras(IEnumerable<CameraBlurb> blurbs)
        {
            List<CameraSummary> summaries = new List<CameraSummary>();
            List<CameraSummary> found = new List<CameraSummary>();

            // Lifecycles of objects in the Baumer API.
            // systemList: entire application. Will initialize all systems, not clear how to uninitialize non Baumer systems.
            // system: entire application. Should be kept open.
            // interface & device: camera session.

            
            //systemList.Refresh();
            //log.DebugFormat("Baumer system list refresh. Looking for devices.");


            try
            {
                // Collect all the devices.
                //foreach (KeyValuePair<string, BGAPI2.System> systemPair in systemList)
                //foreach (BGAPI2.System system in systems)
                foreach (KeyValuePair<string, BGAPI2.System> systemPair in systems)
                {
                    BGAPI2.System system = systemPair.Value;
                    //if (!system.Vendor.Contains("Baumer"))
                      //  continue;

                    if (!system.IsOpen)
                        continue;

                    //system.Open();
                    //if (string.IsNullOrEmpty(system.Id))
                    //{
                    //    system.Close();
                    //    continue;
                    //}

                    system.Interfaces.Refresh(100);
                    foreach (KeyValuePair<string, BGAPI2.Interface> interfacePair in system.Interfaces)
                    {
                        BGAPI2.Interface iface = interfacePair.Value;
                        iface.Open();
                        if (string.IsNullOrEmpty(iface.Id))
                        {
                            iface.Close();
                            continue;
                        }

                        iface.Devices.Refresh(100);
                        foreach (KeyValuePair<string, BGAPI2.Device> devicePair in iface.Devices)
                        {
                            BGAPI2.Device device = devicePair.Value;
                            //log.DebugFormat("Found device: {0}", device.DisplayName);
                            string identifier = device.SerialNumber;
                            bool cached = cache.ContainsKey(identifier);
                            if (cached)
                            {
                                // We've already seen this camera in the current Kinovea session.
                                //deviceIds[identifier] = device.GetDeviceID();
                                summaries.Add(cache[identifier]);
                                found.Add(cache[identifier]);
                                continue;
                            }

                            string alias = device.DisplayName;
                            Bitmap icon = null;
                            SpecificInfo specific = new SpecificInfo();
                            Rectangle displayRectangle = Rectangle.Empty;
                            CaptureAspectRatio aspectRatio = CaptureAspectRatio.Auto;
                            ImageRotation rotation = ImageRotation.Rotate0;
                            //deviceIndices[identifier] = device.GetDeviceID();

                            // Check if we already know this camera from a previous Kinovea session.
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

                            // Keep temporary info in order to find it back later.
                            specific.DeviceKey = devicePair.Key;
                            specific.InterfaceKey = interfacePair.Key;
                            specific.SystemKey = systemPair.Key;

                            icon = icon ?? defaultIcon;
                            CameraSummary summary = new CameraSummary(alias, device.DisplayName, identifier, icon, displayRectangle, aspectRatio, rotation, specific, this);

                            summaries.Add(summary);
                            found.Add(summary);
                            cache.Add(identifier, summary);
                        }

                        iface.Close();
                    }

                    //system.Close();
                }
            }
            catch (Exception e)
            {

            }
            finally
            {
                //systemList.Clear();
                //log.DebugFormat("Baumer system list clear.");
            }


            List<CameraSummary> lost = new List<CameraSummary>();
            foreach (CameraSummary summary in cache.Values)
            {
                if (!found.Contains(summary))
                    lost.Add(summary);
            }

            foreach (CameraSummary summary in lost)
            {
                log.DebugFormat("Lost device: {0}", summary.Name);
                cache.Remove(summary.Identifier);
            }

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
            SnapshotRetriever retriever = new SnapshotRetriever(summary);
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
            FrameGrabber grabber = new FrameGrabber(summary);
            return grabber;
        }

        public override bool Configure(CameraSummary summary)
        {
            throw new NotImplementedException();
        }

        public override bool Configure(CameraSummary summary, Action disconnect, Action connect)
        {
            //bool needsReconnection = false;
            //SpecificInfo info = summary.Specific as SpecificInfo;
            //if (info == null)
            //    return false;

            //FormConfiguration form = new FormConfiguration(summary, disconnect, connect);
            //FormsHelper.Locate(form);
            //if (form.ShowDialog() == DialogResult.OK)
            //{
            //    if (form.AliasChanged)
            //        summary.UpdateAlias(form.Alias, form.PickedIcon);

            //    if (form.SpecificChanged)
            //    {
            //        info.StreamFormat = form.SelectedStreamFormat;
            //        info.CameraProperties = form.CameraProperties;

            //        summary.UpdateDisplayRectangle(Rectangle.Empty);
            //        needsReconnection = true;
            //    }

            //    CameraTypeManager.UpdatedCameraSummary(summary);
            //}

            //form.Dispose();
            //return needsReconnection;
            return false;
        }

        public override string GetSummaryAsText(CameraSummary summary)
        {
            string result = "";
            string alias = summary.Alias;
            SpecificInfo info = summary.Specific as SpecificInfo;

            

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

                XmlNodeList props = doc.SelectNodes("/Baumer/CameraProperties/CameraProperty");
                foreach (XmlNode node in props)
                {
                    XmlAttribute keyAttribute = node.Attributes["key"];
                    if (keyAttribute == null)
                        continue;

                    string key = keyAttribute.Value;
                    CameraProperty property = new CameraProperty();

                    string xpath = string.Format("/Baumer/CameraProperties/CameraProperty[@key='{0}']", key);
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
            XmlElement xmlRoot = doc.CreateElement("Baumer");

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
