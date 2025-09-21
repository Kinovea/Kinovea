using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Exposes functions to transform coordinates from one frame to another, 
    /// taking into account the camera motion.
    /// The camera motion must have already been estimated using the CameraMotion filter.
    /// 
    /// This class is also responsible for the serialization of the transforms to KVA.
    /// </summary>
    public class CameraTransformer
    {
        #region Properties
        /// <summary>
        /// Whether the transformer has been initialized with a list of frame transforms.
        /// </summary>
        public bool Initialized 
        {
            get { return initialized; }
        }
        #endregion

        #region Members
        private bool initialized;
        private Dictionary<long, int> frameIndices = new Dictionary<long, int>();
        private List<OpenCvSharp.Mat> consecTransforms = new List<OpenCvSharp.Mat>();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public void Initialize(CameraTracker tracker)
        {
            if (!tracker.Tracked || tracker.FrameIndices.Count == 0 || tracker.ConsecutiveTransforms.Count == 0)
            {
                Deinitialize();
                return;
            }

            this.frameIndices = tracker.FrameIndices;
            this.consecTransforms = tracker.ConsecutiveTransforms;
            initialized = true;
        }

        /// <summary>
        /// Delete the transforms and reset the state of the transformer.
        /// </summary>
        public void Deinitialize()
        {
            log.DebugFormat("De-initializing the camera transformer.");

            frameIndices.Clear();
            foreach (var consec in consecTransforms)
            {
                consec.Dispose();
            }
            consecTransforms.Clear();
            initialized = false;
        }

        /// <summary>
        /// Takes a point with coordinates p in the image at sourceTimestamp, 
        /// returns the corresponding point in image at targetTimestamp.
        /// Typically source timestamp is the timestamp where the point was placed
        /// on top of a world object in the video. This is the starting point 
        /// of the transform stack.
        /// </summary>
        public PointF Transform(long sourceTimestamp, long targetTimestamp, PointF p)
        {
            if (!initialized)
                return p;

            if (!frameIndices.ContainsKey(sourceTimestamp) || !frameIndices.ContainsKey(targetTimestamp))
                return p;

            if (sourceTimestamp == targetTimestamp)
                return p;

            //--------------------------------------------------------------------------------
            // Note on the tranform stack.
            // Ideally we want to have one reference frame serving as the world coordinate
            // system, and then a series of rotation matrices that transform points from the
            // other frames into that global coordinate system.
            // This is not implemented yet.
            // What we have now is a series of consecutive transforms, each one transforming
            // points from one frame to the next.
            // We also can't pre-compute the transforms going from a given frame to the reference frame
            // or vice-versa, there is too much loss of precision.
            // The only way for now is to pass the point successively through each pairwise transform.
            // This process happens below for points pertaining to drawings.
            //--------------------------------------------------------------------------------

            // consecTransforms[i] transforms points in image I to their corresponding point in image I+1.
            int startIndex = frameIndices[sourceTimestamp];
            int endIndex = frameIndices[targetTimestamp];

            var p2 = new[] { new OpenCvSharp.Point2f(p.X, p.Y) };
            if (startIndex < endIndex)
            {
                // Forward transform.
                for (int i = startIndex; i < endIndex; i++)
                {
                    p2 = OpenCvSharp.Cv2.PerspectiveTransform(p2, consecTransforms[i]);
                }
            }
            else
            {
                // Backward transform.
                for (int i = startIndex; i > endIndex; i--)
                {
                    p2 = OpenCvSharp.Cv2.PerspectiveTransform(p2, consecTransforms[i - 1].Inv());
                }
            }

            return new PointF(p2[0].X, p2[0].Y);
        }

        #region KVA serialization
        /// <summary>
        /// Save camera transforms to KVA.
        /// </summary>
        public void WriteXml(XmlWriter w)
        {
            if (consecTransforms.Count != frameIndices.Count - 1)
            {
                log.ErrorFormat("The number of transforms does not match the number of frames.");
                return;
            }

            // This tells whether we are saving consecutive homographies or global rotations.
            w.WriteAttributeString("type", "Homographies");

            // Note: we are writing the last frame entry which doesn't have an actual transform.
            foreach (var kvp in frameIndices)
            {
                w.WriteStartElement("Frame");
                w.WriteAttributeString("timestamp", kvp.Key.ToString());
                w.WriteAttributeString("index", kvp.Value.ToString());
                
                if (kvp.Value < consecTransforms.Count)
                    w.WriteString(WriteMatrix(consecTransforms[kvp.Value]));

                w.WriteEndElement();
            }
        }

        public void ReadXml(XmlReader r)
        {
            // Note:
            // This function must be a complete alternative to Initialize().

            bool isEmpty = r.IsEmptyElement;

            bool homographies = true;
            if (r.MoveToAttribute("type"))
            {
                string type = r.ReadContentAsString();
                homographies = type == "Homographies";
            }

            r.ReadStartElement();

            if (isEmpty)
                return;

            while (r.NodeType == XmlNodeType.Element)
            {
                if (r.Name != "Frame")
                {
                    log.DebugFormat("Unsupported content in CameraMotion KVA: {0}", r.Name);
                    r.ReadOuterXml();
                    continue;
                }

                bool isEmptyFrame = r.IsEmptyElement;

                // Read attributes.
                bool hasTimestamp = r.MoveToAttribute("timestamp");
                if (!hasTimestamp)
                {
                    log.ErrorFormat("Missing timestamp in CameraMotion KVA.");
                    r.ReadOuterXml();
                    continue;
                }

                long timestamp = r.ReadContentAsLong();

                bool hasIndex = r.MoveToAttribute("index");
                if (!hasIndex)
                {
                    log.ErrorFormat("Missing index in CameraMotion KVA.");
                    r.ReadOuterXml();
                    continue;
                }

                int index = r.ReadContentAsInt();
                if (frameIndices.ContainsKey(timestamp))
                {
                    log.ErrorFormat("Duplicate timestamp in CameraMotion KVA.");
                    r.ReadOuterXml();
                    continue;
                }

                frameIndices.Add(timestamp, index);

                if (isEmptyFrame)
                {
                    // This is the empty frame corresponding to the last frame index
                    // which doesn't have a transform to the "next" frame.
                    r.ReadStartElement();
                    continue;
                }
                else
                {
                    consecTransforms.Add(ReadMatrix(r));
                }
            }

            r.ReadEndElement();

            initialized = true;
    }

        private string WriteMatrix(OpenCvSharp.Mat mat)
        {
            List<string> elements = new List<string>();
            for (int i = 0; i < mat.Rows; i++)
            {
                for (int j = 0; j < mat.Cols; j++)
                {
                    elements.Add(string.Format(CultureInfo.InvariantCulture, "{0}", mat.At<double>(i, j)));
                }
            }

            return string.Join(";", elements);
        }

        private OpenCvSharp.Mat ReadMatrix(XmlReader r)
        {
            r.ReadStartElement();
            string matrix = r.ReadContentAsString();
            string[] elements = matrix.Split(';');
            double[] values = elements.Select(e => double.Parse(e, CultureInfo.InvariantCulture)).ToArray();
            r.ReadEndElement();

            if (values.Length != 9)
                throw new InvalidDataException("Invalid matrix size.");

            return OpenCvSharp.Mat.FromPixelData(3, 3, OpenCvSharp.MatType.CV_64FC1, values);
        }
        #endregion
    }
}
