using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using GxIAPINET;
using System.IO;

namespace Kinovea.Camera.Daheng
{

    /// <summary>
    /// Note on Auto properties:
    /// Switching the corresponding auto property will dynamically impact the writeability of the master prop.
    /// Thus we must not cache the master writeability value.
    /// </summary>
    public static class CameraPropertyManager
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static Dictionary<string, CameraProperty> Read(IGXDevice device)
        {
            Dictionary<string, CameraProperty> properties = new Dictionary<string, CameraProperty>();
            IGXFeatureControl featureControl = device.GetRemoteFeatureControl();

            // Retrieve camera properties we support.
            ReadSize(featureControl, properties, "Width", "WidthMax");
            ReadSize(featureControl, properties, "Height", "HeightMax");
            ReadFramerate(featureControl, properties);
            ReadExposure(featureControl, properties);
            ReadGain(featureControl, properties);
            
            return properties;
        }

        public static void Write(IGXDevice device, CameraProperty property)
        {
            if (!property.Supported || string.IsNullOrEmpty(property.Identifier))
                return;

            // Only write non critical properties: properties that don't change image size.

            IGXFeatureControl featureControl = device.GetRemoteFeatureControl();

            try
            {
                switch (property.Identifier)
                {
                    case "AcquisitionFrameRate":
                        WriteFloat(featureControl, property);
                        break;
                    case "ExposureTime":
                        WriteFloat(featureControl, property);
                        break;
                    case "Gain":
                        WriteFloat(featureControl, property);
                        break;
                    case "Width":
                    case "Height":
                        // Do nothing. These properties must be changed from WriteCriticalProperties below.
                        break;
                    default:
                        log.ErrorFormat("Daheng property not supported: {0}.", property.Identifier);
                        break;
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("Error while writing Daheng property {0}. {1}", property.Identifier, e.Message);
            }
        }

        public static void WriteCriticalProperties(IGXDevice device, Dictionary<string, CameraProperty> properties)
        {
            if (properties == null || properties.Count == 0)
                return;

            IGXFeatureControl featureControl = device.GetRemoteFeatureControl();

            // We need to write all the properties again from here, since the range of possible values depends on image size.
            if (properties.ContainsKey("Width"))
                WriteSize(featureControl, properties["Width"], "OffsetX");

            if (properties.ContainsKey("Height"))
                WriteSize(featureControl, properties["Height"], "OffsetY");

            if (properties.ContainsKey("ExposureTime"))
                WriteFloat(featureControl, properties["ExposureTime"]);

            if (properties.ContainsKey("AcquisitionFrameRate"))
                WriteFloat(featureControl, properties["AcquisitionFrameRate"]);

            if (properties.ContainsKey("Gain"))
                WriteFloat(featureControl, properties["Gain"]);
        }

        private static void ReadSize(IGXFeatureControl featureControl, Dictionary<string, CameraProperty> properties, string identifier, string identifierMax)
        {
            CameraProperty p = new CameraProperty();
            p.Identifier = identifier;
            p.Supported = false;
            p.Type = CameraPropertyType.Integer;

            bool isImplemented = featureControl.IsImplemented(identifier);
            if (!isImplemented)
                return;

            // We don't test for writeable as it's dynamic depending on the camera status.
            bool isReadable = featureControl.IsReadable(identifier);
            p.Supported = isReadable;
            p.ReadOnly = false;

            if (!p.Supported)
                return;

            double value = featureControl.GetIntFeature(identifier).GetValue();
            double min = featureControl.GetIntFeature(identifier).GetMin();
            double max = featureControl.GetIntFeature(identifier).GetMax();
            double step = featureControl.GetIntFeature(identifier).GetInc();

            if (featureControl.IsImplemented(identifierMax) && featureControl.IsReadable(identifierMax))
                max = featureControl.GetIntFeature(identifierMax).GetValue();

            p.Minimum = min.ToString(CultureInfo.InvariantCulture);
            p.Maximum = max.ToString(CultureInfo.InvariantCulture);
            p.Step = step.ToString(CultureInfo.InvariantCulture);
            p.Representation = CameraPropertyRepresentation.LinearSlider;
            p.CurrentValue = value.ToString(CultureInfo.InvariantCulture);

            properties.Add(p.Identifier, p);
        }

        private static CameraProperty ReadFramerate(IGXFeatureControl featureControl, Dictionary<string, CameraProperty> properties)
        {
            string identifier = "AcquisitionFrameRate";
            CameraProperty p = new CameraProperty();
            p.Identifier = identifier;
            p.Supported = false;
            p.Type = CameraPropertyType.Float;

            bool isImplemented = featureControl.IsImplemented(identifier);
            if (!isImplemented)
                return p;

            bool isReadable = featureControl.IsReadable(identifier);
            p.ReadOnly = false;
            p.Supported = isReadable;
            if (!p.Supported)
                return p;

            double value = featureControl.GetFloatFeature(identifier).GetValue();
            double min = featureControl.GetFloatFeature(identifier).GetMin();
            double max = featureControl.GetFloatFeature(identifier).GetMax();
            double step = 1;
            min = Math.Max(1.0, min);

            p.CurrentValue = value.ToString(CultureInfo.InvariantCulture);
            p.Minimum = min.ToString(CultureInfo.InvariantCulture);
            p.Maximum = max.ToString(CultureInfo.InvariantCulture);
            p.Step = step.ToString(CultureInfo.InvariantCulture);

            double range = Math.Log10(max) - Math.Log10(min);
            p.Representation = (range >= 4) ? CameraPropertyRepresentation.LogarithmicSlider : CameraPropertyRepresentation.LinearSlider;

            // AcquisitionFrameRateMode=Off: the framerate is automatically set to the max value possible.
            // AcquisitionFrameRateMode=On: use the custom framerate set by the user in AcquisitionFrameRate.
            
            string autoIdentifier = "AcquisitionFrameRateMode";
            p.AutomaticIdentifier = autoIdentifier;
            p.CanBeAutomatic = featureControl.IsImplemented(autoIdentifier);
            p.Automatic = false;
            if (p.CanBeAutomatic && featureControl.IsReadable(autoIdentifier))
            {
                string autoValue = featureControl.GetEnumFeature(autoIdentifier).GetValue();
                p.Automatic = autoValue == GetAutoTrue(autoIdentifier);
            }

            if (properties != null)
                properties.Add(p.Identifier, p);

            return p;
        }

        private static CameraProperty ReadExposure(IGXFeatureControl featureControl, Dictionary<string, CameraProperty> properties)
        {
            string identifier = "ExposureTime";
            CameraProperty p = new CameraProperty();
            p.Identifier = identifier;
            p.Supported = false;
            p.Type = CameraPropertyType.Float;

            bool isImplemented = featureControl.IsImplemented(identifier);
            if (!isImplemented)
                return p;

            // We can't know if the feature is writeable at this point. Assume it is and test before write.
            bool isReadable = featureControl.IsReadable(identifier);
            p.ReadOnly = false;
            p.Supported = isReadable;
            if (!p.Supported)
                return p;

            double value = featureControl.GetFloatFeature(identifier).GetValue();
            double min = featureControl.GetFloatFeature(identifier).GetMin();
            double max = featureControl.GetFloatFeature(identifier).GetMax();
            double step = 1;

            p.CurrentValue = value.ToString(CultureInfo.InvariantCulture);
            p.Minimum = min.ToString(CultureInfo.InvariantCulture);
            p.Maximum = max.ToString(CultureInfo.InvariantCulture);
            p.Step = step.ToString(CultureInfo.InvariantCulture);

            p.Representation = CameraPropertyRepresentation.LogarithmicSlider;

            string autoIdentifier = "ExposureAuto";
            p.AutomaticIdentifier = autoIdentifier;
            p.CanBeAutomatic = featureControl.IsImplemented(autoIdentifier);
            p.Automatic = false;
            if (p.CanBeAutomatic && featureControl.IsReadable(autoIdentifier))
            {
                string autoValue = featureControl.GetEnumFeature(autoIdentifier).GetValue();
                p.Automatic = autoValue == GetAutoTrue(autoIdentifier);
            }
            
            if (properties != null)
                properties.Add(p.Identifier, p);

            return p;
        }

        private static CameraProperty ReadGain(IGXFeatureControl featureControl, Dictionary<string, CameraProperty> properties)
        {
            string identifier = "Gain";
            CameraProperty p = new CameraProperty();
            p.Identifier = identifier;
            p.Supported = false;
            p.Type = CameraPropertyType.Float;

            bool isImplemented = featureControl.IsImplemented(identifier);
            if (!isImplemented)
                return p;

            bool isReadable = featureControl.IsReadable(identifier);
            p.ReadOnly = false;
            p.Supported = isReadable;
            if (!p.Supported)
                return p;

            double value = featureControl.GetFloatFeature(identifier).GetValue();
            double min = featureControl.GetFloatFeature(identifier).GetMin();
            double max = featureControl.GetFloatFeature(identifier).GetMax();
            double step = 1;

            p.CurrentValue = value.ToString(CultureInfo.InvariantCulture);
            p.Minimum = min.ToString(CultureInfo.InvariantCulture);
            p.Maximum = max.ToString(CultureInfo.InvariantCulture);
            p.Step = step.ToString(CultureInfo.InvariantCulture);

            p.Representation = CameraPropertyRepresentation.LinearSlider;

            string autoIdentifier = "GainAuto";
            p.AutomaticIdentifier = autoIdentifier;
            p.CanBeAutomatic = featureControl.IsImplemented(autoIdentifier);
            p.Automatic = false;
            if (p.CanBeAutomatic && featureControl.IsReadable(autoIdentifier))
            {
                string autoValue = featureControl.GetEnumFeature(autoIdentifier).GetValue();
                p.Automatic = autoValue == GetAutoTrue(autoIdentifier);
            }

            if (properties != null)
                properties.Add(p.Identifier, p);

            return p;
        }

        private static void WriteFloat(IGXFeatureControl featureControl, CameraProperty property)
        {
            if (property.ReadOnly)
                return;

            // If auto is switching OFF, we need to set it off before the main prop to make it writeable.
            // If auto is switching ON, we need to set it ON after the main prop, otherwise it gets turned Off automatically.
            string currentAutoValue = featureControl.GetEnumFeature(property.AutomaticIdentifier).GetValue();
            bool currentAuto = currentAutoValue == GetAutoTrue(property.AutomaticIdentifier);

            if (property.CanBeAutomatic && currentAuto && !property.Automatic && featureControl.IsWritable(property.AutomaticIdentifier))
                featureControl.GetEnumFeature(property.AutomaticIdentifier).SetValue(GetAutoFalse(property.AutomaticIdentifier));

            float value = float.Parse(property.CurrentValue, CultureInfo.InvariantCulture);
            if (featureControl.IsWritable(property.Identifier))
                featureControl.GetFloatFeature(property.Identifier).SetValue(value);

            if (property.CanBeAutomatic && !currentAuto && property.Automatic && featureControl.IsWritable(property.AutomaticIdentifier))
                featureControl.GetEnumFeature(property.AutomaticIdentifier).SetValue(GetAutoTrue(property.AutomaticIdentifier));
        }

        private static void WriteSize(IGXFeatureControl featureControl, CameraProperty property, string identifierOffset)
        {
            if (property.ReadOnly)
                return;

            bool writeable = featureControl.IsWritable(property.Identifier);
            bool writeableOffset = featureControl.IsWritable(identifierOffset);
            if (!writeable || !writeableOffset)
                return;

            int value = int.Parse(property.CurrentValue, CultureInfo.InvariantCulture);
            int min = int.Parse(property.Minimum, CultureInfo.InvariantCulture);
            int max = int.Parse(property.Maximum, CultureInfo.InvariantCulture);
            int step = int.Parse(property.Step, CultureInfo.InvariantCulture);

            int remainder = (value - min) % step;
            if (remainder != 0)
                value = value - remainder + step;

            int offset = (max - value) / 2;
            int minOffset = (int)featureControl.GetIntFeature(identifierOffset).GetMin();
            int stepOffset = (int)featureControl.GetIntFeature(identifierOffset).GetInc();

            int remainderOffset = (offset - minOffset) % stepOffset;
            if (remainderOffset != 0)
                offset = offset - remainderOffset + stepOffset;

            // We need to be careful not to write the value if it doesn't fit, due to the offset.
            int currentValue = (int)featureControl.GetIntFeature(property.Identifier).GetValue();
            if (value > currentValue)
            {
                featureControl.GetIntFeature(identifierOffset).SetValue(offset);
                featureControl.GetIntFeature(property.Identifier).SetValue(value);
            }
            else
            {
                featureControl.GetIntFeature(property.Identifier).SetValue(value);
                featureControl.GetIntFeature(identifierOffset).SetValue(offset);
            }
        }
        
        private static string GetAutoTrue(string identifier)
        {
            switch (identifier)
            {
                case "AcquisitionFrameRateMode":
                    return "Off";
                case "ExposureAuto":
                case "GainAuto":
                default:
                    return "Continuous";
            }
        }

        private static string GetAutoFalse(string identifier)
        {
            switch (identifier)
            {
                case "AcquisitionFrameRateMode":
                    return "On";
                case "ExposureAuto":
                case "GainAuto":
                default:
                    return "Off";
            }
        }
    }
}
