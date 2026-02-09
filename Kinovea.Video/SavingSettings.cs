#region License
/*
Copyright © Joan Charmant 2011.
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
using Kinovea.Services;

namespace Kinovea.Video
{
    /// <summary>
    /// Settings used by exporters using video frames (video, images, documents).
    /// </summary>
    public class SavingSettings
    {
        //-------------------------------
        // Input parameters
        // Describes which frames from the input we are interested in.
        //-------------------------------

        /// <summary>
        /// Section of the video to get the images from.
        /// </summary>
        public VideoSection Section = VideoSection.MakeEmpty();

        /// <summary>
        /// Whether to only collect the key images or all the frames.
        /// </summary>
        public bool KeyframesOnly = false;

        /// <summary>
        /// Interval between the frames to collect in timestamps.
        /// Zero means we collect every frame.
        /// Larger than zero means we do a timelapse-style export and jump from timestamp to timestamp.
        /// </summary>
        public double InputIntervalTimestamps = 0;

        //-------------------------------
        // Output parameters
        //-------------------------------

        /// <summary>
        /// File name where we'll save the result.
        /// </summary>
        public string File = "";

        /// <summary>
        /// Approximate number of images to export.
        /// This is used for the progress bar.
        /// </summary>
        public int TotalFrameCount = 0;

        /// <summary>
        /// Repeat count for normal images.
        /// This is used when saving slow motion video to a framerate that doesn't match the slow motion.
        /// It is sometimes needed because not all output formats support all framerates.
        /// </summary>
        public int Duplication = 1;

        /// <summary>
        /// Repeat count for keyframe images.
        /// This is used to "pause" the video on the key images.
        /// </summary>
        public int DuplicationKeyframes = 1;

        /// <summary>
        /// Whether to paint the drawings on the video frames.
        /// </summary>
        public bool FlushDrawings = true;
        
        /// <summary>
        /// Whether we are in the context of exporting a video with pauses on keyframes.
        /// This is true for video slideshow and video with pauses.
        /// </summary>
        public bool HasDuplicatedKeyframes = false;

        /// <summary>
        /// Interval between exported frames, in milliseconds.
        /// This is only used for video output.
        /// For slowed down or sped up output this may be different than the original.
        /// This should not be zero.
        /// </summary>
        public double OutputIntervalMilliseconds = 0.4;

        //-------------------------------
        // Helpers
        //-------------------------------

        /// <summary>
        /// A delegate taking a video frame and an output bitmap and painting the 
        /// video frame + the drawings at that particular time, to the output bitmap.
        /// </summary>
        public ImageRetriever ImageRetriever;

    }
}
