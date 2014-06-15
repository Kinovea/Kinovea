using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;

namespace Kinovea.ScreenManager
{
    public class DistortionParameters
    {
        public IntrinsicCameraParameters IntrinsicCameraParameters { get; private set; }
        
        // Distortion coefficients.
        public double K1 { get; private set; }
        public double K2 { get; private set; }
        public double K3 { get; private set; }
        public double P1 { get; private set; }
        public double P2 { get; private set; }
        
        // Camera intrinsics.
        public double Fx { get; private set; }
        public double Fy { get; private set; }
        public double Cx { get; private set; }
        public double Cy { get; private set; }

        public DistortionParameters(IntrinsicCameraParameters icp)
        {
            this.IntrinsicCameraParameters = icp;

            this.K1 = icp.DistortionCoeffs[0, 0];
            this.K2 = icp.DistortionCoeffs[1, 0];
            this.K3 = icp.DistortionCoeffs[4, 0];
            this.P1 = icp.DistortionCoeffs[2, 0];
            this.P2 = icp.DistortionCoeffs[3, 0];
            this.Fx = icp.IntrinsicMatrix[0, 0];
            this.Fy = icp.IntrinsicMatrix[1, 1];
            this.Cx = icp.IntrinsicMatrix[0, 2];
            this.Cy = icp.IntrinsicMatrix[1, 2];
        }

        public DistortionParameters(double k1, double k2, double k3, double p1, double p2, double fx, double fy, double cx, double cy)
        {
            this.K1 = k1;
            this.K2 = k2;
            this.K3 = k3;
            this.P1 = p1;
            this.P2 = p2;
            this.Fx = fx;
            this.Fy = fy;
            this.Cx = cx;
            this.Cy = cy;
        }
    }
}
