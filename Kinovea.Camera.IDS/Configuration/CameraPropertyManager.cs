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
            ReadPixelClock(camera, properties);
            ReadFramerate(camera, properties);
            ReadExposure(camera, properties);
            ReadGain(camera, properties);

            return properties;
        }

        /// <summary>
        /// Read a single property and return it.
        /// This is used in the context of dependent properties, to update the master list with new values.
        /// </summary>
        public static CameraProperty Read(uEye.Camera camera, long deviceId, string key)
        {
            if (key == "pixelclock")
                return ReadPixelClock(camera, null);
            else if (key == "framerate")
                return ReadFramerate(camera, null);
            else if (key == "exposure")
                return ReadExposure(camera, null);
            else if (key == "gain")
                return ReadGain(camera, null);
            else
                return null;
        }

        public static void Write(uEye.Camera camera, long deviceId, CameraProperty property)
        {
            if (!property.Supported || string.IsNullOrEmpty(property.Identifier))
                return;

            try
            {
                switch (property.Identifier)
                {
                    case "width":
                        WriteWidth(camera, property);
                        break;
                    case "height":
                        WriteHeight(camera, property);
                        break;
                    case "pixelclock":
                        WritePixelClock(camera, property);
                        break;
                    case "framerate":
                        WriteFramerate(camera, property);
                        break;
                    case "exposure":
                        WriteExposure(camera, property);
                        break;
                    case "gain":
                        WriteGain(camera, property);
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
        /// Writes the set of properties that can only be written when the device is opened but not streaming.
        /// It is assumed that the device is in the correct state when the function is called.
        /// </summary>
        public static void WriteCriticalProperties(uEye.Camera camera, Dictionary<string, CameraProperty> properties)
        {
            if (properties == null || properties.Count == 0)
                return;

            // We actually need to write all the properties again from here.
            // Even framerate, gain and exposure which update in real time would be lost if we don't write them outside of freerun.
            if (properties.ContainsKey("width"))
                WriteWidth(camera, properties["width"]);

            if (properties.ContainsKey("height"))
                WriteHeight(camera, properties["height"]);

            if (properties.ContainsKey("pixelclock"))
                WritePixelClock(camera, properties["pixelclock"]);

            if (properties.ContainsKey("framerate"))
                WriteFramerate(camera, properties["framerate"]);

            if (properties.ContainsKey("exposure"))
                WriteExposure(camera, properties["exposure"]);
            
            if (properties.ContainsKey("gain"))
                WriteGain(camera, properties["gain"]);
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
            propWidth.ReadOnly = false;
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
            propHeight.ReadOnly = false;
            propHeight.Type = CameraPropertyType.Integer;
            propHeight.Minimum = rangeHeight.Minimum.ToString(CultureInfo.InvariantCulture);
            propHeight.Maximum = rangeHeight.Maximum.ToString(CultureInfo.InvariantCulture);
            propHeight.Step = rangeHeight.Increment.ToString(CultureInfo.InvariantCulture);
            propHeight.Representation = CameraPropertyRepresentation.LinearSlider;
            propHeight.CurrentValue = rect.Height.ToString(CultureInfo.InvariantCulture);
            
            properties.Add(propHeight.Identifier, propHeight);
        }

        private static CameraProperty ReadPixelClock(uEye.Camera camera, Dictionary<string, CameraProperty> properties)
        {
            uEye.Types.Range<Int32> range;
            camera.Timing.PixelClock.GetRange(out range);
            
            Int32 currentValue;
            camera.Timing.PixelClock.Get(out currentValue);

            CameraProperty p = new CameraProperty();
            p.Identifier = "pixelclock";
            p.Supported = true;
            p.ReadOnly = false;
            p.Type = CameraPropertyType.Integer;
            p.Minimum = range.Minimum.ToString(CultureInfo.InvariantCulture);
            p.Maximum = range.Maximum.ToString(CultureInfo.InvariantCulture);
            p.Step = range.Increment.ToString(CultureInfo.InvariantCulture);
            p.Representation = CameraPropertyRepresentation.LinearSlider;
            p.CurrentValue = currentValue.ToString(CultureInfo.InvariantCulture);

            if (properties != null)
                properties.Add(p.Identifier, p);

            return p;
        }

        private static CameraProperty ReadFramerate(uEye.Camera camera, Dictionary<string, CameraProperty> properties)
        {
            uEye.Types.Range<Double> range;
            camera.Timing.Framerate.GetFrameRateRange(out range);

            double currentValue;
            camera.Timing.Framerate.Get(out currentValue);

            CameraProperty p = new CameraProperty();
            p.Identifier = "framerate";
            p.Supported = true;
            p.ReadOnly = false;
            p.Type = CameraPropertyType.Float;
            p.Minimum = range.Minimum.ToString(CultureInfo.InvariantCulture);
            p.Maximum = range.Maximum.ToString(CultureInfo.InvariantCulture);
            p.Step = range.Increment.ToString(CultureInfo.InvariantCulture);
            p.Representation = CameraPropertyRepresentation.LinearSlider;
            p.CurrentValue = currentValue.ToString(CultureInfo.InvariantCulture);

            if (properties != null)
                properties.Add(p.Identifier, p);

            return p;
        }

        private static CameraProperty ReadExposure(uEye.Camera camera, Dictionary<string, CameraProperty> properties)
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
            p.ReadOnly = false;
            p.Type = CameraPropertyType.Float;
            p.Minimum = min.ToString(CultureInfo.InvariantCulture);
            p.Maximum = max.ToString(CultureInfo.InvariantCulture);
            p.Step = step.ToString(CultureInfo.InvariantCulture);
            p.Representation = CameraPropertyRepresentation.LinearSlider;
            p.CurrentValue = val.ToString(CultureInfo.InvariantCulture);

            if (properties != null)
                properties.Add(p.Identifier, p);

            return p;
        }

        private static CameraProperty ReadGain(uEye.Camera camera, Dictionary<string, CameraProperty> properties)
        {
            int gain;
            camera.Gain.Hardware.Scaled.GetMaster(out gain);

            CameraProperty p = new CameraProperty();
            p.Identifier = "gain";
            p.Supported = true;
            p.ReadOnly = false;
            p.Type = CameraPropertyType.Float;
            p.Minimum = "0";
            p.Maximum = "100";
            p.Step = "1";
            p.Representation = CameraPropertyRepresentation.LinearSlider;
            p.CurrentValue = gain.ToString(CultureInfo.InvariantCulture);

            if (properties != null)
                properties.Add(p.Identifier, p);

            return p;
        }

        private static void WriteWidth(uEye.Camera camera, CameraProperty property)
        {
            Rectangle rect;
            camera.Size.AOI.Get(out rect);

            int value = int.Parse(property.CurrentValue, CultureInfo.InvariantCulture);
            int step = int.Parse(property.Step, CultureInfo.InvariantCulture);
            int remainder = value % step;
            if (remainder > 0)
                value = value - remainder;

            if (value != rect.Width)
            {
                rect.Width = value;
                camera.Size.AOI.Set(rect);
            }
        }

        private static void WriteHeight(uEye.Camera camera, CameraProperty property)
        {
            Rectangle rect;
            camera.Size.AOI.Get(out rect);

            int value = int.Parse(property.CurrentValue, CultureInfo.InvariantCulture);
            int step = int.Parse(property.Step, CultureInfo.InvariantCulture);
            int remainder = value % step;
            if (remainder > 0)
                value = value - remainder;

            if (value != rect.Height)
            {
                rect.Height = value;
                camera.Size.AOI.Set(rect);
            }
        }

        private static void WritePixelClock(uEye.Camera camera, CameraProperty property)
        {
            int value = int.Parse(property.CurrentValue, CultureInfo.InvariantCulture);
            camera.Timing.PixelClock.Set(value);
        }

        private static void WriteFramerate(uEye.Camera camera, CameraProperty property)
        {
            float value = float.Parse(property.CurrentValue, CultureInfo.InvariantCulture);
            camera.Timing.Framerate.Set(value);
        }

        private static void WriteExposure(uEye.Camera camera, CameraProperty property)
        {
            float value = float.Parse(property.CurrentValue, CultureInfo.InvariantCulture);
            value /= 1000;
            camera.Timing.Exposure.Set(value);
        }

        private static void WriteGain(uEye.Camera camera, CameraProperty property)
        {
            int value = (int)float.Parse(property.CurrentValue, CultureInfo.InvariantCulture);
            camera.Gain.Hardware.Scaled.SetMaster(value);
        }
    }
}