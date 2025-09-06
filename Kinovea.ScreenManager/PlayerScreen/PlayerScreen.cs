#region Licence
/*
Copyright © Joan Charmant 2008.
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
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Windows.Forms;
using Kinovea.ScreenManager.Languages;
using Kinovea.Services;
using Kinovea.Video;

namespace Kinovea.ScreenManager
{
    public class PlayerScreen : AbstractScreen
    {
        #region Events
        public event EventHandler OpenVideoAsked;
        public event EventHandler<EventArgs<CaptureFolder>> OpenReplayWatcherAsked;
        public event EventHandler Loaded;
        public event EventHandler SpeedChanged;
        public event EventHandler HighSpeedFactorChanged;
        public event EventHandler TimeOriginChanged;
        public event EventHandler PlayStarted;
        public event EventHandler PauseAsked;
        public event EventHandler<EventArgs<bool>> SelectionChanged;
        public event EventHandler KVAImported;
        public event EventHandler<EventArgs<Bitmap>> ImageChanged;
        public event EventHandler FilterExited;
        public event EventHandler ResetAsked;
        #endregion

        #region Properties
        public override bool Full
        {
            get { return frameServer.Loaded; }
        }
        public override UserControl UI
        {
            get { return view; }	
        }
        public override Guid Id
        {
            get { return id; }
            set { id = value; }
        }
        public override Metadata Metadata
        {
            get 
            { 
                return frameServer.Loaded ? 
                    frameServer.Metadata : 
                    null;
            }
        }
        public override string FileName
        {
            get 
            { 
                return frameServer.Loaded ? 
                    Path.GetFileName(frameServer.VideoReader.FilePath) :
                    string.Empty;
            }
        }
        public override string FilePath
        {
            get { return frameServer.VideoReader.FilePath; }
        }
        public override string Status
        {
            get	
            {
                return string.IsNullOrEmpty(FileName) ? ScreenManagerLang.statusEmptyScreen : FileName;
            }
        }
        public override bool CapabilityDrawings
        {
            get { return true;}
        }

        #region Image options
        public override ImageAspectRatio AspectRatio
        {
            get { return frameServer.Metadata.ImageAspect; }
            set
            {
                bool uncached = frameServer.ChangeImageAspect(value);
                
                if (uncached && frameServer.VideoReader.DecodingMode == VideoDecodingMode.Caching)
                    view.UpdateWorkingZone(true);
                    
                view.ReferenceImageSizeChanged();
            }
        }
        public override ImageRotation ImageRotation
        {
            get { return frameServer.Metadata.ImageRotation; }
            set
            {
                bool uncached = frameServer.ChangeImageRotation(value);

                if (uncached && frameServer.VideoReader.DecodingMode == VideoDecodingMode.Caching)
                    view.UpdateWorkingZone(true);

                view.ReferenceImageSizeChanged();
            }
        }
        public override Demosaicing Demosaicing
        {
            get { return frameServer.Metadata.Demosaicing; }

            set
            {
                bool uncached = frameServer.ChangeDemosaicing(value);

                if (uncached && frameServer.VideoReader.DecodingMode == VideoDecodingMode.Caching)
                    view.UpdateWorkingZone(true);

                RefreshImage();
            }
        }
        public Guid StabilizationTrack
        {
            get { return frameServer.Metadata.StabilizationTrack; }

            set 
            {
                bool uncached = frameServer.SetStabilizationTrack(value);

                if (uncached && frameServer.VideoReader.DecodingMode == VideoDecodingMode.Caching)
                    view.UpdateWorkingZone(true);

                RefreshImage();
            }
        }
        public override bool Mirrored
        {
            get { return frameServer.Metadata.Mirrored; }
            set
            {
                frameServer.ChangeMirror(value);
                RefreshImage();
            }
        }
        public bool Deinterlaced
        {
            get { return frameServer.Metadata.Deinterlacing; }
            set
            {
                bool uncached = frameServer.ChangeDeinterlacing(value);

                if (uncached && frameServer.VideoReader.DecodingMode == VideoDecodingMode.Caching)
                    view.UpdateWorkingZone(true);

                RefreshImage();
            }
        }
        public Color BackgroundColor 
        {
            get { return frameServer.Metadata.BackgroundColor; }
            set
            {
                frameServer.ChangeBackgroundColor(value);
                RefreshImage();
            }
        }
        public VideoFilterType ActiveVideoFilterType
        {
            get { return frameServer.Metadata.ActiveVideoFilterType; }
        }
        #endregion

        public override bool CoordinateSystemVisible
        {
            get { return frameServer.Metadata.DrawingCoordinateSystem.Visible; }
            set { frameServer.Metadata.DrawingCoordinateSystem.Visible = value; }
        }

        public override bool TestGridVisible
        {
            get { return frameServer.Metadata.DrawingTestGrid.Visible; }
            set { frameServer.Metadata.DrawingTestGrid.Visible = value; }
        }

        public override HistoryStack HistoryStack
        {
            get { return historyStack; }
        }

        public FrameServerPlayer FrameServer
        {
            get { return frameServer; }
            set { frameServer = value; }
        }        
        public bool IsPlaying
        {
            get
            {
                if (!frameServer.Loaded)
                    return false;
                else
                    return view.IsCurrentlyPlaying;
            }
        }
        public bool IsWaitingForIdle
        {
            get { return view.IsWaitingForIdle; }
        }
        public bool IsSingleFrame
        {
            get
            {
                if (!frameServer.Loaded)
                    return false;
                else
                    return frameServer.VideoReader.IsSingleFrame;
            }	
        }
        public bool IsCaching
        {
            get
            {
                if (!frameServer.Loaded)
                    return false;
                else
                    return frameServer.VideoReader.DecodingMode == VideoDecodingMode.Caching;
            }
        }

        public bool IsReplayWatcher
        {
            get 
            {
                if (!frameServer.Loaded)
                {
                    return false;
                }
                else
                {
                    if (replayWatcher.IsEnabled && !view.ScreenDescriptor.IsReplayWatcher)
                    {
                        log.ErrorFormat("Replay watcher is active in non-watcher screen descriptor.");
                    }

                    return replayWatcher.IsEnabled;
                }
            }
        }

        /// <summary>
        /// Returns the current frame time relative to selection start.
        /// The value is a physical time in microseconds, taking high speed factor into account.
        /// </summary>
        public long LocalTime
        {
            get { return view.LocalTime; }
        }

        /// <summary>
        /// Returns the last valid time relative to selection start.
        /// The value is a physical time in microseconds, taking high speed factor into account.
        /// </summary>
        public long LocalLastTime
        {
            get
            {
                return view.LocalLastTime;
            }
        }

        /// <summary>
        /// Returns the average time of one frame.
        /// The value is a physical time in microseconds, taking high speed factor into account.
        /// </summary>
        public long LocalFrameTime
        {
            get
            {
                return view.LocalFrameTime;
            }
        }

        /// <summary>
        /// Returns the time origin relative to selection start.
        /// The value is a physical time in microseconds, taking high speed factor into account.
        /// </summary>
        public long LocalTimeOriginPhysical
        {
            get
            {
                return view.LocalTimeOriginPhysical;
            }

            set
            {
                long absoluteTimestamp = RelativeRealTimeToAbsoluteTimestamp(value);
                frameServer.Metadata.TimeOrigin = absoluteTimestamp;
                view.TimeOriginUpdatedFromSync();
            }
        }

        /// <summary>
        /// Returns the interval between frames in milliseconds, taking slow motion slider into account.
        /// This is suitable for a playback timer or metadata in saved file.
        /// </summary>
        public double PlaybackFrameInterval
        {
            get 
            { 
                if (frameServer.Loaded && frameServer.VideoReader.Info.FrameIntervalMilliseconds > 0)
                    return view.PlaybackFrameInterval;
                else
                    return 40;
            }
        }
        public double RealtimePercentage
        {
            get { return view.RealtimePercentage; }
            set { view.RealtimePercentage = value;}
        }

        /// <summary>
        /// Synched is true as soon as we have two player screens with videos in them.
        /// </summary>
        public bool Synched
        {
            get { return synched; }
            set 
            { 
                view.Synched = value;
                synched = value;
            }
        }
        public long Position
        {
            // Used to feed SyncPosition. 
            get 
            {
                if (frameServer.VideoReader.Current == null)
                    return 0;

                return frameServer.VideoReader.Current.Timestamp - frameServer.VideoReader.Info.FirstTimeStamp; 
            }
        }
        public bool SyncMerge
        {
            set 
            {
                view.SyncMerge = value;
                RefreshImage();
            }
        }
        public bool DualSaveInProgress
        {
            set { view.DualSaveInProgress = value; }
        }
        #endregion

        #region members
        private Guid id = Guid.NewGuid();
        public PlayerScreenUserInterface view;
        private DrawingToolbarPresenter drawingToolbarPresenter = new DrawingToolbarPresenter();
        private HistoryStack historyStack; 
        private FrameServerPlayer frameServer;
        private bool synched;
        private int index;
        private ReplayWatcher replayWatcher;
        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public PlayerScreen()
        {
            log.Debug("Constructing a PlayerScreen.");
            historyStack = new HistoryStack();
            frameServer = new FrameServerPlayer(historyStack);
            replayWatcher = new ReplayWatcher(this);
            view = new PlayerScreenUserInterface(frameServer, drawingToolbarPresenter);

            BindCommands();
        }
        #endregion

        private void BindCommands()
        {
            // Provides implementation for behaviors triggered from the view, either as commands or as event handlers.

            // Forwarded to screen manager via AbstractScreen.
            view.SetAsActiveScreen += (s, e) => RaiseActivated(e);
            view.DualCommandReceived += (s, e) => RaiseDualCommandReceived(e);
            view.LoadAnnotationsAsked += (s, e) => RaiseLoadAnnotationsAsked(e);
            view.CloseAsked += (s, e) => RaiseCloseAsked(e);

            // Forwarded to screen manager specifically from PlayerScreen.
            view.OpenVideoAsked += (s, e) => OpenVideoAsked?.Invoke(this, e);
            view.OpenReplayWatcherAsked += (s, e) => OpenReplayWatcherAsked?.Invoke(this, e);
            view.Loaded += (s, e) => Loaded?.Invoke(this, e);
            view.SelectionChanged += (s, e) => SelectionChanged?.Invoke(this, e);
            view.FilterExited += (s, e) => FilterExited?.Invoke(this, e);
            view.ResetAsked += (s, e) => ResetAsked?.Invoke(this, e);

            // Forwarded to dual player
            view.ImageChanged += (s, e) => ImageChanged(this, e);

            // Implemented at AbstractScreen level.
            view.SaveAnnotationsAsked += (s, e) => SaveAnnotations();
            view.SaveAnnotationsAsAsked += (s, e) => SaveAnnotationsAs();
            view.SaveDefaultPlayerAnnotationsAsked += (s, e) => SaveDefaultAnnotations(true);
            view.SaveDefaultCaptureAnnotationsAsked += (s, e) => SaveDefaultAnnotations(false);
            view.UnloadAnnotationsAsked += (s, e) => UnloadAnnotations();
            view.ReloadDefaultPlayerAnnotationsAsked += (s, e) => ReloadDefaultAnnotations(true);

            // Implemented locally.
            view.StopWatcherAsked += View_StopWatcherAsked;
            view.StartWatcherAsked += View_StartWatcherAsked;
            view.SpeedChanged += View_SpeedChanged;
            view.TimeOriginChanged += View_TimeOriginChanged;
            view.KVAImported += View_KVAImported;
            view.PlayStarted += View_PlayStarted;
            view.PauseAsked += View_PauseAsked;

            // Requests for metadata modification coming from the view.
            // These should push a memento on the history stack.
            view.KeyframeAdding += View_KeyframeAdding;
            view.KeyframeDeleting += View_KeyframeDeleting;
            view.DrawingAdding += View_DrawingAdding;
            view.DrawingDeleting += View_DrawingDeleting;
            view.MultiDrawingItemAdding += View_MultiDrawingItemAdding;
            view.MultiDrawingItemDeleting += View_MultiDrawingItemDeleting;

            // Export requests coming from the view.
            view.ExportImageAsked += (s, e) => ExportImages(ImageExportFormat.Image);
            view.ExportImageSequenceAsked += (s, e) => ExportImages(ImageExportFormat.ImageSequence);
            view.ExportKeyImagesAsked += (s, e) => ExportImages(ImageExportFormat.KeyImages);
            view.ExportVideoAsked += (s, e) => ExportVideo(VideoExportFormat.Video);
            view.ExportVideoSlideshowAsked+= (s, e) => ExportVideo(VideoExportFormat.VideoSlideShow);
            view.ExportVideoWithPausesAsked += (s, e) => ExportVideo(VideoExportFormat.VideoWithPauses);

            // Just for the magnifier. Remove as soon as possible when the adding of the magnifier is handled in Metadata.
            view.TrackableDrawingAdded += (s, e) => AddTrackableDrawing(e.TrackableDrawing);
            
            // Commands
            view.ToggleTrackingCommand = new ToggleCommand(ToggleTracking, IsTracking);
            view.TrackDrawingsCommand = new RelayCommand<VideoFrame>(TrackDrawings);

            frameServer.Metadata.TrackableDrawingAdded += (s, e) => AddTrackableDrawing(e.Value);
            frameServer.Metadata.CameraCalibrationAsked += (s, e) => ShowCameraCalibration();
        }

        #region General events handlers
        private void View_StopWatcherAsked(object sender, EventArgs e)
        {
            if (!replayWatcher.IsEnabled || !frameServer.Loaded)
                return;

            // Stop watching the folder but stay on the same file.
            // Switch the screen descriptor from a replay watcher to the current file.
            ScreenDescriptorPlayback sdp = new ScreenDescriptorPlayback();
            sdp.IsReplayWatcher = false;
            sdp.FullPath = frameServer.VideoReader.FilePath;
            sdp.Autoplay = false;
            sdp.Stretch = false;
            sdp.SpeedPercentage = view.SpeedPercentage;
            string currentFile = frameServer.VideoReader.FilePath;

            view.ScreenDescriptor = sdp;
            StopReplayWatcher();
        }

        private void View_StartWatcherAsked(object sender, EventArgs<CaptureFolder> e)
        {
            // Start watching a capture folder or the parent folder of the current video,
            // or a known capture folder.
            // This is coming from the infobar icon.
            // We do NOT switch to the latest video immediately,
            // but stop playing to indicate that we're ready for the video to arrive.

            // FIXME: deduplicate this with Player_OpenReplayWatcherAsked.
            // That other one should only be for opening with the folder picker.

            if (!frameServer.Loaded)
                return;

            // Prepare the screen descriptor. All replay watchers must have a valid screen descriptor.
            ScreenDescriptorPlayback sdp = new ScreenDescriptorPlayback();
            sdp.IsReplayWatcher = true;
            sdp.Autoplay = true;
            sdp.Stretch = false;
            sdp.SpeedPercentage = view.SpeedPercentage;
            string currentFile = null;

            if (e.Value == null)
            {
                log.DebugFormat("Start watching the parent folder of the current file.");

                // Stay on the current file. We'll only switch when the next video arrives.
                currentFile = frameServer.VideoReader.FilePath;
                if (string.IsNullOrEmpty(currentFile))
                    return;

                // Add the parent folder to the list of capture folders and make the screen descriptor point to it.
                var cf = PreferencesManager.CapturePreferences.AddCaptureFolder(Path.GetDirectoryName(currentFile));
                sdp.FullPath = cf.Id.ToString();

                // TODO: trigger preference update.
            }
            else
            {
                log.DebugFormat("Start watching capture folder {0}.", e.Value.FriendlyName);
                sdp.FullPath = e.Value.Id.ToString();
            }

            view.ScreenDescriptor = sdp;
            view.StopPlaying();
            StartReplayWatcher(currentFile);
        }
        
        public void View_SpeedChanged(object sender, EventArgs e)
        {
            if (SpeedChanged != null)
                SpeedChanged(this, EventArgs.Empty);
        }

        public void View_TimeOriginChanged(object sender, EventArgs e)
        {
            if (TimeOriginChanged != null)
                TimeOriginChanged(this, EventArgs.Empty);
        }

        public void View_KVAImported(object sender, EventArgs e)
        {
            if (KVAImported != null)
                KVAImported(this, EventArgs.Empty);

            if (HighSpeedFactorChanged != null)
                HighSpeedFactorChanged(this, EventArgs.Empty);
        }

        public void View_PlayStarted(object sender, EventArgs e)
        {
            if (PlayStarted != null)
                PlayStarted(this, EventArgs.Empty);
        }

        public void View_PauseAsked(object sender, EventArgs e)
        {
            if (PauseAsked != null)
                PauseAsked(this, EventArgs.Empty);
        }
        #endregion

        #region Requests for Metadata modification coming from the view
        private void View_KeyframeAdding(object sender, KeyframeAddEventArgs e)
        {
            if (frameServer.CurrentImage == null)
                return;

            Keyframe keyframe = new Keyframe(e.Time, e.Name, e.Color, frameServer.Metadata);

            HistoryMementoAddKeyframe memento = new HistoryMementoAddKeyframe(frameServer.Metadata, keyframe.Id);
            frameServer.Metadata.AddKeyframe(keyframe);
            historyStack.PushNewCommand(memento);
        }

        private void View_KeyframeDeleting(object sender, KeyframeEventArgs e)
        {
            HistoryMemento memento = new HistoryMementoDeleteKeyframe(frameServer.Metadata, e.KeyframeId);
            frameServer.Metadata.DeleteKeyframe(e.KeyframeId);
            historyStack.PushNewCommand(memento);
        }

        private void View_DrawingAdding(object sender, DrawingEventArgs e)
        {
            AddDrawingWithMemento(e.ManagerId, e.Drawing);
        }
        
        private void View_DrawingDeleting(object sender, DrawingEventArgs e)
        {
            HistoryMemento memento = new HistoryMementoDeleteDrawing(frameServer.Metadata, e.ManagerId, e.Drawing.Id, e.Drawing.Name);
            frameServer.Metadata.DeleteDrawing(e.ManagerId, e.Drawing.Id);
            historyStack.PushNewCommand(memento);
        }

        private void View_MultiDrawingItemAdding(object sender, MultiDrawingItemEventArgs e)
        {
            HistoryMemento memento = new HistoryMementoAddMultiDrawingItem(frameServer.Metadata, e.Manager, e.Item.Id);
            frameServer.Metadata.AddMultidrawingItem(e.Manager, e.Item);
            historyStack.PushNewCommand(memento);
        }
        
        private void View_MultiDrawingItemDeleting(object sender, MultiDrawingItemEventArgs e)
        {
            HistoryMemento memento = new HistoryMementoDeleteMultiDrawingItem(frameServer.Metadata, e.Manager, e.Item.Id, SerializationFilter.KVA);
            frameServer.Metadata.DeleteMultiDrawingItem(e.Manager, e.Item.Id);
            historyStack.PushNewCommand(memento);
        }
        #endregion

        #region Export requests from the view
        private void ExportImages(ImageExportFormat format)
        {
            ImageExporter exporter = new ImageExporter();
            exporter.Export(format, this, null);
        }
        private void ExportVideo(VideoExportFormat format)
        {
            VideoExporter exporter = new VideoExporter();
            exporter.Export(format, this, null, null);
        }
        #endregion


        #region AbstractScreen Implementation
        public override void DisplayAsActiveScreen(bool _bActive)
        {
            view.DisplayAsActiveScreen(_bActive);
        }
        public override void BeforeClose()
        {
            // Called by the ScreenManager when this screen is about to be closed.
            // Note: We shouldn't call ResetToEmptyState here because we will want
            // the close screen routine to detect if there is something left in the 
            // metadata and alerts the user.
            if(frameServer.Loaded)
                view.StopPlaying();
        }
        public override void AfterClose()
        {
            frameServer.Metadata.Close();
            replayWatcher.Stop();
            
            if(!frameServer.Loaded)
                return;
            
            frameServer.VideoReader.Close();
            view.ResetToEmptyState();
            view.AfterClose();

            drawingToolbarPresenter.Dispose();
        }

        /// <summary>
        /// Called after a change in preferences.
        /// </summary>
        public override void RefreshUICulture()
        {
            // After preferences change, the actual folder pointed by a replay watcher may have changed.
            // We need to restart the watcher on the new folder.
            if (replayWatcher.IsEnabled && view.ScreenDescriptor.IsReplayWatcher)
            {
                CaptureFolder cf = FilesystemHelper.GetCaptureFolder(view.ScreenDescriptor.FullPath);
                if (cf != null)
                {
                    var dateContext = DynamicPathResolver.BuildDateContext();
                    string targetFolder = DynamicPathResolver.Resolve(cf.Path, dateContext);
                    if (replayWatcher.WatchedFolder != targetFolder)
                    {
                        if (!FilesystemHelper.IsValidPath(targetFolder))
                        {
                            log.ErrorFormat("Replay watcher path is invalid \"{0}\"", targetFolder);       
                        }
                        else
                        {
                            if (!Directory.Exists(targetFolder))
                                Directory.CreateDirectory(targetFolder);

                            log.DebugFormat("Replay watcher target path changed from preferences.");
                            log.DebugFormat("Switching watcher from watching \"{0}\" to watching \"{1}\".", Path.GetFileName(replayWatcher.WatchedFolder), Path.GetFileName(targetFolder));
                            
                            view.StopPlaying();
                            replayWatcher.Stop();
                            StartReplayWatcher(null);

                            // Toast notification with the new path for confirmation.
                            view.ToastMessage(replayWatcher.WatchedFolder, 2000);
                        }
                    }
                }
            }

            view.RefreshUICulture();
            drawingToolbarPresenter.RefreshUICulture();
        }
        public override void RefreshImage()
        {
            view.RefreshImage();
        }
        public override void AddImageDrawing(string filename, bool isSvg)
        {
            if (!File.Exists(filename))
                return;

            view.BeforeAddImageDrawing();
            
            if (frameServer.Metadata.HitKeyframe == null)
                return;

            AbstractDrawing drawing = null;
            if (isSvg)
                drawing = new DrawingSVG(frameServer.VideoReader.Current.Timestamp, frameServer.VideoReader.Info.AverageTimeStampsPerFrame, filename);
            else
                drawing = new DrawingBitmap(frameServer.VideoReader.Current.Timestamp, frameServer.VideoReader.Info.AverageTimeStampsPerFrame, filename);

            if (drawing != null)
                AddDrawingWithMemento(frameServer.Metadata.HitKeyframe.Id, drawing);
        }
        public override void AddImageDrawing(Bitmap bmp)
        {
            view.BeforeAddImageDrawing();
            AbstractDrawing drawing = new DrawingBitmap(frameServer.VideoReader.Current.Timestamp, frameServer.VideoReader.Info.AverageTimeStampsPerFrame, bmp);
            frameServer.Metadata.AddDrawing(frameServer.Metadata.HitKeyframe.Id, drawing);
        }
        public override void FullScreen(bool _bFullScreen)
        {
            view.FullScreen(_bFullScreen);
        }

        public override void Identify(int index)
        {
            this.index = index;
        }
        

        public override void ExecuteScreenCommand(int cmd)
        {
            view.ExecuteScreenCommand(cmd);
        }

        /// <summary>
        /// Get a screen descriptor with the current state.
        /// This supports the continue where you left off mode.
        /// </summary>
        public override IScreenDescriptor GetScreenDescriptor()
        {
            if (!frameServer.Loaded || view.ScreenDescriptor == null)
                return new ScreenDescriptorPlayback();

            // Just-in-time update the screen descriptor with latest state and return it.
            // FIXME: why is the view screen descriptor not up to date at this point ?
            var sd = view.ScreenDescriptor;
            sd.Stretch = view.ImageFill;
            sd.SpeedPercentage = view.SpeedPercentage;
            if (!sd.IsReplayWatcher)
            {
               sd.FullPath = frameServer.VideoReader.FilePath;
            }

            ScreenDescriptorPlayback sdp = new ScreenDescriptorPlayback();
            sdp.FullPath = sd.FullPath;
            sdp.Autoplay = sd.Autoplay;
            sdp.SpeedPercentage = sd.SpeedPercentage;
            sdp.Stretch = sd.Stretch;
            sdp.IsReplayWatcher = sd.IsReplayWatcher;
            //sdp.RecoveryLastSave = sdp.RecoveryLastSave;
            return sdp;
        }


        public override void LoadKVA(string path)
        {
            view.StopPlaying();

            MetadataSerializer s = new MetadataSerializer();
            s.Load(frameServer.Metadata, path, true);
        }
        #endregion
        
        #region Other public methods called from the ScreenManager
        public void EnsurePlaying()
        {
            view.EnsurePlaying();
        }
        
        public void StopPlaying()
        {
            view.StopPlaying();
        }
        
        public void GotoNextFrame(bool allowUIUpdate)
        {
            view.ForceCurrentFrame(-1, allowUIUpdate);
        }
        
        public void GotoTime(long microseconds, bool allowUIUpdate)
        {
            long timestamp = RelativeRealTimeToAbsoluteTimestamp(microseconds);
            view.ForcePosition(timestamp, allowUIUpdate);
        }
        
        public void GotoPrevKeyframe()
        {
            view.GotoPreviousKeyframe();
        }
        
        public void GotoNextKeyframe()
        {
            view.GotoNextKeyframe();
        }
        
        public void AddKeyframe()
        {
            view.AddKeyframe();
        }
        
        /// <summary>
        /// A video filter was activated from the main menu for this screen.
        /// </summary>
        public void ActivateVideoFilter(VideoFilterType type)
        {
            if (!IsCaching)
                return;
            
            frameServer.ActivateVideoFilter(type);
            view.ActivateVideoFilter();
        }
        
        /// <summary>
        /// The active video filter was deactivated from the main menu or from closing the filter window.
        /// This does not reset its internal data.
        /// </summary>
        public void DeactivateVideoFilter()
        {
            frameServer.DeactivateVideoFilter();
            view.DeactivateVideoFilter();
        }
        
        public void SetSyncMergeImage(Bitmap _SyncMergeImage, bool _bUpdateUI)
        {
            view.SetSyncMergeImage(_SyncMergeImage, _bUpdateUI);
        }
        
        public void ConfigureTimebase()
        {
            if (!frameServer.Loaded)
                return;

            double captureInterval = frameServer.Metadata.BaselineFrameInterval / frameServer.Metadata.HighSpeedFactor;
            formConfigureSpeed fcs = new formConfigureSpeed(frameServer.VideoReader.Info.FrameIntervalMilliseconds, frameServer.Metadata.BaselineFrameInterval, captureInterval);
            fcs.StartPosition = FormStartPosition.CenterScreen;

            if (fcs.ShowDialog() != DialogResult.OK)
            {
                fcs.Dispose();
                return;
            }

            frameServer.Metadata.BaselineFrameInterval = fcs.UserInterval;
            frameServer.Metadata.HighSpeedFactor = fcs.UserInterval / fcs.CaptureInterval;
            fcs.Dispose();

            log.DebugFormat("Time configuration. File interval:{0:0.###}ms, User interval:{1:0.###}ms, Capture interval:{2:0.###}ms.",
                frameServer.VideoReader.Info.FrameIntervalMilliseconds, frameServer.Metadata.BaselineFrameInterval, fcs.CaptureInterval);

            if (HighSpeedFactorChanged != null)
                HighSpeedFactorChanged(this, EventArgs.Empty);

            frameServer.Metadata.CalibrationHelper.CaptureFramesPerSecond = 1000 * frameServer.Metadata.HighSpeedFactor / frameServer.Metadata.BaselineFrameInterval;
            frameServer.Metadata.UpdateTrajectoriesForKeyframes();

            view.UpdateTimebase();
            view.UpdateTimeLabels();
            view.RefreshImage();
        }

        public void PaintFlushedImage(Bitmap output)
        {
            view.PaintFlushedImage(output);
        }

        public void ShowCoordinateSystem()
        {
            frameServer.Metadata.ShowCoordinateSystem();
            view.RefreshImage();
        }

        public void ShowCameraCalibration()
        {
            List<List<PointF>> points = frameServer.Metadata.GetCameraCalibrationPoints();
            
            FormCalibrateDistortion fcd = new FormCalibrateDistortion(frameServer.CurrentImage, points, frameServer.Metadata.CalibrationHelper);
            FormsHelper.Locate(fcd);
            fcd.ShowDialog();
            fcd.Dispose();
            
            view.RefreshImage();
        }

        public void ShowCalibrationValidation(PlayerScreen otherScreen)
        {
            var otherMetadata = otherScreen?.frameServer.Metadata;

            FormCalibrationValidation fcv = new FormCalibrationValidation(frameServer.Metadata, otherMetadata, view.DoInvalidate);
            FormsHelper.Locate(fcv);
            fcv.ShowDialog();
            fcv.Dispose();

            view.RefreshImage();
        }

        public void ShowTrajectoryAnalysis()
        {
            FormMultiTrajectoryAnalysis f = new FormMultiTrajectoryAnalysis(frameServer.Metadata);
            FormsHelper.Locate(f);
            f.ShowDialog();
            f.Dispose();
        }

        public void ShowScatterDiagram()
        {
            FormPointsAnalysis fpa = new FormPointsAnalysis(frameServer.Metadata);
            FormsHelper.Locate(fpa);
            fpa.ShowDialog();
            fpa.Dispose();
        }

        public void ShowAngularAnalysis()
        {
            FormAngularAnalysis f = new FormAngularAnalysis(frameServer.Metadata);
            FormsHelper.Locate(f);
            f.ShowDialog();
            f.Dispose();
        }

        public void ShowAngleAngleAnalysis()
        {
            FormAngleAngleAnalysis f = new FormAngleAngleAnalysis(frameServer.Metadata);
            FormsHelper.Locate(f);
            f.ShowDialog();
            f.Dispose();
        }
        #endregion

        public void AfterLoad()
        {
            RaiseActivated(EventArgs.Empty);

            if (view.ScreenDescriptor != null && view.ScreenDescriptor.IsReplayWatcher)
            {
                // Bring the whole window back if it was minimized or behind other windows.
                NotificationCenter.RaiseWakeUpAsked(this);
            }

            // Make sure the watcher is watching the right folder.
            if (replayWatcher.IsEnabled)
            {
                // Not the first time we come here.
                if (view.ScreenDescriptor != null && view.ScreenDescriptor.IsReplayWatcher)
                {
                    // We come here when we open a new watcher into an existing one,
                    // or when a new video is created in the watched folder,
                    // or loading a video from the same or a different folder.
                    // The screen descriptor has already been updated.

                    string targetFolder = "";
                    CaptureFolder cf = FilesystemHelper.GetCaptureFolder(view.ScreenDescriptor.FullPath);
                    if (cf != null)
                    {
                        var context = DynamicPathResolver.BuildDateContext();
                        targetFolder = DynamicPathResolver.Resolve(cf.Path, context);
                    }
                    else
                    {
                        targetFolder = Path.GetDirectoryName(view.ScreenDescriptor.FullPath);
                    }
                    
                    if (!FilesystemHelper.IsValidPath(targetFolder))
                    {
                        log.ErrorFormat("Replay watcher set to invalid folder \"{0}\"", targetFolder);
                        StopReplayWatcher();
                        return;
                    }

                    if (replayWatcher.WatchedFolder != targetFolder)
                    {
                        // This happens when we are opening a watcher in an existing watcher.
                        // The screen descriptor has been updated to the new folder already.
                        log.DebugFormat("Switch watcher from watching \"{0}\" to watching \"{1}\".", replayWatcher.WatchedFolder, targetFolder);
                        
                        if (!Directory.Exists(targetFolder))
                            Directory.CreateDirectory(targetFolder);

                        StartReplayWatcher(FilePath);
                    }
                    else
                    {
                        // Opened a watcher on the same folder, or a loading a video coming from the same folder.
                        // Keep watching the original folder and the descriptor is still pointing to it.
                        log.DebugFormat("Continue watching folder: \"{0}\"", targetFolder);
                    }
                }
                else
                {
                    // We should never come here anymore.
                    // If the screen descriptor has been switched back to a file the watcher should have been stopped already.
                    // This happens in View_StopWatcherAsked.
                    log.DebugFormat("Continue watching folder: \"{0}\"", replayWatcher.WatchedFolder);
                }
            }
            else if (view.ScreenDescriptor != null && view.ScreenDescriptor.IsReplayWatcher)
            {
                // First time we come here, start watching.
                string targetFolder = "";
                var cf = FilesystemHelper.GetCaptureFolder(view.ScreenDescriptor.FullPath);
                if (cf != null)
                {
                    var context = DynamicPathResolver.BuildDateContext();
                    targetFolder = DynamicPathResolver.Resolve(cf.Path, context);
                }
                else
                {
                    targetFolder = Path.GetDirectoryName(view.ScreenDescriptor.FullPath);
                }

                if (!FilesystemHelper.IsValidPath(targetFolder))
                {
                    log.ErrorFormat("Replay watcher set to invalid folder \"{0}\"", targetFolder);
                    return;
                }

                log.DebugFormat("Starting replay watcher on: \"{0}\"", targetFolder);
                StartReplayWatcher(FilePath);
            }
            else
            {
                // Not a watcher and not asked to watch anything, nothing to do.
            }
        }

        /// <summary>
        /// Start a replay watcher on the path specified in the screen descriptor.
        /// filePath is the target video file path to load, it may be null.
        /// </summary>
        public void StartReplayWatcher(string filePath)
        {
            replayWatcher.Start(view.ScreenDescriptor, filePath);
            view.UpdateReplayWatcher(replayWatcher.WatchedFolder);
        }

        /// <summary>
        /// Stop watching the folder and switch back the infobar to a regular player.
        /// </summary>
        public void StopReplayWatcher()
        {
            replayWatcher.Stop();
            view.UpdateReplayWatcher(null);
        }

        /// <summary>
        /// Convert from real time in microseconds relative to working zone start into absolute timestamps.
        /// </summary>
        private long RelativeRealTimeToAbsoluteTimestamp(long time)
        {
            double realtimeSeconds = (double)time / 1000000;
            double videoSeconds = realtimeSeconds * frameServer.Metadata.HighSpeedFactor;

            double correctedTPS = frameServer.VideoReader.Info.FrameIntervalMilliseconds * frameServer.VideoReader.Info.AverageTimeStampsPerSeconds / frameServer.Metadata.BaselineFrameInterval;
            double timestamp = videoSeconds * correctedTPS;
            timestamp = Math.Round(timestamp);

            long relativeTimestamp = (long)timestamp;
            long absoluteTimestamp = relativeTimestamp + frameServer.VideoReader.WorkingZone.Start;
            
            return absoluteTimestamp;
        }

        private void AddDrawingWithMemento(Guid managerId, AbstractDrawing drawing)
        {
            // Temporary function.
            // Once the player screen ui uses the viewport, this event handler should be removed.
            // The code here should also be in the metadata manipulator until this function is removed.
            HistoryMementoAddDrawing memento = new HistoryMementoAddDrawing(frameServer.Metadata, managerId, drawing.Id, drawing.ToolDisplayName);
            frameServer.Metadata.AddDrawing(managerId, drawing);
            memento.UpdateCommandName(drawing.Name);
            historyStack.PushNewCommand(memento);
        }
        private void AddTrackableDrawing(ITrackable trackableDrawing)
        {
            frameServer.Metadata.TrackabilityManager.Add(trackableDrawing, frameServer.VideoReader.Current);
        }

        private void ToggleTracking(object parameter)
        {
            ITrackable trackableDrawing = ConvertToTrackable(parameter);
            if(trackableDrawing == null)
                return;
            
            frameServer.Metadata.TrackabilityManager.ToggleTracking(trackableDrawing);
        }
        private bool IsTracking(object parameter)
        {
            ITrackable trackableDrawing = ConvertToTrackable(parameter);
            if(trackableDrawing == null)
                return false;
            
            return frameServer.Metadata.TrackabilityManager.IsTracking(trackableDrawing);
        }
        
        private ITrackable ConvertToTrackable(object parameter)
        {
            ITrackable trackableDrawing = null;
            
            if(parameter is AbstractMultiDrawing)
            {
                AbstractMultiDrawing manager = parameter as AbstractMultiDrawing;
                if(manager != null)
                    trackableDrawing = manager.SelectedItem as ITrackable;    
            }
            else
            {
                trackableDrawing = parameter as ITrackable;
            }
            
            return trackableDrawing;
        }
        
        /// <summary>
        /// Track trackable drawings in the current frame.
        /// This updates the trackable points coordinates to the current frame.
        /// </summary>
        private void TrackDrawings(VideoFrame frameToUse)
        {
            VideoFrame frame = frameToUse ?? frameServer.VideoReader.Current;
            if (frame.Image == null)
                return;

            frameServer.Metadata.TrackabilityManager.Track(frame);
        }
    }
}
