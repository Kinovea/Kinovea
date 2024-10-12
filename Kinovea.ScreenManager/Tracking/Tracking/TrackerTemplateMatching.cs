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
using MathNet.Numerics;
using System.Linq;


namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Tracker using template matching.
    /// Each track has its own tracker instance.
    /// </summary>
    public class TrackerTemplateMatching : AbstractTracker, IDisposable
    {
        #region Properties
        public override TrackingParameters Parameters
        {
            get { return parameters; }
        }
        #endregion

        #region Members
        private TrackingParameters parameters = new TrackingParameters();
        
        private List<TrackingTemplate> trackingTemplates = new List<TrackingTemplate>();

        // Mask used during template matching. Only changes when the template size changes.
        private Bitmap mask;
        private OpenCvSharp.Mat cvMaskGray = new OpenCvSharp.Mat();

        // Debugging.
        private static readonly bool debugging = false;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Construction/Destruction
        public TrackerTemplateMatching(TrackingParameters parameters)
        {
            this.parameters = parameters;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~TrackerTemplateMatching()
        {
            Dispose(false);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                for (int i = trackingTemplates.Count - 1; i >= 0; i--)
                {
                    trackingTemplates[i].Dispose();
                }
             
                trackingTemplates.Clear();
            }
        }

        #endregion

        #region AbstractTracker Implementation

        /// <summary>
        /// Perform a tracking step.
        /// </summary>
        public override bool TrackStep(List<AbstractTrackPoint> timeline, long time, Bitmap currentImage, Mat cvImage, out AbstractTrackPoint currentPoint)
        {
            //---------------------------------------------------------------------
            // The input informations we have at hand are:
            // - The current bitmap we have to find the template into.
            // - The coordinates of all the previous points tracked.
            // - Previous tracking scores, stored in the TrackPoints tracked so far.
            //---------------------------------------------------------------------
            TrackPointBlock lastTrackPoint = timeline.Last() as TrackPointBlock;

            if (lastTrackPoint.Template == null ||
                currentImage == null ||
                lastTrackPoint.Template.Width != parameters.BlockWindow.Width ||
                lastTrackPoint.Template.Height != parameters.BlockWindow.Height)
            {
                // No image or wrong image size.
                // Create the point at the last point location.
                currentPoint = CreateTrackPoint(lastTrackPoint.Point, time, 0.0f, currentImage, timeline);
                log.Debug("Track failed. No input image, or last point doesn't have any cached block image.");

                return false;
            }

            // Perform the template matching.
            TrackResult result = MatchTemplate(cvImage, lastTrackPoint.Template, lastTrackPoint.Point);

            currentPoint = null;
            if (result.Similarity == 0)
            {
                // Program failure.
                // Keep the point at the previous location.
                currentPoint = CreateTrackPoint(lastTrackPoint.Point, time, 0.0f, currentImage, timeline);
                log.DebugFormat("Tracking failed.");
            }
            else if (result.Similarity < parameters.SimilarityThreshold)
            {
                // Tracking failure.
                // Keep the point at the previous location.
                currentPoint = CreateTrackPoint(lastTrackPoint.Point, time, 0.0f, currentImage, timeline);
                log.DebugFormat("Tracking failed. Best candidate: {0} < {1}.", result.Similarity, parameters.SimilarityThreshold);
            }
            else
            {
                // Tracking success, apply template update algorithm.
                currentPoint = CreateTrackPoint(result.Location, time, result.Similarity, currentImage, timeline);
                ((TrackPointBlock)currentPoint).Similarity = result.Similarity;
            }

            return true;
        }


        /// <summary>
        /// Creates a track point from auto-tracking.
        /// Performs the template update algorithm.
        /// </summary>
        public override AbstractTrackPoint CreateTrackPoint(PointF point, long time, float similarity, Bitmap currentImage, List<AbstractTrackPoint> previousPoints)
        {
            // WIP Refactoring in progress.
            // Currently we store the template inside each point in the timeline.
            // It should be possible to store just the time and location and keep the current template
            // as state inside the tracker.
            // The template is not stored to KVA anyway.
            // This is necessary to make the tracking timeline more agnostic of the tracking algorithm.

            log.DebugFormat("Create track point.");

            // Creates a TrackPoint from the input image at the given coordinates.
            // The template is captured at the nearest whole pixel location (rounding).
            // It is important that the matching also uses the same rounding.
            TrackingTemplate trackingTemplate = null;
            Bitmap bmpTemplate = new Bitmap(parameters.BlockWindow.Width, parameters.BlockWindow.Height, PixelFormat.Format32bppPArgb);
            int age = 0;

            // Template update algorithm:
            // - If the match is "poor" we don't update.
            // - If the match is "good", we also don't update, maintain the reference. This avoids drift.
            // - If the match is only "fair" but still considered a match, we update the template.
            // After lots of experiment this approach seems to give good results.
            // Another approach used in for example the software "Tracker" is to constantly evolve the template by blending with 
            // the reference. This still causes drift, just more slowly. Then they reblend the reference
            // on top of the evolved template to combat drift.
            // The approach used here is more similar to what we do manually, the reference is good enough until it isn't.
            // The drawback is that instead of a slow drift we may hook onto a false positive or a shift.
            bool captureTemplate = true;
            if(previousPoints.Count > 0 && (similarity > parameters.TemplateUpdateThreshold || similarity < parameters.SimilarityThreshold))
            {
                // The match is either very good or very bad: do not update.
                // FIXME: we don't need to save the template inside each point, just keep a global one.
                TrackPointBlock prevBlock = previousPoints[previousPoints.Count - 1] as TrackPointBlock;
                if(prevBlock != null && prevBlock.Template != null)
                {
                    bmpTemplate = BitmapHelper.Copy(prevBlock.Template);
                    captureTemplate = false;
                    age = prevBlock.TemplateAge + 1;

                    PositionningSource source = PositionningSource.TemplateMatching;
                    trackingTemplate = new TrackingTemplate(time, point, similarity, bmpTemplate, source);
                }
            }

            if(captureTemplate)
            {
                // We are establishing a new reference template.    
                // The way we round or truncate the point coordinates is important and sould be coherent between capture and matching.
                PointF pointAligned = new PointF((int)Math.Round(point.X), (int)Math.Round(point.Y));

                int startX = (int)(pointAligned.X - ((int)(bmpTemplate.Width / 2.0f)));
                int startY = (int)(pointAligned.Y - ((int)(bmpTemplate.Height / 2.0f)));

                // This might happen if the user place the point so that the template is partially outside the image boundaries.
                startX = Math.Max(startX, 0);
                startY = Math.Max(startY, 0);

                BitmapHelper.CopyROI(currentImage, bmpTemplate, new System.Drawing.Point(startX, startY));

                PositionningSource source = PositionningSource.TemplateMatching;
                trackingTemplate = new TrackingTemplate(time, point, (float)similarity, bmpTemplate, source);
            }

            // Store the full precision value in the point.
            TrackPointBlock tpb = new TrackPointBlock(point.X, point.Y, time, bmpTemplate);
            tpb.TemplateAge = age;
            tpb.IsReferenceBlock = false;
            tpb.Similarity = similarity;

            // Refactoring: store the templates in the tracker, not in the timeline of points.
            trackingTemplates.Add(trackingTemplate);

            return tpb;
        }

        /// <summary>
        /// Creates a Track point from a user-provided location.
        /// Always update the template.
        /// </summary>
        public override AbstractTrackPoint CreateTrackPointReference(PointF point, long time, Bitmap currentImage)
        {
            log.DebugFormat("Create track point reference.");

            TrackingTemplate trackingTemplate = null;
            Bitmap bmpTemplate = new Bitmap(parameters.BlockWindow.Width, parameters.BlockWindow.Height, PixelFormat.Format32bppPArgb);
            int age = 0;

            // We are establishing a new reference template.    
            // The way we round or truncate the point coordinates is important and sould be coherent between capture and matching.
            PointF pointAligned = new PointF((int)Math.Round(point.X), (int)Math.Round(point.Y));
            int startX = (int)(pointAligned.X - ((int)(bmpTemplate.Width / 2.0f)));
            int startY = (int)(pointAligned.Y - ((int)(bmpTemplate.Height / 2.0f)));

            // This might happen if the user place the point so that the template is partially outside the image boundaries.
            startX = Math.Max(startX, 0);
            startY = Math.Max(startY, 0);

            BitmapHelper.CopyROI(currentImage, bmpTemplate, new System.Drawing.Point(startX, startY));

            // Store to tracker state.
            PositionningSource source = PositionningSource.Manual;
            trackingTemplate = new TrackingTemplate(time, point, 1.0f, bmpTemplate, source);
            trackingTemplates.Add(trackingTemplate);
        
            // Return a new track point.
            // TODO: this should return a TimedPoint or some other algorithm agnostic structure.
            TrackPointBlock tpb = new TrackPointBlock(point.X, point.Y, time, bmpTemplate);
            tpb.TemplateAge = age;
            tpb.IsReferenceBlock = true;
            tpb.Similarity = 1.0f;
            return tpb;
        }

        /// <summary>
        /// Create a track point at the specified location without any template update logic.
        /// This is used only in the case of importing existing track data from xml.
        /// </summary>
        public override AbstractTrackPoint CreateOrphanTrackPoint(PointF point, long time)
        {
            // The TrackPoint can't be used as-is to track the next one because it's missing the algo internal data (block).
            // We'll need to reconstruct it when we have the corresponding image.
            return new TrackPointBlock(point.X, point.Y, time);
        }
        #endregion

        #region Drawing gizmo
        /// <summary>
        /// Draw the tracker gizmo for the main viewport during open tracking or for the configuration mini viewport.
        /// </summary>
        public override void Draw(Graphics canvas, AbstractTrackPoint point, IImageToViewportTransformer transformer, Color color, double opacityFactor, bool isConfiguring)
        {
            // The template matching algorithm works aligned to the pixel grid so we do all the math
            // in the original image space and convert at the end.
            // Otherwise we are showing misleading sub-pixel-aligned boxes which makes it harder to position them on top of the video.
            //PointF locationAligned = point.Point.ToPoint();
            PointF locationAligned = new PointF((int)Math.Round(point.X), (int)Math.Round(point.Y));

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

        #region Private

        /// <summary>
        /// Performs the template matching.
        /// This function returns a TrackResult which is just the location and score.
        /// It is the responsibility of the caller to update the template or not.
        /// </summary>
        private TrackResult MatchTemplate(Mat cvImage, Bitmap template, PointF lastPoint)
        {
            TrackResult result;

            // The template matching itself is aligned with the pixel grid. 
            // The output, user-placed or tracked, has sub-pixel coordinates.
            // We match against the closest alignment.
            // Do not re-inject this sub-pixel offset at the end, the result is the location from the refinement.
            PointF lastPointAligned = new PointF((int)Math.Round(lastPoint.X), (int)Math.Round(lastPoint.Y));

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
            var cvTemplate = BitmapConverter.ToMat(template);
            var cvImageROI = cvImage[srchRect.Y, srchRect.Y + srchRect.Height, srchRect.X, srchRect.X + srchRect.Width];

            // Make an ellipse mask to avoid matching on the background.
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
            int smapWidth = srchRect.Width - tmplSize.Width + 1;
            int smapHeight = srchRect.Height - tmplSize.Height + 1;
            Mat cvSimiMap = new Mat(new OpenCvSharp.Size(smapWidth, smapHeight), MatType.CV_32FC1);

            // Perform the actual template matching.
            // This fills the map with the score at each candidate location.
            Cv2.MatchTemplate(cvImageROI, cvTemplate, cvSimiMap, TemplateMatchModes.CCoeffNormed, cvMaskGray);
            //Cv2.MatchTemplate(cvImageROI, cvTemplate, cvSimiMap, TemplateMatchModes.SqDiffNormed, cvMaskGray);
            //Cv2.MatchTemplate(cvImageROI, cvTemplate, cvSimiMap, TemplateMatchModes.SqDiffNormed);

            // Find the best value.
            double min = 0;
            double max = 0;
            OpenCvSharp.Point minLoc;
            OpenCvSharp.Point maxLoc;
            Cv2.MinMaxLoc(cvSimiMap, out min, out max, out minLoc, out maxLoc);
            //log.DebugFormat("With mask: maxloc:{0}, max:{1}", maxLoc, max);

            //Scalar avgDiffSq = Cv2.Mean(cvSimiMap);
            //double avg = avgDiffSq.Val0 + avgDiffSq.Val1 + avgDiffSq.Val2 / 3;
            //double peakHeight = avg / min - 1;
            //log.DebugFormat("avg:{0}, min:{1}, peakHeight:{2}, minLoc:{3}", avg, min, peakHeight, minLoc);

            // For some reason the map may contain Infinity values near the edges (only with masking?).
            if (double.IsInfinity(max) || max > 1.0)
            {
                // Retry without the mask.
                Cv2.MatchTemplate(cvImageROI, cvTemplate, cvSimiMap, TemplateMatchModes.CCoeffNormed);
                Cv2.MinMaxLoc(cvSimiMap, out min, out max, out minLoc, out maxLoc);
                //log.DebugFormat("No mask: maxloc:{0}, max:{1}", maxLoc, max);
            }

            #region Debugging
            // Save the similarity map to file.
            //Mat cvSimiMap8u = new Mat(cvSimiMap.Size(), MatType.CV_8U);
            //Cv2.Normalize(cvSimiMap, cvSimiMap8u, 0, 255, NormTypes.MinMax, MatType.CV_8U);
            //Bitmap bmpSimiMap = cvSimiMap8u.ToBitmap();
            //string tplDirectory = @"G:\temp\simimap";
            //bmpSimiMap.Save(tplDirectory + string.Format(@"\simiMap-{0:000}-{1:0.00}.png", previousPoints.Count, max));
            #endregion

            if (double.IsInfinity(max))
            {
                // Tracking failure.
                result = new TrackResult(0, lastPoint);
            }
            else if (max < parameters.SimilarityThreshold)
            {
                // Tracking failure.
                result = new TrackResult((float)max, lastPoint);
            }
            else
            {
                // Tracking success. Refine the best location based on neighbors scores.
                bool doRefine = true;
                PointF loc = PointF.Empty;

                // If the best candidate is on the edge, there is no point trying to refine it.
                // This is very suspicious and likely a failure case.
                // If the best score is 1.0 (pixel-perfect static image?) we also don't refine.
                //if (!doRefine || double.IsInfinity(peakHeight) || minLoc.X == 0 || minLoc.X == cvSimiMap.Width - 1 || minLoc.Y == 0 || minLoc.Y == cvSimiMap.Height - 1)
                if (!doRefine || max == 1.0 || maxLoc.X == 0 || maxLoc.X == cvSimiMap.Width - 1 || maxLoc.Y == 0 || maxLoc.Y == cvSimiMap.Height - 1)
                {
                    loc = new PointF(maxLoc.X, maxLoc.Y);
                    //loc = new PointF(minLoc.X, minLoc.Y);
                    //log.DebugFormat("Non refined location: {0}", loc);
                }
                else
                {
                    // Approach 0: no refinement.
                    //loc = new PointF(maxLoc.X, maxLoc.Y);

                    // Approach 1: center of gravity of values in the 3x3 neighborhood of the best candidate.
                    // If the top result is over the "good" threshold, only consider other good matches in the neighborhood.
                    // Otherwise consider all "fair" matches.
                    // Don't consider matches that wouldn't have passed the threshold, they are just adding noise.
                    //float threshold = max >= parameters.TemplateUpdateThreshold ? (float)parameters.TemplateUpdateThreshold : (float)parameters.SimilarityThreshold;
                    //loc = RefineLocationCOG(cvSimiMap, maxLoc, threshold);

                    // Approach 2: Fit parabolas along the x and y axes.
                    loc = RefineLocationParabola(cvSimiMap, maxLoc, (float)max);
                    //loc = RefineLocationParabola(cvSimiMap, minLoc, (float)min);

                    //log.DebugFormat("Refined location: {0}", loc);
                }

                // What we have at this point is the location of the top left of the template within the search window.
                // Find back the actual center point based on truncated half size.
                // DO NOT re-inject the sub-pixel offset of the original position into the result. 
                // The goal is to match the reference template in this frame, sub-pixel results of previous frames are irrelevant.
                PointF bestCandidate = new PointF(
                    srchRect.X + loc.X + (int)(cvTemplate.Width / 2.0f),
                    srchRect.Y + loc.Y + (int)(cvTemplate.Height / 2.0f));
                //log.DebugFormat("Tracking, Final point: {0}", bestCandidate);

                //PointF offset = new PointF(bestCandidate.X - lastPoint.X, bestCandidate.Y - lastPoint.Y);
                //log.DebugFormat("Tracking, Point: {0}, Offset: {1}, Score:{2:.0.000}{3}", bestCandidate, offset, max, max >= parameters.TemplateUpdateThreshold ? "" : " <<<< Template update");
                //log.DebugFormat("Tracking, Point: {0}, Offset: {1}, Peak height:{2:.0.000}", bestCandidate, offset, peakHeight);

                // At this point we have the match location.
                result = new TrackResult((float)max, bestCandidate);
            }

            cvImageROI.Dispose();
            cvTemplate.Dispose();
            cvSimiMap.Dispose();

            return result;
        }

        /// <summary>
        /// Computes the center of mass of the similarity scores in the 3x3 neighborhood of the best candidate.
        /// Returns the center of mass as the refined point.
        /// </summary>
        private PointF RefineLocationCOG(Mat map, OpenCvSharp.Point loc, float threshold)
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

        /// <summary>
        /// Fit parabolas along the x and y axes using the immediate neighbors on each side.
        /// Returns the peak of the parabola as the refined point.
        /// </summary>
        private PointF RefineLocationParabola(Mat map, OpenCvSharp.Point loc, float centralValue)
        {
            double[] xValues = new double[3];
            double[] yValues = new double[3];
            xValues[1] = yValues[1] = centralValue;
            for (int i = -1; i < 2; i+=2)
            {
                xValues[i+1] = map.Get<float>(loc.Y, loc.X + i);
                yValues[i+1] = map.Get<float>(loc.Y + i, loc.X);
            }

            // Fit parabola.
            double[] xs = new double[3] { -1, 0, +1};
            double[] cx = Fit.Polynomial(xs, xValues, 2);
            double[] cy = Fit.Polynomial(xs, yValues, 2);
            
            // Find peak (-b/2a).
            // Note that the internal representation is y=a+bx+cx² rather than y=ax²+bx+c so the indices are swapped.
            // A further possibility would be to take the width of the parabola to alter the score.
            double dx = -cx[1] / (2 * cx[2]); 
            double dy = -cy[1] / (2 * cy[2]);

            //log.DebugFormat("xs: {0:0.000}, {1:0.000}, {2:0.000}, dx: {3:0.000}", xValues[0], xValues[1], xValues[2], dx);
            //log.DebugFormat("ys: {0:0.000}, {1:0.000}, {2:0.000}, dy: {3:0.000}", yValues[0], yValues[1], yValues[2], dy);
            //log.DebugFormat("Parabola fit X: a:{0}, b:{1}, c:{2}", cx[0], cx[1], cx[2]);
            //log.DebugFormat("Parabola fit: {0}, {1}", dx, dy);

            return new PointF((float)(loc.X + dx), (float)(loc.Y + dy));
        }
        #endregion
    }
}
