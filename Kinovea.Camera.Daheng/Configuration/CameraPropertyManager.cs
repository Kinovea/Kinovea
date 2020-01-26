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
            ReadSize(featureControl, properties, "Width");
            ReadSize(featureControl, properties, "Height");
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
                WriteInt(featureControl, properties["Width"]);

            if (properties.ContainsKey("Height"))
                WriteInt(featureControl, properties["Height"]);

            if (properties.ContainsKey("ExposureTime"))
                WriteFloat(featureControl, properties["ExposureTime"]);

            if (properties.ContainsKey("AcquisitionFrameRate"))
                WriteFloat(featureControl, properties["AcquisitionFrameRate"]);

            if (properties.ContainsKey("Gain"))
                WriteFloat(featureControl, properties["Gain"]);
        }

        private static void ReadSize(IGXFeatureControl featureControl, Dictionary<string, CameraProperty> properties, string identifier)
        {
            bool isImplemented = featureControl.IsImplemented(identifier);
            bool isReadable = featureControl.IsReadable(identifier);
            bool isWriteable = featureControl.IsWritable(identifier);

            CameraProperty p = new CameraProperty();
            p.Identifier = identifier;
            p.Supported = isImplemented && isReadable;
            p.ReadOnly = !isWriteable;
            p.Type = CameraPropertyType.Integer;

            if (!p.Supported)
                return;

            double value = featureControl.GetIntFeature(identifier).GetValue();
            double min = featureControl.GetIntFeature(identifier).GetMin();
            double max = featureControl.GetIntFeature(identifier).GetMax();
            double step = featureControl.GetIntFeature(identifier).GetInc();

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
            bool isImplemented = featureControl.IsImplemented(identifier);
            bool isReadable = featureControl.IsReadable(identifier);
            bool isWriteable = featureControl.IsWritable(identifier);

            CameraProperty p = new CameraProperty();
            p.Identifier = identifier;
            p.Supported = isImplemented && isReadable;

            p.ReadOnly = !isWriteable;
            p.Type = CameraPropertyType.Float;

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
            
            if (properties != null)
                properties.Add(p.Identifier, p);

            return p;
        }

        private static CameraProperty ReadExposure(IGXFeatureControl featureControl, Dictionary<string, CameraProperty> properties)
        {
            string identifier = "ExposureTime";
            bool isImplemented = featureControl.IsImplemented(identifier);
            bool isReadable = featureControl.IsReadable(identifier);
            
            CameraProperty p = new CameraProperty();
            p.Identifier = identifier;
            p.Supported = isImplemented && isReadable;
            p.ReadOnly = false;
            p.Type = CameraPropertyType.Float;

            if (!p.Supported)
                return p;

            // All values are already in microseconds.
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
            p.CanBeAutomatic = featureControl.IsImplemented(autoIdentifier) && featureControl.IsReadable(autoIdentifier) && featureControl.IsWritable(autoIdentifier);
            if (p.CanBeAutomatic)
            {
                string autoValue = featureControl.GetEnumFeature(autoIdentifier).GetValue();
                p.Automatic = autoValue == "Continuous";
            }

            if (properties != null)
                properties.Add(p.Identifier, p);

            return p;
        }

        private static CameraProperty ReadGain(IGXFeatureControl featureControl, Dictionary<string, CameraProperty> properties)
        {
            string identifier = "Gain";
            bool isImplemented = featureControl.IsImplemented(identifier);
            bool isReadable = featureControl.IsReadable(identifier);

            CameraProperty p = new CameraProperty();
            p.Identifier = identifier;
            p.Supported = isImplemented && isReadable;
            p.ReadOnly = false;
            p.Type = CameraPropertyType.Float;

            if (!p.Supported)
                return p;

            // All values are already in microseconds.
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
            p.CanBeAutomatic = featureControl.IsImplemented(autoIdentifier) && featureControl.IsReadable(autoIdentifier) && featureControl.IsWritable(autoIdentifier);
            if (p.CanBeAutomatic)
            {
                string autoValue = featureControl.GetEnumFeature(autoIdentifier).GetValue();
                p.Automatic = autoValue == "Continuous";
            }

            if (properties != null)
                properties.Add(p.Identifier, p);

            return p;
        }

        private static void WriteFloat(IGXFeatureControl featureControl, CameraProperty property)
        {
            // Commit the auto property first, to flip writeability of the master.
            if (property.CanBeAutomatic)
            {
                if (property.Automatic)
                    featureControl.GetEnumFeature(property.AutomaticIdentifier).SetValue("Continuous");
                else
                    featureControl.GetEnumFeature(property.AutomaticIdentifier).SetValue("Off");
            }

            bool writeable = featureControl.IsWritable(property.Identifier);
            if (!writeable)
                return;
            
            float value = float.Parse(property.CurrentValue, CultureInfo.InvariantCulture);
            featureControl.GetFloatFeature(property.Identifier).SetValue(value);
        }

        private static void WriteInt(IGXFeatureControl featureControl, CameraProperty property)
        {
            bool writeable = featureControl.IsWritable(property.Identifier);
            if (!writeable)
                return;

            int value = int.Parse(property.CurrentValue, CultureInfo.InvariantCulture);
            int min = int.Parse(property.Minimum, CultureInfo.InvariantCulture);
            int step = int.Parse(property.Step, CultureInfo.InvariantCulture);

            int remainder = (value - min) % step;
            if (remainder != 0)
                value = value - remainder + step;

            featureControl.GetIntFeature(property.Identifier).SetValue(value);
        }
    }
}
