using System;
using System.Collections.Generic;
using System.Text;

namespace OpenSURF
{
    public class Surf
    {
        IplImage img;
        List<Ipoint> ipts;
        int index;

        static float pi = (float)Math.PI;

        public Surf(IplImage img, List<Ipoint> ipts)
        {
            this.img = img;
            this.ipts = ipts;
        }

        //! Describe all features in the supplied vector
        public void getDescriptors(bool upright)
        {
            // Check there are Ipoints to be described
            if (ipts.Count==0) return;

            // Get the size of the vector for fixed loop bounds
            int ipts_size = ipts.Count;

            if (upright)
            {
                // U-SURF loop just gets descriptors
                for (int i = 0; i < ipts_size; ++i)
                {
                  // Set the Ipoint to be described
                  index = i;
                  // Extract upright (i.e. not rotation invariant) descriptors
                  getUprightDescriptor();
                }
            }
            else
            {
                // Main SURF-64 loop assigns orientations and gets descriptors
                for (int i = 0; i < ipts_size; ++i)
                {
                  // Set the Ipoint to be described
                  index = i;
                  // Assign Orientations and extract rotation invariant descriptors
                  getOrientation();
                  getDescriptor();
                }
            }
        }

        //! Calculate Haar wavelet responses in x direction
        float haarX(int row, int column, int s)
        {
          return COpenSURF.Area(img, column, row-s/2, s/2, s) 
            -1 * COpenSURF.Area(img, column-s/2, row-s/2, s/2, s);
        }

        //! Calculate Haar wavelet responses in y direction
        float haarY(int row, int column, int s)
        {
            return COpenSURF.Area(img, column - s / 2, row, s, s / 2)
            - 1 * COpenSURF.Area(img, column - s / 2, row - s / 2, s, s / 2);
        }

        void getOrientation()
        {
            Ipoint ipt = ipts[index];
            float gauss=0;
            float scale = ipt.scale;
            int s = COpenSURF.cvRound(scale);
            int r = COpenSURF.cvRound(ipt.y);
            int c = COpenSURF.cvRound(ipt.x);
            List<float> resX = new List<float>();
            List<float> resY = new List<float>();
            List<float> Ang = new List<float>();

            // calculate haar responses for points within radius of 6*scale
            for (int i = -6 * s; i <= 6 * s; i += s)
            {
                for (int j = -6 * s; j <= 6 * s; j += s)
                {
                    if (i * i + j * j < 36 * s * s)
                    {
                        gauss = COpenSURF.gaussian(i, j, 2.5f * s);

                        float _resx = gauss * haarX(r + j, c + i, 4 * s);
                        float _resy = gauss * haarY(r + j, c + i, 4 * s);

                        resX.Add(_resx);
                        resY.Add(_resy);

                        Ang.Add(COpenSURF.getAngle(_resx, _resy));
                    }
                }
            }

            // calculate the dominant direction 
            float sumX, sumY;
            float max=0, old_max = 0, orientation = 0, old_orientation = 0;
            float ang1, ang2, ang;

            // loop slides pi/3 window around feature point
            for(ang1 = 0; ang1 < 2*pi;  ang1+=0.2f)
            {

                ang2 = ( ang1+pi/3.0f > 2*pi ? ang1-5.0f*pi/3.0f : ang1+pi/3.0f);
                sumX = sumY = 0; 

                for(int k = 0; k < Ang.Count; k++) 
                {
                  // get angle from the x-axis of the sample point
                  ang = Ang[k];

                  // determine whether the point is within the window
                  if (ang1 < ang2 && ang1 < ang && ang < ang2) 
                  {
                      sumX += resX[k];
                      sumY += resY[k];
                  } 
                  else if (ang2 < ang1 && 
                                ((ang > 0 && ang < ang2) || (ang > ang1 && ang < 2*pi) )) 
                  {
                      sumX += resX[k];
                      sumY += resY[k];
                  }
                }

                // if the vector produced from this window is longer than all 
                // previous vectors then this forms the new dominant direction
                if (sumX*sumX + sumY*sumY > max) 
                {
                  // store second largest orientation
                  old_max = max;
                  old_orientation = orientation;

                  // store largest orientation
                  max = sumX*sumX + sumY*sumY;
                  orientation = COpenSURF.getAngle(sumX, sumY);
                }

            } // for(ang1 = 0; ang1 < 2*pi;  ang1+=0.2f)

            // check whether there are two dominant orientations based on 0.8 threshold
            if (old_max >= 0.8 * max)
            {
                // assign second largest orientation and push copy onto vector
                ipt.orientation = old_orientation;
                ipts.Add(ipt);

                // Reset ipt to point to correct Ipoint in the vector
                ipt = ipts[index];
            }

            // assign orientation of the dominant response vector
            ipt.orientation = orientation;

        }

        void getDescriptor()
        {
            int y, x, count=0;
            float dx, dy, mdx, mdy, co, si;
            float[] desc;
            int scale;
            int sample_x;
            int sample_y;
            float gauss, rx, ry, rrx, rry, len=0;
            Ipoint ipt = ipts[index];

            scale = (int)ipt.scale;
            x = COpenSURF.cvRound(ipt.x);
            y = COpenSURF.cvRound(ipt.y);  
            co = (float)Math.Cos(ipt.orientation);
            si = (float)Math.Sin(ipt.orientation);
            desc = ipt.descriptor;

            // Calculate descriptor for this interest point
            for (int i = -10; i < 10; i+=5)
            {

                for (int j = -10; j < 10; j+=5)
                {

                    dx=dy=mdx=mdy=0;

                    for (int k = i; k < i + 5; ++k) 
                    {
                        for (int l = j; l < j + 5; ++l) 
                        {
                          // Get coords of sample point on the rotated axis
                          sample_x = COpenSURF.cvRound(x + (-l*scale*si + k*scale*co));
                          sample_y = COpenSURF.cvRound(y + (l * scale * co + k * scale * si));

                          // Get the gaussian weighted x and y responses
                          gauss = COpenSURF.gaussian(k * scale, l * scale, 3.3f * scale);
                          rx = gauss * haarX(sample_y, sample_x, 2 * scale);
                          ry = gauss * haarY(sample_y, sample_x, 2 * scale);

                          // Get the gaussian weighted x and y responses on rotated axis
                          rrx = -rx*si + ry*co;
                          rry = rx*co + ry*si;

                          dx += rrx;
                          dy += rry;
                          mdx += COpenSURF.fabs(rrx);
                          mdy += COpenSURF.fabs(rry);
                        }
                    }

                    // add the values to the descriptor vector
                    desc[count++] = dx;
                    desc[count++] = dy;
                    desc[count++] = mdx;
                    desc[count++] = mdy;

                    // store the current length^2 of the vector
                    len += dx*dx + dy*dy + mdx*mdx + mdy*mdy;

                } // for (int j = -10; j < 10; j+=5)

            } // for (int i = -10; i < 10; i+=5)

            // convert to unit vector
            len = (float)Math.Sqrt(len);
            for (int i = 0; i < 64; i++)
            {
                desc[i] /= len;
            }

        }

        void getUprightDescriptor()
        {
            int y, x, count=0;
            int scale;
            float dx, dy, mdx, mdy;
            float gauss, rx, ry, len = 0.0f;
            float[] desc;

            Ipoint ipt = ipts[index];
            scale = (int)ipt.scale;
            y = COpenSURF.cvRound(ipt.y);
            x = COpenSURF.cvRound(ipt.x);
            desc = ipt.descriptor;

            // Calculate descriptor for this interest point
            for (int i = -10; i < 10; i+=5)
            {
                for (int j = -10; j < 10; j+=5) 
                {
                    dx=dy=mdx=mdy=0;
                    for (int k = i; k < i + 5; ++k) 
                    {
                        for (int l = j; l < j + 5; ++l) 
                        {
                          // get Gaussian weighted x and y responses
                          gauss = COpenSURF.gaussian(k*scale, l*scale, 3.3f*scale);  
                          rx = gauss * haarX(k*scale+y, l*scale+x, 2*scale);
                          ry = gauss * haarY(k*scale+y, l*scale+x, 2*scale);

                          dx += rx;
                          dy += ry;
                          mdx += COpenSURF.fabs(rx);
                          mdy += COpenSURF.fabs(ry);
                        }
                    }

                    // add the values to the descriptor vector
                    desc[count++] = dx;
                    desc[count++] = dy;
                    desc[count++] = mdx;
                    desc[count++] = mdy;

                    // store the current length^2 of the vector
                    len += dx*dx + dy*dy + mdx*mdx + mdy*mdy;
                }
            }

            // convert to unit vector
            len = (float)Math.Sqrt(len);
            for(int i = 0; i < 64; i++)
            {
                desc[i] /= len;
            }

        }

    }
}
