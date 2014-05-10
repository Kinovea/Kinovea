using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Kinovea.Services;
using System.Globalization;
using System.Xml.Xsl;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Reflection;
using System.Xml.Serialization;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// This class is used to import additional metadata into an existing metadata object and save metadata to KVA.
    /// Metadata is whatever is saved into a KVA file.
    /// When importing, the data may be adapted to fit the size and duration of the current video.
    /// TODO: group scaling and timestamp remapping into a dedicated MetadataAdapter class that we would pass to parsers.
    /// </summary>
    public class MetadataSerializer
    {
        private Metadata metadata;
        private string inputFileName;
        private Size inputImageSize;
        private long inputAverageTimeStampsPerFrame;
        private long inputFirstTimeStamp;
        private long inputSelectionStart;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void Load(Metadata metadata, string source, bool isFile)
        {
            if (metadata == null)
                throw new ArgumentNullException("metadata");

            if (string.IsNullOrEmpty(source))
                throw new ArgumentNullException("source");

            this.metadata = metadata;
            metadata.StopAllTracking();
            metadata.UnselectAll();

            string kva = MetadataConverter.Convert(source, isFile);

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            settings.IgnoreProcessingInstructions = true;
            settings.IgnoreWhitespace = true;
            settings.CloseInput = true;

            XmlReader reader = null;

            try
            {
                reader = isFile ? XmlReader.Create(kva, settings) : XmlReader.Create(new StringReader(kva), settings);
                Load(reader);
            }
            catch (Exception e)
            {
                log.Error("An error happened during the parsing of the KVA metadata");
                log.Error(e);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }

            metadata.AfterKVAImport();
        }
        public string SaveToString(Metadata metadata)
        {
            if (metadata == null)
                throw new ArgumentNullException("metadata");

            this.metadata = metadata;

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = false;
            settings.CloseOutput = true;

            StringBuilder builder = new StringBuilder();
            using (XmlWriter w = XmlWriter.Create(builder, settings))
            {
                try
                {
                    WriteXml(w);
                }
                catch (Exception e)
                {
                    log.Error("An error happened during the writing of the kva string");
                    log.Error(e);
                }
            }

            return builder.ToString();
        }
        public void SaveToFile(Metadata metadata, string file)
        {
            if (metadata == null)
                throw new ArgumentNullException("metadata");

            if (string.IsNullOrEmpty(file))
                throw new ArgumentNullException("file");

            if (!Directory.Exists(Path.GetDirectoryName(file)))
                Directory.CreateDirectory(Path.GetDirectoryName(file));
            
            this.metadata = metadata;

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.CloseOutput = true;

            using (XmlWriter w = XmlWriter.Create(file, settings))
            {
                try
                {
                    WriteXml(w);
                }
                catch (Exception e)
                {
                    log.Error("An error happened during the writing of the kva file");
                    log.Error(e);
                }
            }
        }

        public static string ExtractFullPath(string source)
        {
            // Extract the referenced video file from the given KVA file.
            // Used in the context of crash recovery.
            string kva = MetadataConverter.Convert(source, true);

            XmlDocument doc = new XmlDocument();
            doc.Load(kva);
            
            XmlNode pathNode = doc.DocumentElement.SelectSingleNode("descendant::FullPath");
            return pathNode != null ? pathNode.InnerText : null;
        }

        #region load
        private void Load(XmlReader r)
        {
            // Note: the order of tags is somewhat important.
            // Image size and the timing information must be at the top so we can adapt 
            // all coordinates and times found in the file to the existing video.
            log.Debug("Importing Metadata from KVA file.");

            r.MoveToContent();

            if (!(r.Name == "KinoveaVideoAnalysis"))
                return;

            PointF scaling = PointF.Empty;

            r.ReadStartElement();
            r.ReadElementContentAsString("FormatVersion", "");

            while (r.NodeType == XmlNodeType.Element)
            {
                switch (r.Name)
                {
                    case "Producer":
                        r.ReadElementContentAsString();
                        break;
                    case "OriginalFilename":
                        inputFileName = r.ReadElementContentAsString();
                        break;
                    case "FullPath":
                        string fullPath = r.ReadElementContentAsString();
                        if (string.IsNullOrEmpty(metadata.FullPath))
                            metadata.FullPath = fullPath;
                        break;
                    case "GlobalTitle":
                        metadata.GlobalTitle = r.ReadElementContentAsString();
                        break;
                    case "ImageSize":
                        inputImageSize = XmlHelper.ParseSize(r.ReadElementContentAsString());
                        scaling = GetScaling();
                        break;
                    case "AverageTimeStampsPerFrame":
                        inputAverageTimeStampsPerFrame = r.ReadElementContentAsLong();
                        break;
                    case "FirstTimeStamp":
                        inputFirstTimeStamp = r.ReadElementContentAsLong();
                        break;
                    case "CaptureFramerate":
                        metadata.CalibrationHelper.FramesPerSecond = r.ReadElementContentAsDouble();
                        break;
                    case "SelectionStart":
                        inputSelectionStart = r.ReadElementContentAsLong();
                        break;
                    case "Calibration":
                        metadata.CalibrationHelper.ReadXml(r, scaling);
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
                        metadata.AutoNumberManager.ReadXml(r, scaling, RemapTimestamp, metadata.AverageTimeStampsPerFrame);
                        break;
                    case "CoordinateSystem":
                        metadata.DrawingCoordinateSystem.ReadXml(r);
                        break;
                    case "Trackability":
                        metadata.TrackabilityManager.ReadXml(r, scaling, RemapTimestamp);
                        break;
                    default:
                        // Skip the unparsed nodes.
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

            while (r.NodeType == XmlNodeType.Element)
            {
                if (r.Name == "Keyframe")
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
            // It must be followed by a call to PostImportMetadata() so we can create the thumbnail.
            Keyframe keyframe = new Keyframe(metadata);

            r.ReadStartElement();

            while (r.NodeType == XmlNodeType.Element)
            {
                switch (r.Name)
                {
                    case "Position":
                        int inputPosition = r.ReadElementContentAsInt();
                        keyframe.Position = RemapTimestamp(inputPosition, false);
                        break;
                    case "Title":
                        keyframe.Title = r.ReadElementContentAsString();
                        break;
                    case "Comment":
                        keyframe.Comments = r.ReadElementContentAsString();
                        break;
                    case "Drawings":
                        ParseDrawings(r, keyframe);
                        break;
                    default:
                        string unparsed = r.ReadOuterXml();
                        log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                        break;
                }
            }

            r.ReadEndElement();

            MergeInsertKeyframe(keyframe);
        }
        private void ParseDrawings(XmlReader r, Keyframe keyframe)
        {
            // TODO: catch empty tag <Drawings/>.

            r.ReadStartElement();

            while (r.NodeType == XmlNodeType.Element)
            {
                AbstractDrawing drawing = ParseDrawing(r);

                if (drawing != null)
                {
                    keyframe.Drawings.Add(drawing);
                    drawing.InfosFading.ReferenceTimestamp = keyframe.Position;
                    drawing.InfosFading.AverageTimeStampsPerFrame = metadata.AverageTimeStampsPerFrame;
                    metadata.AfterDrawingCreation(drawing);
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
            foreach (Type t in a.GetTypes())
            {
                if (t.BaseType != typeof(AbstractDrawing))
                    continue;

                object[] attributes = t.GetCustomAttributes(typeof(XmlTypeAttribute), false);
                if (attributes.Length <= 0 || ((XmlTypeAttribute)attributes[0]).TypeName != r.Name)
                    continue;

                ConstructorInfo ci = t.GetConstructor(new[] { typeof(XmlReader), typeof(PointF), typeof(Metadata)});
                if (ci == null)
                    break;

                PointF scaling = GetScaling();
                object[] parameters = new object[] { r, scaling, metadata };
                drawing = (AbstractDrawing)Activator.CreateInstance(t, parameters);
                drawingRead = drawing != null;
                
                break;
            }

            if (!drawingRead)
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

            while (r.NodeType == XmlNodeType.Element)
            {
                // When we have other Chrono tools (cadence tool), make this dynamic
                // on a similar model than for attached drawings. (see ParseDrawing())
                if (r.Name == "Chrono")
                {
                    DrawingChrono dc = new DrawingChrono(r, GetScaling(), RemapTimestamp);

                    if (dc != null)
                        metadata.AddChrono(dc);
                }
                else
                {
                    string unparsed = r.ReadOuterXml();
                    log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                }
            }

            r.ReadEndElement();
        }
        private void ParseTracks(XmlReader r)
        {
            // TODO: catch empty tag <Tracks/>.

            r.ReadStartElement();

            while (r.NodeType == XmlNodeType.Element)
            {
                if (r.Name == "Track")
                {
                    DrawingTrack trk = new DrawingTrack(r, GetScaling(), RemapTimestamp, metadata.ImageSize);

                    if (!trk.Invalid)
                    {
                        metadata.AddTrack(trk, metadata.ClosestFrameDisplayer, trk.MainColor);
                        trk.Status = TrackStatus.Interactive;
                    }
                }
                else
                {
                    string unparsed = r.ReadOuterXml();
                    log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                }
            }

            r.ReadEndElement();
        }
        private void ParseSpotlights(XmlReader r)
        {
            // Fixme: move this code into a SpotlightManager.ReadXml().
            r.ReadStartElement();

            while (r.NodeType == XmlNodeType.Element)
            {
                if (r.Name == "Spotlight")
                {
                    Spotlight spotlight = new Spotlight(r, GetScaling(), RemapTimestamp, metadata.AverageTimeStampsPerFrame);
                    metadata.SpotlightManager.Add(spotlight);
                }
                else
                {
                    string unparsed = r.ReadOuterXml();
                    log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                }
            }

            r.ReadEndElement();
        }
        private void MergeInsertKeyframe(Keyframe keyframe)
        {
            bool processed = false;

            for (int i = 0; i < metadata.Keyframes.Count; i++)
            {
                Keyframe k = metadata.Keyframes[i];
                
                if (keyframe.Position < k.Position)
                {
                    metadata.Keyframes.Insert(i, keyframe);
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
                metadata.Keyframes.Add(keyframe);
        }
        private PointF GetScaling()
        {
            PointF scaling = new PointF(1.0f, 1.0f);
            if (!metadata.ImageSize.IsEmpty && !inputImageSize.IsEmpty)
            {
                scaling.X = (float)metadata.ImageSize.Width / (float)inputImageSize.Width;
                scaling.Y = (float)metadata.ImageSize.Height / (float)inputImageSize.Height;
            }

            return scaling;
        }
        private long RemapTimestamp(long inputTimestamp, bool relative)
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

            if (inputAverageTimeStampsPerFrame == 0)
                return inputTimestamp;

            if ((inputFirstTimeStamp == metadata.FirstTimeStamp) &&
                (inputAverageTimeStampsPerFrame == metadata.AverageTimeStampsPerFrame) &&
                (inputFileName == Path.GetFileNameWithoutExtension(metadata.FullPath)))
                return inputTimestamp;

            // Different contexts or different files.
            // We use the relative positions and adapt the context.
            long outputTimestamp = 0;
            int frameNumber;

            if (relative)
            {
                frameNumber = (int)(inputTimestamp / inputAverageTimeStampsPerFrame);
                outputTimestamp = (int)(frameNumber * metadata.AverageTimeStampsPerFrame);
            }
            else
            {
                long start = Math.Max(inputSelectionStart, inputFirstTimeStamp);
                frameNumber = (int)((inputTimestamp - start) / inputAverageTimeStampsPerFrame);
                outputTimestamp = (int)(frameNumber * metadata.AverageTimeStampsPerFrame) + metadata.FirstTimeStamp;
            }
            
            return outputTimestamp;
        }
        #endregion

        #region Save
        private void WriteXml(XmlWriter w)
        {
            // Convert the metadata to XML.
            // The XML Schema for the format should be available in the "tools/Schema/" folder of the source repository.
            // The format contains both core infos to deserialize back to Metadata and helpers data for XSLT exports, 
            // so these exports have more user friendly values. (timecode vs timestamps, cm vs pixels, etc.)

            w.WriteStartElement("KinoveaVideoAnalysis");
            
            WriteGeneralInformation(w);
            WriteKeyframes(w);
            WriteChronos(w);
            WriteTracks(w);
            WriteSpotlights(w);
            WriteAutoNumbers(w);
            WriteCoordinateSystem(w);
            WriteTrackablePoints(w);

            w.WriteEndElement();
        }
        private void WriteGeneralInformation(XmlWriter w)
        {
            w.WriteElementString("FormatVersion", "2.0");
            w.WriteElementString("Producer", Software.ApplicationName + "." + Software.Version);
            w.WriteElementString("OriginalFilename", Path.GetFileNameWithoutExtension(metadata.FullPath));
            w.WriteElementString("FullPath", metadata.FullPath);

            if (!string.IsNullOrEmpty(metadata.GlobalTitle))
                w.WriteElementString("GlobalTitle", metadata.GlobalTitle);

            w.WriteElementString("ImageSize", metadata.ImageSize.Width + ";" + metadata.ImageSize.Height);
            w.WriteElementString("AverageTimeStampsPerFrame", metadata.AverageTimeStampsPerFrame.ToString());
            w.WriteElementString("CaptureFramerate", string.Format(CultureInfo.InvariantCulture, "{0}", metadata.CalibrationHelper.FramesPerSecond));
            w.WriteElementString("FirstTimeStamp", metadata.FirstTimeStamp.ToString());
            w.WriteElementString("SelectionStart", metadata.SelectionStart.ToString());

            WriteCalibrationHelp(w);
        }
        private void WriteKeyframes(XmlWriter w)
        {
            int enabled = metadata.Keyframes.Count(kf => !kf.Disabled);
            if (enabled == 0)
                return;
            
            w.WriteStartElement("Keyframes");

            foreach (Keyframe kf in metadata.Keyframes.Where(kf => !kf.Disabled))
            {
                w.WriteStartElement("Keyframe");
                kf.WriteXml(w);
                w.WriteEndElement();
            }

            w.WriteEndElement();
        }
        private void WriteChronos(XmlWriter w)
        {
            bool atLeastOne = false;
            foreach (AbstractDrawing ad in metadata.ExtraDrawings)
            {
                DrawingChrono dc = ad as DrawingChrono;
                if (dc == null)
                    continue;
                
                if (!atLeastOne)
                {
                    w.WriteStartElement("Chronos");
                    atLeastOne = true;
                }

                w.WriteStartElement("Chrono");
                dc.WriteXml(w);
                w.WriteEndElement();
            }

            if (atLeastOne)
                w.WriteEndElement();
        }
        private void WriteTracks(XmlWriter w)
        {
            bool atLeastOne = false;
            foreach (AbstractDrawing ad in metadata.ExtraDrawings)
            {
                DrawingTrack trk = ad as DrawingTrack;
                if (trk == null)
                    continue;
                
                if (!atLeastOne)
                {
                    w.WriteStartElement("Tracks");
                    atLeastOne = true;
                }

                w.WriteStartElement("Track");
                trk.WriteXml(w);
                w.WriteEndElement();
            }

            if (atLeastOne)
                w.WriteEndElement();
        }
        private void WriteSpotlights(XmlWriter w)
        {
            if (metadata.SpotlightManager.Count == 0)
                return;

            w.WriteStartElement("Spotlights");
            metadata.SpotlightManager.WriteXml(w);
            w.WriteEndElement();
        }
        private void WriteAutoNumbers(XmlWriter w)
        {
            if (metadata.AutoNumberManager.Count == 0)
                return;

            w.WriteStartElement("AutoNumbers");
            metadata.AutoNumberManager.WriteXml(w);
            w.WriteEndElement();
        }
        private void WriteCoordinateSystem(XmlWriter w)
        {
            w.WriteStartElement("CoordinateSystem");
            w.WriteAttributeString("id", metadata.DrawingCoordinateSystem.ID.ToString());
            metadata.DrawingCoordinateSystem.WriteXml(w);
            w.WriteEndElement();
        }
        private void WriteCalibrationHelp(XmlWriter w)
        {
            w.WriteStartElement("Calibration");
            metadata.CalibrationHelper.WriteXml(w);
            w.WriteEndElement();
        }
        private void WriteTrackablePoints(XmlWriter w)
        {
            w.WriteStartElement("Trackability");
            metadata.TrackabilityManager.WriteXml(w);
            w.WriteEndElement();
        }
        #endregion
    }
}
