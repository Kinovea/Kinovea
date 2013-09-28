#region License
/*
Copyright © Joan Charmant 2010.
joan.charmant@gmail.com 
 
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
        #region Members
        // Options - initialize in the constructor.
        private float similarityTreshold = 0.0f;					// Discard candidate block with lower similarity.
        private float templateUpdateThreshold = 1.0f;	// Only update the template if that dissimilar.
        private Size blockWindow = new Size(20, 20);						// Size of block to be matched.
        private Size searchWindow = new Size(100, 100);				// Size of window of candidates.
        
        // Monitoring, debugging.
        private static readonly bool monitoring = false;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion		
        
        #region Constructor
        public TrackerBlock2(int width, int height)
        {
            similarityTreshold = 0.50f;
            
            // If simi is better than this, we keep the same template, to avoid the template update drift.
            
            // When using CCORR : 0.90 or 0.95.
            // When using CCOEFF : 0.80
            templateUpdateThreshold = 0.80f;
            
            
            //int blockFactor = 15;	// Bigger template.
            int blockFactor = 20;	// Smaller template can improve tracking by focusing on the object instead of Bg.
            int blockWidth = width / blockFactor;
            int blockHeight = height / blockFactor;
            
            if(blockWidth < 20) 
            {
                blockWidth = 20;
            }
            
            if(blockHeight < 20) 
            {
                blockHeight = 20;
            }
            
            blockWindow = new Size(blockWidth, blockHeight);
            
            
            float searchFactor = 4.0f;
            searchWindow = new Size((int)(blockWidth * searchFactor), (int)(blockHeight * searchFactor));
            
            log.Debug(String.Format("Template matching: Image:{0}x{1}, Template:{2}, Search Window:{3}, Similarity thr.:{4}, Tpl update thr.:{5}",
                                    width, height, blockWindow.ToString(), searchWindow.ToString(), similarityTreshold, templateUpdateThreshold));
            
        }
        public TrackerBlock2(TrackerBlockParameters parameters)
        {
            similarityTreshold = parameters.SimilarityThreshold;
            templateUpdateThreshold = parameters.TemplateUpdateThreshold;
            searchWindow = parameters.SearchWindow;
            blockWindow = parameters.BlockWindow;
        }
        #endregion
        
        #region AbstractTracker Implementation
        public override bool Track(List<AbstractTrackPoint> previousPoints, Bitmap currentImage, long position, out AbstractTrackPoint currentPoint)
        {
            //---------------------------------------------------------------------
            // The input informations we have at hand are:
            // - The current bitmap we have to find the point into.
            // - The coordinates of all the previous points tracked.
            // - Previous tracking infos, stored in the TrackPoints tracked so far.
            //---------------------------------------------------------------------
                
            TrackPointBlock lastTrackPoint = (TrackPointBlock)previousPoints[previousPoints.Count - 1];
            Point lastPoint = lastTrackPoint.Point;
            
            bool matched = false;
            currentPoint = null;
            
            if (lastTrackPoint.Template != null && currentImage != null)
            {
                // Center search zone around last point.
                Point searchCenter = lastPoint;
                Rectangle searchZone = new Rectangle(	searchCenter.X - (searchWindow.Width/2), 
                                                        searchCenter.Y - (searchWindow.Height/2), 
                                                        searchWindow.Width, 
                                                        searchWindow.Height);
                
                searchZone.Intersect(new Rectangle(0,0,currentImage.Width, currentImage.Height));
                
                double bestScore = 0;
                Point bestCandidate = new Point(-1,-1);
                
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
                Point p1 = new Point(0,0);
                Point p2 = new Point(0,0);
                double fMin = 0;
                double fMax = 0;
                
                CvInvoke.cvMinMaxLoc(similarityMap.Ptr, ref fMin, ref fMax, ref p1, ref p2, IntPtr.Zero);
                
                if(fMax > similarityTreshold)
                {
                    bestCandidate = new Point(searchZone.Left + p2.X + tpl.Width / 2, searchZone.Top + p2.Y + tpl.Height / 2);
                    bestScore = fMax;
                }
            
                #region Monitoring
                if(monitoring)
                {
                    // Save the similarity map to file.
                    Image<Gray, Byte> mapNormalized = new Image<Gray, Byte>(similarityMap.Width, similarityMap.Height);
                    CvInvoke.cvNormalize(similarityMap.Ptr, mapNormalized.Ptr, 0, 255, NORM_TYPE.CV_MINMAX, IntPtr.Zero);
            
                    Bitmap bmpMap = mapNormalized.ToBitmap();
                    
                    string tplDirectory = @"C:\Documents and Settings\Administrateur\Mes documents\Dev  Prog\Videa\Video Testing\Tracking\Template Update";
                    bmpMap.Save(tplDirectory + String.Format(@"\simiMap-{0:000}-{1:0.00}.bmp", previousPoints.Count, bestScore));
                }
                #endregion
                
                // Result of the matching.
                if(bestCandidate.X != -1 && bestCandidate.Y != -1)
                {
                    // Save template in the point.
                    currentPoint = CreateTrackPoint(false, bestCandidate.X, bestCandidate.Y, bestScore, position, img, previousPoints);
                    ((TrackPointBlock)currentPoint).Similarity = bestScore;
                    
                    matched = true;
                }
                else
                {
                    // No match. Create the point at the center of the search window (whatever that might be).
                    currentPoint = CreateTrackPoint(false, searchCenter.X, searchCenter.Y, 0.0f, position, img, previousPoints);
                    log.Debug("Track failed. No block over the similarity treshold in the search window.");	
                }
            }
            else
            {
                // No image. (error case ?)
                // Create the point at the last point location.
                currentPoint = CreateTrackPoint(false, lastTrackPoint.X, lastTrackPoint.Y, 0.0f, position, currentImage, previousPoints);
                log.Debug("Track failed. No input image, or last point doesn't have any cached block image.");
            }
            
            return matched;
        }
        public override AbstractTrackPoint CreateTrackPoint(bool manual, int x, int y, double similarity, long t, Bitmap currentImage, List<AbstractTrackPoint> previousPoints)
        {
            // Creates a TrackPoint from the input image at the given coordinates.
            // Stores algorithm internal data in the point, to help next match.
            // _t is in relative timestamps from the first point.
            
            // Copy the template from the image into its own Bitmap.
            
            Bitmap tpl = new Bitmap(blockWindow.Width, blockWindow.Height, PixelFormat.Format32bppPArgb);
            
            bool updateWithCurrentImage = true;
            
            if(!manual && previousPoints.Count > 0 && similarity > templateUpdateThreshold)
            {
                // Do not update the template if it's not that different.
                TrackPointBlock prevBlock = previousPoints[previousPoints.Count - 1] as TrackPointBlock;
                if(prevBlock != null && prevBlock.Template != null)
                {		
                    tpl = AForge.Imaging.Image.Clone(prevBlock.Template);
                    updateWithCurrentImage = false;
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
                
                int startY = y - (blockWindow.Height / 2);
                int startX = x - (blockWindow.Width / 2);
                
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
                string tplDirectory = @"C:\Documents and Settings\Administrateur\Mes documents\Dev  Prog\Videa\Video Testing\Tracking\Template Update";
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
            
            TrackPointBlock tpb = new TrackPointBlock(x, y, t, tpl);
            tpb.IsReferenceBlock = manual;
            tpb.Similarity = manual ? 1.0f : similarity;
        
            return tpb;
        }
        public override AbstractTrackPoint CreateOrphanTrackPoint(int x, int y, long t)
        {
            // This creates a bare bone TrackPoint.
            // This is used only in the case of importing from xml.
            // The TrackPoint can't be used as-is to track the next one because it's missing the algo internal data (block).
            // We'll need to reconstruct it when we have the corresponding image.
            return new TrackPointBlock(x, y, t);
        }
        public override void Draw(Graphics canvas, Point point, IImageToViewportTransformer transformer, Color color, double opacityFactor)
        {
            // Draw the search and template boxes around the point.
            Point p = transformer.Transform(point);
            using(Pen pen = new Pen(Color.FromArgb((int)(opacityFactor * 192), color)))
            {
                canvas.DrawRectangle(pen, p.Box(transformer.Transform(searchWindow)));
                canvas.DrawRectangle(pen, p.Box(transformer.Transform(blockWindow)));
            }
        }
        public override Rectangle GetEditRectangle(Point position)
        {
            return position.Box(searchWindow);
        }
        #endregion
    }
}
