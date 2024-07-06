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
        /// Takes a point with coordinates p in the image at sourceTimestamp, 
        /// returns the corresponding point in image at targetTimestamp.
        /// Typically reference timestamp is the timestamp where the point was placed
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
    }
}
