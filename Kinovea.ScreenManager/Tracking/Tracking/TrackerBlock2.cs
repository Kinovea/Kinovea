#region License
/*
Copyright © Joan Charmant 2010.
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Kinovea.Services;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// TrackerBlock2 uses Template Matching through Normalized cross correlation to perform tracking.
    /// It uses TrackPointBlock to describe a tracked point.
    ///
    /// Working:
    /// To find the point in image I:
    /// - use the template found in image I-1.
    /// - save the template in point at image I.
    /// - no need to save the relative search window as points are saved in absolute coords.
    /// </summary>
    public class TrackerBlock2 : AbstractTracker
    {
        #region Properties
        public override TrackingParameters Parameters
        {
            get { return parameters; }
        }
        #endregion

        #region Members
        private TrackingParameters parameters = new TrackingParameters();
        private Bitmap mask;
        private OpenCvSharp.Mat cvMaskGray = new OpenCvSharp.Mat();

        // Monitoring, debugging.
        private static readonly bool monitoring = false;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public TrackerBlock2(TrackingParameters parameters)
        {
            this.parameters = parameters;
        }
        #endregion

        #region AbstractTracker Implementation
        public override bool Track(List<AbstractTrackPoint> previousPoints, Bitmap currentImage, Mat cvImage, long time, out AbstractTrackPoint currentPoint)
        {
            //---------------------------------------------------------------------
            // The input informations we have at hand are:
            // - The current bitmap we have to find the point into.
            // - The coordinates of all the previous points tracked.
            // - Previous tracking scores, stored in the TrackPoints tracked so far.
            //---------------------------------------------------------------------
            TrackPointBlock lastTrackPoint = (TrackPointBlock)previousPoints[previousPoints.Count - 1];
            PointF lastPoint = lastTrackPoint.Point;
            PointF subpixel = new PointF(lastPoint.X - (int)lastPoint.X, lastPoint.Y - (int)lastPoint.Y);

            bool matched = false;
            currentPoint = null;

            if (lastTrackPoint.Template != null && currentImage != null)
            {
                // Center search zone around last point.
                PointF searchCenter = lastPoint;
                Rectangle searchZone = new Rectangle(	(int)(searchCenter.X - (parameters.SearchWindow.Width/2)),
                                                        (int)(searchCenter.Y - (parameters.SearchWindow.Height/2)),
                                                        parameters.SearchWindow.Width,
                                                        parameters.SearchWindow.Height);

                searchZone.Intersect(new Rectangle(0, 0, cvImage.Width, cvImage.Height));
                var cvTemplate = BitmapConverter.ToMat(lastTrackPoint.Template);
                var cvImageROI = cvImage[searchZone.Y, searchZone.Y + searchZone.Height, searchZone.X, searchZone.X + searchZone.Width];

                int resWidth = searchZone.Width - lastTrackPoint.Template.Width + 1;
                int resHeight = searchZone.Height - lastTrackPoint.Template.Height + 1;

                
                // Make a mask to avoid matching on the background.
                System.Drawing.Size tplSize = new System.Drawing.Size(cvTemplate.Width, cvTemplate.Height);
                if (mask == null || mask.Width != tplSize.Width || mask.Height != tplSize.Height)
                {
                    if (mask != null)
                    {
                        mask.Dispose();
                    }

                    // Paint a white ellipse on a blank template.
                    // Store it in cvMaskGray.
                    mask = new Bitmap(tplSize.Width, tplSize.Height);
                    Graphics g = Graphics.FromImage(mask);
                    g.FillEllipse(Brushes.White, new Rectangle(System.Drawing.Point.Empty, tplSize));
                    var cvMask = OpenCvSharp.Extensions.BitmapConverter.ToMat(mask);
                    OpenCvSharp.Cv2.CvtColor(cvMask, cvMaskGray, OpenCvSharp.ColorConversionCodes.BGR2GRAY, 0);
                    cvMask.Dispose();
                }

                Mat similarityMap = new Mat(new OpenCvSharp.Size(resWidth, resHeight), MatType.CV_32FC1);
                Cv2.MatchTemplate(cvImageROI, cvTemplate, similarityMap, TemplateMatchModes.CCoeffNormed, cvMaskGray);

                // Find max
                double bestScore = 0;
                PointF bestCandidate = new PointF(-1,-1);

                double min = 0;
                double max = 0;
                OpenCvSharp.Point minLoc;
                OpenCvSharp.Point maxLoc;
                Cv2.MinMaxLoc(similarityMap, out min, out max, out minLoc, out maxLoc);

                if(max > parameters.SimilarityThreshold)
                {
                    PointF loc = RefineLocation(similarityMap, maxLoc, parameters.RefinementNeighborhood);
                
                    // The template matching was done on a template aligned with the integer part of the actual position.
                    // We reinject the floating point part of the orginal positon into the result.
                    loc = loc.Translate(subpixel.X, subpixel.Y);

                    bestCandidate = new PointF(searchZone.Left + loc.X + cvTemplate.Width / 2, searchZone.Top + loc.Y + cvTemplate.Height / 2);
                    bestScore = max;
                }

                cvImageROI.Dispose();
                cvTemplate.Dispose();

                #region Monitoring
                //if(monitoring)
                //{
                //    // Save the similarity map to file.
                //    //Mat mapNormalized = new Mat(new OpenCvSharp.Size(resWidth, resHeight), MatType.CV_32FC1);
                //    //Image<Gray, Byte> mapNormalized = new Image<Gray, Byte>(similarityMap.Width, similarityMap.Height);
                //    //CvInvoke.cvNormalize(similarityMap.Ptr, mapNormalized.Ptr, 0, 255, NORM_TYPE.CV_MINMAX, IntPtr.Zero);
                //    Mat map8u = new Mat(new OpenCvSharp.Size(resWidth, resHeight), MatType.CV_8U);
                //    //similarityMap.ConvertTo(map8u, MatType.CV_8U);
                //    Cv2.Normalize(similarityMap, map8u, 0, 255, NormTypes.MinMax, MatType.CV_8U);

                //    Bitmap bmpMap = map8u.ToBitmap();
                //    string tplDirectory = @"G:\temp\simimap";
                //    bmpMap.Save(tplDirectory + string.Format(@"\simiMap-{0:000}-{1:0.00}.png", previousPoints.Count, bestScore));
                //}
                #endregion

                similarityMap.Dispose();

                // Result of the matching.
                if(bestCandidate.X != -1 && bestCandidate.Y != -1)
                {
                    currentPoint = CreateTrackPoint(false, bestCandidate, bestScore, time, currentImage, previousPoints);
                    ((TrackPointBlock)currentPoint).Similarity = bestScore;
                }
                else
                {
                    // No match. Create the point at the center of the search window (whatever that might be).
                    currentPoint = CreateTrackPoint(false, lastPoint, max, time, currentImage, previousPoints);
                    log.DebugFormat("Track failed. Best candidate: {0} < {1}.", max, parameters.SimilarityThreshold);

                    // from master.
                    //currentPoint = CreateTrackPoint(false, lastPoint, 0.0f, position, img, previousPoints);
                    //log.Debug("Track failed. No block over the similarity treshold in the search window.");
                }

                matched = true;
            }
            else
            {
                // No image. (error case ?)
                // Create the point at the last point location.
                currentPoint = CreateTrackPoint(false, lastPoint, 0.0f, time, currentImage, previousPoints);
                log.Debug("Track failed. No input image, or last point doesn't have any cached block image.");
            }

            return matched;
        }
        public override AbstractTrackPoint CreateTrackPoint(bool manual, PointF p, double similarity, long time, Bitmap currentImage, List<AbstractTrackPoint> previousPoints)
        {
            // Creates a TrackPoint from the input image at the given coordinates.
            // Stores algorithm internal data in the point, to help next match.
            // time is in relative timestamps from the first point.

            // Copy the template from the image into its own Bitmap.

            Bitmap tpl = new Bitmap(parameters.BlockWindow.Width, parameters.BlockWindow.Height, PixelFormat.Format32bppPArgb);
            int age = 0;

            bool updateWithCurrentImage = true;

            if(!manual && previousPoints.Count > 0 && similarity > parameters.TemplateUpdateThreshold || similarity < parameters.SimilarityThreshold)
            {
                // Do not update the template if it's not that different.
                TrackPointBlock prevBlock = previousPoints[previousPoints.Count - 1] as TrackPointBlock;
                if(prevBlock != null && prevBlock.Template != null)
                {
                    tpl = BitmapHelper.Copy(prevBlock.Template);
                    updateWithCurrentImage = false;
                    age = prevBlock.TemplateAge + 1;
                }
            }


            if(updateWithCurrentImage)
            {
                BitmapData imageData = currentImage.LockBits( new Rectangle( 0, 0, currentImage.Width, currentImage.Height ), ImageLockMode.ReadOnly, currentImage.PixelFormat );
                BitmapData templateData = tpl.LockBits(new Rectangle( 0, 0, tpl.Width, tpl.Height ), ImageLockMode.ReadWrite, tpl.PixelFormat );

                int pixelSize = 4;

                int tplStride = templateData.Stride;
                int templateWidthInBytes = parameters.BlockWindow.Width * pixelSize;
                int tplOffset = tplStride - templateWidthInBytes;

                int imgStride = imageData.Stride;
                int imageWidthInBytes = currentImage.Width * pixelSize;
                int imgOffset = imgStride - (currentImage.Width * pixelSize) + imageWidthInBytes - templateWidthInBytes;

                int startY = (int)(p.Y - (parameters.BlockWindow.Height / 2.0));
                int startX = (int)(p.X - (parameters.BlockWindow.Width / 2.0));

                if(startX < 0)
                    startX = 0;

                if(startY < 0)
                    startY = 0;

                unsafe
                {
                    byte* pTpl = (byte*) templateData.Scan0.ToPointer();
                    byte* pImg = (byte*) imageData.Scan0.ToPointer()  + (imgStride * startY) + (pixelSize * startX);

                    for ( int row = 0; row < parameters.BlockWindow.Height; row++ )
                    {
                        if(startY + row > imageData.Height - 1)
                        {
                            break;
                        }

                        for ( int col = 0; col < templateWidthInBytes; col++, pTpl++, pImg++ )
                        {
                            if(startX * pixelSize + col < imageWidthInBytes)
                            {
                                *pTpl = *pImg;
                            }
                        }

                        pTpl += tplOffset;
                        pImg += imgOffset;
                    }
                }

                currentImage.UnlockBits( imageData );
                tpl.UnlockBits( templateData );
            }

            #region Monitoring
            if(monitoring && updateWithCurrentImage)
            {
                // Save current template to file, to visually monitor the drift.
                //string tplDirectory = @"";
                //if(previousPoints.Count <= 1)
                //{
                //    // Clean up folder.
                //    string[] tplFiles = Directory.GetFiles(tplDirectory, "*.bmp");
                //    foreach (string f in tplFiles)
                //    {
                //        File.Delete(f);
                //    }
                //}
                //String iFileName = String.Format("{0}\\tpl-{1:000}.bmp", tplDirectory, previousPoints.Count);
                //tpl.Save(iFileName);
            }
            #endregion

            TrackPointBlock tpb = new TrackPointBlock(p.X, p.Y, time, tpl);
            //TrackPointBlock tpb = new TrackPointBlock(p.X, p.Y, t, tpl);
            tpb.TemplateAge = age;
            tpb.IsReferenceBlock = manual;
            tpb.Similarity = manual ? 1.0f : similarity;

            return tpb;
        }
        public override AbstractTrackPoint CreateOrphanTrackPoint(PointF p, long t)
        {
            // This creates a bare bone TrackPoint.
            // This is used only in the case of importing from xml.
            // The TrackPoint can't be used as-is to track the next one because it's missing the algo internal data (block).
            // We'll need to reconstruct it when we have the corresponding image.
            return new TrackPointBlock(p.X, p.Y, t);
        }
        public override void Draw(Graphics canvas, AbstractTrackPoint point, IImageToViewportTransformer transformer, Color color, double opacityFactor)
        {
            // Draw the search and template boxes around the point.
            var p = transformer.Transform(point.Point);
            Rectangle rectSearch = p.Box(transformer.Transform(parameters.SearchWindow));
            Rectangle rectTemplate = p.Box(transformer.Transform(parameters.BlockWindow));

            using(Pen pen = new Pen(Color.FromArgb((int)(opacityFactor * 192), color)))
            {
                canvas.DrawRectangle(pen, rectSearch);
                canvas.DrawRectangle(pen, rectTemplate);
                canvas.DrawEllipse(pen, rectTemplate);

                DrawDebugInfo(canvas, point, rectSearch, color);
            }
        }

        private void DrawDebugInfo(Graphics canvas, AbstractTrackPoint point, RectangleF search, Color color)
        {
            TrackPointBlock tpb = point as TrackPointBlock;

            if (tpb == null)
                return;

            Font f = new Font("Consolas", 10, FontStyle.Bold);
            string text = string.Format("{0:0.000} ({1})", tpb.Similarity, tpb.TemplateAge);
            using (Brush b = new SolidBrush(color))
                canvas.DrawString(text, f, b, search.Location.Translate(0, -25));

            f.Dispose();
        }
        #endregion

        /// <summary>
        /// Computes the center of mass of the similarity scores in the vicinity of the best candidate.
        /// This allows to find a floating point location for the best match.
        /// </summary>
        private PointF RefineLocation(Mat map, OpenCvSharp.Point loc, int neighborhood)
        {
            // The best candidate location is expanded by "neighborhood" pixels in each direction.
            float numX = 0;
            float numY = 0;
            float den = 0;
            for (int i = loc.X - neighborhood; i <= loc.X + neighborhood; i++)
            {
                if (i < 0 || i >= map.Cols)
                    continue;

                for (int j = loc.Y - neighborhood; j <= loc.Y + neighborhood; j++)
                {
                    if (j < 0 || j >= map.Rows)
                        continue;

                    // Weight each location by its similarity score.
                    float value = map.Get<float>(j, i);
                    numX += (i * value);
                    numY += (j * value);
                    den += value;
                }
            }

            float x = numX / den;
            float y = numY / den;
            return new PointF(x, y);
        }
    }
}
