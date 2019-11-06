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

            string deviceClass = "BaslerGigE";
            try
            {
                deviceClass = Pylon.DeviceInfoGetPropertyValueByName(device.DeviceInfoHandle, Pylon.cPylonDeviceInfoDeviceClassKey);
            }
            catch
            {
                log.ErrorFormat("Could not read Basler device class. Assuming BaslerGigE.");
            }

            properties.Add("width", ReadIntegerProperty(deviceHandle, "Width"));
            properties.Add("height", ReadIntegerProperty(deviceHandle, "Height"));

            // Camera properties in Kinovea combine the value and the "auto" flag.
            // We potentially need to read several Basler camera properties to create one Kinovea camera property.
            // Furthermore, some properties name or type depends on whether the camera is USB or GigE.
            ReadFramerate(deviceHandle, deviceClass, properties);
            ReadExposure(deviceHandle, deviceClass, properties);
            ReadGain(deviceHandle, deviceClass, properties);
            
            return properties;
        }

        public static void Write(PYLON_DEVICE_HANDLE deviceHandle, CameraProperty property)
        {
            if (!property.Supported || string.IsNullOrEmpty(property.Identifier) || !deviceHandle.IsValid)
                return;

            // If "auto" flag is OFF we should write it first. On some cameras the value is not writable until the corresponding auto flag is off.
            // If it's ON (continuous), it doesn't matter as our value will be overwritten soon anyway.
            if (!string.IsNullOrEmpty(property.AutomaticIdentifier))
            {
                string enumValue = property.Automatic ? "Continuous" : "Off";
                PylonHelper.WriteEnum(deviceHandle, property.AutomaticIdentifier, enumValue);
            }

            NODEMAP_HANDLE nodeMapHandle = Pylon.DeviceGetNodeMap(deviceHandle);
            NODE_HANDLE nodeHandle = GenApi.NodeMapGetNode(nodeMapHandle, property.Identifier);
            if (!nodeHandle.IsValid)
                return;

            EGenApiAccessMode accessMode = GenApi.NodeGetAccessMode(nodeHandle);
            if (accessMode != EGenApiAccessMode.RW)
            {
                if (!string.IsNullOrEmpty(property.AutomaticIdentifier) && !property.Automatic)
                {
                    log.ErrorFormat("Error while writing Basler Pylon GenICam property {0}", property.Identifier);
                    log.ErrorFormat("The property is not writable.");
                }

                return;
            }

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
                log.ErrorFormat("Error while writing Basler Pylon GenICam property {0}", property.Identifier);
            }
        }

        private static void ReadFramerate(PYLON_DEVICE_HANDLE deviceHandle, string deviceClass, Dictionary<string, CameraProperty> properties)
        {
            properties.Add("enableFramerate", ReadBooleanProperty(deviceHandle, "AcquisitionFrameRateEnable"));

            if (deviceClass == "BaslerUsb")
                properties.Add("framerate", ReadFloatProperty(deviceHandle, "AcquisitionFrameRate"));
            else
                properties.Add("framerate", ReadFloatProperty(deviceHandle, "AcquisitionFrameRateAbs"));
        }

        private static void ReadExposure(PYLON_DEVICE_HANDLE deviceHandle, string deviceClass, Dictionary<string, CameraProperty> properties)
        {
            CameraProperty prop = null;
            if (deviceClass == "BaslerUsb")
                prop = ReadFloatProperty(deviceHandle, "ExposureTime");
            else
                prop = ReadFloatProperty(deviceHandle, "ExposureTimeAbs");

            prop.CanBeAutomatic = true;
            prop.AutomaticIdentifier = "ExposureAuto";
            GenApiEnum auto = PylonHelper.ReadEnumCurrentValue(deviceHandle, prop.AutomaticIdentifier);
            prop.Automatic = auto != null && auto.Symbol == "Continuous";
            properties.Add("exposure", prop);
        }

        private static void ReadGain(PYLON_DEVICE_HANDLE deviceHandle, string deviceClass, Dictionary<string, CameraProperty> properties)
        {
            CameraProperty prop = null;
            if (deviceClass == "BaslerUsb")
                prop = ReadFloatProperty(deviceHandle, "Gain");
            else
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

            p.Supported = true;
            p.Type = CameraPropertyType.Integer;
            p.ReadOnly = accessMode != EGenApiAccessMode.RW;

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