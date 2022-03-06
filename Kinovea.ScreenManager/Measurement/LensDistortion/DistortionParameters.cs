using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Kinovea.ScreenManager
{
    public class DistortionParameters
    {
        // Camera intrinsics.
        public double Fx
        {
            get { return fx; }
            set { fx = value; Build(); }
        }
        public double Fy
        {
            get { return fy; }
            set { fy = value; Build(); }
        }
        public double Cx
        {
            get { return cx; }
            set { cx = value; Build(); }
        }
        public double Cy
        {
            get { return cy; }
            set { cy = value; Build(); }
        }


        // Distortion coefficients.
        public double K1
        {
            get { return k1; }
            set { k1 = value; Build(); }
        }
        public double K2
        {
            get { return k2; }
            set { k2 = value; Build(); }
        }
        public double K3
        {
            get { return k3; }
            set { k3 = value; Build(); }
        }
        public double P1
        {
            get { return p1; }
            set { p1 = value; Build(); }
        }
        public double P2
        {
            get { return p2; }
            set { p2 = value; Build(); }
        }

        public double PixelsPerMillimeter { get; set; }

        public double[,] cameraMatrix = new double[,] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } };
        public double[] distCoeffs = new double[5];

        // Default camera intrinsics based on Blender values for Go Pro Hero 3.
        public const double defaultFocalLength = 2.77;
        public const double defaultSensorWidth = 6.160;

        private double fx;
        private double fy;
        private double cx;
        private double cy;
        private double k1;
        private double k2;
        private double k3;
        private double p1;
        private double p2;

        /// <summary>
        /// Constructor used when the parameters are fit internally.
        /// </summary>
        public DistortionParameters(double k1, double k2, double k3, double p1, double p2, double fx, double fy, double cx, double cy, Size imageSize)
            : this(k1, k2, k3, p1, p2, fx, fy, cx, cy, imageSize.Width / defaultSensorWidth)
        {
        }

        /// <summary>
        /// Constructor used when reading existing data.
        /// </summary>
        public DistortionParameters(double k1, double k2, double k3, double p1, double p2, double fx, double fy, double cx, double cy, double pixelsPerMillimeter)
        {
            this.fx = fx;
            this.fy = fy;
            this.cx = cx;
            this.cy = cy;
            
            this.k1 = k1;
            this.k2 = k2;
            this.k3 = k3;
            this.p1 = p1;
            this.p2 = p2;

            this.PixelsPerMillimeter = pixelsPerMillimeter;
            
            Build();
        }

        /// <summary>
        /// Constructor used to create a default set of parameters.
        /// </summary>
        public DistortionParameters(Size imageSize)
        {
            PixelsPerMillimeter = imageSize.Width / defaultSensorWidth;
            
            fx = defaultFocalLength * PixelsPerMillimeter;
            fy = fx;
            cx = imageSize.Width / 2.0;
            cy = imageSize.Height / 2.0;

            k1 = 0;
            k2 = 0;
            k3 = 0;
            p1 = 0;
            p2 = 0;

            Build();
        }

        /// <summary>
        /// Build the object actually used for distortion compensation.
        /// </summary>
        private void Build()
        {
            distCoeffs[0] = K1;
            distCoeffs[1] = K2;
            distCoeffs[4] = K3;
            distCoeffs[2] = P1;
            distCoeffs[3] = P2;

            cameraMatrix[0, 0] = Fx;
            cameraMatrix[1, 1] = Fy;
            cameraMatrix[0, 2] = Cx;
            cameraMatrix[1, 2] = Cy;
            cameraMatrix[2, 2] = 1.0f;
        }

        public int ContentHash
        {
            get
            {
                return K1.GetHashCode() ^
                    K2.GetHashCode() ^
                    K3.GetHashCode() ^
                    P1.GetHashCode() ^
                    P2.GetHashCode() ^
                    Fx.GetHashCode() ^
                    Fy.GetHashCode() ^
                    Cx.GetHashCode() ^
                    Cy.GetHashCode();
            }
        }
    }
}
