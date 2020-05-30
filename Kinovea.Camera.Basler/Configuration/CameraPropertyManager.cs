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

            properties.Add("width", ReadIntegerProperty(deviceHandle, "Width"));
            properties.Add("height", ReadIntegerProperty(deviceHandle, "Height"));

            // Camera properties in Kinovea combine the value and the "auto" flag.
            // We potentially need to read several Basler camera properties to create one Kinovea camera property.
            ReadFramerate(deviceHandle, properties);
            ReadExposure(deviceHandle, properties);
            ReadGain(deviceHandle, properties);
            
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
                    case "AcquisitionFrameRateEnable":
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

        private static void ReadFramerate(PYLON_DEVICE_HANDLE deviceHandle, Dictionary<string, CameraProperty> properties)
        {
            properties.Add("enableFramerate", ReadBooleanProperty(deviceHandle, "AcquisitionFrameRateEnable"));

            CameraProperty prop = ReadFloatProperty(deviceHandle, "AcquisitionFrameRate");
            if (!prop.Supported)
                prop = ReadFloatProperty(deviceHandle, "AcquisitionFrameRateAbs");

            properties.Add("framerate", prop);
        }

        private static void ReadExposure(PYLON_DEVICE_HANDLE deviceHandle, Dictionary<string, CameraProperty> properties)
        {
            CameraProperty prop = ReadFloatProperty(deviceHandle, "ExposureTime");
            if (!prop.Supported)
                prop = ReadFloatProperty(deviceHandle, "ExposureTimeAbs");

            prop.CanBeAutomatic = true;
            prop.AutomaticIdentifier = "ExposureAuto";
            GenApiEnum auto = PylonHelper.ReadEnumCurrentValue(deviceHandle, prop.AutomaticIdentifier);
            prop.Automatic = auto != null && auto.Symbol == "Continuous";
            properties.Add("exposure", prop);
        }

        private static void ReadGain(PYLON_DEVICE_HANDLE deviceHandle, Dictionary<string, CameraProperty> properties)
        {
            CameraProperty prop = ReadFloatProperty(deviceHandle, "Gain");
            if (!prop.Supported)
                prop = ReadIntegerProperty(deviceHandle, "GainRaw");

            prop.CanBeAutomatic = true;
            prop.AutomaticIdentifier = "GainAuto";
            GenApiEnum auto = PylonHelper.ReadEnumCurrentValue(deviceHandle, prop.AutomaticIdentifier);
            prop.Automatic = auto != null && auto.Symbol == "Continuous";
            properties.Add("gain", prop);
        }

        private static CameraProperty ReadIntegerProperty(PYLON_DEVICE_HANDLE deviceHandle, string symbol)
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

            long min = GenApi.IntegerGetMin(nodeHandle);
            long max = GenApi.IntegerGetMax(nodeHandle);
            long step = GenApi.IntegerGetInc(nodeHandle);
            EGenApiRepresentation repr = GenApi.IntegerGetRepresentation(nodeHandle);

            // Fix values that should be log.
            double range = Math.Log10(max) - Math.Log10(min);
            if (range > 4 && repr == EGenApiRepresentation.Linear)
                repr = EGenApiRepresentation.Logarithmic;

            long currentValue = GenApi.IntegerGetValue(nodeHandle);

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

        /// <summary>
        /// Write either width or height as a centered region of interest.
        /// </summary>
        private static void WriteSize(PYLON_DEVICE_HANDLE deviceHandle, CameraProperty property, string identifierOffset)
        {
            if (property.ReadOnly)
                return;

            NODEMAP_HANDLE nodeMapHandle = Pylon.DeviceGetNodeMap(deviceHandle);
            NODE_HANDLE nodeHandle = GenApi.NodeMapGetNode(nodeMapHandle, property.Identifier);
            NODE_HANDLE nodeHandleOffset = GenApi.NodeMapGetNode(nodeMapHandle, identifierOffset);
            if (!nodeHandle.IsValid || !nodeHandleOffset.IsValid)
                return;

            EGenApiAccessMode accessMode = GenApi.NodeGetAccessMode(nodeHandle);
            EGenApiAccessMode accessModeOffset = GenApi.NodeGetAccessMode(nodeHandleOffset);
            if (accessMode != EGenApiAccessMode.RW || accessModeOffset != EGenApiAccessMode.RW)
                return;

            int value = int.Parse(property.CurrentValue, CultureInfo.InvariantCulture);
            int min = int.Parse(property.Minimum, CultureInfo.InvariantCulture);
            int max = int.Parse(property.Maximum, CultureInfo.InvariantCulture);
            int step = int.Parse(property.Step, CultureInfo.InvariantCulture);

            int remainder = (value - min) % step;
            if (remainder != 0)
                value = value - remainder + step;

            int offset = (max - value) / 2;
            int minOffset = (int)GenApi.IntegerGetMin(nodeHandleOffset);
            int stepOffset = (int)GenApi.IntegerGetInc(nodeHandleOffset);

            int remainderOffset = (offset - minOffset) % stepOffset;
            if (remainderOffset != 0)
                offset = offset - remainderOffset + stepOffset;

            // We need to be careful with the order and not write a value that doesn't fit due to the offset, or vice versa.
            int currentValue = (int)GenApi.IntegerGetValue(nodeHandle);
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

        /// <summary>
        /// Write generic property with optional auto flag.
        /// </summary>
        private static void WriteProperty(PYLON_DEVICE_HANDLE deviceHandle, CameraProperty property)
        {
            if (property.ReadOnly)
                return;

            // Switch OFF the auto flag if needed, to be able to write the main property.
            bool currentAuto = false;
            if (!string.IsNullOrEmpty(property.AutomaticIdentifier))
            {
                GenApiEnum currentAutoValue = PylonHelper.ReadEnumCurrentValue(deviceHandle, property.AutomaticIdentifier);
                currentAuto = currentAutoValue != null && currentAutoValue.Symbol == "Continuous";

                if (property.CanBeAutomatic && currentAuto && !property.Automatic)
                    PylonHelper.WriteEnum(deviceHandle, property.AutomaticIdentifier, "Off");
            }

            // At this point the auto flag is off.
            NODEMAP_HANDLE nodeMapHandle = Pylon.DeviceGetNodeMap(deviceHandle);
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
                            long step = long.Parse(property.Step, CultureInfo.InvariantCulture);
                            long remainder = value % step;
                            if (remainder > 0)
                                value = value - remainder;

                            GenApi.IntegerSetValue(nodeHandle, value);
                            break;
                        }
                    case CameraPropertyType.Float:
                        {
                            double max = GenApi.FloatGetMax(nodeHandle);
                            double min = GenApi.FloatGetMin(nodeHandle);
                            double value = double.Parse(property.CurrentValue, CultureInfo.InvariantCulture);
                            value = Math.Min(Math.Max(value, min), max);

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
                if (property.CanBeAutomatic && property.Automatic)
                    PylonHelper.WriteEnum(deviceHandle, property.AutomaticIdentifier, "Continuous");
            }
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
        

    
    }
}