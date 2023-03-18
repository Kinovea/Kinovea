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
        /// User facing name of the filter.
        /// </summary>
        string FriendlyName { get; }

        /// <summary>
        /// Rendered bitmap to be displayed on the viewport.
        /// </summary>
        Bitmap Current { get; }

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
        /// Draw extra graphics on top of the current image.
        /// These may be dependent on the current time while the main image might not.
        /// </summary>
        void DrawExtra(Graphics canvas, IImageToViewportTransformer transformer, long timestamp);

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
        /// Called during serialization.
        /// The filter should export its configuration to an XML fragment.
        /// </summary>
        void WriteData(XmlWriter w);

        /// <summary>
        /// Called during deserialization.
        /// The filter should configure itself from the current XML fragment.
        /// </summary>
        void ReadData(XmlReader r);
        #endregion
    }
}
