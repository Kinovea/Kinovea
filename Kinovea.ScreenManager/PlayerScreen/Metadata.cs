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
    /// Main class responsible for KVA import, export, and add/remove drawings.
    /// </summary>
    public class Metadata
    {
        #region Events and commands
        public RelayCommand<ITrackable> AddTrackableDrawingCommand { get; set; }
        public RelayCommand<ITrackable> DeleteTrackableDrawingCommand { get; set; }
        #endregion
        
        #region Properties
        public TimeCodeBuilder TimeCodeBuilder
        {
            get { return timecodeBuilder; }
        }
        public bool IsDirty
        {
            get 
            {
                int currentHash = GetCurrentHash();
                bool dirty = currentHash != referenceHash;
                log.DebugFormat("Dirty:{0}, reference hash:{1}, current:{2}.", dirty.ToString(), referenceHash, currentHash);
                return dirty;
            }
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
        public int SelectedExtraDrawing
        {
            get { return hitExtraDrawingIndex; }
            set { hitExtraDrawingIndex = value; }
        }
        public AbstractDrawing HitDrawing
        {
            get { return hitDrawing;}
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
        private TimeCodeBuilder timecodeBuilder;
        private ClosestFrameDisplayer closestFrameDisplayer;
        
        private string fullPath;
        
        private List<Keyframe> keyframes = new List<Keyframe>();
        private int hitDrawingFrameIndex = -1;
        private int hitDrawingIndex = -1;
        private AbstractDrawing hitDrawing;
        
        // Drawings not attached to any key image.
        private List<AbstractDrawing> extraDrawings = new List<AbstractDrawing>();
        private int hitExtraDrawingIndex = -1;
        private int totalStaticExtraDrawings;           // TODO: might be removed when even Chronos and tracks are represented by a single manager object.
        private Magnifier magnifier = new Magnifier();
        private SpotlightManager spotlightManager;
        private AutoNumberManager autoNumberManager;
        private DrawingCoordinateSystem drawingCoordinateSystem;
        private TrackerParameters lastUsedTrackerParameters;
        
        private bool mirrored;
        private bool showingMeasurables;
        private bool initialized;
        
        private string globalTitle = " ";
        private Size imageSize = new Size(0,0);
        private long averageTimeStampsPerFrame = 1;
        private long firstTimeStamp;
        private long selectionStart;
        private int referenceHash;
        private CalibrationHelper calibrationHelper = new CalibrationHelper();
        private CoordinateSystem coordinateSystem = new CoordinateSystem();
        private TrackabilityManager trackabilityManager = new TrackabilityManager();
        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public Metadata(TimeCodeBuilder timecodeBuilder, ClosestFrameDisplayer closestFrameDisplayer)
        { 
            this.timecodeBuilder = timecodeBuilder;
            this.closestFrameDisplayer = closestFrameDisplayer;
            
            calibrationHelper.CalibrationChanged += CalibrationHelper_CalibrationChanged;
            
            CreateStaticExtraDrawings();
            CleanupHash();
            
            log.Debug("Constructing new Metadata object.");
        }
        public Metadata(string kvaString,  VideoInfo info, TimeCodeBuilder timecodeBuilder, ClosestFrameDisplayer closestFrameDisplayer)
            : this(timecodeBuilder, closestFrameDisplayer)
        {
            imageSize = info.AspectRatioSize;
            AverageTimeStampsPerFrame = info.AverageTimeStampsPerFrame;
            fullPath = info.FilePath;

            MetadataSerializer serializer = new MetadataSerializer();
            serializer.Load(this, kvaString, false);
        }
        #endregion

        #region Public Interface
        
        #region Key images
        public void Clear()
        {
            keyframes.Clear();
        }
        public void Add(Keyframe _kf)
        {
            keyframes.Add(_kf);
        }
        public void Sort()
        {
            keyframes.Sort();
        }
        public void RemoveAt(int _index)
        {
            keyframes.RemoveAt(_index);
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
        
        #region Add/Delete drawings
        public int GetKeyframeIndex(long position)
        {
            for (int i = 0; i < keyframes.Count; i++)
                if (keyframes[i].Position == position)
                    return i;

            return -1;
        }
        public void AddDrawing(AbstractDrawing drawing, int keyframeIndex)
        {
            keyframes[keyframeIndex].AddDrawing(drawing);
            hitDrawingFrameIndex = keyframeIndex;
            hitDrawingIndex = 0;
            hitDrawing = drawing;
            AfterDrawingCreation(drawing);
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
        public void AddChrono(DrawingChrono _chrono)
        {
            _chrono.ParentMetadata = this;
            extraDrawings.Add(_chrono);
            hitExtraDrawingIndex = extraDrawings.Count - 1;
        }
        public void AddTrack(DrawingTrack _track, ClosestFrameDisplayer _showClosestFrame, Color _color)
        {
            _track.ParentMetadata = this;
            _track.Status = TrackStatus.Edit;
            _track.ShowClosestFrame = _showClosestFrame;
            _track.MainColor = _color;
            if (lastUsedTrackerParameters != null)
                _track.TrackerParameters = lastUsedTrackerParameters;
            extraDrawings.Add(_track);
            hitExtraDrawingIndex = extraDrawings.Count - 1;

            _track.TrackerParametersChanged += Track_TrackerParametersChanged;
        }

        private void Track_TrackerParametersChanged(object sender, EventArgs e)
        {
            // Remember last trackerparameters used.
            DrawingTrack track = sender as DrawingTrack;
            if (track == null)
                return;

            lastUsedTrackerParameters = track.TrackerParameters;
        }
        
        public void DeleteHitDrawing()
        {
            // TODO: handle multi drawings and trackable drawings.
            // Create command so we can undo.
            if(hitDrawing == null)
                return;
                
            keyframes[hitDrawingFrameIndex].Drawings.Remove(hitDrawing);
            
            /*
            
            Block of code originally in the screens.
            
            if(drawing is AbstractMultiDrawing)
            {
                IUndoableCommand cds = new CommandDeleteMultiDrawingItem(this, metadataManipulator.Metadata);
                CommandManager cm = CommandManager.Instance();
                cm.LaunchUndoableCommand(cds);
            }
            else
            {
                ITrackable trackable = drawing as ITrackable;
                if(trackable != null && TrackableDrawingDeleted != null)
                    TrackableDrawingDeleted(this, new TrackableDrawingEventArgs(trackable));
                
                IUndoableCommand cdd = new CommandDeleteDrawing(DoInvalidate, m_FrameServer.Metadata, m_FrameServer.Metadata[m_FrameServer.Metadata.SelectedDrawingFrame].Position, m_FrameServer.Metadata.SelectedDrawing);
                CommandManager cm = CommandManager.Instance();
                cm.LaunchUndoableCommand(cdd);
                DoInvalidate();
            }*/
        }
        public void DeleteDrawing(int _frameIndex, int _drawingIndex)
        {
            // TODO: remove hooks.
            keyframes[_frameIndex].Drawings.RemoveAt(_drawingIndex);
            UnselectAll();
        }
        public void DeleteTrackableDrawing(ITrackable drawing)
        {
            // TODO: when removal of all regular drawings is handled here in Metadata, we can set this method to private.
            // We'll also need to unhook from measurableDrawing.ShowMeasurableInfoChanged. 
            trackabilityManager.Remove(drawing);
        }
        public void UndeleteDrawing(int frameIndex, int drawingIndex, AbstractDrawing drawing)
        {
            if(frameIndex >= keyframes.Count)
                return;
            
            keyframes[frameIndex].Drawings.Insert(drawingIndex, drawing);
            hitDrawingFrameIndex = frameIndex;
            hitDrawingIndex = drawingIndex;
            hitDrawing = drawing;
            AfterDrawingCreation(drawing);
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
            CleanupHash();
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
        public void PerformTracking(VideoFrame _current)
        {
            foreach(DrawingTrack t in Tracks())
                if (t.Status == TrackStatus.Edit)
                    t.TrackCurrentPosition(_current);
        }
        public void StopAllTracking()
        {
            foreach(DrawingTrack t in Tracks())
                t.StopTracking();
        }
        public void UpdateTrackPoint(Bitmap _bmp)
        {
            // Happens when mouse up and editing a track.
            if(hitExtraDrawingIndex < 0)
                return;
            
            DrawingTrack t = extraDrawings[hitExtraDrawingIndex] as DrawingTrack;
            if(t != null && (t.Status == TrackStatus.Edit || t.Status == TrackStatus.Configuration))
                t.UpdateTrackPoint(_bmp);
        }
        public void CleanupHash()
        {
            referenceHash = GetCurrentHash();
            log.Debug(String.Format("Metadata content hash reset:{0}.", referenceHash));
        }
        public List<Bitmap> GetFullImages()
        {
            return keyframes.Select(kf => kf.FullFrame).ToList();
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
            hitExtraDrawingIndex = -1;
            hitDrawing = null;
        }
        public void SelectExtraDrawing(AbstractDrawing drawing)
        {
            int index = extraDrawings.FindIndex(d => d == drawing);
            hitExtraDrawingIndex = index;
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
        public void AfterKVAImport()
        {
            foreach (ITrackable drawing in TrackableDrawings())
                trackabilityManager.Assign(drawing);

            trackabilityManager.CleanUnassigned();
            
            AfterCalibrationChanged();
        }

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
        public AbstractDrawing IsOnExtraDrawing(Point _MouseLocation, long _iTimestamp)
        {
            // Check if the mouse is on one of the drawings not attached to any key image.
            // Returns the drawing on which we stand (or null if none), and select it on the way.
            // the caller will then check its type and decide which action to perform.
            
            AbstractDrawing result = null;
            
            for(int i=extraDrawings.Count-1;i>=0;i--)
            {
                AbstractDrawing candidate = extraDrawings[i];
                int hitRes = candidate.HitTest(_MouseLocation, _iTimestamp, coordinateSystem, coordinateSystem.Zooming);
                if(hitRes >= 0)
                {
                    hitExtraDrawingIndex = i;
                    result = candidate;
                    hitDrawing = candidate;
                    break;
                }
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
        private int GetCurrentHash()
        {
            return GetKeyframesContentHash() ^ GetExtraDrawingsContentHash() ^ trackabilityManager.ContentHash;
        }
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
            hitDrawingFrameIndex = -1;
            hitDrawingIndex = -1;
            hitDrawing = null;
            
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
        private int ActiveKeyframes()
        {
            return keyframes.Count(kf => !kf.Disabled);
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
        #endregion
    }
}
