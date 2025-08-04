using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Finds intrinsic parameters and distortion coefficients from image points of pattern.
    ///
    /// DEPRECATED: this class was used to process the "distortion grid" drawing.
    /// From 2024.1 forward we use the Lens calibration filter instead which is much 
    /// more flexible and automated.
    /// 
    /// References:
    /// Background and original implementation:
    /// http://www.vision.caltech.edu/bouguetj/calib_doc/
    /// http://docs.opencv.org/doc/tutorials/calib3d/camera_calibration/camera_calibration.html
    /// </summary>
    public class CameraCalibrator
    {
        public bool Valid
        {
            get { return valid; }
        }

        private List<List<OpenCvSharp.Point3f>> objectPoints;
        private List<List<OpenCvSharp.Point2f>> imagePoints;

        private Size imageSize;
        private bool valid;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public CameraCalibrator(List<List<PointF>> imagePoints, Size imageSize)
        {
            if (imagePoints == null || imagePoints.Count == 0)
                return;

            ImportImagePoints(imagePoints);
            this.imageSize = imageSize;

            ComputeObjectPoints(imagePoints);

            valid = true;
        }

        public DistortionParameters Calibrate()
        {
            var cameraMatrix = new double[,] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } };
            var distCoeffs = new double[5];
            OpenCvSharp.CalibrationFlags flags = OpenCvSharp.CalibrationFlags.RationalModel;
            var termCriteriaType = OpenCvSharp.CriteriaTypes.MaxIter | OpenCvSharp.CriteriaTypes.Eps;
            int maxIter = 30;
            float eps = 0.001f;
            var termCriteria = new OpenCvSharp.TermCriteria(termCriteriaType, maxIter, eps);
            
            OpenCvSharp.Cv2.CalibrateCamera(
                objectPoints,
                imagePoints,
                new OpenCvSharp.Size(imageSize.Width, imageSize.Height),
                cameraMatrix,
                distCoeffs,
                out var rotationVectors, 
                out var translationVectors,
                flags,
                termCriteria
            );

            double k1 = distCoeffs[0];
            double k2 = distCoeffs[1];
            double k3 = distCoeffs[4];
            double p1 = distCoeffs[2];
            double p2 = distCoeffs[3];

            double fx = cameraMatrix[0, 0];
            double fy = cameraMatrix[1, 1];
            double cx = cameraMatrix[0, 2];
            double cy = cameraMatrix[1, 2];

            var parameters = new DistortionParameters(k1, k2, k3, p1, p2, fx, fy, cx, cy, imageSize);
            log.DebugFormat("Distortion coefficients: k1:{0:0.000}, k2:{1:0.000}, k3:{2:0.000}, p1:{3:0.000}, p2:{4:0.000}.", k1, k2, k3, p1, p2);
            log.DebugFormat("Camera intrinsics: fx:{0:0.000}, fy:{1:0.000}, cx:{2:0.000}, cy:{3:0.000}", fx, fy, cx, cy);

            return parameters;
        }

        private void ImportImagePoints(List<List<PointF>> inputImagePoints)
        {
            imagePoints = new List<List<OpenCvSharp.Point2f>>();
            foreach (var a in inputImagePoints)
            {
                var n = a.Select(p => new OpenCvSharp.Point2f(p.X, p.Y)).ToList();
                imagePoints.Add(n);
            }
        }
        
        private void ComputeObjectPoints(List<List<PointF>> inputImagePoints)
        {
            // Precompute the 3D coordinates of the pattern.
            // Z is always 0 as the calibration object is planar.
            // The number of points depends on the number of points in the input.
            // The units are not important.
            // The pattern is assumed to only contain squares (chessboard like).

            int width = 100;
            int height = 100;
            objectPoints = new List<List<OpenCvSharp.Point3f>>();

            // Loop over the input images.
            foreach (var list in inputImagePoints)
            {
                // FIXME: get number of rows and cols from imagePoints.
                int rows = 5;
                int cols = 5;
                List<OpenCvSharp.Point3f> points = new List<OpenCvSharp.Point3f>(rows * cols);

                float stepWidth = width / cols;
                float stepHeight = height / rows;
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        float x = j * stepWidth;
                        float y = i * stepHeight;
                        float z = 0;

                        points[i * cols + j] = new OpenCvSharp.Point3f(x, y, z);
                    }
                }

                objectPoints.Add(points);
            }
        }
    }
}
