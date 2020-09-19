using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using BGAPI2;

namespace Kinovea.Camera.Baumer
{
    /// <summary>
    /// Reads and writes a list of supported camera properties from/to the device.
    /// </summary>
    public static class CameraPropertyManager
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static Dictionary<string, CameraProperty> Read(Device device, string fullName)
        {
            Dictionary<string, CameraProperty> properties = new Dictionary<string, CameraProperty>();

            if (device == null || !device.IsOpen)
                return properties;

            try
            {
                properties.Add("width", ReadIntegerProperty(device, "Width", "WidthMax"));
                properties.Add("height", ReadIntegerProperty(device, "Height", "HeightMax"));

                // Camera properties in Kinovea combine the value and the "auto" flag.
                // We potentially need to read several Basler camera properties to create one Kinovea camera property.
                ReadFramerate(device, properties);
                ReadExposure(device, properties);
                ReadGain(device, properties);
            }
            catch (Exception e)
            {
                log.ErrorFormat("Error while reading Baumer camera properties. {0}.", e.Message);
            }

            return properties;
        }

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
                        WriteProperty(device, property);
                        break;
                    case "Width":
                    case "Height":
                        // Do nothing. These properties must be changed from WriteCriticalProperties below.
                        break;
                    default:
                        log.ErrorFormat("Baumer property not supported: {0}.", property.Identifier);
                        break;
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("Error while writing Baumer property {0}. {1}", property.Identifier, e.Message);
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
                WriteCenter(device);

                foreach (var pair in properties)
                {
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
                log.ErrorFormat("Error while writing Baumer property. {0}", e.Message);
            }
        }

        private static CameraProperty ReadFramerate(Device device, Dictionary<string, CameraProperty> properties)
        {
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
                    return p;
            }

            Node node = device.RemoteNodeList[p.Identifier];
            if (!node.IsImplemented || !node.IsAvailable || !node.IsReadable)
                return p;

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

            // AcquisitionFrameRateEnable=false: the framerate is automatically set to the max value possible (auto = true).
            // AcquisitionFrameRateEnable=true: use the custom framerate set by the user in AcquisitionFrameRate (auto = false).

            string autoIdentifier = "AcquisitionFrameRateEnable";
            p.AutomaticIdentifier = autoIdentifier;
            p.Automatic = false;
            p.CanBeAutomatic = false;
            Node autoNode = null;
            bool autoPresent = device.RemoteNodeList.GetNodePresent(p.AutomaticIdentifier);
            if (autoPresent)
            {
                autoNode = device.RemoteNodeList[p.AutomaticIdentifier];
                p.CanBeAutomatic = autoNode.IsWriteable;
            }
 
            if (p.CanBeAutomatic)
            {
                string currentAutoValue = ((bool)autoNode.Value).ToString(CultureInfo.InvariantCulture).ToLower();
                p.Automatic = currentAutoValue == GetAutoTrue(autoIdentifier);
            }

            if (properties != null)
                properties.Add("framerate", p);

            return p;
        }

        private static void ReadExposure(Device device, Dictionary<string, CameraProperty> properties)
        {
            CameraProperty p = ReadFloatProperty(device, "ExposureTime");
            if (!p.Supported)
                p = ReadFloatProperty(device, "ExposureTimeAbs");

            string autoIdentifier = "ExposureAuto";
            p.AutomaticIdentifier = autoIdentifier;
            p.CanBeAutomatic = false;
            p.Automatic = false;
            bool autoReadable = BaumerHelper.NodeIsReadable(device, p.AutomaticIdentifier);
            if (autoReadable)
            {
                p.CanBeAutomatic = true;
                string autoValue = BaumerHelper.GetString(device, p.AutomaticIdentifier);
                p.Automatic = autoValue == GetAutoTrue(autoIdentifier);
            }

            properties.Add("exposure", p);
        }

        private static void ReadGain(Device device, Dictionary<string, CameraProperty> properties)
        {
            CameraProperty p = ReadFloatProperty(device, "Gain");
            if (!p.Supported)
                p = ReadIntegerProperty(device, "GainRaw", null);

            string autoIdentifier = "GainAuto";
            p.AutomaticIdentifier = autoIdentifier;
            p.CanBeAutomatic = false;
            p.Automatic = false;
            bool autoReadable = BaumerHelper.NodeIsReadable(device, p.AutomaticIdentifier);
            if (autoReadable)
            {
                p.CanBeAutomatic = true;
                string autoValue = BaumerHelper.GetString(device, p.AutomaticIdentifier);
                p.Automatic = autoValue == GetAutoTrue(autoIdentifier);
            }

            properties.Add("gain", p);
        }

        private static CameraProperty ReadIntegerProperty(Device device, string symbol, string symbolMax)
        {
            CameraProperty p = new CameraProperty();
            p.Identifier = symbol;

            bool readable = BaumerHelper.NodeIsReadable(device, symbol);
            if (!readable)
            {
                log.WarnFormat("Could not read Baumer property {0}: the property is not supported.", symbol);
                return p;
            }

            p.Supported = true;
            p.Type = CameraPropertyType.Integer;
            p.ReadOnly = false;

            Node node = device.RemoteNodeList[symbol];
            int currentValue = (int)node.Value;
            int min = (int)node.Min;
            int max = (int)node.Max;
            int step = (int)node.Inc;

            // Get the real max from another property, the bare max depends on the current offset.
            if (!string.IsNullOrEmpty(symbolMax))
            {
                bool maxReadable = BaumerHelper.NodeIsReadable(device, symbolMax);
                if (maxReadable)
                    max = (int)device.RemoteNodeList[symbolMax].Value;
            }

            bool highRange = (Math.Log10(max) - Math.Log10(min)) > 4;
            
            p.Minimum = min.ToString(CultureInfo.InvariantCulture);
            p.Maximum = max.ToString(CultureInfo.InvariantCulture);
            p.Step = step.ToString(CultureInfo.InvariantCulture);
            p.Representation = highRange ? CameraPropertyRepresentation.LogarithmicSlider : CameraPropertyRepresentation.LinearSlider;
            p.CurrentValue = currentValue.ToString(CultureInfo.InvariantCulture);

            return p;
        }

        private static CameraProperty ReadFloatProperty(Device device, string symbol)
        {
            CameraProperty p = new CameraProperty();
            p.Identifier = symbol;

            bool readable = BaumerHelper.NodeIsReadable(device, symbol);
            if (!readable)
            {
                log.WarnFormat("Could not read Baumer property {0}: the property is not supported.", symbol);
                return p;
            }

            p.Supported = true;
            p.Type = CameraPropertyType.Float;
            p.ReadOnly = false;

            Node node = device.RemoteNodeList[symbol];
            double min = node.Min;
            double max = node.Max;
            string repr = node.Representation;
            double currentValue = node.Value;
            
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

            return p;
        }

        private static CameraProperty ReadBooleanProperty(Device device, string symbol)
        {
            return null;
            //CameraProperty p = new CameraProperty();
            //p.Identifier = symbol;

            //NODEMAP_HANDLE nodeMapHandle = Pylon.DeviceGetNodeMap(deviceHandle);
            //NODE_HANDLE nodeHandle = GenApi.NodeMapGetNode(nodeMapHandle, symbol);
            //if (!nodeHandle.IsValid)
            //{
            //    log.WarnFormat("Could not read Basler property {0}: node handle is not valid. (The property is not supported).", symbol);
            //    return p;
            //}

            //EGenApiAccessMode accessMode = GenApi.NodeGetAccessMode(nodeHandle);
            //if (accessMode == EGenApiAccessMode._UndefinedAccesMode || accessMode == EGenApiAccessMode.NA ||
            //    accessMode == EGenApiAccessMode.NI || accessMode == EGenApiAccessMode.WO)
            //{
            //    log.WarnFormat("Could not read Basler property {0}: Access mode not supported. (The property is not readable).", symbol);
            //    return p;
            //}

            //EGenApiNodeType type = GenApi.NodeGetType(nodeHandle);
            //if (type != EGenApiNodeType.BooleanNode)
            //{
            //    log.WarnFormat("Could not read Basler property {0}: the node is of the wrong type. Expected: Boolean. Received:{1}", symbol, type.ToString());
            //    return p;
            //}

            //p.Supported = true;
            //p.Type = CameraPropertyType.Boolean;
            //p.ReadOnly = accessMode != EGenApiAccessMode.RW;

            //bool currentValue = GenApi.BooleanGetValue(nodeHandle);

            //p.Representation = CameraPropertyRepresentation.Checkbox;
            //p.CurrentValue = currentValue.ToString(CultureInfo.InvariantCulture);

            //return p;
        }

        private static void WriteCenter(Device device)
        {
            // Force write the CenterX and CenterY properties if supported.
            // https://docs.baslerweb.com/center-x-and-center-y.html
            // This is apparently required in order for the MaxWidth/MaxHeight properties to behave correctly 
            // and independently of the offset property.
            // To summarize we use the following approach:
            // 1. If CenterX/CenterY are supported properties, we use them and OffsetX/OffsetY will be automated by Pylon.
            // 2. Otherwise we use manually write OffsetX/OffsetY to center the image.
            //NODEMAP_HANDLE nodeMapHandle = Pylon.DeviceGetNodeMap(deviceHandle);
            //NODE_HANDLE nodeHandleX = GenApi.NodeMapGetNode(nodeMapHandle, "CenterX");
            //NODE_HANDLE nodeHandleY = GenApi.NodeMapGetNode(nodeMapHandle, "CenterY");
            //if (nodeHandleX.IsValid && nodeHandleY.IsValid)
            //{
            //    EGenApiAccessMode accessModeOffsetX = GenApi.NodeGetAccessMode(nodeHandleX);
            //    EGenApiAccessMode accessModeOffsetY = GenApi.NodeGetAccessMode(nodeHandleY);
            //    if (accessModeOffsetX == EGenApiAccessMode.RW && accessModeOffsetY == EGenApiAccessMode.RW)
            //    {
            //        GenApi.BooleanSetValue(nodeHandleX, true);
            //        GenApi.BooleanSetValue(nodeHandleY, true);
            //    }
            //}
        }

        /// <summary>
        /// Write either width or height as a centered region of interest.
        /// </summary>
        private static void WriteSize(Device device, CameraProperty property, string identifierOffset)
        {
            if (property.ReadOnly)
                return;

            //NODEMAP_HANDLE nodeMapHandle = Pylon.DeviceGetNodeMap(deviceHandle);
            //NODE_HANDLE nodeHandle = GenApi.NodeMapGetNode(nodeMapHandle, property.Identifier);
            //if (!nodeHandle.IsValid)
            //    return;

            //EGenApiAccessMode accessMode = GenApi.NodeGetAccessMode(nodeHandle);
            //if (accessMode != EGenApiAccessMode.RW)
            //    return;

            //long value = long.Parse(property.CurrentValue, CultureInfo.InvariantCulture);
            //long min = GenApi.IntegerGetMin(nodeHandle);
            //long max = GenApi.IntegerGetMax(nodeHandle);
            //long step = GenApi.IntegerGetInc(nodeHandle);
            
            //value = FixValue(value, min, max, step);
            
            //// Offset handling.
            //// Some cameras have a CenterX/CenterY property.
            //// When it is set, the offset is automatic and becomes read-only.
            //bool setOffset = false;
            //NODE_HANDLE nodeHandleOffset = GenApi.NodeMapGetNode(nodeMapHandle, identifierOffset);
            //if (nodeHandleOffset.IsValid)
            //{
            //    EGenApiAccessMode accessModeOffset = GenApi.NodeGetAccessMode(nodeHandleOffset);
            //    if (accessModeOffset == EGenApiAccessMode.RW)
            //        setOffset = true;
            //}

            //if (setOffset)
            //{
            //    long offset = (max - value) / 2;
            //    long minOffset = GenApi.IntegerGetMin(nodeHandleOffset);
            //    long stepOffset = GenApi.IntegerGetInc(nodeHandleOffset);
            //    long remainderOffset = (offset - minOffset) % stepOffset;
            //    if (remainderOffset != 0)
            //        offset = offset - remainderOffset + stepOffset;

            //    // We need to be careful with the order and not write a value that doesn't fit due to the offset, or vice versa.
            //    long currentValue = GenApi.IntegerGetValue(nodeHandle);
            //    if (value > currentValue)
            //    {
            //        GenApi.IntegerSetValue(nodeHandleOffset, offset);
            //        GenApi.IntegerSetValue(nodeHandle, value);
            //    }
            //    else
            //    {
            //        GenApi.IntegerSetValue(nodeHandle, value);
            //        GenApi.IntegerSetValue(nodeHandleOffset, offset);
            //    }
            //}
            //else
            //{
            //    GenApi.IntegerSetValue(nodeHandle, value);
            //}
        }

        /// <summary>
        /// Write generic property with optional auto flag.
        /// </summary>
        private static void WriteProperty(Device device, CameraProperty property)
        {
            if (property.ReadOnly)
                return;

            //NODEMAP_HANDLE nodeMapHandle = Pylon.DeviceGetNodeMap(deviceHandle);

            //// Switch OFF the auto flag if needed, to be able to write the main property.
            //if (!string.IsNullOrEmpty(property.AutomaticIdentifier))
            //{
            //    NODE_HANDLE nodeHandleAuto = GenApi.NodeMapGetNode(nodeMapHandle, property.AutomaticIdentifier);
            //    if (nodeHandleAuto.IsValid)
            //    {
            //        bool writeable = GenApi.NodeIsWritable(nodeHandleAuto);
            //        bool currentAuto = ReadAuto(nodeHandleAuto, property.AutomaticIdentifier);
            //        if (writeable && property.CanBeAutomatic && currentAuto && !property.Automatic)
            //            WriteAuto(nodeHandleAuto, property.AutomaticIdentifier, false);
            //    }
            //}

            //// At this point the auto flag is off. Write the main property.
            //NODE_HANDLE nodeHandle = GenApi.NodeMapGetNode(nodeMapHandle, property.Identifier);
            //if (!nodeHandle.IsValid)
            //    return;

            //EGenApiAccessMode accessMode = GenApi.NodeGetAccessMode(nodeHandle);
            //if (accessMode != EGenApiAccessMode.RW)
            //    return;

            //try
            //{
            //    switch (property.Type)
            //    {
            //        case CameraPropertyType.Integer:
            //            {
            //                long value = long.Parse(property.CurrentValue, CultureInfo.InvariantCulture);
            //                long min = GenApi.IntegerGetMin(nodeHandle);
            //                long max = GenApi.IntegerGetMax(nodeHandle);
            //                long step = GenApi.IntegerGetInc(nodeHandle);
            //                value = FixValue(value, min, max, step);
            //                GenApi.IntegerSetValue(nodeHandle, value);
            //                break;
            //            }
            //        case CameraPropertyType.Float:
            //            {
            //                double value = double.Parse(property.CurrentValue, CultureInfo.InvariantCulture);
            //                double min = GenApi.FloatGetMin(nodeHandle);
            //                double max = GenApi.FloatGetMax(nodeHandle);
            //                value = FixValue(value, min, max);
            //                GenApi.FloatSetValue(nodeHandle, value);
            //                break;
            //            }
            //        case CameraPropertyType.Boolean:
            //            {
            //                bool value = bool.Parse(property.CurrentValue);
            //                GenApi.BooleanSetValue(nodeHandle, value);
            //                break;
            //            }
            //        default:
            //            break;
            //    }
            //}
            //catch
            //{
            //    log.ErrorFormat("Error while writing Baumer GenICam property {0}.", property.Identifier);
            //}

            //// Finally, switch ON the auto flag if needed.
            //if (!string.IsNullOrEmpty(property.AutomaticIdentifier))
            //{
            //    NODE_HANDLE nodeHandleAuto = GenApi.NodeMapGetNode(nodeMapHandle, property.AutomaticIdentifier);
            //    if (nodeHandleAuto.IsValid && GenApi.NodeIsWritable(nodeHandleAuto) && property.CanBeAutomatic && property.Automatic)
            //        WriteAuto(nodeHandleAuto, property.AutomaticIdentifier, true);
            //}
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
        /// Read the auto property value and put it into a boolean.
        /// </summary>
        private static bool ReadAuto(Device device, string identifier)
        {
            return false;
            //switch (identifier)
            //{
            //    case "AcquisitionFrameRateEnable":
            //        {
            //            string currentAutoValue = GenApi.BooleanGetValue(nodeHandle).ToString(CultureInfo.InvariantCulture).ToLower();
            //            return currentAutoValue == GetAutoTrue(identifier);
            //        }
            //    case "GainAuto":
            //    case "ExposureAuto":
            //    default:
            //        {
            //            string currentAutoValue = GenApi.NodeToString(nodeHandle);
            //            return currentAutoValue == GetAutoTrue(identifier);
            //        }
            //}
        }

        /// <summary>
        /// Takes a boolean of whether auto is ON or OFF, convert it to the correct representation and write it in the auto property.
        /// </summary>
        private static void WriteAuto(Device device, string identifier, bool isAuto)
        {
            //string newValue = isAuto ? GetAutoTrue(identifier) : GetAutoFalse(identifier);

            //switch (identifier)
            //{
            //    case "AcquisitionFrameRateEnable":
            //        {
            //            bool newValueBool = bool.Parse(newValue);
            //            GenApi.BooleanSetValue(nodeHandle, newValueBool);
            //            break;
            //        }
            //    case "GainAuto":
            //    case "ExposureAuto":
            //    default:
            //        {
            //            PylonHelper.WriteEnum(nodeHandle, identifier, newValue);
            //            break;
            //        }
            //}
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
                case "GainAuto":
                case "ExposureAuto":
                default:
                    return "Off";
            }
        }
    }
}