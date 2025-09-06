#region License
/*
Copyright © Joan Charmant 2024.
jcharmant@gmail.com 
 
This file is part of Kinovea.

Kinovea is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License version 2 
as published by the Free Software Foundation.

Kinovea is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Kinovea. If not, see http://www.gnu.org/licenses/.
*/
#endregion
using System;
using System.Drawing;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Computes the camera 3D position from the plane-based calibration and lens calibration.
    /// </summary>
    public class CameraPoser
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Gets the 3D position of the camera in the space of the calibration rectangle.
        /// Plane is the calibration rectangle in world space.
        /// </summary>
        public static Vector3 Compute(QuadrilateralF plane, ProjectiveMapper mapping, DistortionParameters lensCalib)
        {
            // Based on: MONOCULAR RECTANGLE RECONSTRUCTION. Wefelscheid, 2011.
            // Paragraph 3.1 - Geometric method.

            // Get the mid points of the rectangle edges in homogenous coordinates,
            // that is on the image plane at z=1. In the paper this part is calculated
            // using the vanishing points but we don't need that since we already have the homography.
            // This covers equations 1 to 4 of the paper.
            // First get the mid points in world space then project them to the image plane.
            PointF p12 = new PointF((plane.A.X + plane.B.X) / 2, (plane.A.Y + plane.B.Y) / 2);
            PointF p23 = new PointF((plane.B.X + plane.C.X) / 2, (plane.B.Y + plane.C.Y) / 2);
            PointF p34 = new PointF((plane.C.X + plane.D.X) / 2, (plane.C.Y + plane.D.Y) / 2);
            PointF p14 = new PointF((plane.A.X + plane.D.X) / 2, (plane.A.Y + plane.D.Y) / 2);
            PointF m = new PointF((plane.A.X + plane.B.X) / 2, (plane.A.Y + plane.D.Y) / 2);
            p12 = mapping.Forward(p12);
            p23 = mapping.Forward(p23);
            p34 = mapping.Forward(p34);
            p14 = mapping.Forward(p14);
            m = mapping.Forward(m);

            // Normalize and inverse Y direction.
            float cx = (float)lensCalib.Cx;
            float cy = (float)lensCalib.Cy;
            float fx = (float)lensCalib.Fx;
            float fy = (float)lensCalib.Fy;
            p12 = new PointF(p12.X - cx, - (p12.Y - cy));
            p23 = new PointF(p23.X - cx, -(p23.Y - cy));
            p34 = new PointF(p34.X - cx, -(p34.Y - cy));
            p14 = new PointF(p14.X - cx, -(p14.Y - cy));
            m = new PointF(m.X - cx, -(m.Y - cy));

            // Projections of the mid-points on the image plane but expressed in world space.
            Vector3 pp12 = new Vector3(p12.X / fx, p12.Y / fy, 1);
            Vector3 pp23 = new Vector3(p23.X / fx, p23.Y / fy, 1);
            Vector3 pp34 = new Vector3(p34.X / fx, p34.Y / fy, 1);
            Vector3 pp14 = new Vector3(p14.X / fx, p14.Y / fy, 1);
            Vector3 mp = new Vector3(m.X / fx, m.Y / fy, 1);

            // Find the angles (fovport) to the mid-points projections.
            // Eq. 5 in the paper.
            // This works via dot product.
            // Order: Top, bottom, right, left.
            float alpha = (float)Math.Acos(Vector3.Dot(pp12, mp) / (pp12.Norm * mp.Norm));
            float beta = (float)Math.Acos(Vector3.Dot(pp34, mp) / (pp34.Norm * mp.Norm));
            float delta = (float)Math.Acos(Vector3.Dot(pp23, mp) / (pp23.Norm * mp.Norm));
            float gamma = (float)Math.Acos(Vector3.Dot(pp14, mp) / (pp14.Norm * mp.Norm));

            log.DebugFormat("---------------------------------");
            log.DebugFormat("Alpha = {0}, Beta = {1}, Delta = {2}, Gamma = {3}",
                MathHelper.Degrees(alpha), MathHelper.Degrees(beta), MathHelper.Degrees(delta), MathHelper.Degrees(gamma));

            // Find the distance to the actual mid-points up to an arbitrary scale factor d.
            // Eq. 6 and 7 in the paper.
            // This works via sine law.
            // d is the distance from camera to plane center point, we don't know the actual 
            // value yet. We will scale it later.
            float d = 10;
            float dp12 = (float)((2 * d * Math.Sin(beta)) / Math.Sin(alpha + beta));
            float dp14 = (float)((2 * d * Math.Sin(delta)) / Math.Sin(delta + gamma));
            //float dp34 = (float)((2 * d * Math.Sin(alpha)) / Math.Sin(alpha + beta));
            //float dp23 = (float)((2 * d * Math.Sin(gamma)) / Math.Sin(delta + gamma));
            //log.DebugFormat("dp12 = {0}, dp34 = {1}, dp14 = {2}, dp23 = {3}", dp12, dp34, dp14, dp23);

            // Find the world location of the mid-points (in camera space).
            // Eq. 8 in the paper.
            // This just scales the direction vectors going from the camera to the projected points
            // by the distance we just calculated.
            Vector3 dir12 = pp12 / pp12.Norm;
            Vector3 dir14 = pp14 / pp14.Norm;
            Vector3 p12cam = dir12 * dp12;
            Vector3 p14cam = dir14 * dp14;

            // Parameterize the rectangle by the vectors going to the mid points.
            // Eq. 9 and 10 in the paper.
            Vector3 mdir = mp / mp.Norm;
            Vector3 mcam = mdir * d;
            Vector3 ucam = p12cam - mcam;
            Vector3 vcam = p14cam - mcam;
            //log.DebugFormat("mcam: [{0}, {1}, {2}]", mcam.X, mcam.Y, mcam.Z);
            //log.DebugFormat("ucam: [{0}, {1}, {2}]", ucam.X, ucam.Y, ucam.Z);
            //log.DebugFormat("vcam: [{0}, {1}, {2}]", vcam.X, vcam.Y, vcam.Z);
            //log.DebugFormat("ucam length: {0}", ucam.Norm);
            //log.DebugFormat("vcam length: {0}", vcam.Norm);

            // With the length of these basis vectors we can compute the
            // rectangle aspect ratio and the scale factor.
            // This replaces eq. 11. that computes the corners of the rectangle.

            // Verify aspect ratio.
            float ar = vcam.Norm / ucam.Norm;
            float width = plane.B.X - plane.A.X;
            float height = plane.D.Y - plane.A.Y;
            float arRef = width / height;
            log.DebugFormat("Computed aspect ratio: {0}, Ref: {1}", ar, arRef);

            // Average the scale factor from both directions.
            float scaleHeight = ucam.Norm * 2 / height;
            float scaleWidth = vcam.Norm * 2 / width;
            float scale = (scaleHeight + scaleWidth) / 2;
            
            // Apply the scale to get the true distance.
            d /= scale;
            log.DebugFormat("Computed scale factor: {0}", scale);
            log.DebugFormat("Estimated distance to the plane:{0}", d);

            // Now that we know the real distance, recompute the mid-points
            // and get the new basis vectors.
            p12cam = p12cam / scale;
            p14cam = p14cam / scale;
            mcam = mcam / scale;
            ucam = p12cam - mcam;
            vcam = p14cam - mcam;

            // We don't need to calculate the plane orientation. (Eq. 12.)
            // The paper stops here.

            // We now compute the camera position in rectangle space.
            // Build direction vectors for the plane basis and project the view vector onto this basis.
            // Axes: X: to the right, Y: into the screen, Z: up.
            Vector3 toCam = mcam.Negate();
            Vector3 xdir = (vcam / vcam.Norm).Negate();
            Vector3 ydir = ucam / ucam.Norm; 
            Vector3 zdir = Vector3.Cross(ydir, xdir);
            float x = Vector3.Dot(toCam, xdir);
            float y = Vector3.Dot(toCam, ydir);
            float z = Vector3.Dot(toCam, zdir);
            log.DebugFormat("Camera view ray in rectangle basis: x = {0}, y = {1}, z = {2}", x, y, z);

            // Translate origin to bottom-left corner.
            // TODO: take user's offset, mirroring and origin corner into account.
            x += width / 2;
            y += height / 2;

            return new Vector3(x, y, z);
        }
    }
}
