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
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Kinovea.Services;

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
        public override TrackerParameters Parameters
        {
            get { return parameters; }
            set 
            { 
                parameters = value;
                SetParameters(parameters);
            }
        }

        #region Members
        // Options - initialize in the constructor.
        private double similarityTreshold = 0.0f;		// Discard candidate block with lower similarity.
        private double templateUpdateThreshold = 1.0f;	// Only update the template if that dissimilar.
        private int refinementNeighborhood = 1;
        private Size blockWindow = new Size(20, 20);
        private Size searchWindow = new Size(100, 100);
        private TrackerParameters parameters;

        // Monitoring, debugging.
        private static readonly bool monitoring = false;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion		
        
        #region Constructor
        public TrackerBlock2(TrackerParameters parameters)
        {
            this.parameters = parameters;
            SetParameters(parameters);
        }
        #endregion
        
        #region AbstractTracker Implementation
        public override bool Track(List<AbstractTrackPoint> previousPoints, Bitmap currentImage, long position, out AbstractTrackPoint currentPoint)
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
                Rectangle searchZone = new Rectangle(	(int)(searchCenter.X - (searchWindow.Width/2)), 
                                                        (int)(searchCenter.Y - (searchWindow.Height/2)), 
                                                        searchWindow.Width, 
                                                        searchWindow.Height);
                
                searchZone.Intersect(new Rectangle(0,0,currentImage.Width, currentImage.Height));
                
                //Image<Bgr, Byte> cvTemplate = new Image<Bgr, Byte>(lastTrackPoint.Template);
                //Image<Bgr, Byte> cvImage = new Image<Bgr, Byte>(_CurrentImage);
                
                Bitmap img = currentImage;
                Bitmap tpl = lastTrackPoint.Template;

                BitmapData imageData = img.LockBits( new Rectangle( 0, 0, img.Width, img.Height ), ImageLockMode.ReadOnly, img.PixelFormat );
                BitmapData templateData = tpl.LockBits(new Rectangle( 0, 0, tpl.Width, tpl.Height ), ImageLockMode.ReadOnly, tpl.PixelFormat );
                
                Image<Bgra, Byte> cvImage = new Image<Bgra, Byte>(imageData.Width, imageData.Height, imageData.Stride, imageData.Scan0);
                Image<Bgra, Byte> cvTemplate = new Image<Bgra, Byte>(templateData.Width, templateData.Height, templateData.Stride, templateData.Scan0);
                
                cvImage.ROI = searchZone;
                
                int resWidth = searchZone.Width - lastTrackPoint.Template.Width + 1;
                int resHeight = searchZone.Height - lastTrackPoint.Template.Height + 1;
                
                Image<Gray, Single> similarityMap = new Image<Gray, Single>(resWidth, resHeight);
                
                //CvInvoke.cvMatchTemplate(cvImage.Ptr, cvTemplate.Ptr, similarityMap.Ptr, TM_TYPE.CV_TM_SQDIFF_NORMED);
                //CvInvoke.cvMatchTemplate(cvImage.Ptr, cvTemplate.Ptr, similarityMap.Ptr, TM_TYPE.CV_TM_CCORR_NORMED);
                CvInvoke.cvMatchTemplate(cvImage.Ptr, cvTemplate.Ptr, similarityMap.Ptr, TM_TYPE.CV_TM_CCOEFF_NORMED);
                
                img.UnlockBits(imageData);
                tpl.UnlockBits(templateData);
                
                // Find max
                double bestScore = 0;
                PointF bestCandidate = new PointF(-1,-1);
                Point minLoc = Point.Empty;
                Point maxLoc = Point.Empty;
                double min = 0;
                double max = 0;
                CvInvoke.cvMinMaxLoc(similarityMap.Ptr, ref min, ref max, ref minLoc, ref maxLoc, IntPtr.Zero);
                
                if(max > similarityTreshold)
                {
                    PointF loc = RefineLocation(similarityMap.Data, maxLoc, parameters.RefinementNeighborhood);
                    
                    // The template matching was done on a template aligned with the integer part of the actual position.
                    // We reinject the floating point part of the orginal positon into the result.
                    loc = loc.Translate(subpixel.X, subpixel.Y);

                    bestCandidate = new PointF(searchZone.Left + loc.X + tpl.Width / 2, searchZone.Top + loc.Y + tpl.Height / 2);
                    bestScore = max;
                }
            
                #region Monitoring
                if(monitoring)
                {
                    // Save the similarity map to file.
                    Image<Gray, Byte> mapNormalized = new Image<Gray, Byte>(similarityMap.Width, similarityMap.Height);
                    CvInvoke.cvNormalize(similarityMap.Ptr, mapNormalized.Ptr, 0, 255, NORM_TYPE.CV_MINMAX, IntPtr.Zero);
            
                    Bitmap bmpMap = mapNormalized.ToBitmap();

                    string tplDirectory = @"C:\Users\Joan\Videos\Kinovea\Video Testing\Tracking\simimap";
                    bmpMap.Save(tplDirectory + String.Format(@"\simiMap-{0:000}-{1:0.00}.bmp", previousPoints.Count, bestScore));
                }
                #endregion
                
                // Result of the matching.
                if(bestCandidate.X != -1 && bestCandidate.Y != -1)
                {
                    currentPoint = CreateTrackPoint(false, bestCandidate, bestScore, position, img, previousPoints);
                    ((TrackPointBlock)currentPoint).Similarity = bestScore;
                }
                else
                {
                    // No match. Create the point at the center of the search window (whatever that might be).
                    currentPoint = CreateTrackPoint(false, lastPoint, 0.0f, position, img, previousPoints);
                    log.Debug("Track failed. No block over the similarity treshold in the search window.");	
                }

                matched = true;
            }
            else
            {
                // No image. (error case ?)
                // Create the point at the last point location.
                currentPoint = CreateTrackPoint(false, lastPoint, 0.0f, position, currentImage, previousPoints);
                log.Debug("Track failed. No input image, or last point doesn't have any cached block image.");
            }
            
            return matched;
        }
        public override AbstractTrackPoint CreateTrackPoint(bool manual, PointF p, double similarity, long t, Bitmap currentImage, List<AbstractTrackPoint> previousPoints)
        {
            // Creates a TrackPoint from the input image at the given coordinates.
            // Stores algorithm internal data in the point, to help next match.
            // _t is in relative timestamps from the first point.
            
            // Copy the template from the image into its own Bitmap.
            
            Bitmap tpl = new Bitmap(blockWindow.Width, blockWindow.Height, PixelFormat.Format32bppPArgb);
            int age = 0;
            
            bool updateWithCurrentImage = true;
            
            if(!manual && previousPoints.Count > 0 && similarity > templateUpdateThreshold || similarity < similarityTreshold)
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
                int templateWidthInBytes = blockWindow.Width * pixelSize;
                int tplOffset = tplStride - templateWidthInBytes;
                
                int imgStride = imageData.Stride;
                int imageWidthInBytes = currentImage.Width * pixelSize;
                int imgOffset = imgStride - (currentImage.Width * pixelSize) + imageWidthInBytes - templateWidthInBytes;
                
                int startY = (int)(p.Y - (blockWindow.Height / 2.0));
                int startX = (int)(p.X - (blockWindow.Width / 2.0));
                
                if(startX < 0) 
                    startX = 0;
                
                if(startY < 0)
                    startY = 0;
                
                unsafe
                {
                    byte* pTpl = (byte*) templateData.Scan0.ToPointer();
                    byte* pImg = (byte*) imageData.Scan0.ToPointer()  + (imgStride * startY) + (pixelSize * startX);
                    
                    for ( int row = 0; row < blockWindow.Height; row++ )
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
                string tplDirectory = @"C:\Users\Joan\Videos\Kinovea\Video Testing\Tracking\simimap";
                if(previousPoints.Count <= 1)
                {
                    // Clean up folder.
                    string[] tplFiles = Directory.GetFiles(tplDirectory, "*.bmp");
                    foreach (string f in tplFiles)
                    {
                        File.Delete(f);
                    }
                }
                String iFileName = String.Format("{0}\\tpl-{1:000}.bmp", tplDirectory, previousPoints.Count);
                tpl.Save(iFileName);
            }
            #endregion
            
            TrackPointBlock tpb = new TrackPointBlock(p.X, p.Y, t, tpl);
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
            Point p = transformer.Transform(point.Point);
            Rectangle search = p.Box(transformer.Transform(searchWindow));

            using(Pen pen = new Pen(Color.FromArgb((int)(opacityFactor * 192), color)))
            {
                canvas.DrawRectangle(pen, search);
                canvas.DrawRectangle(pen, p.Box(transformer.Transform(blockWindow)));

                //DrawDebugInfo(canvas, point, search);
            }
        }

        private void DrawDebugInfo(Graphics canvas, AbstractTrackPoint point, RectangleF search)
        {
            TrackPointBlock tpb = point as TrackPointBlock;
            
            if (tpb == null)
                return;
            
            Font f = new Font("Consolas", 8, FontStyle.Bold);
            string text = string.Format("simi:{0:0.000}, age:{1}, pos:{2:0.000}×{3:0.000}", tpb.Similarity, tpb.TemplateAge, tpb.Point.X, tpb.Point.Y);
            Brush b = tpb.Similarity > parameters.TemplateUpdateThreshold ? Brushes.Green : Brushes.Red;
            canvas.DrawString(text, f, b, search.Location.Translate(0, -25));

            f.Dispose();
        }

        public override RectangleF GetEditRectangle(PointF position)
        {
            return position.Box(searchWindow);
        }
        #endregion
    
        private void SetParameters(TrackerParameters parameters)
        {
            similarityTreshold = parameters.SimilarityThreshold;
            templateUpdateThreshold = parameters.TemplateUpdateThreshold;
            refinementNeighborhood = parameters.RefinementNeighborhood;
            searchWindow = parameters.SearchWindow;
            blockWindow = parameters.BlockWindow;

            if (!blockWindow.FitsIn(searchWindow))
                searchWindow = blockWindow;
        }

        /// <summary>
        /// Computes the center of mass of the similarity scores in the vicinity of the best candidate.
        /// This allows to find a floating point location for the best match.
        /// </summary>
        private PointF RefineLocation(float[,,] data, Point loc, int neighborhood)
        {
            // The best candidate location is expanded by "neighborhood" pixels in each direction.
            float numX = 0;
            float numY = 0;
            float den = 0;
            for (int i = loc.X - neighborhood; i <= loc.X + neighborhood; i++)
            {
                if (i < 0 || i > data.GetUpperBound(1))
                    continue;

                for (int j = loc.Y - neighborhood; j <= loc.Y + neighborhood; j++)
                {
                    if (j < 0 || j > data.GetUpperBound(0))
                        continue;

                    float value = data[j, i, 0];
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
