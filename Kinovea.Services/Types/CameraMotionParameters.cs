using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Globalization;

namespace Kinovea.Services
{
    /// <summary>
    /// Parameters used in the Camera motion video filter.
    /// </summary>
    public class CameraMotionParameters
    {
        #region Properties
        
        /// <summary>
        /// Whether the action menu shows individual steps or a single "Run" command.
        /// </summary>
        public bool StepByStep { get; set; } = true;

        /// <summary>
        /// Maximum number of features per frame.
        /// </summary>
        public int FeaturesPerFrame { get; set; } = 2048;

        /// <summary>
        /// Threshold to consider that a point is an inlier during 
        /// robust motion estimation.
        /// </summary>
        public float RansacReprojThreshold { get; set; } = 1.25f;
        #endregion

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public CameraMotionParameters Clone()
        {
            CameraMotionParameters clone = new CameraMotionParameters();
            clone.FeaturesPerFrame = this.FeaturesPerFrame;
            clone.RansacReprojThreshold = this.RansacReprojThreshold;
            return clone;
        }

        public int GetContentHash()
        {
            int hash = 0;
            hash ^= StepByStep.GetHashCode();
            hash ^= FeaturesPerFrame.GetHashCode();
            hash ^= RansacReprojThreshold.GetHashCode();
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
                    case "StepByStep":
                        StepByStep = XmlHelper.ParseBoolean(r.ReadElementContentAsString());
                        break;
                    case "FeaturesPerFrame":
                        FeaturesPerFrame = int.Parse(r.ReadElementContentAsString(), CultureInfo.InvariantCulture);
                        break;
                    case "RansacReprojThreshold":
                        RansacReprojThreshold = XmlHelper.ParseFloat(r.ReadElementContentAsString());
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
            w.WriteElementString("StepByStep", XmlHelper.WriteBoolean(StepByStep));
            w.WriteElementString("FeaturesPerFrame", FeaturesPerFrame.ToString(CultureInfo.InvariantCulture));
            w.WriteElementString("RansacReprojThreshold", XmlHelper.WriteFloat(RansacReprojThreshold));
        }
        #endregion
    }
}
