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
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Kinovea.ScreenManager.Languages;
using Kinovea.Services;
using Kinovea.Video;
using Kinovea.Video.FFMpeg;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// FrameServerPlayer controls the video reader and the metadata for a single video.
    /// PlayerScreenUserInterface is the View, FrameServerPlayer is the controller.
    /// </summary>
    public class FrameServerPlayer
    {
        #region Properties
        public VideoReader VideoReader
        {
            get { return videoReader; }
        }
        public HistoryStack HistoryStack
        {
            get { return historyStack; }
        }
        public Metadata Metadata
        {
            get { return metadata; }
            set { metadata = value; }
        }
        public ImageTransform ImageTransform
        {
            get { return metadata.ImageTransform; }
        }
        public bool Loaded
        {
            get { return videoReader != null && videoReader.Loaded; }
        }
        public Bitmap CurrentImage 
        {
            get 
            { 
                if(videoReader == null || !videoReader.Loaded || videoReader.Current == null)
                {
                    return null;
                }
                else if (metadata.ActiveVideoFilter != null && metadata.ActiveVideoFilter.Current != null)
                {
                    return metadata.ActiveVideoFilter.Current;
                }
                else
                {
                    return videoReader.Current.Image;
                }
            }
        }
        #endregion
        
        #region Members
        private VideoReader videoReader;
        private HistoryStack historyStack;
        private Metadata metadata;

        #region Video geometry settings
        // Settings that aren't saved in metadata.
        Size presentationSize = Size.Empty;
        bool allowPreScaling = true;
        List<TimedPoint> stabilizationData = null;
        #endregion

        private bool savingMetada;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public FrameServerPlayer(HistoryStack historyStack)
        {
            this.historyStack = historyStack;
        }
        #endregion

        #region Public

        /// <summary>
        /// Open and load a video or replay watcher by path.
        /// Creates an appropriate video reader and assign it to `videoReader`.
        /// 
        /// `filePath` may be:
        /// - the id of a capture folder.
        /// - a wildcard path like "G:\video\*".
        /// - a file path of a single video or image.
        /// - a file path of an image that is part of a sequence.
        /// </summary>
        public OpenVideoResult Load(string filePath)
        {
            // Instanciate appropriate video reader class.
            string sequenceFilename = FilesystemHelper.GetSequenceFilename(filePath);
            if (!string.IsNullOrEmpty(sequenceFilename))
            {
                filePath = Path.Combine(Path.GetDirectoryName(filePath), sequenceFilename);
                videoReader = VideoTypeManager.GetImageSequenceReader();
            }
            else
            {
                CaptureFolder cf = FilesystemHelper.GetCaptureFolder(filePath);
                if (cf != null)
                {
                    // We are loading a replay watcher on a known capture folder.
                    var context = DynamicPathResolver.BuildDateContext();
                    string folderPath = DynamicPathResolver.Resolve(cf.Path, context);

                    if (!FilesystemHelper.IsValidPath(folderPath))
                    {
                        log.ErrorFormat("Replay watcher started on invalid path {0}", folderPath);
                        return OpenVideoResult.FileNotOpenned;
                    }

                    filePath = Path.Combine(folderPath, "*");
                    filePath = VideoTypeManager.GetMostRecentSupportedVideo(filePath);
                    if (string.IsNullOrEmpty(filePath))
                    {
                        // If the directory doesn't have any supported files yet it's not an error.
                        // Just load an empty player and get ready.
                        return OpenVideoResult.EmptyWatcher;
                    }
                }
                else if (FilesystemHelper.IsReplayWatcher(filePath))
                {
                    // We are loading a replay watcher on a file system folder.
                    // These should not contain variables.
                    // We shouldn't come here anymore.
                    filePath = VideoTypeManager.GetMostRecentSupportedVideo(filePath);
                    if (string.IsNullOrEmpty(filePath))
                    {
                        // If the directory doesn't have any supported files yet it's not an error.
                        // Just load an empty player and get ready.
                        return OpenVideoResult.EmptyWatcher;
                    }
                }

                // At this point the file path should point to a video file.
                // Get the video reader module for the target file.
                videoReader = VideoTypeManager.GetVideoReader(Path.GetExtension(filePath));
            }

            try
            {
                if(videoReader != null)
                {
                    
                    // Open will set up videoReader.Info and videoReader.Geometry.
                    OpenVideoResult res = videoReader.Open(filePath);

                    if (res == OpenVideoResult.Success)
                    {
                        // Deinterlacing and Image aspect ratio have already been initialized
                        // to the default from preferences in Metadata constructor.
                        metadata.ImageRotation = videoReader.Info.OriginalRotation;

                        // Publish an initial geometry request to serve as reference for the reader.
                        // This one will have presentationSize at 0x0 and allowPreScaling = true.
                        PublishVideoGeometryRequest();
                    }

                    return res;

                }
                else
                {
                    return OpenVideoResult.NotSupported;
                }
            }
            catch
            {
                return OpenVideoResult.NotSupported;
            }
        }

        /// <summary>
        /// This is called when the screen is about to be emptied, 
        /// we are about to load a new video in the same screen,
        /// or the video failed to load correctly.
        /// </summary>
        public void Unload()
        {
            // Prepare the FrameServer for a new video by resetting everything.
            if(videoReader != null && videoReader.Loaded)
                videoReader.Close();
            
            metadata?.HardReset();

            ImageTransform.Reset();
        }

        /// <summary>
        /// Set up the metadata related to the video itself.
        /// This is done after loading the video the first time or after loading a new metadata file.
        /// </summary>
        public void SetupMetadata(bool init)
        {
            // Setup Metadata global infos in case we want to flush it to a file.
            
            if(metadata == null || videoReader == null)
                return;

            if (init)
            {
                metadata.ImageSize = videoReader.Geometry.ReferenceSize;
                metadata.ImageRotation = videoReader.Info.OriginalRotation;
                // aspect, mirror, deinterlace, etc. should have the same defaults.

                metadata.BaselineFrameInterval = videoReader.Info.FrameIntervalMilliseconds;
                metadata.AverageTimeStampsPerFrame = videoReader.Info.AverageTimeStampsPerFrame;
                metadata.AverageTimeStampsPerSecond = videoReader.Info.AverageTimeStampsPerSeconds;
                metadata.FirstTimeStamp = videoReader.Info.FirstTimeStamp;
                metadata.CalibrationHelper.CaptureFramesPerSecond = videoReader.Info.FramesPerSeconds;
            }

            metadata.PostSetupVideo(init);
            
            log.Debug("Setup metadata.");
        }

        /// <summary>
        /// Returns a textual representation of a time or duration in the user-preferred format.
        /// The time must be passed in absolute timestamps, and the time type is used to make it relative.
        /// This is the implementation of the "TimeCodeBuilder" delegate used by drawings.
        /// </summary>
        public string TimeStampsToTimecode(long timestamps, TimeType type, TimecodeFormat format, bool symbol)
        {
            if (videoReader == null || !videoReader.Loaded)
                return "0";

            TimecodeFormat tcf = format == TimecodeFormat.Unknown ? PreferencesManager.PlayerPreferences.TimecodeFormat : format;
            long actualTimestamps;
            switch (type)
            {
                case TimeType.UserOrigin:
                    actualTimestamps = timestamps - metadata.TimeOrigin;
                    break;
                case TimeType.Absolute:
                case TimeType.Duration:
                default:
                    actualTimestamps = timestamps;
                    break;
            }

            double averageTimestampsPerFrame = videoReader.Info.AverageTimeStampsPerFrame;

            int fractionalFrames = 0;
            if (averageTimestampsPerFrame != 0)
                fractionalFrames = (int)Math.Round(actualTimestamps / averageTimestampsPerFrame);

            if (type == TimeType.Duration)
                fractionalFrames++;

            double milliseconds = fractionalFrames * videoReader.Info.FrameIntervalMilliseconds / metadata.HighSpeedFactor;
            double framerate = 1000.0 / videoReader.Info.FrameIntervalMilliseconds * metadata.HighSpeedFactor;
            double durationTimestamps = videoReader.Info.DurationTimeStamps - averageTimestampsPerFrame;
            double totalFrames = durationTimestamps / averageTimestampsPerFrame;

            return TimeHelper.GetTimestring(framerate, fractionalFrames, milliseconds, actualTimestamps, durationTimestamps, totalFrames, tcf, symbol);
        }

        /// <summary>
        /// Returns the physical time in microseconds for this timestamp.
        /// Used in the context of synchronization.
        /// Input in timestamps relative to sel start.
        /// convert it into video time then to real time using high speed factor.
        public long TimestampToRealtime(long timestamp)
        {
            double correctedTPS = videoReader.Info.FrameIntervalMilliseconds * videoReader.Info.AverageTimeStampsPerSeconds / metadata.BaselineFrameInterval;

            if (correctedTPS == 0 || metadata.HighSpeedFactor == 0)
                return 0;

            double videoSeconds = (double)timestamp / correctedTPS;
            double realSeconds = videoSeconds / metadata.HighSpeedFactor;
            double realMicroseconds = realSeconds * 1000000;
            return (long)realMicroseconds;
        }

        public void ActivateVideoFilter(VideoFilterType type)
        {
            metadata.ActivateVideoFilter(type);
            metadata.ActiveVideoFilter.SetFrames(VideoReader.WorkingZoneFrames);
        }

        public void DeactivateVideoFilter()
        {
            metadata.DeactivateVideoFilter();
        }

        #endregion

        #region Update video geometry
        public bool ChangePresentationSize(Size value)
        {
            if (value == presentationSize)
                return false;

            presentationSize = value;

            return PublishVideoGeometryRequest();
        }

        public bool ChangeAllowPrescaling(bool value)
        {
            if (value == allowPreScaling)
                return false;

            allowPreScaling = value;

            return PublishVideoGeometryRequest();
        }

        public bool ChangeImageAspect(ImageAspectRatio value)
        {
            if (!VideoReader.CanChangeAspectRatio)
                return false;

            if (value == metadata.ImageAspect)
                return false;

            metadata.ImageAspect = value;

            return PublishVideoGeometryRequest();
        }

        public bool ChangeImageRotation(ImageRotation value)
        {
            if (!VideoReader.CanChangeImageRotation)
                return false;

            if (value == metadata.ImageRotation)
                return false;

            metadata.ImageRotation = value;

            // If we are sideways change the presentation size.
            // This is a temporary hack because we pre-constrain the canvas inside the viewport.
            // When we switch to normal zooming we should be able to pass the zoom factor and 
            // the full viewport size and let the reader do the maths.
            if (value == ImageRotation.Rotate90 || value == ImageRotation.Rotate270)
            {
                presentationSize = new Size(presentationSize.Height, presentationSize.Width);
            }

            return PublishVideoGeometryRequest();
        }

        public bool ChangeDemosaicing(Demosaicing value)
        {
            if (!VideoReader.CanChangeDemosaicing)
                return false;

            metadata.Demosaicing = value;

            return PublishVideoGeometryRequest();
        }

        public bool ChangeDeinterlacing(bool value)
        {
            if (!VideoReader.CanChangeDeinterlacing)
                return false;

            metadata.Deinterlacing = value;

            return PublishVideoGeometryRequest();
        }

        public bool ChangeStabilizationTrack(Guid id)
        {
            if (!VideoReader.CanStabilize)
                return false;

            metadata.StabilizationTrack = id;
            UpdateStabilizationData();

            return PublishVideoGeometryRequest();
        }

        private void UpdateStabilizationData()
        {
            Guid id = metadata.StabilizationTrack;
            if (id == Guid.Empty)
            {
                stabilizationData = null;
            }
            else
            {
                // Find the track object.
                var drawing = metadata.GetDrawing(metadata.TrackManager.Id, id);
                DrawingTrack track = drawing as DrawingTrack;
                if (track == null)
                {
                    log.ErrorFormat("Stabilization track not found: {0}", id);
                    stabilizationData = null;
                }

                List<TimedPoint> points = track.GetTimedPoints();
                stabilizationData = points;
            }
        }

        public bool ChangeMirror(bool value)
        {
            // Nothing else to do, mirroring is handled at render time.
            metadata.Mirrored = value;
            return false;
        }

        public bool ChangeBackgroundColor(Color value)
        {
            metadata.BackgroundColor = value;
            return false;
        }

        /// <summary>
        /// Consolidate image options after metadata import.
        /// </summary>
        public void RestoreImageOptions()
        {
            // Note: for now we assume the reader for this video has the same capabilities as 
            // what is saved in the metadata file.
            // If not, the reader should ignore unsupported options anyway.
            UpdateStabilizationData();
            PublishVideoGeometryRequest();

            // Options affecting render side only.
            ChangeMirror(metadata.Mirrored);
            ChangeBackgroundColor(metadata.BackgroundColor);
        }

        private bool PublishVideoGeometryRequest()
        {
            log.DebugFormat("Video geometry request: Presentation size: {0}x{1}, Allow pre-scaling: {2}.",
                presentationSize.Width, presentationSize.Height, allowPreScaling);

            VideoGeometryRequest request = new VideoGeometryRequest(
                presentationSize,
                allowPreScaling,
                metadata.ImageAspect,
                metadata.ImageRotation,
                metadata.Demosaicing,
                metadata.Deinterlacing,
                stabilizationData
            );

            return VideoReader.UpdateVideoGeometry(request);
        }
        #endregion

        #region Support functions for exporters that need the images
        public void AfterSave()
        {
            if(savingMetada)
            {
                Metadata.ResetContentHash();
                savingMetada = false;
            }

            NotificationCenter.RaiseRefreshFileList(false);
        }

        /// <summary>
        /// Builds an image file name with the passed timecode.
        /// This returns a file name without the directory and without the extension.
        /// </summary>
        public string GetImageFilename(string videoFilePath, long timestamp, TimecodeFormat format)
        {
            if (format == TimecodeFormat.TimeAndFrames)
                format = TimecodeFormat.ClassicTime;

            string suffix = TimeStampsToTimecode(timestamp, TimeType.UserOrigin, format, false);
            string maxSuffix = TimeStampsToTimecode(metadata.SelectionEnd, TimeType.UserOrigin, format, false);

            switch (format)
            {
                case TimecodeFormat.Frames:
                case TimecodeFormat.Milliseconds:
                case TimecodeFormat.Microseconds:
                case TimecodeFormat.TenThousandthOfHours:
                case TimecodeFormat.HundredthOfMinutes:

                    int padding = maxSuffix.Length - suffix.Length;
                    for (int i = 0; i < padding; i++)
                        suffix = suffix.Insert(0, "0");
                    break;
                default:
                    break;
            }

            // Reconstruct filename
            return Path.GetFileNameWithoutExtension(videoFilePath) + "-" + suffix.Replace(':', '.');
        }
        #endregion

        /// <summary>
        /// Enumerates the images from the video, for export purposes.
        /// This includes skipping and duplicating frames as needed.
        /// This returns an internal bitmap and the caller should do its own copy.
        /// Prebuffering must be deactivated prior to enumerating.
        /// </summary>
        public IEnumerable<Bitmap> EnumerateImages(VideoExportSettings settings)
        {
            if (videoReader.DecodingMode == VideoDecodingMode.PreBuffering)
            {
                log.ErrorFormat("Frame enumeration called while prebuffering.");
                yield break;
            }

            // Use one reusable staging bitmap.
            Bitmap staging = null;

            if (settings.KeyframesOnly)
            {
                throw new InvalidProgramException("EnumerateImages should not be called with KeyframesOnly, use EnumerateKeyImages instead.");
            }

            // Enumerates the raw frames from the video.
            int count = 0;
            foreach (VideoFrame vf in videoReader.EnumerateFrames(settings.InputIntervalTimestamps))
            {
                if (vf == null)
                {
                    log.Error("Working zone enumerator yield null.");
                    
                    if (staging != null)
                        staging.Dispose();

                    yield break;
                }

                //log.DebugFormat("Enumerated frame [{0}]: {1}", count, vf.Timestamp);
                count++;

                // Initialize the output Bitmap if not done already.
                if (staging == null)
                {
                    staging = new Bitmap(settings.RenderingSize.Width, settings.RenderingSize.Height, settings.PixelFormat);
                }
                
                // Paint the frame + annotations to our bitmap.
                bool onKeyframe = settings.ImageRetriever(vf, staging);

                // Store the input timestamp in the bitmap, this may be used by the caller to build a file name for image exports.
                staging.Tag = vf.Timestamp;

                int repeatCount = (settings.HasDuplicatedKeyframes && onKeyframe) ? settings.DuplicationKeyframes : settings.Duplication;
                for (int i = 0; i < repeatCount; i++)
                { 
                    yield return staging;
                }
            }

            // End of enumeration.
            if (staging != null)
            {
                staging.Dispose();
            }
        }

        /// <summary>
        /// Lazily enumerates the keyframe images from the video, for export purposes.
        /// Same as above but jumps from key frame to key frame.
        /// Returns an internal bitmap and the caller should do its own copy.
        /// Prebuffering must be deactivated prior to enumerating.
        /// </summary>
        public IEnumerable<Bitmap> EnumerateKeyImages(VideoExportSettings settings)
        {
            if (videoReader.DecodingMode == VideoDecodingMode.PreBuffering)
            {
                log.ErrorFormat("Frame enumeration called while prebuffering.");
                yield break;
            }

            // Use one reusable staging bitmap.
            Bitmap staging = null;

            var keyframes = this.metadata.Keyframes;
            long currentTimestamp = settings.Section.Start;

            for (int i = 0; i < keyframes.Count; i++)
            {
                var kf = keyframes[i];
                if (kf.Timestamp < settings.Section.Start)
                {
                    continue;
                }

                if (kf.Timestamp > settings.Section.End)
                {
                    break;
                }

                // Move timeline to the target timestamp and get the image.
                videoReader.MoveTo(kf.Timestamp);
                VideoFrame vf = videoReader.Current;
                currentTimestamp = vf.Timestamp;

                // Initialize the output Bitmap if not done already.
                if (staging == null)
                {
                    staging = new Bitmap(vf.Image.Width, vf.Image.Height, settings.PixelFormat);
                }

                // Paint the frame + annotations to our bitmap.
                settings.ImageRetriever(vf, staging);

                // Store the input timestamp in the bitmap, this may be used by the caller to build a file name for image exports.
                staging.Tag = vf.Timestamp;

                yield return staging;
            }

            // End of enumeration.
            if (staging != null)
            {
                staging.Dispose();
            }
        }

        public void ReportError(VideoExportResult exportResult)
        {
            if (exportResult == VideoExportResult.Cancelled)
                return;

            string error = ScreenManagerLang.Error_SaveMovie_LowLevelError;
            MessageBox.Show(
                error.Replace("\\n", "\n"),
                ScreenManagerLang.Error_SaveMovie_Title,
                MessageBoxButtons.OK,
                MessageBoxIcon.Exclamation);
        }
    }
}
