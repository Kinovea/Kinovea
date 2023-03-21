#region License
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
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Xsl;
using System.Linq;
using Kinovea.Services;
using Kinovea.Video;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Main class representing the annotations added to the video.
    /// This contains the drawings, calibration information, comments, tracking data, etc.
    /// This is what gets serialized to KVA xml.
    /// This also contains video import options like image rotation, demosaicing, deinterlacing, 
    /// and rendering options like mirroring.
    ///
    /// We have essentially 3 types of drawings:
    /// - attached to a keyframe (ex: a line or angle object),
    /// - detached (ex: a stopwatch or track object),
    /// - singletons (ex: the coordinate system or the number sequence).
    /// </summary>
    public class Metadata
    {
        #region Events and commands
        public EventHandler KVAImported;
        public EventHandler<KeyframeEventArgs> KeyframeAdded;
        public EventHandler KeyframeDeleted;
        public EventHandler<DrawingEventArgs> DrawingAdded;
        public EventHandler<DrawingEventArgs> DrawingModified; 
        public EventHandler DrawingDeleted;
        public EventHandler<MultiDrawingItemEventArgs> MultiDrawingItemAdded;
        public EventHandler MultiDrawingItemDeleted;
        public EventHandler CameraCalibrationAsked;
        public EventHandler VideoFilterModified;

        public RelayCommand<ITrackable> AddTrackableDrawingCommand { get; set; }
        public RelayCommand<ITrackable> DeleteTrackableDrawingCommand { get; set; }
        #endregion
        
        #region Properties
        /// <summary>
        /// Helper function to generate timecodes.
        /// </summary>
        public TimeCodeBuilder TimeCodeBuilder
        {
            get { return timecodeBuilder; }
        }

        /// <summary>
        /// Stack of recent commands for undo/redo mechanics.
        /// </summary>
        public HistoryStack HistoryStack
        {
            get { return historyStack; }
        }

        /// <summary>
        /// True when the object contains unsaved changes.
        /// </summary>
        public bool IsDirty
        {
            get
            {
                int currentHash = GetContentHash();
                bool dirty = currentHash != referenceHash;
                log.DebugFormat("Dirty:{0}, reference hash:{1}, current:{2}.", dirty.ToString(), referenceHash, currentHash);
                return dirty;
            }
        }
        public bool KVAImporting
        {
            get { return kvaImporting; }
        }

        /// <summary>
        /// The path to the KVA file this metadata was imported from or last saved.
        /// Returns an empty string if the file is a default KVA, to avoid overwriting them.
        /// </summary>
        public string LastKVAPath
        {
            get 
            {
                if (string.IsNullOrEmpty(lastKVAPath) ||
                    lastKVAPath == PreferencesManager.PlayerPreferences.PlaybackKVA ||
                    lastKVAPath == PreferencesManager.CapturePreferences.CaptureKVA ||
                    lastKVAPath.StartsWith(Software.TempDirectory))
                {
                    return "";
                }
                    
                return lastKVAPath;
            } 
            set
            {
                lastKVAPath = value;
            }
        }
        public string GlobalTitle
        {
            get { return globalTitle; }
            set { globalTitle = value; }
        }

        /// <summary>
        /// The reference image size used by drawings for their coordinates.
        /// This may be different than the size on disk, as this takes aspect ratio and rotation into account.
        /// It may also be different from the image coming out of the video reader, as that may be decoded at 
        /// a scale matching the viewport for performance reasons.
        /// </summary>
        public Size ImageSize
        {
            get { return imageSize; }
            set
            {
                imageSize = value;
                trackabilityManager.Initialize(imageSize);
                calibrationHelper.Initialize(imageSize, GetCalibrationOrigin, GetCalibrationQuad, HasTrackingData);
            }
        }

        /// <summary>
        /// A helper used to transform from image space coordinates to viewport coordinates.
        /// </summary>
        public ImageTransform ImageTransform
        {
            get { return imageTransform; }
        }

        /// <summary>
        /// The import mode for aspect ratio. 
        /// This tells whether we are using the original aspect ratio or 
        /// forcing the images to conform to a specific one.
        /// </summary>
        public ImageAspectRatio ImageAspect { get; set; }
        
        /// <summary>
        /// The import mode for image rotation.
        /// </summary>
        public ImageRotation ImageRotation { get; set; }
        
        /// <summary>
        /// Whether the image should be mirrored.
        /// This has almost no impact on drawings which still keep their coordinates in the non-mirrored space.
        /// Except for the magnifier which needs to mirror the source area and picture-in-picture.
        /// </summary>
        public bool Mirrored { get; set; }
        
        /// <summary>
        /// Import mode for demosaicing.
        /// </summary>
        public Demosaicing Demosaicing { get; set; }
        
        /// <summary>
        /// Import mode for deinterlacing.
        /// </summary>
        public bool Deinterlacing { get; set; }

        /// <summary>
        /// Path to the video file this metadata was created on.
        /// </summary>
        public string VideoPath
        {
            get { return videoPath; }
            set { videoPath = value;}
        }

        /// <summary>
        /// Keyframe accessor.
        /// </summary>
        public Keyframe this[int index]
        {
            // Indexor
            get 
            { 
                if(index < 0 || index >= keyframes.Count)
                    return null;
                else
                    return keyframes[index]; 
            }
            set { keyframes[index] = value; }
        }

        /// <summary>
        /// Collection of keyframes.
        /// </summary>
        public List<Keyframe> Keyframes
        {
            get { return keyframes; }
        }

        /// <summary>
        /// Number of keyframes.
        /// </summary>
        public int Count
        {
            get { return keyframes.Count; }
        }

        /// <summary>
        /// Whether there are any annotations that can be drawn on top of images.
        /// </summary>
        public bool HasVisibleData
        {
            get 
            {
                // This is used to know if there is anything to draw on the images when saving.
                // All objects should be taken into account here, even those
                // that we currently don't save to the .kva but only draw on the image.
                return keyframes.Count > 0 ||
                        chronoManager.Drawings.Count > 0 ||
                        trackManager.Drawings.Count > 0 ||
                        drawingSpotlight.Count > 0 ||
                        drawingNumberSequence.Count > 0 ||
                        drawingTestGrid.Visible ||
                        drawingCoordinateSystem.Visible ||
                        magnifier.Mode != MagnifierMode.Inactive;
            }
        }

        /// <summary>
        /// Whether we are currently in the process of tracking objects.
        /// </summary>
        public bool Tracking 
        {
            get 
            { 
                return TrackManager.Drawings.Any(t => ((DrawingTrack)t).Status == TrackStatus.Edit) || TrackabilityManager.Tracking; 
            }
        }

        /// <summary>
        /// Whether we are currently in the process of editing a text label.
        /// </summary>
        public bool TextEditingInProgress
        {
            get { return Labels().Any(l => l.Editing); }
        }

        /// <summary>
        /// The drawing that was hit during the last hit test, if any.
        /// </summary>
        public AbstractDrawing HitDrawing
        {
            get { return hitDrawing;}
        }

        /// <summary>
        /// The keyframe owning the drawing that was hit during the last hit test, if any.
        /// </summary>
        public Keyframe HitKeyframe
        {
            get { return hitKeyframe; }
        }

        /// <summary>
        /// The drawing manager owning the drawing that was hit during the last hit test, if any.
        /// </summary>
        public AbstractDrawingManager HitDrawingOwner
        {
            get { return hitDrawingOwner; }
        }

        public Magnifier Magnifier
        {
            get { return magnifier;}
            set { magnifier = value;}
        }
        public DrawingSpotlight DrawingSpotlight
        {
            get { return drawingSpotlight;}
        }
        public DrawingNumberSequence DrawingNumberSequence
        {
            get { return drawingNumberSequence;}
        }
        public DrawingCoordinateSystem DrawingCoordinateSystem
        {
            get { return drawingCoordinateSystem; }
        }
        public DrawingTestGrid DrawingTestGrid
        {
            get { return drawingTestGrid; }
        }
        public DrawingManager<AbstractDrawing> ChronoManager
        {
            get { return chronoManager; }
        }
        public DrawingManager<DrawingTrack> TrackManager
        {
            get { return trackManager; }
        }

        public DrawingManager<AbstractDrawing> SingletonDrawingsManager
        {
            get { return singletonDrawingsManager; }
        }
        
        /// <summary>
        /// Whether we are currently in the process of initializing a drawing.
        /// This is true for example when we are setting the second point of a line drawing.
        /// </summary>
        public bool DrawingInitializing
        {
            get
            {
                IInitializable initializable = hitDrawing as IInitializable;
                if (initializable == null)
                    return false;

                return initializable.Initializing;
            }
        }
        
        // General infos
        public long AverageTimeStampsPerFrame
        {
            get { return averageTimeStampsPerFrame; }
            set { averageTimeStampsPerFrame = value;}
        }
        public double AverageTimeStampsPerSecond
        {
            get { return averageTimeStampsPerSecond; }
            set { averageTimeStampsPerSecond = value; }
        }
        public long FirstTimeStamp
        {
            get { return firstTimeStamp; }
            set { firstTimeStamp = value; }
        }
        public long SelectionStart
        {
            get { return selectionStart; }
            set { selectionStart = value; }
        }
        public long SelectionEnd
        {
            get { return selectionEnd; }
            set { selectionEnd = value; }
        }

        /// <summary>
        /// User-defined origin of the time coordinate system, in absolute timestamps. 
        /// </summary>
        public long TimeOrigin
        {
            get { return timeOrigin; }
            set { timeOrigin = value; }
        }
        public bool TestGridVisible
        {
            get { return drawingTestGrid.Visible; }
            set { drawingTestGrid.Visible = value; }        
        }

        /// <summary>
        /// The ratio between the capture framerate and the video framerate.
        /// The slowdown factor of the video relatively to real time.
        /// </summary>
        public double HighSpeedFactor
        {
            get { return highSpeedFactor; }
            set { highSpeedFactor = value == 0 ? 1.0 : value; }
        }

        /// <summary>
        /// The frame interval used for playback timer, as specified by the user.
        /// </summary>
        public double UserInterval
        {
            get { return userInterval; }
            set { userInterval = value; }
        }

        public VideoFilterType ActiveVideoFilterType
        {
            get { return activeVideoFilterType; }
        }

        public IVideoFilter ActiveVideoFilter
        {
            get 
            {
                if (activeVideoFilterType == VideoFilterType.None)
                    return null;
                else
                    return videoFilters[activeVideoFilterType];
            }
        }

        /// <summary>
        /// Helper holding the necessary transforms to go from image space to world space.
        /// </summary>
        public CalibrationHelper CalibrationHelper 
        {
            get { return calibrationHelper; }
        }
        public TrackabilityManager TrackabilityManager
        {
            get { return trackabilityManager;}
        }
        #endregion

        #region Members
        private Guid id = Guid.NewGuid();
        private bool initialized;
        private TimeCodeBuilder timecodeBuilder;
        private HistoryStack historyStack;
        private int referenceHash;
        private bool kvaImporting;
        private bool captureKVA;

        // Folders
        private string videoPath;
        private string tempFolder;
        private AutoSaver autoSaver;
        private string lastKVAPath;

        // Keyframes & attached drawings.
        private List<Keyframe> keyframes = new List<Keyframe>();
        private Keyframe hitKeyframe;
        private AbstractDrawing hitDrawing;
        private AbstractDrawingManager hitDrawingOwner;

        // Detached drawings.
        private DrawingManager<AbstractDrawing> chronoManager = new DrawingManager<AbstractDrawing>();
        private DrawingManager<DrawingTrack> trackManager = new DrawingManager<DrawingTrack>();
        
        // Singleton drawings
        private DrawingManager<AbstractDrawing> singletonDrawingsManager = new DrawingManager<AbstractDrawing>();
        private DrawingSpotlight drawingSpotlight;
        private DrawingNumberSequence drawingNumberSequence;
        private DrawingCoordinateSystem drawingCoordinateSystem;
        private DrawingTestGrid drawingTestGrid;
        private Guid memoCoordinateSystemId;
        
        // The magnifier is not a regular drawing.
        private Magnifier magnifier = new Magnifier();

        private TrackerParameters lastUsedTrackerParameters;
        private MeasureLabelType mesureLabelType;
        private TrackabilityManager trackabilityManager = new TrackabilityManager();

        // Other info not related to drawings.
        private string globalTitle;
        private Size imageSize = new Size(0,0);
        private CalibrationHelper calibrationHelper = new CalibrationHelper();
        private Temporizer calibrationChangedTemporizer;
        private ImageTransform imageTransform = new ImageTransform();

        // Timing information
        private long averageTimeStampsPerFrame = 1;
        private double averageTimeStampsPerSecond = 25;
        private long firstTimeStamp;
        private long timeOrigin;
        private double highSpeedFactor = 1.0;
        private double userInterval = 40;
        private long selectionStart;
        private long selectionEnd;

        // Video filters
        private Dictionary<VideoFilterType, IVideoFilter> videoFilters = new Dictionary<VideoFilterType, IVideoFilter>();
        private VideoFilterType activeVideoFilterType = VideoFilterType.None;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor

        /// <summary>
        /// Main constructor used by the screens when they initialize themselves.
        /// This is the primary metadata that all other loads are going to merge into.
        /// The timecodeBuilder MUST be implemented as any metadata is subject to be exported
        /// and the user time attributes use it.
        /// </summary>
        public Metadata(HistoryStack historyStack, TimeCodeBuilder timecodeBuilder)
        {
            this.historyStack = historyStack;
            this.timecodeBuilder = timecodeBuilder;

            calibrationHelper.CalibrationChanged += CalibrationHelper_CalibrationChanged;
            
            autoSaver = new AutoSaver(this);
            
            CreateSingletonDrawings();
            CreateVideoFilters();
            CleanupHash();
            SetupTempDirectory(id);

            calibrationChangedTemporizer = new Temporizer(200, TracksCalibrationChanged);

            log.Debug("Constructing new Metadata object.");
        }
        public Metadata(string kvaString,  VideoInfo info, HistoryStack historyStack, TimeCodeBuilder timecodeBuilder, ClosestFrameDisplayer closestFrameDisplayer)
            : this(historyStack, timecodeBuilder)
        {
            // This should reflect what we do in FrameServerPlayer.SetupMetadata
            imageSize = info.ReferenceSize;
            userInterval = info.FrameIntervalMilliseconds;
            averageTimeStampsPerFrame = info.AverageTimeStampsPerFrame;
            averageTimeStampsPerSecond = info.AverageTimeStampsPerSeconds;
            calibrationHelper.CaptureFramesPerSecond = info.FramesPerSeconds;
            firstTimeStamp = info.FirstTimeStamp;
            
            videoPath = info.FilePath;

            MetadataSerializer serializer = new MetadataSerializer();
            serializer.Load(this, kvaString, false);
        }
        #endregion

        #region Public Interface
        
        #region Keyframes
        public Keyframe GetKeyframe(Guid id)
        {
            return keyframes.FirstOrDefault(kf => kf.Id == id);
        }
        public void AddKeyframe(Keyframe keyframe)
        {
            keyframes.Add(keyframe);
            keyframes.Sort();
            SelectKeyframe(keyframe);
            UpdateTrajectoriesForKeyframes();
            
            if (KeyframeAdded != null)
                KeyframeAdded(this, new KeyframeEventArgs(keyframe.Id));
        }
        public void DeleteKeyframe(Guid id)
        {
            foreach (Keyframe keyframe in keyframes)
            {
                if (keyframe.Id != id)
                    continue;

                foreach (AbstractDrawing drawing in keyframe.Drawings)
                    BeforeDrawingDeletion(drawing);
            }

            keyframes.RemoveAll(k => k.Id == id);
            UpdateTrajectoriesForKeyframes();
            
            if (KeyframeDeleted != null)
                KeyframeDeleted(this, new KeyframeEventArgs(id));
        }
        public void SelectKeyframe(Keyframe keyframe)
        {
            hitKeyframe = keyframe;
        }

        public void SelectManager(AbstractDrawingManager manager)
        {
            hitDrawingOwner = manager;
        }

        public void EnableDisableKeyframes()
        {
            foreach(Keyframe keyframe in keyframes)
            {
                keyframe.TimeCode = timecodeBuilder(keyframe.Position, TimeType.UserOrigin, PreferencesManager.PlayerPreferences.TimecodeFormat, true);
                keyframe.Disabled = keyframe.Position < selectionStart || keyframe.Position > selectionEnd;
            }
        }
        public int GetKeyframeIndex(long position)
        {
            for (int i = 0; i < keyframes.Count; i++)
                if (keyframes[i].Position == position)
                    return i;

            return -1;
        }
        public Guid GetKeyframeId(int keyframeIndex)
        {
            return keyframes[keyframeIndex].Id;
        }

        /// <summary>
        /// Returns whether this drawing is the kind of drawing that is attached to a keyframe.
        /// This doesn't necessarily mean it is currently parented to an actual keyframe.
        /// </summary>
        public bool IsAttachedDrawing(AbstractDrawing drawing)
        {
            bool detached = 
                chronoManager.Drawings.Contains(drawing) ||
                trackManager.Drawings.Contains(drawing) ||
                SingletonDrawingsManager.Drawings.Contains(drawing);

            return !detached; 
        }

        /// <summary>
        /// Returns the id of the manager managing this drawing.
        /// </summary>
        public Guid FindManagerId(AbstractDrawing drawing)
        {
            if (drawing is DrawingChrono || drawing is DrawingChronoMulti)
            {
                return chronoManager.Id;
            }
            else if (drawing is DrawingTrack)
            {
                return trackManager.Id;
            }
            else if (SingletonDrawingsManager.Drawings.Contains(drawing))
            {
                return SingletonDrawingsManager.Id;
            }
            else
            {
                return FindAttachmentKeyframeId(drawing);
            }
        }

        /// <summary>
        /// Returns the id of the keyframe the drawing is attached to.
        /// </summary>
        public Guid FindAttachmentKeyframeId(AbstractDrawing drawing)
        {
            Keyframe foundKeyframe = null;
            foreach (Keyframe k in keyframes)
            {
                foreach (AbstractDrawing d in k.Drawings)
                {
                    if (d.Id != drawing.Id)
                        continue;

                    foundKeyframe = k;
                    break;
                }

                if (foundKeyframe != null)
                    break;
            }

            return foundKeyframe == null ? Guid.Empty : foundKeyframe.Id;
        }


        /// <summary>
        /// Returns the (keyframe-attached) drawing with the passed id.
        /// </summary>
        public AbstractDrawing FindDrawing(Guid drawingId)
        {
            foreach (Keyframe k in keyframes)
            {
                foreach (AbstractDrawing d in k.Drawings)
                {
                    if (d.Id != drawingId)
                        continue;

                    return d;
                }
            }

            return null;
        }


        public int GetKeyframeIndex(Guid id)
        {
            // Temporary function to accomodate clients of the old API where we used indices to reference keyframes and drawings.
            for (int i = 0; i < keyframes.Count; i++)
                if (keyframes[i].Id == id)
                    return i;

            return -1;
        }
        public void MergeInsertKeyframe(Keyframe keyframe)
        {
            bool processed = false;

            // If the keyframe is already known (by ID) we don't import nor merge it.
            Keyframe known = keyframes.FirstOrDefault((kf) => kf.Id == keyframe.Id);
            if (known != null)
                return;
            
            for (int i = 0; i < keyframes.Count; i++)
            {
                Keyframe k = keyframes[i];

                if (keyframe.Position < k.Position)
                {
                    keyframes.Insert(i, keyframe);
                    processed = true;
                    break;
                }
                else if (keyframe.Position == k.Position)
                {
                    foreach (AbstractDrawing ad in keyframe.Drawings)
                        k.Drawings.Add(ad);

                    processed = true;
                    break;
                }
            }

            if (!processed)
                keyframes.Add(keyframe);

            // Post-init for the new drawings.
            foreach (AbstractDrawing ad in keyframe.Drawings)
                AfterDrawingCreation(ad);
        }
        #endregion
        
        #region Filtered iterators
        public IEnumerable<DrawingTrack> Tracks()
        {
            foreach (AbstractDrawing drawing in TrackManager.Drawings)
                if (drawing is DrawingTrack)
                    yield return (DrawingTrack)drawing;
        }
        public IEnumerable<AbstractDrawing> AttachedDrawings()
        {
            foreach (Keyframe kf in keyframes)
                foreach (AbstractDrawing drawing in kf.Drawings)
                    yield return drawing;
        }
        public IEnumerable<DrawingText> Labels()
        {
            foreach (AbstractDrawing drawing in AttachedDrawings())
                if (drawing is DrawingText)
                    yield return (DrawingText)drawing;
        }
        public IEnumerable<DrawingLine> Lines()
        {
            foreach (AbstractDrawing drawing in AttachedDrawings())
                if (drawing is DrawingLine)
                    yield return (DrawingLine)drawing;
        }
        public IEnumerable<DrawingAngle> Angles()
        {
            foreach (AbstractDrawing drawing in AttachedDrawings())
                if (drawing is DrawingAngle)
                    yield return (DrawingAngle)drawing;
        }
        public IEnumerable<DrawingCrossMark> CrossMarks()
        {
            foreach (AbstractDrawing drawing in AttachedDrawings())
                if (drawing is DrawingCrossMark)
                    yield return (DrawingCrossMark)drawing;
        }
        public IEnumerable<DrawingGenericPosture> GenericPostures()
        {
            foreach (AbstractDrawing drawing in AttachedDrawings())
                if (drawing is DrawingGenericPosture)
                    yield return (DrawingGenericPosture)drawing;
        }
        public IEnumerable<DrawingPlane> Planes()
        {
            foreach (AbstractDrawing drawing in AttachedDrawings())
                if (drawing is DrawingPlane)
                    yield return (DrawingPlane)drawing;
        }
        
        public IEnumerable<DrawingSVG> SVGs()
        {
            foreach (AbstractDrawing drawing in AttachedDrawings())
                if (drawing is DrawingSVG)
                    yield return (DrawingSVG)drawing;
        }

        /// <summary>
        /// Returns trackable drawings, does not include Tracks themselves.
        /// </summary>
        public IEnumerable<ITrackable> TrackableDrawings()
        {
            foreach (AbstractDrawing drawing in singletonDrawingsManager.Drawings)
            {
                if (drawing is ITrackable)
                    yield return (ITrackable)drawing;
            }

            foreach (AbstractDrawing drawing in AttachedDrawings())
            {
                if (drawing is ITrackable)
                    yield return (ITrackable)drawing;
            }
        }
        public IEnumerable<DrawingDistortionGrid> DistortionGrids()
        {
            foreach (AbstractDrawing drawing in AttachedDrawings())
                if (drawing is DrawingDistortionGrid)
                    yield return (DrawingDistortionGrid)drawing;
        }
        #endregion
        
        #region Drawings
        public AbstractDrawingManager GetDrawingManager(Guid managerId)
        {
            if (managerId == chronoManager.Id)
                return chronoManager;
            else if (managerId == trackManager.Id)
                return trackManager;
            else if (managerId == singletonDrawingsManager.Id)
                return singletonDrawingsManager;
            else
                return GetKeyframe(managerId);
        }

        public AbstractDrawing GetDrawing(Guid managerId, Guid drawingId)
        {
            AbstractDrawingManager manager = null;

            if (managerId == chronoManager.Id)
                manager = chronoManager;
            else if (managerId == trackManager.Id)
                manager = trackManager;
            else
                manager = GetKeyframe(managerId);

            if (manager == null)
                return null;

            return manager.GetDrawing(drawingId);
        }
        
        /// <summary>
        /// General method to add a drawing on a manager (keyframe-attched, chrono, track).
        /// </summary>
        public void AddDrawing(Guid managerId, AbstractDrawing drawing)
        {
            if (drawing == null)
                return;

            Keyframe keyframe = GetKeyframe(managerId);
            if (keyframe != null)
            {
                bool known = keyframe.Drawings.Any(d => d.Id == drawing.Id);
                if (!known)
                    AddDrawing(keyframe, drawing);
                return;
            }

            if (chronoManager.Id == managerId && (drawing is DrawingChrono || drawing is DrawingChronoMulti))
            {
                bool known = chronoManager.Drawings.Any(d => d.Id == drawing.Id);
                if (!known)
                    AddChrono(drawing);

                return;
            }

            if (trackManager.Id == managerId && drawing is DrawingTrack)
            {
                bool known = trackManager.Drawings.Any(d => d.Id == drawing.Id);
                if (!known)
                    AddTrack(drawing as DrawingTrack);
                return;
            }
        }
        
        /// <summary>
        /// Adds a drawing to the specified keyframe.
        /// </summary>
        public void AddDrawing(Keyframe keyframe, AbstractDrawing drawing)
        {
            if (keyframe == null || drawing == null || !drawing.IsValid)
                return;

            keyframe.AddDrawing(drawing);
            drawing.ParentMetadata = this;
            drawing.InfosFading.ReferenceTimestamp = keyframe.Position;
            drawing.InfosFading.AverageTimeStampsPerFrame = averageTimeStampsPerFrame;
            if (captureKVA)
            {
                drawing.InfosFading.UseDefault = false;
                drawing.InfosFading.AlwaysVisible = true;
            }

            SelectKeyframe(keyframe);
            SelectDrawing(drawing);

            AfterDrawingCreation(drawing);

            if (DrawingAdded != null)
                DrawingAdded(this, new DrawingEventArgs(drawing, keyframe.Id));
        }
        
        /// <summary>
        /// Adds a new item to a multi drawing.
        /// </summary>
        public void AddMultidrawingItem(AbstractMultiDrawing multidrawing, AbstractMultiDrawingItem item)
        {
            multidrawing.Add(item);
            SelectDrawing(multidrawing);

            if (MultiDrawingItemAdded != null)
                MultiDrawingItemAdded(this, new MultiDrawingItemEventArgs(item, multidrawing));
        }
        
        /// <summary>
        /// Adds a new chronometer drawing.
        /// </summary>
        public void AddChrono(AbstractDrawing chrono)
        {
            chronoManager.AddDrawing(chrono);
            chrono.ParentMetadata = this;
            
            hitDrawing = chrono;

            AfterDrawingCreation(chrono);

            if (DrawingAdded != null)
                DrawingAdded(this, new DrawingEventArgs(chrono, chronoManager.Id));
        }

        /// <summary>
        /// Adds a new track drawing.
        /// </summary>
        public void AddTrack(DrawingTrack track)
        {
            trackManager.AddDrawing(track);
            track.ParentMetadata = this;
            
            if (lastUsedTrackerParameters != null)
                track.TrackerParameters = lastUsedTrackerParameters;
            
            track.TrackerParametersChanged += Track_TrackerParametersChanged;

            hitDrawing = track;
            
            AfterDrawingCreation(track);

            // The following is necessary for the "undo of deletion" case.
            track.UpdateKinematics();
            track.IntegrateKeyframes();
            
            if (DrawingAdded != null)
                DrawingAdded(this, new DrawingEventArgs(track, trackManager.Id));
        }

        public void ModifiedDrawing(Guid managerId, Guid drawingId)
        {
            AbstractDrawing drawing = GetDrawing(managerId, drawingId);
            
            DrawingTrack track = drawing as DrawingTrack;
            if (track != null)
            {
                track.UpdateKinematics();
                track.IntegrateKeyframes();
            }

            if (DrawingModified != null)
                DrawingModified(this, new DrawingEventArgs(drawing, managerId));
        }

        public void ModifiedVideoFilter()
        {
            if (VideoFilterModified != null)
                VideoFilterModified(this, EventArgs.Empty);
        }
        
        public void DeleteDrawing(Guid managerId, Guid drawingId)
        {
            // Remove event handlers from the drawing as well as all associated data like tracking data,
            // and finally remove the drawing itself.

            AbstractDrawingManager manager = null;

            if (managerId == chronoManager.Id)
                manager = chronoManager;
            else if (managerId == trackManager.Id)
                manager = trackManager;
            else
                manager = GetKeyframe(managerId);

            if (manager == null)
                return;
                
            AbstractDrawing drawing = manager.GetDrawing(drawingId);
            if (drawing == null)
                return;

            BeforeDrawingDeletion(drawing);
            
            manager.RemoveDrawing(drawingId);
            DeselectAll();
            
            if (DrawingDeleted != null)
                DrawingDeleted(this, EventArgs.Empty);
        }
        
        public void DeleteMultiDrawingItem(AbstractMultiDrawing manager, Guid itemId)
        {
            ITrackable item = manager.GetItem(itemId) as ITrackable;
            if (item != null)
                DeleteTrackableDrawing(item);

            manager.Remove(itemId);
            DeselectAll();

            if (MultiDrawingItemDeleted != null)
                MultiDrawingItemDeleted(this, EventArgs.Empty);
        }
        
        private void DeleteTrackableDrawing(ITrackable drawing)
        {
            trackabilityManager.Remove(drawing);
        }

        private void Track_TrackerParametersChanged(object sender, EventArgs e)
        {
            // Remember these track parameters to bootstrap the next trackable.
            DrawingTrack track = sender as DrawingTrack;
            if (track == null)
                return;

            lastUsedTrackerParameters = track.TrackerParameters;
        }
        
        public void InitializeCommit(VideoFrame videoFrame, PointF point)
        {
            magnifier.InitializeCommit(point);

            IInitializable initializable = hitDrawing as IInitializable;
            if (initializable == null || !initializable.Initializing)
                return;
            
            string key = initializable.InitializeCommit(point);

            if (string.IsNullOrEmpty(key))
                return;

            ITrackable trackable = hitDrawing as ITrackable;
            if (trackable != null)
                trackabilityManager.AddPoint(trackable, videoFrame, key, point);
        }
        public void InitializeEnd(bool cancelCurrentPoint)
        {
            IInitializable initializable = hitDrawing as IInitializable;
            if (initializable == null || !initializable.Initializing)
                return;

            string key = initializable.InitializeEnd(cancelCurrentPoint);

            if (string.IsNullOrEmpty(key) || !cancelCurrentPoint)
                return;
            
            ITrackable trackable = hitDrawing as ITrackable;
            if (trackable != null)
                trackabilityManager.RemovePoint(trackable, key);
        }
        #endregion
        
        /// <summary>
        /// Collect measured data for spreadsheet export.
        /// </summary>
        public MeasuredData CollectMeasuredData()
        {
            MeasuredData md = new MeasuredData();
            md.Producer = Software.ApplicationName + "." + Software.Version;
            md.OriginalFilename = Path.GetFileNameWithoutExtension(videoPath);
            md.FullPath = videoPath;
            md.ImageSize = imageSize;
            md.CaptureFramerate = (float)calibrationHelper.CaptureFramesPerSecond;
            md.UserFramerate = (float)(1000.0 / userInterval);

            MeasuredDataUnits mdu = new MeasuredDataUnits();

            if (PreferencesManager.PlayerPreferences.ExportSpace == ExportSpace.WorldSpace)
            { 
                mdu.LengthUnit = CalibrationHelper.LengthUnit.ToString();
                mdu.LengthSymbol = UnitHelper.LengthAbbreviation(CalibrationHelper.LengthUnit);
            }
            else
            {
                mdu.LengthUnit = LengthUnit.Pixels.ToString();
                mdu.LengthSymbol = UnitHelper.LengthAbbreviation(LengthUnit.Pixels);
            }
            
            mdu.SpeedUnit = PreferencesManager.PlayerPreferences.SpeedUnit.ToString();
            mdu.SpeedSymbol = UnitHelper.SpeedAbbreviation(PreferencesManager.PlayerPreferences.SpeedUnit);
            mdu.AccelerationUnit = PreferencesManager.PlayerPreferences.AccelerationUnit.ToString();
            mdu.AccelerationSymbol = UnitHelper.AccelerationAbbreviation(PreferencesManager.PlayerPreferences.AccelerationUnit);
            mdu.AngleUnit = PreferencesManager.PlayerPreferences.AngleUnit.ToString();
            mdu.AngleSymbol = UnitHelper.AngleAbbreviation(PreferencesManager.PlayerPreferences.AngleUnit);
            mdu.AngularVelocityUnit = PreferencesManager.PlayerPreferences.AngularVelocityUnit.ToString();
            mdu.AngularVelocitySymbol = UnitHelper.AngularVelocityAbbreviation(PreferencesManager.PlayerPreferences.AngularVelocityUnit);
            mdu.AngularAccelerationUnit = PreferencesManager.PlayerPreferences.AngularAccelerationUnit.ToString();
            mdu.AngularAccelerationSymbol = UnitHelper.AngularAccelerationAbbreviation(PreferencesManager.PlayerPreferences.AngularAccelerationUnit);
            mdu.TimeSymbol = UnitHelper.TimeAbbreviation(PreferencesManager.PlayerPreferences.TimecodeFormat);
            md.Units = mdu;

            foreach (Keyframe kf in Keyframes.Where(kf => !kf.Disabled))
            {
                var mdkf = kf.CollectMeasuredData();
                md.Keyframes.Add(mdkf);

                List<MeasuredDataPosition> mdps = new List<MeasuredDataPosition>();
                List<MeasuredDataDistance> mdds = new List<MeasuredDataDistance>();
                List<MeasuredDataAngle> mdas = new List<MeasuredDataAngle>();
                foreach (AbstractDrawing d in kf.Drawings)
                {
                    // Positions from markers.
                    if (d is DrawingCrossMark)
                        mdps.Add(((DrawingCrossMark)d).CollectMeasuredData());

                    // Positions from postures.
                    if (d is DrawingGenericPosture)
                        mdps.AddRange(((DrawingGenericPosture)d).CollectMeasuredDataPositions());

                    // Distances from lines.
                    if (d is DrawingLine)
                        mdds.Add(((DrawingLine)d).CollectMeasuredData());

                    // Distances from postures.
                    if (d is DrawingGenericPosture)
                        mdds.AddRange(((DrawingGenericPosture)d).CollectMeasuredDataDistances());

                    // Angles from angle tools.
                    if (d is DrawingAngle)
                        mdas.Add(((DrawingAngle)d).CollectMeasuredData());

                    // Angles from postures.
                    if (d is DrawingGenericPosture)
                        mdas.AddRange(((DrawingGenericPosture)d).CollectMeasuredDataAngles());
                }

                // Sort drawings on the same keyframe by name.
                mdps.Sort((a, b) => a.Name.CompareTo(b.Name));
                mdds.Sort((a, b) => a.Name.CompareTo(b.Name));
                mdas.Sort((a, b) => a.Name.CompareTo(b.Name));

                // Inject time.
                foreach (MeasuredDataPosition mdp in mdps)
                    mdp.Time = mdkf.Time;
                foreach (MeasuredDataDistance mdd in mdds)
                    mdd.Time = mdkf.Time;
                foreach (MeasuredDataAngle mda in mdas)
                    mda.Time = mdkf.Time;

                // Add to the global list.
                md.Positions.AddRange(mdps);
                md.Distances.AddRange(mdds);
                md.Angles.AddRange(mdas);
            }

            // Times.
            foreach (AbstractDrawing d in chronoManager.Drawings)
            {   
                if (d is DrawingChrono)
                    md.Times.Add(((DrawingChrono)d).CollectMeasuredData());

                if (d is DrawingChronoMulti)
                    md.Times.AddRange(((DrawingChronoMulti)d).CollectMeasuredData());
            }
            md.Times.Sort((a, b) => a.Start.CompareTo(b.Start));

            md.Timeseries = new List<MeasuredDataTimeseries>();
            
            // Tracks.
            foreach (DrawingTrack track in trackManager.Drawings)
                md.Timeseries.Add(track.CollectMeasuredData());
            
            // Timelines.
            trackabilityManager.CollectMeasuredData(this, md.Timeseries);

            md.Timeseries.Sort((a, b) => a.FirstTimestamp.CompareTo(b.FirstTimestamp));

            return md;
        }

        /// <summary>
        /// Convert from timestamps to a numerical time format.
        /// If the preferred time format is not numeric we return seconds.
        /// TimeType.WorkingZone is not supported.
        /// </summary>
        public float GetNumericalTime(long timestamps, TimeType type)
        {
            TimecodeFormat tcf = PreferencesManager.PlayerPreferences.TimecodeFormat;
            
            // TimecodeFormat.Normalized is not supported at this point.
            if (tcf == TimecodeFormat.Normalized)
                tcf = TimecodeFormat.ClassicTime;

            long actualTimestamps = timestamps;
            if (type == TimeType.UserOrigin)
                actualTimestamps = timestamps - timeOrigin;

            // TODO: use double for info.AverageTimestampsPerFrame.
            //double averageTimestampsPerFrame = AverageTimeStampsPerSeconds / FramesPerSeconds;
            double averageTimestampsPerFrame = this.AverageTimeStampsPerFrame;

            float frames = 0;
            if (AverageTimeStampsPerFrame != 0)
                frames = (float)Math.Round(actualTimestamps / averageTimestampsPerFrame);

            if (type == TimeType.Duration)
                frames++;

            double milliseconds = frames * UserInterval / HighSpeedFactor;
            
            double time;
            switch (tcf)
            {
                case TimecodeFormat.Frames:
                    time = frames;
                    break;
                case TimecodeFormat.Milliseconds:
                    time = milliseconds;
                    break;
                case TimecodeFormat.Microseconds:
                    time = milliseconds * 1000;
                    break;
                case TimecodeFormat.TenThousandthOfHours:
                    // 1 Ten Thousandth of Hour = 360 ms.
                    time = Math.Round(milliseconds) / 360.0;
                    break;
                case TimecodeFormat.HundredthOfMinutes:
                    // 1 Hundredth of minute = 600 ms.
                    time = Math.Round(milliseconds) / 600.0;
                    break;
                case TimecodeFormat.Timestamps:
                    time = timestamps;
                    break;
                default:
                    time = Math.Round(milliseconds) / 1000.0;
                    break;
            }

            return (float)time;
        }

        /// <summary>
        /// Get the timecode at a fraction of the frame interval.
        /// This is currently only used by the time segment tool.
        /// </summary>
        public string GetFractionTime(long timestamps, float fraction)
        {
            TimecodeFormat tcf = PreferencesManager.PlayerPreferences.TimecodeFormat;
            long actualTimestamps = timestamps - timeOrigin;
            double averageTimestampsPerFrame = this.AverageTimeStampsPerFrame;
            float frames = 0;
            if (AverageTimeStampsPerFrame != 0)
                frames = (float)Math.Round(actualTimestamps / averageTimestampsPerFrame);

            double startMS = frames * UserInterval / HighSpeedFactor;
            double endMS = (frames + 1) * UserInterval / HighSpeedFactor;
            double milliseconds = startMS + ((endMS - startMS) * fraction);

            double framerate = 1000.0 / UserInterval * HighSpeedFactor;
            double framerateMagnitude = Math.Log10(framerate);
            int precision = (int)Math.Ceiling(framerateMagnitude);
            
            string outputTimeCode = "";
            switch (tcf)
            {
                case TimecodeFormat.Frames:
                    outputTimeCode = String.Format("{0}", frames + Math.Round(fraction * 1000) / 1000);
                    break;
                case TimecodeFormat.Milliseconds:
                    outputTimeCode = String.Format("{0}", Math.Round(milliseconds * 1000) / 1000);
                    outputTimeCode += " ms";
                    break;
                case TimecodeFormat.Microseconds:
                    outputTimeCode = String.Format("{0}", Math.Round(milliseconds * 1000000) / 1000);
                    outputTimeCode += " µs";
                    break;
                case TimecodeFormat.ClassicTime:
                default:
                    // Increase magnitude by 1. This means for example if we are exactly at 100 fps,
                    // we'll get the value in milliseconds instead of centiseconds.
                    // It is equivalent to having 10 possible values along the segment.
                    // This is the worst case scenario, in other cases we'll get somewhere between 10 and 100 values along the segment.
                    // We limit the precision here to avoid giving a false sense of accuracy.
                    outputTimeCode = TimeHelper.MillisecondsToTimecode(milliseconds, precision + 1);
                    break;
            }

            return outputTimeCode;
        }

        public void PostSetup(bool init)
        {
            if (init)
            {
                trackabilityManager.Initialize(imageSize);
                calibrationHelper.Initialize(imageSize, GetCalibrationOrigin, GetCalibrationQuad, HasTrackingData);
            }

            if (!initialized)
            {
                foreach (AbstractDrawing d in singletonDrawingsManager.Drawings)
                    AfterDrawingCreation(d);
            }
            else
            {
                AfterDrawingCreation(drawingCoordinateSystem);
            }

            CleanupHash();
            initialized = true;
        }

        public void PostSetupCapture()
        {
            captureKVA = true;
            trackabilityManager.Initialize(imageSize);
            calibrationHelper.Initialize(imageSize, GetCalibrationOrigin, GetCalibrationQuad, HasTrackingData);

            foreach (AbstractDrawing d in singletonDrawingsManager.Drawings)
                AfterDrawingCreation(d);
        }

        public void Reset()
        {
            // Complete reset. (used when over loading a new video)
            log.Debug("Metadata Reset.");

            globalTitle = "";
            imageSize = new Size(0, 0);
            videoPath = "";
            lastKVAPath = "";
            averageTimeStampsPerFrame = 1;
            firstTimeStamp = 0;
            
            ResetCoreContent();
            autoSaver.Stop();
            EmptyTempDirectory();
            CleanupHash();
        }
        public void Close()
        {
            foreach (IVideoFilter filter in videoFilters.Values)
                filter.Dispose();

            DeleteTempDirectory();
        }
        public void ShowCoordinateSystem()
        {
            drawingCoordinateSystem.Visible = true;
        }
        public void UpdateTrajectoriesForKeyframes()
        {
            foreach (DrawingTrack t in Tracks())
                t.CalibrationChanged();
        }
        public void FixRelativeTrajectories()
        {
            foreach (DrawingTrack t in Tracks())
                t.FixRelativeTrajectories();
        }
        public void AllDrawingTextToNormalMode()
        {
            foreach (DrawingText label in Labels())
                label.SetEditMode(false, PointF.Empty, null);
        }
        public void PerformTracking(VideoFrame videoframe)
        {
            foreach(DrawingTrack t in Tracks())
                if (t.Status == TrackStatus.Edit)
                    t.TrackCurrentPosition(videoframe);
        }
        public void StopAllTracking()
        {
            foreach(DrawingTrack t in Tracks())
                t.StopTracking();
        }
        public void ClearTracking()
        {
            foreach (DrawingTrack t in Tracks())
                t.Clear();
        }

        public void UpdateTrackPoint(Bitmap bitmap)
        {
            // Happens when mouse up and editing a track.
            DrawingTrack t = hitDrawing as DrawingTrack;
            if(t != null && (t.Status == TrackStatus.Edit || t.Status == TrackStatus.Configuration))
                t.UpdateTrackPoint(bitmap);
        }
        public int GetContentHash()
        {
            int hash = 0;
            hash ^= ImageAspect.GetHashCode();
            hash ^= ImageRotation.GetHashCode();
            hash ^= Mirrored.GetHashCode();
            hash ^= Demosaicing.GetHashCode();
            hash ^= Deinterlacing.GetHashCode();
            hash ^= selectionStart.GetHashCode();
            hash ^= selectionEnd.GetHashCode();
            hash ^= timeOrigin.GetHashCode();
            hash ^= calibrationHelper.ContentHash;
            hash ^= GetKeyframesContentHash();
            hash ^= GetSingletonDrawingsContentHash();
            hash ^= trackabilityManager.ContentHash;
            hash ^= GetVideoFiltersContentHash();
            return hash;
        }
        public void CleanupHash()
        {
            referenceHash = GetContentHash();
            autoSaver.Clear();
            log.Debug(String.Format("Metadata content hash reset:{0}.", referenceHash));
        }
        public void ResizeFinished()
        {
            // This function can be used to trigger an update to drawings that do not 
            // render in the same way when the user is resizing the window or not.
            foreach(DrawingSVG svg in SVGs())
                svg.ResizeFinished();
        }
        public void DeselectAll()
        {
            hitDrawingOwner = null;
            hitKeyframe = null;
            hitDrawing = null;
        }
        public void SelectDrawing(AbstractDrawing drawing)
        {
            hitDrawing = drawing;
        }
        private void AfterDrawingCreation(AbstractDrawing drawing)
        {
            // When passing here, it is possible that the drawing has already been initialized.
            // (for example, for undo of delete, paste or reload from KVA).

            if (string.IsNullOrEmpty(drawing.Name))
                SetDrawingName(drawing);

            drawing.ParentMetadata = this;

            if (drawing is IScalable)
                ((IScalable)drawing).Scale(this.ImageSize);

            if (drawing is ITrackable && AddTrackableDrawingCommand != null)
                AddTrackableDrawingCommand.Execute(drawing as ITrackable);

            if (drawing is IMeasurable)
            {
                IMeasurable measurableDrawing = drawing as IMeasurable;
                measurableDrawing.CalibrationHelper = calibrationHelper;

                measurableDrawing.InitializeMeasurableData(mesureLabelType);
                measurableDrawing.ShowMeasurableInfoChanged += MeasurableDrawing_ShowMeasurableInfoChanged;
            }

            if (drawing is DrawingDistortionGrid)
            {
                DrawingDistortionGrid d = drawing as DrawingDistortionGrid;
                d.LensCalibrationAsked += LensCalibrationAsked;
            }
        }

        private void BeforeDrawingDeletion(AbstractDrawing drawing)
        {
            ITrackable trackableDrawing = drawing as ITrackable;
            if (trackableDrawing != null)
                DeleteTrackableDrawing(trackableDrawing);

            IMeasurable measurableDrawing = drawing as IMeasurable;
            if (measurableDrawing != null)
                measurableDrawing.ShowMeasurableInfoChanged -= MeasurableDrawing_ShowMeasurableInfoChanged;

            if (drawing is DrawingDistortionGrid)
                ((DrawingDistortionGrid)drawing).LensCalibrationAsked -= LensCalibrationAsked;
        }

        public void BeforeKVAImport()
        {
            kvaImporting = true;
            StopAllTracking();
            DeselectAll();
            
            memoCoordinateSystemId = drawingCoordinateSystem.Id;
        }
        public void AfterKVAImport()
        {
            trackabilityManager.UpdateId(memoCoordinateSystemId, drawingCoordinateSystem.Id);
            
            foreach (ITrackable drawing in TrackableDrawings())
                trackabilityManager.Assign(drawing);

            trackabilityManager.CleanUnassigned();
            
            AfterCalibrationChanged();
            kvaImporting = false;

            if (KVAImported != null)
                KVAImported(this, EventArgs.Empty);
        }
        public void AfterManualExport()
        {
            CleanupHash();
            DeleteAutosaveFile();
        }
        public bool Recover(Guid id)
        {
            DeleteTempDirectory();
            SetupTempDirectory(id);
            string autosaveFile = Path.Combine(tempFolder, "autosave.kva");
            bool recovered = false;
            if (File.Exists(autosaveFile))
            {
                MetadataSerializer s = new MetadataSerializer();
                s.Load(this, autosaveFile, true);
                recovered = true;
            }

            return recovered;
        }

        public List<List<PointF>> GetCameraCalibrationPoints()
        {
            List<List<PointF>> points = new List<List<PointF>>();
            foreach (DrawingDistortionGrid grid in DistortionGrids())
                points.Add(grid.Points);

            return points;
        }

        #region Autosave
        public void StartAutosave()
        {
            autoSaver.FreshStart();
        }
        public void PauseAutosave()
        {
            autoSaver.Stop();
        }
        public void UnpauseAutosave()
        {
            autoSaver.Start();
        }
        public void PerformAutosave()
        {
            MetadataSerializer serializer = new MetadataSerializer();
            serializer.SaveToFile(this, Path.Combine(tempFolder, "autosave.kva"));
        }
        #endregion

        #region Objects Hit Tests

        // Note: these hit tests are for right click only.
        // They work slightly differently than the hit test in the PointerTool which is for left click.
        // The main difference is that here we only need to know if the drawing was hit at all,
        // in the pointer tool, we need to differenciate which handle was hit.
        // When a drawing is hit it will be stored in Metadata.hitDrawing.

        /// <summary>
        /// Hit test the passed point with regards to attached drawings.
        /// </summary>
        public bool IsOnDrawing(int _iActiveKeyframeIndex, PointF point, long _iTimestamp)
        {
            // Returns whether the mouse is on a drawing attached to a key image.
            bool hit = false;
            
            if (PreferencesManager.PlayerPreferences.DefaultFading.Enabled && keyframes.Count > 0)
            {
                int[] zOrder = GetKeyframesZOrder(_iTimestamp);

                for(int i=0;i<zOrder.Length;i++)
                {
                    hit = DrawingsHitTest(zOrder[i], point, _iTimestamp);
                    if (hit)
                        break;
                }
            }
            else if (_iActiveKeyframeIndex >= 0)
            {
                // If fading is off, only try the current keyframe (if any)
                hit = DrawingsHitTest(_iActiveKeyframeIndex, point, _iTimestamp);
            }

            return hit;
        }


        /// <summary>
        /// Hit test the passed point with regards to detached and singleton drawings.
        /// Returns the hit drawing (or null if none), and selects it.
        /// </summary>
        public AbstractDrawing IsOnDetachedDrawing(PointF point, long timestamp)
        {
            AbstractDrawing result = null;

            foreach (AbstractDrawing chrono in chronoManager.Drawings)
            {
                int hit = chrono.HitTest(point, timestamp, calibrationHelper.DistortionHelper, imageTransform, imageTransform.Zooming);
                if (hit < 0)
                    continue;

                result = chrono;
                hitDrawing = chrono;
                hitDrawingOwner = chronoManager;
                break;
            }

            if (result != null)
                return result;

            foreach (DrawingTrack track in trackManager.Drawings)
            {
                int hit = track.HitTest(point, timestamp, calibrationHelper.DistortionHelper, imageTransform, imageTransform.Zooming);
                if (hit < 0)
                    continue;

                result = track;
                hitDrawing = track;
                hitDrawingOwner = trackManager;
                break;
            }

            if (result != null)
                return result;

            foreach (AbstractDrawing drawing in singletonDrawingsManager.Drawings)
            {
                int hit = drawing.HitTest(point, timestamp, calibrationHelper.DistortionHelper, imageTransform, imageTransform.Zooming);
                if (hit < 0)
                    continue;

                result = drawing;
                hitDrawing = drawing;
                hitDrawingOwner = singletonDrawingsManager;
            }
            
            return result;
        }
        
        /// <summary>
        /// Hit test the passed point with regards to the magnifier.
        /// </summary>
        public bool IsOnMagnifier(PointF point)
        {
            if (magnifier.Mode != MagnifierMode.Active)
                return false;

            int hitRes = magnifier.HitTest(point, imageTransform);

            return hitRes >= 0;
        }

        /// <summary>
        /// Returns a list of indices in the keyframe collection for hit testing.
        /// </summary>
        public int[] GetKeyframesZOrder(long _iTimestamp)
        {
            // TODO: turn this into an iterator.
            
            // Get the Z ordering of Keyframes for hit tests & draw.
            int[] zOrder = new int[keyframes.Count];

            if (keyframes.Count > 0)
            {
                if (_iTimestamp <= keyframes[0].Position)
                {
                    // All key frames are after this position
                    for(int i=0;i<keyframes.Count;i++)
                    {
                        zOrder[i] = i;
                    }
                }
                else if (_iTimestamp > keyframes[keyframes.Count - 1].Position)
                {
                    // All keyframes are before this position
                    for (int i = 0; i < keyframes.Count; i++)
                    {
                        zOrder[i] = keyframes.Count - i - 1;
                    }
                }
                else
                {
                    // Some keyframes are after, some before.
                    // Start at the first kf after this position until the end,
                    // then go backwards from the first kf before this position until the begining.

                    int iCurrentFrame = keyframes.Count;
                    int iClosestNext = keyframes.Count - 1;
                    while (iCurrentFrame > 0)
                    {
                        iCurrentFrame--;
                        if (keyframes[iCurrentFrame].Position >= _iTimestamp)
                        {
                            iClosestNext = iCurrentFrame;
                        }
                        else
                        {
                            break;
                        }
                    }

                    for(int i=iClosestNext;i<keyframes.Count;i++)
                    {
                        zOrder[i - iClosestNext] = i;
                    }
                    for (int i = 0; i < iClosestNext; i++)
                    {
                        zOrder[keyframes.Count - i - 1] = i;
                    }
                }
            }

            return zOrder;
        }
        #endregion

        #region Video filters
        public void ActivateVideoFilter(VideoFilterType type)
        {
            if (!videoFilters.ContainsKey(type))
                throw new InvalidProgramException();
            
            activeVideoFilterType = type;
        }

        public void DeactivateVideoFilter()
        {
            activeVideoFilterType = VideoFilterType.None;
        }

        public IVideoFilter GetVideoFilter(VideoFilterType type)
        {
            if (videoFilters.ContainsKey(type))
                return videoFilters[type];

            return null;
        }

        public void WriteVideoFilters(XmlWriter w)
        {
            foreach (var pair in videoFilters)
            {
                string xmlName = VideoFilterFactory.GetName(pair.Key);
                w.WriteStartElement(xmlName);
                pair.Value.WriteData(w);
                w.WriteEndElement();
            }
        }

        public void ReadVideoFilters(XmlReader r)
        {
            bool isEmpty = r.IsEmptyElement;

            if (r.MoveToAttribute("active"))
                activeVideoFilterType = VideoFilterFactory.GetFilterType(r.ReadContentAsString());
            
            r.ReadStartElement();

            if (isEmpty)
                return;

            while (r.NodeType == XmlNodeType.Element)
            {
                VideoFilterType type = VideoFilterFactory.GetFilterType(r.Name);
                if (type == VideoFilterType.None)
                {
                    log.DebugFormat("Unsupported video filter: {0}", r.Name);
                    r.ReadOuterXml();
                    continue;
                }

                videoFilters[type].ReadData(r);
            }

            r.ReadEndElement();
        }
        #endregion

        #endregion

        #region Lower level Helpers
        private void ResetCoreContent()
        {
            // Semi reset: we keep Image size and AverageTimeStampsPerFrame
            trackabilityManager.Clear();
            keyframes.Clear();
            ClearTracking();
            trackManager.Clear();
            chronoManager.Clear();

            // Keep the singletons but delete their children if any.
            foreach (AbstractDrawing d in singletonDrawingsManager.Drawings)
            {
                if (d is AbstractMultiDrawing)
                    ((AbstractMultiDrawing)d).Clear();
            }

            magnifier.ResetData();
            imageTransform.Reset();
            drawingCoordinateSystem.Visible = false;
            drawingTestGrid.Visible = false;
            
            // Do not reset the calibration when loading new files in the same screen.
            // The existing calibration is as good the default one.
            // This supports the scenario of setting up the calibration in a dedicated video and using it in all videos of the same folder,
            // without having to save and load a dedicated KVA file for it.
            // If the new file has its own calibration in the KVA it will still be loaded correctly later.
            
            ResetVideoFilters();
            
            ImageAspect = ImageAspectRatio.Auto;
            ImageRotation = ImageRotation.Rotate0;
            Mirrored = false;
            Demosaicing = Demosaicing.None;
            Deinterlacing = false;

            DeselectAll();
        }
        private bool DrawingsHitTest(int keyFrameIndex, PointF mouseLocation, long timestamp)
        {
            // Look for a hit in all drawings of a particular Key Frame.
            // Important side effect : the drawing being hit becomes Selected. This is then used for right click menu.

            if (keyframes.Count == 0)
                return false;

            bool isOnDrawing = false;
            Keyframe keyframe = keyframes[keyFrameIndex];

            DeselectAll();
            
            int currentDrawing = 0;
            int hitResult = -1;
            while (hitResult < 0 && currentDrawing < keyframe.Drawings.Count)
            {
                AbstractDrawing drawing = keyframe.Drawings[currentDrawing];
                hitResult = drawing.HitTest(mouseLocation, timestamp, calibrationHelper.DistortionHelper, imageTransform, imageTransform.Zooming);
                
                if (hitResult < 0)
                {
                    currentDrawing++;
                    continue;
                }

                isOnDrawing = true;
                SelectDrawing(drawing);
                SelectKeyframe(keyframe);
                SelectManager(keyframe);
            }

            return isOnDrawing;
        }
        private void SetDrawingName(AbstractDrawing drawing)
        {
            // Use a unique name based on drawing type.
            string toolDisplayName = drawing.ToolDisplayName;
            int index = 1;
            bool done = false;
            string name = "";
            while (!done)
            {
                name = string.Format("{0} {1}", toolDisplayName, index);
                if (!IsNameTaken(name))
                    break;

                index++;
            }

            drawing.Name = name;
        }
        private bool IsNameTaken(string name)
        {
            // Lookup all drawing names to find a match.
            return AttachedDrawings().Any(d => d.Name == name) || 
                   chronoManager.Drawings.Any(d => d.Name == name) ||
                   trackManager.Drawings.Any(d => d.Name == name);
        }
        private int GetKeyframesContentHash()
        {
            int hash = 0;
            foreach (Keyframe kf in keyframes)
                hash ^= kf.ContentHash;

            return hash;
        }
        private int GetSingletonDrawingsContentHash()
        {
            int hash = 0;
            foreach (AbstractDrawing chrono in chronoManager.Drawings)
                hash ^= chrono.ContentHash;

            foreach (DrawingTrack track in trackManager.Drawings)
                hash ^= track.ContentHash;

            foreach (AbstractDrawing d in singletonDrawingsManager.Drawings)
                hash ^= d.ContentHash;

            return hash;
        }

        private int GetVideoFiltersContentHash()
        {
            int hash = 0;
            hash ^= activeVideoFilterType.GetHashCode();
            foreach (IVideoFilter filter in videoFilters.Values)
                hash ^= filter.ContentHash;

            return hash;
        }
        private void CreateSingletonDrawings()
        {
            // Add the singleton drawings.
            // These drawings are unique and not attached to any particular key image.
            
            drawingSpotlight = new DrawingSpotlight();
            drawingNumberSequence = new DrawingNumberSequence(ToolManager.GetStylePreset("NumberSequence"));
            drawingCoordinateSystem = new DrawingCoordinateSystem(Point.Empty, ToolManager.GetStylePreset("CoordinateSystem"));
            drawingTestGrid = new DrawingTestGrid(ToolManager.GetStylePreset("TestGrid"));

            singletonDrawingsManager.AddDrawing(drawingSpotlight);
            singletonDrawingsManager.AddDrawing(drawingNumberSequence);
            singletonDrawingsManager.AddDrawing(drawingCoordinateSystem);
            singletonDrawingsManager.AddDrawing(drawingTestGrid);

            // Additional setup.
            drawingCoordinateSystem.ParentMetadata = this;
            drawingTestGrid.ParentMetadata = this;

            // Handle the children of the spotlight which are trackable.
            drawingSpotlight.TrackableDrawingAdded += (s, e) =>
            {
                if(AddTrackableDrawingCommand != null) 
                    AddTrackableDrawingCommand.Execute(e.TrackableDrawing); 
            };
            
            drawingSpotlight.TrackableDrawingDeleted += (s, e) => DeleteTrackableDrawing(e.TrackableDrawing);
        }
        private void CalibrationHelper_CalibrationChanged(object sender, EventArgs e)
        {
            AfterCalibrationChanged();
        }
        private void AfterCalibrationChanged()
        {
            if (drawingCoordinateSystem.CalibrationHelper == null)
                drawingCoordinateSystem.CalibrationHelper = calibrationHelper;

            drawingCoordinateSystem.UpdateOrigin();
            calibrationChangedTemporizer.Call();
        }
        private void TracksCalibrationChanged()
        {
            foreach (DrawingTrack t in Tracks())
                t.CalibrationChanged();
        }
        private void MeasurableDrawing_ShowMeasurableInfoChanged(object sender, EventArgs<MeasureLabelType> e)
        {
            mesureLabelType = e.Value;
        }

        /// <summary>
        /// Returns the position of the coordinate system origin at the specified time.
        /// </summary>
        private PointF GetCalibrationOrigin(long time)
        {
            if (captureKVA)
                return imageSize.Center();
            
            // When using CalibrationLine and a tracked coordinate system, 
            // this function retrieves the coordinates origin based on the specified time.
            return trackabilityManager.GetLocation(drawingCoordinateSystem, "0", time);
        }

        /// <summary>
        /// Returns the calibration quad at the specified time.
        /// </summary>
        private QuadrilateralF GetCalibrationQuad(long time, CalibratorType calibratorType, Guid calibrationDrawingId)
        {
            if (captureKVA)
                return QuadrilateralF.GetUnitSquare();
            
            if (calibratorType == CalibratorType.None || calibrationDrawingId == Guid.Empty)
                throw new InvalidProgramException();


            QuadrilateralF quadImage = QuadrilateralF.GetUnitSquare();

            // Retrieve the image coordinates of the quad defining the calibration transform at the specified time.
            if (calibratorType == CalibratorType.Plane)
            {
                // Get the corners of the quad.
                PointF a = trackabilityManager.GetLocation(calibrationDrawingId, "0", time);
                PointF b = trackabilityManager.GetLocation(calibrationDrawingId, "1", time);
                PointF c = trackabilityManager.GetLocation(calibrationDrawingId, "2", time);
                PointF d = trackabilityManager.GetLocation(calibrationDrawingId, "3", time);

                quadImage = new QuadrilateralF(a, b, c, d);
            }
            else
            {
                PointF a = trackabilityManager.GetLocation(calibrationDrawingId, "a", time);
                PointF b = trackabilityManager.GetLocation(calibrationDrawingId, "b", time);

                // Create a fake quad just to transport the segment.
                // This will be turned into a real quad based on the calibration axis in the caller.
                quadImage = new QuadrilateralF(a, b, a, b);
            }

            return quadImage;
        }

        private bool HasTrackingData(Guid id)
        {
            return trackabilityManager.HasData(id);
        }

        private void CreateVideoFilters()
        {
            videoFilters.Add(VideoFilterType.Kinogram, VideoFilterFactory.CreateFilter(VideoFilterType.Kinogram, this));
        }

        private void ResetVideoFilters()
        {
            foreach (var filter in videoFilters.Values)
                filter.ResetData();
        }

        private void SetupTempDirectory(Guid id)
        {
            tempFolder = Path.Combine(Software.TempDirectory, id.ToString());
            if (!Directory.Exists(tempFolder))
                Directory.CreateDirectory(tempFolder);
        }
        private void DeleteTempDirectory()
        {
            if (Directory.Exists(tempFolder))
                Directory.Delete(tempFolder, true);
        }
        private void EmptyTempDirectory()
        {
            DeleteAutosaveFile();
        }
        private void DeleteAutosaveFile()
        {
            string autosaveFile = Path.Combine(tempFolder, "autosave.kva");
            if (File.Exists(autosaveFile))
                File.Delete(autosaveFile);
        }
        private void LensCalibrationAsked(object sender, EventArgs e)
        {
            if (CameraCalibrationAsked != null)
                CameraCalibrationAsked(this, EventArgs.Empty);
        }
        #endregion
    }
}
