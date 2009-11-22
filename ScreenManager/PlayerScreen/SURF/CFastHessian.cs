
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace OpenSURF
{

    public class CFastHessian
    {
        public const int OCTAVES = 3;
        public const int INTERVALS = 4;
        public const float THRES = 0.002f;
        public const int INIT_SAMPLE = 2;
        public const int INTERP_STEPS = 5;

        public int octaves = OCTAVES;
        public int intervals = INTERVALS;
        public int init_sample = INIT_SAMPLE;
        public float thres = THRES;
        public int interp_steps = INTERP_STEPS;

        IplImage img;
        int i_width;
        int i_height;
        float[] m_det;

        List<Ipoint> ipts;

        public CFastHessian(List<Ipoint> aIpoint, int octaves, int intervals, int init_sample, 
                                 float thres, int interp_steps)
        {
            this.octaves = octaves;
            this.intervals = intervals;
            this.init_sample = init_sample;
            this.thres = thres;
            this.interp_steps = interp_steps;
        }

        public CFastHessian(IplImage img,
                                ref List<Ipoint> aIpoint, int octaves, int intervals, int init_sample,
                                float thres, int interp_steps)
        {
            ipts = new List<Ipoint>();
            aIpoint = ipts;

            this.octaves = octaves;
            this.intervals = intervals;
            this.init_sample = init_sample;
            this.thres = thres;
            this.interp_steps = interp_steps;

            setIntImage(img);
        }

        public void save_Det(string Path)
        {
            FileStream pfd = null;
            try
            {
                pfd = new FileStream(Path+".DET", FileMode.Create, FileAccess.Write);
                BinaryWriter pbw = new BinaryWriter(pfd);
                pbw.Write(octaves);
                pbw.Write(intervals);
                pbw.Write(i_width);
                pbw.Write(i_height);
                foreach (float vdet in m_det)
                {
                    pbw.Write(vdet);
                }
            }
            finally
            {
                if (pfd != null) pfd.Close();
            }
        }

        #region --- private

        void setIntImage(IplImage img)
        {
            this.img = img;
            this.i_width = img.width;
            this.i_height = img.height;
            this.m_det = new float[octaves * intervals * i_width * i_height];
        }

        void buildDet()
        {
            int lobe, border, step;
            float Dxx=0, Dyy=0, Dxy=0, scale;
            int ixdet = 0;

            for (int o = 0; o < octaves; o++)
            {
                // calculate filter border for this octave
                border = (3 * COpenSURF.cvRound(COpenSURF.pow(2.0f, o + 1) * (intervals) + 1) + 1) / 2;
                step = init_sample * COpenSURF.cvRound(COpenSURF.pow(2.0f, o));

                for (int i = 0; i < intervals; i++)
                {
                    // calculate lobe length (filter side length/3)
                    lobe = COpenSURF.cvRound(COpenSURF.pow(2.0f, o + 1) * (i + 1) + 1);
                    scale = 1.0f / COpenSURF.pow((float)(3 * lobe), 2);

                    for (int r = border; r < i_height - border; r += step)
                    {

                        for (int c = border; c < i_width - border; c += step)
                        {
                            /***
                            Dyy = COpenSURF.Area(img, c - (lobe - 1), r - ((3 * lobe - 1) / 2), (2 * lobe - 1), lobe)
                               - 2 * COpenSURF.Area(img, c - (lobe - 1), r - ((lobe - 1) / 2), (2 * lobe - 1), lobe)
                                 + COpenSURF.Area(img, c - (lobe - 1), r + ((lobe + 1) / 2), (2 * lobe - 1), lobe);


                            Dxx = COpenSURF.Area(img, c - ((3 * lobe - 1) / 2), r - (lobe - 1), lobe, (2 * lobe - 1))
                               - 2 * COpenSURF.Area(img, c - ((lobe - 1) / 2), r - (lobe - 1), lobe, (2 * lobe - 1))
                                 + COpenSURF.Area(img, c + ((lobe + 1) / 2), r - (lobe - 1), lobe, (2 * lobe - 1));

                            Dxy = COpenSURF.Area(img, c - lobe, r - lobe, lobe, lobe)
                                  + COpenSURF.Area(img, c + 1, r + 1, lobe, lobe)
                                  - COpenSURF.Area(img, c - lobe, r + 1, lobe, lobe)
                                  - COpenSURF.Area(img, c + 1, r - lobe, lobe, lobe);
                            ***/
                            {
                                float dyy0 = COpenSURF.Area(img, c - (lobe - 1), r - ((3 * lobe - 1) / 2), (2 * lobe - 1), lobe);
                                float dyy1 = COpenSURF.Area(img, c - (lobe - 1), r - ((lobe - 1) / 2), (2 * lobe - 1), lobe);
                                float dyy2 = COpenSURF.Area(img, c - (lobe - 1), r + ((lobe + 1) / 2), (2 * lobe - 1), lobe);
                                Dyy = dyy0 - 2 * dyy1 + dyy2;
                            }
                            {
                                float dxx0 = COpenSURF.Area(img, c - ((3 * lobe - 1) / 2), r - (lobe - 1), lobe, (2 * lobe - 1));
                                float dxx1 = COpenSURF.Area(img, c - ((lobe - 1) / 2), r - (lobe - 1), lobe, (2 * lobe - 1));
                                float dxx2 = COpenSURF.Area(img, c + ((lobe + 1) / 2), r - (lobe - 1), lobe, (2 * lobe - 1));
                                Dxx = dxx0 - 2 * dxx1 + dxx2;
                            }
                            {
                                float dxy0 = COpenSURF.Area(img, c - lobe, r - lobe, lobe, lobe);
                                float dxy1 = COpenSURF.Area(img, c + 1, r + 1, lobe, lobe);
                                float dxy2 = COpenSURF.Area(img, c - lobe, r + 1, lobe, lobe);
                                float dxy3 = COpenSURF.Area(img, c + 1, r - lobe, lobe, lobe);
                                Dxy = dxy0 + dxy1 - dxy2 - dxy3;
                            }

                            // Normalise the filter responses with respect to their size
                            Dxx *= scale;
                            Dyy *= scale;
                            Dxy *= scale;

                            // Get the sign of the laplacian
                            int lap_sign = (Dxx + Dyy >= 0 ? 1 : -1);

                            // Get the determinant of hessian response
                            float res = (Dxx * Dyy - 0.9f * 0.9f * Dxy * Dxy);
                            res = (res < thres ? 0 : lap_sign * res);

                            // calculate approximated determinant of hessian value
                            m_det[(o * intervals + i) * (i_width * i_height) + (r * i_width + c)] = res;
                            ixdet += 1;

                            if (res > 0)
                            {
                            }

                        }

                    }

                }

            }

        }

        //! Return the value of the approximated determinant of hessian
        float getVal(int o, int i, int c, int r)
        {
            return COpenSURF.fabs(m_det[(o * intervals + i) * (i_width * i_height) + (r * i_width + c)]);
        }

        //! Return the sign of the laplacian (trace of the hessian)
        float getLaplacian(int o, int i, int c, int r)
        {
          float res = (m_det[(o*intervals+i)*(i_width*i_height) + (r*i_width+c)]);
          return (res >= 0 ? 1 : -1);
        }

        //! Perform a step of interpolation (fitting 3D quadratic)
        void stepInterp(int o, int i, int c, int r, float[] x)
        {
          float v, dx, dy, ds, dxx, dyy, dss, dxy, dxs, dys, det;
          int step = init_sample * COpenSURF.cvRound(COpenSURF.pow(2.0f,o));

          // value of current pixel
          v = getVal(o, i, c, r);

          // first order derivs in 3D
          dx = ( getVal(o, i, c+step, r) - getVal(o, i, c-step, r) ) / 2.0f;
          dy = ( getVal(o, i, c, r+step) - getVal(o, i, c, r-step) ) / 2.0f;
          ds = ( getVal(o, i+1, c, r) - getVal(o, i-1, c, r) ) / 2.0f;

          // second order derivs in 3D
          dxx = ( getVal(o, i, c+step, r) + getVal(o, i, c-step, r) - 2 * v );
          dyy = ( getVal(o, i, c, r+step) + getVal(o, i, c, r-step) - 2 * v );
          dss = ( getVal(o, i+1, c, r) +	getVal(o, i-1, c, r) - 2 * v );
          dxy = ( getVal(o, i, c+step, r+step) -	getVal(o, i, c-step, r+step) -
                  getVal(o, i, c+step, r-step) +	getVal(o, i, c-step, r-step) ) / 4.0f;
          dxs = ( getVal(o, i+1, c+step, r) -	getVal(o, i+1, c-step, r) -
                  getVal(o, i-1, c+step, r) +	getVal(o, i-1, c-step, r) ) / 4.0f;
          dys = ( getVal(o, i+1, c, r+step) -	getVal(o, i+1, c, r-step) -
                  getVal(o, i-1, c, r+step) +	getVal(o, i-1, c, r-step) ) / 4.0f;

          // calculate determinant of:
          //	| dxx dxy dxs |
          //	| dxy dyy dys | 
          //	| dxs dys dss |
          det = dxx * ( dyy*dss-dys*dys) - dxy * (dxy*dss-dxs*dys) + dxs * (dxy*dys-dxs*dyy);

          // calculate resulting vector after matrix mult:
          //	| dxx dxy dxs |-1  | dx |
          //	| dxy dyy dys |  X | dy |
          //	| dxs dys dss |    | ds |
          x[0] = -1.0f/det * ( dx * ( dyy*dss-dys*dys ) + dy * ( dxs*dys-dss*dxy ) + ds * ( dxy*dys-dyy*dxs ) );
          x[1] = -1.0f/det * ( dx * ( dys*dxs-dss*dxy ) + dy * ( dxx*dss-dxs*dxs ) + ds * ( dxs*dxy-dys*dxx ) );
          x[2] = -1.0f/det * ( dx * ( dxy*dys-dxs*dyy ) + dy * ( dxy*dxs-dxx*dys ) + ds * ( dxx*dyy-dxy*dxy ) );
        }

        //! Interpolate feature to sub pixel accuracy
        void getIpoint(int o, int i, int c, int r)
        {
          bool converged=false;
          float[] x=new float[3];

          for(int steps = 0; steps <= interp_steps; ++steps) 
          {
            // perform a step of the interpolation
            stepInterp(o, i, c, r, x);

            // check stopping conditions
            if( COpenSURF.fabs(x[0]) < 0.5 && COpenSURF.fabs(x[1]) < 0.5 && COpenSURF.fabs(x[2]) < 0.5 ) 
            {
              converged = true;
              break;
            }

            // find coords of different sample point
            c += COpenSURF.cvRound( x[0] );
            r += COpenSURF.cvRound( x[1] );
            i += COpenSURF.cvRound( x[2] );

            // check all param are within bounds
            if (i < 1 || i >= intervals - 1 || c < 1 || r < 1 || c > i_width - 1 || r > i_height - 1)
            {
                return;
            }
          }

          // if interpolation has not converged on a result
          if(!converged)
          {
            return;
          }

          // create Ipoint and push onto Ipoints vector
          Ipoint ipt=new Ipoint();
          ipt.x = (float) (c + x[0]);
          ipt.y = (float) (r + x[1]);
          ipt.scale = (1.2f/9.0f) * (3*(COpenSURF.pow(2.0f, o+1) * (i+x[2]+1)+1));
          ipt.laplacian = (int)getLaplacian(o, i, c, r);
          if (ipts == null) ipts = new List<Ipoint>();
          ipts.Add(ipt);
        }

        //! Non Maximal Suppression function
        int isExtremum(int octave, int interval, int c, int r)
        {
          float val = getVal(octave, interval, c, r);
          int step = init_sample * COpenSURF.cvRound(COpenSURF.pow(2.0f,octave));

          // reject points with low response to the det of hessian function
          if(val < thres) return 0;

          // check for maximum 
          for (int i = -1; i <= 1; i++)
          {
              for (int j = -step; j <= step; j += step)
              {
                  for (int k = -step; k <= step; k += step)
                  {
                      if (i != 0 || j != 0 || k != 0)
                      {
                          if (getVal(octave, interval + i, c + j, r + k) > val)
                          {
                              return 0;
                          }
                      }
                  }
              }
          }

          return 1;
        }

        //! Find the image features and write into vector of features
        public void getIpoints()
        {
            int extremum_count = 0;
          // Clear the vector of exisiting ipts
          //!!!!ipts = null;

          // Calculate approximated determinant of hessian values
          buildDet();

          for(int o=0; o < octaves; o++) 
          {
            // for each octave double the sampling step of the previous
            int step = init_sample * COpenSURF.cvRound(COpenSURF.pow(2.0f,o));

            // determine border width for the largest filter for each ocave
            int border = (3 * COpenSURF.cvRound(COpenSURF.pow(2.0f,o+1)*(intervals)+1) + 1)/2;

            // check for maxima across the scale space
            for(int i = 1; i < intervals-1; ++i) 
              for(int r = border; r < i_height - border; r += step)
                for(int c = border; c < i_width - border; c += step)
                    if (isExtremum(o, i, c, r) != 0)
                    {
                        extremum_count += 1;
                        interp_extremum(o, i, r, c);
                    }
          } 

        }

        float getValLowe(int o, int i, int r, int c)
        {
            int index=(o*intervals+i)*(i_width*i_height) + (r*i_width+c);
            if (m_det == null) return 0;
            if (index < 0 || index >= m_det.Length) return 0;
            return COpenSURF.fabs(m_det[index]);
        }

        void deriv_3D(int octv, int intvl, int r, int c,
                            out double dx, out double dy, out double ds)
        {
            dx = dy = ds = 0;

            int step = init_sample * COpenSURF.cvRound(COpenSURF.pow(2.0f, octv));

            dx = (getValLowe(octv, intvl, r, c + step) -
                  getValLowe(octv, intvl, r, c - step)) / 2.0;
            dy = (getValLowe(octv, intvl, r + step, c) -
                getValLowe(octv, intvl, r - step, c)) / 2.0;
            ds = (getValLowe(octv, intvl + 1, r, c) -
                getValLowe(octv, intvl - 1, r, c)) / 2.0;

        }

        CDVMatrix hessian_3D(int octv, int intvl, int r, int c)
        {
            CDVMatrix vret = new CDVMatrix();

            double v, dxx, dyy, dss, dxy, dxs, dys;
            int step = init_sample * COpenSURF.cvRound(COpenSURF.pow(2.0f, octv));

            v = getValLowe(octv, intvl, r, c);
            dxx = (getValLowe(octv, intvl, r, c + step) +
                    getValLowe(octv, intvl, r, c - step) - 2 * v);
            dyy = (getValLowe(octv, intvl, r + step, c) +
                    getValLowe(octv, intvl, r - step, c) - 2 * v);
            dss = (getValLowe(octv, intvl + 1, r, c) +
                    getValLowe(octv, intvl - 1, r, c) - 2 * v);
            dxy = (getValLowe(octv, intvl, r + step, c + step) -
                    getValLowe(octv, intvl, r + step, c - step) -
                    getValLowe(octv, intvl, r - step, c + step) +
                    getValLowe(octv, intvl, r - step, c - step)) / 4.0;
            dxs = (getValLowe(octv, intvl + 1, r, c + step) -
                    getValLowe(octv, intvl + 1, r, c - step) -
                    getValLowe(octv, intvl - 1, r, c + step) +
                    getValLowe(octv, intvl - 1, r, c - step)) / 4.0;
            dys = (getValLowe(octv, intvl + 1, r + step, c) -
                    getValLowe(octv, intvl + 1, r - step, c) -
                    getValLowe(octv, intvl - 1, r + step, c) +
                    getValLowe(octv, intvl - 1, r - step, c)) / 4.0;

            /***
                        cvmSet( H, 0, 0, dxx );
                        cvmSet( H, 0, 1, dxy );
                        cvmSet( H, 0, 2, dxs );
             * 
                        cvmSet( H, 1, 0, dxy );
                        cvmSet( H, 1, 1, dyy );
                        cvmSet( H, 1, 2, dys );
             * 
                        cvmSet( H, 2, 0, dxs );
                        cvmSet( H, 2, 1, dys );
                        cvmSet( H, 2, 2, dss );
            ***/

            vret.M11 = dxx;
            vret.M12 = dxy;
            vret.M13 = dxs;

            vret.M21 = dxy;
            vret.M22 = dyy;
            vret.M23 = dys;

            vret.M31 = dxs;
            vret.M32 = dys;
            vret.M33 = dss;

            return vret;
        }

        bool interp_step( int octv, int intvl, int r, int c, out double xi, out double xr, out double xc )
        {
            xi = xr = xc = 0;

            double _dx=0;
            double _dy=0;
            double _ds=0;
            deriv_3D(octv, intvl, r, c, out _dx, out _dy, out _ds);

            CDVMatrix H = hessian_3D(octv, intvl, r, c);
            if (H == null) return false;

            CDVMatrix H_inv = CDVMatrix.Inversion(H);
            if (H_inv == null) return false;

            H_inv.MultiplyVector(_dx, _dy, _ds, out xi, out xr, out xc);

            return true;
        }

        void interp_extremum(int octv, int intvl, int r, int c)
        {
            double xi = 0, xr = 0, xc = 0;
            int step = init_sample * COpenSURF.cvRound(COpenSURF.pow(2.0f, octv));

            // Get the offsets to the actual location of the extremum
            bool bok = interp_step( octv, intvl, r, c, out xi, out xr, out xc );
            if (bok == false) return;
      
            // If point is sufficiently close to the actual extremum
            if (COpenSURF.fabs((float)xi) <= 0.5 && COpenSURF.fabs((float)xr) <= 0.5 && COpenSURF.fabs((float)xc) <= 0.5)
            {
                // Create Ipoint and push onto Ipoints vector
                Ipoint ipt =new Ipoint();
                ipt.x = (float) (c + step*xc);
                ipt.y = (float) (r + step*xr);
                ipt.scale = (float)((1.2f / 9.0f) * (3 * (COpenSURF.pow(2.0f, octv + 1) * (intvl + xi + 1) + 1)));
                ipt.laplacian = (int)getLaplacian(octv, intvl, c, r);
                ipts.Add(ipt);
            }

        }

        #endregion

    }

}
