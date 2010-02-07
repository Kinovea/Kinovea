using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace OpenSURF
{
    public class COpenSURF
    {

        public static float[] ImageIntegral(float[] imageDataS, int WidthS, int HeightS, int RowByteSize, int ChannelS)
        {
            float[] imageDataD = new float[RowByteSize * HeightS];

            int WidthStepS = RowByteSize / sizeof(float);

            float[] rs = new float[ChannelS];
            int ixdataS = 0;
            int ixdataD = 0;
            int ixdataD_prevrow = 0;

            for (int row = 0; row < HeightS; row++)
            {
                for (int channel = 0; channel < ChannelS; channel++) rs[channel] = 0;

                int _ixdataS = ixdataS;
                int _ixdataD = ixdataD;
                int _ixdataD_prevrow = ixdataD_prevrow;

                for (int col = 0; col < WidthS; col++)
                {

                    if (row == 0)
                    {
                        for (int channel = 0; channel < ChannelS; channel++)
                        {
                            rs[channel] += imageDataS[_ixdataS++];
                            imageDataD[_ixdataD++] = rs[channel];
                        }
                    }
                    else
                    {
                        for (int channel = 0; channel < ChannelS; channel++)
                        {
                            rs[channel] += imageDataS[_ixdataS++];
                            imageDataD[_ixdataD++] = rs[channel] + imageDataD[ixdataD_prevrow++];
                        }
                    }

                }

                ixdataS += WidthStepS;
                ixdataD_prevrow = ixdataD;
                ixdataD += WidthStepS;
            }

            return imageDataD;
        }

        public static float[] ImageIntegral(float[] imageDataS, int WidthS, int HeightS, int RowByteSize)
        {
            double[] imageDataD = new double[RowByteSize * HeightS];

            int WidthStepS = RowByteSize / sizeof(float);

            double rs = 0;
            int ixdataS = 0;
            int ixdataD = 0;
            int ixdataD_prevrow = 0;

            for (int row = 0; row < HeightS; row++)
            {
                rs = 0;

                int _ixdataS = ixdataS;
                int _ixdataD = ixdataD;
                int _ixdataD_prevrow = ixdataD_prevrow;

                for (int col = 0; col < WidthS; col++)
                {

                    if (row == 0)
                    {
                        rs += imageDataS[_ixdataS++];
                        imageDataD[_ixdataD++] = rs;
                    }
                    else
                    {
                        rs += imageDataS[_ixdataS++];
                        imageDataD[_ixdataD++] = rs + imageDataD[ixdataD_prevrow++];
                    }

                }

                ixdataS += WidthStepS;
                ixdataD_prevrow = ixdataD;
                ixdataD += WidthStepS;
            }

            float[] vret = new float[RowByteSize * HeightS];

            for (int i = 0; i < vret.Length; i++)
            {
                vret[i] = (float)imageDataD[i];
            }

            return vret;
        }

        public static float Area(float[] imageData, int Width, int Height, int RowByteSize, int X, int Y, int W, int H)
        {
            float vret = 0;

            int WidthStep = RowByteSize / sizeof(float);
            int x1 = X - 1;
            int y1 = Y - 1;
            int x2 = x1 + W;
            int y2 = y1 + H;

            if (x1 < 0) return vret;
            if (y1 < 0) return vret;
            if (x2 >= Width) return vret;
            if (y2 >= Height) return vret;

            vret += imageData[x2 + y2 * WidthStep];
            vret += imageData[x1 + y1 * WidthStep];
            vret -= imageData[x1 + y2 * WidthStep];
            vret -= imageData[x2 + y1 * WidthStep];

            return vret;
        }

        public static float Area(IplImage pIplImage, int X, int Y, int W, int H)
        {
            return Area(pIplImage.imageData, pIplImage.width, pIplImage.height, pIplImage.widthStep,
                            X,Y,W,H);
        }

        public static void surfDetDes(	string Path, IplImage pIplImage, out List<Ipoint> ipts, bool upright, int octaves, 
                                      	int intervals, int init_sample, float thres, int interp_steps)
        {
        	// Detects interest points.
        	// Describe them in SURF.
        	// Fill ipts array with them.
        	
            ipts = new List<Ipoint>();

            // 
            IplImage pint_img = pIplImage.BuildIntegral(Path);

            CFastHessian pCFastHessian = new CFastHessian(pint_img,
                                                                ref ipts, octaves, intervals, init_sample,
                                                                thres, interp_steps);
            pCFastHessian.getIpoints();

            pCFastHessian.save_Det(Path);

            // At this point the IPoints are filled with: 
            // x=340,4788 y=154 scale=2 
            // Not filled : descriptor {0}, orientation=0 laplacian=-1 
            Surf pSurf = new Surf(pint_img, ipts);
            pSurf.getDescriptors(upright);
        }

        #region --- internal static

        internal static int cvRound(float Value)
        {
            return (int)Math.Round(Value);
        }

        internal static float pow(float X, int Y)
        {
            return (float)(Math.Pow(X, Y));
        }

        internal static float fabs(float X)
        {
            return Math.Abs(X);
        }

        internal static float exp(float X)
        {
            return (float)Math.Exp(X);
        }

        static float pi = (float)Math.PI;

        internal static float gaussian(int x, int y, float sig)
        {
          return 1.0f/(2.0f*pi*sig*sig) * exp( -(x*x+y*y)/(2.0f*sig*sig));
        }

        //! Calculate the value of the 2d gaussian at x,y
        internal static float gaussian(float x, float y, float sig)
        {
            return 1.0f/(2.0f*pi*sig*sig) * exp( -(x*x+y*y)/(2.0f*sig*sig));
        }

        //! Get the angle from the +ve x-axis of the vector given by (X Y)
        internal static float getAngle(float X, float Y)
        {
          if(X >= 0 && Y >= 0)
            return (float)Math.Atan(Y/X);

          if(X < 0 && Y >= 0)
              return (float)(Math.PI - Math.Atan(-Y / X));

          if(X < 0 && Y < 0)
              return (float)(Math.PI + Math.Atan(Y / X));

          if(X >= 0 && Y < 0)
              return (float)(2 * Math.PI - Math.Atan(-Y / X));

          return 0;
        }

        internal static CDVMatrix cvCreateMat()
        {
            CDVMatrix vret = new CDVMatrix();
            return vret;
        }

        internal static CDVMatrix cvInvert(CDVMatrix pCDVMatrix)
        {
            return (pCDVMatrix != null ? CDVMatrix.Inversion(pCDVMatrix) : null);
        }

        /***
        void  cvGEMM( const CvArr* src1, const CvArr* src2, double alpha,
                      const CvArr* src3, double beta, CvArr* dst, int tABC=0 );
        #define cvMatMulAdd( src1, src2, src3, dst ) cvGEMM( src1, src2, 1, src3, 1, dst, 0 )
        #define cvMatMul( src1, src2, dst ) cvMatMulAdd( src1, src2, 0, dst )

        src1
            The first source array. 
        src2
            The second source array. 
        src3
            The third source array (shift). Can be NULL, if there is no shift. 
        dst
            The destination array. 
        tABC
            The operation flags that can be 0 or combination of the following values:
            CV_GEMM_A_T - transpose src1
            CV_GEMM_B_T - transpose src2
            CV_GEMM_C_T - transpose src3
            for example, CV_GEMM_A_T+CV_GEMM_C_T corresponds to

            alpha*src1T*src2 + beta*srcT

        The function cvGEMM performs generalized matrix multiplication:

        dst = alpha*op(src1)*op(src2) + beta*op(src3), where op(X) is X or XT
        ***/

        internal static void cvGEMM(CDVMatrix src1,CDVMatrix src2,double alpha, CDVMatrix src3, double beta,CDVMatrix dst,int tABC)
        {
            CDVMatrix matrix1 = new CDVMatrix(src1);
            matrix1.Multiply(alpha);
            matrix1.Multiply(src2);

            CDVMatrix matrix2 = new CDVMatrix(src3);
            matrix2.Multiply(beta);

            dst.Copy(matrix1);
            dst.Add(matrix2);
        }


        #endregion

        public static int Compare_DETFiles(string PathS, string PathD)
        {
            FileStream pfs = null;
            FileStream pfd = null;
            try
            {
                pfs = new FileStream(PathS, FileMode.Open, FileAccess.Read);
                pfd = new FileStream(PathD, FileMode.Open, FileAccess.Read);

                BinaryReader pbrs = new BinaryReader(pfs);
                BinaryReader pbrd = new BinaryReader(pfd);

                int octaves_s = pbrs.ReadInt32();
                int intervals_s = pbrs.ReadInt32();
                int i_withs = pbrs.ReadInt32();
                int i_heights = pbrs.ReadInt32();
                float[] dets = new float[octaves_s * intervals_s * i_withs * i_heights];
                for (int i = 0; i < dets.Length; i++)
                {
                    dets[i] = pbrs.ReadSingle();
                }

                int octaves_d = pbrd.ReadInt32();
                int intervals_d = pbrd.ReadInt32();
                int i_withd = pbrd.ReadInt32();
                int i_heightd = pbrd.ReadInt32();
                float[] detd = new float[octaves_d * intervals_d * i_withd * i_heightd];
                for (int i = 0; i < detd.Length; i++)
                {
                    detd[i] = pbrd.ReadSingle();
                }

                if (octaves_s != octaves_d) return 1;
                if (intervals_s != intervals_d) return 1;
                if (i_withs != i_withd) return 1;
                if (i_heights != i_heightd) return 1;

                int errorcount = 0;
                double rdeltamax = 0;

                double vdetsmin = 0;
                double vdetsmax = 0;
                for (int i = 0; i < dets.Length; i++)
                {
                    double vdets = (double)dets[i];
                    if (i == 0)
                    {
                        vdetsmin = vdetsmax = vdets;
                    }
                    else
                    {
                        if (vdetsmin > vdets) vdetsmin = vdets;
                        if (vdetsmax < vdets) vdetsmax = vdets;
                    }
                }

                for (int i = 0; i < dets.Length; i++)
                {
                    double vdets = (double)dets[i];
                    double vdetd = (double)detd[i];
                    double adelta = Math.Abs(vdets - vdetd);
                    double vsd = Math.Abs(vdets + vdetd);
                    if (vsd > 0)
                    {
                        double rdelta = adelta / vsd;
                        if (rdeltamax < rdelta)
                        {
                            rdeltamax = rdelta;
                        }
                        if (rdelta >= 0.01d)
                        {
                            errorcount += 1;
                        }
                    }
                    else
                    {
                        if (adelta >= 0.01d)
                        {
                            errorcount += 1;
                        }
                    }
                }

                return errorcount;
            }
            finally
            {
                if (pfs != null) pfs.Close();
                if (pfd != null) pfd.Close();
            }
        }

        public static bool Compare_INTFiles(string PathS, string PathD)
        {
        	// source, dest.
        	
        	
            FileStream pfs = null;
            FileStream pfd = null;
            try
            {
                pfs = new FileStream(PathS, FileMode.Open, FileAccess.Read);
                pfd = new FileStream(PathD, FileMode.Open, FileAccess.Read);

                BinaryReader pbrs = new BinaryReader(pfs);
                BinaryReader pbrd = new BinaryReader(pfd);

                // int width, int height, float[width×height] int_s
                
                
                int width_s = pbrs.ReadInt32();
                int height_s = pbrs.ReadInt32();
                float[] int_s = new float[width_s * height_s];
                for (int i = 0; i < int_s.Length; i++)
                {
                    int_s[i] = pbrs.ReadSingle();
                }

                int width_d = pbrd.ReadInt32();
                int height_d = pbrd.ReadInt32();
                float[] int_d = new float[width_d * height_d];
                for (int i = 0; i < int_d.Length; i++)
                {
                    int_d[i] = pbrd.ReadSingle();
                }

                if (width_s != width_d) return false;
                if (height_s != height_d) return false;
                
                
                // 
                
                int errorcount = 0;
                int testcount = 0;
                float rdeltamax = 0;
                for (int i = 0; i < int_s.Length; i++)
                {
                    testcount += 1;
                    float vs = int_s[i];
                    float vd = int_d[i];
                    float delta = (float)Math.Abs(vs-vd);
                    if (delta > 0f)
                    {
                        float avsd=Math.Abs(vs+vd);
                        if (avsd > 0)
                        {
                            float rdelta = delta / avsd;
                            if (rdelta >= 0.001f) errorcount += 1;
                            if (rdeltamax < rdelta)
                            {
                                rdeltamax = rdelta;
                            }
                        }
                        else
                        {
                            if (delta >= 0.001f) errorcount += 1;
                        }

                    }
                }

                return (errorcount==0);
            }
            finally
            {
                if (pfs != null) pfs.Close();
                if (pfd != null) pfd.Close();
            }
        }

        public static void SavePoints(List<Ipoint> aIpoint, string PathD)
        {
            FileStream pfd = null;
            try
            {
                pfd = new FileStream(PathD, FileMode.Create, FileAccess.Write);
                BinaryWriter pbw = new BinaryWriter(pfd);

                if (aIpoint == null) return;

                foreach (Ipoint pIpoint in aIpoint)
                {
                    if (pIpoint == null) continue;

                    pbw.Write(pIpoint.x);
                    pbw.Write(pIpoint.y);
                    pbw.Write(pIpoint.scale);
                    pbw.Write(pIpoint.orientation);
                    pbw.Write(pIpoint.laplacian);

                    for (int i = 0; i < 64; i++)
                    {
                        pbw.Write(pIpoint.descriptor[i]);
                    }
                }
            }
            finally
            {
                if (pfd != null) pfd.Close();
            }
        }

        public static void PaintOpenSURF(string PathS, string PathOpenSURFS, string PathD)
        {
        	// Draw surf points on image, using surf data as input.
        	        	
            Bitmap pBitmapS = null;
            Stream pfPathOpenSURFS = null;
            Bitmap pBitmapD = null;
            Graphics pGD = null;
            Pen ppen = null;
            try
            {
                pBitmapS = new Bitmap(PathS);

                pBitmapD = new Bitmap(pBitmapS.Width, pBitmapS.Height, PixelFormat.Format32bppRgb);
                pGD = Graphics.FromImage(pBitmapD);

                pGD.DrawImage(pBitmapS, new Rectangle(0, 0, pBitmapS.Width, pBitmapS.Height));

                ppen = new Pen(Color.Red);

                pfPathOpenSURFS = new FileStream(PathOpenSURFS, FileMode.Open, FileAccess.Read);
                {
                    BinaryReader pbrPathOpenSURFS = new BinaryReader(pfPathOpenSURFS);
                    while (pfPathOpenSURFS.Position < pfPathOpenSURFS.Length)
                    {
                        float x = pbrPathOpenSURFS.ReadSingle();
                        float y = pbrPathOpenSURFS.ReadSingle();
                        float scale = pbrPathOpenSURFS.ReadSingle();
                        float orientation = pbrPathOpenSURFS.ReadSingle();
                        int laplacian = pbrPathOpenSURFS.ReadInt32();
                        float[] descriptor = new float[64];
                        for (int i = 0; i < descriptor.Length; i++)
                        {
                            descriptor[i] = pbrPathOpenSURFS.ReadSingle();
                        }

                        float _radius = ((9.0f / 1.2f) * scale) / 3.0f;
                        float _x = x - _radius;
                        float _y = y - _radius;
                        float _w = (2 * _radius);
                        float _h = (2 * _radius);

                        pGD.DrawEllipse(ppen, _x, _y, _w, _h);

                        float _xs = _x + _radius;
                        float _ys = _y + _radius;
                        float _xd = _xs + (float)(_radius * Math.Cos(orientation));
                        float _yd = _ys + (float)(_radius * Math.Sin(orientation));
                        pGD.DrawLine(ppen, new Point((int)_xs, (int)_ys), new Point((int)_xd, (int)_yd));

                    }
                }

                pBitmapD.Save(PathD);

            }
            finally
            {
                if (ppen != null) ppen.Dispose();
                if (pBitmapS != null) pBitmapS.Dispose();
                if (pfPathOpenSURFS != null) pfPathOpenSURFS.Close();
                if (pGD != null) pGD.Dispose();
                if (pBitmapD != null) pBitmapD.Dispose();
            }
        }

        public static void MatchPoints(List<Ipoint> ipts1, List<Ipoint> ipts2, out List<Match> matches)
        {
        	// Find the nearest neighbour of each ipts1 point, in ipts2 set.
        	// K-D tree implementation taken from Sebastian Nowozin's Autopano-sift.
        	
        	matches = new List<Match>();
        	
        	// K-D tree of candidate points.
        	ArrayList aTreePoints = new ArrayList(ipts2);
        	KDTree kdt2 = KDTree.CreateKDTree(aTreePoints);
        	
        	// Loop through all input points.
        	int searchDepth = (int)Math.Max(130.0, (Math.Log (aTreePoints.Count) / Math.Log (1000.0)) * 130.0);
        	foreach(Ipoint p in ipts1)
        	{
        		// Find which point in ipts2 is the nearest from p.        		
        		Ipoint best = (Ipoint)kdt2.NearestNeighbourListBBF(p, searchDepth);
        		Match m = new Match(p, best);
        		matches.Add(m);
        	}
        }
        public static void MatchPoint(Ipoint ipt, List<Ipoint> ipts, out Match m)
        {
        	// Find the nearest neighbour of ipt point, in ipts set.
        	// K-D tree implementation from Sebastian Nowozin's Autopano-sift.

        	ArrayList aTreePoints = new ArrayList(ipts);
        	KDTree kdt2 = KDTree.CreateKDTree(aTreePoints);
        	
        	int searchDepth = (int)Math.Max(130.0, (Math.Log (aTreePoints.Count) / Math.Log (1000.0)) * 130.0);
    		
    		Ipoint best = (Ipoint)kdt2.NearestNeighbourListBBF(ipt, searchDepth);
    		m = new Match(ipt, best);
        }
    }
}
