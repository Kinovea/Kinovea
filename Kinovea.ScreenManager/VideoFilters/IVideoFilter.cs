#region License
/*
Copyright © Joan Charmant 2009.
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
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using Kinovea.Video;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// IVideoFilter defines the way all Video filters should behave.
    /// A VideoFilter here is an object that creates a display frame
    /// on the fly by processing the cached frames.
    /// </summary>
    public interface IVideoFilter : IDisposable
    {
        #region Properties
        /// <summary>
        /// Filter type.
        /// </summary>
        VideoFilterType Type { get; }

        /// <summary>
        /// Resource string for the user facing name of the filter.
        /// </summary>
        string FriendlyNameResource { get; }

        /// <summary>
        /// Rendered bitmap to be displayed on the viewport.
        /// </summary>
        Bitmap Current { get; }

        /// <summary>
        /// Whether this filter has data that should be saved to KVA.
        /// This is true when 1. the filter has been visited by the user,
        /// and 2. the filter should save its data to KVA in the first place.
        /// </summary>
        bool HasKVAData { get; }

        /// <summary>
        /// Whether this filter has a set of custom context menus.
        /// They will then be retrieved from GetContextMenu.
        /// </summary>
        bool HasContextMenu { get; }

        /// <summary>
        /// Whether the aspect ratio of the output image is inverted compared to the input images.
        /// This is useful when the input images are in portrait mode but the output is better in landscape mode.
        /// The main viewport should take this into account and change the viewport aspect ratio accordingly
        /// while still providing images in their original aspect ratio.
        /// </summary>
        bool RotatedCanvas { get; }

        /// <summary>
        /// Whether we should draw the keyframe-attached drawings on top of the bitmap rendered by this filter.
        /// </summary>
        bool DrawAttachedDrawings { get; }

        /// <summary>
        /// Whether we should draw the detached drawings on top of the bitmap rendered by this filter.
        /// </summary>
        bool DrawDetachedDrawings { get; }

        /// <summary>
        /// Whether this filter is capable of exporting video.
        /// If this is true a "Save video" menu will be shown 
        /// and the `ExportVideo` callback should be implemented.
        /// </summary>
        bool CanExportVideo { get; }

        /// <summary>
        /// Whether this filter is capable of exporting single images.
        /// If this is true a "Save image" menu will be shown 
        /// and the `ExportImage` callback should be implemented.
        /// </summary>
        bool CanExportImage { get; }

        /// <summary>
        /// Whether this filter is capable of exporting its data 
        /// to external files (not KVA).
        /// This only makes sense for filters whose data is not 
        /// part of the KVA file, like lens calibration.
        /// If this is true a custom save menu will be requested 
        /// via GetExportDataMenu.
        /// </summary>
        bool CanExportData { get; }

        /// <summary>
        /// A hash of the settings of the filter, used to detect changes and prompt user for saving.
        /// </summary>
        int ContentHash { get; }
        #endregion

        #region Methods

        /// <summary>
        /// Retrieve the list of context menus specific to the filter.
        /// The menus for exporting images, videos and data should not be present in this menu,
        /// they will be automatically created at the screen level based on the CanExportVideo and 
        /// CanExportImage properties and the event handlers will call into ExportVideo and ExportImage.
        /// </summary>
        List<ToolStripItem> GetContextMenu(PointF pivot, long timestamp);


        /// <summary>
        /// Retrieve a collection of menus for exporting the filter data.
        /// This will only be called if `CanExportData` is true.
        /// </summary>
        List<ToolStripItem> GetExportDataMenu();

        /// <summary>
        /// Called by the screen when the number or content of the frame buffer has changed.
        /// The filter should reset itself with the new frames, while keeping existing settings when possible.
        /// </summary>
        void SetFrames(IWorkingZoneFramesContainer framesContainer);

        /// <summary>
        /// Called when the query time changes. 
        /// If the filter has dynamic content it should update itself with a new image matching this time.
        /// </summary>
        void UpdateTime(long timestamp);

        /// <summary>
        /// Called when the time origin of the video changes.
        /// </summary>
        void UpdateTimeOrigin(long timestamp);

        /// <summary>
        /// Called when the user starts a move action on the filter image.
        /// </summary>
        void StartMove(PointF p);

        /// <summary>
        /// Called when the user terminates the move action on the filter image.
        /// </summary>
        void StopMove();

        /// <summary>
        /// Called for every mouse update during user move action.
        /// dx and dy are relative to the previous call.
        /// </summary>
        void Move(float dx, float dy, Keys modifiers);

        /// <summary>
        /// Called when the user scrolls with Alt while the filter is active.
        /// </summary>
        void Scroll(int steps, PointF p, Keys modifiers);

        /// <summary>
        /// Draw extra graphics on top of the current image.
        /// These may be dependent on the current time while the main image might not.
        /// </summary>
        void DrawExtra(Graphics canvas, DistortionHelper distorter, IImageToViewportTransformer transformer, long timestamp, bool export);

        /// <summary>
        /// Called when the user wants to export the video using images created by the filter.
        /// </summary>
        void ExportVideo(IDrawingHostView host);

        /// <summary>
        /// Called when the user wants to export the image currently displayed by the filter.
        /// This function must create its own saving dialog to allow for possible extra saving options.
        /// </summary>
        void ExportImage(IDrawingHostView host);

        /// <summary>
        /// Called when the filter should completely reset itself to a default state.
        /// This is called when a new video is loaded in the screen for example.
        /// </summary>
        void ResetData();

        /// <summary>
        /// Save filter data to KVA file.
        /// The filter should also have a parameters object with its own serialization mechanism
        /// for saving the default configuration to the preferences.
        /// This will only be called if HasKVAData is true.
        /// </summary>
        void WriteData(XmlWriter w);

        /// <summary>
        /// Called when loading filter data saved in KVA.
        /// </summary>
        void ReadData(XmlReader r);
        #endregion
    }
}
