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
        public TimeCodeBuilder TimeStampsToTimecode
        {
            get { return timeStampsToTimecode; }
        }
        public bool IsDirty
        {
            get 
            {
                int currentHash = GetKeyframesContentHash() ^ GetExtraDrawingsContentHash();

                log.DebugFormat("IsDirty. Content hashes = reference:{0}, current:{1}.",referenceHash, currentHash);
                return currentHash != referenceHash;
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
            //get { return m_iFirstTimeStamp; }
            set { firstTimeStamp = value; }
        }
        public long SelectionStart
        {
            //get { return m_iSelectionStart; }
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
        #endregion

        #region Members
        private TimeCodeBuilder timeStampsToTimecode;
        private ClosestFrameAction showClosestFrameCallback;
        
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
        
        private bool mirrored;
        private bool showingMeasurables;
        private bool initialized;
        
        private string globalTitle = " ";
        private Size imageSize = new Size(0,0);
        private long averageTimeStampsPerFrame = 1;
        private long firstTimeStamp;
        private long selectionStart;
        private int duplicateFactor = 1;
        private int referenceHash;
        private CalibrationHelper calibrationHelper = new CalibrationHelper();
        private CoordinateSystem coordinateSystem = new CoordinateSystem();
        private TrackabilityManager trackabilityManager = new TrackabilityManager();
        
        // Read from XML, used for adapting the data to the current video
        private Size inputImageSize = new Size(0, 0);
        private long inputAverageTimeStampsPerFrame;    // The one read from the XML
        private long inputFirstTimeStamp;
        private long inputSelectionStart;
        private string inputFileName;
        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public Metadata(TimeCodeBuilder _TimeStampsToTimecodeCallback, ClosestFrameAction _ShowClosestFrameCallback)
        { 
            timeStampsToTimecode = _TimeStampsToTimecodeCallback;
            showClosestFrameCallback = _ShowClosestFrameCallback;
            
            calibrationHelper.CalibrationChanged += CalibrationHelper_CalibrationChanged;
            
            CreateStaticExtraDrawings();
            CleanupHash();
            
            log.Debug("Constructing new Metadata object.");
        }
        public Metadata(string _kvaString,  VideoInfo _info, TimeCodeBuilder _TimeStampsToTimecodeCallback, ClosestFrameAction _ShowClosestFrameCallback)
            : this(_TimeStampsToTimecodeCallback, _ShowClosestFrameCallback)
        {
            // Deserialization constructor
            imageSize = _info.AspectRatioSize;
            AverageTimeStampsPerFrame = _info.AverageTimeStampsPerFrame;
            fullPath = _info.FilePath;
            Load(_kvaString, false);
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
            PostDrawingCreationHooks(drawing);
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
                PostDrawingCreationHooks(drawing);
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
            PostDrawingCreationHooks(drawing);
                
            UnselectAll();
        }
        public void AddChrono(DrawingChrono _chrono)
        {
            _chrono.ParentMetadata = this;
            extraDrawings.Add(_chrono);
            hitExtraDrawingIndex = extraDrawings.Count - 1;
        }
        public void AddTrack(DrawingTrack _track, ClosestFrameAction _showClosestFrame, Color _color)
        {
            _track.ParentMetadata = this;
            _track.Status = TrackStatus.Edit;
            _track.ShowClosestFrame = _showClosestFrame;
            _track.MainColor = _color;
            extraDrawings.Add(_track);
            hitExtraDrawingIndex = extraDrawings.Count - 1;
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
            PostDrawingCreationHooks(drawing);
        }
        #endregion
        
        public void PostSetup()
        {
            if(initialized)
                return;

            trackabilityManager.Initialize(imageSize);

            for(int i = 0; i<totalStaticExtraDrawings;i++)
                PostDrawingCreationHooks(extraDrawings[i]);
            
            CleanupHash();
            initialized = true;
        }
        public void Reset()
        {
            // Complete reset. (used when over loading a new video)
            log.Debug("Metadata Reset.");

            globalTitle = "";
            imageSize = new Size(0, 0);
            inputImageSize = new Size(0, 0);
            fullPath = "";
            averageTimeStampsPerFrame = 1;
            firstTimeStamp = 0;
            inputAverageTimeStampsPerFrame = 0;
            inputFirstTimeStamp = 0;

            ResetCoreContent();
            CleanupHash();
        }
        public void ShowCoordinateSystem()
        {
            drawingCoordinateSystem.Visible = true;
        }
        public void UpdateTrajectoriesForKeyframes()
        {
            foreach(DrawingTrack t in Tracks())
                t.IntegrateKeyframes();
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
            if(t != null && t.Status == TrackStatus.Edit)
                t.UpdateTrackPoint(_bmp);
        }
        public void CleanupHash()
        {
            referenceHash = GetKeyframesContentHash() ^ GetExtraDrawingsContentHash();
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
                int hitRes = candidate.HitTest(_MouseLocation, _iTimestamp, coordinateSystem);
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
        
        #region Serialization
        
        #region Reading
        public void Load(string _kva, bool _bIsFile)
        {
            // _kva parameter can either be a file or a string.
            StopAllTracking();
            UnselectAll();
            
            string kva = ConvertIfNeeded(_kva, _bIsFile);
            
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            settings.IgnoreProcessingInstructions = true;
            settings.IgnoreWhitespace = true;
            settings.CloseInput = true;

            XmlReader reader = null;
            if(_bIsFile)
                reader = XmlReader.Create(kva, settings);
            else
               reader = XmlReader.Create(new StringReader(kva), settings);
            
            try
            {
                ReadXml(reader);
            }
            catch(Exception e)
            {
                log.Error("An error happened during the parsing of the KVA metadata");
                log.Error(e);
            }
            finally
            {
                if(reader != null) 
                    reader.Close();
            }
            
            if(calibrationHelper.CalibratorType == CalibratorType.Line && !calibrationHelper.CalibrationByLine_GetIsOriginSet())
            {
                PointF origin = new Point(imageSize.Width / 2, imageSize.Height / 2);
                calibrationHelper.CalibrationByLine_SetOrigin(origin);
            }
            
            UpdateTrajectoriesForKeyframes();
            drawingCoordinateSystem.UpdateOrigin();
        }
        private string ConvertIfNeeded(string _kva, bool _bIsFile)
        {
            // _kva parameter can either be a filepath or the xml string. We return the same kind of string as passed in.
            string result = _kva;
            
            XmlDocument kvaDoc = new XmlDocument();
            if(_bIsFile)
                kvaDoc.Load(_kva);
            else
                kvaDoc.LoadXml(_kva);
            
            string tempFile = Software.SettingsDirectory + "\\temp.kva";
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            
            XmlNode formatNode = kvaDoc.DocumentElement.SelectSingleNode("descendant::FormatVersion");
            double format;
            bool read = double.TryParse(formatNode.InnerText, NumberStyles.Any, CultureInfo.InvariantCulture, out format);
            if(!read)
            {
                log.ErrorFormat("The format couldn't be read. No conversion will be attempted. Read:{0}", formatNode.InnerText);
                return result;
            }
                  
            if(format < 2.0 && format >= 1.3)
            {
                log.DebugFormat("Older format detected ({0}). Starting conversion", format); 
                
                try
                {
                    XslCompiledTransform xslt = new XslCompiledTransform();
                    string stylesheet = Application.StartupPath + "\\xslt\\kva-1.5to2.0.xsl";
                    xslt.Load(stylesheet);
                    
                    if(_bIsFile)
                    {
                        using (XmlWriter xw = XmlWriter.Create(tempFile, settings))
                        {
                            xslt.Transform(kvaDoc, xw);
                        } 
                        result = tempFile;
                    }
                    else
                    {
                        StringBuilder builder = new StringBuilder();
                        using(XmlWriter xw = XmlWriter.Create(builder, settings))
                        {    
                            xslt.Transform(kvaDoc, xw);
                        }
                        result = builder.ToString();
                    }
                    
                    log.DebugFormat("Older format converted.");
                }
                catch(Exception)
                {
                    log.ErrorFormat("An error occurred during KVA conversion. Conversion aborted.", format.ToString());
                }
            }
            else if(format <= 1.2)
            {
                log.ErrorFormat("Format too old ({0}). No conversion will be attempted.", format.ToString());
            }
            
            return result;
        }
        private void ReadXml(XmlReader r)
        {
            log.Debug("Importing Metadata from Kva XML.");

            r.MoveToContent();
            
            if(!(r.Name == "KinoveaVideoAnalysis"))
                return;
            
            r.ReadStartElement();
            r.ReadElementContentAsString("FormatVersion", "");
            
            while(r.NodeType == XmlNodeType.Element)
            {
                switch(r.Name)
                {
                    case "Producer":
                        r.ReadElementContentAsString();
                        break;
                    case "OriginalFilename":
                        inputFileName = r.ReadElementContentAsString();
                        break;
                    case "GlobalTitle":
                        globalTitle = r.ReadElementContentAsString();
                        break;
                    case "ImageSize":
                        Point p = XmlHelper.ParsePoint(r.ReadElementContentAsString());
                        inputImageSize = new Size(p);
                        break;
                    case "AverageTimeStampsPerFrame":
                        inputAverageTimeStampsPerFrame = r.ReadElementContentAsLong();
                        break;
                    case "FirstTimeStamp":
                        inputFirstTimeStamp = r.ReadElementContentAsLong();
                        break;
                    case "SelectionStart":
                        inputSelectionStart = r.ReadElementContentAsLong();
                        break;
                    case "DuplicationFactor":
                        duplicateFactor = r.ReadElementContentAsInt();
                        break;
                    case "Calibration":
                        CalibrationHelper.ReadXml(r);
                        break;
                    case "Keyframes":
                        ParseKeyframes(r);
                        break;
                    case "Tracks":
                        ParseTracks(r);
                        break;
                    case "Chronos":
                        ParseChronos(r);
                        break;
                    case "Spotlights":
                        ParseSpotlights(r);
                        break;
                    case "AutoNumbers":
                        ParseAutoNumbers(r);
                        break;
                    default:
                        // We still need to properly skip the unparsed nodes.
                        string unparsed = r.ReadOuterXml();
                        log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                        break;
                }
            }
            
            r.ReadEndElement();
        }
        private void ParseKeyframes(XmlReader r)
        {
            // TODO: catch empty tag <Keyframes/>.
            
            r.ReadStartElement();
            
            while(r.NodeType == XmlNodeType.Element)
            {
                if(r.Name == "Keyframe")
                {
                    ParseKeyframe(r);
                }
                else
                {
                    string unparsed = r.ReadOuterXml();
                    log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                }
            }
            
            r.ReadEndElement();
        }
        private void ParseKeyframe(XmlReader r)
        {
            // This will not create a fully functionnal Keyframe.
            // Must be followed by a call to PostImportMetadata()
            Keyframe kf = new Keyframe(this);
            
            r.ReadStartElement();
            
            while(r.NodeType == XmlNodeType.Element)
            {
                switch(r.Name)
                {
                    case "Position":
                        int iInputPosition = r.ReadElementContentAsInt();
                        kf.Position = DoRemapTimestamp(iInputPosition, false);
                        break;
                    case "Title":
                        kf.Title = r.ReadElementContentAsString();
                        break;
                    case "Comment":
                        kf.CommentRtf = r.ReadElementContentAsString();
                        break;
                    case "Drawings":
                        ParseDrawings(r, kf);
                        break;
                    default:
                        string unparsed = r.ReadOuterXml();
                        log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                        break;
                }
            }
            
            r.ReadEndElement();
            
            // Merge: insert key frame at the right place or merge drawings if there's already a keyframe.
            bool merged = false;
            for(int i = 0; i<keyframes.Count; i++)
            {
                if(kf.Position < keyframes[i].Position)
                {
                    keyframes.Insert(i, kf);
                    merged = true;
                    break;
                }
                else if(kf.Position == keyframes[i].Position)
                {
                    foreach(AbstractDrawing ad in kf.Drawings)
                    {
                        keyframes[i].Drawings.Add(ad);
                    }
                    merged = true;
                    break;
                }
            }
            
            if(!merged)
            {
                keyframes.Add(kf);
            }
        }
        private void ParseDrawings(XmlReader r, Keyframe _keyframe)
        {
            // TODO: catch empty tag <Drawings/>.
            
            r.ReadStartElement();
            
            while(r.NodeType == XmlNodeType.Element)
            {
                AbstractDrawing drawing = ParseDrawing(r);
                    
                if (drawing != null)
                {
                    _keyframe.Drawings.Insert(0, drawing);
                    _keyframe.Drawings[0].InfosFading.ReferenceTimestamp = _keyframe.Position;
                    _keyframe.Drawings[0].InfosFading.AverageTimeStampsPerFrame = averageTimeStampsPerFrame;
                    PostDrawingCreationHooks(drawing);
                }
            }
            
            r.ReadEndElement();
        }
        private AbstractDrawing ParseDrawing(XmlReader r)
        {
            AbstractDrawing drawing = null;
            
            // Find the right class to instanciate.
            // The class must derive from AbstractDrawing and have the corresponding [XmlType] C# attribute.
            bool drawingRead = false;
            Assembly a = Assembly.GetExecutingAssembly();
            foreach(Type t in a.GetTypes())
            {
                if(t.BaseType == typeof(AbstractDrawing))
                {
                    object[] attributes = t.GetCustomAttributes(typeof(XmlTypeAttribute), false);
                    if(attributes.Length > 0 && ((XmlTypeAttribute)attributes[0]).TypeName == r.Name)
                    {
                        // Verify that the drawing has a constructor with the right parameter list.
                        ConstructorInfo ci = t.GetConstructor(new[] {typeof(XmlReader), typeof(PointF), this.GetType()});
                        
                        if(ci != null)
                        {
                            PointF scaling = GetScaling();
                            
                            // Instanciate the drawing.
                            object[] parameters = new object[]{r, scaling, this};
                            drawing = (AbstractDrawing)Activator.CreateInstance(t, parameters);
                        
                            if(drawing != null)
                               drawingRead = true;
                        }
                       
                        break;
                    }
                }
            }
            
            if(!drawingRead)
            {
                string unparsed = r.ReadOuterXml();
                log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);   
            }
            
            return drawing;
        }
        private void ParseChronos(XmlReader r)
        {
            // TODO: catch empty tag <Chronos/>.
            
            r.ReadStartElement();
            
            while(r.NodeType == XmlNodeType.Element)
            {
                // When we have other Chrono tools (cadence tool), make this dynamic
                // on a similar model than for attached drawings. (see ParseDrawing())
                if(r.Name == "Chrono")
                {
                    DrawingChrono dc = new DrawingChrono(r, GetScaling(), DoRemapTimestamp);
                    
                    if (dc != null)
                        AddChrono(dc);
                }
                else
                {
                    string unparsed = r.ReadOuterXml();
                    log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                }
            }
            
            r.ReadEndElement();
        }
        private void ParseTracks(XmlReader _xmlReader)
        {
             // TODO: catch empty tag <Tracks/>.
            
            _xmlReader.ReadStartElement();
            
            while(_xmlReader.NodeType == XmlNodeType.Element)
            {
                if(_xmlReader.Name == "Track")
                {
                    DrawingTrack trk = new DrawingTrack(_xmlReader, GetScaling(), DoRemapTimestamp, imageSize);
                    
                    if (!trk.Invalid)
                    {
                        AddTrack(trk, showClosestFrameCallback, trk.MainColor);
                        trk.Status = TrackStatus.Interactive;
                    }
                }
                else
                {
                    string unparsed = _xmlReader.ReadOuterXml();
                    log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                }
            }
            
            _xmlReader.ReadEndElement();
        }
        private void ParseSpotlights(XmlReader _xmlReader)
        {
            _xmlReader.ReadStartElement();
            
            while(_xmlReader.NodeType == XmlNodeType.Element)
            {
                if(_xmlReader.Name == "Spotlight")
                {
                    Spotlight spotlight = new Spotlight(_xmlReader, GetScaling(), DoRemapTimestamp, averageTimeStampsPerFrame);
                    spotlightManager.Add(spotlight);
                }
                else
                {
                    string unparsed = _xmlReader.ReadOuterXml();
                    log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                }
            }
            
            _xmlReader.ReadEndElement();
        }
        private void ParseAutoNumbers(XmlReader _xmlReader)
        {
            int index = extraDrawings.IndexOf(autoNumberManager);
            autoNumberManager = new AutoNumberManager(_xmlReader, GetScaling(), DoRemapTimestamp, averageTimeStampsPerFrame);
            extraDrawings.RemoveAt(index);
            extraDrawings.Insert(index, autoNumberManager);
        }
        private PointF GetScaling()
        {
            PointF scaling = new PointF(1.0f, 1.0f);
            if(!imageSize.IsEmpty && !inputImageSize.IsEmpty)
            {
                scaling.X = (float)imageSize.Width / (float)inputImageSize.Width;
                scaling.Y = (float)imageSize.Height / (float)inputImageSize.Height;    
            }
            return scaling;       
        }
        #endregion
        
        #region Writing
        public String ToXmlString(int _iDuplicateFactor)
        {
            // The duplicate factor is used in the context of extreme slow motion (causing the output to be less than 8fps).
            // In that case there is frame duplication and we store this information in the metadata when it is embedded in the file.
            // On input, it will be used to adjust the key images positions.
            // We change the global variable so it can be used during xml export, but it's only temporary.
            // It is possible that an already duplicated clip is further slowed down.
            int memoDuplicateFactor = duplicateFactor;
            duplicateFactor *= _iDuplicateFactor;
            
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = false;
            settings.CloseOutput = true;
            
            StringBuilder builder = new StringBuilder();
            using(XmlWriter w = XmlWriter.Create(builder, settings))
            {
                try
                {
                   WriteXml(w);
                }
                catch(Exception e)
                {
                    log.Error("An error happened during the writing of the kva string");
                    log.Error(e);
                }
            }
            
            duplicateFactor = memoDuplicateFactor;
            
            return builder.ToString();
        }
        public void ToXmlFile(string _file)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.CloseOutput = true;
            
            using(XmlWriter w = XmlWriter.Create(_file, settings))
            {
                try
                {
                   WriteXml(w);
                   CleanupHash();
                }
                catch(Exception e)
                {
                    log.Error("An error happened during the writing of the kva file");
                    log.Error(e);
                }
            }
        }
        private void WriteXml(XmlWriter w)
        {
            // Convert the metadata to XML.
            // The XML Schema for the format should be available in the "tools/Schema/" folder of the source repository.
            
            // The format contains both core infos to deserialize back to Metadata and helpers data for XSLT exports, 
            // so these exports have more user friendly values. (timecode vs timestamps, cm vs pixels, etc.)
            
            // Notes: 
            // Doubles must be written with the InvariantCulture. ("1.52" not "1,52").
            // Booleans must be converted to proper XML Boolean type ("true" not "True").
            
            w.WriteStartElement("KinoveaVideoAnalysis");
            WriteGeneralInformation(w);
            
            // Keyframes
            if (ActiveKeyframes() > 0)
            {
                w.WriteStartElement("Keyframes");
                foreach (Keyframe kf in keyframes.Where(kf => !kf.Disabled))
                {
                    w.WriteStartElement("Keyframe");
                    kf.WriteXml(w);
                    w.WriteEndElement();
                }
                w.WriteEndElement();
            }
            
            WriteChronos(w);
            WriteTracks(w);
            WriteSpotlights(w);
            WriteAutoNumbers(w);
            
            w.WriteEndElement();
        }
        private void WriteChronos(XmlWriter w)
        {
            bool atLeastOne = false;
            foreach(AbstractDrawing ad in extraDrawings)
            {
                DrawingChrono dc = ad as DrawingChrono;
                if(dc != null)
                {
                    if(!atLeastOne)
                    {
                        w.WriteStartElement("Chronos");
                        atLeastOne = true;
                    }
                    
                    w.WriteStartElement("Chrono");
                    dc.WriteXml(w);
                    w.WriteEndElement();
                }
            }
            if(atLeastOne)
            {
                w.WriteEndElement();
            }
        }
        private void WriteTracks(XmlWriter w)
        {
            bool atLeastOne = false;
            foreach(AbstractDrawing ad in extraDrawings)
            {
                DrawingTrack trk = ad as DrawingTrack;
                if(trk != null)
                {
                    if(!atLeastOne)
                    {
                        w.WriteStartElement("Tracks");
                        atLeastOne = true;
                    }
                    
                    w.WriteStartElement("Track");
                    trk.WriteXml(w);
                    w.WriteEndElement();
                }
            }
            if(atLeastOne)
            {
                w.WriteEndElement();
            }
        }
        private void WriteSpotlights(XmlWriter w)
        {
            if(spotlightManager.Count == 0)
                return;
            
            w.WriteStartElement("Spotlights");
            spotlightManager.WriteXml(w);
            w.WriteEndElement();
        }
        private void WriteAutoNumbers(XmlWriter w)
        {
            if(autoNumberManager.Count == 0)
                return;
            
            w.WriteStartElement("AutoNumbers");
            autoNumberManager.WriteXml(w);
            w.WriteEndElement();
        }
          
        private void WriteGeneralInformation(XmlWriter w)
        {
            w.WriteElementString("FormatVersion", "2.0");
            w.WriteElementString("Producer", Software.ApplicationName + "." + Software.Version);
            w.WriteElementString("OriginalFilename", Path.GetFileNameWithoutExtension(fullPath));
            
            if(!string.IsNullOrEmpty(globalTitle))
                w.WriteElementString("GlobalTitle", globalTitle);
            
            w.WriteElementString("ImageSize", imageSize.Width + ";" + imageSize.Height);
            w.WriteElementString("AverageTimeStampsPerFrame", averageTimeStampsPerFrame.ToString());
            w.WriteElementString("FirstTimeStamp", firstTimeStamp.ToString());
            w.WriteElementString("SelectionStart", selectionStart.ToString());
            
            if(duplicateFactor > 1)
                w.WriteElementString("DuplicationFactor", duplicateFactor.ToString());
            
            // Calibration
            WriteCalibrationHelp(w);
        }
        private void WriteCalibrationHelp(XmlWriter w)
        {
            w.WriteStartElement("Calibration");
            CalibrationHelper.WriteXml(w);
            w.WriteEndElement();
        }
        #endregion
        
        #endregion
        
        public void Export(string _filePath, MetadataExportFormat _format)
        {
            switch(_format)
            {
                case MetadataExportFormat.ODF:
                    ExporterODF exporterODF = new ExporterODF();
                    exporterODF.Export(_filePath, this);
                    break;
                    
                case MetadataExportFormat.MSXML:
                    ExporterMSXML exporterMSXML = new ExporterMSXML();
                    exporterMSXML.Export(_filePath, this);
                    break;
                 case MetadataExportFormat.XHTML:
                    ExporterXHTML exporterXHTML = new ExporterXHTML();
                    exporterXHTML.Export(_filePath, this);
                    break;
                 case MetadataExportFormat.TrajectoryText:
                    ExporterTrajectoryText exporterTrajText = new ExporterTrajectoryText();
                    exporterTrajText.Export(_filePath, this);
                    break;
            }
        }
   
        #region Lower level Helpers
        public long DoRemapTimestamp(long _iInputTimestamp, bool bRelative)
        {
            //-----------------------------------------------------------------------------------------
            // In the general case:
            // The Input position was stored as absolute position, in the context of the original video.
            // It must be adapted in several ways:
            //
            // 1. Timestamps (TS) of first frames may differ.
            // 2. A selection might have been in place, 
            //      in that case we use relative TS if different file and absolute TS if same file.
            // 3. TS might be expressed in completely different timebase.
            //
            // In the specific case of trajectories, the individual positions are stored relative to 
            // the start of the trajectory.
            //-----------------------------------------------------------------------------------------

            // Vérifier qu'en arrivant ici on a bien : 
            // le nom du fichier courant, 
            // (on devrait aussi avoir le first ts courant mais on ne l'a pas.

            // le nom du fichier d'origine, le first ts d'origine, le ts de début  de selection d'origine.

            long iOutputTimestamp = 0;

            if (inputAverageTimeStampsPerFrame != 0)
            {
                if ((inputFirstTimeStamp != firstTimeStamp) ||
                      (inputAverageTimeStampsPerFrame != averageTimeStampsPerFrame) ||
                      (inputFileName != Path.GetFileNameWithoutExtension(fullPath)))
                {
                    //----------------------------------------------------
                    // Different contexts or different files.
                    // We use the relative positions and adapt the context
                    //----------------------------------------------------

                    // 1. Translate the input position into frame number (subject to rounding error)
                    // 2. Translate the frame number back into output position.
                    int iFrameNumber;

                    if (bRelative)
                    {
                        iFrameNumber = (int)(_iInputTimestamp / inputAverageTimeStampsPerFrame);
                        iFrameNumber *= duplicateFactor;
                        iOutputTimestamp = (int)(iFrameNumber * averageTimeStampsPerFrame);
                    }
                    else
                    {
                        if (inputSelectionStart - inputFirstTimeStamp > 0)
                        {
                            // There was a selection.
                            iFrameNumber = (int)((_iInputTimestamp - inputSelectionStart) / inputAverageTimeStampsPerFrame);
                            iFrameNumber *= duplicateFactor;
                        }
                        else
                        {
                            iFrameNumber = (int)((_iInputTimestamp - inputFirstTimeStamp) / inputAverageTimeStampsPerFrame);
                            iFrameNumber *= duplicateFactor;
                        }
                        
                        iOutputTimestamp = (int)(iFrameNumber * averageTimeStampsPerFrame) + firstTimeStamp;
                    }
                }
                else
                {
                    //--------------------
                    // Same context.
                    //--------------------
                    iOutputTimestamp = _iInputTimestamp;
                }
            }
            else
            {
                // hmmm ?
                iOutputTimestamp = _iInputTimestamp;
            }
            
            return iOutputTimestamp;
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

            bool hit = false;
            Keyframe keyframe = keyframes[keyFrameIndex];
            hitDrawingFrameIndex = -1;
            hitDrawingIndex = -1;
            hitDrawing = null;
            
            int drawingIndex = 0;
            int hitRes = -1;
            while (hitRes < 0 && drawingIndex < keyframe.Drawings.Count)
            {
                AbstractDrawing drawing = keyframe.Drawings[drawingIndex];
                hitRes = drawing.HitTest(mouseLocation, timestamp, coordinateSystem);
                if (hitRes >= 0)
                {
                    hit = true;
                    hitDrawingIndex = drawingIndex;
                    hitDrawingFrameIndex = keyFrameIndex;
                    hitDrawing = drawing;
                }
                else
                {
                    drawingIndex++;
                }
            }

            return hit;
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
            drawingCoordinateSystem = new DrawingCoordinateSystem(new Point(-1,-1), ToolManager.CoordinateSystem.StylePreset.Clone());
            
            extraDrawings.Add(spotlightManager);
            extraDrawings.Add(autoNumberManager);
            extraDrawings.Add(drawingCoordinateSystem);
            
            // m_iStaticExtraDrawings is used to differenciate between static extra drawings
            // like multidrawing managers and dynamic extra drawings like tracks and chronos.
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
            foreach (DrawingTrack t in Tracks())
                t.CalibrationChanged();
        }
        private void MeasurableDrawing_ShowMeasurableInfoChanged(object sender, EventArgs e)
        {
            showingMeasurables = !showingMeasurables;
        }
        private void PostDrawingCreationHooks(AbstractDrawing drawing)
        {
            // When passing here, it is possible that the drawing has already been initialized.
            // (for example, when undeleting a drawing).
            
            if(drawing is IScalable)
                ((IScalable)drawing).Scale(this.ImageSize);
            
            if(drawing is ITrackable && AddTrackableDrawingCommand != null)
                AddTrackableDrawingCommand.Execute(drawing as ITrackable);
            
            if(drawing is IMeasurable)
            {
                IMeasurable measurableDrawing = drawing as IMeasurable;
                measurableDrawing.CalibrationHelper = calibrationHelper;
                
                if(!measurableDrawing.ShowMeasurableInfo)
                    measurableDrawing.ShowMeasurableInfo = showingMeasurables;
                
                measurableDrawing.ShowMeasurableInfoChanged += MeasurableDrawing_ShowMeasurableInfoChanged;
            }            
        }
        #endregion
    }
}
