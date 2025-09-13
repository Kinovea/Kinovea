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
    /// FrameServerPlayer encapsulate the video file, meta data and everything 
    /// needed to render the frame and access file functions.
    /// PlayerScreenUserInterface is the View, FrameServerPlayer is the Model.
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
        /// Load a video or replay watcher by path.
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
                    videoReader.Options = new VideoOptions(
                        PreferencesManager.PlayerPreferences.AspectRatio, 
                        ImageRotation.Rotate0, 
                        Demosaicing.None, 
                        PreferencesManager.PlayerPreferences.DeinterlaceByDefault);

                    return videoReader.Open(filePath);
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
            
            if(metadata != null)
                metadata.HardReset();
        }

        /// <summary>
        /// Set up the metadata related to the video itself.
        /// This is done after loading the video the first time or after loading a new metadata file.
        /// </summary>
        public void SetupMetadata(bool init)
        {
            // Setup Metadata global infos in case we want to flush it to a file (or mux).
            
            if(metadata == null || videoReader == null)
                return;

            if (init)
            {
                metadata.ImageSize = videoReader.Info.ReferenceSize;
                metadata.ImageRotation = videoReader.Info.ImageRotation;
                // aspect and mirror ?

                metadata.BaselineFrameInterval = videoReader.Info.FrameIntervalMilliseconds;
                metadata.AverageTimeStampsPerFrame = videoReader.Info.AverageTimeStampsPerFrame;
                metadata.AverageTimeStampsPerSecond = videoReader.Info.AverageTimeStampsPerSeconds;
                metadata.FirstTimeStamp = videoReader.Info.FirstTimeStamp;
                metadata.CalibrationHelper.CaptureFramesPerSecond = videoReader.Info.FramesPerSeconds;
            }

            metadata.PostSetupVideo(init);
            
            log.Debug("Setup metadata.");
        }

        public bool ChangeImageAspect(ImageAspectRatio value)
        {
            if (!VideoReader.CanChangeAspectRatio)
                return false;

            metadata.ImageAspect = value;
            return VideoReader.ChangeAspectRatio(value);
        }

        public bool ChangeImageRotation(ImageRotation value)
        {
            if (!VideoReader.CanChangeImageRotation)
                return false;

            metadata.ImageRotation = value;
            return VideoReader.ChangeImageRotation(value);
        }

        public bool ChangeMirror(bool value)
        {
            // Nothing else to do, mirroring is handled at render time.
            metadata.Mirrored = value;
            return false;
        }

        public bool ChangeDemosaicing(Demosaicing value)
        {
            if (!VideoReader.CanChangeDemosaicing)
                return false;

            metadata.Demosaicing = value;
            return VideoReader.ChangeDemosaicing(value);
        }

        public bool ChangeDeinterlacing(bool value)
        {
            if (!VideoReader.CanChangeDeinterlacing)
                return false;

            metadata.Deinterlacing = value;
            return VideoReader.ChangeDeinterlace(value);
        }

        public bool ChangeBackgroundColor(Color value)
        {
            metadata.BackgroundColor = value;
            return false;
        }

        public bool SetStabilizationTrack(Guid id)
        {
            if (!VideoReader.CanStabilize)
                return false;

            // This function is called either when selecting a track in the 
            // Image > Stabilization menu, or when reloading a KVA file.
            // In the case of loading the following assignation is redundant.
            metadata.StabilizationTrack = id;

            if (id == Guid.Empty)
            {
                VideoReader.SetStabilizationData(null);
            }
            else
            {
                // Find the track object.
                var drawing = metadata.GetDrawing(metadata.TrackManager.Id, id);
                DrawingTrack track = drawing as DrawingTrack;
                if (track == null)
                {
                    log.ErrorFormat("Stabilization track not found: {0}", id);
                    return false;
                }

                List<TimedPoint> points = track.GetTimedPoints();
                VideoReader.SetStabilizationData(points);
            }

            return true;
            // Find the track object.
            //var drawing = frameServer.Metadata.GetDrawing(frameServer.Metadata.TrackManager.Id, trackId);
            //DrawingTrack track2 = drawing as DrawingTrack;
            //if (track2 == null)
            //{
            //    throw new InvalidProgramException();
            //}

            //// Tell the metadata that we are stabilizing on this track.
            //metadata.StabilizationTrack = track2.Id;
            
            //// Hide the track.
            //track.IsVisible = false;

            //bool uncached = frameServer.VideoReader.SetStabilizationData(points);

            //if (uncached && frameServer.VideoReader.DecodingMode == VideoDecodingMode.Caching)
            //    view.UpdateWorkingZone(true);
        }

        /// <summary>
        /// Consolidate image options after metadata import.
        /// </summary>
        public void RestoreImageOptions()
        {
            ChangeImageAspect(metadata.ImageAspect);
            ChangeImageRotation(metadata.ImageRotation);
            ChangeMirror(metadata.Mirrored);
            ChangeDemosaicing(metadata.Demosaicing);
            ChangeDeinterlacing(metadata.Deinterlacing);
            ChangeBackgroundColor(metadata.BackgroundColor);
            SetStabilizationTrack(metadata.StabilizationTrack);
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
                case TimeType.WorkingZone:
                    actualTimestamps = timestamps - videoReader.WorkingZone.Start;
                    break;
                case TimeType.UserOrigin:
                    actualTimestamps = timestamps - metadata.TimeOrigin;
                    break;
                case TimeType.Absolute:
                case TimeType.Duration:
                default:
                    actualTimestamps = timestamps;
                    break;
            }

            // TODO: use double for info.AverageTimestampsPerFrame.
            double averageTimestampsPerFrame = videoReader.Info.AverageTimeStampsPerSeconds / videoReader.Info.FramesPerSeconds;

            int frames = 0;
            if (averageTimestampsPerFrame != 0)
                frames = (int)Math.Round(actualTimestamps / averageTimestampsPerFrame);

            if (type == TimeType.Duration)
                frames++;

            double milliseconds = frames * metadata.BaselineFrameInterval / metadata.HighSpeedFactor;
            double framerate = 1000.0 / metadata.BaselineFrameInterval * metadata.HighSpeedFactor;
            double durationTimestamps = videoReader.Info.DurationTimeStamps - averageTimestampsPerFrame;
            double totalFrames = durationTimestamps / averageTimestampsPerFrame;

            return TimeHelper.GetTimestring(framerate, frames, milliseconds, actualTimestamps, durationTimestamps, totalFrames, tcf, symbol);
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
        /// Lazily enumerates the images from the video, for export purposes.
        /// This includes skipping and duplicating frames as needed.
        /// This returns an internal bitmap and the caller should do its own copy.
        /// </summary>
        public IEnumerable<Bitmap> EnumerateImages(SavingSettings settings)
        {
            Bitmap output = null;
            int consumedKeyframes = 0;

            // Enumerates the raw frames from the video.
            foreach (VideoFrame vf in videoReader.EnumerateFrames(settings.InputIntervalTimestamps))
            {
                if (vf == null)
                {
                    log.Error("Working zone enumerator yield null.");
                    
                    if (output != null)
                        output.Dispose();

                    yield break;
                }

                // We have a video frame.
                bool isKeyframe = this.metadata.IsKeyframe(vf.Timestamp);
                if (settings.KeyframesOnly && !isKeyframe)
                    continue;

                // Keep track of how many keyframes we have seen to exit early if we are only interested in these.
                if (isKeyframe)
                    consumedKeyframes++;

                // Initialize the output Bitmap if not done already.
                if (output == null)
                    output = new Bitmap(vf.Image.Width, vf.Image.Height, vf.Image.PixelFormat);
                
                // Paint the frame + annotations to our bitmap.
                bool onKeyframe = settings.ImageRetriever(vf, output);

                // Store the input timestamp in the bitmap, this may be used by the caller to build a file name for image exports.
                output.Tag = vf.Timestamp;

                int repeatCount = (settings.HasDuplicatedKeyframes && onKeyframe) ? settings.DuplicationKeyframes : settings.Duplication;
                for (int i = 0; i < repeatCount; i++)
                { 
                    yield return output;
                }

                // If we are done getting keyframes, no need to enumerate further.
                if (settings.KeyframesOnly && consumedKeyframes == metadata.Keyframes.Count)
                {
                    if (output != null)
                        output.Dispose();
                    
                    yield break;
                }
            }

            // End of enumeration.
            if (output != null)
                output.Dispose();
        }
        
        public void ReportError(SaveResult saveResult)
        {
            switch(saveResult)
            {
                case SaveResult.Cancelled:
                    // No error message if the user cancelled herself.
                    break;
                
                case SaveResult.FileHeaderNotWritten:
                case SaveResult.FileNotOpened:
                    DisplayErrorMessage(ScreenManagerLang.Error_SaveMovie_FileError);
                    break;
                
                case SaveResult.EncoderNotFound:
                case SaveResult.EncoderNotOpened:
                case SaveResult.EncoderParametersNotAllocated:
                case SaveResult.EncoderParametersNotSet:
                case SaveResult.InputFrameNotAllocated:
                case SaveResult.MuxerNotFound:
                case SaveResult.MuxerParametersNotAllocated:
                case SaveResult.MuxerParametersNotSet:
                case SaveResult.VideoStreamNotCreated:
                case SaveResult.ReadingError:
                case SaveResult.UnknownError:
                default:
                    DisplayErrorMessage(ScreenManagerLang.Error_SaveMovie_LowLevelError);
                    break;
            }
        }
        
        private void DisplayErrorMessage(string error)
        {
            MessageBox.Show(
                error.Replace("\\n", "\n"),
                ScreenManagerLang.Error_SaveMovie_Title,
                MessageBoxButtons.OK,
                MessageBoxIcon.Exclamation);
        }
    }
}
