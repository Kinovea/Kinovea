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
using Kinovea.ScreenManager.Languages;

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
        private long inputTimeOrigin;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void Load(Metadata metadata, string source, bool isFile)
        {
            if (metadata == null)
                throw new ArgumentNullException("metadata");

            if (string.IsNullOrEmpty(source))
                throw new ArgumentNullException("source");

            this.metadata = metadata;
            metadata.BeforeKVAImport();

            string extension = Path.GetExtension(source).ToLower();
            if (extension == ".kva" || extension == ".xml")
            {
                ImportKVA(source, isFile);
            }
            else if (extension == ".srt")
            {
                MetadataImporterSRT.Import(metadata, source, isFile);
            }
            else if (source.EndsWith("_keypoints.json"))
            {
                MetadataImporterOpenPose.Import(metadata, source, isFile);
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

        /// <summary>
        /// Save to the last known storage location of this KVA if any, otherwise ask for a target filename.
        /// </summary>
        public void UserSave(Metadata metadata, string videoFile)
        {
            if (string.IsNullOrEmpty(metadata.LastKVAPath))
            {
                UserSaveAs(metadata, videoFile);
            }
            else
            {
                SaveToFile(metadata, metadata.LastKVAPath);
                metadata.AfterManualExport();
            }
        }

        /// <summary>
        /// Ask for a filename and then save the metadata to this filename.
        /// </summary>
        public void UserSaveAs(Metadata metadata, string videoFile)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = ScreenManagerLang.dlgSaveAnalysisTitle;

            // Go to this video directory and suggest sidecar filename.
            saveFileDialog.InitialDirectory = Path.GetDirectoryName(videoFile);
            saveFileDialog.FileName = Path.GetFileNameWithoutExtension(videoFile);
            saveFileDialog.Filter = FilesystemHelper.SaveKVAFilter();
            saveFileDialog.FilterIndex = 1;

            if (saveFileDialog.ShowDialog() != DialogResult.OK || string.IsNullOrEmpty(saveFileDialog.FileName))
                return;

            string filename = saveFileDialog.FileName;
            if (!filename.ToLower().EndsWith(".kva") && !filename.ToLower().EndsWith(".xml"))
                filename += ".kva";

            SaveToFile(metadata, filename);
            metadata.LastKVAPath = filename;
            metadata.AfterManualExport();
        }

        /// <summary>
        /// Save the metadata to the passed file.
        /// </summary>
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

        /// <summary>
        /// Extract the referenced video file from the given KVA file.
        /// Used in the context of crash recovery.
        /// </summary>
        public static string ExtractFullPath(string source)
        {
            bool relativeTrajectories;
            string kva = MetadataConverter.Convert(source, true, out relativeTrajectories);

            XmlDocument doc = new XmlDocument();
            doc.Load(kva);
            
            XmlNode pathNode = doc.DocumentElement.SelectSingleNode("descendant::FullPath");
            return pathNode != null ? pathNode.InnerText : null;
        }

        /// <summary>
        /// Determines if a given file can be imported as metadata.
        /// </summary>
        public static bool IsMetadataFile(string path)
        {
            return SupportedFileFormats().Contains(Path.GetExtension(path).ToLower());
        }

        /// <summary>
        /// List of supported metadata file extensions by order of preference.
        /// </summary>
        public static List<string> SupportedFileFormats()
        {
            return new List<string>() { ".kva", ".srt", ".xml", ".json" };
        }


        #region load
        private void ImportKVA(string source, bool isFile)
        {
            XmlReader reader = null;

            try
            {
                bool relativeTrajectories;
                string kva = MetadataConverter.Convert(source, isFile, out relativeTrajectories);

                XmlReaderSettings settings = new XmlReaderSettings();
                settings.IgnoreComments = true;
                settings.IgnoreProcessingInstructions = true;
                settings.IgnoreWhitespace = true;
                settings.CloseInput = true;

                reader = isFile ? XmlReader.Create(kva, settings) : XmlReader.Create(new StringReader(kva), settings);
                Load(reader);
                if (relativeTrajectories)
                    metadata.FixRelativeTrajectories();

                if (isFile)
                    metadata.LastKVAPath = source;
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
        }
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
                        if (string.IsNullOrEmpty(metadata.VideoPath))
                            metadata.VideoPath = fullPath;
                        break;
                    case "GlobalTitle":
                        metadata.GlobalTitle = r.ReadElementContentAsString();
                        break;
                    case "ImageSize":
                        inputImageSize = XmlHelper.ParseSize(r.ReadElementContentAsString());
                        scaling = GetScaling();
                        break;
                    case "Aspect":
                        metadata.ImageAspect = (ImageAspectRatio)Enum.Parse(typeof(ImageAspectRatio), r.ReadElementContentAsString());
                        break;
                    case "Rotation":
                        metadata.ImageRotation = (ImageRotation)Enum.Parse(typeof(ImageRotation), r.ReadElementContentAsString());
                        break;
                    case "Mirror":
                        metadata.Mirrored = XmlHelper.ParseBoolean(r.ReadElementContentAsString());
                        break;
                    case "Demosaicing":
                        metadata.Demosaicing = (Demosaicing)Enum.Parse(typeof(Demosaicing), r.ReadElementContentAsString());
                        break;
                    case "Deinterlacing":
                        metadata.Deinterlacing = XmlHelper.ParseBoolean(r.ReadElementContentAsString());
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
                        long selStart = r.ReadElementContentAsLong();
                        if (IsSameContext())
                            metadata.SelectionStart = selStart;
                        break;
                    case "SelectionEnd":
                        long selEnd = r.ReadElementContentAsLong();
                        if (IsSameContext())
                            metadata.SelectionEnd = selEnd;
                        break;
                    case "TimeOrigin":
                        inputTimeOrigin = r.ReadElementContentAsLong();
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
                        metadata.DrawingSpotlight.ReadXml(r, scaling, RemapTimestamp, metadata);
                        break;
                    case "AutoNumbers":
                    case "NumberSequence":
                        metadata.DrawingNumberSequence.ReadXml(r, scaling, RemapTimestamp, metadata);
                        break;
                    case "CoordinateSystem":
                        metadata.DrawingCoordinateSystem.ReadXml(r);
                        break;
                    case "Trackability":
                        metadata.TrackabilityManager.ReadXml(r, scaling, RemapTimestamp);
                        break;
                    case "VideoFilters":
                        metadata.ReadVideoFilters(r);
                        break;
                    default:
                        // Skip the unparsed nodes.
                        string unparsed = r.ReadOuterXml();
                        log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                        break;
                }
            }

            if (IsSameContext())
                metadata.TimeOrigin = inputTimeOrigin;

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
        private long RemapTimestamp(long inputTimestamp)
        {
            // The Input position was stored as an absolute timestamp in the context of the original video.
            // The only exception to this are trajectory points from 0.8.15 files.

            if (inputAverageTimeStampsPerFrame == 0)
                return inputTimestamp;

            if (IsSameContext())
                return inputTimestamp;

            // Different contexts or different files.
            // Compute the number of frames relatively to the time origin and convert back to timestamps.
            double frame = (double)(inputTimestamp - inputTimeOrigin) / inputAverageTimeStampsPerFrame;
            double outputAverageTimestampsPerFrame = metadata.AverageTimeStampsPerSecond / (1000.0 / metadata.UserInterval);
            long outputTimestamp = (long)Math.Round(frame * outputAverageTimestampsPerFrame) + metadata.TimeOrigin;
            
            return outputTimestamp;
        }

        /// <summary>
        /// Returns true if we are in the exact same context and file.
        /// </summary>
        private bool IsSameContext()
        {
            return ((inputFirstTimeStamp == metadata.FirstTimeStamp) &&
                    (inputAverageTimeStampsPerFrame == metadata.AverageTimeStampsPerFrame) &&
                    (inputFileName == Path.GetFileNameWithoutExtension(metadata.VideoPath)));
        }
        #endregion

        #region Save
        private void WriteXml(XmlWriter w)
        {
            // Convert the metadata to KVA XML.
            w.WriteStartElement("KinoveaVideoAnalysis");
            
            WriteGeneralInformation(w);
            WriteKeyframes(w, SerializationFilter.KVA);
            WriteChronos(w, SerializationFilter.KVA);
            WriteTracks(w, SerializationFilter.KVA);
            WriteSpotlights(w);
            WriteNumberSequence(w);
            WriteCoordinateSystem(w);
            WriteTrackablePoints(w);
            WriteVideoFilters(w);

            w.WriteEndElement();
        }

        private void WriteGeneralInformation(XmlWriter w)
        {
            w.WriteElementString("FormatVersion", "2.0");
            w.WriteElementString("Producer", Software.ApplicationName + "." + Software.Version);
            w.WriteElementString("OriginalFilename", Path.GetFileNameWithoutExtension(metadata.VideoPath));
            w.WriteElementString("FullPath", metadata.VideoPath);

            if (!string.IsNullOrEmpty(metadata.GlobalTitle))
                w.WriteElementString("GlobalTitle", metadata.GlobalTitle);

            w.WriteElementString("ImageSize", metadata.ImageSize.Width + ";" + metadata.ImageSize.Height);
            
            w.WriteElementString("Aspect", metadata.ImageAspect.ToString());
            w.WriteElementString("Rotation", metadata.ImageRotation.ToString());
            w.WriteElementString("Mirror", metadata.Mirrored.ToString().ToLower());
            w.WriteElementString("Demosaicing", metadata.Demosaicing.ToString());
            w.WriteElementString("Deinterlacing", metadata.Deinterlacing.ToString().ToLower());

            w.WriteElementString("AverageTimeStampsPerFrame", metadata.AverageTimeStampsPerFrame.ToString());
            w.WriteElementString("CaptureFramerate", string.Format(CultureInfo.InvariantCulture, "{0}", metadata.CalibrationHelper.CaptureFramesPerSecond));
            w.WriteElementString("UserFramerate", string.Format(CultureInfo.InvariantCulture, "{0}", 1000 / metadata.UserInterval));
            w.WriteElementString("FirstTimeStamp", metadata.FirstTimeStamp.ToString());
            w.WriteElementString("SelectionStart", metadata.SelectionStart.ToString());
            w.WriteElementString("SelectionEnd", metadata.SelectionEnd.ToString());
            w.WriteElementString("TimeOrigin", metadata.TimeOrigin.ToString());

            WriteCalibrationHelp(w);
        }

        private void WriteKeyframes(XmlWriter w, SerializationFilter filter)
        {
            if (metadata.Keyframes.Count() == 0)
                return;
            
            w.WriteStartElement("Keyframes");

            foreach (Keyframe kf in metadata.Keyframes)
                KeyframeSerializer.Serialize(w, kf, filter);

            w.WriteEndElement();
        }
        private void WriteChronos(XmlWriter w, SerializationFilter filter)
        {
            if (metadata.ChronoManager.Drawings.Count == 0)
                return;

            w.WriteStartElement("Chronos");
            
            foreach (AbstractDrawing chrono in metadata.ChronoManager.Drawings)
            {
                IKvaSerializable d = chrono as IKvaSerializable;

                if (chrono is DrawingChrono)
                {
                    w.WriteStartElement("Chrono");
                }
                else if (chrono is DrawingChronoMulti)
                {
                    w.WriteStartElement("ChronoMulti");
                }

                w.WriteAttributeString("id", d.Id.ToString());
                w.WriteAttributeString("name", d.Name);
                d.WriteXml(w, filter);
                w.WriteEndElement();
            }

            w.WriteEndElement();
        }
        private void WriteTracks(XmlWriter w, SerializationFilter filter)
        {
            if (metadata.TrackManager.Drawings.Count == 0)
                return;

            w.WriteStartElement("Tracks");

            foreach (DrawingTrack track in metadata.TrackManager.Drawings)
            {
                w.WriteStartElement("Track");
                w.WriteAttributeString("id", track.Id.ToString());
                w.WriteAttributeString("name", track.Name);
                track.WriteXml(w, filter);
                w.WriteEndElement();
            }

            w.WriteEndElement();
        }
        private void WriteSpotlights(XmlWriter w)
        {
            if (metadata.DrawingSpotlight.Count == 0)
                return;

            w.WriteStartElement("Spotlights");
            metadata.DrawingSpotlight.WriteXml(w, SerializationFilter.KVA);
            w.WriteEndElement();
        }
        private void WriteNumberSequence(XmlWriter w)
        {
            if (metadata.DrawingNumberSequence.Count == 0)
                return;

            w.WriteStartElement("NumberSequence");
            metadata.DrawingNumberSequence.WriteXml(w, SerializationFilter.KVA);
            w.WriteEndElement();
        }
        private void WriteCoordinateSystem(XmlWriter w)
        {
            w.WriteStartElement("CoordinateSystem");
            w.WriteAttributeString("id", metadata.DrawingCoordinateSystem.Id.ToString());
            w.WriteAttributeString("name", metadata.DrawingCoordinateSystem.Name);
            metadata.DrawingCoordinateSystem.WriteXml(w, SerializationFilter.KVA);
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
        private void WriteVideoFilters(XmlWriter w)
        {
            w.WriteStartElement("VideoFilters");
            string name = VideoFilterFactory.GetName(metadata.ActiveVideoFilterType);
            if (!string.IsNullOrEmpty(name))
                w.WriteAttributeString("active", name);
            
            metadata.WriteVideoFilters(w);
            w.WriteEndElement();
        }
        #endregion
    }
}
