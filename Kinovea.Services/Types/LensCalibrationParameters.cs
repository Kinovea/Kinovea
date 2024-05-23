using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Globalization;
using System.ComponentModel;

namespace Kinovea.Services
{
    /// <summary>
    /// Parameters controlling the Lens calibration algorithm.
    /// These are saved in the preferences, not in the KVA.
    /// </summary>
    public class LensCalibrationParameters
    {
        #region Properties
        /// <summary>
        /// Number of images to use.
        /// </summary>
        public int MaxImages { get; set; } = 12;
        
        /// <summary>
        /// Number of squares of the checkerboard pattern.
        /// </summary>
        public Size PatternSize { get; set; } = new Size(10, 7);

        /// <summary>
        /// Number of iterations for the solve.
        /// </summary>
        public int MaxIterations { get; set; } = 30;
        #endregion

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Returns a deep clone of this object.
        /// </summary>
        public LensCalibrationParameters Clone()
        {
            LensCalibrationParameters clone = new LensCalibrationParameters();
            clone.MaxImages = this.MaxImages;
            clone.PatternSize = this.PatternSize;
            clone.MaxIterations = this.MaxIterations;
            return clone;
        }

        public int GetContentHash()
        {
            int hash = 0;
            hash ^= MaxImages.GetHashCode();
            hash ^= PatternSize.GetHashCode();
            hash ^= MaxIterations.GetHashCode();
            return hash;
        }

        #region Serialization
        public void ReadXml(XmlReader r)
        {
            r.ReadStartElement();

            while (r.NodeType == XmlNodeType.Element)
            {
                switch (r.Name)
                {
                    case "MaxImages":
                        MaxImages = int.Parse(r.ReadElementContentAsString(), CultureInfo.InvariantCulture);
                        break;
                    case "PatternSize":
                        PatternSize = XmlHelper.ParseSize(r.ReadElementContentAsString());
                        break;
                    case "MaxIterations":
                        MaxIterations = int.Parse(r.ReadElementContentAsString(), CultureInfo.InvariantCulture);
                        break;
                    default:
                        string outerXml = r.ReadOuterXml();
                        log.DebugFormat("Unparsed content in XML: {0}", outerXml);
                        break;
                }
            }

            r.ReadEndElement();
        }

        public void WriteXml(XmlWriter w)
        {
            w.WriteElementString("MaxImages", MaxImages.ToString(CultureInfo.InvariantCulture));
            w.WriteElementString("PatternSize", XmlHelper.WriteSize(PatternSize));
            w.WriteElementString("MaxIterations", MaxIterations.ToString(CultureInfo.InvariantCulture));
        }
        #endregion
    }
}
