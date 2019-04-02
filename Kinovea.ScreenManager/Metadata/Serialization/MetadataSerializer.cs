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
            metadata.BeforeKVAImport();

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
                        metadata.CalibrationHelper.CaptureFramesPerSecond = r.ReadElementContentAsDouble();
                        break;
                    case "UserFramerate":
                        metadata.UserInterval = 1000 / r.ReadElementContentAsDouble();
                        break;
                    case "SelectionStart":
                        inputSelectionStart = r.ReadElementContentAsLong();
                        break;
                    case "Calibration":
                        metadata.CalibrationHelper.ReadXml(r, scaling, inputImageSize);
                        break;
                    case "Keyframes":
                        ParseKeyframes(r);
                        break;
                    case "Tracks":
                        ParseTracks(r, scaling);
                        break;
                    case "Chronos":
                        ParseChronos(r, scaling);
                        break;
                    case "Spotlights":
                        metadata.SpotlightManager.ReadXml(r, scaling, RemapTimestamp, metadata);
                        break;
                    case "AutoNumbers":
                        metadata.AutoNumberManager.ReadXml(r, scaling, RemapTimestamp, metadata);
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

            // Note: unlike chrono and tracks, keyframes are "merged" into existing keyframes if one already exists at the same position.
            // This has an impact on how we add drawings.
            // We keep the drawings internally to the keyframe during the parse, and only perform the post-drawing init at the end, 
            // when the keyframe is merge-inserted into the collection.
            // For chrono and tracks on the other hand, we perform the post-drawing init on the fly during the parse.

            r.ReadStartElement();

            while (r.NodeType == XmlNodeType.Element)
            {
                if (r.Name == "Keyframe")
                {
                    Keyframe keyframe = KeyframeSerializer.Deserialize(r, GetScaling(), RemapTimestamp, metadata);
                    if (keyframe != null)
                        metadata.MergeInsertKeyframe(keyframe);
                }
                else
                {
                    string unparsed = r.ReadOuterXml();
                    log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                }
            }

            r.ReadEndElement();
        }
        
        private void ParseChronos(XmlReader r, PointF scale)
        {
            // TODO: catch empty tag <Chronos/>.

            r.ReadStartElement();

            while (r.NodeType == XmlNodeType.Element)
            {
                AbstractDrawing drawing = DrawingSerializer.Deserialize(r, scale, TimeHelper.IdentityTimestampMapper, metadata);
                metadata.AddDrawing(metadata.ChronoManager.Id, drawing);
            }

            r.ReadEndElement();
        }
        private void ParseTracks(XmlReader r, PointF scale)
        {
            // TODO: catch empty tag <Tracks/>.

            r.ReadStartElement();

            while (r.NodeType == XmlNodeType.Element)
            {
                AbstractDrawing drawing = DrawingSerializer.Deserialize(r, scale, RemapTimestamp, metadata);
                metadata.AddDrawing(metadata.TrackManager.Id, drawing);
            }

            r.ReadEndElement();
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
            w.WriteElementString("CaptureFramerate", string.Format(CultureInfo.InvariantCulture, "{0}", metadata.CalibrationHelper.CaptureFramesPerSecond));
            w.WriteElementString("UserFramerate", string.Format(CultureInfo.InvariantCulture, "{0}", 1000 / metadata.UserInterval));
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
                KeyframeSerializer.Serialize(w, kf);
            }

            w.WriteEndElement();
        }
        private void WriteChronos(XmlWriter w)
        {
            bool atLeastOne = false;
            foreach (DrawingChrono chrono in metadata.ChronoManager.Drawings)
            {   
                if (!atLeastOne)
                {
                    w.WriteStartElement("Chronos");
                    atLeastOne = true;
                }

                w.WriteStartElement("Chrono");
                w.WriteAttributeString("id", chrono.Id.ToString());
                w.WriteAttributeString("name", chrono.Name);
                chrono.WriteXml(w, SerializationFilter.All);
                w.WriteEndElement();
            }

            if (atLeastOne)
                w.WriteEndElement();
        }
        private void WriteTracks(XmlWriter w)
        {
            bool atLeastOne = false;
            foreach (DrawingTrack track in metadata.Tracks())
            {
                if (!atLeastOne)
                {
                    w.WriteStartElement("Tracks");
                    atLeastOne = true;
                }

                w.WriteStartElement("Track");
                w.WriteAttributeString("id", track.Id.ToString());
                w.WriteAttributeString("name", track.Name);
                track.WriteXml(w, SerializationFilter.All);
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
            metadata.SpotlightManager.WriteXml(w, SerializationFilter.All);
            w.WriteEndElement();
        }
        private void WriteAutoNumbers(XmlWriter w)
        {
            if (metadata.AutoNumberManager.Count == 0)
                return;

            w.WriteStartElement("AutoNumbers");
            metadata.AutoNumberManager.WriteXml(w, SerializationFilter.All);
            w.WriteEndElement();
        }
        private void WriteCoordinateSystem(XmlWriter w)
        {
            w.WriteStartElement("CoordinateSystem");
            w.WriteAttributeString("id", metadata.DrawingCoordinateSystem.Id.ToString());
            w.WriteAttributeString("name", metadata.DrawingCoordinateSystem.Name);
            metadata.DrawingCoordinateSystem.WriteXml(w, SerializationFilter.All);
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
