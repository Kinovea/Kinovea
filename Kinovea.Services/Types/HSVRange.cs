using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Kinovea.Services
{
    /// <summary>
    /// Min-Max range for HSV values.
    /// </summary>
    public class HSVRange
    {
        public float HueMin { get; set; }
        public float HueMax { get; set; }
        public float SaturationMin { get; set; }
        public float SaturationMax { get; set; }
        public float ValueMin { get; set; }
        public float ValueMax { get; set; }

        public int ContentHash
        {
            get
            {
                int hash = 0;
                hash ^= HueMin.GetHashCode();
                hash ^= HueMax.GetHashCode();
                hash ^= SaturationMin.GetHashCode();
                hash ^= SaturationMax.GetHashCode();
                hash ^= ValueMin.GetHashCode();
                hash ^= ValueMax.GetHashCode();
                return hash;
            }
        }

        public HSVRange()
        {
            this.HueMin = 0;
            this.HueMax = 360;
            this.SaturationMin = 0;
            this.SaturationMax = 255;
            this.ValueMin = 0;
            this.ValueMax = 255;
        }

        public HSVRange(float hueMin, float hueMax, float saturationMin, float saturationMax, float valueMin, float valueMax)
        {
            this.HueMin = hueMin;
            this.HueMax = hueMax;
            this.SaturationMin = saturationMin;
            this.SaturationMax = saturationMax;
            this.ValueMin = valueMin;
            this.ValueMax = valueMax;
        }

        public HSVRange Clone()
        {
            return new HSVRange(this.HueMin, this.HueMax, this.SaturationMin, this.SaturationMax, this.ValueMin, this.ValueMax);
        }

        public override string ToString()
        {
            return string.Format("Hue: {0}-{1}, Saturation: {2}-{3}, Value: {4}-{5}",
                this.HueMin, this.HueMax, this.SaturationMin, this.SaturationMax, this.ValueMin, this.ValueMax);
        }


        public void WriteXml(XmlWriter xmlWriter)
        {
            xmlWriter.WriteString(String.Format(CultureInfo.InvariantCulture, "{0};{1};{2};{3};{4};{5}",
                this.HueMin, this.HueMax, this.SaturationMin, this.SaturationMax, this.ValueMin, this.ValueMax));
                
        }

        public void ReadXml(XmlReader xmlReader)
        {
            string xmlString = xmlReader.ReadElementContentAsString();
            string[] split = xmlString.Split(new Char[] { ';' });
            try
            {
                this.HueMin = float.Parse(split[0], CultureInfo.InvariantCulture);
                this.HueMax = float.Parse(split[1], CultureInfo.InvariantCulture);
                this.SaturationMin = float.Parse(split[2], CultureInfo.InvariantCulture);
                this.SaturationMax = float.Parse(split[3], CultureInfo.InvariantCulture);
                this.ValueMin = float.Parse(split[4], CultureInfo.InvariantCulture);
                this.ValueMax = float.Parse(split[5], CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                // Conversion issue
                // will default to {0,0,0,0}.
            }
        }
    }
}
