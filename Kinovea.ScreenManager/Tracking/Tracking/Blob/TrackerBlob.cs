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
using Kinovea.Video;


namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Tracker using blob detection.
    /// </summary>
    public class TrackerBlob : AbstractTracker, IDisposable
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

        // Copy of the ROI at the current time.
        // Used to paint the HSV filtered image for configuration feedback.
        private Mat cvROI;

        private PointF srchTopLeft;

        // Extra parameters.
        private bool blurROI = false;
        private int blurKernelSize = 5;

        // Debugging.
        private static readonly bool debugging = false;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Construction/Destruction
        public TrackerBlob(TrackingParameters parameters)
        {
            this.parameters = parameters;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~TrackerBlob()
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

            Circle circle = MatchBlob(cvImage, lastTrackPoint);
            
            bool matched = false;
            if (circle.Radius == 0)
            {
                // Tracking failure.
                // Keep the point at the previous location.
                Circle dummy = new Circle(lastTrackPoint.Point, lastTrackPoint.R);
                currentPoint = CreateTrackPoint(dummy, time, currentImage, timeline);
                log.DebugFormat("Blob tracking failed.");
                return false;
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

            // Backup the ROI for configuration feedback.
            if (cvROI != null)
                cvROI.Dispose();

            //log.DebugFormat("GetROI for create reference track point at {0}", point.T);
            var cvImage = OpenCvSharp.Extensions.BitmapConverter.ToMat(currentImage);
            cvROI = GetROI(cvImage, point.Point);
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

        /// <summary>
        /// Update the image used for HSV filtering feedback.
        /// </summary>
        public override void UpdateImage(long time, Mat cvImage, List<TimedPoint> previousPoints)
        {
            // Keep a copy of the search window.
            // We will then apply the HSV filtering on this copy and paint it as configuration feedback.
            
            // Location of the point at the passed time.
            TimedPoint p = previousPoints.Find(tp => tp.T == time);
            if (p == null)
            {
                p = previousPoints.Last();
            }

            if (cvROI != null)
            {
                cvROI.Dispose();
            }

            //log.DebugFormat("GetROI for UpdateImage at {0}", time);
            cvROI = GetROI(cvImage, p.Point);
        }

        /// <summary>
        /// Clear all internal data.
        /// </summary>
        public override void Clear()
        {
            referenceTimes.Clear();

            if (cvROI != null)
                cvROI.Dispose();
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

                // Draw the HSV filtered ROI.
                if (cvROI != null)
                {
                    Mat cvWorking = new Mat();

                    //Cv2.CvtColor(cvROI, cvWorking, ColorConversionCodes.BGR2GRAY);
                    //Cv2.MedianBlur(cvWorking, cvWorking, 5);
                    Cv2.CvtColor(cvROI, cvWorking, ColorConversionCodes.BGR2HSV);
                    Scalar hsvMin = new Scalar(parameters.HSVRange.HueMin, parameters.HSVRange.SaturationMin, parameters.HSVRange.ValueMin);
                    Scalar hsvMax = new Scalar(parameters.HSVRange.HueMax, parameters.HSVRange.SaturationMax, parameters.HSVRange.ValueMax);
                    Cv2.InRange(cvWorking, hsvMin, hsvMax, cvWorking);

                    // Dilate/Erode.
                    if (parameters.Dilate > 0)
                        Cv2.Dilate(cvWorking, cvWorking, null, null, parameters.Dilate);
                    
                    if (parameters.Erode > 0)
                        Cv2.Erode(cvWorking, cvWorking, null, null, parameters.Erode);

                    //Mat cvWorking2 = new Mat();
                    //Cv2.CopyTo(cvROI, cvWorking2, cvWorking);
                    //cvWorking.CvtColor(cvWorking, cvWorking, ColorConversionCodes.2bgr)
                    //Cv2.BitwiseAnd(cvROI, cvWorking, cvWorking);
                    Cv2.Split(cvROI, out Mat[] planes);
                    Cv2.BitwiseAnd(planes[0], cvWorking, planes[0]);
                    Cv2.BitwiseAnd(planes[1], cvWorking, planes[1]);
                    Cv2.BitwiseAnd(planes[2], cvWorking, planes[2]);
                    Cv2.Merge(planes, cvWorking);

                    cvWorking.SaveImage(@"G:\temp\blob\roi_filtered.png");
                    
                    Bitmap bmp = cvWorking.ToBitmap();
                    canvas.DrawImage(bmp, srchRect);
                    bmp.Dispose();
                    cvWorking.Dispose();
                    //cvWorking2.Dispose();
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
            string text = string.Format("BLOB - {0:0.000} ({1})", point.R, manual ? "M" : "A");
            using (Font f = new Font("Consolas", 10, FontStyle.Bold))
            using (Brush b = new SolidBrush(color))
            {
                canvas.DrawString(text, f, b, search.Location.Translate(0, -25));
            }
        }
        #endregion

        #region Private

        /// <summary>
        /// Extract the region of interest around the point.
        /// </summary>
        private Mat GetROI(Mat cvImage, PointF point)
        {
            PointF pointAligned = new PointF((int)Math.Round(point.X), (int)Math.Round(point.Y));

            // The boxes themselves may have odd or even sizes.
            System.Drawing.Size srchSize = parameters.SearchWindow;
            System.Drawing.Size tmplSize = parameters.BlockWindow;
            PointF srchTopLeft = new PointF(pointAligned.X - (int)(srchSize.Width / 2.0f), pointAligned.Y - (int)(srchSize.Height / 2.0f));
            Rectangle srchRect = new Rectangle((int)srchTopLeft.X, (int)srchTopLeft.Y, srchSize.Width, srchSize.Height);
            srchRect.Intersect(new Rectangle(0, 0, cvImage.Width, cvImage.Height));
            srchTopLeft = srchRect.Location;

            this.srchTopLeft = srchTopLeft;

            // Extract the ROI
            Mat cvImageROI = cvImage[srchRect.Y, srchRect.Y + srchRect.Height, srchRect.X, srchRect.X + srchRect.Width];
            return cvImageROI;
        }

        /// <summary>
        /// Performs the circle matching.
        /// This function returns a pair containing the best guess circle
        /// and the amount of supporting votes it got. Zero votes indicates tracking failure.
        /// </summary>
        private Circle MatchBlob(Mat cvImage, TimedPoint point)
        {
            //KeyPoint[] result = new KeyPoint();

            // This will also update the global srchTopLeft.
            Mat cvImageROI = GetROI(cvImage, point.Point);

            //----------------------------------
            // Blob detection
            //----------------------------------
            Mat cvWorking = new Mat();
            Cv2.CopyTo(cvImageROI, cvWorking);

            // Optional blurring.
            if (blurROI)
            {
                Cv2.MedianBlur(cvWorking, cvWorking, blurKernelSize);
            }

            // Convert to HSV.
            Cv2.CvtColor(cvImageROI, cvWorking, ColorConversionCodes.BGR2HSV);

            // Apply HSV thresholding.
            Scalar hsvMin = new Scalar(parameters.HSVRange.HueMin, parameters.HSVRange.SaturationMin, parameters.HSVRange.ValueMin);
            Scalar hsvMax = new Scalar(parameters.HSVRange.HueMax, parameters.HSVRange.SaturationMax, parameters.HSVRange.ValueMax);
            Cv2.InRange(cvWorking, hsvMin, hsvMax, cvWorking);

            // Dilate/Erode.
            if (parameters.Dilate > 0)
                Cv2.Dilate(cvWorking, cvWorking, null, null, parameters.Dilate);

            if (parameters.Erode > 0)
                Cv2.Erode(cvWorking, cvWorking, null, null, parameters.Erode);

            // Invert the image to detect dark blobs on a light background.
            Cv2.BitwiseNot(cvWorking, cvWorking);

            //cvWorking.SaveImage(@"G:\temp\blob\roi.png");


            float minRadius = point.R * 0.85f;
            float maxRadius = point.R * 1.15f;


            // Blob filtering.
            SimpleBlobDetector.Params detectorParams = new SimpleBlobDetector.Params();
            // Defaults:
            //thresholdStep = 10,
            //minThreshold = 50,
            //maxThreshold = 220,
            //minRepeatability = 2,
            //minDistBetweenBlobs = 10,
            //filterByColor = 1,
            //blobColor = 0,
            //filterByArea = 1,
            //minArea = 25,
            //maxArea = 5000,
            //filterByCircularity = 0,
            //minCircularity = 0.8f,
            //maxCircularity = float.MaxValue,
            //filterByInertia = 1,
            //minInertiaRatio = 0.1f,
            //maxInertiaRatio = float.MaxValue,
            //filterByConvexity = 1,
            //minConvexity = 0.95f,
            //maxConvexity = float.MaxValue


            detectorParams.MinThreshold = 1;
            detectorParams.MaxThreshold = 200;
            detectorParams.MinDistBetweenBlobs = 1;
            detectorParams.MinRepeatability = 2;

            detectorParams.FilterByColor = true;
            detectorParams.BlobColor = 0;

            // Circularity
            // (4 * π * Area) / (perimeter * perimeter)
            detectorParams.FilterByCircularity = false;
            detectorParams.MinCircularity = 0.1f;
            detectorParams.MaxCircularity = 1.0f;

            // Convexity
            // (Area of the Blob) / (Area of the Convex Hull)
            detectorParams.FilterByConvexity = false;
            detectorParams.MinConvexity = 0.1f;
            detectorParams.MaxConvexity = 1.0f;

            detectorParams.FilterByArea = false;
            detectorParams.MinArea = 100;
            detectorParams.MaxArea = 10000;
            
            detectorParams.FilterByInertia = false;
            detectorParams.MinInertiaRatio = 0.1f;

            // Detect blobs.
            var detector = SimpleBlobDetector.Create(detectorParams);
            KeyPoint[] kp = detector.Detect(cvWorking);
            detector.Dispose();

            
            
            // Blob matching. Return the largest blob.
            Circle result = new Circle();
            foreach (var k in kp)
            {
                if (k.Size / 2.0f > result.Radius)
                {
                    result.Center = new PointF(srchTopLeft.X + k.Pt.X, srchTopLeft.Y + k.Pt.Y);
                    result.Radius = k.Size / 2.0f;
                }
            }

            log.DebugFormat("Blob tracking at {0}, blobs:{1}, largest:{2}", point.T, kp.Length, result.Radius);

            cvWorking.Dispose();
            cvImageROI.Dispose();

            return result;
        }

       #endregion
    }
}
