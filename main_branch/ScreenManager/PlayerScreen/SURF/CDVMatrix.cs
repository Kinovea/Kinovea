
using System;
using System.Collections.Generic;

namespace OpenSURF
{
    public class CDVMatrix
    {
        public double M11;
        public double M12;
        public double M13;
        public double M14;

        public double M21;
        public double M22;
        public double M23;
        public double M24;

        public double M31;
        public double M32;
        public double M33;
        public double M34;

        public double M41;
        public double M42;
        public double M43;
        public double M44 = 1;

        public CDVMatrix()
        {
        }

        public CDVMatrix(CDVMatrix pDVMatrixS)
        {
            if (pDVMatrixS == null) return;

            M11 = pDVMatrixS.M11;
            M12 = pDVMatrixS.M12;
            M13 = pDVMatrixS.M13;
            M14 = pDVMatrixS.M14;

            M21 = pDVMatrixS.M21;
            M22 = pDVMatrixS.M22;
            M23 = pDVMatrixS.M23;
            M24 = pDVMatrixS.M24;

            M31 = pDVMatrixS.M31;
            M32 = pDVMatrixS.M32;
            M33 = pDVMatrixS.M33;
            M34 = pDVMatrixS.M34;

            M41 = pDVMatrixS.M41;
            M42 = pDVMatrixS.M42;
            M43 = pDVMatrixS.M43;
            M44 = pDVMatrixS.M44;
        }

        public CDVMatrix(double[,] Mij)
        {
            M11 = (double)Mij[0, 0];
            M12 = (double)Mij[0, 1];
            M13 = (double)Mij[0, 2];
            M14 = (double)Mij[0, 3];

            M21 = (double)Mij[1, 0];
            M22 = (double)Mij[1, 1];
            M23 = (double)Mij[1, 2];
            M24 = (double)Mij[1, 3];

            M31 = (double)Mij[2, 0];
            M32 = (double)Mij[2, 1];
            M33 = (double)Mij[2, 2];
            M34 = (double)Mij[2, 3];

            M41 = (double)Mij[3, 0];
            M42 = (double)Mij[3, 1];
            M43 = (double)Mij[3, 2];
            M44 = (double)Mij[3, 3];
        }

        public CDVMatrix(double[] Mij)
        {
            int ix = 0;
            M11 = (double)Mij[ix++];
            M12 = (double)Mij[ix++];
            M13 = (double)Mij[ix++];
            M14 = (double)Mij[ix++];

            M21 = (double)Mij[ix++];
            M22 = (double)Mij[ix++];
            M23 = (double)Mij[ix++];
            M24 = (double)Mij[ix++];

            M31 = (double)Mij[ix++];
            M32 = (double)Mij[ix++];
            M33 = (double)Mij[ix++];
            M34 = (double)Mij[ix++];

            M41 = (double)Mij[ix++];
            M42 = (double)Mij[ix++];
            M43 = (double)Mij[ix++];
            M44 = (double)Mij[ix++];
        }

        public override string ToString()
        {
            return String.Format("CDVMatrix ( ({0} {1} {2} {3}) ({4} {5} {6} {7}) ({8} {9} {10} {11}) ({12} {13} {14} {15}))",
                                        M11, M12, M13, M14, M21, M22, M23, M24, M31, M32, M33, M34, M41, M42, M43, M44);
        }

        public bool Compare(CDVMatrix pDVMatrix)
        {
            if (pDVMatrix == null) return false;
            if (M11 != pDVMatrix.M11 || M12 != pDVMatrix.M12 || M13 != pDVMatrix.M13 || M14 != pDVMatrix.M14) return false;
            if (M21 != pDVMatrix.M21 || M22 != pDVMatrix.M22 || M23 != pDVMatrix.M23 || M24 != pDVMatrix.M24) return false;
            if (M31 != pDVMatrix.M31 || M32 != pDVMatrix.M32 || M33 != pDVMatrix.M33 || M34 != pDVMatrix.M34) return false;
            if (M41 != pDVMatrix.M41 || M42 != pDVMatrix.M42 || M43 != pDVMatrix.M43 || M44 != pDVMatrix.M44) return false;
            return true;
        }

        public void Translate(double x, double y, double z)
        {
            M14 += x;
            M24 += y;
            M34 += z;
        }

        public void Scale(double fx, double fy, double fz)
        {
            M11 *= fx;
            M22 *= fy;
            M33 *= fz;
        }

        public void Copy(CDVMatrix pCDVMatrix)
        {
            if (pCDVMatrix == null) return;

            M11 = pCDVMatrix.M11;
            M12 = pCDVMatrix.M12;
            M13 = pCDVMatrix.M13;
            M14 = pCDVMatrix.M14;

            M21 = pCDVMatrix.M21;
            M22 = pCDVMatrix.M22;
            M23 = pCDVMatrix.M23;
            M24 = pCDVMatrix.M24;

            M31 = pCDVMatrix.M31;
            M32 = pCDVMatrix.M32;
            M33 = pCDVMatrix.M33;
            M34 = pCDVMatrix.M34;

            M41 = pCDVMatrix.M41;
            M42 = pCDVMatrix.M42;
            M43 = pCDVMatrix.M43;
            M44 = pCDVMatrix.M44;
        }

        public void Add(CDVMatrix pCDVMatrix)
        {
            if (pCDVMatrix == null) return;

            M11 += pCDVMatrix.M11;
            M12 += pCDVMatrix.M12;
            M13 += pCDVMatrix.M13;
            M14 += pCDVMatrix.M14;

            M21 += pCDVMatrix.M21;
            M22 += pCDVMatrix.M22;
            M23 += pCDVMatrix.M23;
            M24 += pCDVMatrix.M24;

            M31 += pCDVMatrix.M31;
            M32 += pCDVMatrix.M32;
            M33 += pCDVMatrix.M33;
            M34 += pCDVMatrix.M34;

            M41 += pCDVMatrix.M41;
            M42 += pCDVMatrix.M42;
            M43 += pCDVMatrix.M43;
            M44 += pCDVMatrix.M44;
        }

        public void Multiply(CDVMatrix pCDVMatrix)
        {
            if (pCDVMatrix == null) return;

            double m11 = M11 * pCDVMatrix.M11 + M12 * pCDVMatrix.M21 + M13 * pCDVMatrix.M31 + M14 * pCDVMatrix.M41;
            double m12 = M11 * pCDVMatrix.M12 + M12 * pCDVMatrix.M22 + M13 * pCDVMatrix.M32 + M14 * pCDVMatrix.M42;
            double m13 = M11 * pCDVMatrix.M13 + M12 * pCDVMatrix.M23 + M13 * pCDVMatrix.M33 + M14 * pCDVMatrix.M43;
            double m14 = M11 * pCDVMatrix.M14 + M12 * pCDVMatrix.M24 + M13 * pCDVMatrix.M34 + M14 * pCDVMatrix.M44;

            double m21 = M21 * pCDVMatrix.M11 + M22 * pCDVMatrix.M21 + M23 * pCDVMatrix.M31 + M24 * pCDVMatrix.M41;
            double m22 = M21 * pCDVMatrix.M12 + M22 * pCDVMatrix.M22 + M23 * pCDVMatrix.M32 + M24 * pCDVMatrix.M42;
            double m23 = M21 * pCDVMatrix.M13 + M22 * pCDVMatrix.M23 + M23 * pCDVMatrix.M33 + M24 * pCDVMatrix.M43;
            double m24 = M21 * pCDVMatrix.M14 + M22 * pCDVMatrix.M24 + M23 * pCDVMatrix.M34 + M24 * pCDVMatrix.M44;

            double m31 = M31 * pCDVMatrix.M11 + M32 * pCDVMatrix.M21 + M33 * pCDVMatrix.M31 + M34 * pCDVMatrix.M41;
            double m32 = M31 * pCDVMatrix.M12 + M32 * pCDVMatrix.M22 + M33 * pCDVMatrix.M32 + M34 * pCDVMatrix.M42;
            double m33 = M31 * pCDVMatrix.M13 + M32 * pCDVMatrix.M23 + M33 * pCDVMatrix.M33 + M34 * pCDVMatrix.M43;
            double m34 = M31 * pCDVMatrix.M14 + M32 * pCDVMatrix.M24 + M33 * pCDVMatrix.M34 + M34 * pCDVMatrix.M44;

            double m41 = M41 * pCDVMatrix.M11 + M42 * pCDVMatrix.M21 + M43 * pCDVMatrix.M31 + M44 * pCDVMatrix.M41;
            double m42 = M41 * pCDVMatrix.M12 + M42 * pCDVMatrix.M22 + M43 * pCDVMatrix.M32 + M44 * pCDVMatrix.M42;
            double m43 = M41 * pCDVMatrix.M13 + M42 * pCDVMatrix.M23 + M43 * pCDVMatrix.M33 + M44 * pCDVMatrix.M43;
            double m44 = M41 * pCDVMatrix.M14 + M42 * pCDVMatrix.M24 + M43 * pCDVMatrix.M34 + M44 * pCDVMatrix.M44;

            M11 = m11;
            M12 = m12;
            M13 = m13;
            M14 = m14;

            M21 = m21;
            M22 = m22;
            M23 = m23;
            M24 = m24;

            M31 = m31;
            M32 = m32;
            M33 = m33;
            M34 = m34;

            M41 = m41;
            M42 = m42;
            M43 = m43;
            M44 = m44;
        }

        public void Multiply(double Factor)
        {
            M11 *= Factor;
            M12 *= Factor;
            M13 *= Factor;
            M14 *= Factor;

            M21 *= Factor;
            M22 *= Factor;
            M23 *= Factor;
            M24 *= Factor;

            M31 *= Factor;
            M32 *= Factor;
            M33 *= Factor;
            M34 *= Factor;

            M41 *= Factor;
            M42 *= Factor;
            M43 *= Factor;
            M44 *= Factor;
        }

        public void RotateXYZ(double AngleX, double AngleY, double AngleZ)
        {
            if (AngleX != 0)
            {
                Multiply(CDVMatrix.RotationX(AngleX));
            }
            if (AngleY != 0)
            {
                Multiply(CDVMatrix.RotationY(AngleY));
            }
            if (AngleZ != 0)
            {
                Multiply(CDVMatrix.RotationZ(AngleZ));
            }
        }

        public void MultiplyVector(float Xs, float Ys, float Zs, out float Xd, out float Yd, out float Zd)
        {
            double xs = (double)Xs;
            double ys = (double)Ys;
            double zs = (double)Zs;
            Xd = (float)((double)M11 * xs + (double)M12 * ys + (double)M13 * zs + (double)M14);
            Yd = (float)((double)M21 * xs + (double)M22 * ys + (double)M23 * zs + (double)M24);
            Zd = (float)((double)M31 * xs + (double)M32 * ys + (double)M33 * zs + (double)M34);
        }

        public void MultiplyVector(double xs, double ys, double zs, out double Xd, out double Yd, out double Zd)
        {
            Xd = ((double)M11 * xs + (double)M12 * ys + (double)M13 * zs + (double)M14);
            Yd = ((double)M21 * xs + (double)M22 * ys + (double)M23 * zs + (double)M24);
            Zd = ((double)M31 * xs + (double)M32 * ys + (double)M33 * zs + (double)M34);
            double Wd = ((double)M41 * xs + (double)M42 * ys + (double)M43 * zs + (double)M44 * 1);
            if (Wd != 0 && Wd != 1)
            {
                Xd /= Wd;
                Yd /= Wd;
                Zd /= Wd;
            }
        }

        public void MultiplyVector(double xs, double ys, double zs, double ws, out double Xd, out double Yd, out double Zd, out double Wd)
        {
            Xd = ((double)M11 * xs + (double)M12 * ys + (double)M13 * zs + (double)M14 * ws);
            Yd = ((double)M21 * xs + (double)M22 * ys + (double)M23 * zs + (double)M24 * ws);
            Zd = ((double)M31 * xs + (double)M32 * ys + (double)M33 * zs + (double)M34 * ws);
            Wd = ((double)M41 * xs + (double)M42 * ys + (double)M43 * zs + (double)M44 * ws);
        }

        public void Transpose()
        {
            double v = 0;

            v = M12;
            M12 = M21;
            M21 = v;
            v = M13;
            M13 = M31;
            M31 = v;
            v = M14;
            M14 = M41;
            M41 = v;

            v = M23;
            M23 = M32;
            M32 = v;
            v = M24;
            M24 = M42;
            M42 = v;

            v = M34;
            M34 = M43;
            M43 = v;
        }

        public void MultiplyXYZArrayToXYZ(double[] aXYZi, out double[] aXYZf)
        {
            aXYZf = null;
            if (aXYZi == null || aXYZi.Length < 3) return;
            int im = aXYZi.Length / 3;
            aXYZf = new double[im * 3];
            int ixs = 0;
            int ixd = 0;
            for (int i = 0; i < im; i++)
            {
                double xs = aXYZi[ixs++];
                double ys = aXYZi[ixs++];
                double zs = aXYZi[ixs++];
                aXYZf[ixd++] = (M11 * xs + M12 * ys + M13 * zs + M14);
                aXYZf[ixd++] = (M21 * xs + M22 * ys + M23 * zs + M24);
                aXYZf[ixd++] = (M31 * xs + M32 * ys + M33 * zs + M34);
            }
        }

        public void MultiplyXYZArrayToXYZ(double[] aXYZi, out float[] aXYZf)
        {
            aXYZf = null;
            if (aXYZi == null || aXYZi.Length < 3) return;
            int im = aXYZi.Length / 3;
            aXYZf = new float[im * 3];
            int ixs = 0;
            int ixd = 0;
            for (int i = 0; i < im; i++)
            {
                double xs = aXYZi[ixs++];
                double ys = aXYZi[ixs++];
                double zs = aXYZi[ixs++];
                aXYZf[ixd++] = (float)(M11 * xs + M12 * ys + M13 * zs + M14);
                aXYZf[ixd++] = (float)(M21 * xs + M22 * ys + M23 * zs + M24);
                aXYZf[ixd++] = (float)(M31 * xs + M32 * ys + M33 * zs + M34);
            }
        }

        public void MultiplyXYZArrayToXY(double[] aXYZi, out double[] aXYf)
        {
            aXYf = null;
            if (aXYZi == null || aXYZi.Length < 3) return;
            int im = aXYZi.Length / 3;
            aXYf = new double[im * 2];
            int ixs = 0;
            int ixd = 0;
            for (int i = 0; i < im; i++)
            {
                double xs = aXYZi[ixs++];
                double ys = aXYZi[ixs++];
                double zs = aXYZi[ixs++];
                aXYf[ixd++] = (M11 * xs + M12 * ys + M13 * zs + M14);
                aXYf[ixd++] = (M21 * xs + M22 * ys + M23 * zs + M24);
            }
        }

        public void MultiplyXYZArrayToXY(double[] aXYZi, out float[] aXYf)
        {
            aXYf = null;
            if (aXYZi == null || aXYZi.Length < 3) return;
            int im = aXYZi.Length / 3;
            aXYf = new float[im * 2];
            int ixs = 0;
            int ixd = 0;
            for (int i = 0; i < im; i++)
            {
                double xs = aXYZi[ixs++];
                double ys = aXYZi[ixs++];
                double zs = aXYZi[ixs++];
                aXYf[ixd++] = (float)(M11 * xs + M12 * ys + M13 * zs + M14);
                aXYf[ixd++] = (float)(M21 * xs + M22 * ys + M23 * zs + M24);
            }
        }

        public void MultiplyXYArrayToXYZ(double[] aXYi, out double[] aXYZf)
        {
            aXYZf = null;
            if (aXYi == null || aXYi.Length < 2) return;
            int im = aXYi.Length / 2;
            aXYZf = new double[im * 3];
            int ixs = 0;
            int ixd = 0;
            for (int i = 0; i < im; i++)
            {
                double xs = aXYi[ixs++];
                double ys = aXYi[ixs++];
                double zs = 0;
                aXYZf[ixd++] = (M11 * xs + M12 * ys + M13 * zs + M14);
                aXYZf[ixd++] = (M21 * xs + M22 * ys + M23 * zs + M24);
                aXYZf[ixd++] = (M31 * xs + M32 * ys + M33 * zs + M34);
            }
        }

        public void MultiplyXYArrayToXY(double[] aXYi, out double[] aXYf)
        {
            aXYf = null;

            if (aXYi == null || aXYi.Length < 2) return;

            int im = aXYi.Length / 2;
            aXYf = new double[im * 2];

            int ixs = 0;
            int ixd = 0;
            for (int i = 0; i < im; i++)
            {
                double xs = aXYi[ixs++];
                double ys = aXYi[ixs++];
                double zs = 0;
                aXYf[ixd++] = (M11 * xs + M12 * ys + M13 * zs + M14);
                aXYf[ixd++] = (M21 * xs + M22 * ys + M23 * zs + M24);
            }
        }

        #region ---  public static
        public static CDVMatrix Identity
        {
            get
            {
                CDVMatrix vret = new CDVMatrix();

                vret.M11 = 1;
                vret.M12 = 0;
                vret.M13 = 0;
                vret.M14 = 0;

                vret.M21 = 0;
                vret.M22 = 1;
                vret.M23 = 0;
                vret.M24 = 0;

                vret.M31 = 0;
                vret.M32 = 0;
                vret.M33 = 1;
                vret.M34 = 0;

                vret.M41 = 0;
                vret.M42 = 0;
                vret.M43 = 0;
                vret.M44 = 1;

                return vret;
            }
        }

        public static CDVMatrix Zero
        {
            get
            {
                CDVMatrix vret = new CDVMatrix();

                vret.M11 = 0;
                vret.M12 = 0;
                vret.M13 = 0;
                vret.M14 = 0;

                vret.M21 = 0;
                vret.M22 = 0;
                vret.M23 = 0;
                vret.M24 = 0;

                vret.M31 = 0;
                vret.M32 = 0;
                vret.M33 = 0;
                vret.M34 = 0;

                vret.M41 = 0;
                vret.M42 = 0;
                vret.M43 = 0;
                vret.M44 = 1;

                return vret;
            }
        }

        public static CDVMatrix Multiplication(CDVMatrix pCDVMatrix1, CDVMatrix pCDVMatrix2)
        {
            if (pCDVMatrix1 == null || pCDVMatrix2 == null)
            {
                return null;
            }
            CDVMatrix vret = new CDVMatrix(pCDVMatrix1);
            vret.Multiply(pCDVMatrix2);
            return vret;
        }

        public static CDVMatrix Multiplication(List<CDVMatrix> apCDVMatrix)
        {
            return Multiplication(apCDVMatrix, false);
        }

        public static CDVMatrix Multiplication(List<CDVMatrix> apCDVMatrix, bool bRevertOrder)
        {
            if (apCDVMatrix == null || apCDVMatrix.Count == 0)
            {
                return null;
            }

            CDVMatrix vret = null;

            if (bRevertOrder == false)
            {
                for (int i = 0; i < apCDVMatrix.Count; i++)
                {
                    if (apCDVMatrix[i] == null) continue;
                    if (vret == null)
                    {
                        vret = new CDVMatrix((CDVMatrix)apCDVMatrix[i]);
                    }
                    else
                    {
                        vret = Multiplication((CDVMatrix)apCDVMatrix[i], vret);
                    }
                }
            }
            else
            {
                for (int i = apCDVMatrix.Count - 1; i >= 0; i--)
                {
                    if (apCDVMatrix[i] == null) continue;
                    if (vret == null)
                    {
                        vret = new CDVMatrix((CDVMatrix)apCDVMatrix[i]);
                    }
                    else
                    {
                        vret = Multiplication((CDVMatrix)apCDVMatrix[i], vret);
                    }
                }
            }

            return vret;
        }

        public static CDVMatrix Translation(double x, double y, double z)
        {
            CDVMatrix vret = new CDVMatrix();

            vret.M11 = 1;
            vret.M12 = 0;
            vret.M13 = 0;
            vret.M14 = x;

            vret.M21 = 0;
            vret.M22 = 1;
            vret.M23 = 0;
            vret.M24 = y;

            vret.M31 = 0;
            vret.M32 = 0;
            vret.M33 = 1;
            vret.M34 = z;

            return vret;
        }

        public static CDVMatrix Scaling(double fx, double fy, double fz)
        {
            CDVMatrix vret = new CDVMatrix();

            vret.M11 = fx;
            vret.M12 = 0;
            vret.M13 = 0;
            vret.M14 = 0;

            vret.M21 = 0;
            vret.M22 = fy;
            vret.M23 = 0;
            vret.M24 = 0;

            vret.M31 = 0;
            vret.M32 = 0;
            vret.M33 = fz;
            vret.M34 = 0;

            return vret;
        }

        public static CDVMatrix RotationX(double angle)
        {
            CDVMatrix vret = new CDVMatrix();

            double cos = (double)Math.Cos(angle);
            double sin = (double)Math.Sin(angle);

            vret.M11 = 1;
            vret.M12 = 0;
            vret.M13 = 0;

            vret.M21 = 0;
            vret.M22 = cos;
            vret.M23 = -sin;

            vret.M31 = 0;
            vret.M32 = sin;
            vret.M33 = cos;

            return vret;
        }

        public static CDVMatrix RotationY(double angle)
        {
            CDVMatrix vret = new CDVMatrix();

            double cos = (double)Math.Cos(angle);
            double sin = (double)Math.Sin(angle);

            vret.M11 = cos;
            vret.M12 = 0;
            vret.M13 = -sin;

            vret.M21 = 0;
            vret.M22 = 1;
            vret.M23 = 0;

            vret.M31 = sin;
            vret.M32 = 0;
            vret.M33 = cos;

            return vret;
        }

        public static CDVMatrix RotationZ(double angle)
        {
            CDVMatrix vret = new CDVMatrix();

            double cos = (double)Math.Cos(angle);
            double sin = (double)Math.Sin(angle);

            vret.M11 = cos;
            vret.M12 = -sin;
            vret.M13 = 0;

            vret.M21 = sin;
            vret.M22 = cos;
            vret.M23 = 0;

            vret.M31 = 0;
            vret.M32 = 0;
            vret.M33 = 1;

            return vret;
        }

        public static CDVMatrix RotationAlphaBeta(double alpha, double beta)
        {
            CDVMatrix vret = new CDVMatrix();

            double cosalpha = (double)Math.Cos(alpha);
            double sinalpha = (double)Math.Sin(alpha);
            double cosbeta = (double)Math.Cos(beta);
            double sinbeta = (double)Math.Sin(beta);

            vret.M11 = cosalpha * cosbeta;
            vret.M12 = -sinalpha;
            vret.M13 = -cosalpha * sinbeta;

            vret.M21 = sinalpha * cosbeta;
            vret.M22 = cosalpha;
            vret.M23 = -sinalpha * sinbeta;

            vret.M31 = sinbeta;
            vret.M32 = 0;
            vret.M33 = cosbeta;

            return vret;
        }

        public static CDVMatrix RotationAlphaBetaGamma(double alpha, double beta, double gamma)
        {
            CDVMatrix vret = new CDVMatrix();

            CDVMatrix mbetaalpha = CDVMatrix.RotationAlphaBeta(alpha, beta);
            mbetaalpha.Transpose();

            CDVMatrix mgamma = CDVMatrix.RotationX(gamma);
            mgamma.Transpose();

            vret = CDVMatrix.Multiplication(mgamma, mbetaalpha);
            vret.Transpose();

            return vret;
        }

        public static CDVMatrix InvertX()
        {
            CDVMatrix vret = new CDVMatrix(Identity);
            vret.M11 = -1d;
            return vret;
        }

        public static CDVMatrix InvertY()
        {
            CDVMatrix vret = new CDVMatrix(Identity);
            vret.M22 = -1d;
            return vret;
        }

        public static CDVMatrix InvertZ()
        {
            CDVMatrix vret = new CDVMatrix(Identity);
            vret.M33 = -1d;
            return vret;
        }

        public static double Determinant(CDVMatrix pCDVMatrix)
        {
            double vret = 0;
            if (pCDVMatrix == null) return vret;
            vret = pCDVMatrix.M11 * (pCDVMatrix.M22 * pCDVMatrix.M33 - pCDVMatrix.M32 * pCDVMatrix.M23)
                    - pCDVMatrix.M12 * (pCDVMatrix.M21 * pCDVMatrix.M33 - pCDVMatrix.M31 * pCDVMatrix.M23)
                    + pCDVMatrix.M13 * (pCDVMatrix.M21 * pCDVMatrix.M32 - pCDVMatrix.M31 * pCDVMatrix.M22);
            return vret;
        }

        public static CDVMatrix Inversion(CDVMatrix pCDVMatrix)
        {
            if (pCDVMatrix == null) return null;

            CDVMatrix vret = null;
            try
            {
                double[,] Mij = pCDVMatrix.Mij();
                double[,] mij = Invert(Mij);
                if (mij == null) return null;
                vret = new CDVMatrix(mij);
            }
            finally
            {
            }

            return vret;
        }

        public static CDVMatrix BuildModulationMatrix(float X, float Y, float DX, float DY)
        {
            CDVMatrix vret = null;
            try
            {
                CDVMatrix _vret = new CDVMatrix();

                double x1 = X;
                double y1 = Y;
                double x2 = X + DX;
                double y2 = Y + DY;

                _vret.M11 = 1 / (x2 - x1);
                _vret.M12 = 0;
                _vret.M13 = 0;
                _vret.M14 = -x1 / (x2 - x1);

                _vret.M21 = 0;
                _vret.M22 = 1 / (y2 - y1);
                _vret.M23 = 0;
                _vret.M24 = -y1 / (y2 - y1);

                vret = _vret;
            }
            finally
            {
            }
            return vret;
        }

        public static CDVMatrix BuildWindowMatrix(double Xs, double Ys, double DXs, double DYs, double Xd, double Yd, double DXd, double DYd)
        {
            if (DXs == 0 || DYs == 0) return null;

            CDVMatrix vret = new CDVMatrix();

            vret.M11 = DXd / DXs;
            vret.M12 = 0;
            vret.M13 = 0;
            vret.M14 = (Xd - (Xs * DXd) / DXs);

            vret.M21 = 0;
            vret.M22 = DYd / DYs;
            vret.M23 = 0;
            vret.M24 = (Yd - (Ys * DYd) / DYs);

            return vret;
        }

        public static void TranslateXYZ(double[] aXYZ, double DX, double DY, double DZ)
        {
            if (aXYZ == null) return;
            int im = aXYZ.Length / 3;
            int j = 0;
            for (int i = 0; i < im; i++)
            {
                aXYZ[j++] += DX;
                aXYZ[j++] += DY;
                aXYZ[j++] += DZ;
            }
        }

        /**
         * - Heading_Tilt_Roll retourne la CDVMatrix représentant la suite d'opérations:
         *      - rotation de Heading / Z
         *      - rotation de Tilt / X
         *      - rotation de Roll / Z
         * - Heading/Tilt/Roll: en radians
        **/
        public static CDVMatrix Heading_Tilt_Roll(double Heading, double Tilt, double Roll)
        {
            List<CDVMatrix> apCDVMatrix = new List<CDVMatrix>();
            apCDVMatrix.Add(CDVMatrix.RotationZ(Heading));
            apCDVMatrix.Add(CDVMatrix.RotationX(Tilt));
            apCDVMatrix.Add(CDVMatrix.RotationZ(Roll));
            return CDVMatrix.Multiplication(apCDVMatrix, true);
        }

        #endregion

        #region ---  private

        double[,] Mij()
        {
            double[,] vret = new double[4, 4];

            vret[0, 0] = M11;
            vret[0, 1] = M12;
            vret[0, 2] = M13;
            vret[0, 3] = M14;

            vret[1, 0] = M21;
            vret[1, 1] = M22;
            vret[1, 2] = M23;
            vret[1, 3] = M24;

            vret[2, 0] = M31;
            vret[2, 1] = M32;
            vret[2, 2] = M33;
            vret[2, 3] = M34;

            vret[3, 0] = M41;
            vret[3, 1] = M42;
            vret[3, 2] = M43;
            vret[3, 3] = M44;

            return vret;
        }

        static int Dimension(double[,] Mij)
        {
            int dimension = 0;

            switch (Mij.Length)
            {
                case 4:
                    dimension = 2;
                    break;
                case 9:
                    dimension = 3;
                    break;
                case 16:
                    dimension = 4;
                    break;
            }

            return dimension;
        }

        static double[,] CoMatrix(double[,] Mij, int Row, int Column)
        {
            double[,] vret = null;
            int dimension = Dimension(Mij);

            vret = new double[dimension - 1, dimension - 1];

            int irow = 0;
            for (int row = 0; row < dimension; row++)
            {
                if (row == Row) continue;
                int icol = 0;
                for (int col = 0; col < dimension; col++)
                {
                    if (col == Column) continue;
                    vret[irow, icol] = Mij[row, col];
                    icol++;
                }
                irow++;
            }

            return vret;
        }

        static double Determinant(double[,] Mij)
        {
            double vret = 0;

            if (Mij.Length == 4)
            {
                vret = Mij[0, 0] * Mij[1, 1] - Mij[1, 0] * Mij[0, 1];
                return vret;
            }

            double[,] comatrix00 = CoMatrix(Mij, 0, 0);
            double[,] comatrix01 = CoMatrix(Mij, 0, 1);
            double[,] comatrix02 = CoMatrix(Mij, 0, 2);
            double det00 = Determinant(comatrix00);
            double det01 = Determinant(comatrix01);
            double det02 = Determinant(comatrix02);

            double m00 = Mij[0, 0];
            double m01 = Mij[0, 1];
            double m02 = Mij[0, 2];

            vret = m00 * det00
                    - m01 * det01
                    + m02 * det02;

            return vret;
        }

        static double coeff(int Row, int Col)
        {
            double vret = 1;
            int im = (Row + 1) + (Col + 1);
            for (int i = 0; i < im; i++)
            {
                vret *= -1;
            }
            return vret;
        }

        static double[,] Invert(double[,] Mij)
        {
            double[,] vret = null;

            try
            {
                int dimension = Dimension(Mij);
                double determinant = Determinant(Mij);
                if (determinant == 0)
                {
                    return vret;
                }

                vret = new double[dimension, dimension];

                for (int row = 0; row < dimension; row++)
                {
                    for (int col = 0; col < dimension; col++)
                    {
                        double[,] matrix = CoMatrix(Mij, row, col);
                        double det = Determinant(matrix);
                        vret[col, row] = (coeff(row, col) * det) / determinant;// + Transposition
                    }
                }

            }
            finally
            {
            }

            return vret;
        }

        #endregion

    }
}
