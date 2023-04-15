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
        //-------------------------------

        /// <summary>
        /// Section of the video to get the images from.
        /// </summary>
        public VideoSection Section = VideoSection.Empty;

        /// <summary>
        /// Whether to only collect the key images or all the frames.
        /// </summary>
        public bool KeyframesOnly = false;

        public double InputIntervalMilliseconds = 0.4;

        //-------------------------------
        // Output parameters
        //-------------------------------

        /// <summary>
        /// File name where we'll save the result.
        /// </summary>
        public string File = "";

        /// <summary>
        /// Approximate number of images to export.
        /// </summary>
        public int EstimatedTotal = 0;

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
        public int KeyframeDuplication = 1;

        /// <summary>
        /// Whether to paint the drawings on the video frames.
        /// </summary>
        public bool FlushDrawings = true;
        
        /// <summary>
        /// Whether we are in the context of exporting a video with pauses.
        /// </summary>
        public bool PausedVideo = false;

        /// <summary>
        /// Interval between exported frames, in milliseconds.
        /// For timelapse or image sequence this will be different than in the original video.
        /// For video output this is used to compute the bitrate.
        /// For image sequence output this is not used, use OutputIntervalTimestamps instead.
        /// Zero means we export every frame.
        /// </summary>
        public double OutputIntervalMilliseconds = 0;

        /// <summary>
        /// Interval between exported frames for timelapse or image sequence, in timestamps.
        /// Zero means we export every frame.
        /// </summary>
        public long OutputIntervalTimestamps = 0;

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
