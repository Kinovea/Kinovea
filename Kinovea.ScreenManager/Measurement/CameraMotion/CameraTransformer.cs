using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Exposes functions to transform coordinates from one frame to another, 
    /// taking into account the camera motion.
    /// The camera motion must have already been estimated using the CameraMotion filter.
    /// </summary>
    public class CameraTransformer
    {
        #region Properties
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
            if (tracker.FrameIndices.Count == 0 || tracker.ConsecutiveTransforms.Count == 0)
            {
                log.ErrorFormat("The camera tracker does not have transforms.");
                return;
            }

            this.frameIndices = tracker.FrameIndices;
            this.consecTransforms = tracker.ConsecutiveTransforms;

            initialized = true;
        }

        /// <summary>
        /// Takes a point with coordinates p in the image at referenceTimestamp, 
        /// returns the corresponding point in image at currentTimestamp.
        /// Typically reference timestamp is the timestamp where the point was placed
        /// on top of a world object in the video. This is the starting point 
        /// of the transform stack.
        /// </summary>
        public Point Transform(long referenceTimestamp, long currentTimestamp, Point p)
        {
            if (!initialized)
                return p;

            if (!frameIndices.ContainsKey(referenceTimestamp) || !frameIndices.ContainsKey(currentTimestamp))
                return p;

            // Pairs of conscutive frames transforms.
            // consecTransforms[i] transforms points in image I to their corresponding point in image I+1.
            int startIndex = frameIndices[referenceTimestamp];
            int endIndex = frameIndices[currentTimestamp];

            if (startIndex == endIndex)
                return p;
            
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

            return new Point((int)p2[0].X, (int)p2[0].Y);
        }

        public Point Transform(long src, long dst, PointF p)
        {
            return Transform(src, dst, p.ToPoint());
        }
    }
}
