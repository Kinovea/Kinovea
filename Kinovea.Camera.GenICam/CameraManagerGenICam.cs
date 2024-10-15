using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Threading;
using System.Globalization;
using Kinovea.Services;
using BGAPI2;
using System.Diagnostics;

namespace Kinovea.Camera.GenICam
{
    /// <summary>
    /// The main camera manager for GenICam compliant cameras.
    /// Currently based on the Baumer SDK.
    /// 
    /// Camera discovery, instanciating snapshotter and framegrabber.
    /// </summary>
    public class CameraManagerGenICam : CameraManager
    {
        #region Properties
        public override bool Enabled
        {
            get { return true; }
        }

        public override string CameraType
        {
            get { return "AF3C7759-6B92-4BF7-9954-57204D76B170"; }
        }
        public override string CameraTypeFriendlyName
        {
            get { return "GenICam"; }
        }
        public override bool HasConnectionWizard
        {
            get { return false; }
        }
        #endregion

        #region Members
        private Bitmap defaultIcon;
        // List of currently active snapshot retrievers, to avoid starting one on the same camera.
        private List<SnapshotRetriever> snapshotting = new List<SnapshotRetriever>();
        // Cache of discovered devices.
        private Dictionary<string, CameraSummary> cache = new Dictionary<string, CameraSummary>();
        // Cache of known interfaces, to speed up discovery we don't search systems and interfaces each time.
        private Dictionary<string, Interface> interfaces = new Dictionary<string, Interface>();
        private List<Interface> blackListInterfaces = new List<Interface>();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Public methods - CameraManager implementation
        
        /// <summary>
        /// Constructor. This is called dynamically from introspection.
        /// </summary>
        public CameraManagerGenICam()
        {
            defaultIcon = IconLibrary.GetIcon("baumer");
        }

        public override bool SanityCheck()
        {
            bool result = false;
            try
            {
                // This should trigger the loading of bgapi2_genicam_dotnet.dll assembly and the native dependencies.
                var systemList = SystemList.Instance;
                result = true;
            }
            catch (Exception e)
            {
                log.DebugFormat("GenICam Camera subsystem not available.");
                log.ErrorFormat(e.Message);
            }

            return result;
        }

        public override List<CameraSummary> DiscoverCameras(IEnumerable<CameraBlurb> blurbs)
        {
            bool verbose = false;

            // We only check the list of systems and interfaces once at application startup.
            // These are found from the .cti files (DLLs) loaded by the Baumer API.
            // They are searched for in the plugin directory and the GENICAM_GENTL64_PATH env variable. 
            if (interfaces.Count == 0)
            {
                RefreshInterfaces();
            }

            List<CameraSummary> summaries = new List<CameraSummary>();
            if (interfaces.Count == 0)
                return summaries;

            // Search for devices on the known interfaces.
            // Unfortunately the same interfaces are seen differently by each vendor
            // so we can't skip the discovery of devices on interfaces we know are already
            // used by a device from a different vendor.
            Stopwatch stopwatch = Stopwatch.StartNew();
            foreach (KeyValuePair<string, Interface> interfacePair in interfaces)
            {
                try
                {
                    stopwatch.Restart();
                    if (blackListInterfaces.Contains(interfacePair.Value))
                        continue;

                    var interf = interfacePair.Value;
                    if (string.IsNullOrEmpty(interf.Id))
                        continue;

                    if (!interf.IsOpen)
                        interf.Open();
                        
                    if (!interf.IsOpen)
                        continue;

                    // Search for devices.
                    interf.Devices.Refresh(200);

                    if (verbose)
                    {
                        log.DebugFormat("Listing devices for {0} > {1}: {2} ms", interf.Parent.Vendor, interf.DisplayName, stopwatch.ElapsedMilliseconds);
                    }
                    
                    foreach (KeyValuePair<string, Device> devicePair in interf.Devices)
                    {
                        var device = devicePair.Value;
                        if (verbose) 
                        {
                            log.DebugFormat("Found device: {0} ({1})", device.DisplayName, device.Vendor);
                        }
                            
                        // We would like to only load the device through the right vendor system.
                        // The problem is that the vendor name at the device level is not always the
                        // same as the vendor name at the system level.
                        if (device.Vendor != interf.Parent.Vendor)
                        {
                            log.WarnFormat("Device vendor:{0}, System vendor:{1}", device.Vendor, interf.Parent.Vendor);
                        }

                        string identifier = device.SerialNumber;
                        bool cached = cache.ContainsKey(identifier);
                        if (cached)
                        {
                            // We've already seen this camera in the current Kinovea session.
                            summaries.Add(cache[identifier]);
                            continue;
                        }

                        log.DebugFormat("Found new device.");

                        string alias = device.DisplayName;
                        Bitmap icon = null;
                        SpecificInfo specific = new SpecificInfo();
                        Rectangle displayRectangle = Rectangle.Empty;
                        CaptureAspectRatio aspectRatio = CaptureAspectRatio.Auto;
                        ImageRotation rotation = ImageRotation.Rotate0;
                        bool mirror = false;

                        // Check if we already know this camera from a previous Kinovea session.
                        if (blurbs != null)
                        {
                            foreach (CameraBlurb blurb in blurbs)
                            {
                                if (blurb.CameraType != this.CameraType || blurb.Identifier != identifier)
                                    continue;

                                // We know this camera from a previous Kinovea session, restore the user custom values.
                                log.DebugFormat("Recognized device from previous session, importing data.");
                                alias = blurb.Alias;
                                icon = blurb.Icon ?? defaultIcon;
                                displayRectangle = blurb.DisplayRectangle;
                                if (!string.IsNullOrEmpty(blurb.AspectRatio))
                                    aspectRatio = (CaptureAspectRatio)Enum.Parse(typeof(CaptureAspectRatio), blurb.AspectRatio);
                                if (!string.IsNullOrEmpty(blurb.Rotation))
                                    rotation = (ImageRotation)Enum.Parse(typeof(ImageRotation), blurb.Rotation);
                                mirror = blurb.Mirror;
                                specific = SpecificInfoDeserialize(blurb.Specific);
                                break;
                            }
                        }

                        specific.Device = device;

                        icon = icon ?? defaultIcon;
                        CameraSummary summary = new CameraSummary(
                            alias, 
                            device.DisplayName, 
                            identifier, 
                            icon, 
                            displayRectangle, 
                            aspectRatio, 
                            rotation, 
                            mirror, 
                            specific, 
                            this);

                        summaries.Add(summary);
                        cache.Add(identifier, summary);
                        log.DebugFormat("Added new device to cache: {0}", stopwatch.ElapsedMilliseconds);
                    }
                }
                catch (BGAPI2.Exceptions.ErrorException e)
                {
                    log.ErrorFormat("ErrorException while scanning for devices on interface: {0}", interfacePair.Value.DisplayName);
                    log.ErrorFormat("Description: {0}", e.GetErrorDescription());
                    log.ErrorFormat("Function name: {0}", e.GetFunctionName());

                    blackListInterfaces.Add(interfacePair.Value);
                }
                catch (BGAPI2.Exceptions.LowLevelException e)
                {
                    log.ErrorFormat("LowLevelException while scanning for devices on interface: {0}", interfacePair.Value.DisplayName);
                    log.ErrorFormat("Description: {0}", e.GetErrorDescription());
                    log.ErrorFormat("Function name: {0}", e.GetFunctionName());

                    blackListInterfaces.Add(interfacePair.Value);
                }
                catch (Exception e)
                {
                    log.ErrorFormat("Exception while scanning for devices on interface: {0}", interfacePair.Value.DisplayName);
                    log.ErrorFormat("Description: {0}", e.Message);

                    blackListInterfaces.Add(interfacePair.Value);
                }
            }

            List<CameraSummary> lost = new List<CameraSummary>();
            foreach (CameraSummary summary in cache.Values)
            {
                if (!summaries.Contains(summary))
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

        public override CameraSummary GetCameraSummary(string alias)
        {
            return cache.Values.FirstOrDefault(s => s.Alias == alias);
        }

        public override void StartThumbnail(CameraSummary summary)
        {
            SnapshotRetriever snapper = snapshotting.FirstOrDefault(s => s.Identifier == summary.Identifier);
            if (snapper != null)
                return;

            snapper = new SnapshotRetriever(summary);
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
            return new FrameGrabber(summary);
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
                if (form.AliasChanged)
                    summary.UpdateAlias(form.Alias, form.PickedIcon);

                if (form.SpecificChanged)
                {
                    info.StreamFormat = form.SelectedStreamFormat;
                    info.Demosaicing = form.Demosaicing;
                    info.Compression = form.Compression;
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
            result = string.Format("{0}", alias);
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
                    double framerate = CameraPropertyManager.GetResultingFramerate(info.Device);
                    if (framerate == 0)
                        framerate = double.Parse(info.CameraProperties["framerate"].CurrentValue, CultureInfo.InvariantCulture);

                    result = string.Format("{0} - {1}×{2} @ {3:0.##} fps ({4}).", alias, width, height, framerate, format);
                }
            }
            catch
            {
            }

            return result;
        }

        public override Control GetConnectionWizard()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Private methods

        /// <summary>
        /// Search for all GenICam systems and interfaces.
        /// This is fairly long so we only do it once on the first discover step.
        /// </summary>
        private void RefreshInterfaces()
        {
            log.DebugFormat("Searching GenICam systems and interfaces.");
            var systemList = SystemList.Instance;
            systemList.Refresh();
            foreach (KeyValuePair<string, BGAPI2.System> systemPair in systemList)
            {
                BGAPI2.System system = systemPair.Value;
                if (string.IsNullOrEmpty(system.Id))
                    continue;

                try
                {
                    if (!system.IsOpen)
                        system.Open();

                    if (!system.IsOpen)
                    {
                        log.DebugFormat("Could not open system {0} ({1}).", system.DisplayName, system.Vendor);
                        continue;
                    }

                    log.DebugFormat("Opened system {0}: {1} (Vendor={2}, TLType={3}, Version={4}).", 
                        system.FileName, system.DisplayName, system.Vendor, system.TLType, system.Version);
                    //systems.Add(systemPair.Key, system);

                    system.Interfaces.Refresh(200);
                    int foundInterfaces = 0;
                    foreach (KeyValuePair<string, Interface> interfacePair in system.Interfaces)
                    {
                        var interf = interfacePair.Value;
                        if (string.IsNullOrEmpty(interf.Id))
                            continue;

                        try
                        {
                            if (!interf.IsOpen)
                                interf.Open();

                            log.DebugFormat("\t\tOpened interface {1}.", system.FileName, interf.DisplayName);
                            string key = string.Format("{0}/{1}", system.FileName, interfacePair.Key);
                            interfaces.Add(key, interf);
                            foundInterfaces++;
                        }
                        catch (BGAPI2.Exceptions.ErrorException e)
                        {
                            log.ErrorFormat("ErrorException while opening interface: {0}", interfacePair.Value.DisplayName);
                            log.ErrorFormat("Description: {0}", e.GetErrorDescription());
                            log.ErrorFormat("Function name: {0}", e.GetFunctionName());

                            blackListInterfaces.Add(interfacePair.Value);
                        }
                        catch (BGAPI2.Exceptions.LowLevelException e)
                        {
                            log.ErrorFormat("LowLevelException while scanning for devices on interface: {0}", interfacePair.Value.DisplayName);
                            log.ErrorFormat("Description: {0}", e.GetErrorDescription());
                            log.ErrorFormat("Function name: {0}", e.GetFunctionName());

                            blackListInterfaces.Add(interfacePair.Value);
                        }
                        catch (Exception e)
                        {
                            log.ErrorFormat("Exception while scanning for devices on interface: {0}", interfacePair.Value.DisplayName);
                            log.ErrorFormat("Description: {0}", e.Message);

                            blackListInterfaces.Add(interfacePair.Value);
                        }

                    }

                    if (foundInterfaces == 0)
                    {
                        system.Close();
                    }
                }
                catch (BGAPI2.Exceptions.ErrorException e)
                {
                    log.ErrorFormat("Error while listing interfaces on GenICam system {0}", system.DisplayName);
                    log.ErrorFormat("Description: {0}", e.GetErrorDescription());
                    log.ErrorFormat("Function name: {0}", e.GetFunctionName());
                }
                catch (BGAPI2.Exceptions.LowLevelException e)
                {
                    log.ErrorFormat("LowLevelException while listing interfaces on GenICam system: {0}", system.DisplayName);
                    log.ErrorFormat("Description: {0}", e.GetErrorDescription());
                    log.ErrorFormat("Function name: {0}", e.GetFunctionName());
                }
                catch (Exception e)
                {
                    log.ErrorFormat("Exception while listing interfaces on GenICam system: {0}", system.DisplayName);
                    log.ErrorFormat("Description: {0}", e.Message);
                }
            }
        }

        private void SnapshotRetriever_CameraThumbnailProduced(object sender, CameraThumbnailProducedEventArgs e)
        {
            Invoke((Action)delegate { ProcessThumbnail(sender, e); });
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
            if (string.IsNullOrEmpty(xml))
                return null;

            SpecificInfo info = null;

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(new StringReader(xml));

                info = new SpecificInfo();

                //------------------------
                // Non-generic properties.
                //------------------------
                string streamFormat = "";
                XmlNode xmlStreamFormat = doc.SelectSingleNode("/GenICam/StreamFormat");
                if (xmlStreamFormat != null)
                    streamFormat = xmlStreamFormat.InnerText;

                bool demosaicing = false;
                XmlNode xmlDemosaicing = doc.SelectSingleNode("/GenICam/Demosaicing");
                if (xmlDemosaicing != null)
                    demosaicing = XmlHelper.ParseBoolean(xmlDemosaicing.InnerText);

                bool compression = false;
                XmlNode xmlCompression = doc.SelectSingleNode("/GenICam/Compression");
                if (xmlCompression != null)
                    compression = XmlHelper.ParseBoolean(xmlCompression.InnerText);

                info.StreamFormat = streamFormat;
                info.Demosaicing = demosaicing;
                info.Compression = compression;

                //------------------------
                // Generic properties.
                //------------------------
                Dictionary<string, CameraProperty> cameraProperties = new Dictionary<string, CameraProperty>();

                XmlNodeList props = doc.SelectNodes("/GenICam/CameraProperties/CameraProperty");
                foreach (XmlNode node in props)
                {
                    XmlAttribute keyAttribute = node.Attributes["key"];
                    if (keyAttribute == null)
                        continue;

                    string key = keyAttribute.Value;
                    CameraProperty property = new CameraProperty();

                    string xpath = string.Format("/GenICam/CameraProperties/CameraProperty[@key='{0}']", key);
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
            XmlElement xmlRoot = doc.CreateElement("GenICam");

            //------------------------
            // Non-generic properties.
            //------------------------
            XmlElement xmlStreamFormat = doc.CreateElement("StreamFormat");
            xmlStreamFormat.InnerText = info.StreamFormat;
            xmlRoot.AppendChild(xmlStreamFormat);

            XmlElement xmlDemosaicing = doc.CreateElement("Demosaicing");
            xmlDemosaicing.InnerText = info.Demosaicing.ToString().ToLowerInvariant();
            xmlRoot.AppendChild(xmlDemosaicing);

            XmlElement xmlCompression = doc.CreateElement("Compression");
            xmlCompression.InnerText = info.Compression.ToString().ToLowerInvariant();
            xmlRoot.AppendChild(xmlCompression);

            //------------------------
            // Generic properties.
            //------------------------
            XmlElement xmlCameraProperties = doc.CreateElement("CameraProperties");
            foreach (KeyValuePair<string, CameraProperty> pair in info.CameraProperties)
            {
                if (pair.Value == null)
                    continue;

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
