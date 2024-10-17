using System;
using System.Drawing;
using System.Globalization;
using System.Xml;

namespace Kinovea.Services
{
    /// <summary>
    /// A 2D point at a specific timestamp.
    /// </summary>
    public class TimedPoint
    {
        public PointF Point
        {
            get { return new PointF(x, y); }
        }

        /// <summary>
        /// Gets or sets the x-coordinate. 
        /// </summary>
        public float X
        {
            get { return x; }
            set { x = value; }
        }

        /// <summary>
        /// Gets or sets the y-coordinate 
        /// </summary>
        public float Y
        {
            get { return y; }
            set { y = value; }
        }

        /// <summary>
        /// Gets or sets the timestamp.
        /// </summary>
        public long T
        {
            get { return t; }
            set { t = value; }
        }

        /// <summary>
        /// Gets or sets the radius.
        /// </summary>
        public float R
        {
            get { return r; }
            set { r = value; }
        }

        private float x;
        private float y;
        private long t;
        private float r;

        public int ContentHash
        {
            get 
            { 
                return x.GetHashCode() ^ 
                       y.GetHashCode() ^ 
                       t.GetHashCode() ^
                       r.GetHashCode(); 
            }
        }

        public TimedPoint(float x, float y, long t)
        {
            this.x = x;
            this.y = y;
            this.t = t;
            this.r = 0;
        }

        public TimedPoint(float x, float y, long t, float r)
        {
            this.x = x;
            this.y = y;
            this.t = t;
            this.r = r;
        }

        public void WriteXml(XmlWriter xmlWriter)
        {
            xmlWriter.WriteString(String.Format(CultureInfo.InvariantCulture, "{0};{1};{2};{3}", x, y, t, r));
        }
        public void ReadXml(XmlReader xmlReader)
        {
            string xmlString = xmlReader.ReadElementContentAsString();
            string[] split = xmlString.Split(new Char[] { ';' });
            try
            {
                x = float.Parse(split[0], CultureInfo.InvariantCulture);
                y = float.Parse(split[1], CultureInfo.InvariantCulture);
                t = long.Parse(split[2], CultureInfo.InvariantCulture);

                if (split.Length > 3)
                    r = float.Parse(split[3], CultureInfo.InvariantCulture);
                else 
                    r = 0;
            }
            catch (Exception)
            {
                // Conversion issue
                // will default to {0,0,0,0}.
            }
        }
    }
}
