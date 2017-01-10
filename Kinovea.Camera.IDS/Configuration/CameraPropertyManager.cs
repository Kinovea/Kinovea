using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Drawing;

namespace Kinovea.Camera.IDS
{
    /// <summary>
    /// Reads and writes a list of supported camera properties from/to the device.
    /// </summary>
    public static class CameraPropertyManager
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static uEye.Camera camera = new uEye.Camera();

        public static Dictionary<string, CameraProperty> Read(uEye.Camera camera, long deviceId)
        {
            Dictionary<string, CameraProperty> properties = new Dictionary<string, CameraProperty>();
            
            // Retrieve camera properties that we support.
            // TODO: some models may not support the basic set of properties we want to expose here.
            ReadSize(camera, properties);
            ReadFramerate(camera, properties);
            ReadExposure(camera, properties);
            ReadGain(camera, properties);
            return properties;
        }

        public static void Write(uEye.Camera camera, long deviceId, CameraProperty property)
        {
            if (!property.Supported || string.IsNullOrEmpty(property.Identifier))
                return;

            try
            {
                switch (property.Identifier)
                {
                    case "framerate":
                        {
                            float value = float.Parse(property.CurrentValue, CultureInfo.InvariantCulture);
                            camera.Timing.Framerate.Set(value);
                            break;
                        }
                    case "exposure":
                        {
                            float value = float.Parse(property.CurrentValue, CultureInfo.InvariantCulture);
                            
                            // Convert back from microseconds to milliseconds.
                            value /= 1000;

                            camera.Timing.Exposure.Set(value);
                            break;
                        }
                    case "gain":
                        {
                            int value = (int)float.Parse(property.CurrentValue, CultureInfo.InvariantCulture);
                            camera.Gain.Hardware.Scaled.SetMaster(value);
                            break;
                        }
                    case "width":
                    case "height":
                        // These properties cannot be changed live.
                        break;
                    default:
                        log.ErrorFormat("IDS uEye property not supported: {0}", property.Identifier);
                        break;
                }
            }
            catch
            {
                log.ErrorFormat("Error while writing IDS uEye property {0}", property.Identifier);
            }
        }

        /// <summary>
        /// Writes the set of properties that can only be written when the device is opened but not streaming yet.
        /// It is assumed that the device is in the correct state when the function is called.
        /// </summary>
        public static void WriteCriticalProperties(uEye.Camera camera, Dictionary<string, CameraProperty> properties)
        {
            if (properties == null || properties.Count == 0)
                return;

            // We actually need to write all the properties again from here.
            // Even framerate, gain and exposure which update in real time would be lost if we don't write them outside of freerun.

            Rectangle rect;
            camera.Size.AOI.Get(out rect);

            if (properties.ContainsKey("width"))
            {
                CameraProperty p = properties["width"];
                int value = int.Parse(p.CurrentValue, CultureInfo.InvariantCulture);
                int step = int.Parse(p.Step, CultureInfo.InvariantCulture);
                int remainder = value % step;
                if (remainder > 0)
                    value = value - remainder;

                if (value != rect.Width)
                {
                    rect.Width = value;
                    camera.Size.AOI.Set(rect);
                }
            }

            if (properties.ContainsKey("height"))
            {
                CameraProperty p = properties["height"];
                int value = int.Parse(p.CurrentValue, CultureInfo.InvariantCulture);
                int step = int.Parse(p.Step, CultureInfo.InvariantCulture);
                int remainder = value % step;
                if (remainder > 0)
                    value = value - remainder;

                if (value != rect.Height)
                {
                    rect.Height = value;
                    camera.Size.AOI.Set(rect);
                }
            }

            if (properties.ContainsKey("framerate"))
            {
                float value = float.Parse(properties["framerate"].CurrentValue, CultureInfo.InvariantCulture);
                camera.Timing.Framerate.Set(value);
            }

            if (properties.ContainsKey("exposure"))
            {
                float value = float.Parse(properties["exposure"].CurrentValue, CultureInfo.InvariantCulture);
                value /= 1000;
                camera.Timing.Exposure.Set(value);
            }
        
            if (properties.ContainsKey("gain"))
            {
                int value = (int)float.Parse(properties["gain"].CurrentValue, CultureInfo.InvariantCulture);
                camera.Gain.Hardware.Scaled.SetMaster(value);
            }
        }

        private static void ReadSize(uEye.Camera camera, Dictionary<string, CameraProperty> properties)
        {
            uEye.Types.Range<Int32> rangeWidth, rangeHeight;
            camera.Size.AOI.GetSizeRange(out rangeWidth, out rangeHeight);

            Rectangle rect;
            camera.Size.AOI.Get(out rect);

            CameraProperty propWidth = new CameraProperty();
            propWidth.Identifier = "width";
            propWidth.Supported = true;
            propWidth.ReadOnly = true;
            propWidth.Type = CameraPropertyType.Integer;
            propWidth.Minimum = rangeWidth.Minimum.ToString(CultureInfo.InvariantCulture);
            propWidth.Maximum = rangeWidth.Maximum.ToString(CultureInfo.InvariantCulture);
            propWidth.Step = rangeWidth.Increment.ToString(CultureInfo.InvariantCulture);
            propWidth.Representation = CameraPropertyRepresentation.LinearSlider;
            propWidth.CurrentValue = rect.Width.ToString(CultureInfo.InvariantCulture);
            
            properties.Add(propWidth.Identifier, propWidth);

            CameraProperty propHeight = new CameraProperty();
            propHeight.Identifier = "height";
            propHeight.Supported = true;
            propHeight.ReadOnly = true;
            propHeight.Type = CameraPropertyType.Integer;
            propHeight.Minimum = rangeHeight.Minimum.ToString(CultureInfo.InvariantCulture);
            propHeight.Maximum = rangeHeight.Maximum.ToString(CultureInfo.InvariantCulture);
            propHeight.Step = rangeHeight.Increment.ToString(CultureInfo.InvariantCulture);
            propHeight.Representation = CameraPropertyRepresentation.LinearSlider;
            propHeight.CurrentValue = rect.Height.ToString(CultureInfo.InvariantCulture);
            
            properties.Add(propHeight.Identifier, propHeight);
        }

        private static void ReadFramerate(uEye.Camera camera, Dictionary<string, CameraProperty> properties)
        {
            uEye.Types.Range<Double> range;
            camera.Timing.Framerate.GetFrameRateRange(out range);

            double currentValue;
            camera.Timing.Framerate.Get(out currentValue);

            CameraProperty p = new CameraProperty();
            p.Identifier = "framerate";
            p.Supported = true;
            p.ReadOnly = true;
            p.Type = CameraPropertyType.Float;
            p.Minimum = range.Minimum.ToString(CultureInfo.InvariantCulture);
            p.Maximum = range.Maximum.ToString(CultureInfo.InvariantCulture);
            p.Step = range.Increment.ToString(CultureInfo.InvariantCulture);
            p.Representation = CameraPropertyRepresentation.LinearSlider;
            p.CurrentValue = currentValue.ToString(CultureInfo.InvariantCulture);

            properties.Add(p.Identifier, p);
        }

        private static void ReadExposure(uEye.Camera camera, Dictionary<string, CameraProperty> properties)
        {
            uEye.Types.Range<Double> range;
            camera.Timing.Exposure.GetRange(out range);
            //camera.Timing.Exposure.Fine.GetRange(out range); // Not supported on test camera.

            double currentValue;
            camera.Timing.Exposure.Get(out currentValue);

            // Switch to microseconds.
            double min = range.Minimum * 1000;
            double max = range.Maximum * 1000;
            double step = range.Increment * 1000;
            double val = currentValue * 1000;

            CameraProperty p = new CameraProperty();
            p.Identifier = "exposure";
            p.Supported = true;
            p.ReadOnly = true;
            p.Type = CameraPropertyType.Float;
            p.Minimum = min.ToString(CultureInfo.InvariantCulture);
            p.Maximum = max.ToString(CultureInfo.InvariantCulture);
            p.Step = step.ToString(CultureInfo.InvariantCulture);
            p.Representation = CameraPropertyRepresentation.LinearSlider;
            p.CurrentValue = val.ToString(CultureInfo.InvariantCulture);

            properties.Add(p.Identifier, p);
        }

        private static void ReadGain(uEye.Camera camera, Dictionary<string, CameraProperty> properties)
        {
            int gain;
            camera.Gain.Hardware.Scaled.GetMaster(out gain);

            CameraProperty p = new CameraProperty();
            p.Identifier = "gain";
            p.Supported = true;
            p.ReadOnly = true;
            p.Type = CameraPropertyType.Float;
            p.Minimum = "0";
            p.Maximum = "100";
            p.Step = "1";
            p.Representation = CameraPropertyRepresentation.LinearSlider;
            p.CurrentValue = gain.ToString(CultureInfo.InvariantCulture);

            properties.Add(p.Identifier, p);
        }
    }
}