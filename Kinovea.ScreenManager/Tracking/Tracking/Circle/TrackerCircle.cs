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
    /// Tracker using Circle Hough Transform.
    /// </summary>
    public class TrackerCircle : AbstractTracker, IDisposable
    {
        #region Properties
        public override TrackingParameters Parameters
        {
            get { return parameters; }
        }
        #endregion

        #region Members
        private TrackingParameters parameters = new TrackingParameters();

        // Set of reference times during the tracking session.
        // This is not stored to KVA.
        // This is used to limit the averaging window to the last reference radius.
        // Apart from that the Circle tracker is mostly stateless, unlike the
        // template matching tracker.
        private SortedSet<long> referenceTimes = new SortedSet<long>();

        // Max number of points taken into account to establish the reference radius.
        // TODO: move this to parameters.
        // Guidelines:
        // - Lower number for objects moving towards or away from the camera.
        // - Higher number for objects moving in the plane perpendicular to the camera axis.
        // We note that the use-case of tracking a ball moving towards can't be used for 
        // measurements so we optimize for the other case.
        private int averagingWindow = 4;

        // Extra parameters.
        // Whether to blur the search window prior to circle detection.
        // Not blurring is found to be more robust to partial occlusions.
        private bool blurROI = false;
        private int blurKernelSize = 5;
        private int maxGuessVoteThreshold = 50;
        private int guessVoteThresholdStep = 2;
        private double houghParam1 = 50; // Used for the canny edge transform.

        // Debugging.
        private static readonly bool debugging = false;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Construction/Destruction
        public TrackerCircle(TrackingParameters parameters)
        {
            this.parameters = parameters;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~TrackerCircle()
        {
            Dispose(false);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Clear();
            }
        }

        #endregion

        #region AbstractTracker Implementation

        /// <summary>
        /// Whether we are ready to track or not.
        /// If not the caller should call some other function to prepare the tracker.
        /// </summary>
        public override bool IsReady(TimedPoint lastTrackedPoint)
        {
            // This can happen when reloading an old trajectory or 
            // switching from a trajectory tracked with a different algorithm.
            // In this case we need to go through CreateReferenceTrackPoint to 
            // set the initial reference radius.
            if (lastTrackedPoint.R == 0 || referenceTimes.Count == 0)
                return false;

            return true;
        }

        /// <summary>
        /// Perform a tracking step.
        /// </summary>
        public override bool TrackStep(List<TimedPoint> timeline, long time, Bitmap currentImage, Mat cvImage, out TimedPoint currentPoint)
        {
            TimedPoint lastTrackPoint = timeline.Last();
            
            //log.DebugFormat("Track step. last track point at {0}, last tracked circle at {1}.", lastTrackPoint.T, lastTrackedCircle.Time);
            if (currentImage == null)
            {
                // Unrecoverable issue.
                Circle dummy = new Circle(lastTrackPoint.Point, lastTrackPoint.R);
                currentPoint = CreateTrackPoint(dummy, time, currentImage, timeline);
                log.Error("Tracking impossible: no input image.");
                return false;
            }

            if (lastTrackPoint.R == 0)
            {
                Circle dummy = new Circle(lastTrackPoint.Point, lastTrackPoint.R);
                currentPoint = CreateTrackPoint(dummy, time, currentImage, timeline);
                log.Error("Tracking impossible: tracker is not ready.");
                return false;
            }

            // Calculate the reference radius.
            // We want to average samples up to the averaging window but not past the last reference
            // otherwise when the user trims and tunes a point manually, the next tracking 
            // step would not use the exact user-tuned radius.
            // Get a list of the tracked circles that are after the last reference time.
            long lastReferenceTime = referenceTimes.Last();
            List<TimedPoint> afterLastRef = timeline.SkipWhile(p => p.T < lastReferenceTime).ToList();
            int samples = Math.Min(averagingWindow, afterLastRef.Count());
            float averageRadius = timeline.Skip(timeline.Count - samples).Average(p => p.R);
            //log.DebugFormat("Reference radius: {0:0.000} ({1} samples)", averageRadius, samples);

            // Perform the circle matching.
            Pair<Circle, int> circleResult = MatchCircle(cvImage, averageRadius, lastTrackPoint.Point);
            var circle = circleResult.First;
            var votes = circleResult.Second;

            bool matched = false;
            currentPoint = null;
            if (votes == 0)
            {
                // Tracking failure.
                // Keep the point at the previous location.
                Circle dummy = new Circle(lastTrackPoint.Point, lastTrackPoint.R);
                currentPoint = CreateTrackPoint(dummy, time, currentImage, timeline);
                log.DebugFormat("Circle tracking failed.");
            }
            else
            {
                // Tracking success.
                currentPoint = CreateTrackPoint(circle, time, currentImage, timeline);
                matched = true;
            }

            return matched;
        }


        /// <summary>
        /// Creates a track point from auto-tracking.
        /// </summary>
        public override TimedPoint CreateTrackPoint(object trackingResult, long time, Bitmap currentImage, List<TimedPoint> previousPoints)
        {
            if (!(trackingResult is Circle))
                throw new InvalidProgramException();

            // Creates a TrackPoint from the tracking result.
            Circle circle = (Circle)trackingResult;
            return new TimedPoint(circle.Center.X, circle.Center.Y, time, circle.Radius);
        }

        /// <summary>
        /// Creates the necessary internal state from a pre-existing input point.
        /// Always updates the template.
        /// This does not return a timed point since it will always be the same as the passed input.
        /// </summary>
        public override void CreateReferenceTrackPoint(TimedPoint point, Bitmap currentImage)
        {
            // Modify the radius of the passed timed point with the current parameters.
            float radius = Math.Max(parameters.BlockWindow.Width / 2.0f, parameters.BlockWindow.Height/ 2.0f);
            point.R = radius;

            // Add the point to the reference timeline.
            referenceTimes.Add(point.T);
        }


        /// <summary>
        /// Trim internal data related to points after the passed time.
        /// </summary>
        public override void Trim(long time)
        {
            if (referenceTimes.Count == 0)
                return;

            // Remove all reference points after the passed time.
            referenceTimes.RemoveWhere(t => t > time);
        }

        public override void UpdateImage(long time, Mat cvImage, List<TimedPoint> previousPoints)
        {
        }

        /// <summary>
        /// Clear all internal data.
        /// </summary>
        public override void Clear()
        {
            referenceTimes.Clear();
        }

        #endregion

        #region Drawing gizmo
        /// <summary>
        /// Draw the tracker gizmo for the main viewport during open tracking or for the configuration mini viewport.
        /// </summary>
        public override void Draw(Graphics canvas, TimedPoint point, IImageToViewportTransformer transformer, Color color, double opacityFactor, bool isConfiguring)
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

                // Extra info
                if (!isConfiguring)
                {
                    DrawDebugInfo(canvas, point, srchRect, color);
                }

                // Circle
                var pCenter = transformer.Transform(point.Point);
                canvas.DrawEllipse(pen, pCenter.Box(transformer.Transform(point.R)));

                // Center
                int radius = 4;
                canvas.DrawLine(pen, pCenter.X, pCenter.Y - radius, pCenter.X, pCenter.Y + radius);
                canvas.DrawLine(pen, pCenter.X - radius, pCenter.Y, pCenter.X + radius, pCenter.Y);
            }
        }

        private void DrawDebugInfo(Graphics canvas, TimedPoint point, RectangleF search, Color color)
        {
            bool manual = referenceTimes.Contains(point.T);
            string text = string.Format("CIRC - {0:0.000} ({1})", point.R, manual ? "M" : "A");
            using (Font f = new Font("Consolas", 10, FontStyle.Bold))
            using (Brush b = new SolidBrush(color))
            {
                canvas.DrawString(text, f, b, search.Location.Translate(0, -25));
            }
        }
        #endregion

        #region Private

        /// <summary>
        /// Performs the circle matching.
        /// This function returns a pair containing the best guess circle
        /// and the amount of supporting votes it got. Zero votes indicates tracking failure.
        /// </summary>
        private Pair<Circle, int> MatchCircle(Mat cvImage, float refRadius, PointF lastPoint)
        {
            Pair<Circle, int> result = new Pair<Circle, int>();

            PointF lastPointAligned = new PointF((int)Math.Round(lastPoint.X), (int)Math.Round(lastPoint.Y));

            // The boxes themselves may have odd or even sizes.
            System.Drawing.Size srchSize = parameters.SearchWindow;
            PointF srchTopLeft = new PointF(lastPointAligned.X - (int)(srchSize.Width / 2.0f), lastPointAligned.Y - (int)(srchSize.Height / 2.0f));
            Rectangle srchRect = new Rectangle((int)srchTopLeft.X, (int)srchTopLeft.Y, srchSize.Width, srchSize.Height);
            srchRect.Intersect(new Rectangle(0, 0, cvImage.Width, cvImage.Height));
            srchTopLeft = srchRect.Location;

            // Extract the ROI
            Mat cvImageROI = cvImage[srchRect.Y, srchRect.Y + srchRect.Height, srchRect.X, srchRect.X + srchRect.Width];

            //----------------------------------
            // Circle tracking.
            //----------------------------------
            Mat cvWorking = new OpenCvSharp.Mat();
            Cv2.CvtColor(cvImageROI, cvWorking, OpenCvSharp.ColorConversionCodes.BGR2GRAY, 0);
            if (blurROI)
            {
                Cv2.MedianBlur(cvWorking, cvWorking, blurKernelSize);
            }

            //------------------------------------------------------------------------------
            // Circle matching algorithm
            // 
            // The Hough Transform function will find all circles in a radius range that pass 
            // a certain vote threshold (vote = supporting evidence for the circle).
            // The vote threshold is highly dependent on the video source.
            // A naive call at a low threshold produces too many false circles to be useful.
            // The general approach is to start at a high threshold and run the call in a loop 
            // lowering the threshold until we find a circle.
            //
            // Algo #1: scan radius space up or down. That is, for each vote threshold,
            // go through all possible radiuses and see if there is any at that vote level.
            // Change radius until we find a circle.
            // This fails with ball that have circular patterns on them for example.
            // This gives priority to small or large circles depending on the direction of the scan.
            // 
            // Algo #2: We already have a good guess for the radius from the previous frames.
            // Give priority to circles with similar radius as the previous few frames.
            // Start by establishing what kind of support we have for the initial guess radius.
            // Then we move up and down in radius space but only as long as the vote count is higher than the baseline.
            // This works quite well when the object doesn't change size too much between two frames.
            // This fails if the object is moving towards or away from the camera at a high rate,
            // it locks into a local maximum of votes. The detectable circles obviously
            // doesn't follow a gaussian distribution, either there is a circle of that radius or not.
            // This approach relies on detecting false circles at radiuses between the previous and the true one.
            //
            // Algo #3: find all circles in a range around a baseline guess radius.
            // Give priority to circles with high vote count. 
            // This can be done by linear or binary search in radius space.
            //------------------------------------------------------------------------------
            int minVotes = 4;

            // Algo #2.
            result = FindBestCircleAtRadius(cvWorking, refRadius, refRadius + 1, srchTopLeft, lastPoint, minVotes);
            log.DebugFormat("Baseline circle around reference radius of {0:0.000}: r:{1:0.000}, votes:{1}.",
                refRadius, result.First.Radius, result.Second);

            // Scan radius space down.
            float minRadius = refRadius / 1.5f;
            float guessRadius = refRadius - 1;
            float bestRadiusDiff = 0;
            minVotes = Math.Max(minVotes, result.Second);
            while (true)
            {
                Pair<Circle, int> candidate = FindBestCircleAtRadius(cvWorking, guessRadius, guessRadius + 1, srchTopLeft, lastPoint, minVotes);

                if (candidate.Second > result.Second)
                {
                    result = candidate;
                    bestRadiusDiff = Math.Abs(guessRadius - refRadius);
                    log.DebugFormat("Found better circle at radius {0}, Votes:{1}", guessRadius, candidate.Second);
                    guessRadius--;
                    if (guessRadius < minRadius)
                        break;
                }
                else
                {
                    break;
                }
            }

            // Scan radius space up.
            // Note that this time we might find something with less votes but closer in radius
            // so we shouldn't update the min vote count, keep it at the baseline.
            float maxRadius = refRadius * 1.5f;
            guessRadius = refRadius + 1;
            while (true)
            {
                Pair<Circle, int> candidate = FindBestCircleAtRadius(cvWorking, guessRadius, guessRadius + 1, srchTopLeft, lastPoint, minVotes);

                // Only consider the candidate if it's closer in radius than the current best.
                // This can happen if we find better than the baseline on both sides.
                float radiusDiff = Math.Abs(guessRadius - refRadius);
                if (candidate.Second > result.Second || (candidate.Second == result.Second && radiusDiff < bestRadiusDiff))
                {
                    result = candidate;
                    log.DebugFormat("Found better circle at radius {0}, Votes:{1}", guessRadius, candidate.Second);
                    guessRadius++;
                    if (guessRadius > maxRadius)
                        break;
                }
                else
                {
                    break;
                }
            }

            // Algo #3.
            //float minRadius = refRadius * 0.85f;
            //float maxRadius = refRadius * 1.15f;
            //result = FindBestCircleAtRadius(cvRoiGray, minRadius, maxRadius, srchTopLeft, lastPoint, minVotes);
            //log.DebugFormat("Baseline circle around reference radius of {0:0.000}: r:{1:0.000}, votes:{2}.",
            //    refRadius, result.First.Radius, result.Second);

            cvWorking.Dispose();
            cvImageROI.Dispose();

            return result;
        }


        /// <summary>
        /// Find the best supported circle at the passed radius.
        /// This detects circles in the passed image that pass a given vote threshold.
        /// To get the best possible circle we start at a high threshold and lower it until we find a circle.
        /// </summary>
        private Pair<Circle, int> FindBestCircleAtRadius(Mat cvRoiGray, float minRadius, float maxRadius, PointF srchTopLeft, PointF lastPoint, int minVotes)
        {
            // Scan the vote space from high to low to make sure we find the best possible circle.
            // Typically this will go from 0 to 1 circle found when the threshold is lowered.
            // Sometimes from 0 to n. 
            Pair<Circle, int> result = new Pair<Circle, int>();

            // Fixed parameters.
            HoughModes mode = HoughModes.Gradient;
            double guessDp = 1.0; // Inverse ratio of resolution (1=full res, 2=half res, etc)
            double minDist = Math.Max(cvRoiGray.Cols / 16, 8);

            // Quantity of votes to qualify for a circle.
            int guessVoteThreshold = maxGuessVoteThreshold;
            CircleSegment[] circles;

            while (guessVoteThreshold > minVotes)
            {
                // Find all circles that pass the threshold, at exactly the current guess radius.
                circles = Cv2.HoughCircles(
                       cvRoiGray,
                       mode,
                       guessDp,
                       minDist,
                       houghParam1,
                       guessVoteThreshold,
                       (int)minRadius,
                       (int)maxRadius);

                if (circles.Length > 0)
                {
                    // Found one or more circles.
                    if (circles.Length == 1)
                    {
                        // Found exactly one circle = good candidate.
                        CircleSegment circle = circles[0];
                        //log.DebugFormat("Found unique circle at radius {0}, Votes:{1}", circle.Radius, guessVoteThreshold);
                        PointF center = new PointF(srchTopLeft.X + circle.Center.X, srchTopLeft.Y + circle.Center.Y);
                        Circle c = new Circle(center, circle.Radius);
                        result.First = c;
                        result.Second = guessVoteThreshold;
                    }
                    else
                    {
                        // Found multiple circles.
                        // We can't discard them all. The original assumption that 
                        // there is only one circle at the right size doesn't hold because 
                        // sometimes we get false positives with the same score from 
                        // shadows or patterns on fabric.
                        // So it's likely that the true circle is there, among the false positives.
                        // Pick the closest to the previous position.
                        float bestDist = float.MaxValue;
                        for (int i = 0; i < circles.Length; i++)
                        {
                            CircleSegment circle = circles[i];
                            PointF center = new PointF(srchTopLeft.X + circle.Center.X, srchTopLeft.Y + circle.Center.Y);
                            float dist = GeometryHelper.GetDistance(center, lastPoint);
                            if (dist < bestDist)
                            {
                                bestDist = dist;
                                result.First = new Circle(center, circle.Radius);
                                result.Second = guessVoteThreshold;
                            }
                        }

                        //log.WarnFormat("Found {0} circles at radius {1}, Votes {2}.", circles.Length, radius, guessVoteThreshold);
                        
                    }

                    // If we lower the vote threshold we'll find even more circles
                    // so it doesn't make sense to continue. 
                    break;
                }
                else
                {
                    // We haven't found any circle at this level of vote threshold.
                    // Lower the threshold and try again.
                    guessVoteThreshold -= guessVoteThresholdStep;
                    continue;
                }
            }

            return result;
        }

       #endregion
    }
}
