using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PylonC.NET;
using System.Globalization;
using PylonC.NETSupportLibrary;

namespace Kinovea.Camera.Basler
{
    /// <summary>
    /// Reads and writes a list of supported camera properties from/to the device.
    /// </summary>
    public static class CameraPropertyManager
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static Dictionary<string, CameraProperty> Read(PYLON_DEVICE_HANDLE deviceHandle, string fullName)
        {
            Dictionary<string, CameraProperty> properties = new Dictionary<string, CameraProperty>();

            // Enumerate devices again to make sure we have the correct device index and find the device class.
            DeviceEnumerator.Device device = null;
            List<DeviceEnumerator.Device> devices = DeviceEnumerator.EnumerateDevices();
            foreach (DeviceEnumerator.Device candidate in devices)
            {
                if (candidate.FullName != fullName)
                    continue;

                device = candidate;
                break;
            }

            if (device == null)
                return properties;

            try
            {
                properties.Add("width", ReadIntegerProperty(deviceHandle, "Width", "WidthMax"));
                properties.Add("height", ReadIntegerProperty(deviceHandle, "Height", "HeightMax"));

                // Camera properties in Kinovea combine the value and the "auto" flag.
                // We potentially need to read several Basler camera properties to create one Kinovea camera property.
                ReadFramerate(deviceHandle, properties);
                ReadExposure(deviceHandle, properties);
                ReadGain(deviceHandle, properties);
            }
            catch (Exception e)
            {
                log.ErrorFormat("Error while reading Basler camera properties. {0}.", e.Message);
            }

            
            return properties;
        }

        /// <summary>
        /// Commit value of properties that can be written during streaming and don't require a reconnect to be applied.
        /// This is used by the configuration, to update the image while configuring.
        /// </summary>
        public static void Write(PYLON_DEVICE_HANDLE deviceHandle, CameraProperty property)
        {
            if (!property.Supported || string.IsNullOrEmpty(property.Identifier) || !deviceHandle.IsValid)
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
                        WriteProperty(deviceHandle, property);
                        break;
                    case "Width":
                    case "Height":
                        // Do nothing. These properties must be changed from WriteCriticalProperties below.
                        break;
                    default:
                        log.ErrorFormat("Basler  property not supported: {0}.", property.Identifier);
                        break;
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("Error while writing Basler property {0}. {1}", property.Identifier, e.Message);
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

        public static void WriteCriticalProperties(PYLON_DEVICE_HANDLE deviceHandle, Dictionary<string, CameraProperty> properties)
        {
            if (properties == null || properties.Count == 0)
                return;

            try
            {
                WriteCenter(deviceHandle);

                foreach (var pair in properties)
                {
                    if (pair.Key == "width")
                        WriteSize(deviceHandle, pair.Value, "OffsetX");
                    else if (pair.Key == "height")
                        WriteSize(deviceHandle, pair.Value, "OffsetY");
                    else
                        WriteProperty(deviceHandle, pair.Value);
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("Error while writing Basler property. {0}", e.Message);
            }

        }

        private static CameraProperty ReadFramerate(PYLON_DEVICE_HANDLE deviceHandle, Dictionary<string, CameraProperty> properties)
        {
            CameraProperty p = new CameraProperty();
            p.Identifier = "AcquisitionFrameRate";
            p.Supported = false;
            p.Type = CameraPropertyType.Float;

            NODEMAP_HANDLE nodeMapHandle = Pylon.DeviceGetNodeMap(deviceHandle);
            NODE_HANDLE nodeHandle = GenApi.NodeMapGetNode(nodeMapHandle, p.Identifier);
            if (!nodeHandle.IsValid)
            {
                p.Identifier = "AcquisitionFrameRateAbs";
                nodeHandle = GenApi.NodeMapGetNode(nodeMapHandle, p.Identifier);
                if (!nodeHandle.IsValid)
                    return p;
            }

            EGenApiAccessMode accessMode = GenApi.NodeGetAccessMode(nodeHandle);
            if (accessMode == EGenApiAccessMode._UndefinedAccesMode || accessMode == EGenApiAccessMode.NA ||
                accessMode == EGenApiAccessMode.NI || accessMode == EGenApiAccessMode.WO)
            {
                log.WarnFormat("Could not read Basler property {0}: Access mode not supported. (The property is not readable).", p.Identifier);
                return p;
            }

            EGenApiNodeType type = GenApi.NodeGetType(nodeHandle);
            if (type != EGenApiNodeType.FloatNode)
            {
                log.WarnFormat("Could not read Basler property {0}: the node is of the wrong type. Expected: Float. Received:{1}", p.Identifier, type.ToString());
                return p;
            }

            p.ReadOnly = false;
            p.Supported = true;

            double currentValue = GenApi.FloatGetValue(nodeHandle);
            double min = GenApi.FloatGetMin(nodeHandle);
            double max = GenApi.FloatGetMax(nodeHandle);
            double step = 1.0;
            min = Math.Max(1.0, min);

            p.Minimum = min.ToString(CultureInfo.InvariantCulture);
            p.Maximum = max.ToString(CultureInfo.InvariantCulture);
            p.CurrentValue = currentValue.ToString(CultureInfo.InvariantCulture);
            p.Step = step.ToString(CultureInfo.InvariantCulture);
            
            // Fix values that should be log.
            double range = Math.Log10(max) - Math.Log10(min);
            p.Representation = (range >= 4) ? CameraPropertyRepresentation.LogarithmicSlider : CameraPropertyRepresentation.LinearSlider;

            // AcquisitionFrameRateEnable=false: the framerate is automatically set to the max value possible.
            // AcquisitionFrameRateEnable=true: use the custom framerate set by the user in AcquisitionFrameRate.

            string autoIdentifier = "AcquisitionFrameRateEnable";
            p.AutomaticIdentifier = autoIdentifier;
            NODE_HANDLE nodeHandleAuto = GenApi.NodeMapGetNode(nodeMapHandle, autoIdentifier);
            p.CanBeAutomatic = nodeHandleAuto.IsValid && GenApi.NodeIsWritable(nodeHandleAuto);
            p.Automatic = false;
            if (p.CanBeAutomatic)
            {
                string currentAutoValue = GenApi.BooleanGetValue(nodeHandleAuto).ToString(CultureInfo.InvariantCulture).ToLower();
                p.Automatic = currentAutoValue == GetAutoTrue(autoIdentifier);
            }

            if (properties != null)
                properties.Add("framerate", p);

            return p;
        }

        private static void ReadExposure(PYLON_DEVICE_HANDLE deviceHandle, Dictionary<string, CameraProperty> properties)
        {
            CameraProperty p = ReadFloatProperty(deviceHandle, "ExposureTime");
            if (!p.Supported)
                p = ReadFloatProperty(deviceHandle, "ExposureTimeAbs");

            string autoIdentifier = "ExposureAuto";
            p.AutomaticIdentifier = autoIdentifier;
            GenApiEnum auto = PylonHelper.ReadEnumCurrentValue(deviceHandle, p.AutomaticIdentifier);
            p.CanBeAutomatic = auto != null;
            p.Automatic = false;
            if (p.CanBeAutomatic && !string.IsNullOrEmpty(auto.Symbol))
            {
                p.Automatic = auto.Symbol == GetAutoTrue(autoIdentifier);
            }

            properties.Add("exposure", p);
        }

        private static void ReadGain(PYLON_DEVICE_HANDLE deviceHandle, Dictionary<string, CameraProperty> properties)
        {
            CameraProperty p = ReadFloatProperty(deviceHandle, "Gain");
            if (!p.Supported)
                p = ReadIntegerProperty(deviceHandle, "GainRaw", null);

            string autoIdentifier = "GainAuto";
            p.AutomaticIdentifier = autoIdentifier;
            GenApiEnum auto = PylonHelper.ReadEnumCurrentValue(deviceHandle, p.AutomaticIdentifier);
            p.CanBeAutomatic = auto != null;
            p.Automatic = false;
            if (p.CanBeAutomatic && !string.IsNullOrEmpty(auto.Symbol))
            {
                p.Automatic = auto.Symbol == GetAutoTrue(autoIdentifier);
            }

            properties.Add("gain", p);
        }

        private static CameraProperty ReadIntegerProperty(PYLON_DEVICE_HANDLE deviceHandle, string symbol, string symbolMax)
        {
            CameraProperty p = new CameraProperty();
            p.Identifier = symbol;
            
            NODEMAP_HANDLE nodeMapHandle = Pylon.DeviceGetNodeMap(deviceHandle);
            NODE_HANDLE nodeHandle = GenApi.NodeMapGetNode(nodeMapHandle, symbol);
            if (!nodeHandle.IsValid)
            {
                log.WarnFormat("Could not read Basler property {0}: node handle is not valid. (The property is not supported).", symbol);
                return p;
            }

            EGenApiAccessMode accessMode = GenApi.NodeGetAccessMode(nodeHandle);
            if (accessMode == EGenApiAccessMode._UndefinedAccesMode || accessMode == EGenApiAccessMode.NA || 
                accessMode == EGenApiAccessMode.NI || accessMode == EGenApiAccessMode.WO)
            {
                log.WarnFormat("Could not read Basler property {0}: Access mode not supported. (The property is not readable).", symbol);
                return p;
            }

            EGenApiNodeType type = GenApi.NodeGetType(nodeHandle);
            if (type != EGenApiNodeType.IntegerNode)
            {
                log.WarnFormat("Could not read Basler property {0}: the node is of the wrong type. Expected: Integer. Received:{1}", symbol, type.ToString());
                return p;
            }

            // We don't test for writeable as it's usually dynamic depending on the camera status.
            p.Supported = true;
            p.Type = CameraPropertyType.Integer;
            p.ReadOnly = false;

            long currentValue = GenApi.IntegerGetValue(nodeHandle);
            long min = GenApi.IntegerGetMin(nodeHandle);
            long max = GenApi.IntegerGetMax(nodeHandle);
            long step = GenApi.IntegerGetInc(nodeHandle);
            EGenApiRepresentation repr = GenApi.IntegerGetRepresentation(nodeHandle);
            if (!string.IsNullOrEmpty(symbolMax))
            {
                NODE_HANDLE nodeHandleMax = GenApi.NodeMapGetNode(nodeMapHandle, symbolMax);
                if (nodeHandleMax.IsValid)
                    max = GenApi.IntegerGetValue(nodeHandleMax);
            }

            // Fix values that should be log.
            double range = Math.Log10(max) - Math.Log10(min);
            if (range > 4 && repr == EGenApiRepresentation.Linear)
                repr = EGenApiRepresentation.Logarithmic;

            p.Minimum = min.ToString(CultureInfo.InvariantCulture);
            p.Maximum = max.ToString(CultureInfo.InvariantCulture);
            p.Step = step.ToString(CultureInfo.InvariantCulture);
            p.Representation = ConvertRepresentation(repr);
            p.CurrentValue = currentValue.ToString(CultureInfo.InvariantCulture);

            return p;
        }

        private static CameraProperty ReadFloatProperty(PYLON_DEVICE_HANDLE deviceHandle, string symbol)
        {
            CameraProperty p = new CameraProperty();
            p.Identifier = symbol;
            
            NODEMAP_HANDLE nodeMapHandle = Pylon.DeviceGetNodeMap(deviceHandle);
            NODE_HANDLE nodeHandle = GenApi.NodeMapGetNode(nodeMapHandle, symbol);
            if (!nodeHandle.IsValid)
            {
                log.WarnFormat("Could not read Basler property {0}: node handle is not valid. (The property is not supported).", symbol);
                return p;
            }

            EGenApiAccessMode accessMode = GenApi.NodeGetAccessMode(nodeHandle);
            if (accessMode == EGenApiAccessMode._UndefinedAccesMode || accessMode == EGenApiAccessMode.NA ||
                accessMode == EGenApiAccessMode.NI || accessMode == EGenApiAccessMode.WO)
            {
                log.WarnFormat("Could not read Basler property {0}: Access mode not supported. (The property is not readable).", symbol);
                return p;
            }

            EGenApiNodeType type = GenApi.NodeGetType(nodeHandle);
            if (type != EGenApiNodeType.FloatNode)
            {
                log.WarnFormat("Could not read Basler property {0}: the node is of the wrong type. Expected: Float. Received:{1}", symbol, type.ToString());
                return p;
            }

            p.Supported = true;
            p.Type = CameraPropertyType.Float;
            p.ReadOnly = accessMode != EGenApiAccessMode.RW;

            double min = GenApi.FloatGetMin(nodeHandle);
            double max = GenApi.FloatGetMax(nodeHandle);
            EGenApiRepresentation repr = GenApi.FloatGetRepresentation(nodeHandle);
            double currentValue = GenApi.FloatGetValue(nodeHandle);

            // We don't support a dedicated control for "pure numbers" just use the regular slider.
            if (repr == EGenApiRepresentation.PureNumber)
                repr = EGenApiRepresentation.Linear;

            // Fix values that should be log.
            double range = Math.Log10(max) - Math.Log10(min);
            if (range > 4 && repr == EGenApiRepresentation.Linear)
                repr = EGenApiRepresentation.Logarithmic;

            p.Minimum = min.ToString(CultureInfo.InvariantCulture);
            p.Maximum = max.ToString(CultureInfo.InvariantCulture);
            p.Representation = ConvertRepresentation(repr);
            p.CurrentValue = currentValue.ToString(CultureInfo.InvariantCulture);

            return p;
        }

        private static CameraProperty ReadBooleanProperty(PYLON_DEVICE_HANDLE deviceHandle, string symbol)
        {
            CameraProperty p = new CameraProperty();
            p.Identifier = symbol;

            NODEMAP_HANDLE nodeMapHandle = Pylon.DeviceGetNodeMap(deviceHandle);
            NODE_HANDLE nodeHandle = GenApi.NodeMapGetNode(nodeMapHandle, symbol);
            if (!nodeHandle.IsValid)
            {
                log.WarnFormat("Could not read Basler property {0}: node handle is not valid. (The property is not supported).", symbol);
                return p;
            }

            EGenApiAccessMode accessMode = GenApi.NodeGetAccessMode(nodeHandle);
            if (accessMode == EGenApiAccessMode._UndefinedAccesMode || accessMode == EGenApiAccessMode.NA ||
                accessMode == EGenApiAccessMode.NI || accessMode == EGenApiAccessMode.WO)
            {
                log.WarnFormat("Could not read Basler property {0}: Access mode not supported. (The property is not readable).", symbol);
                return p;
            }

            EGenApiNodeType type = GenApi.NodeGetType(nodeHandle);
            if (type != EGenApiNodeType.BooleanNode)
            {
                log.WarnFormat("Could not read Basler property {0}: the node is of the wrong type. Expected: Boolean. Received:{1}", symbol, type.ToString());
                return p;
            }

            p.Supported = true;
            p.Type = CameraPropertyType.Boolean;
            p.ReadOnly = accessMode != EGenApiAccessMode.RW;

            bool currentValue = GenApi.BooleanGetValue(nodeHandle);

            p.Representation = CameraPropertyRepresentation.Checkbox;
            p.CurrentValue = currentValue.ToString(CultureInfo.InvariantCulture);

            return p;
        }

        private static void WriteCenter(PYLON_DEVICE_HANDLE deviceHandle)
        {
            // Force write the CenterX and CenterY properties if supported.
            // https://docs.baslerweb.com/center-x-and-center-y.html
            // This is apparently required in order for the MaxWidth/MaxHeight properties to behave correctly 
            // and independently of the offset property.
            // To summarize we use the following approach:
            // 1. If CenterX/CenterY are supported properties, we use them and OffsetX/OffsetY will be automated by Pylon.
            // 2. Otherwise we use manually write OffsetX/OffsetY to center the image.
            NODEMAP_HANDLE nodeMapHandle = Pylon.DeviceGetNodeMap(deviceHandle);
            NODE_HANDLE nodeHandleX = GenApi.NodeMapGetNode(nodeMapHandle, "CenterX");
            NODE_HANDLE nodeHandleY = GenApi.NodeMapGetNode(nodeMapHandle, "CenterY");
            if (nodeHandleX.IsValid && nodeHandleY.IsValid)
            {
                EGenApiAccessMode accessModeOffsetX = GenApi.NodeGetAccessMode(nodeHandleX);
                EGenApiAccessMode accessModeOffsetY = GenApi.NodeGetAccessMode(nodeHandleY);
                if (accessModeOffsetX == EGenApiAccessMode.RW && accessModeOffsetY == EGenApiAccessMode.RW)
                {
                    GenApi.BooleanSetValue(nodeHandleX, true);
                    GenApi.BooleanSetValue(nodeHandleY, true);
                }
            }
        }

        /// <summary>
        /// Write either width or height as a centered region of interest.
        /// </summary>
        private static void WriteSize(PYLON_DEVICE_HANDLE deviceHandle, CameraProperty property, string identifierOffset)
        {
            if (property.ReadOnly)
                return;

            NODEMAP_HANDLE nodeMapHandle = Pylon.DeviceGetNodeMap(deviceHandle);
            NODE_HANDLE nodeHandle = GenApi.NodeMapGetNode(nodeMapHandle, property.Identifier);
            if (!nodeHandle.IsValid)
                return;

            EGenApiAccessMode accessMode = GenApi.NodeGetAccessMode(nodeHandle);
            if (accessMode != EGenApiAccessMode.RW)
                return;

            long value = long.Parse(property.CurrentValue, CultureInfo.InvariantCulture);
            long min = GenApi.IntegerGetMin(nodeHandle);
            long max = GenApi.IntegerGetMax(nodeHandle);
            long step = GenApi.IntegerGetInc(nodeHandle);
            
            value = FixValue(value, min, max, step);
            
            // Offset handling.
            // Some cameras have a CenterX/CenterY property.
            // When it is set, the offset is automatic and becomes read-only.
            bool setOffset = false;
            NODE_HANDLE nodeHandleOffset = GenApi.NodeMapGetNode(nodeMapHandle, identifierOffset);
            if (nodeHandleOffset.IsValid)
            {
                EGenApiAccessMode accessModeOffset = GenApi.NodeGetAccessMode(nodeHandleOffset);
                if (accessModeOffset == EGenApiAccessMode.RW)
                    setOffset = true;
            }

            if (setOffset)
            {
                long offset = (max - value) / 2;
                long minOffset = GenApi.IntegerGetMin(nodeHandleOffset);
                long stepOffset = GenApi.IntegerGetInc(nodeHandleOffset);
                long remainderOffset = (offset - minOffset) % stepOffset;
                if (remainderOffset != 0)
                    offset = offset - remainderOffset + stepOffset;

                // We need to be careful with the order and not write a value that doesn't fit due to the offset, or vice versa.
                long currentValue = GenApi.IntegerGetValue(nodeHandle);
                if (value > currentValue)
                {
                    GenApi.IntegerSetValue(nodeHandleOffset, offset);
                    GenApi.IntegerSetValue(nodeHandle, value);
                }
                else
                {
                    GenApi.IntegerSetValue(nodeHandle, value);
                    GenApi.IntegerSetValue(nodeHandleOffset, offset);
                }
            }
            else
            {
                GenApi.IntegerSetValue(nodeHandle, value);
            }
        }

        /// <summary>
        /// Write generic property with optional auto flag.
        /// </summary>
        private static void WriteProperty(PYLON_DEVICE_HANDLE deviceHandle, CameraProperty property)
        {
            if (property.ReadOnly)
                return;

            NODEMAP_HANDLE nodeMapHandle = Pylon.DeviceGetNodeMap(deviceHandle);

            // Switch OFF the auto flag if needed, to be able to write the main property.
            if (!string.IsNullOrEmpty(property.AutomaticIdentifier))
            {
                NODE_HANDLE nodeHandleAuto = GenApi.NodeMapGetNode(nodeMapHandle, property.AutomaticIdentifier);
                if (nodeHandleAuto.IsValid)
                {
                    bool writeable = GenApi.NodeIsWritable(nodeHandleAuto);
                    bool currentAuto = ReadAuto(nodeHandleAuto, property.AutomaticIdentifier);
                    if (writeable && property.CanBeAutomatic && currentAuto && !property.Automatic)
                        WriteAuto(nodeHandleAuto, property.AutomaticIdentifier, false);
                }
            }

            // At this point the auto flag is off. Write the main property.
            NODE_HANDLE nodeHandle = GenApi.NodeMapGetNode(nodeMapHandle, property.Identifier);
            if (!nodeHandle.IsValid)
                return;

            EGenApiAccessMode accessMode = GenApi.NodeGetAccessMode(nodeHandle);
            if (accessMode != EGenApiAccessMode.RW)
                return;

            try
            {
                switch (property.Type)
                {
                    case CameraPropertyType.Integer:
                        {
                            long value = long.Parse(property.CurrentValue, CultureInfo.InvariantCulture);
                            long min = GenApi.IntegerGetMin(nodeHandle);
                            long max = GenApi.IntegerGetMax(nodeHandle);
                            long step = GenApi.IntegerGetInc(nodeHandle);
                            value = FixValue(value, min, max, step);
                            GenApi.IntegerSetValue(nodeHandle, value);
                            break;
                        }
                    case CameraPropertyType.Float:
                        {
                            double value = double.Parse(property.CurrentValue, CultureInfo.InvariantCulture);
                            double min = GenApi.FloatGetMin(nodeHandle);
                            double max = GenApi.FloatGetMax(nodeHandle);
                            value = FixValue(value, min, max);
                            GenApi.FloatSetValue(nodeHandle, value);
                            break;
                        }
                    case CameraPropertyType.Boolean:
                        {
                            bool value = bool.Parse(property.CurrentValue);
                            GenApi.BooleanSetValue(nodeHandle, value);
                            break;
                        }
                    default:
                        break;
                }
            }
            catch
            {
                log.ErrorFormat("Error while writing Basler Pylon GenICam property {0}.", property.Identifier);
            }

            // Finally, switch ON the auto flag if needed.
            if (!string.IsNullOrEmpty(property.AutomaticIdentifier))
            {
                NODE_HANDLE nodeHandleAuto = GenApi.NodeMapGetNode(nodeMapHandle, property.AutomaticIdentifier);
                if (nodeHandleAuto.IsValid && GenApi.NodeIsWritable(nodeHandleAuto) && property.CanBeAutomatic && property.Automatic)
                    WriteAuto(nodeHandleAuto, property.AutomaticIdentifier, true);
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

        private static CameraPropertyRepresentation ConvertRepresentation(EGenApiRepresentation representation)
        {
            switch (representation)
            {
                case EGenApiRepresentation.Linear:
                    return CameraPropertyRepresentation.LinearSlider;
                case EGenApiRepresentation.Logarithmic:
                    return CameraPropertyRepresentation.LogarithmicSlider;
                case EGenApiRepresentation.Boolean:
                    return CameraPropertyRepresentation.Checkbox;
                case EGenApiRepresentation.PureNumber:
                    return CameraPropertyRepresentation.EditBox;
                case EGenApiRepresentation.HexNumber:
                case EGenApiRepresentation._UndefinedRepresentation:
                default:
                    return CameraPropertyRepresentation.Undefined;
            }
        }
        
        /// <summary>
        /// Read the auto property value and put it into a boolean.
        /// </summary>
        private static bool ReadAuto(NODE_HANDLE nodeHandle, string identifier)
        {
            switch (identifier)
            {
                case "AcquisitionFrameRateEnable":
                    {
                        string currentAutoValue = GenApi.BooleanGetValue(nodeHandle).ToString(CultureInfo.InvariantCulture).ToLower();
                        return currentAutoValue == GetAutoTrue(identifier);
                    }
                case "GainAuto":
                case "ExposureAuto":
                default:
                    {
                        string currentAutoValue = GenApi.NodeToString(nodeHandle);
                        return currentAutoValue == GetAutoTrue(identifier);
                    }
            }
        }

        /// <summary>
        /// Takes a boolean of whether auto is ON or OFF, convert it to the correct representation and write it in the auto property.
        /// </summary>
        private static void WriteAuto(NODE_HANDLE nodeHandle, string identifier, bool isAuto)
        {
            string newValue = isAuto ? GetAutoTrue(identifier) : GetAutoFalse(identifier);

            switch (identifier)
            {
                case "AcquisitionFrameRateEnable":
                    {
                        bool newValueBool = bool.Parse(newValue);
                        GenApi.BooleanSetValue(nodeHandle, newValueBool);
                        break;
                    }
                case "GainAuto":
                case "ExposureAuto":
                default:
                    {
                        PylonHelper.WriteEnum(nodeHandle, identifier, newValue);
                        break;
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