using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Globalization;
using System.ComponentModel;

namespace Kinovea.Services
{
    /// <summary>
    /// Parameters used in the Camera motion video filter.
    /// </summary>
    public class CameraMotionParameters
    {
        #region Properties
        
        /// <summary>
        /// Whether the action menu shows menus for each individual steps 
        /// </summary>
        public bool StepByStep { get; set; } = true;

        /// <summary>
        /// Type of feature to track and match.
        /// </summary>
        public CameraMotionFeatureType FeatureType { get; set; } = CameraMotionFeatureType.SIFT;

        /// <summary>
        /// Maximum number of features used per frame.
        /// </summary>
        public int FeaturesPerFrame { get; set; } = 2048;

        /// <summary>
        /// Threshold to consider that a point is an inlier during 
        /// robust motion estimation.
        /// </summary>
        public float RansacReprojThreshold { get; set; } = 1.25f;

        /// <summary>
        /// If true, filter out matches where the feature jumps over a large distance.
        /// The distance is d = distanceThresholdNormalized * image width.
        /// </summary>
        public bool UseDistanceThreshold { get; set; } = true;

        /// <summary>
        /// If useDistanceThreshold, matches spanning more than this fraction 
        /// of the image width are filtered out.
        /// </summary>
        public float DistanceThresholdNormalized { get; set; } = 0.1f;

        /// <summary>
        /// If true, matches are filtered out if the distance ratio between 
        /// the best and second best match is too small (Lowe's distance ratio test).
        /// If false, matches are filtered based on the "cross-check" test
        /// where both features must have the other one as its nearest neighbor.
        /// </summary>
        public bool UseDistanceRatioTest { get; set; } = true;

        /// <summary>
        /// When following features over multiple frames we only keep 
        /// tracks if they span at least this many frames.
        /// </summary>
        public int MinTrackLength { get; set; } = 6;
        #endregion

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public CameraMotionParameters Clone()
        {
            CameraMotionParameters clone = new CameraMotionParameters();
            clone.StepByStep = this.StepByStep;
            clone.FeatureType = this.FeatureType;
            clone.FeaturesPerFrame = this.FeaturesPerFrame;
            clone.RansacReprojThreshold = this.RansacReprojThreshold;
            clone.UseDistanceThreshold = this.UseDistanceThreshold;
            clone.DistanceThresholdNormalized = this.DistanceThresholdNormalized;
            clone.UseDistanceRatioTest = this.UseDistanceRatioTest;
            clone.MinTrackLength = this.MinTrackLength;
            return clone;
        }

        public int GetContentHash()
        {
            int hash = 0;
            hash ^= StepByStep.GetHashCode();
            hash ^= FeatureType.GetHashCode();
            hash ^= FeaturesPerFrame.GetHashCode();
            hash ^= RansacReprojThreshold.GetHashCode();
            hash ^= UseDistanceThreshold.GetHashCode();
            hash ^= DistanceThresholdNormalized.GetHashCode();
            hash ^= UseDistanceRatioTest.GetHashCode();
            hash ^= MinTrackLength.GetHashCode();
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
                    case "FeatureType":
                        FeatureType = XmlHelper.ParseEnum<CameraMotionFeatureType>(r.ReadElementContentAsString(), CameraMotionFeatureType.SIFT);
                        break;
                    case "FeaturesPerFrame":
                        FeaturesPerFrame = int.Parse(r.ReadElementContentAsString(), CultureInfo.InvariantCulture);
                        break;
                    case "RansacReprojThreshold":
                        RansacReprojThreshold = XmlHelper.ParseFloat(r.ReadElementContentAsString());
                        break;
                    case "UseDistanceThreshold":
                        UseDistanceThreshold = XmlHelper.ParseBoolean(r.ReadElementContentAsString());
                        break;
                    case "DistanceThresholdNormalized":
                        DistanceThresholdNormalized = XmlHelper.ParseFloat(r.ReadElementContentAsString());
                        break;
                    case "UseDistanceRatioTest":
                        UseDistanceRatioTest = XmlHelper.ParseBoolean(r.ReadElementContentAsString());
                        break;
                    case "MinTrackLength":
                        MinTrackLength = int.Parse(r.ReadElementContentAsString(), CultureInfo.InvariantCulture);
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
            
            TypeConverter enumConverter = TypeDescriptor.GetConverter(typeof(CameraMotionFeatureType));
            string xmlFeatureType = enumConverter.ConvertToString(FeatureType);
            w.WriteElementString("FeatureType", xmlFeatureType);
            
            w.WriteElementString("FeaturesPerFrame", FeaturesPerFrame.ToString(CultureInfo.InvariantCulture));
            w.WriteElementString("RansacReprojThreshold", XmlHelper.WriteFloat(RansacReprojThreshold));
            w.WriteElementString("UseDistanceThreshold", XmlHelper.WriteBoolean(UseDistanceThreshold));
            w.WriteElementString("DistanceThresholdNormalized", XmlHelper.WriteFloat(DistanceThresholdNormalized));
            w.WriteElementString("UseDistanceRatioTest", XmlHelper.WriteBoolean(UseDistanceRatioTest));
            w.WriteElementString("MinTrackLength", MinTrackLength.ToString(CultureInfo.InvariantCulture));
        }
        #endregion
    }
}
