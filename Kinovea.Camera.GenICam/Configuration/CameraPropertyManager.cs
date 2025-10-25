using System;
using System.Collections.Generic;
using System.Globalization;
using BGAPI2;
using Kinovea.Services;
using static BGAPI2.Node;

namespace Kinovea.Camera.GenICam
{
    /// <summary>
    /// Reads and writes a list of supported camera properties from/to the device.
    /// </summary>
    public static class CameraPropertyManager
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void AfterOpen(Device device)
        {
            if (device == null || !device.IsOpen)
                return;

            if (device.Vendor == "Daheng Imaging")
            {
                WriteEnum(device, "AcquisitionMode", "Continuous");
                WriteEnum(device, "TriggerMode", "Off");

                // Force white balance at least once.
                ContinuousOrOnce(device, "BalanceWhiteAuto");

                // Make the camera send the max bandwidth it can, possibly saturating the link.
                WriteEnum(device, "DeviceLinkThroughputLimitMode", "Off");
            }
        }
        #region Read high level

        /// <summary>
        /// Read all supported properties and return them.
        /// </summary>
        public static Dictionary<string, CameraProperty> ReadAll(Device device, string fullName)
        {
            Dictionary<string, CameraProperty> properties = new Dictionary<string, CameraProperty>();

            if (device == null || !device.IsOpen)
                return properties;

            try
            {
                // Camera properties in Kinovea combine the value and the "auto" flag.
                // We potentially need to read several GenICam properties to create one Kinovea camera property.
                ReadSize(device, properties);
                ReadFramerate(device, properties);
                ReadExposure(device, properties);
                ReadGain(device, properties);
                ReadCompressionQuality(device, properties);
                ReadDeviceClock(device, properties);
            }
            catch (Exception e)
            {
                log.ErrorFormat("Error while reading GenICam camera properties. {0}.", e.Message);
            }

            return properties;
        }

        /// <summary>
        /// Read a single property and return it.
        /// This is used in the context of dependent properties, to update the master list with new values.
        /// </summary>
        public static void Reload(Device device, Dictionary<string, CameraProperty> properties, string key)
        {
            if (key == "framerate")
                ReadFramerate(device, properties);
            else if (key == "exposure")
                ReadExposure(device, properties);
        }

        private static void ReadSize(Device device, Dictionary<string, CameraProperty> properties)
        {
            properties.Add("width", ReadIntegerProperty(device, "Width", "WidthMax"));
            properties.Add("height", ReadIntegerProperty(device, "Height", "HeightMax"));
        }
        
        /// <summary>
        /// Read the exposure property and add it to the dictionary.
        /// </summary>
        private static void ReadExposure(Device device, Dictionary<string, CameraProperty> properties)
        {
            string key = "exposure";

            CameraProperty p = ReadFloatProperty(device, "ExposureTime");
            if (!p.Supported)
                p = ReadFloatProperty(device, "ExposureTimeAbs");

            string autoIdentifier = "ExposureAuto";
            p.AutomaticIdentifier = autoIdentifier;
            p.CanBeAutomatic = false;
            p.Automatic = false;
            bool autoReadable = NodeIsReadable(device, p.AutomaticIdentifier);
            if (autoReadable)
            {
                p.CanBeAutomatic = true;
                string autoValue = ReadString(device, p.AutomaticIdentifier);
                p.Automatic = autoValue == GetAutoTrue(autoIdentifier);
            }

            if (properties != null)
            {
                if (properties.ContainsKey(key))
                    properties[key] = p;
                else
                    properties.Add(key, p);
            }
        }

        /// <summary>
        /// Read the gain property and add it to the dictionary.
        /// </summary>
        private static void ReadGain(Device device, Dictionary<string, CameraProperty> properties)
        {
            string key = "gain";

            CameraProperty p = ReadFloatProperty(device, "Gain");
            if (!p.Supported)
                p = ReadIntegerProperty(device, "GainRaw", null);

            string autoIdentifier = "GainAuto";
            p.AutomaticIdentifier = autoIdentifier;
            p.CanBeAutomatic = false;
            p.Automatic = false;
            bool autoReadable = NodeIsReadable(device, p.AutomaticIdentifier);
            if (autoReadable)
            {
                p.CanBeAutomatic = true;
                string autoValue = ReadString(device, p.AutomaticIdentifier);
                p.Automatic = autoValue == GetAutoTrue(autoIdentifier);
            }

            properties.Add(key, p);
        }

        private static void ReadCompressionQuality(Device device, Dictionary<string, CameraProperty> properties)
        {
            // This property is found on Baumer cameras that support hardware JPEG compression.
            if (device.Vendor != "Baumer")
                return;

            string key = "compressionQuality";

            CameraProperty p = ReadIntegerProperty(device, "ImageCompressionQuality", "");

            p.AutomaticIdentifier = "";
            p.CanBeAutomatic = false;
            p.Automatic = false;

            properties.Add(key, p);
        }
        #endregion

        #region Frame rate & clock
        /// <summary>
        /// Read the frame rate property and add it to the dictionary.
        /// </summary>
        private static void ReadFramerate(Device device, Dictionary<string, CameraProperty> properties)
        {
            // Verified working on: Basler, Baumer.

            string key = "framerate";

            CameraProperty p = new CameraProperty();
            p.Identifier = "AcquisitionFrameRate";
            p.Supported = false;
            p.Type = CameraPropertyType.Float;

            bool present = device.RemoteNodeList.GetNodePresent(p.Identifier);
            if (!present)
            {
                p.Identifier = "AcquisitionFrameRateAbs";
                present = device.RemoteNodeList.GetNodePresent(p.Identifier);
                if (!present)
                    return;
            }

            Node node = device.RemoteNodeList[p.Identifier];
            if (!node.IsImplemented || !node.IsAvailable || !node.IsReadable)
                return;

            p.ReadOnly = false;
            p.Supported = true;

            double currentValue = node.Value;
            double min = node.Min;
            double max = node.Max;
            double step = 1.0;
            min = Math.Max(1.0, min);

            p.Minimum = min.ToString(CultureInfo.InvariantCulture);
            p.Maximum = max.ToString(CultureInfo.InvariantCulture);
            p.CurrentValue = currentValue.ToString(CultureInfo.InvariantCulture);
            p.Step = step.ToString(CultureInfo.InvariantCulture);

            // Fix values that should be log.
            bool highRange = (Math.Log10(max) - Math.Log10(min)) >= 4.0;
            p.Representation = highRange ? CameraPropertyRepresentation.LogarithmicSlider : CameraPropertyRepresentation.LinearSlider;

            // Auto flag.
            // Basler & Baumer use a boolean property.
            // AcquisitionFrameRateEnable=false: the framerate is automatically set to the max value possible (auto = true).
            // AcquisitionFrameRateEnable=true: use the custom framerate set by the user in AcquisitionFrameRate (auto = false).
            // Daheng uses a string property:
            // AcquisitionFrameRateMode="Off": the framerate is automatically set to the max value possible.
            // AcquisitionFrameRateMode="On": use the custom framerate set by the user in AcquisitionFrameRate.

            string autoIdentifier = "AcquisitionFrameRateEnable";
            if (device.Vendor.Contains("Daheng"))
            {
                autoIdentifier = "AcquisitionFrameRateMode";
            }

            p.AutomaticIdentifier = autoIdentifier;
            p.CanBeAutomatic = false;
            p.Automatic = false;
            Node autoNode = null;
            bool autoPresent = device.RemoteNodeList.GetNodePresent(p.AutomaticIdentifier);
            if (autoPresent)
            {
                autoNode = device.RemoteNodeList[p.AutomaticIdentifier];
                p.CanBeAutomatic = autoNode.IsWriteable;
            }

            if (p.CanBeAutomatic)
            {
                string currentAutoValue = autoNode.Value;
                if (device.Vendor == "Baumer" || device.Vendor == "Basler")
                {
                    currentAutoValue = BoolValueToString(autoNode.Value);
                }
                 
                p.Automatic = currentAutoValue == GetAutoTrue(autoIdentifier);
            }

            if (properties != null)
            {
                if (properties.ContainsKey(key))
                    properties[key] = p;
                else
                    properties.Add(key, p);
            }

            return;
        }

        /// <summary>
        /// Get the maximum possible framerate based on current image size, exposure and target frame rate value.
        /// </summary>
        public static float GetResultingFramerate(Device device)
        {
            if (device == null || !device.IsOpen)
                return 0;

            switch (device.Vendor)
            {
                case "Baumer":
                    return GetResultingFramerateBaumer(device);
                case "Basler":
                case "Daheng Imaging":
                case "Vision Datum":
                default:
                    return GetResultingFramerateCommon(device);
            }
        }

        /// <summary>
        /// Get the maximum frame rate for a Basler camera.
        /// </summary>
        private static float GetResultingFramerateCommon(Device device)
        {
            // Some cameras have a dedicated property for the resulting framerate.
            // Note: it looks like casting a double to float is broken in the Baumer API, 
            // it results in an integer. Make sure we first cast to double.
            try
            {
                // Basler, Vision Datum
                Node node = GetNode(device.RemoteNodeList, "ResultingFrameRateAbs");
                if (node != null && node.IsAvailable && node.IsReadable)
                    return (float)(double)node.Value;

                node = GetNode(device.RemoteNodeList, "ResultingFrameRate");
                if (node != null && node.IsAvailable && node.IsReadable)
                    return (float)(double)node.Value;

                // Daheng Imaging
                node = GetNode(device.RemoteNodeList, "CurrentAcquisitionFrameRate");
                if (node != null && node.IsReadable)
                    return (float)(double)node.Value;
            }
            catch (BGAPI2.Exceptions.ErrorException e)
            {
                log.ErrorFormat("ErrorException while reading resulting frame rate");
                log.ErrorFormat("Description: {0}", e.GetErrorDescription());
                log.ErrorFormat("Function name: {0}", e.GetFunctionName());
            }
            catch (BGAPI2.Exceptions.LowLevelException e)
            {
                log.ErrorFormat("ErrorException while reading resulting frame rate");
                log.ErrorFormat("Description: {0}", e.GetErrorDescription());
                log.ErrorFormat("Function name: {0}", e.GetFunctionName());
            }
            catch (Exception e)
            {
                log.ErrorFormat("ErrorException while reading resulting frame rate");
                log.ErrorFormat("Description: {0}", e.Message);
            }

            return 0;
        }


        /// <summary>
        /// Get the maximum frame rate for a Baumer camera.
        /// </summary>
        private static float GetResultingFramerateBaumer(Device device)
        {
            // FIXME: The resulting value is only correct when using grayscale output.

            float resultingFramerate = 0;

            try
            {
                Node nodeReadOut = GetNode(device.RemoteNodeList, "ReadOutTime");
                Node nodeExposure = GetNode(device.RemoteNodeList, "ExposureTime");
                if (nodeReadOut == null || !nodeReadOut.IsAvailable || !nodeReadOut.IsReadable ||
                    nodeExposure == null || !nodeExposure.IsAvailable || !nodeExposure.IsReadable)
                    return resultingFramerate;

                double framerateReadout = 1000000.0 / nodeReadOut.Value;
                double framerateExposure = 1000000.0 / nodeExposure.Value;
                resultingFramerate = (float)Math.Min(framerateReadout, framerateExposure);

                Node nodeFramerate = GetNode(device.RemoteNodeList, "AcquisitionFrameRate");
                Node nodeFramerateEnable = GetNode(device.RemoteNodeList, "AcquisitionFrameRateEnable");
                if (nodeFramerate == null || !nodeFramerate.IsAvailable || !nodeFramerate.IsReadable ||
                    nodeFramerateEnable == null || !nodeFramerateEnable.IsAvailable || !nodeFramerateEnable.IsReadable)
                    return resultingFramerate;

                double framerateSelected = nodeFramerate.Value;
                bool framerateEnabled = nodeFramerateEnable.Value;
                if (!framerateEnabled)
                    return resultingFramerate;

                resultingFramerate = (float)Math.Min(resultingFramerate, framerateSelected);
            }
            catch (Exception e)
            {
                log.ErrorFormat("Error while computing resulting framerate (Baumer). {0}", e.Message);
            }

            return resultingFramerate;
        }

        private static void ReadDeviceClock(Device device, Dictionary<string, CameraProperty> properties)
        {
            // This property is found on IDS cameras.
            if (device.Vendor != "IDS")
                return;

            string key = "clock";

            CameraProperty p = new CameraProperty();
            p.Identifier = "DeviceClockFrequency";
            p.Supported = false;
            p.Type = CameraPropertyType.Float;

            bool present = device.RemoteNodeList.GetNodePresent(p.Identifier);
            if (!present)
                return;

            Node node = device.RemoteNodeList[p.Identifier];
            if (!node.IsImplemented || !node.IsAvailable || !node.IsReadable)
                return;

            p.ReadOnly = false;
            p.Supported = true;

            // FIXME: find a way to expose MHz instead of Hz.
            double currentValue = node.Value;
            double min = node.Min;
            double max = node.Max;
            double step = 1000000.0;
            min = Math.Max(1.0, min);

            p.Minimum = min.ToString(CultureInfo.InvariantCulture);
            p.Maximum = max.ToString(CultureInfo.InvariantCulture);
            p.CurrentValue = currentValue.ToString(CultureInfo.InvariantCulture);
            p.Step = step.ToString(CultureInfo.InvariantCulture);

            // Fix values that should be log.
            bool highRange = (Math.Log10(max) - Math.Log10(min)) >= 4.0;
            p.Representation = highRange ? CameraPropertyRepresentation.LogarithmicSlider : CameraPropertyRepresentation.LinearSlider;

            p.AutomaticIdentifier = "";
            p.CanBeAutomatic = false;
            p.Automatic = false;

            if (properties != null)
                properties.Add(key, p);

            return;
        }
        #endregion

        #region Read low level
        /// <summary>
        /// Read an Integer property.
        /// Optionally the Max value can come from another property.
        /// </summary>
        private static CameraProperty ReadIntegerProperty(Device device, string symbol, string symbolMax)
        {
            CameraProperty p = new CameraProperty();
            p.Identifier = symbol;

            bool readable = NodeIsReadable(device, symbol);
            if (!readable)
            {
                log.WarnFormat("Could not read GenICam INT property {0}: the property is not supported.", symbol);
                return p;
            }

            p.Supported = true;
            p.Type = CameraPropertyType.Integer;
            p.ReadOnly = false;

            Node node = device.RemoteNodeList[symbol];
            int currentValue = (int)node.Value;
            int min = (int)node.Min;
            int max = (int)node.Max;
            int step = node.HasInc ? (int)node.Inc : 1;
                
            // Get the real max from another property if needed. 
            // This happens for width and height where the max of the primary property is dynamic based on the offset.
            if (!string.IsNullOrEmpty(symbolMax))
            {
                bool isMaxReadable = NodeIsReadable(device, symbolMax);
                if (isMaxReadable)
                    max = (int)device.RemoteNodeList[symbolMax].Value;
            }

            // Check if the value should be on a logarithmic scale.
            // We override the representation provided by the property.
            bool highRange = (Math.Log10(max) - Math.Log10(min)) > 4;
            CameraPropertyRepresentation repr = highRange ? CameraPropertyRepresentation.LogarithmicSlider : CameraPropertyRepresentation.LinearSlider;
            p.Representation = repr;

            p.Minimum = min.ToString(CultureInfo.InvariantCulture);
            p.Maximum = max.ToString(CultureInfo.InvariantCulture);
            p.Step = step.ToString(CultureInfo.InvariantCulture);
            p.CurrentValue = currentValue.ToString(CultureInfo.InvariantCulture);
            
            log.DebugFormat("Read GenICam property: \"{0}\" = {1}", symbol, p.CurrentValue);

            return p;
        }

        /// <summary>
        /// Read a Float property.
        /// </summary>
        private static CameraProperty ReadFloatProperty(Device device, string symbol)
        {
            CameraProperty p = new CameraProperty();
            p.Identifier = symbol;

            bool readable = NodeIsReadable(device, symbol);
            if (!readable)
            {
                log.WarnFormat("Could not read GenICam FLOAT property {0}: the property is not supported.", symbol);
                return p;
            }

            p.Supported = true;
            p.Type = CameraPropertyType.Float;
            p.ReadOnly = false;

            // Note: it's important to read these as `double` and not `float` otherwise they
            // get casted to `int` for some reason.
            Node node = device.RemoteNodeList[symbol];
            double currentValue = node.Value;
            double min = node.Min;
            double max = node.Max;
            string repr = node.Representation;

            // We don't support a dedicated control for "pure numbers" just use the regular slider.
            if (repr == "PureNumber")
                repr = "Linear";

            // Fix values that should be log.
            if ((Math.Log10(max) - Math.Log10(min)) > 4.0)
                repr = "Logarithmic";

            p.Minimum = min.ToString(CultureInfo.InvariantCulture);
            p.Maximum = max.ToString(CultureInfo.InvariantCulture);
            p.Representation = ConvertRepresentation(repr);
            p.CurrentValue = currentValue.ToString(CultureInfo.InvariantCulture);

            log.DebugFormat("Read GenICam property: \"{0}\" = {1}", symbol, p.CurrentValue);

            return p;
        }
        
        /// <summary>
        /// Read an integer value directly.
        /// </summary>
        public static int ReadInteger(Device device, string symbol)
        {
            bool readable = NodeIsReadable(device, symbol);
            if (!readable)
            {
                return default;
            }

            return (int)device.RemoteNodeList[symbol].Value;
        }

        /// <summary>
        /// Read a string property directly.
        /// </summary>
        public static string ReadString(Device device, string symbol)
        {
            bool readable = NodeIsReadable(device, symbol);
            if (!readable)
            {
                return default;
            }

            return (string)device.RemoteNodeList[symbol].Value;
        }

        /// <summary>
        /// Read the auto property value and put it into a boolean.
        /// </summary>
        private static bool ReadAuto(Node node, string identifier)
        {
            switch (identifier)
            {
                case "AcquisitionFrameRateEnable":
                    {
                        string currentAutoValue = ((bool)node.Value).ToString(CultureInfo.InvariantCulture).ToLower();
                        return currentAutoValue == GetAutoTrue(identifier);
                    }
                case "GainAuto":
                case "ExposureAuto":
                default:
                    {
                        string currentAutoValue = node.Value;
                        return currentAutoValue == GetAutoTrue(identifier);
                    }
            }
        }

        /// <summary>
        /// Returns a string representation of the value of the auto property corresponding to when the main property IS automatically set.
        /// </summary>
        private static string GetAutoTrue(string identifier)
        {
            switch (identifier)
            {
                case "AcquisitionFrameRateEnable":
                    return "false";
                
                case "AcquisitionFrameRateMode":
                    // Daheng-specific.
                    return "Off";

                case "GainAuto":
                case "ExposureAuto":
                default:
                    return "Continuous";
            }
        }

        /// <summary>
        /// Returns a string representation of the value of the auto property corresponding to when the main property is NOT automatically set.
        /// </summary>
        private static string GetAutoFalse(string identifier)
        {
            switch (identifier)
            {
                case "AcquisitionFrameRateEnable":
                    return "true";
                
                case "AcquisitionFrameRateMode":
                    // Daheng-specific.
                    return "On";

                case "GainAuto":
                case "ExposureAuto":
                default:
                    return "Off";
            }
        }

        /// <summary>
        /// Convert from GenICam property representation to our own.
        /// </summary>
        private static CameraPropertyRepresentation ConvertRepresentation(string representation)
        {
            switch (representation)
            {
                case "Linear":
                    return CameraPropertyRepresentation.LinearSlider;
                case "Logarithmic":
                    return CameraPropertyRepresentation.LogarithmicSlider;
                case "Boolean":
                    return CameraPropertyRepresentation.Checkbox;
                case "PureNumber":
                    return CameraPropertyRepresentation.EditBox;
                case "HexNumber":
                case "_UndefinedRepresentation":
                default:
                    return CameraPropertyRepresentation.Undefined;
            }
        }

        /// <summary>
        /// Return true if the node exists and is readable.
        /// </summary>
        public static bool NodeIsReadable(Device device, string name)
        {
            if (device == null)
                throw new InvalidProgramException();

            if (!device.IsOpen)
                return false;

            bool present = device.RemoteNodeList.GetNodePresent(name);
            if (!present)
            {
                log.DebugFormat("Property {0} not found: node is not present.", name);
                return false;
            }

            Node node = device.RemoteNodeList[name];
            if (!node.IsImplemented)
            {
                log.DebugFormat("Property {0} not found: node is not implemented.", name);
                return false;
            }

            if (!node.IsAvailable)
            {
                log.DebugFormat("Property {0} not found: node is not available.", name);
                return false;
            }

            if (!node.IsReadable)
            {
                log.DebugFormat("Property {0} not found: node is not readable.", name);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Get a handle to a specific GenICam node in the passed NodeMap.
        /// Returns null if the node wasn't found, is not implemented or is not available.
        /// </summary>
        public static Node GetNode(NodeMap map, string name)
        {
            bool present = map.GetNodePresent(name);
            if (!present)
                return null;

            Node node = map[name];
            if (!node.IsImplemented || !node.IsAvailable)
                return null;

            return node;
        }

        /// <summary>
        /// Execute a command property.
        /// Return true if the command was executed.
        /// </summary>
        public static bool ExecuteCommand(Device device, string command)
        {
            if (device == null || !device.IsOpen)
                return false;

            bool result = false;
            try
            {
                if (!device.RemoteNodeList.GetNodePresent(command))
                    return false;

                Node node = device.RemoteNodeList[command];
         
                // FIXME: check that this node is of type command.
                if (!node.IsImplemented || !node.IsAvailable)
                    return false;

                node.Execute();
                log.DebugFormat("Executed command: \"{0}\"", command);
                result = true;
            }
            catch (BGAPI2.Exceptions.ErrorException e)
            {
                log.ErrorFormat("ErrorException while executing command: {0}", command);
                log.ErrorFormat("Description: {0}", e.GetErrorDescription());
                log.ErrorFormat("Function name: {0}", e.GetFunctionName());
            }
            catch (BGAPI2.Exceptions.LowLevelException e)
            {
                log.ErrorFormat("LowLevelException while executing command: {0}", command);
                log.ErrorFormat("Description: {0}", e.GetErrorDescription());
                log.ErrorFormat("Function name: {0}", e.GetFunctionName());
            }
            catch (Exception e)
            {
                log.ErrorFormat("Exception while executing command: {0}", command);
                log.ErrorFormat("Description: {0}", e.Message);
            }

            return result;
        }
        #endregion

        #region Write
        /// <summary>
        /// Commit value of properties that can be written during streaming and don't require a reconnect to be applied.
        /// This is used by the configuration, to update the image while configuring.
        /// </summary>
        public static void Write(Device device, CameraProperty property)
        {
            if (!property.Supported || string.IsNullOrEmpty(property.Identifier) || device == null || !device.IsOpen)
                return;

            try
            {
                switch (property.Identifier)
                {
                    case "AcquisitionFrameRate":
                    case "AcquisitionFrameRateAbs":
                    case "ExposureTime":
                    case "ExposureTimeAbs":
                    case "Gain":
                    case "GainRaw":
                    case "ImageCompressionQuality":
                    case "DeviceClockFrequency":
                        WriteProperty(device, property);
                        break;
                    case "Width":
                    case "Height":
                        // Do nothing, these properties can't be changed while the
                        // camera is streaming.
                        // We write them via WriteCriticalProperties below.
                        break;
                    default:
                        log.ErrorFormat("GenICam property not supported: {0}.", property.Identifier);
                        break;
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("Error while writing GenICam property {0}. {1}", property.Identifier, e.Message);
            }
        }

        /// <summary>
        /// Push values from XML based simple properties into current list of properties.
        /// </summary>
        public static void MergeProperties(Dictionary<string, CameraProperty> dest, Dictionary<string, CameraProperty> source)
        {
            // This is used to import values from simple XML based representation into properties instances.
            foreach (var pair in source)
            {
                if (!dest.ContainsKey(pair.Key))
                    continue;

                dest[pair.Key].Automatic = pair.Value.Automatic;
                dest[pair.Key].CurrentValue = pair.Value.CurrentValue;
            }
        }

        public static void WriteCriticalProperties(Device device, Dictionary<string, CameraProperty> properties)
        {
            if (properties == null || properties.Count == 0)
                return;

            try
            {
                // IDS specific.
                // It seems the DeviceClock property is used to calculate other properties.
                // Basically it looks like the IDS system is expecting that the exposure and
                // frame rate properties are written as if the clock was at its nominal value
                // and when a different value is written it automatically recalculates the
                // exposure and frame rate.
                // Since we store the actual absolute user value, it creates a situation where 
                // every time we connect the camera and write the custom device clock, 
                // it changes the user value to something else, repeatedly.
                // The Peak Cockpit doesn't see this problem because every time 
                // the camera is connected it resets the device clock value to its 
                // nominal value anyway, which is arguably even worse.
                // The solution is to write the device clock first, it will still change the 
                // other properties, but then we overwrite them with the wanted user values.
                if (device.Vendor == "IDS" && properties.ContainsKey("clock"))
                {
                    WriteProperty(device, properties["clock"]);
                }

                foreach (var pair in properties)
                {
                    if (device.Vendor == "IDS" && pair.Key == "clock")
                        continue;

                    if (pair.Key == "width")
                        WriteSize(device, pair.Value, "OffsetX");
                    else if (pair.Key == "height")
                        WriteSize(device, pair.Value, "OffsetY");
                    else
                        WriteProperty(device, pair.Value);
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("Error while writing GenICam property. {0}", e.Message);
            }
        }
   
        /// <summary>
        /// Write either width or height as a centered region of interest.
        /// </summary>
        private static void WriteSize(Device device, CameraProperty property, string identifierOffset)
        {
            if (property.ReadOnly)
                return;

            NodeMap nodemap = device.RemoteNodeList;
            Node node = GetNode(nodemap, property.Identifier);
            if (node == null || !node.IsReadable || !node.IsWriteable)
                return;
            
            long value = long.Parse(property.CurrentValue, CultureInfo.InvariantCulture);
            long min = node.Min;
            long step = node.HasInc ? node.Inc : 1;

            // Do not clamp on max, the max is based on the offset instead of the true max.
            value = Math.Max(value, min);
            long remainder = (value - min) % step;
            if (remainder != 0)
                value = value - remainder + step;

            // Offset handling.
            // Some cameras have a CenterX/CenterY property.
            // When it is set, the offset is automatic and becomes read-only.
            // If the offset can be written we use the normal computation.
            Node nodeOffset = GetNode(nodemap, identifierOffset);
            bool setOffset = nodeOffset != null && node.IsReadable && node.IsWriteable;
            if (setOffset)
            {
                long currentValue = node.Value;
                long max = currentValue + nodeOffset.Max;
                long offset = (max - value) / 2;
                long minOffset = nodeOffset.Min;
                long stepOffset = nodeOffset.HasInc ? nodeOffset.Inc : 1;
                
                long remainderOffset = (offset - minOffset) % stepOffset;
                if (remainderOffset != 0)
                    offset = offset - remainderOffset + stepOffset;

                // We need to be careful with the order and not write a value that doesn't fit due to the offset, or vice versa.
                if (value > currentValue)
                {
                    nodeOffset.Value = offset;
                    node.Value = value;
                }
                else
                {
                    node.Value = value;
                    nodeOffset.Value = offset;
                }
            }
            else
            {
                node.Value = value;
            }
        }

        /// <summary>
        /// Write generic property with optional auto flag.
        /// </summary>
        private static void WriteProperty(Device device, CameraProperty property)
        {
            if (property.ReadOnly)
                return;

            NodeMap nodeMap = device.RemoteNodeList;

            // Switch OFF the auto flag if needed, to be able to write the main property.
            if (!string.IsNullOrEmpty(property.AutomaticIdentifier))
            {
                Node nodeAuto = GetNode(nodeMap, property.AutomaticIdentifier);
                if (nodeAuto != null)
                {
                    bool writeable = nodeAuto.IsWriteable;
                    bool currentAuto = ReadAuto(nodeAuto, property.AutomaticIdentifier);
                    if (writeable && property.CanBeAutomatic && currentAuto && !property.Automatic)
                        WriteAuto(nodeAuto, property.AutomaticIdentifier, false);
                }
            }

            // At this point the auto flag is off. Write the main property.
            Node node = GetNode(nodeMap, property.Identifier);
            if (node == null)
                return;

            if (!node.IsReadable || !node.IsWriteable)
                return;

            try
            {
                switch (property.Type)
                {
                    case CameraPropertyType.Integer:
                        {
                            long value = long.Parse(property.CurrentValue, CultureInfo.InvariantCulture);
                            long min = node.Min;
                            long max = node.Max;
                            long step = node.HasInc ? node.Inc : 1;
                            value = FixValue(value, min, max, step);
                            node.Value = value;
                            log.DebugFormat("Wrote GenICam property \"{0}\"={1}", property.Identifier, value);
                            break;
                        }
                    case CameraPropertyType.Float:
                        {
                            double value = double.Parse(property.CurrentValue, CultureInfo.InvariantCulture);
                            double min = node.Min;
                            double max = node.Max;
                            value = FixValue(value, min, max);
                            node.Value = value;
                            log.DebugFormat("Wrote GenICam property \"{0}\"={1}", property.Identifier, value);
                            break;
                        }
                    case CameraPropertyType.Boolean:
                        {
                            bool value = bool.Parse(property.CurrentValue);
                            node.Value = value;
                            log.DebugFormat("Wrote GenICam property \"{0}\"={1}", property.Identifier, value);
                            break;
                        }
                    default:
                        break;
                }
            }
            catch
            {
                log.ErrorFormat("Error while writing GenICam property {0}.", property.Identifier);
            }

            // Finally, switch ON the auto flag if needed.
            if (!string.IsNullOrEmpty(property.AutomaticIdentifier))
            {
                Node nodeAuto = GetNode(nodeMap, property.AutomaticIdentifier);
                if (nodeAuto != null && nodeAuto.IsWriteable && property.CanBeAutomatic && property.Automatic)
                    WriteAuto(nodeAuto, property.AutomaticIdentifier, true);
            }
        }

        private static long FixValue(long value, long min, long max, long step)
        {
            value = Math.Min(max, Math.Max(min, value));

            long remainder = value % step;
            if (remainder > 0)
                value = value - remainder;

            return value;
        }

        private static double FixValue(double value, double min, double max)
        {
            return Math.Min(max, Math.Max(min, value));
        }
 
        /// <summary>
        /// Takes a boolean of whether auto is ON or OFF, convert it to the correct representation and write it in the auto property.
        /// </summary>
        private static void WriteAuto(Node node, string identifier, bool isAuto)
        {
            string newValue = isAuto ? GetAutoTrue(identifier) : GetAutoFalse(identifier);

            switch (identifier)
            {
                case "AcquisitionFrameRateEnable":
                    {
                        bool newValueBool = bool.Parse(newValue);
                        node.Value = newValueBool;
                        break;
                    }
                case "GainAuto":
                case "ExposureAuto":
                default:
                    {
                        node.Value = newValue;
                        break;
                    }
            }
        }

        private static string BoolValueToString(SmartValue value)
        {
            return ((bool)value).ToString(CultureInfo.InvariantCulture).ToLower();
        }

        /// <summary>
        /// Write the value into an enum property.
        /// </summary>
        public static void WriteEnum(Device device, string enumName, string enumValue)
        {
            if (device == null || !device.IsOpen)
                throw new InvalidProgramException();

            bool present = device.RemoteNodeList.GetNodePresent(enumName);
            if (!present)
                return;

            Node node = device.RemoteNodeList[enumName];
            if (!node.IsImplemented || !node.IsAvailable || !node.IsWriteable)
                return;

            node.Value = enumValue;
        }

        /// <summary>
        /// Make sure the feature is triggered at least once, 
        /// either it's currently in continuous mode or we trigger it manually.
        /// </summary>
        private static void ContinuousOrOnce(Device device, string identifier)
        {
            if (device == null || !device.IsOpen)
                throw new InvalidProgramException();

            bool present = device.RemoteNodeList.GetNodePresent(identifier);
            if (!present)
                return;

            Node node = device.RemoteNodeList[identifier];
            if (!node.IsImplemented || !node.IsAvailable || !node.IsWriteable)
                return;

            // These nodes are either on "Off" or "Continuous".
            if (node.Value == "Off")
            {
                node.Value = "Once";
            }
        }
        #endregion

        #region JPEG Hardware compression (Baumer)
        /// <summary>
        /// Whether the device supports hardware JPEG compression.
        /// </summary>
        public static bool SupportsJPEG(Device device)
        {
            if (device == null || device.Vendor != "Baumer")
                return false;

            bool isReadable = NodeIsReadable(device, "ImageCompressionMode");
            if (!isReadable)
                return false;

            NodeMap enumCompression = device.RemoteNodeList["ImageCompressionMode"].EnumNodeList;
            return enumCompression.GetNodePresent("JPEG");
        }

        /// <summary>
        /// Whether the pixel format is compatible with hardware compression.
        /// </summary>
        public static bool FormatCanCompress(Device device, string pixelFormat)
        {
            if (device == null || device.Vendor != "Baumer")
                return false;

            return pixelFormat == "Mono8" || pixelFormat == "YCbCr422_8";
        }

        /// <summary>
        /// Enable or disable JPEG compression.
        /// </summary>
        public static void SetJPEG(Device device, bool enable)
        {
            if (enable)
                device.RemoteNodeList["ImageCompressionMode"].Value = "JPEG";
            else
                device.RemoteNodeList["ImageCompressionMode"].Value = "Off";
        }

        /// <summary>
        /// Reads the current value of JPEG compression.
        /// </summary>
        public static bool GetJPEG(Device device)
        {
            if (!SupportsJPEG(device))
                return false;

            return device.RemoteNodeList["ImageCompressionMode"].Value == "JPEG";
        }
        #endregion

    }
}