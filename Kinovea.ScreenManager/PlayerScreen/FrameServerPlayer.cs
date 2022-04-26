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
    public class FrameServerPlayer : AbstractFrameServer
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
                else if (metadata.ActiveVideoFilter != null)
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
        private formProgressBar formProgressBar;
        private BackgroundWorker bgWorkerSave = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
        private SaveResult saveResult;
        private bool savingMetada;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public FrameServerPlayer(HistoryStack historyStack)
        {
            this.historyStack = historyStack;
            bgWorkerSave.ProgressChanged += bgWorkerSave_ProgressChanged;
            bgWorkerSave.RunWorkerCompleted += bgWorkerSave_RunWorkerCompleted;
            bgWorkerSave.DoWork += bgWorkerSave_DoWork;
        }
        #endregion
        
        #region Public
        public OpenVideoResult Load(string filePath)
        {
            // Instanciate appropriate video reader class.
            string sequenceFilename = FilesystemHelper.GetSequenceFilename(filePath);
            if (!string.IsNullOrEmpty(sequenceFilename))
            {
                videoReader = VideoTypeManager.GetImageSequenceReader();
                filePath = Path.Combine(Path.GetDirectoryName(filePath), sequenceFilename);
            }
            else
            {
                if (FilesystemHelper.IsReplayWatcher(filePath))
                {
                    // This happens when we first load a file watcher into this screen.
                    // Subsequent calls by the watcher will use the actual file name.
                    // For this initial step, run the most recent file of the directory, if any.
                    filePath = VideoTypeManager.GetMostRecentSupportedVideo(filePath);
                    if (string.IsNullOrEmpty(filePath))
                    {
                        // If the directory doesn't have any supported files yet it's not an error, we just load an empty player and get ready.
                        return OpenVideoResult.EmptyWatcher;
                    }
                }

                videoReader = VideoTypeManager.GetVideoReader(Path.GetExtension(filePath));
            }

            try
            {
                if(videoReader != null)
                {
                    videoReader.Options = new VideoOptions(PreferencesManager.PlayerPreferences.AspectRatio, ImageRotation.Rotate0, Demosaicing.None, PreferencesManager.PlayerPreferences.DeinterlaceByDefault);
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

        public void Unload()
        {
            // Prepare the FrameServer for a new video by resetting everything.
            if(videoReader != null && videoReader.Loaded)
                videoReader.Close();
            
            if(metadata != null)
                metadata.Reset();
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
                metadata.UserInterval = videoReader.Info.FrameIntervalMilliseconds;
                metadata.AverageTimeStampsPerFrame = videoReader.Info.AverageTimeStampsPerFrame;
                metadata.AverageTimeStampsPerSecond = videoReader.Info.AverageTimeStampsPerSeconds;
                metadata.CalibrationHelper.CaptureFramesPerSecond = videoReader.Info.FramesPerSeconds;
                metadata.FirstTimeStamp = videoReader.Info.FirstTimeStamp;
                metadata.ImageRotation = videoReader.Info.ImageRotation;
            }

            metadata.PostSetup(init);
            
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
            metadata.Mirrored = value;
            
            // Nothing else to do, mirroring is handled at render time.
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
        }

        public override void Draw(Graphics canvas)
        {
            // Draw the current image on canvas according to conf.
            // This is called back from screen paint method.
        }

        /// <summary>
        /// Main video export.
        /// </summary>
        public void SaveVideo(double playbackFrameInterval, double slowmotionPercentage, ImageRetriever imageRetriever)
        {
            // Show the intermediate dialog for export options.
            formVideoExport fve = new formVideoExport(videoReader.FilePath, slowmotionPercentage);
            if (fve.ShowDialog() != DialogResult.OK)
            {
                fve.Dispose();
                return;
            }

            if (!FilesystemHelper.CanWrite(fve.Filename))
            {
                DisplayErrorMessage(ScreenManagerLang.Error_SaveMovie_FileError);
                fve.Dispose();
                return;
            }

            DoSave(fve.Filename,
                   fve.UseSlowMotion ? playbackFrameInterval : metadata.UserInterval,
                   true,
                   false,
                   false,
                   imageRetriever);

            // Save this as the "preferred" format for video exports.
            PreferencesManager.PlayerPreferences.VideoFormat = FilesystemHelper.GetVideoFormat(fve.Filename);
            PreferencesManager.Save();
            
            fve.Dispose();
        }

        public void SaveDiaporama(ImageRetriever imageRetriever, bool diapo)
        {
            // Let the user configure the diaporama export.
            using(formDiapoExport fde = new formDiapoExport(diapo))
            {
                if(fde.ShowDialog() == DialogResult.OK)
                {
                    DoSave(fde.Filename, 
                            fde.FrameInterval,
                            true, 
                            fde.PausedVideo ? false : true,
                            fde.PausedVideo,
                            imageRetriever);
                }
            }
        }

        public void AfterSave()
        {
            if(savingMetada)
            {
                Metadata.CleanupHash();
                savingMetada = false;
            }

            NotificationCenter.RaiseRefreshFileExplorer(this, false);
        }

        /// <summary>
        /// Returns a textual representation of a time or duration in the user-preferred format.
        /// The time must be passed in absolute timestamps, and the time type is used to make it relative.
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

            double milliseconds = frames * metadata.UserInterval / metadata.HighSpeedFactor;
            double framerate = 1000.0 / metadata.UserInterval * metadata.HighSpeedFactor;
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
        
        #region Saving processing
        private void DoSave(string filePath, double frameInterval, bool flushDrawings, bool keyframesOnly, bool pausedVideo, ImageRetriever imageRetriever)
        {
            SavingSettings s = new SavingSettings();
            s.Section = videoReader.WorkingZone;
            s.File = filePath;
            s.InputFrameInterval = frameInterval;
            s.FlushDrawings = flushDrawings;
            s.KeyframesOnly = keyframesOnly;
            s.PausedVideo = pausedVideo;
            s.ImageRetriever = imageRetriever;
            
            formProgressBar = new formProgressBar(true);
            formProgressBar.Cancel = Cancel_Asked;
            bgWorkerSave.RunWorkerAsync(s);
            formProgressBar.ShowDialog();
        }
        
        #region Background worker event handlers
        private void bgWorkerSave_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.CurrentThread.Name = "Saving";
            BackgroundWorker bgWorker = sender as BackgroundWorker;

            if(!(e.Argument is SavingSettings))
            {
                saveResult = SaveResult.UnknownError;
                e.Result = 0;
                return;
            }
            
            SavingSettings settings = (SavingSettings)e.Argument;
            
            if(settings.ImageRetriever == null || settings.InputFrameInterval < 0 || bgWorker == null)
            {
                saveResult = SaveResult.UnknownError;
                e.Result = 0;
                return;
            }
            
            try
            {
                log.DebugFormat("Saving selection [{0}]->[{1}] to: {2}", settings.Section.Start, settings.Section.End, Path.GetFileName(settings.File));

                // TODO it may actually make more sense to split the saving methods for regular
                // save, paused video and diaporama. It will cause inevitable code duplication but better encapsulation and simpler algo.
                // When each save method has its own class and UI panel, it will be a better design.

                if(!settings.PausedVideo)
                {
                    // Take special care for slowmotion, the frame interval can not go down indefinitely.
                    // Use frame duplication when under 8fps.
                    settings.Duplication = (int)Math.Ceiling(settings.InputFrameInterval / 125.0);
                    settings.KeyframeDuplication = settings.Duplication;
                    settings.OutputFrameInterval = settings.InputFrameInterval / settings.Duplication;
                    if(settings.KeyframesOnly)
                        settings.EstimatedTotal = metadata.Count * settings.Duplication;
                    else
                        settings.EstimatedTotal = videoReader.EstimatedFrames * settings.Duplication;
                }
                else
                {
                    // For paused video, slow motion is not supported.
                    // InputFrameInterval will have been set to a multiple of the original frame interval.
                    settings.Duplication = 1;
                    settings.KeyframeDuplication = (int)(settings.InputFrameInterval / metadata.UserInterval);
                    settings.OutputFrameInterval = metadata.UserInterval;
                    
                    long regularFramesTotal = videoReader.EstimatedFrames - metadata.Count;
                    long keyframesTotal = metadata.Count * settings.KeyframeDuplication;
                    settings.EstimatedTotal = regularFramesTotal + keyframesTotal;
                }
                
                log.DebugFormat("interval:{0}, duplication:{1}, kf duplication:{2}", settings.OutputFrameInterval, settings.Duplication, settings.KeyframeDuplication);
                
                videoReader.BeforeFrameEnumeration();
                IEnumerable<Bitmap> images = EnumerateImages(settings);

                VideoFileWriter w = new VideoFileWriter();
                string formatString = FilenameHelper.GetFormatString(settings.File);
                saveResult = w.Save(settings, videoReader.Info, formatString, images, bgWorker);
                videoReader.AfterFrameEnumeration();
            }
            catch (Exception exp)
            {
                saveResult = SaveResult.UnknownError;
                log.Error("Unknown error while saving video.");
                log.Error(exp.StackTrace);
            }
            
            e.Result = 0;
        }
        
        /// <summary>
        /// Lazily enumerate the images that will end up in the final file.
        /// Return fully painted bitmaps ready for saving in the output.
        /// In case of early cancellation or error, the caller must dispose the bitmap to avoid a leak.
        /// </summary>
        private IEnumerable<Bitmap> EnumerateImages(SavingSettings settings)
        {
            Bitmap output = null;

            // Enumerates the raw frames from the video (at original video size).
            foreach (VideoFrame vf in videoReader.FrameEnumerator())
            {
                if (vf == null)
                {
                    log.Error("Working zone enumerator yield null.");
                    
                    if (output != null)
                        output.Dispose();

                    yield break;
                }

                if (output == null)
                    output = new Bitmap(vf.Image.Width, vf.Image.Height, vf.Image.PixelFormat);
                
                bool onKeyframe = settings.ImageRetriever(vf, output);
                bool savable = onKeyframe || !settings.KeyframesOnly;

                if (savable)
                {
                    int duplication = settings.PausedVideo && onKeyframe ? settings.KeyframeDuplication : settings.Duplication;
                    for (int i = 0; i < duplication; i++)
                        yield return output;
                }
            }

            if (output != null)
                output.Dispose();
        }

        private void bgWorkerSave_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // This method should be called back from the writer when a frame has been processed.
            // call snippet : bgWorker.ReportProgress(iCurrentValue, iMaximum);
            // Fix the int/long madness.
            int iMaximum = (int)(long)e.UserState;
            int iValue = (int)Math.Min((long)e.ProgressPercentage, iMaximum);
            formProgressBar.Update(iValue, iMaximum, true);
        }

        private void bgWorkerSave_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            formProgressBar.Close();
            formProgressBar.Dispose();
            
            if(saveResult != SaveResult.Success)
                ReportError(saveResult);
            else
                AfterSave();
        }
        #endregion
        
        private void ReportError(SaveResult saveResult)
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
        private void Cancel_Asked(object sender, EventArgs e)
        {
            // User cancelled from progress form.
            bgWorkerSave.CancelAsync();
            formProgressBar.Dispose();
        }
        #endregion
    }
}
