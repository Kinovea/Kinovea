using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Runtime.InteropServices;
using Emgu.CV.CvEnum;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Finds distortion coefficients and intrinsic parameters from image points of pattern.
    ///
    /// References :
    /// Background and original implementation:
    /// http://www.vision.caltech.edu/bouguetj/calib_doc/
    /// http://docs.opencv.org/doc/tutorials/calib3d/camera_calibration/camera_calibration.html
    /// Usage in EmguCV:
    /// http://www.emgu.com/wiki/index.php/Camera_Calibration
    /// </summary>
    public class CameraCalibrator
    {
        public bool Valid
        {
            get { return valid; }
        }

        private MCvPoint3D32f[][] allObjectPoints;
        private PointF[][] allImagePoints;
        private Size imageSize;
        private bool valid;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public CameraCalibrator(List<List<PointF>> imagePoints, Size imageSize)
        {
            ImportImagePoints(imagePoints);
            this.imageSize = imageSize;

            ComputeObjectPoints(imagePoints);

            valid = true;
        }

        public void Calibrate()
        {
            CALIB_TYPE flags = 
                //CALIB_TYPE.CV_CALIB_FIX_ASPECT_RATIO |
                //CALIB_TYPE.CV_CALIB_FIX_FOCAL_LENGTH |
                //CALIB_TYPE.CV_CALIB_FIX_PRINCIPAL_POINT |
                CALIB_TYPE.CV_CALIB_FIX_K3 |
                (CALIB_TYPE)16384; // CV_CALIB_RATIONAL_MODEL*/

            int imageCount = allImagePoints.Length;

            int[] pointCounts = new int[allObjectPoints.Length];
            for (int i = 0; i < allObjectPoints.Length; i++)
            {
                // TODO: Check that both image and object have the same number of points for this image.
                pointCounts[i] = allObjectPoints[i].Length;
            }

            IntrinsicCameraParameters icp = new IntrinsicCameraParameters();

            using (Matrix<float> objectPointMatrix = ToMatrix(allObjectPoints))
            using (Matrix<float> imagePointMatrix = ToMatrix(allImagePoints))
            using (Matrix<int> pointCountsMatrix = new Matrix<int>(pointCounts))
            using (Matrix<double> rotationVectors = new Matrix<double>(imageCount, 3))
            using (Matrix<double> translationVectors = new Matrix<double>(imageCount, 3))
            {
                CvInvoke.cvCalibrateCamera2(
                    objectPointMatrix.Ptr,
                    imagePointMatrix.Ptr,
                    pointCountsMatrix.Ptr,
                    imageSize,
                    icp.IntrinsicMatrix,
                    icp.DistortionCoeffs,
                    rotationVectors,
                    translationVectors,
                    flags);
            }

            double k1 = icp.DistortionCoeffs[0, 0];
            double k2 = icp.DistortionCoeffs[1, 0];
            double k3 = icp.DistortionCoeffs[4, 0];
            double p1 = icp.DistortionCoeffs[2, 0];
            double p2 = icp.DistortionCoeffs[3, 0];
            double fx = icp.IntrinsicMatrix[0, 0];
            double fy = icp.IntrinsicMatrix[1, 1];
            double cx = icp.IntrinsicMatrix[0, 2];
            double cy = icp.IntrinsicMatrix[1, 2];

            log.DebugFormat("Distortion parameters: k1:{0:0.000}, k2:{1:0.000}, k3:{2:0.000}, p1:{3:0.000}, p2:{4:0.000}.", k1, k2, k3, p1, p2);
            log.DebugFormat("Camera intrinsics: fx:{0:0.000}, fy:{1:0.000}, cx:{2:0.000}, cy:{3:0.000}", fx, fy, cx, cy);
        }

        private void ImportImagePoints(List<List<PointF>> imagePoints)
        {
            allImagePoints = new PointF[imagePoints.Count][];

            for (int i = 0; i < imagePoints.Count; i++)
                allImagePoints[i] = imagePoints[i].ToArray();
        }

        private void ComputeObjectPoints(List<List<PointF>> imagePoints)
        {
            // For each input, precompute the 3D coordinates of the pattern.
            // Z is always 0 as the calibration object is planar.
            // The number of points depends on the number of points in the input.
            
            // The units are not important.
            // The pattern is assumed to be squares (chessboard like).
            int width = 100;
            int height = 100;

            allObjectPoints = new MCvPoint3D32f[imagePoints.Count][];

            int image = 0;

            foreach (List<PointF> points in imagePoints)
            {
                // FIXME: get number of rows and cols from imagePoints.
                int rows = 5;
                int cols = 5;

                MCvPoint3D32f[] objectPoints = new MCvPoint3D32f[rows * cols];

                float stepWidth = width / cols;
                float stepHeight = height / rows;

                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        float x = j * stepWidth;
                        float y = i * stepHeight;
                        float z = 0;

                        objectPoints[i * cols + j] = new MCvPoint3D32f(x, y, z);
                    }
                }

                allObjectPoints[image] = objectPoints;
                image++;
            }

        }


        private static Matrix<float> ToMatrix(MCvPoint3D32f[][] data)
        {
            int elementCount = 0;
            foreach (MCvPoint3D32f[] d in data) 
                elementCount += d.Length;

            Matrix<float> res = new Matrix<float>(elementCount, 3);

            Int64 address = res.MCvMat.data.ToInt64();

            foreach (MCvPoint3D32f[] d in data)
            {
                int lengthInBytes = d.Length * StructSize.MCvPoint3D32f;
                GCHandle handle = GCHandle.Alloc(d, GCHandleType.Pinned);
                Emgu.Util.Toolbox.memcpy(new IntPtr(address), handle.AddrOfPinnedObject(), lengthInBytes);
                handle.Free();
                address += lengthInBytes;
            }

            return res;
        }

        private static Matrix<float> ToMatrix(PointF[][] data)
        {
            int elementCount = 0;
            foreach (PointF[] d in data) 
                elementCount += d.Length;

            Matrix<float> res = new Matrix<float>(elementCount, 2);
            Int64 address = res.MCvMat.data.ToInt64();

            foreach (PointF[] d in data)
            {
                int lengthInBytes = d.Length * StructSize.PointF;
                GCHandle handle = GCHandle.Alloc(d, GCHandleType.Pinned);
                Emgu.Util.Toolbox.memcpy(new IntPtr(address), handle.AddrOfPinnedObject(), lengthInBytes);
                handle.Free();
                address += lengthInBytes;
            }

            return res;
        }
    }
}
