#region License
/*
Copyright © Joan Charmant 2008.
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

using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Zip;
using Kinovea.Services;
using Kinovea.Video;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Main class representing the analysis document.
    /// </summary>
    public class Metadata
    {
        #region Events and commands
        public EventHandler KVAImported;
        public EventHandler<KeyframeEventArgs> KeyframeAdded;
        public EventHandler KeyframeDeleted;
        public EventHandler<DrawingEventArgs> DrawingAdded;
        public EventHandler DrawingDeleted;
        public EventHandler<MultiDrawingItemEventArgs> MultiDrawingItemAdded;
        public EventHandler MultiDrawingItemDeleted;


        public RelayCommand<ITrackable> AddTrackableDrawingCommand { get; set; }
        public RelayCommand<ITrackable> DeleteTrackableDrawingCommand { get; set; }
        #endregion
        
        #region Properties
        public TimeCodeBuilder TimeCodeBuilder
        {
            get { return timecodeBuilder; }
        }
        public HistoryStack HistoryStack
        {
            get { return historyStack; }
        }
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
        public string GlobalTitle
        {
            get { return globalTitle; }
            set { globalTitle = value; }
        }
        public Size ImageSize
        {
            get { return imageSize; }
            set { imageSize = value; }
        }
        public CoordinateSystem CoordinateSystem
        {
            get { return coordinateSystem; }
        }
        public string FullPath
        {
            get { return fullPath; }
            set { fullPath = value;}
        }

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
        public List<Keyframe> Keyframes
        {
            get { return keyframes; }
        }
        public int Count
        {
            get { return keyframes.Count; }
        }
        public bool HasData
        {
            get 
            {
                // This is used to know if there is anything to burn on the images when saving.
                // All kind of objects should be taken into account here, even those
                // that we currently don't save to the .kva but only draw on the image.
                // TODO: detect if any extradrawing is dirty.
                return keyframes.Count > 0 ||
                        extraDrawings.Count > totalStaticExtraDrawings ||
                        chronoManager.Drawings.Count > 0 ||
                        magnifier.Mode != MagnifierMode.None;
            }
        }
        public bool Tracking 
        {
            get { return Tracks().Any(t => t.Status == TrackStatus.Edit) || TrackabilityManager.Tracking; }
        }
        public bool HasTrack 
        {
            get { return extraDrawings.Any(drawing => drawing is DrawingTrack); }
        }
        public bool TextEditingInProgress
        {
            get { return Labels().Any(l => l.Editing); }
        }
        public int SelectedDrawingFrame
        {
            get { return hitDrawingFrameIndex; }
            set { hitDrawingFrameIndex = value; }
        }
        public int SelectedDrawing
        {
            get {return hitDrawingIndex; }
            set { hitDrawingIndex = value; }
        }
        public List<AbstractDrawing> ExtraDrawings
        {
            get { return extraDrawings;}
        }
        public AbstractDrawing HitDrawing
        {
            get { return hitDrawing;}
        }
        public Keyframe HitDrawingKeyframe
        {
            get { return hitDrawingKeyframe; }
        }
        
        public Magnifier Magnifier
        {
            get { return magnifier;}
            set { magnifier = value;}
        }
        public SpotlightManager SpotlightManager
        {
            get { return spotlightManager;}
        }
        public AutoNumberManager AutoNumberManager
        {
            get { return autoNumberManager;}
        }
        public DrawingCoordinateSystem DrawingCoordinateSystem
        {
            get { return drawingCoordinateSystem; }
        }
        public ChronoManager ChronoManager
        {
            get { return chronoManager; }
        }
        public bool Mirrored
        {
            get { return mirrored; }
            set { mirrored = value; }
        }
        
        // General infos
        public long AverageTimeStampsPerFrame
        {
            get { return averageTimeStampsPerFrame; }
            set { averageTimeStampsPerFrame = value;}
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
        public double HighSpeedFactor
        {
            get { return highSpeedFactor; }
            set { highSpeedFactor = value; }
        }
        public CalibrationHelper CalibrationHelper 
        {
            get { return calibrationHelper; }
        }
        public TrackabilityManager TrackabilityManager
        {
            get { return trackabilityManager;}
        }
        public ClosestFrameDisplayer ClosestFrameDisplayer
        {
            get { return closestFrameDisplayer; }
        }
        #endregion

        #region Members
        private Guid id = Guid.NewGuid();
        private TimeCodeBuilder timecodeBuilder;
        private ClosestFrameDisplayer closestFrameDisplayer;
        private bool kvaImporting;
        
        // Folders
        private string fullPath;
        private string tempFolder;
        private AutoSaver autoSaver;

        private HistoryStack historyStack;
        private List<Keyframe> keyframes = new List<Keyframe>();
        private int hitDrawingFrameIndex = -1;
        private int hitDrawingIndex = -1;
        private Keyframe hitDrawingKeyframe;
        private AbstractDrawing hitDrawing;
        
        // Drawings not attached to any key image.
        private List<AbstractDrawing> extraDrawings = new List<AbstractDrawing>();
        private int totalStaticExtraDrawings;           // TODO: might be removed when even Chronos and tracks are represented by a single manager object.
        private Magnifier magnifier = new Magnifier();
        private SpotlightManager spotlightManager;
        private AutoNumberManager autoNumberManager;
        private DrawingCoordinateSystem drawingCoordinateSystem;
        private ChronoManager chronoManager = new ChronoManager();

        private TrackerParameters lastUsedTrackerParameters;
        
        private bool mirrored;
        private bool showingMeasurables;
        private bool initialized;
        
        private string globalTitle;
        private Size imageSize = new Size(0,0);
        private long averageTimeStampsPerFrame = 1;
        private long firstTimeStamp;
        private long selectionStart;
        private long selectionEnd;
        private double highSpeedFactor = 1.0;
        private int referenceHash;
        private CalibrationHelper calibrationHelper = new CalibrationHelper();
        private CoordinateSystem coordinateSystem = new CoordinateSystem();
        private TrackabilityManager trackabilityManager = new TrackabilityManager();
        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public Metadata(HistoryStack historyStack, TimeCodeBuilder timecodeBuilder, ClosestFrameDisplayer closestFrameDisplayer)
        {
            this.historyStack = historyStack;
            this.timecodeBuilder = timecodeBuilder;
            this.closestFrameDisplayer = closestFrameDisplayer;
            
            calibrationHelper.CalibrationChanged += CalibrationHelper_CalibrationChanged;
            
            autoSaver = new AutoSaver(this);
            
            CreateStaticExtraDrawings();
            CleanupHash();

            SetupTempDirectory(id);

            log.Debug("Constructing new Metadata object.");
        }
        public Metadata(string kvaString,  VideoInfo info, HistoryStack historyStack, TimeCodeBuilder timecodeBuilder, ClosestFrameDisplayer closestFrameDisplayer)
            : this(historyStack, timecodeBuilder, closestFrameDisplayer)
        {
            imageSize = info.AspectRatioSize;
            AverageTimeStampsPerFrame = info.AverageTimeStampsPerFrame;
            fullPath = info.FilePath;

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
            UpdateTrajectoriesForKeyframes();
            
            if (KeyframeAdded != null)
                KeyframeAdded(this, new KeyframeEventArgs(keyframe.Id));
        }
        public void DeleteKeyframe(Guid id)
        {
            keyframes.RemoveAll(k => k.Id == id);
            UpdateTrajectoriesForKeyframes();
            
            if (KeyframeDeleted != null)
                KeyframeDeleted(this, new KeyframeEventArgs(id));
        }
        public void EnableDisableKeyframes()
        {
            foreach(Keyframe keyframe in keyframes)
            {
                keyframe.TimeCode = timecodeBuilder(keyframe.Position - selectionStart, TimeType.Time, PreferencesManager.PlayerPreferences.TimecodeFormat, false);
                keyframe.Disabled = keyframe.Position < selectionStart || keyframe.Position > selectionEnd;
            }
        }
        public int GetKeyframeIndex(long position)
        {
            // Temporary function to accomodate clients of the old API where we used indices to reference keyframes and drawings.
            // The only real client of this is PSUI which then use the index internally.
            // Remove this function as soon as PSUI uses the Guid of the frame.
            for (int i = 0; i < keyframes.Count; i++)
                if (keyframes[i].Position == position)
                    return i;

            return -1;
        }
        public Guid GetKeyframeId(int keyframeIndex)
        {
            return keyframes[keyframeIndex].Id;
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
                    {
                        k.Drawings.Add(ad);
                    }

                    processed = true;
                    break;
                }
            }

            if (!processed)
                keyframes.Add(keyframe);
        }
        #endregion
        
        #region Filtered iterators
        public IEnumerable<VideoFrame> EnabledKeyframes()
        {
            return keyframes.Where(kf => !kf.Disabled).Select(kf => new VideoFrame(kf.Position, kf.FullFrame));
        }
        public IEnumerable<DrawingTrack> Tracks()
        {
            foreach (AbstractDrawing drawing in extraDrawings)
                if(drawing is DrawingTrack)
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
        public IEnumerable<ITrackable> TrackableDrawings()
        {
            foreach (AbstractDrawing drawing in extraDrawings)
            {
                if (drawing is ITrackable)
                    yield return (ITrackable)drawing;

                // TODO: multi drawings.
            }

            foreach (AbstractDrawing drawing in AttachedDrawings())
                if (drawing is ITrackable)
                    yield return (ITrackable)drawing;
        }
        #endregion
        
        #region Drawings
        public void AddDrawing(Guid managerId, AbstractDrawing drawing)
        {
            // TODO: check if we could have a generic AddDrawing method taking an AbstractDrawingManager.

            Keyframe keyframe = GetKeyframe(managerId);
            if (keyframe != null)
            {
                AddDrawing(keyframe, drawing);
                return;
            }

            if (chronoManager.Id == managerId && drawing is DrawingChrono)
            {
                AddChrono(drawing as DrawingChrono);
                return;
            }
        }
        public void AddDrawing(Keyframe keyframe, AbstractDrawing drawing)
        {
            keyframe.AddDrawing(drawing);
            drawing.InfosFading.ReferenceTimestamp = keyframe.Position;
            drawing.InfosFading.AverageTimeStampsPerFrame = averageTimeStampsPerFrame;

            // FIXME: remove usage of hitDrawingFrameIndex and hitDrawingIndex.
            hitDrawingFrameIndex = GetKeyframeIndex(keyframe.Id);
            hitDrawingIndex = 0;
            hitDrawingKeyframe = keyframe;
            hitDrawing = drawing;

            AfterDrawingCreation(drawing);

            if (DrawingAdded != null)
                DrawingAdded(this, new DrawingEventArgs(drawing, keyframe.Id));
        }
        public void AddMultidrawingItem(AbstractMultiDrawing manager, AbstractMultiDrawingItem item)
        {
            manager.Add(item);
            SelectExtraDrawing(manager);

            if (MultiDrawingItemAdded != null)
                MultiDrawingItemAdded(this, new MultiDrawingItemEventArgs(item, manager));
        }
        public void AddChrono(DrawingChrono chrono)
        {
            chronoManager.AddDrawing(chrono);
            chrono.ParentMetadata = this;
            
            hitDrawing = chrono;

            AfterDrawingCreation(chrono);

            if (DrawingAdded != null)
                DrawingAdded(this, new DrawingEventArgs(chrono, chronoManager.Id));
        }

        public void AddImageDrawing(string filename, bool isSVG, long time)
        {
            // TODO: Use a drawing tool to do that ?
            
            if(!File.Exists(filename))
                return;
            
            AllDrawingTextToNormalMode();
                
            AbstractDrawing drawing = null;
            if(isSVG)
            {
                try
                {
                    drawing = new DrawingSVG(imageSize.Width, imageSize.Height, time, averageTimeStampsPerFrame, filename);
                }
                catch
                {
                    // An error occurred during the creation. TODO: inform the user.
                    // example : external DTD an no network or invalid svg file.
                }
            }
            else
            {
                drawing = new DrawingBitmap(imageSize.Width, imageSize.Height, time, averageTimeStampsPerFrame, filename);
            }
            
            if(drawing != null)
            {
                keyframes[hitDrawingFrameIndex].AddDrawing(drawing);
                hitDrawingIndex = 0;
                hitDrawing = drawing;
                AfterDrawingCreation(drawing);
            }
            
            UnselectAll();
        }
        public void AddImageDrawing(Bitmap bmp, long time)
        {
            AllDrawingTextToNormalMode();
            DrawingBitmap drawing = new DrawingBitmap(imageSize.Width, imageSize.Height, time, averageTimeStampsPerFrame, bmp);
            
            keyframes[hitDrawingFrameIndex].AddDrawing(drawing);
            hitDrawingIndex = 0;
            hitDrawing = drawing;
            AfterDrawingCreation(drawing);
                
            UnselectAll();
        }
        
        public void AddTrack(DrawingTrack track, ClosestFrameDisplayer closestFrameDisplayer, Color color)
        {
            track.ParentMetadata = this;
            track.Status = TrackStatus.Edit;
            track.ClosestFrameDisplayer = closestFrameDisplayer;
            track.MainColor = color;
            if (lastUsedTrackerParameters != null)
                track.TrackerParameters = lastUsedTrackerParameters;
            extraDrawings.Add(track);

            hitDrawing = track;

            track.TrackerParametersChanged += Track_TrackerParametersChanged;
        }

        public void DeleteDrawing(Guid managerId, Guid drawingId)
        {
            // Remove event handlers from the drawing as well as all associated data like tracking data,
            // and finally remove the drawing itself.

            AbstractDrawingManager manager = null;
            
            if (managerId == chronoManager.Id)
                manager = chronoManager;
            else 
                manager = GetKeyframe(managerId);

            if (manager == null)
                return;
                
            AbstractDrawing drawing = manager.GetDrawing(drawingId);
            if (drawing == null)
                return;

            ITrackable trackableDrawing = drawing as ITrackable;
            if (trackableDrawing != null)
                DeleteTrackableDrawing(trackableDrawing);
            
            IMeasurable measurableDrawing = drawing as IMeasurable;
            if (measurableDrawing != null)
                measurableDrawing.ShowMeasurableInfoChanged -= MeasurableDrawing_ShowMeasurableInfoChanged;

            manager.RemoveDrawing(drawingId);

            UnselectAll();
            
            if (DrawingDeleted != null)
                DrawingDeleted(this, EventArgs.Empty);
        }
        public void DeleteMultiDrawingItem(AbstractMultiDrawing manager, Guid itemId)
        {
            // check if trackable.

            manager.Remove(itemId);
            UnselectAll();

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
        #endregion
        
        public void PostSetup()
        {
            if(initialized)
                return;

            trackabilityManager.Initialize(imageSize);
            calibrationHelper.Initialize(imageSize);

            for(int i = 0; i<totalStaticExtraDrawings;i++)
                AfterDrawingCreation(extraDrawings[i]);
            
            CleanupHash();
            initialized = true;
        }
        public void Reset()
        {
            // Complete reset. (used when over loading a new video)
            log.Debug("Metadata Reset.");

            globalTitle = "";
            imageSize = new Size(0, 0);
            fullPath = "";
            averageTimeStampsPerFrame = 1;
            firstTimeStamp = 0;
            
            ResetCoreContent();
            autoSaver.Stop();
            EmptyTempDirectory();
            CleanupHash();
        }
        public void Close()
        {
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
        public void AllDrawingTextToNormalMode()
        {
            foreach (DrawingText label in Labels())
                label.SetEditMode(false, null);
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
        public void UpdateTrackPoint(Bitmap bitmap)
        {
            // Happens when mouse up and editing a track.
            DrawingTrack t = hitDrawing as DrawingTrack;
            if(t != null && (t.Status == TrackStatus.Edit || t.Status == TrackStatus.Configuration))
                t.UpdateTrackPoint(bitmap);
        }
        public int GetContentHash()
        {
            return GetKeyframesContentHash() ^ GetExtraDrawingsContentHash() ^ trackabilityManager.ContentHash;
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
        public void UnselectAll()
        {
            hitDrawingIndex = -1;
            hitDrawingFrameIndex = -1;

            hitDrawingKeyframe = null;
            hitDrawing = null;
        }
        public void SelectExtraDrawing(AbstractDrawing drawing)
        {
            hitDrawing = drawing;
        }
        public void AfterDrawingCreation(AbstractDrawing drawing)
        {
            // When passing here, it is possible that the drawing has already been initialized.
            // (for example, when undeleting a drawing).

            if (drawing is IScalable)
                ((IScalable)drawing).Scale(this.ImageSize);

            if (drawing is ITrackable && AddTrackableDrawingCommand != null)
                AddTrackableDrawingCommand.Execute(drawing as ITrackable);

            if (drawing is IMeasurable)
            {
                IMeasurable measurableDrawing = drawing as IMeasurable;
                measurableDrawing.CalibrationHelper = calibrationHelper;

                if (!measurableDrawing.ShowMeasurableInfo)
                    measurableDrawing.ShowMeasurableInfo = showingMeasurables;

                measurableDrawing.ShowMeasurableInfoChanged += MeasurableDrawing_ShowMeasurableInfoChanged;
            }
        }

        public void BeforeKVAImport()
        {
            kvaImporting = true;
            StopAllTracking();
            UnselectAll();
        }
        public void AfterKVAImport()
        {
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
        public void Recover(Guid id)
        {
            DeleteTempDirectory();
            SetupTempDirectory(id);
            string autosaveFile = Path.Combine(tempFolder, "autosave.kva");
            if (File.Exists(autosaveFile))
            {
                MetadataSerializer s = new MetadataSerializer();
                s.Load(this, autosaveFile, true);
            }
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
        // For example, Tracks can here be handled with all other ExtraDrawings.
        public bool IsOnDrawing(int _iActiveKeyframeIndex, Point _MouseLocation, long _iTimestamp)
        {
            // Returns whether the mouse is on a drawing attached to a key image.
            bool hit = false;
            
            if (PreferencesManager.PlayerPreferences.DefaultFading.Enabled && keyframes.Count > 0)
            {
                int[] zOrder = GetKeyframesZOrder(_iTimestamp);

                for(int i=0;i<zOrder.Length;i++)
                {
                    hit = DrawingsHitTest(zOrder[i], _MouseLocation, _iTimestamp);
                    if (hit)
                        break;
                }
            }
            else if (_iActiveKeyframeIndex >= 0)
            {
                // If fading is off, only try the current keyframe (if any)
                hit = DrawingsHitTest(_iActiveKeyframeIndex, _MouseLocation, _iTimestamp);
            }

            return hit;
        }
        public AbstractDrawing IsOnExtraDrawing(Point point, long timestamp)
        {
            // Check if the mouse is on one of the drawings not attached to any key image.
            // Returns the drawing on which we stand (or null if none), and select it on the way.
            // the caller will then check its type and decide which action to perform.
            
            AbstractDrawing result = null;

            foreach (DrawingChrono chrono in chronoManager.Drawings)
            {
                int hit = chrono.HitTest(point, timestamp, coordinateSystem, coordinateSystem.Zooming);
                if (hit < 0)
                    continue;

                result = chrono;
                hitDrawing = chrono;
                break;
            }

            if (result != null)
                return result;

            for(int i = extraDrawings.Count - 1; i >= 0; i--)
            {
                AbstractDrawing candidate = extraDrawings[i];
                int hitRes = candidate.HitTest(point, timestamp, coordinateSystem, coordinateSystem.Zooming);
                if (hitRes < 0)
                    continue;
                
                result = candidate;
                hitDrawing = candidate;
                break;
            }
            
            return result;
        }
        public int[] GetKeyframesZOrder(long _iTimestamp)
        {
            // TODO: turn this into an iterator.
            
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
        
        #endregion
   
        #region Lower level Helpers
        private void ResetCoreContent()
        {
            // Semi reset: we keep Image size and AverageTimeStampsPerFrame
            trackabilityManager.Clear();
            keyframes.Clear();
            StopAllTracking();
            extraDrawings.RemoveRange(totalStaticExtraDrawings, extraDrawings.Count - totalStaticExtraDrawings);
            magnifier.ResetData();
            
            foreach(AbstractDrawing extraDrawing in extraDrawings)
            {
                if(extraDrawing is AbstractMultiDrawing)
                    ((AbstractMultiDrawing)extraDrawing).Clear();
            }
            
            mirrored = false;
            UnselectAll();
        }
        private bool DrawingsHitTest(int keyFrameIndex, Point mouseLocation, long timestamp)
        {
            // Look for a hit in all drawings of a particular Key Frame.
            // Important side effect : the drawing being hit becomes Selected. This is then used for right click menu.

            bool isOnDrawing = false;
            Keyframe keyframe = keyframes[keyFrameIndex];

            UnselectAll();
            
            int currentDrawing = 0;
            int hitResult = -1;
            while (hitResult < 0 && currentDrawing < keyframe.Drawings.Count)
            {
                AbstractDrawing drawing = keyframe.Drawings[currentDrawing];
                hitResult = drawing.HitTest(mouseLocation, timestamp, coordinateSystem, coordinateSystem.Zooming);
                
                if (hitResult < 0)
                {
                    currentDrawing++;
                    continue;
                }

                isOnDrawing = true;
                hitDrawingIndex = currentDrawing;
                hitDrawingFrameIndex = keyFrameIndex;
                hitDrawing = drawing;
            }

            return isOnDrawing;
        }
        private int GetKeyframesContentHash()
        {
            int hash = 0;
            foreach (Keyframe kf in keyframes)
                hash ^= kf.ContentHash;

            return hash;
        }
        private int GetExtraDrawingsContentHash()
        {
            int hash = 0;
            foreach (DrawingChrono chrono in chronoManager.Drawings)
                hash ^= chrono.ContentHash;

            foreach (AbstractDrawing ad in extraDrawings)
                hash ^= ad.ContentHash;

            return hash;
        }
        private void CreateStaticExtraDrawings()
        {
            // Add the static extra drawings.
            // These drawings are unique and not attached to any particular key image.
            
            spotlightManager = new SpotlightManager();
            autoNumberManager = new AutoNumberManager(ToolManager.AutoNumbers.StylePreset.Clone());
            drawingCoordinateSystem = new DrawingCoordinateSystem(Point.Empty, ToolManager.CoordinateSystem.StylePreset.Clone());
            
            extraDrawings.Add(spotlightManager);
            extraDrawings.Add(autoNumberManager);
            extraDrawings.Add(drawingCoordinateSystem);

            // totalStaticExtraDrawings is used to differenciate between static extra drawings like multidrawing managers
            // and dynamic extra drawings like tracks and chronos.
            totalStaticExtraDrawings = extraDrawings.Count;
            
            spotlightManager.TrackableDrawingAdded += (s, e) =>
            {
                if(AddTrackableDrawingCommand != null) 
                    AddTrackableDrawingCommand.Execute(e.TrackableDrawing); 
            };
            
            spotlightManager.TrackableDrawingDeleted += (s, e) => DeleteTrackableDrawing(e.TrackableDrawing);
        }
        private void CalibrationHelper_CalibrationChanged(object sender, EventArgs e)
        {
            AfterCalibrationChanged();
        }
        private void AfterCalibrationChanged()
        {
            drawingCoordinateSystem.UpdateOrigin();

            foreach (DrawingTrack t in Tracks())
                t.CalibrationChanged();
        }
        private void MeasurableDrawing_ShowMeasurableInfoChanged(object sender, EventArgs e)
        {
            showingMeasurables = !showingMeasurables;
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
        #endregion
    }
}
