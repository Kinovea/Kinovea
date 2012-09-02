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
    public class Metadata
    {
        #region Events and commands
        public RelayCommand<ITrackable> AddTrackableDrawingCommand { get; set; }
        public RelayCommand<ITrackable> DeleteTrackableDrawingCommand { get; set; }
        #endregion
        
        #region Properties
        public TimeCodeBuilder TimeStampsToTimecode
        {
            get { return m_TimeStampsToTimecode; }
        }
        public bool IsDirty
        {
            get 
            {
            	int iCurrentHash = GetHashCode();
				log.Debug(String.Format("Reading hash for Metadata.IsDirty, Ref Hash:{0}, Current Hash:{1}",m_iLastCleanHash, iCurrentHash));
            	return m_iLastCleanHash != iCurrentHash;
            }
        }
        public string GlobalTitle
        {
            get { return m_GlobalTitle; }
            set { m_GlobalTitle = value; }
        }
        public Size ImageSize
        {
            get { return m_ImageSize; }
            set { m_ImageSize = value; }
        }
        public CoordinateSystem CoordinateSystem
		{
			get { return m_CoordinateSystem; }
		}
        public string FullPath
        {
            get { return m_FullPath; }
            set { m_FullPath = value;}
        }

        public Keyframe this[int index]
        {
            // Indexor
            get { return m_Keyframes[index]; }
            set { m_Keyframes[index] = value; }
        }
        public List<Keyframe> Keyframes
        {
            get { return m_Keyframes; }
        }
        public int Count
        {
            get { return m_Keyframes.Count; }
        }
        public bool HasData
        {
            get 
            {
            	// This is used to know if there is anything to burn on the images when saving.
            	// All kind of objects should be taken into account here, even those
            	// that we currently don't save to the .kva but only draw on the image.
            	// TODO: detect if any extradrawing is dirty.
                return m_Keyframes.Count > 0 ||
                        m_ExtraDrawings.Count > m_iStaticExtraDrawings ||
                        m_Magnifier.Mode != MagnifierMode.None;
            }
        }
        public bool Tracking {
            get { return Tracks().Any(t => t.Status == TrackStatus.Edit) || TrackabilityManager.Tracking; }
        }
        public bool HasTrack {
            get { return m_ExtraDrawings.Any(drawing => drawing is DrawingTrack); }
        }
        public int SelectedDrawingFrame
        {
            get { return m_iSelectedDrawingFrame; }
            set { m_iSelectedDrawingFrame = value; }
        }
        public int SelectedDrawing
        {
            get {return m_iSelectedDrawing; }
            set { m_iSelectedDrawing = value; }
        }
        public List<AbstractDrawing> ExtraDrawings
		{
			get { return m_ExtraDrawings;}
		}
        public int SelectedExtraDrawing
		{
			get { return m_iSelectedExtraDrawing; }
			set { m_iSelectedExtraDrawing = value; }
		}
        public AbstractDrawing HitDrawing
        {
            get { return hitDrawing;}
        }
        public Magnifier Magnifier
        {
        	get { return m_Magnifier;}
        	set { m_Magnifier = value;}
        }
        public SpotlightManager SpotlightManager
        {
            get { return m_SpotlightManager;}
        }
        public AutoNumberManager AutoNumberManager
        {
            get { return m_AutoNumberManager;}
        }
        public bool Mirrored
        {
            get { return m_Mirrored; }
            set { m_Mirrored = value; }
        }
        
        // General infos
        public long AverageTimeStampsPerFrame
        {
            get { return m_iAverageTimeStampsPerFrame; }
            set { m_iAverageTimeStampsPerFrame = value;}
        }
        public long FirstTimeStamp
        {
            //get { return m_iFirstTimeStamp; }
            set { m_iFirstTimeStamp = value; }
        }
        public long SelectionStart
        {
            //get { return m_iSelectionStart; }
            set { m_iSelectionStart = value; }
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
        private TimeCodeBuilder m_TimeStampsToTimecode;
        private ClosestFrameAction m_ShowClosestFrameCallback;
        
        private PreferencesManager m_PrefManager = PreferencesManager.Instance();
        private string m_FullPath;
        
        private List<Keyframe> m_Keyframes = new List<Keyframe>();
        private int m_iSelectedDrawingFrame = -1;
        private int m_iSelectedDrawing = -1;
        private AbstractDrawing hitDrawing;
        
        // Drawings not attached to any key image.
        private List<AbstractDrawing> m_ExtraDrawings = new List<AbstractDrawing>();
        private int m_iSelectedExtraDrawing = -1;
        private int m_iStaticExtraDrawings;			// TODO: might be removed when even Chronos and tracks are represented by a single manager object.

        private Magnifier m_Magnifier = new Magnifier();
        private SpotlightManager m_SpotlightManager;
        private AutoNumberManager m_AutoNumberManager;
        private DrawingCoordinateSystem drawingCoordinateSystem;
        
        private bool m_Mirrored;
        private bool showingMeasurables;
        private bool initialized;
        
        private string m_GlobalTitle = " ";
        private Size m_ImageSize = new Size(0,0);
        private long m_iAverageTimeStampsPerFrame = 1;
        private long m_iFirstTimeStamp;
        private long m_iSelectionStart;
        private int m_iDuplicateFactor = 1;
        private int m_iLastCleanHash;
        private CalibrationHelper calibrationHelper = new CalibrationHelper();
		private CoordinateSystem m_CoordinateSystem = new CoordinateSystem();
		private TrackabilityManager trackabilityManager = new TrackabilityManager();
        
        // Read from XML, used for adapting the data to the current video
        private Size m_InputImageSize = new Size(0, 0);
        private long m_iInputAverageTimeStampsPerFrame;    // The one read from the XML
        private long m_iInputFirstTimeStamp;
        private long m_iInputSelectionStart;
        private string m_InputFileName;
        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public Metadata(TimeCodeBuilder _TimeStampsToTimecodeCallback, ClosestFrameAction _ShowClosestFrameCallback)
        { 
            m_TimeStampsToTimecode = _TimeStampsToTimecodeCallback;
            m_ShowClosestFrameCallback = _ShowClosestFrameCallback;
            
            calibrationHelper.CalibrationChanged += CalibrationHelper_CalibrationChanged;
            
            CreateStaticExtraDrawings();
            
            log.Debug("Constructing new Metadata object.");
            CleanupHash();
        }
        public Metadata(string _kvaString,  VideoInfo _info, TimeCodeBuilder _TimeStampsToTimecodeCallback, ClosestFrameAction _ShowClosestFrameCallback)
            : this(_TimeStampsToTimecodeCallback, _ShowClosestFrameCallback)
		{
            // Deserialization constructor
            m_ImageSize = _info.AspectRatioSize;
            AverageTimeStampsPerFrame = _info.AverageTimeStampsPerFrame;
            m_FullPath = _info.FilePath;
            Load(_kvaString, false);
		}
        #endregion

        #region Public Interface
        
        #region Key images
        public void Clear()
        {
            m_Keyframes.Clear();
        }
        public void Add(Keyframe _kf)
        {
            m_Keyframes.Add(_kf);
        }
        public void Sort()
        {
            m_Keyframes.Sort();
        }
        public void RemoveAt(int _index)
        {
            m_Keyframes.RemoveAt(_index);
        }
        #endregion
        
        #region Filtered iterators
        public IEnumerable<VideoFrame> EnabledKeyframes()
        {
            return m_Keyframes.Where(kf => !kf.Disabled).Select(kf => new VideoFrame(kf.Position, kf.FullFrame));
        }
        public IEnumerable<DrawingTrack> Tracks()
        {
            foreach (AbstractDrawing drawing in m_ExtraDrawings)
                if(drawing is DrawingTrack)
                    yield return (DrawingTrack)drawing;
        }
        public IEnumerable<AbstractDrawing> AttachedDrawings()
        {
            foreach (Keyframe kf in m_Keyframes)
                foreach (AbstractDrawing drawing in kf.Drawings)
                    yield return drawing;
        }
        public IEnumerable<DrawingText> Labels()
        {
            foreach (AbstractDrawing drawing in AttachedDrawings())
                if (drawing is DrawingText)
                    yield return (DrawingText)drawing;
        }
        public IEnumerable<DrawingSVG> SVGs()
        {
            foreach (AbstractDrawing drawing in AttachedDrawings())
                if (drawing is DrawingSVG)
                    yield return (DrawingSVG)drawing;
        }
        #endregion
        
        public void AddChrono(DrawingChrono _chrono)
        {
        	_chrono.ParentMetadata = this;
        	m_ExtraDrawings.Add(_chrono);
        	m_iSelectedExtraDrawing = m_ExtraDrawings.Count - 1;
        }
        public void AddTrack(DrawingTrack _track, ClosestFrameAction _showClosestFrame, Color _color)
        {
        	_track.ParentMetadata = this;
        	_track.Status = TrackStatus.Edit;
        	_track.m_ShowClosestFrame = _showClosestFrame;
        	_track.MainColor = _color;
        	m_ExtraDrawings.Add(_track);
        	m_iSelectedExtraDrawing = m_ExtraDrawings.Count - 1;
        }
        public void AddDrawing(AbstractDrawing drawing, int keyframeIndex)
        {
            m_Keyframes[keyframeIndex].AddDrawing(drawing);
            m_iSelectedDrawingFrame = keyframeIndex;
            m_iSelectedDrawing = 0;
            hitDrawing = drawing;
            
            PostDrawingCreationHooks(drawing);
        }
        public void DeleteTrackableDrawing(ITrackable drawing)
        {
            // TODO: when removal of all regular drawings is handled here in Metadata, we can set this method to private.
            trackabilityManager.Remove(drawing);
        }
        
        public void PostSetup()
        {
            if(!initialized)
            {
                for(int i = 0; i<m_iStaticExtraDrawings;i++)
                    PostDrawingCreationHooks(m_ExtraDrawings[i]);
                initialized = true;
            }
        }
        public void Reset()
        {
            // Complete reset. (used when over loading a new video)
			log.Debug("Metadata Reset.");
			
            m_GlobalTitle = "";
            m_ImageSize = new Size(0, 0);
            m_InputImageSize = new Size(0, 0);
            m_FullPath = "";
            m_iAverageTimeStampsPerFrame = 1;
            m_iFirstTimeStamp = 0;
            m_iInputAverageTimeStampsPerFrame = 0;
            m_iInputFirstTimeStamp = 0;

            ResetCoreContent();
            CleanupHash();
        }
        public void ShowCoordinateSystem()
        {
            drawingCoordinateSystem.Visible = true;
        }
        public void UpdateTrajectoriesForKeyframes()
        {
            // Called when keyframe added, removed or title changed
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
        	if(m_iSelectedExtraDrawing < 0)
        	    return;
        	
        	DrawingTrack t = m_ExtraDrawings[m_iSelectedExtraDrawing] as DrawingTrack;
        	if(t != null && t.Status == TrackStatus.Edit)
                t.UpdateTrackPoint(_bmp);
        }
        public void CleanupHash()
        {
            m_iLastCleanHash = GetHashCode();
            log.Debug(String.Format("Metadata hash reset. New reference hash is: {0}", m_iLastCleanHash));
        }
        public override int GetHashCode()
        {
            // Combine all fields hashes, using XOR operator.
            int iHashCode = GetKeyframesHashCode() ^ GetExtraDrawingsHashCode();
            return iHashCode;
        }
        public List<Bitmap> GetFullImages()
        {
            return m_Keyframes.Select(kf => kf.FullFrame).ToList();
        }
        public void ResizeFinished()
        {
        	// This function can be used to trigger an update to drawings that do not 
        	// render in the same way when the user is resizing the window or not.
            foreach(DrawingSVG svg in SVGs())
                svg.ResizeFinished();
        }
        
        public void DeleteDrawing(int _frameIndex, int _drawingIndex)
        {
            m_Keyframes[_frameIndex].Drawings.RemoveAt(_drawingIndex);
            UnselectAll();
        }
        public void UnselectAll()
        {
            m_iSelectedDrawing = -1;
            m_iSelectedDrawingFrame = -1;
            m_iSelectedExtraDrawing = -1;
            hitDrawing = null;
        }
        public void SelectExtraDrawing(AbstractDrawing drawing)
        {
            int index = m_ExtraDrawings.FindIndex(d => d == drawing);
            m_iSelectedExtraDrawing = index;
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
            bool bDrawingHit = false;

            if (m_PrefManager.DefaultFading.Enabled && m_Keyframes.Count > 0)
            {
                int[] zOrder = GetKeyframesZOrder(_iTimestamp);

                for(int i=0;i<zOrder.Length;i++)
                {
                    bDrawingHit = DrawingsHitTest(zOrder[i], _MouseLocation, _iTimestamp);
                    if (bDrawingHit)
                        break;
                }
            }
            else if (_iActiveKeyframeIndex >= 0)
            {
                // If fading is off, only try the current keyframe (if any)
                bDrawingHit = DrawingsHitTest(_iActiveKeyframeIndex, _MouseLocation, _iTimestamp);
            }

            return bDrawingHit;
        }
        public AbstractDrawing IsOnExtraDrawing(Point _MouseLocation, long _iTimestamp)
        {
        	// Check if the mouse is on one of the drawings not attached to any key image.
        	// Returns the drawing on which we stand (or null if none), and select it on the way.
        	// the caller will then check its type and decide which action to perform.
			
        	AbstractDrawing result = null;
			
			for(int i=m_ExtraDrawings.Count-1;i>=0;i--)
            {
			    AbstractDrawing candidate = m_ExtraDrawings[i];
				int hitRes = candidate.HitTest(_MouseLocation, _iTimestamp);
            	if(hitRes >= 0)
            	{
            		m_iSelectedExtraDrawing = i;
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
            int[] zOrder = new int[m_Keyframes.Count];

            if (m_Keyframes.Count > 0)
            {
                if (_iTimestamp <= m_Keyframes[0].Position)
                {
                    // All key frames are after this position
                    for(int i=0;i<m_Keyframes.Count;i++)
                    {
                        zOrder[i] = i;
                    }
                }
                else if (_iTimestamp > m_Keyframes[m_Keyframes.Count - 1].Position)
                {
                    // All keyframes are before this position
                    for (int i = 0; i < m_Keyframes.Count; i++)
                    {
                        zOrder[i] = m_Keyframes.Count - i - 1;
                    }
                }
                else
                {
                    // Some keyframes are after, some before.
                    // Start at the first kf after this position until the end,
                    // then go backwards from the first kf before this position until the begining.

                    int iCurrentFrame = m_Keyframes.Count;
                    int iClosestNext = m_Keyframes.Count - 1;
                    while (iCurrentFrame > 0)
                    {
                        iCurrentFrame--;
                        if (m_Keyframes[iCurrentFrame].Position >= _iTimestamp)
                        {
                            iClosestNext = iCurrentFrame;
                        }
                        else
                        {
                            break;
                        }
                    }

                    for(int i=iClosestNext;i<m_Keyframes.Count;i++)
                    {
                        zOrder[i - iClosestNext] = i;
                    }
                    for (int i = 0; i < iClosestNext; i++)
                    {
                        zOrder[m_Keyframes.Count - i - 1] = i;
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
            {
                reader = XmlReader.Create(kva, settings);
            }
            else
            {
               reader = XmlReader.Create(new StringReader(kva), settings);
            }
            
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
                if(reader != null) reader.Close();
            }
            
            UpdateTrajectoriesForKeyframes();
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
            
    		string folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Kinovea\\";
        	string tempFile = folder + "\\temp.kva";
        	XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;
            
    		XmlNode formatNode = kvaDoc.DocumentElement.SelectSingleNode("descendant::FormatVersion");
            double format;
    		bool read = double.TryParse(formatNode.InnerText, NumberStyles.Any, CultureInfo.InvariantCulture, out format);
    		if(read)
    		{
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
    		}
    		else
    		{
    		    log.ErrorFormat("The format couldn't be read. No conversion will be attempted. Read:{0}", formatNode.InnerText);
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
                        m_InputFileName = r.ReadElementContentAsString();
						break;
					case "GlobalTitle":
                        m_GlobalTitle = r.ReadElementContentAsString();
						break;
					case "ImageSize":
						Point p = XmlHelper.ParsePoint(r.ReadElementContentAsString());
						m_InputImageSize = new Size(p);
						break;
					case "AverageTimeStampsPerFrame":
						m_iInputAverageTimeStampsPerFrame = r.ReadElementContentAsLong();
						break;
					case "FirstTimeStamp":
						m_iInputFirstTimeStamp = r.ReadElementContentAsLong();
						break;
					case "SelectionStart":
						m_iInputSelectionStart = r.ReadElementContentAsLong();
						break;
                    case "DuplicationFactor":
						m_iDuplicateFactor = r.ReadElementContentAsInt();
						break;
					case "CalibrationHelp":
                        ParseCalibrationHelp(r);
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
        private void ParseCalibrationHelp(XmlReader r)
        {       
            r.ReadStartElement();
            
			while(r.NodeType == XmlNodeType.Element)
			{
				switch(r.Name)
				{
					case "PixelToUnit":
				        double fPixelToUnit = double.Parse(r.ReadElementContentAsString(), CultureInfo.InvariantCulture);
				        calibrationHelper.PixelToUnit = fPixelToUnit;
						break;
					case "LengthUnit":
						TypeConverter enumConverter = TypeDescriptor.GetConverter(typeof(LengthUnits));
                        calibrationHelper.CurrentLengthUnit = (LengthUnits)enumConverter.ConvertFromString(r.ReadElementContentAsString());
						break;
					case "CoordinatesOrigin":
						// Note: we don't adapt to the destination image size. It makes little sense anyway.
                    	calibrationHelper.CoordinatesOrigin = XmlHelper.ParsePoint(r.ReadElementContentAsString());
						break;
					default:
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
			for(int i = 0; i<m_Keyframes.Count; i++)
			{
			    if(kf.Position < m_Keyframes[i].Position)
			    {
			        m_Keyframes.Insert(i, kf);
			        merged = true;
			        break;
			    }
			    else if(kf.Position == m_Keyframes[i].Position)
			    {
			        foreach(AbstractDrawing ad in kf.Drawings)
			        {
			            m_Keyframes[i].Drawings.Add(ad);
			        }
			        merged = true;
			        break;
			    }
			}
			
			if(!merged)
			{
			    m_Keyframes.Add(kf);
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
                    _keyframe.Drawings[0].infosFading.ReferenceTimestamp = _keyframe.Position;
                    _keyframe.Drawings[0].infosFading.AverageTimeStampsPerFrame = m_iAverageTimeStampsPerFrame;
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
                    DrawingTrack trk = new DrawingTrack(_xmlReader, GetScaling(), DoRemapTimestamp, m_ImageSize);
                    
                    if (!trk.Invalid)
                    {
                        AddTrack(trk, m_ShowClosestFrameCallback, trk.MainColor);
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
                    Spotlight spotlight = new Spotlight(_xmlReader, GetScaling(), DoRemapTimestamp, m_iAverageTimeStampsPerFrame);
                    m_SpotlightManager.Add(spotlight);
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
            int index = m_ExtraDrawings.IndexOf(m_AutoNumberManager);
            m_AutoNumberManager = new AutoNumberManager(_xmlReader, GetScaling(), DoRemapTimestamp, m_iAverageTimeStampsPerFrame);
            m_ExtraDrawings.RemoveAt(index);
            m_ExtraDrawings.Insert(index, m_AutoNumberManager);
        }
        private PointF GetScaling()
        {
            PointF scaling = new PointF(1.0f, 1.0f);
            if(!m_ImageSize.IsEmpty && !m_InputImageSize.IsEmpty)
            {
                scaling.X = (float)m_ImageSize.Width / (float)m_InputImageSize.Width;
                scaling.Y = (float)m_ImageSize.Height / (float)m_InputImageSize.Height;    
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
        	int memoDuplicateFactor = m_iDuplicateFactor;
        	m_iDuplicateFactor *= _iDuplicateFactor;
        	
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
			
			m_iDuplicateFactor = memoDuplicateFactor;
            
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
                foreach (Keyframe kf in m_Keyframes.Where(kf => !kf.Disabled))
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
            foreach(AbstractDrawing ad in m_ExtraDrawings)
            {
            	DrawingChrono dc = ad as DrawingChrono;
            	if(dc != null)
            	{
            		if(atLeastOne == false)
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
            foreach(AbstractDrawing ad in m_ExtraDrawings)
            {
            	DrawingTrack trk = ad as DrawingTrack;
            	if(trk != null)
            	{
            		if(atLeastOne == false)
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
            if(m_SpotlightManager.Count == 0)
                return;
            
            w.WriteStartElement("Spotlights");
            m_SpotlightManager.WriteXml(w);
            w.WriteEndElement();
        }
        private void WriteAutoNumbers(XmlWriter w)
        {
            if(m_AutoNumberManager.Count == 0)
                return;
            
            w.WriteStartElement("AutoNumbers");
            m_AutoNumberManager.WriteXml(w);
            w.WriteEndElement();
        }
          
        private void WriteGeneralInformation(XmlWriter w)
        {
            w.WriteElementString("FormatVersion", "2.0");
			w.WriteElementString("Producer", "Kinovea." + PreferencesManager.ReleaseVersion);
			w.WriteElementString("OriginalFilename", Path.GetFileNameWithoutExtension(m_FullPath));
			
			if(!string.IsNullOrEmpty(m_GlobalTitle))
			    w.WriteElementString("GlobalTitle", m_GlobalTitle);
			
			w.WriteElementString("ImageSize", m_ImageSize.Width + ";" + m_ImageSize.Height);
			w.WriteElementString("AverageTimeStampsPerFrame", m_iAverageTimeStampsPerFrame.ToString());
			w.WriteElementString("FirstTimeStamp", m_iFirstTimeStamp.ToString());
			w.WriteElementString("SelectionStart", m_iSelectionStart.ToString());
			
			if(m_iDuplicateFactor > 1)
			    w.WriteElementString("DuplicationFactor", m_iDuplicateFactor.ToString());
			
			// Calibration
			WriteCalibrationHelp(w);
        }
        private void WriteCalibrationHelp(XmlWriter w)
        {
            // TODO: Make Calbrabtion helper responsible for this.
            
            w.WriteStartElement("CalibrationHelp");
            
            w.WriteElementString("PixelToUnit", calibrationHelper.PixelToUnit.ToString(CultureInfo.InvariantCulture));
            w.WriteStartElement("LengthUnit");
            w.WriteAttributeString("UserUnitLength", calibrationHelper.GetLengthAbbreviation());
            
            TypeConverter enumConverter = TypeDescriptor.GetConverter(typeof(LengthUnits));
            string unit = enumConverter.ConvertToString((LengthUnits)calibrationHelper.CurrentLengthUnit);
            w.WriteString(unit);

            w.WriteEndElement();
            w.WriteElementString("CoordinatesOrigin", String.Format("{0};{1}", calibrationHelper.CoordinatesOrigin.X, calibrationHelper.CoordinatesOrigin.Y));

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

            if (m_iInputAverageTimeStampsPerFrame != 0)
            {
                if ((m_iInputFirstTimeStamp != m_iFirstTimeStamp) ||
                      (m_iInputAverageTimeStampsPerFrame != m_iAverageTimeStampsPerFrame) ||
                      (m_InputFileName != Path.GetFileNameWithoutExtension(m_FullPath)))
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
                        iFrameNumber = (int)(_iInputTimestamp / m_iInputAverageTimeStampsPerFrame);
                        iFrameNumber *= m_iDuplicateFactor;
                        iOutputTimestamp = (int)(iFrameNumber * m_iAverageTimeStampsPerFrame);
                    }
                    else
                    {
                        if (m_iInputSelectionStart - m_iInputFirstTimeStamp > 0)
                        {
                            // There was a selection.
                            iFrameNumber = (int)((_iInputTimestamp - m_iInputSelectionStart) / m_iInputAverageTimeStampsPerFrame);
                            iFrameNumber *= m_iDuplicateFactor;
                        }
                        else
                        {
                            iFrameNumber = (int)((_iInputTimestamp - m_iInputFirstTimeStamp) / m_iInputAverageTimeStampsPerFrame);
                            iFrameNumber *= m_iDuplicateFactor;
                        }
                        
                        iOutputTimestamp = (int)(iFrameNumber * m_iAverageTimeStampsPerFrame) + m_iFirstTimeStamp;
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
            m_Keyframes.Clear();
            StopAllTracking();
            m_ExtraDrawings.RemoveRange(m_iStaticExtraDrawings, m_ExtraDrawings.Count - m_iStaticExtraDrawings);
            m_Magnifier.ResetData();
            
            foreach(AbstractDrawing extraDrawing in m_ExtraDrawings)
            {
                if(extraDrawing is AbstractMultiDrawing)
                    ((AbstractMultiDrawing)extraDrawing).Clear();
            }
            
            m_Mirrored = false;
            UnselectAll();
        }
        private bool DrawingsHitTest(int _iKeyFrameIndex, Point _MouseLocation, long _iTimestamp)
        {
            // Look for a hit in all drawings of a particular Key Frame.
            // Important side effect : the drawing being hit becomes Selected. This is then used for right click menu.

            bool bDrawingHit = false;
            Keyframe kf = m_Keyframes[_iKeyFrameIndex];
            int hitRes = -1;
            int iCurrentDrawing = 0;

            while (hitRes < 0 && iCurrentDrawing < kf.Drawings.Count)
            {
                AbstractDrawing drawing = kf.Drawings[iCurrentDrawing];
                hitRes = drawing.HitTest(_MouseLocation, _iTimestamp);
                if (hitRes >= 0)
                {
                    bDrawingHit = true;
                    m_iSelectedDrawing = iCurrentDrawing;
                    m_iSelectedDrawingFrame = _iKeyFrameIndex;
                    
                    hitDrawing = drawing;
                }
                else
                {
                    iCurrentDrawing++;
                }
            }

            return bDrawingHit;
        }
        private int ActiveKeyframes()
        {
            return m_Keyframes.Count(kf => !kf.Disabled);
        }
        private int GetKeyframesHashCode()
        {
            // Keyframes hashcodes are XORed with one another. 
            int iHashCode = 0;
            foreach (Keyframe kf in m_Keyframes)
            {
                iHashCode ^= kf.GetHashCode();
            }
            return iHashCode;    
        }
        private int GetExtraDrawingsHashCode()
        {
        	int iHashCode = 0;
            foreach (AbstractDrawing ad in m_ExtraDrawings)
                iHashCode ^= ad.GetHashCode();

            return iHashCode;
        }
        private void CreateStaticExtraDrawings()
        {
        	// Add the static extra drawings.
        	// These drawings are unique and not attached to any particular key image.
        	
            m_SpotlightManager = new SpotlightManager();
        	m_AutoNumberManager = new AutoNumberManager(ToolManager.AutoNumbers.StylePreset.Clone());
        	drawingCoordinateSystem = new DrawingCoordinateSystem(new Point(-1,-1), ToolManager.CoordinateSystem.StylePreset.Clone());
        	
        	m_ExtraDrawings.Add(m_SpotlightManager);
        	m_ExtraDrawings.Add(m_AutoNumberManager);
        	m_ExtraDrawings.Add(drawingCoordinateSystem);
        	
        	// m_iStaticExtraDrawings is used to differenciate between static extra drawings
        	// like multidrawing managers and dynamic extra drawings like tracks and chronos.
        	m_iStaticExtraDrawings = m_ExtraDrawings.Count;
        	
        	m_SpotlightManager.TrackableDrawingAdded += (s, e) =>
        	{
        	    if(AddTrackableDrawingCommand != null) 
        	        AddTrackableDrawingCommand.Execute(e.TrackableDrawing); 
        	};
        	
        	m_SpotlightManager.TrackableDrawingDeleted += (s, e) => DeleteTrackableDrawing(e.TrackableDrawing);
        }
		private void CalibrationHelper_CalibrationChanged(object sender, EventArgs e)
        {
            UpdateTrajectoriesForKeyframes();
        }
        private void MeasurableDrawing_ShowMeasurableInfoChanged(object sender, EventArgs e)
        {
            showingMeasurables = !showingMeasurables;
        }
        private void PostDrawingCreationHooks(AbstractDrawing drawing)
        {
            if(drawing is IScalable)
			    ((IScalable)drawing).Scale(this.ImageSize);
			
			if(drawing is ITrackable && AddTrackableDrawingCommand != null)
			    AddTrackableDrawingCommand.Execute(drawing as ITrackable);
            
            if(drawing is IMeasurable)
            {
                IMeasurable measurableDrawing = drawing as IMeasurable;
                measurableDrawing.CalibrationHelper = calibrationHelper;
                measurableDrawing.ShowMeasurableInfo = showingMeasurables;
                measurableDrawing.ShowMeasurableInfoChanged += MeasurableDrawing_ShowMeasurableInfoChanged;
            }
        }
        #endregion
    }
}
