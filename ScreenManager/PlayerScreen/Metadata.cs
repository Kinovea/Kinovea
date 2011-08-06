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

using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Zip;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{

    public delegate long DelegateRemapTimestamp(long _iInputTimestamp, bool bRelative);
    public delegate string GetTimeCode(long _iTimestamp, TimeCodeFormat _timeCodeFormat, bool _bSynched);

    public class Metadata
    {
        #region Properties
        
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
            set 
            { 
                m_ImageSize.Width   = value.Width;
                m_ImageSize.Height  = value.Height; 
            }
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
            	// (grids, magnifier).
            	bool hasData =  
            		(m_Keyframes.Count != 0) ||
            		(m_ExtraDrawings.Count > m_iStaticExtraDrawings) || 
            		//m_Plane.Visible || 
            		//m_Grid.Visible || 
            		(m_Magnifier.Mode != MagnifierMode.NotVisible);
            	return hasData;
            }
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
        public Magnifier Magnifier
        {
        	get { return m_Magnifier;}
        	set { m_Magnifier = value;}
        }
        public bool Mirrored
        {
            get { return m_Mirrored; }
            set { m_Mirrored = value; }
        }
        
        // General infos
        public Int64 AverageTimeStampsPerFrame
        {
            get { return m_iAverageTimeStampsPerFrame; }
            set { m_iAverageTimeStampsPerFrame = value;}
        }
        public Int64 FirstTimeStamp
        {
            //get { return m_iFirstTimeStamp; }
            set { m_iFirstTimeStamp = value; }
        }
        public Int64 SelectionStart
        {
            //get { return m_iSelectionStart; }
            set { m_iSelectionStart = value; }
        }         
		public CalibrationHelper CalibrationHelper 
		{
			get { return m_CalibrationHelper; }
			set { m_CalibrationHelper = value; }
		}
        #endregion

        #region Members
        public GetTimeCode m_TimeStampsToTimecodeCallback;      // Public because accessed from DrawingChrono.
        private ShowClosestFrame m_ShowClosestFrameCallback;
        
        private PreferencesManager m_PrefManager = PreferencesManager.Instance();
        private string m_FullPath;
        
        private List<Keyframe> m_Keyframes = new List<Keyframe>();
        private int m_iSelectedDrawingFrame = -1;
        private int m_iSelectedDrawing = -1;
        
        // Drawings not attached to any key image.
        private List<AbstractDrawing> m_ExtraDrawings = new List<AbstractDrawing>();
        private int m_iSelectedExtraDrawing = -1;
        private int m_iStaticExtraDrawings;			// TODO: might be removed when even Chronos and tracks are represented by a single manager object.

        private Magnifier m_Magnifier = new Magnifier();
        
        private bool m_Mirrored;
        
        private string m_GlobalTitle = " ";
        private Size m_ImageSize = new Size(0,0);
        private Int64 m_iAverageTimeStampsPerFrame = 1;
        private Int64 m_iFirstTimeStamp;
        private Int64 m_iSelectionStart;
        private int m_iDuplicateFactor = 1;
        private int m_iLastCleanHash;
        private CalibrationHelper m_CalibrationHelper = new CalibrationHelper();
		private CoordinateSystem m_CoordinateSystem = new CoordinateSystem();
        
        // Read from XML, used for adapting the data to the current video
        private Size m_InputImageSize = new Size(0, 0);
        private Int64 m_iInputAverageTimeStampsPerFrame;    // The one read from the XML
        private Int64 m_iInputFirstTimeStamp;
        private Int64 m_iInputSelectionStart;
        private string m_InputFileName;
        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public Metadata(GetTimeCode _TimeStampsToTimecodeCallback, ShowClosestFrame _ShowClosestFrameCallback)
        { 
            m_TimeStampsToTimecodeCallback = _TimeStampsToTimecodeCallback;
            m_ShowClosestFrameCallback = _ShowClosestFrameCallback;
           
            InitExtraDrawingTools();
            
            log.Debug("Constructing new Metadata object.");
            CleanupHash();
        }
        public Metadata(string _kvaString,  int _iWidth, int _iHeight, long _iAverageTimestampPerFrame, String _FullPath, GetTimeCode _TimeStampsToTimecodeCallback, ShowClosestFrame _ShowClosestFrameCallback)
            : this(_TimeStampsToTimecodeCallback, _ShowClosestFrameCallback)
		{
            // Deserialization constructor
            m_ImageSize = new Size(_iWidth, _iHeight);
            AverageTimeStampsPerFrame = _iAverageTimestampPerFrame;
            m_FullPath = _FullPath;
                
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
        
        public void AddChrono(DrawingChrono _chrono)
        {
        	_chrono.ParentMetadata = this;
        	m_ExtraDrawings.Add(_chrono);
        	m_iSelectedExtraDrawing = m_ExtraDrawings.Count - 1;
        }
        public void AddTrack(Track _track, ShowClosestFrame _showClosestFrame, Color _color)
        {
        	_track.ParentMetadata = this;
        	_track.Status = Track.TrackStatus.Edit;
        	_track.m_ShowClosestFrame = _showClosestFrame;
        	_track.MainColor = _color;
        	m_ExtraDrawings.Add(_track);
        	m_iSelectedExtraDrawing = m_ExtraDrawings.Count - 1;
        }
        public bool HasTrack()
        {
        	// Used for file menu to know if we can export to text.
        	bool hasTrack = false;
        	foreach(AbstractDrawing ad in m_ExtraDrawings)
        	{
        		if(ad is Track)
        		{
        			hasTrack = true;
        			break;
        		}
        	}
        	return hasTrack;
        }
        
        public void Reset()
        {
            // Complete reset. (used when over loading a new video)
			log.Debug("Metadata Reset.");
			
            m_GlobalTitle = "";
            m_ImageSize = new Size(0, 0);
            m_InputImageSize = new Size(0, 0);
            if (m_FullPath != null)
            {
                if (m_FullPath.Length > 0)
                {
                    m_FullPath = "";
                }
            }
            m_iAverageTimeStampsPerFrame = 1;
            m_iFirstTimeStamp = 0;
            m_iInputAverageTimeStampsPerFrame = 0;
            m_iInputFirstTimeStamp = 0;

            ResetCoreContent();
            CleanupHash();
        }
        public void UpdateTrajectoriesForKeyframes()
        {
            // Called when keyframe added, removed or title changed
            // => Updates the trajectories.
            foreach (AbstractDrawing ad in m_ExtraDrawings)
            {
            	Track t = ad as Track;
            	if(t != null)
            	{
            		t.IntegrateKeyframes();
            	}
            }
        }
        public void AllDrawingTextToNormalMode()
        {
            foreach (Keyframe kf in m_Keyframes)
            {
                foreach (AbstractDrawing ad in kf.Drawings)
                {
                    if (ad is DrawingText)
                    {
                        ((DrawingText)ad).EditMode = false;
                    }
                }
            }
        }
        public void StopAllTracking()
        {
           foreach (AbstractDrawing ad in m_ExtraDrawings)
            {
            	Track t = ad as Track;
            	if(t != null)
            	{
            		t.StopTracking();
            	}
            }
        }
        public void UpdateTrackPoint(Bitmap _bmp)
        {
        	// Happens when mouse up and editing a track.
        	if(m_iSelectedExtraDrawing > 0)
        	{
        		Track t = m_ExtraDrawings[m_iSelectedExtraDrawing] as Track;
        		if(t != null && t.Status == Track.TrackStatus.Edit)
        		{
        			t.UpdateTrackPoint(_bmp);
        		}
        	}
        }
        public void CleanupHash()
        {
            m_iLastCleanHash = GetHashCode();
            log.Debug(String.Format("Metadata hash reset. New reference hash is: {0}", m_iLastCleanHash));
        }
        public override int GetHashCode()
        {
            // Combine all fields hashes, using XOR operator.
            //int iHashCode = GetKeyframesHashCode() ^ GetChronometersHashCode() ^ GetTracksHashCode();
            int iHashCode = GetKeyframesHashCode() ^ GetExtraDrawingsHashCode();
            return iHashCode;
        }
        public List<Bitmap> GetFullImages()
        {
        	List<Bitmap> images = new List<Bitmap>();
        	foreach(Keyframe kf in m_Keyframes)
        	{
        		images.Add(kf.FullFrame);
        	}
        	return images;
        }
        
        public void ResizeFinished()
        {
        	// This function is used to trigger an update to drawings and guides that do not 
        	// render in the same way when the user is resizing the window or not.
        	// This is typically used for SVG Drawing, which take a long time to render themselves.
        	foreach(Keyframe kf in m_Keyframes)
        	{
        		foreach(AbstractDrawing d in kf.Drawings)
        		{
        			DrawingSVG svg = d as DrawingSVG;
        			if(svg != null)
        			{
        				svg.ResizeFinished();
        			}
        		}
        	}
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
                    {
                        break;
                    }
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
			
        	AbstractDrawing hitDrawing = null;
			
			for(int i=m_ExtraDrawings.Count-1;i>=0;i--)
            {
				int hitRes = m_ExtraDrawings[i].HitTest(_MouseLocation, _iTimestamp);
            	if(hitRes >= 0)
            	{
            		m_iSelectedExtraDrawing = i;
            		hitDrawing = m_ExtraDrawings[i];
            		break;
            	}
            }
			
			return hitDrawing;
        }
        public void UnselectAll()
        {
            m_iSelectedDrawingFrame = -1;
            m_iSelectedDrawing = -1;
            m_iSelectedExtraDrawing = -1;
        }
        public int[] GetKeyframesZOrder(long _iTimestamp)
        {
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
        	string tempFile = folder + "\\temp.xml";
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
        	string version = r.ReadElementContentAsString("FormatVersion", "");
            // TODO: switch on version.
        	
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
				        m_CalibrationHelper.PixelToUnit = fPixelToUnit;
						break;
					case "LengthUnit":
						TypeConverter enumConverter = TypeDescriptor.GetConverter(typeof(CalibrationHelper.LengthUnits));
                        m_CalibrationHelper.CurrentLengthUnit = (CalibrationHelper.LengthUnits)enumConverter.ConvertFromString(r.ReadElementContentAsString());
						//m_CalibrationHelper.CurrentLengthUnit = (CalibrationHelper.LengthUnits)int.Parse(r.ReadElementContentAsString());
						break;
					case "CoordinatesOrigin":
						// Note: we don't adapt to the destination image size. It makes little sense anyway.
                    	m_CalibrationHelper.CoordinatesOrigin = XmlHelper.ParsePoint(r.ReadElementContentAsString());
						break;
					default:
						string unparsed = r.ReadOuterXml();
						log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
						break;
				}
			}
			
			r.ReadEndElement();
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
                    PointF scaling = new PointF();
                    scaling.X = (float)m_ImageSize.Width / (float)m_InputImageSize.Width;
                    scaling.Y = (float)m_ImageSize.Height / (float)m_InputImageSize.Height;
                    
                    DrawingChrono dc = new DrawingChrono(r, scaling, new DelegateRemapTimestamp(DoRemapTimestamp));
                    
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
                AbstractDrawing ad = ParseDrawing(r);
                    
                if (ad != null)
                {
                    _keyframe.Drawings.Insert(0, ad);
                    _keyframe.Drawings[0].infosFading.ReferenceTimestamp = _keyframe.Position;
                    _keyframe.Drawings[0].infosFading.AverageTimeStampsPerFrame = m_iAverageTimeStampsPerFrame;
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
                            PointF scaling = new PointF();
                            scaling.X = (float)m_ImageSize.Width / (float)m_InputImageSize.Width;
                            scaling.Y = (float)m_ImageSize.Height / (float)m_InputImageSize.Height;
                    
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
        private void ParseTracks(XmlReader _xmlReader)
        {
             // TODO: catch empty tag <Tracks/>.
            
            _xmlReader.ReadStartElement();
            
            while(_xmlReader.NodeType == XmlNodeType.Element)
			{
                // When we have other Chrono tools (cadence tool), make this dynamic
                // on a similar model than for attached drawings. (see ParseDrawing())
                if(_xmlReader.Name == "Track")
				{
                    PointF scaling = new PointF();
                    scaling.X = (float)m_ImageSize.Width / (float)m_InputImageSize.Width;
                    scaling.Y = (float)m_ImageSize.Height / (float)m_InputImageSize.Height;
                    
                    Track trk = new Track(_xmlReader, scaling, new DelegateRemapTimestamp(DoRemapTimestamp), m_ImageSize);
                    
                    if (!trk.Invalid)
                    {
                        AddTrack(trk, m_ShowClosestFrameCallback, trk.MainColor);
                        trk.Status = Track.TrackStatus.Interactive;
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
                foreach (Keyframe kf in m_Keyframes)
                {
                    if (!kf.Disabled)
                    {
                        w.WriteStartElement("Keyframe");
                        kf.WriteXml(w);
                        w.WriteEndElement();
                    }
                }
                w.WriteEndElement();
            }
            
            // Chronos
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
            
            // Tracks
            atLeastOne = false;
            foreach(AbstractDrawing ad in m_ExtraDrawings)
            {
            	Track trk = ad as Track;
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
            
            w.WriteElementString("PixelToUnit", m_CalibrationHelper.PixelToUnit.ToString(CultureInfo.InvariantCulture));
            w.WriteStartElement("LengthUnit");
            w.WriteAttributeString("UserUnitLength", m_CalibrationHelper.GetLengthAbbreviation());
            
            TypeConverter enumConverter = TypeDescriptor.GetConverter(typeof(CalibrationHelper.LengthUnits));
            string unit = enumConverter.ConvertToString((CalibrationHelper.LengthUnits)m_CalibrationHelper.CurrentLengthUnit);
            w.WriteString(unit);

            w.WriteEndElement();
            w.WriteElementString("CoordinatesOrigin", String.Format("{0};{1}", m_CalibrationHelper.CoordinatesOrigin.X, m_CalibrationHelper.CoordinatesOrigin.Y));

            w.WriteEndElement();
        }
        #endregion
        
        #endregion
        
        #region XSLT Export
    	public void Export(string _filePath, MetadataExportFormat _format)
    	{
    		// Get current data as kva XML.
    		string kvaString = ToXmlString(1);
    		
    		if(string.IsNullOrEmpty(kvaString))
    		{
    		    log.Error("Couldn't get metadata string. Aborting export.");
    		    return;
    		}
			
    		// Export the current meta data to spreadsheet doc through XSLT transform.
    		XslCompiledTransform xslt = new XslCompiledTransform();
            XmlDocument kvaDoc = new XmlDocument();
			kvaDoc.LoadXml(kvaString);
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;
				    
    		switch(_format)
    		{
    			case MetadataExportFormat.ODF:
    			{
    		        xslt.Load(Application.StartupPath + "\\xslt\\kva2odf-en.xsl");
    		        ExportODF(_filePath, xslt, kvaDoc, settings);
    				break;
    			}
    			case MetadataExportFormat.MSXML:
				{
    		        xslt.Load(Application.StartupPath + "\\xslt\\kva2msxml-en.xsl");
    		        ExportXSLT(_filePath, xslt, kvaDoc, settings, false);
					break;
				}
    			case MetadataExportFormat.XHTML:
				{
    		        xslt.Load(Application.StartupPath + "\\xslt\\kva2xhtml-en.xsl");
				    settings.OmitXmlDeclaration = true;
				    ExportXSLT(_filePath,  xslt, kvaDoc, settings, false);
    		        break;
				}
    			case MetadataExportFormat.TEXT:
				{
    		        xslt.Load(Application.StartupPath + "\\xslt\\kva2txt-en.xsl");    
    				ExportXSLT(_filePath,  xslt, kvaDoc, null, true);
    		        break;
				}
    			default:
    				break;
    		}
    	}
    	private void ExportODF(string _filePath, XslCompiledTransform _xslt, XmlDocument _xmlDoc, XmlWriterSettings _settings)
    	{
    		// Transform kva to ODF's content.xml 
    		// and packs it into a proper .ods using zip compression.
    		try
			{
	            // Create archive.
	            using (ZipOutputStream zos = new ZipOutputStream(File.Create(_filePath)))
	            {
					zos.UseZip64 = UseZip64.Dynamic;
					
					// Content.xml (where the actual content is.)
					MemoryStream ms = new MemoryStream();
					using (XmlWriter xw = XmlWriter.Create(ms, _settings))
					{
		   				_xslt.Transform(_xmlDoc, xw);
					}
					
					AddODFZipFile(zos, "content.xml", ms.ToArray());
					
					AddODFZipFile(zos, "meta.xml", GetODFMeta());
					AddODFZipFile(zos, "settings.xml", GetODFSettings());
					AddODFZipFile(zos, "styles.xml", GetODFStyles());
					
					AddODFZipFile(zos, "META-INF/manifest.xml", GetODFManifest());
	            }
			}
			catch(Exception ex)
			{
				log.Error("Exception thrown during export to ODF.");
				ReportError(ex);
			}
    	}
    	private byte[] GetODFMeta()
    	{
    		// Return the minimal xml file in a byte array so in can be written to zip.
    		return GetMinimalODF("office:document-meta");
    	}
    	private byte[] GetODFStyles()
    	{
    		// Return the minimal xml file in a byte array so in can be written to zip.
    		return GetMinimalODF("office:document-styles");
    	}
    	private byte[] GetODFSettings()
    	{
    		// Return the minimal xml file in a byte array so in can be written to zip.
    		return GetMinimalODF("office:document-settings");
    	}
    	private byte[] GetMinimalODF(string _element)
    	{
    		// Return the minimal xml data for required files 
    		// in a byte array so in can be written to zip.
    		// A bit trickier than necessary because .NET StringWriter is UTF-16 and we want UTF-8.
    		
    		MemoryStream ms = new MemoryStream();
			XmlTextWriter xmlw = new XmlTextWriter(ms, new System.Text.UTF8Encoding());
			xmlw.Formatting = Formatting.Indented; 
	            
			xmlw.WriteStartDocument();
			xmlw.WriteStartElement(_element);
			xmlw.WriteAttributeString("xmlns", "office", null, "urn:oasis:names:tc:opendocument:xmlns:office:1.0");
			
			xmlw.WriteStartAttribute("office:version");
            xmlw.WriteString("1.1"); 
            xmlw.WriteEndAttribute();
				 
			xmlw.WriteEndElement();
        	xmlw.Flush();
        	xmlw.Close();
        	
        	return ms.ToArray();
    	}
    	private byte[] GetODFManifest()
    	{
    		// Return the minimal manifest.xml in a byte array so it can be written to zip.
    			
    		MemoryStream ms = new MemoryStream();
			XmlTextWriter xmlw = new XmlTextWriter(ms, new System.Text.UTF8Encoding());
			xmlw.Formatting = Formatting.Indented; 
	            
			xmlw.WriteStartDocument();
			xmlw.WriteStartElement("manifest:manifest");
			xmlw.WriteAttributeString("xmlns", "manifest", null, "urn:oasis:names:tc:opendocument:xmlns:manifest:1.0");
			
			// Manifest itself
			xmlw.WriteStartElement("manifest:file-entry");
			xmlw.WriteStartAttribute("manifest:media-type");
			xmlw.WriteString("application/vnd.oasis.opendocument.spreadsheet");
			xmlw.WriteEndAttribute();
			xmlw.WriteStartAttribute("manifest:full-path");
			xmlw.WriteString("/");
			xmlw.WriteEndAttribute();
			xmlw.WriteEndElement();
			
			// Minimal set of files.
			OutputODFManifestEntry(xmlw, "content.xml");
			OutputODFManifestEntry(xmlw, "styles.xml");
			OutputODFManifestEntry(xmlw, "meta.xml");
			OutputODFManifestEntry(xmlw, "settings.xml");
			
			xmlw.WriteEndElement();
        	xmlw.Flush();
        	xmlw.Close();
        	
        	return ms.ToArray();	
    	}
    	private void OutputODFManifestEntry(XmlTextWriter _xmlw, string _file)
    	{
    		_xmlw.WriteStartElement("manifest:file-entry");
			_xmlw.WriteStartAttribute("manifest:media-type");
			_xmlw.WriteString("text/xml");
			_xmlw.WriteEndAttribute();
			_xmlw.WriteStartAttribute("manifest:full-path");
			_xmlw.WriteString(_file);
			_xmlw.WriteEndAttribute();
			_xmlw.WriteEndElement();
    	}
    	private void AddODFZipFile(ZipOutputStream _zos, string _file, byte[] _data)
    	{
    		// Creates an entry in the ODF zip for a specific file, using the specific data.
			ZipEntry entry = new ZipEntry(_file);
			
			//entry.IsUnicodeText = false;
			entry.DateTime = DateTime.Now;
			entry.Size = _data.Length; 
			
			//Crc32 crc = new Crc32();
			//crc.Update(_data);
			//entry.Crc = crc.Value;
			
			_zos.PutNextEntry(entry);
			_zos.Write(_data, 0, _data.Length);
    	}
    	private void AddODFZipDirectory(ZipOutputStream _zos, string _dir)
    	{
    		// Creates an entry in the ODF zip for a specific directory.
    		ZipEntry entry = new ZipEntry(_dir);
    		entry.Size = 0;
    		entry.DateTime = DateTime.Now;
			entry.ExternalFileAttributes = 16;
			
			_zos.PutNextEntry(entry);
    	}
    	private void ExportXSLT(string _filePath, XslCompiledTransform _xslt, XmlDocument _kvaDoc, XmlWriterSettings _settings, bool _text)
    	{
			try
			{
			    if(_text)
			    {
			        using(StreamWriter sw = new StreamWriter(_filePath))
            		{
    		           	_xslt.Transform(_kvaDoc, null, sw);
    				}
			    }
			    else
			    {
			        using (XmlWriter xw = XmlWriter.Create(_filePath, _settings))
			        {
			            _xslt.Transform(_kvaDoc, xw);
    				}    
			    }
			}
			catch(Exception ex)
			{
				log.Error("Exception thrown during spreadsheet export.");
				ReportError(ex);
			}
    	}
    	private void ReportError(Exception ex)
    	{
    		// TODO: Error message the user, so at least he knows something went wrong !
    		log.Error(ex.Message);
			log.Error(ex.Source);
			log.Error(ex.StackTrace);
    	}
    	#endregion
   
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
            m_Keyframes.Clear();
            StopAllTracking();
            m_ExtraDrawings.RemoveRange(m_iStaticExtraDrawings, m_ExtraDrawings.Count - m_iStaticExtraDrawings);
            m_Magnifier.ResetData();
            m_Mirrored = false;
            UnselectAll();
        }
        private bool DrawingsHitTest(int _iKeyFrameIndex, Point _MouseLocation, long _iTimestamp)
        {
            //----------------------------------------------------------
            // Look for a hit in all drawings of a particular Key Frame.
            // The drawing being hit becomes Selected.
            //----------------------------------------------------------
            bool bDrawingHit = false;
            Keyframe kf = m_Keyframes[_iKeyFrameIndex];
            int hitRes = -1;
            int iCurrentDrawing = 0;

            while (hitRes < 0 && iCurrentDrawing < kf.Drawings.Count)
            {
                hitRes = kf.Drawings[iCurrentDrawing].HitTest(_MouseLocation, _iTimestamp);
                if (hitRes >= 0)
                {
                    bDrawingHit = true;
                    m_iSelectedDrawing = iCurrentDrawing;
                    m_iSelectedDrawingFrame = _iKeyFrameIndex;
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
            int iTotalActive = m_Keyframes.Count;

            for (int i = 0; i < m_Keyframes.Count; i++)
            {
                if (m_Keyframes[i].Disabled)
                    iTotalActive--;
            }

            return iTotalActive;
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
            {
                iHashCode ^= ad.GetHashCode();
            }
            return iHashCode;
        }
        private void InitExtraDrawingTools()
        {
        	// Add the static extra drawing tools to the list of drawings.
        	// These drawings are unique and not attached to any particular key image.
        	// It could be proxy drawings, like SpotlightManager.
        	
        	// [0.8.16] - This function currently doesn't do anything as the Grids have been moved to attached drawings.
        	// It is kept nevertheless because it will be needed for SpotlightManager and others.
        	
            //m_ExtraDrawings.Add(m_Plane);
            m_iStaticExtraDrawings = m_ExtraDrawings.Count;
        }
		#endregion
    }

	public enum MetadataExportFormat
	{
		ODF,
		MSXML,
		XHTML,
		TEXT
	}
}
