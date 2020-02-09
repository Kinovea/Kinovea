using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Emgu.CV;

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

        public IntrinsicCameraParameters IntrinsicCameraParameters { get; private set; }
        
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
        public DistortionParameters(IntrinsicCameraParameters icp, Size imageSize)
        {
            this.IntrinsicCameraParameters = icp;

            fx = icp.IntrinsicMatrix[0, 0];
            fy = icp.IntrinsicMatrix[1, 1];
            cx = icp.IntrinsicMatrix[0, 2];
            cy = icp.IntrinsicMatrix[1, 2];

            k1 = icp.DistortionCoeffs[0, 0];
            k2 = icp.DistortionCoeffs[1, 0];
            k3 = icp.DistortionCoeffs[4, 0];
            p1 = icp.DistortionCoeffs[2, 0];
            p2 = icp.DistortionCoeffs[3, 0];

            PixelsPerMillimeter = imageSize.Width / defaultSensorWidth;
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
            IntrinsicCameraParameters icp = new IntrinsicCameraParameters();
            icp.DistortionCoeffs[0, 0] = K1;
            icp.DistortionCoeffs[1, 0] = K2;
            icp.DistortionCoeffs[4, 0] = K3;
            icp.DistortionCoeffs[2, 0] = P1;
            icp.DistortionCoeffs[3, 0] = P2;

            icp.IntrinsicMatrix[0, 0] = Fx;
            icp.IntrinsicMatrix[1, 1] = Fy;
            icp.IntrinsicMatrix[0, 2] = Cx;
            icp.IntrinsicMatrix[1, 2] = Cy;
            icp.IntrinsicMatrix[2, 2] = 1;

            this.IntrinsicCameraParameters = icp;
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
