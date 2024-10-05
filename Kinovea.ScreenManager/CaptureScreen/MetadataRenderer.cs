#region License
/*
Copyright © Joan Charmant 2013.
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
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Renderer for metadata.
    /// This renderer is currently only used by the capture screen and by the trajectory configuration window.
    /// The playback screen uses a different mechanism that hasn't been factored in yet.
    /// </summary>
    public class MetadataRenderer
    {
        private Metadata metadata;
        private bool renderTimedDrawings = true;
        private bool isSolo = false;
        private Guid soloId = Guid.Empty;
        private bool configureTracking = false;

        public MetadataRenderer(Metadata metadata, bool renderTimedDrawings)
        {
            this.metadata = metadata;
            this.renderTimedDrawings = renderTimedDrawings;
        }

        /// <summary>
        /// Enable solo mode for a drawing.
        /// The renderer will only draw this drawing.
        /// </summary>
        public void SetSoloMode(bool isSolo, Guid soloId, bool configureTracking)
        {
            this.isSolo = isSolo;
            this.soloId = soloId;
            this.configureTracking = configureTracking;
        }

        public void Render(Graphics viewportCanvas, Point imageLocation, float imageZoom, long timestamp)
        {
            if(metadata == null)
                return;

            IImageToViewportTransformer transformer = new ImageToViewportTransformer(imageLocation, imageZoom);

            viewportCanvas.PixelOffsetMode = PixelOffsetMode.HighSpeed;
            viewportCanvas.SmoothingMode = SmoothingMode.AntiAlias;

            RenderExtraDrawings(metadata, timestamp, viewportCanvas, transformer);
            RenderDrawings(metadata, timestamp, viewportCanvas, transformer);
            //RenderMagnifier();
        }

        private void RenderExtraDrawings(Metadata metadata, long timestamp, Graphics canvas, IImageToViewportTransformer transformer)
        {
            DistortionHelper distorter = null;
            CameraTransformer camTransformer = null;

            // Coordinate system and capture test grid.
            RenderDrawing(metadata.DrawingCoordinateSystem, canvas, distorter, camTransformer, transformer, false, timestamp);
            RenderDrawing(metadata.DrawingTestGrid, canvas, distorter, camTransformer, transformer, false, timestamp);

            if (renderTimedDrawings)
            {
                // Spotlights
                if (!isSolo || (isSolo && soloId == metadata.DrawingSpotlight.Id))
                {
                    metadata.DrawingSpotlight.Draw(canvas, distorter, camTransformer, transformer, false, timestamp);
                }

                // Numbers
                if (!isSolo || (isSolo && soloId == metadata.DrawingNumberSequence.Id))
                {
                    metadata.DrawingNumberSequence.Draw(canvas, distorter, camTransformer, transformer, false, timestamp);
                }

                // Chronometers
                foreach (AbstractDrawing ad in metadata.ChronoManager.Drawings)
                    RenderDrawing(ad, canvas, distorter, camTransformer, transformer, false, timestamp);

                // Trajectories
                foreach (AbstractDrawing ad in metadata.TrackManager.Drawings)
                    RenderDrawing(ad, canvas, distorter, camTransformer, transformer, false, timestamp);
            }
        }

        private void RenderDrawings(Metadata metadata, long timestamp, Graphics canvas, IImageToViewportTransformer transformer)
        {
            DistortionHelper distorter = metadata.CalibrationHelper.DistortionHelper;
            CameraTransformer camTransformer = metadata.CameraTransformer;

            foreach (Keyframe keyframe in metadata.Keyframes)
                foreach (AbstractDrawing drawing in keyframe.Drawings)
                    RenderDrawing(drawing, canvas, distorter, camTransformer, transformer, drawing == metadata.HitDrawing, timestamp);
        }

        private void RenderDrawing(AbstractDrawing drawing, Graphics canvas, DistortionHelper distorter, CameraTransformer cameraTransformer, IImageToViewportTransformer transformer, bool selected, long timestamp)
        {
            if (!isSolo || (isSolo && soloId == drawing.Id))
            {
                if (isSolo && configureTracking)
                {
                    if (drawing is DrawingTrack)
                    {
                        DrawingTrack track = drawing as DrawingTrack;
                        track.SetConfiguring(true);
                        track.Draw(canvas, distorter, cameraTransformer, transformer, false, timestamp);
                        track.SetConfiguring(false);
                    }
                    else
                    {
                        // TODO: set trackable drawings to configuration mode.
                        drawing.Draw(canvas, distorter, cameraTransformer, transformer, false, timestamp);
                    }
                }
                else
                {
                    drawing.Draw(canvas, distorter, cameraTransformer, transformer, false, timestamp);
                }
            }
        }
    }
}
