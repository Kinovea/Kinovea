using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.Camera.FrameGenerator
{
    public static class CameraPropertyManager
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static Dictionary<string, CameraProperty> Read(SpecificInfo info)
        {
            Dictionary<string, CameraProperty> properties = new Dictionary<string, CameraProperty>();

            ReadInteger(properties, "width", "1", "4096", "1", false, info.Width.ToString());
            ReadInteger(properties, "height", "1", "4096", "1", false, info.Height.ToString());
            ReadInteger(properties, "framerate", "1", "9999", "1", true, info.Framerate.ToString());

            return properties;
        }

        private static void ReadInteger(Dictionary<string, CameraProperty> properties, string id, string min, string max, string step, bool log, string value)
        {
            CameraProperty prop = new CameraProperty();
            prop.Identifier = id;
            prop.Supported = true;
            prop.ReadOnly = false;
            prop.Type = CameraPropertyType.Integer;
            prop.Minimum = min;
            prop.Maximum = max;
            prop.Step = step;
            prop.Representation = log ? CameraPropertyRepresentation.LogarithmicSlider : CameraPropertyRepresentation.LinearSlider;
            prop.CurrentValue = value;
            properties.Add(prop.Identifier, prop);
        }

        public static void Write(SpecificInfo info, CameraProperty property)
        {
            if (!property.Supported || string.IsNullOrEmpty(property.Identifier))
                return;

            switch (property.Identifier)
            {
                case "width":
                    info.Width = int.Parse(property.CurrentValue);
                    break;
                case "height":
                    info.Height = int.Parse(property.CurrentValue);
                    break;
                case "framerate":
                    info.Framerate = int.Parse(property.CurrentValue);
                    break;
                default:
                    log.ErrorFormat("Camera simulator property not supported: {0}.", property.Identifier);
                    break;
            }
        }

    }
}
