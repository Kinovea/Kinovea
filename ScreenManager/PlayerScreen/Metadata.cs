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
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Xsl;

using Videa.Services;

namespace Videa.ScreenManager
{

    public delegate long DelegateRemapTimestamp(long _iInputTimestamp, bool bRelative);
    public delegate string GetTimeCode(long _iTimestamp, TimeCodeFormat _timeCodeFormat, bool _bSynched);

    public class Metadata
    {
        #region Properties
        public bool Dirty
        {
            get { return m_iLastCleanHash != GetHashCode();}
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
                return ((Count != 0) || (m_Tracks.Count > 0) || (m_Chronos.Count > 0) );
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

        public List<Track> Tracks
        {
            get { return m_Tracks; }
        }
        public int SelectedTrack
        {
            get { return m_iSelectedTrack; }
            set { m_iSelectedTrack = value;}
        }
        public List<DrawingChrono> Chronos
        {
            get { return m_Chronos; }
        }
        public int SelectedChrono
        {
            get { return m_iSelectedChrono; }
            set { m_iSelectedChrono = value; }
        }
        public Plane3D Plane
        {
            get { return m_Plane; }
        }
        public Plane3D Grid
        {
            get { return m_Grid; }
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
            set { m_iAverageTimeStampsPerFrame = value; }
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
		public LineLengthHelper LineLengthHelper 
		{
			get { return m_LineLengthHelper; }
			set { m_LineLengthHelper = value; }
		}
        #endregion

        #region Members
        public GetTimeCode m_TimeStampsToTimecodeCallback;      // Public because accessed from DrawingChrono.
        private ShowClosestFrame m_ShowClosestFrameCallback;
        private PreferencesManager m_PrefManager; 
        private string m_FullPath;
        private List<Keyframe> m_Keyframes;
        private int m_iSelectedDrawingFrame;
        private int m_iSelectedDrawing;
        private List<Track> m_Tracks;
        private int m_iSelectedTrack;
        private List<DrawingChrono> m_Chronos;
        private int m_iSelectedChrono;
        private Plane3D m_Plane;
        private Plane3D m_Grid;
        private bool m_Mirrored;
        private string m_GlobalTitle;
        private Size m_ImageSize;
        private Int64 m_iAverageTimeStampsPerFrame;
        private Int64 m_iFirstTimeStamp;
        private Int64 m_iSelectionStart;
        private LineLengthHelper m_LineLengthHelper = new LineLengthHelper();

        // Read from XML, used for adapting the data to the current video
        private Size m_InputImageSize;
        private Int64 m_iInputAverageTimeStampsPerFrame;    // The one read from the XML
        private Int64 m_iInputFirstTimeStamp;
        private Int64 m_iInputSelectionStart;
        private string m_InputFileName;

        // Clean hash
        private int m_iLastCleanHash;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public Metadata(GetTimeCode _TimeStampsToTimecodeCallback, ShowClosestFrame _ShowClosestFrameCallback)
        {
            log.Debug("Constructing Metadata");
            m_PrefManager = PreferencesManager.Instance();
            m_TimeStampsToTimecodeCallback = _TimeStampsToTimecodeCallback;
            m_ShowClosestFrameCallback = _ShowClosestFrameCallback;
            
            m_Keyframes = new List<Keyframe>();
            m_Tracks = new List<Track>();
            m_Chronos = new List<DrawingChrono>();
            m_Plane = new Plane3D(500, 8, true);
            m_Grid = new Plane3D(500, 8, false);
            
            m_Mirrored = false;
            m_GlobalTitle = " ";
            m_ImageSize = new Size(0, 0);
            m_iAverageTimeStampsPerFrame = 1;
            m_iFirstTimeStamp = 0;
            m_iSelectionStart = 0;

            UnselectAll();
            
            m_iInputAverageTimeStampsPerFrame = 0;
            m_iInputFirstTimeStamp = 0;
            m_InputImageSize = new Size(0, 0);
            m_iInputSelectionStart = 0;
            m_InputFileName = "";

            CleanupHash();
        }
        public static Metadata FromXmlString(String _xmlString, int _iWidth, int _iHeight, long _iAverageTimestampPerFrame, String _FullPath, GetTimeCode _TimeStampsToTimecodeCallback, ShowClosestFrame _ShowClosestFrameCallback)
        {
            Metadata md = new Metadata(_TimeStampsToTimecodeCallback, _ShowClosestFrameCallback);

    		string xmlString = ConvertFormat(_xmlString);
            
            StringReader reader = new StringReader(xmlString);
            XmlTextReader xmlReader = new XmlTextReader(reader);

            // We must set the Image Size for scaling computations.
            md.m_ImageSize.Width = _iWidth;
            md.m_ImageSize.Height = _iHeight;
            md.AverageTimeStampsPerFrame = _iAverageTimestampPerFrame;
            md.m_FullPath = _FullPath;

            md.FromXml(xmlReader);

            if (md.m_Keyframes.Count > 0 && md.m_Tracks.Count > 0)
            {
                foreach (Track t in md.m_Tracks)
                {
                    t.IntegrateKeyframes();
                }
            }

            md.m_Plane.SetLocations(md.m_ImageSize, 1.0, new Point(0, 0));
            md.m_Grid.SetLocations(md.m_ImageSize, 1.0, new Point(0, 0));
            md.CleanupHash();
            
            return md;
        }
        private static string ConvertFormat(string _xmlString)
        {
        	string result;
        	
        	XmlDocument mdDoc = new XmlDocument();
			mdDoc.LoadXml(_xmlString);

    		XmlNode rootNode = mdDoc.DocumentElement;
    		XmlNode formatNode = rootNode.SelectSingleNode("descendant::FormatVersion");

    		double format = double.Parse(formatNode.InnerText, CultureInfo.InvariantCulture);
    		
    		if(format <= 1.2)
    		{
    			log.Debug(String.Format("Old format detected ({0}). Converting to new format.", formatNode.InnerText));
    			
    			try
    			{
	    			// Convert from 1.2 to 1.3.
					XslCompiledTransform xslt = new XslCompiledTransform();
					xslt.Load(@"xslt\kva-1.2to1.3.xsl");
					
					StringWriter writer = new StringWriter();
					XmlTextWriter xmlWriter = new XmlTextWriter(writer);
	
					xslt.Transform(mdDoc, xmlWriter);
	    			
					result = writer.ToString();
    			}
    			catch(Exception)
    			{
    				result = _xmlString;
    			}
    		}
    		else
    		{
    			result = _xmlString;
    		}
    		
        	return result;
        }
        #endregion

        #region Public Interface
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
        public String ToXmlString()
        {
            StringWriter writer = new StringWriter();
            
            // No pretty print here.
            XmlTextWriter xmlWriter = new XmlTextWriter(writer);
            ToXml(xmlWriter);

            return writer.ToString();
        }
        public void ToXmlFile(string _file)
        {
            XmlTextWriter xmlWriter = new XmlTextWriter(_file, Encoding.UTF8);
            xmlWriter.Formatting = Formatting.Indented;
            ToXml(xmlWriter);
        }
        public void LoadFromFile(string _file)
        {
            // TODO - merge mechanics

            ResetCoreContent();

            m_FullPath = _file;

            XmlTextReader xmlReader = new XmlTextReader(_file);
            FromXml(xmlReader);

            if (m_Tracks.Count > 0 && m_Keyframes.Count > 0)
            {
                UpdateTrajectoriesForKeyframes();
            }
        }
        public void LoadFromString(String _xmlString)
        {
            ResetCoreContent();

            StringReader reader = new StringReader(_xmlString);
            XmlTextReader xmlReader = new XmlTextReader(reader);

            FromXml(xmlReader);

            if (m_Keyframes.Count > 0 && m_Tracks.Count > 0)
            {
                foreach (Track t in m_Tracks)
                {
                    t.IntegrateKeyframes();
                }
            }

            m_Plane.SetLocations(m_ImageSize, 1.0, new Point(0, 0));
            m_Grid.SetLocations(m_ImageSize, 1.0, new Point(0, 0));
        }
        public void Reset()
        {
            // Complete reset. (used when over loading a new video)

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
        private void ResetCoreContent()
        {
            // Semi reset: we keep Image size and AverageTimeStampsPerFrame
            m_Keyframes.Clear();
            m_Chronos.Clear();
            
            /*for(Track t in m_Tracks)
            {
            	t.Release();
            }*/
            StopAllTracking();
            m_Tracks.Clear();
            m_Grid.Reset();
            m_Plane.Reset();
            m_Mirrored = false;
        }
        public void UpdateTrajectoriesForKeyframes()
        {
            // Called when keyframe added, removed or title changed
            // => Updates the trajectories.
            foreach (Track t in m_Tracks)
            {
                t.IntegrateKeyframes();
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
            foreach (Track t in m_Tracks)
            {
                t.StopTracking();
            }
        }
        public void CleanupHash()
        {
            m_iLastCleanHash = GetHashCode();
            log.Debug(String.Format("Hash cleaned up. Metadata hash is: {0}", m_iLastCleanHash));
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
        #region Objects Hit Tests
        public void UnselectAll()
        {
            m_iSelectedDrawingFrame = -1;
            m_iSelectedDrawing = -1;
            m_iSelectedTrack = -1;
            m_iSelectedChrono = -1;
            m_Plane.Selected = false;
            m_Grid.Selected = false;
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
        public bool IsOnDrawing(int _iActiveKeyframeIndex, Point _MouseLocation, long _iTimestamp)
        {
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
        public bool IsOnChronometer(Point _MouseLocation, long _iTimestamp)
        {
            // Note: Mouse coordinates are already descaled.
            
            bool bChronoHit = false;

            int iCurrentChrono = 0;
            while (iCurrentChrono < m_Chronos.Count && !bChronoHit)
            {
                if (m_Chronos[iCurrentChrono].HitTest(_MouseLocation, _iTimestamp) >= 0)
                {
                    bChronoHit = true;
                    m_iSelectedChrono = iCurrentChrono;
                }
                else
                {
                    iCurrentChrono++;
                }
            }

            return bChronoHit;
        }
        public bool IsOnTrack(Point _MouseLocation, long _iTimestamp)
        {
            // Note: Mouse coordinates are already descaled.

            bool bTrackHit = false;
            int iCurrentTrack = 0;
            while (!bTrackHit && iCurrentTrack < m_Tracks.Count)
            {
                if (m_Tracks[iCurrentTrack].HitTest(_MouseLocation, _iTimestamp) >= 0)
                {
                    bTrackHit = true;
                    m_iSelectedTrack = iCurrentTrack;
                }
                else
                {
                    iCurrentTrack++;
                }
            }

            return bTrackHit;
        }
        public bool IsOnGrid(Point _MouseLocation)
        {
            // Note: Mouse coordinates are already descaled.

            bool bGridHit = false;
            if (m_Plane.Visible == true)
            {
                if (m_Plane.HitTest(_MouseLocation) >= 0)
                {
                    bGridHit = true;
                    m_Plane.Selected = true;
                }
            }

            if (!bGridHit && m_Grid.Visible == true)
            {
                if (m_Grid.HitTest(_MouseLocation) >= 0)
                {
                    bGridHit = true;
                    m_Grid.Selected = true;
                }
            }

            return bGridHit;
        }
        #endregion

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
                        iOutputTimestamp = (int)(iFrameNumber * m_iAverageTimeStampsPerFrame);
                    }
                    else
                    {
                        if (m_iInputSelectionStart - m_iInputFirstTimeStamp > 0)
                        {
                            // There was a selection.
                            iFrameNumber = (int)((_iInputTimestamp - m_iInputSelectionStart) / m_iInputAverageTimeStampsPerFrame);
                        }
                        else
                        {
                            iFrameNumber = (int)((_iInputTimestamp - m_iInputFirstTimeStamp) / m_iInputAverageTimeStampsPerFrame);
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
        private void ToXml(XmlTextWriter _xmlWriter)
        {
            try
            {
                _xmlWriter.WriteStartDocument();
                _xmlWriter.WriteStartElement("KinoveaVideoAnalysis");

                // Infos générales
                WriteAnalysisInfos(_xmlWriter);

                // Info de chaque Keyframe.
                if (ActiveKeyframes() > 0)
                {
                    _xmlWriter.WriteStartElement("Keyframes");
                    foreach (Keyframe kf in m_Keyframes)
                    {
                        if (!kf.Disabled)
                        {
                            kf.ToXmlString(_xmlWriter);
                        }
                    }
                    _xmlWriter.WriteEndElement();
                }

                // Infos des Tracks
                if (m_Tracks.Count > 0)
                {
                    _xmlWriter.WriteStartElement("Tracks");
                    foreach (Track trk in m_Tracks)
                    {
                        trk.ToXmlString(_xmlWriter);
                    }
                    _xmlWriter.WriteEndElement();
                }

                // Infos des Chronos
                if (m_Chronos.Count > 0)
                {
                    _xmlWriter.WriteStartElement("Chronos");
                    foreach (DrawingChrono dc in m_Chronos)
                    {
                        dc.ToXmlString(_xmlWriter);
                    }
                    _xmlWriter.WriteEndElement();
                }

                _xmlWriter.WriteEndElement();
                _xmlWriter.WriteEndDocument();
                _xmlWriter.Flush();
                _xmlWriter.Close();
            }
            catch (Exception)
            {
                // Possible cause:doesn't have rights to write.
            }

            // Now that we will save those data, keep a trace of what state we were in.
            CleanupHash();
        }
        private void WriteAnalysisInfos(XmlTextWriter _xmlWriter)
        {
            // Format version
            _xmlWriter.WriteStartElement("FormatVersion");
            _xmlWriter.WriteString("1.3");
            _xmlWriter.WriteEndElement();

            // Application Version
            _xmlWriter.WriteStartElement("Producer");
            _xmlWriter.WriteString("Kinovea." + PreferencesManager.ReleaseVersion);
            _xmlWriter.WriteEndElement();

            // Original Filename 
            _xmlWriter.WriteStartElement("OriginalFilename");
            _xmlWriter.WriteString(Path.GetFileNameWithoutExtension(m_FullPath));
            _xmlWriter.WriteEndElement();

            // General Title (?)
            _xmlWriter.WriteStartElement("GlobalTitle");
            _xmlWriter.WriteString(m_GlobalTitle);
            _xmlWriter.WriteEndElement();

            // ImageSize
            _xmlWriter.WriteStartElement("ImageSize");
            _xmlWriter.WriteString(m_ImageSize.Width + ";" + m_ImageSize.Height);
            _xmlWriter.WriteEndElement();

            // AverageTimeStampPerFrame
            _xmlWriter.WriteStartElement("AverageTimeStampsPerFrame");
            _xmlWriter.WriteString(m_iAverageTimeStampsPerFrame.ToString());
            _xmlWriter.WriteEndElement();

            // FirstTimeStamp
            _xmlWriter.WriteStartElement("FirstTimeStamp");
            _xmlWriter.WriteString(m_iFirstTimeStamp.ToString());
            _xmlWriter.WriteEndElement();

            // SelectionStart
            _xmlWriter.WriteStartElement("SelectionStart");
            _xmlWriter.WriteString(m_iSelectionStart.ToString());
            _xmlWriter.WriteEndElement();
        }
        private void FromXml(XmlTextReader _xmlReader)
        {
        	
        	// TODO:Metadata from xml
        	// 1. Have a function to discover the file format.
        	// 2. Have a common interface for all parsers.
        	// 3. Instanciate this file format parser, and parse the data.
        	
            if (_xmlReader != null)
            {
                try
                {
                    _xmlReader.Read();
                    
                    if (_xmlReader.IsStartElement())
                    {
                        if (_xmlReader.Name == "KinoveaVideoAnalysis")
                        {
                            ParseAnalysis(_xmlReader);
                        }
                        else if (_xmlReader.Name == "Storyboard")
                        {
                            // Dartfish StoryBoard, we'll try to read it.
                            DartfishStoryboardParser dsp = new DartfishStoryboardParser();
            				dsp.Parse(_xmlReader, this);
                        }
                        else if (_xmlReader.Name == "LIBRARY_ITEM")
                        {
                            // .dartclip Horror file, we'll try to read it.
                            DartfishLibraryItemParser dlip = new DartfishLibraryItemParser();
            				dlip.Parse(_xmlReader, this);
                        }
                        else
                        {
                            // Unsupported format.
                        }
                    }
                }
                catch (Exception)
                {
                    // Une erreur est survenue pendant le parsing.
                }
                finally
                {
                    _xmlReader.Close();
                }
            }

            CleanupHash();
        }
        public override int GetHashCode()
        {
            // Combine all fields hashes, using XOR operator.
            int iHashCode = GetKeyframesHashCode() ^ GetChronometersHashCode() ^ GetTracksHashCode();
            return iHashCode;
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
        private int GetChronometersHashCode()
        {
            // Chronos hashcodes are XORed with one another. 
            int iHashCode = 0;
            foreach (DrawingChrono dc in m_Chronos)
            {
                iHashCode ^= dc.GetHashCode();
            }
            return iHashCode;    
        }
        private int GetTracksHashCode()
        {
            // Tracks hashcodes are XORed with one another. 
            int iHashCode = 0;
            foreach (Track trk in m_Tracks)
            {
                iHashCode ^= trk.GetHashCode();
            }
            return iHashCode;    
        }

        #endregion
        
        #region Parse Kinovea Analysis
        
        private void ParseAnalysis(XmlTextReader _xmlReader)
        {
            while (_xmlReader.Read())
            {
                if (_xmlReader.IsStartElement())
                {
                    // FormatVersion
                    // Producer

                    if (_xmlReader.Name == "GlobalTitle")
                    {
                        m_GlobalTitle = _xmlReader.ReadString();
                    }
                    else if (_xmlReader.Name == "OriginalFilename")
                    {
                        m_InputFileName = _xmlReader.ReadString();
                    }
                    else if (_xmlReader.Name == "ImageSize")
                    {
                        Point p = XmlHelper.PointParse(_xmlReader.ReadString(), ';');
                        m_InputImageSize.Width = p.X;
                        m_InputImageSize.Height = p.Y;
                    }
                    else if (_xmlReader.Name == "AverageTimeStampsPerFrame")
                    {
                        m_iInputAverageTimeStampsPerFrame = Int64.Parse(_xmlReader.ReadString());
                    }
                    else if (_xmlReader.Name == "FirstTimeStamp")
                    {
                        m_iInputFirstTimeStamp = Int64.Parse(_xmlReader.ReadString());
                    }
                    else if (_xmlReader.Name == "SelectionStart")
                    {
                        m_iInputSelectionStart = Int64.Parse(_xmlReader.ReadString());
                    }
                    else if (_xmlReader.Name == "Keyframes")
                    {
                        ParseKeyframes(_xmlReader);
                    }
                    else if (_xmlReader.Name == "Tracks")
                    {
                        ParseTracks(_xmlReader);
                    }
                    else if (_xmlReader.Name == "Chronos")
                    {
                        ParseChronos(_xmlReader);
                    }
                    else
                    {
                        // not supported in this version.
                    }
                }
                else if (_xmlReader.Name == "KinoveaVideoAnalysis")
                {
                    break;
                }
                else
                {
                    // Fermeture d'un tag interne.
                }
            }
        }
        private void ParseKeyframes(XmlTextReader _xmlReader)
        {
            while (_xmlReader.Read())
            {
                if (_xmlReader.IsStartElement())
                {
                    if (_xmlReader.Name == "Keyframe")
                    {
                        Keyframe kf = ParseKeyframe(_xmlReader);
                        m_Keyframes.Add(kf);
                    }
                }
                else if (_xmlReader.Name == "Keyframes")
                {
                    break;
                }
                else
                {
                    // Fermeture d'un tag interne.
                }
            }

        }
        private Keyframe ParseKeyframe(XmlTextReader _xmlReader)
        {
            Keyframe kf = new Keyframe();

            while (_xmlReader.Read())
            {
                if (_xmlReader.IsStartElement())
                {
                    if (_xmlReader.Name == "Position")
                    {
                        ParsePosition(_xmlReader, kf);  
                    }
                    if (_xmlReader.Name == "Title")
                    {
                        kf.Title = _xmlReader.ReadString();
                    }
                    if (_xmlReader.Name == "CommentLines")
                    {
                        ParseComments(_xmlReader, kf);
                    }
                    if (_xmlReader.Name == "Drawings")
                    {
                        ParseDrawings(_xmlReader, kf);  
                    }
                }
                else if (_xmlReader.Name == "Keyframe")
                {
                    break;
                }
                else
                {
                    // Fermeture d'un tag interne.
                }
            }

            return kf;
        }
        private void ParsePosition(XmlTextReader _xmlReader, Keyframe _keyframe)
        {
            int iInputPosition = int.Parse(_xmlReader.ReadString());

            _keyframe.Position = DoRemapTimestamp(iInputPosition, false);

        }
        private void ParseComments(XmlTextReader _xmlReader, Keyframe _keyframe)
        {
            while (_xmlReader.Read())
            {
                if (_xmlReader.IsStartElement())
                {
                    if (_xmlReader.Name == "CommentLine")
                    {
                        _keyframe.Comments.Add(_xmlReader.ReadString());
                    }
                }
                else if (_xmlReader.Name == "CommentLines")
                {
                    break;
                }
                else
                {
                    // Fermeture d'un tag interne.
                }
            }
        }
        private void ParseDrawings(XmlTextReader _xmlReader, Keyframe _keyframe)
        {
            while (_xmlReader.Read())
            {
                if (_xmlReader.IsStartElement())
                {
                    if (_xmlReader.Name == "Drawing")
                    {
                        AbstractDrawing ad = ParseDrawing(_xmlReader);
                        if (ad != null)
                        {
                            _keyframe.Drawings.Insert(0, ad);
                            _keyframe.Drawings[0].infosFading.ReferenceTimestamp = _keyframe.Position;
                            _keyframe.Drawings[0].infosFading.AverageTimeStampsPerFrame = m_iAverageTimeStampsPerFrame;
                        }
                    }
                }
                else if (_xmlReader.Name == "Drawings")
                {
                    break;
                }
                else
                {
                    // Fermeture d'un tag interne.
                }
            }
        }
        private AbstractDrawing ParseDrawing(XmlTextReader _xmlReader)
        {
            // The drawings will be scaled to the new image size
            PointF scaling = new PointF();
            scaling.X = (float)m_ImageSize.Width / (float)m_InputImageSize.Width;
            scaling.Y = (float)m_ImageSize.Height / (float)m_InputImageSize.Height;

            AbstractDrawing ad = null;
            string type = _xmlReader.GetAttribute("Type");
            switch (type)
            {
                case "DrawingLine2D":
                    ad = DrawingLine2D.FromXml(_xmlReader, scaling);
                    ((DrawingLine2D)ad).ParentMetadata = this;
                    break;
                case "DrawingCross2D":
                    ad = DrawingCross2D.FromXml(_xmlReader, scaling);
                    break;
                case "DrawingAngle2D":
                    ad = DrawingAngle2D.FromXml(_xmlReader, scaling);
                    break;
                case "DrawingPencil":
                    ad = DrawingPencil.FromXml(_xmlReader, scaling);
                    break;
                case "DrawingText":
                    ad = DrawingText.FromXml(_xmlReader, scaling);
                    break;
                case "DrawingChrono":
                    ad = DrawingChrono.FromXml(_xmlReader, scaling, new DelegateRemapTimestamp(DoRemapTimestamp));
                    break;
                default:
                    // Unkown Drawing. 
                    // Forward compatibility : return null.
                    break;
            }

            return ad;
        }
        private void ParseChronos(XmlTextReader _xmlReader)
        {
            while (_xmlReader.Read())
            {
                if (_xmlReader.IsStartElement())
                {
                    if (_xmlReader.Name == "Chrono")
                    {
                        DrawingChrono dc = (DrawingChrono)ParseDrawing(_xmlReader);
                        if (dc != null)
                        {
                            m_Chronos.Add(dc);
                            
                            // complete setup
                            this.SelectedChrono = m_Chronos.Count - 1;
                            m_Chronos[SelectedChrono].ParentMetadata = this;
                        }
                    }
                }
                else if (_xmlReader.Name == "Chronos")
                {
                    break;
                }
                else
                {
                    // Fermeture d'un tag interne.
                }
            }

        }
        private void ParseTracks(XmlTextReader _xmlReader)
        {
            while (_xmlReader.Read())
            {
                if (_xmlReader.IsStartElement())
                {
                    if (_xmlReader.Name == "Track")
                    {
                        PointF scaling = new PointF();
                        scaling.X = (float)m_ImageSize.Width / (float)m_InputImageSize.Width;
                        scaling.Y = (float)m_ImageSize.Height / (float)m_InputImageSize.Height;

                        Track trk = Track.FromXml(_xmlReader, scaling, new DelegateRemapTimestamp(DoRemapTimestamp));
                        if (trk != null)
                        {
                            // Finish setup
                            trk.ParentMetadata = this;
                            trk.m_ShowClosestFrame = m_ShowClosestFrameCallback;

                            m_Tracks.Add(trk);
                            this.SelectedTrack = m_Tracks.Count - 1;
                        }
                    }
                }
                else if (_xmlReader.Name == "Tracks")
                {
                    break;
                }
                else
                {
                    // Fermeture d'un tag interne.
                }
            }
        }
        #endregion

        #region Dartfish files Parsers
        private void ParseDartfishStoryboard(XmlTextReader _xmlReader)
        {
        	DartfishStoryboardParser dsp = new DartfishStoryboardParser();
        	dsp.Parse(_xmlReader, this);
        }
        private void ParseDartfishLibraryItem(XmlTextReader _xmlReader)
        {
            DartfishLibraryItemParser dlip = new DartfishLibraryItemParser();
            dlip.Parse(_xmlReader, this);
        }
        #endregion
    }
}
