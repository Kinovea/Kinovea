#region License
/*
Copyright © Joan Charmant 2009.
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
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

using Kinovea.Base;
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
                    return null;
                else
                    return videoReader.Current.Image;
            }
        }
        public long SyncTimestampRelative
        {
            get { return syncTimestampRelative; }
            set { syncTimestampRelative = value; }
        }
        #endregion
        
        #region Members
        private VideoReader videoReader;
        private HistoryStack historyStack;
        private Metadata metadata;
        private long syncTimestampRelative;
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
            // Instanciate appropriate video reader class depending on extension.
            string extension = Path.GetExtension(filePath);
            videoReader = VideoTypeManager.GetVideoReader(extension);
            if(videoReader != null)
            {
                videoReader.Options = new VideoOptions(PreferencesManager.PlayerPreferences.AspectRatio, ImageRotation.Rotate0, PreferencesManager.PlayerPreferences.DeinterlaceByDefault);
                return videoReader.Open(filePath);
            }
            else
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
            }

            metadata.PostSetup(init);
            
            log.Debug("Setup metadata.");
        }

        public override void Draw(Graphics canvas)
        {
            // Draw the current image on canvas according to conf.
            // This is called back from screen paint method.
        }

        /// <summary>
        /// Main video saving pipeline. Saves either a video or the analysis data.
        /// </summary>
        public void Save(double playbackFrameInterval, double slowmotionPercentage, ImageRetriever imageRetriever)
        {
            // Let the user select what he wants to save exactly.
            formVideoExport fve = new formVideoExport(videoReader.FilePath, metadata, slowmotionPercentage);
            if (fve.Spawn() != DialogResult.OK)
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

            if(fve.SaveAnalysis)
            {
                MetadataSerializer serializer = new MetadataSerializer();
                serializer.SaveToFile(metadata, fve.Filename);
                metadata.AfterManualExport();
            }
            else
            {
                DoSave(fve.Filename,
                        fve.UseSlowMotion ? playbackFrameInterval : metadata.UserInterval,
                        fve.BlendDrawings,
                        false,
                        false,
                        imageRetriever);

                PreferencesManager.PlayerPreferences.VideoFormat = FilesystemHelper.GetVideoFormat(fve.Filename);
                PreferencesManager.Save();
            }

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
        
        public string TimeStampsToTimecode(long timestamps, TimeType type, TimecodeFormat format, bool isSynched)
        {
            // Input    : TimeStamp (might be a duration. If starting ts isn't 0, it should already be shifted.)
            // Output   : time in a specific format
            
            if (videoReader == null || !videoReader.Loaded)
                return "0";

            TimecodeFormat tcf = format == TimecodeFormat.Unknown ? PreferencesManager.PlayerPreferences.TimecodeFormat : format;

            long actualTimestamps = timestamps;
            switch (type)
            {
                case TimeType.Time:
                    actualTimestamps = isSynched ? timestamps - syncTimestampRelative : timestamps;
                    break;
                case TimeType.Duration:
                default:
                    actualTimestamps = timestamps;
                    break;
            }

            // timestamp to milliseconds. (Needed for most formats)
            double correctedTPS = videoReader.Info.FrameIntervalMilliseconds * videoReader.Info.AverageTimeStampsPerSeconds / metadata.UserInterval;
            double seconds = (double)actualTimestamps / correctedTPS;
            double milliseconds = 1000 * (seconds / metadata.HighSpeedFactor);
            bool showThousandth = (metadata.UserInterval / metadata.HighSpeedFactor) <= 10;

            int frames = 1;
            if (videoReader.Info.AverageTimeStampsPerFrame != 0)
                frames = (int)((double)actualTimestamps / videoReader.Info.AverageTimeStampsPerFrame) + 1;

            string frameString = String.Format("{0}", frames);
            string outputTimeCode;

            switch (tcf)
            {
                case TimecodeFormat.ClassicTime:
                    outputTimeCode = TimeHelper.MillisecondsToTimecode(milliseconds, showThousandth, true);
                    break;
                case TimecodeFormat.Frames:
                    outputTimeCode = frameString;
                    break;
                case TimecodeFormat.Milliseconds:
                    outputTimeCode = String.Format("{0}", (int)Math.Round(milliseconds));
                    break;
                case TimecodeFormat.Microseconds:
                    outputTimeCode = String.Format("{0}", (int)Math.Round(milliseconds * 1000));
                    break;
                case TimecodeFormat.TenThousandthOfHours:
                    // 1 Ten Thousandth of Hour = 360 ms.
                    double inTenThousandsOfAnHour = milliseconds / 360.0;
                    outputTimeCode = String.Format("{0}:{1:00}", (int)inTenThousandsOfAnHour, Math.Floor((inTenThousandsOfAnHour - (int)inTenThousandsOfAnHour) * 100));
                    break;
                case TimecodeFormat.HundredthOfMinutes:
                    // 1 Hundredth of minute = 600 ms.
                    double inHundredsOfAMinute = milliseconds / 600.0;
                    outputTimeCode = String.Format("{0}:{1:00}", (int)inHundredsOfAMinute, Math.Floor((inHundredsOfAMinute - (int)inHundredsOfAMinute) * 100));
                    break;
                case TimecodeFormat.TimeAndFrames:
                    String timeString = TimeHelper.MillisecondsToTimecode(milliseconds, showThousandth, true);
                    outputTimeCode = String.Format("{0} ({1})", timeString, frameString);
                    break;
                case TimecodeFormat.Normalized:
                    long duration = videoReader.Info.DurationTimeStamps - videoReader.Info.AverageTimeStampsPerFrame;
                    double totalFrames = (double)duration / videoReader.Info.AverageTimeStampsPerFrame;
                    int magnitude = (int)Math.Ceiling(Math.Log10(totalFrames));
                    string outputFormat = string.Format("{{0:0.{0}}}", new string('0', magnitude));
                    double normalized = (double)actualTimestamps / duration;
                    outputTimeCode = String.Format(outputFormat, normalized);
                    break;
                case TimecodeFormat.Timestamps:
                    outputTimeCode = String.Format("{0}", (int)actualTimestamps);
                    break;
                default:
                    outputTimeCode = TimeHelper.MillisecondsToTimecode(milliseconds, showThousandth, true);
                    break;
            }

            return outputTimeCode;
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
