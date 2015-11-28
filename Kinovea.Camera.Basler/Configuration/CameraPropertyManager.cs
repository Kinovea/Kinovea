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
            properties.Add("enableFramerate", ReadBooleanProperty(deviceHandle, "AcquisitionFrameRateEnable"));

            if (deviceClass == "BaslerUsb")
            {
                properties.Add("framerate", ReadFloatProperty(deviceHandle, "AcquisitionFrameRate"));
                properties.Add("exposure", ReadFloatProperty(deviceHandle, "ExposureTime"));
                properties.Add("gain", ReadFloatProperty(deviceHandle, "Gain"));
            }
            else
            {
                properties.Add("framerate", ReadFloatProperty(deviceHandle, "AcquisitionFrameRateAbs"));
                properties.Add("exposure", ReadFloatProperty(deviceHandle, "ExposureTimeAbs"));
                properties.Add("gain", ReadIntegerProperty(deviceHandle, "GainRaw"));
            }

            return properties;
        }

        public static void Write(PYLON_DEVICE_HANDLE deviceHandle, CameraProperty property)
        {
            if (!property.Supported || string.IsNullOrEmpty(property.Identifier))
                return;

            NODEMAP_HANDLE nodeMapHandle = Pylon.DeviceGetNodeMap(deviceHandle);
            NODE_HANDLE nodeHandle = GenApi.NodeMapGetNode(nodeMapHandle, property.Identifier);
            if (!nodeHandle.IsValid)
                return;

            EGenApiAccessMode accessMode = GenApi.NodeGetAccessMode(nodeHandle);
            if (accessMode != EGenApiAccessMode.RW)
                return;

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
                        double value = double.Parse(property.CurrentValue, CultureInfo.InvariantCulture);
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

        private static CameraProperty ReadIntegerProperty(PYLON_DEVICE_HANDLE deviceHandle, string symbol)
        {
            CameraProperty p = new CameraProperty();
            p.Identifier = symbol;
            
            NODEMAP_HANDLE nodeMapHandle = Pylon.DeviceGetNodeMap(deviceHandle);
            NODE_HANDLE nodeHandle = GenApi.NodeMapGetNode(nodeMapHandle, symbol);
            if (!nodeHandle.IsValid)
                return p;

            EGenApiAccessMode accessMode = GenApi.NodeGetAccessMode(nodeHandle);
            if (accessMode == EGenApiAccessMode._UndefinedAccesMode || accessMode == EGenApiAccessMode.NA || 
                accessMode == EGenApiAccessMode.NI || accessMode == EGenApiAccessMode.WO)
                return p;

            p.Supported = true;
            p.ReadOnly = accessMode != EGenApiAccessMode.RW;

            EGenApiNodeType type = GenApi.NodeGetType(nodeHandle);
            if (type != EGenApiNodeType.IntegerNode)
                return p;

            p.Type = CameraPropertyType.Integer;

            long min = GenApi.IntegerGetMin(nodeHandle);
            long max = GenApi.IntegerGetMax(nodeHandle);
            long step = GenApi.IntegerGetInc(nodeHandle);
            EGenApiRepresentation repr = GenApi.IntegerGetRepresentation(nodeHandle);
            
            // Fix values that should be log.
            double range = Math.Log(max - min, 10);
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
                return p;

            EGenApiAccessMode accessMode = GenApi.NodeGetAccessMode(nodeHandle);
            if (accessMode == EGenApiAccessMode._UndefinedAccesMode || accessMode == EGenApiAccessMode.NA ||
                accessMode == EGenApiAccessMode.NI || accessMode == EGenApiAccessMode.WO)
                return p;

            p.Supported = true;
            p.ReadOnly = accessMode != EGenApiAccessMode.RW;

            EGenApiNodeType type = GenApi.NodeGetType(nodeHandle);
            if (type != EGenApiNodeType.FloatNode)
                return p;

            p.Type = CameraPropertyType.Float;

            double min = GenApi.FloatGetMin(nodeHandle);
            double max = GenApi.FloatGetMax(nodeHandle);
            EGenApiRepresentation repr = GenApi.FloatGetRepresentation(nodeHandle);
            double currentValue = GenApi.FloatGetValue(nodeHandle);

            // Fix values that should be log.
            double range = Math.Log(max - min, 10);
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
                return p;

            EGenApiAccessMode accessMode = GenApi.NodeGetAccessMode(nodeHandle);
            if (accessMode == EGenApiAccessMode._UndefinedAccesMode || accessMode == EGenApiAccessMode.NA ||
                accessMode == EGenApiAccessMode.NI || accessMode == EGenApiAccessMode.WO)
                return p;

            p.Supported = true;
            p.ReadOnly = accessMode != EGenApiAccessMode.RW;

            EGenApiNodeType type = GenApi.NodeGetType(nodeHandle);
            if (type != EGenApiNodeType.BooleanNode)
                return p;

            p.Type = CameraPropertyType.Boolean;

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