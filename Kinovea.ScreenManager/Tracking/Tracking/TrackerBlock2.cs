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
using System.Drawing.Drawing2D;

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
            // - The current bitmap we have to find the template into.
            // - The coordinates of all the previous points tracked.
            // - Previous tracking scores, stored in the TrackPoints tracked so far.
            //---------------------------------------------------------------------
            TrackPointBlock lastTrackPoint = (TrackPointBlock)previousPoints[previousPoints.Count - 1];
            
            if (lastTrackPoint.Template == null ||
                currentImage == null ||
                lastTrackPoint.Template.Width != parameters.BlockWindow.Width ||
                lastTrackPoint.Template.Height != parameters.BlockWindow.Height)
            {
                // No image or wrong image size.
                // Create the point at the last point location.
                currentPoint = CreateTrackPoint(false, lastTrackPoint.Point, 0.0f, time, currentImage, previousPoints);
                log.Debug("Track failed. No input image, or last point doesn't have any cached block image.");

                return false;
            }

            // The template matching is aligned with the pixel grid.
            // This means the point we are tracking is not necessarily at the center of
            // the template/search boxes used for matching.
            // We match aligned with the grid and re-inject the offset at the end.
            PointF lastPoint = lastTrackPoint.Point;
            PointF lastPointAligned = new PointF((int)lastPoint.X, (int)lastPoint.Y);
            PointF pixelFract = new PointF(lastPoint.X - lastPointAligned.X, lastPoint.Y - lastPointAligned.Y);
            //log.DebugFormat("lastPoint:{0}, lastPointAligned:{1}, pixelFract:{2}", lastPoint, lastPointAligned, pixelFract);

            // The boxes themselves may have odd or even sizes.
            System.Drawing.Size srchSize = parameters.SearchWindow;
            System.Drawing.Size tmplSize = parameters.BlockWindow;
            PointF srchTopLeft = new PointF(lastPointAligned.X - (int)(srchSize.Width / 2.0f), lastPointAligned.Y - (int)(srchSize.Height / 2.0f));
            PointF tmplTopLeft = new PointF(lastPointAligned.X - (int)(tmplSize.Width / 2.0f), lastPointAligned.Y - (int)(tmplSize.Height / 2.0f));

            // Current best guess for the integer location of the template within the search window.
            PointF lastLoc = new PointF(tmplTopLeft.X - srchTopLeft.X, tmplTopLeft.Y - srchTopLeft.Y);
            //log.DebugFormat("srchTopLeft:{0}, tmplTopLeft:{1}, lastLoc:{2}", srchTopLeft, tmplTopLeft, lastLoc);

            Rectangle srchRect = new Rectangle((int)srchTopLeft.X, (int)srchTopLeft.Y, srchSize.Width, srchSize.Height);
            //log.DebugFormat("srchRect:{0}", srchRect);

            srchRect.Intersect(new Rectangle(0, 0, cvImage.Width, cvImage.Height));
            var cvTemplate = BitmapConverter.ToMat(lastTrackPoint.Template);
            var cvImageROI = cvImage[srchRect.Y, srchRect.Y + srchRect.Height, srchRect.X, srchRect.X + srchRect.Width];
                
            // Make a mask to avoid matching on the background.
                
            currentPoint = null;
            if (mask == null || mask.Width != tmplSize.Width || mask.Height != tmplSize.Height)
            {
                if (mask != null)
                {
                    mask.Dispose();
                }

                // Paint a white ellipse on a black template.
                // Store it in cvMaskGray.
                mask = new Bitmap(tmplSize.Width, tmplSize.Height);
                Graphics g = Graphics.FromImage(mask);
                g.Clear(Color.Black);
                g.FillEllipse(Brushes.White, new Rectangle(System.Drawing.Point.Empty, tmplSize));

                //mask.Save(@"G:\temp\mask\mask.png");

                var cvMask = OpenCvSharp.Extensions.BitmapConverter.ToMat(mask);
                OpenCvSharp.Cv2.CvtColor(cvMask, cvMaskGray, OpenCvSharp.ColorConversionCodes.BGR2GRAY, 0);
                cvMask.Dispose();
            }

            // Prepare the result similarity map as a sliding of the template inside the search window.
            int smapWidth  = srchRect.Width  - tmplSize.Width  + 1;
            int smapHeight = srchRect.Height - tmplSize.Height + 1;
            Mat cvSimiMap = new Mat(new OpenCvSharp.Size(smapWidth, smapHeight), MatType.CV_32FC1);

            // Perform the actual template matching.
            // This fills the map with the score at each candidate location.
            Cv2.MatchTemplate(cvImageROI, cvTemplate, cvSimiMap, TemplateMatchModes.CCoeffNormed, cvMaskGray);

            // Find the top value.
            double min = 0;
            double max = 0;
            OpenCvSharp.Point minLoc;
            OpenCvSharp.Point maxLoc;
            Cv2.MinMaxLoc(cvSimiMap, out min, out max, out minLoc, out maxLoc);

            // For some reason the map may contain Infinity values near the edges (only with masking?).
            //log.DebugFormat("With mask: maxloc:{0}, max:{1}", maxLoc, max);
            if (double.IsInfinity(max) || max > 1.0)
            {
                // Retry without the mask.
                Cv2.MatchTemplate(cvImageROI, cvTemplate, cvSimiMap, TemplateMatchModes.CCoeffNormed);
                Cv2.MinMaxLoc(cvSimiMap, out min, out max, out minLoc, out maxLoc);
                //log.DebugFormat("No mask: maxloc:{0}, max:{1}", maxLoc, max);
            }

            
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

            if (double.IsInfinity(max) || max < parameters.SimilarityThreshold)
            {
                // Tracking failure.
                // Keep the point at the previous location.
                currentPoint = CreateTrackPoint(false, lastPoint, 0.0f, time, currentImage, previousPoints);
                log.DebugFormat("Tracking failed. Best candidate: {0} < {1}.", max, parameters.SimilarityThreshold);
            }
            else
            { 
                // Tracking success. Try to refine the best location based on neighbors scores.
                PointF loc = PointF.Empty;

                bool doRefine = false;

                // If the best candidate is on the edge, there is no point trying to refine it.
                // This is very suspicious and likely a failure case.
                // If the best score is 1.0 (how?) we also don't refine.
                if (!doRefine || max == 1.0 || maxLoc.X == 0 || maxLoc.X == cvSimiMap.Width - 1 || maxLoc.Y == 0 || maxLoc.Y == cvSimiMap.Height - 1)
                {
                    loc = new PointF(maxLoc.X, maxLoc.Y);
                    //log.DebugFormat("Non refined location: {0}", loc);
                }
                else
                {
                    // If the top result is over the "good" threshold, only consider other good matches in the neighborhood.
                    // Otherwise consider all "fair" matches.
                    // Don't consider matches that wouldn't have passed the threshold, they are just adding noise.
                    float threshold = max >= parameters.TemplateUpdateThreshold ? (float)parameters.TemplateUpdateThreshold : (float)parameters.SimilarityThreshold;
                    loc = RefineLocation(cvSimiMap, maxLoc, threshold);
                    //log.DebugFormat("Refined location: {0}", loc);
                }

                // What we have at this point is the location of the top left of the template within the search window.
                // Find back the actual center point based on truncated half size.
                // And reinject the sub-pixel offset of the original position into the result since
                // the template matching was done on the integer pixel grid.
                PointF bestCandidate = new PointF(
                    srchRect.X + loc.X + (int)(cvTemplate.Width / 2.0f)  + pixelFract.X, 
                    srchRect.Y + loc.Y + (int)(cvTemplate.Height / 2.0f) + pixelFract.Y);
                //log.DebugFormat("Final point: {0}", bestCandidate);

                PointF offset = new PointF(bestCandidate.X - lastPoint.X, bestCandidate.Y - lastPoint.Y);
                log.DebugFormat("Tracking: Offset: {0}, Score:{1:.0.000}", offset, max);

                currentPoint = CreateTrackPoint(false, bestCandidate, max, time, currentImage, previousPoints);
                ((TrackPointBlock)currentPoint).Similarity = max;
            }

            cvImageROI.Dispose();
            cvTemplate.Dispose();
            cvSimiMap.Dispose();

            return true;
        }
        /// <summary>
        /// Computes the center of mass of the similarity scores in the vicinity of the best candidate.
        /// Finds a floating point location for the best match.
        /// </summary>
        private PointF RefineLocation(Mat map, OpenCvSharp.Point loc, float threshold)
        {
            // At this point we should never be on the edge of the image so we can take ±1 locations safely.

            // Center of gravity of the 3x3 neighborhood.
            float x = 0.0f;
            float y = 0.0f;
            float sum = 0.0f;
            int count = 0;
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    float value = map.Get<float>(loc.Y + j, loc.X + i);
                    
                    //log.DebugFormat("value at i:{0}, j:{1} = {2} {3}", i, j, value, value >= threshold ? "<-" : "");

                    // Bail out if the map contains garbage.
                    if (float.IsInfinity(value) || float.IsNaN(value))
                        return new PointF(loc.X, loc.Y);

                    // Only consider candidates that would pass the threshold on their own, 
                    // otherwise we are just adding noise.
                    if (value < threshold)
                        continue;


                    x += (i * value);
                    y += (j * value);
                    sum += value;
                    count++;
                }
            }

            return new PointF(loc.X + x / sum, loc.Y + y / sum);
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

                int startY = (int)(p.Y - ((int)(parameters.BlockWindow.Height / 2.0f)));
                int startX = (int)(p.X - ((int)(parameters.BlockWindow.Width / 2.0f)));

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

        /// <summary>
        /// Draw the tracker gizmo for the main viewport during open tracking or for the configuration mini viewport.
        /// </summary>
        public override void Draw(Graphics canvas, AbstractTrackPoint point, IImageToViewportTransformer transformer, Color color, double opacityFactor, bool isConfiguring)
        {
            // The template matching algorithm works aligned to the pixel grid so we do all the math
            // in the original image space and convert at the end.
            // Otherwise we are showing misleading sub-pixel-aligned boxes which makes it harder to position them on top of the video.
            PointF locationAligned = point.Point.ToPoint();
            System.Drawing.Size srchSize = parameters.SearchWindow;
            System.Drawing.Size tmplSize = parameters.BlockWindow;
            PointF srchTopLeft = new PointF(locationAligned.X - (int)(srchSize.Width / 2.0f), locationAligned.Y - (int)(srchSize.Height / 2.0f));
            PointF tmplTopLeft = new PointF(locationAligned.X - (int)(tmplSize.Width / 2.0f), locationAligned.Y - (int)(tmplSize.Height / 2.0f));
            Rectangle srchRect = new Rectangle((int)srchTopLeft.X, (int)srchTopLeft.Y, srchSize.Width, srchSize.Height);
            Rectangle tmplRect = new Rectangle((int)tmplTopLeft.X, (int)tmplTopLeft.Y, tmplSize.Width, tmplSize.Height);
            srchRect = transformer.Transform(srchRect);
            tmplRect = transformer.Transform(tmplRect);


            if (isConfiguring)
            {
                // Dim the whole background.
                GraphicsPath backgroundPath = new GraphicsPath();
                backgroundPath.AddRectangle(canvas.ClipBounds);
                GraphicsPath srchBoxPath = new GraphicsPath();
                srchBoxPath.AddRectangle(srchRect);
                backgroundPath.AddPath(srchBoxPath, false);
                using (SolidBrush brushBackground = new SolidBrush(Color.FromArgb(160, Color.Black)))
                {
                    canvas.FillPath(brushBackground, backgroundPath);
                }
            }

            int alpha = isConfiguring ? 255 : (int)(opacityFactor * 192);

            using (Pen pen = new Pen(Color.FromArgb(alpha, color)))
            using (SolidBrush brush = new SolidBrush(pen.Color))
            {
                // Draw the search and template boxes around the point.
                canvas.DrawRectangle(pen, srchRect);
                canvas.DrawRectangle(pen, tmplRect);

                // Draw the handles for configuration mode.
                if (isConfiguring)
                {
                    // Search box.
                    int widen = 4;
                    int size = widen * 2;
                    canvas.FillEllipse(brush, srchRect.Left - widen, srchRect.Top - widen, size, size);
                    canvas.FillEllipse(brush, srchRect.Left - widen, srchRect.Bottom - widen, size, size);
                    canvas.FillEllipse(brush, srchRect.Right - widen, srchRect.Top - widen, size, size);
                    canvas.FillEllipse(brush, srchRect.Right - widen, srchRect.Bottom - widen, size, size);

                    // Template box.
                    widen = 3;
                    size = widen * 2;
                    canvas.FillEllipse(brush, tmplRect.Left - widen, tmplRect.Top - widen, size, size);
                    canvas.FillEllipse(brush, tmplRect.Left - widen, tmplRect.Bottom - widen, size, size);
                    canvas.FillEllipse(brush, tmplRect.Right - widen, tmplRect.Top - widen, size, size);
                    canvas.FillEllipse(brush, tmplRect.Right - widen, tmplRect.Bottom - widen, size, size);
                }

                // Draw the ellipse representing the mask.
                canvas.DrawEllipse(pen, tmplRect);

                // Extra info
                if (!isConfiguring)
                {
                    DrawDebugInfo(canvas, point, srchRect, color);
                }
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

       
    }
}
