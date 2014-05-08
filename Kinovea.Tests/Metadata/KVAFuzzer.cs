using System;
using System.Drawing;
using System.Text;
using System.Xml;
using System.Reflection;
using System.IO;
using System.Globalization;
using Kinovea.ScreenManager;
using Kinovea.Services;
using System.Collections.Generic;
using System.Drawing.Drawing2D;

namespace Kinovea.Tests.Metadata
{
    /// <summary>
    /// Generates random kva.
    /// TODO:
    /// Chronos, Tracks, drawings other than label.
    /// Add a summary to get stats on what was generated.
    /// </summary>
    public class KVAFuzzer20
    {
        private const string KVA_VERSION = "2.0";
        private Random random = new Random();
        private string producer;
        
        private long durationTimestamps;
        private Size imageSize;
        private long averageTimestampPerFrame;

        public KVAFuzzer20()
        {
            Version v = Assembly.GetExecutingAssembly().GetName().Version;
            producer = string.Format("{0}.{1}.{2}.{3}", this.GetType().ToString(), v.Major, v.Minor, v.Build);
        }
        
        public void CreateKVA(string outputPath)
        {
            string filename = string.Format("{0:yyyyMMddTHHmmss}.kva", DateTime.Now);
            string output = Path.Combine(outputPath, filename);

            Initialize();
            
            XmlTextWriter w = new XmlTextWriter(output, Encoding.UTF8);
            w.Formatting = Formatting.Indented;
            
            w.WriteStartDocument();
            w.WriteStartElement("KinoveaVideoAnalysis");

            WriteGeneralInformation(w);
            //WriteKeyframes(w);
            //WriteChronos(w);
            //WriteExtraDrawings(w, 50, "Chronos", "Chrono", WriteChrono);
            WriteExtraDrawings(w, 50, "Tracks", "Track", WriteTrack);
            //WriteTracks(w);
            //WriteSpotlights(w);
            //WriteAutoNumbers(w);
            //WriteCoordinateSystem(w);
            //WriteTrackablePoints(w);
            
            w.WriteEndElement();
            w.WriteEndDocument();

            w.Flush();
            w.Close();
        }

        private void Initialize()
        {
            durationTimestamps = 100000;
            imageSize = random.NextSize(50, 4096);
            averageTimestampPerFrame = 1;
        }

        private void WriteGeneralInformation(XmlTextWriter w)
        {
            //long averageTimestampPerFrame = random.Next(1, 5000);
            
            double captureFPS = 1 + random.NextDouble() * 600;
            //long firstTimestamp = random.Next(0, 50);
            long firstTimestamp = 0;
            //long selectionStart = random.Next((int)firstTimestamp, (int)firstTimestamp + 100);
            long selectionStart = 0;


            w.WriteElementString("FormatVersion", KVA_VERSION);
            w.WriteElementString("Producer", producer);
            w.WriteElementString("OriginalFilename", Path.GetRandomFileName());
            w.WriteElementString("GlobalTitle", random.NextString(20));

            w.WriteElementString("ImageSize", string.Format("{0};{1}", imageSize.Width, imageSize.Height));
            w.WriteElementString("AverageTimeStampsPerFrame", averageTimestampPerFrame.ToString());
            w.WriteElementString("CaptureFramerate", string.Format(CultureInfo.InvariantCulture, "{0}", captureFPS));
            w.WriteElementString("FirstTimeStamp", firstTimestamp.ToString());
            w.WriteElementString("SelectionStart", selectionStart.ToString());

            WriteCalibrationHelp(w);
        }

        private void WriteCalibrationHelp(XmlTextWriter w)
        {
            bool line = random.NextBoolean();
            Array values = Enum.GetValues(typeof(LengthUnit));
            LengthUnit lengthUnit = (LengthUnit)values.GetValue(random.Next(values.Length));

            w.WriteStartElement("Calibration");

            if (line)
            {
                PointF origin = random.NextPointF(0, imageSize.Width, 0, imageSize.Height);
                float scale = (float)(random.NextDouble() * 50);
                
                w.WriteStartElement("CalibrationLine");
                w.WriteElementString("Origin", XmlHelper.WritePointF(origin));
                w.WriteElementString("Scale", string.Format(CultureInfo.InvariantCulture, "{0}", scale));
                w.WriteEndElement();
            }
            else
            {
                SizeF calibrationSize = random.NextSizeF(1, 100, 1, 100);
                QuadrilateralF quad = GetRandomQuadrilateral();
                PointF origin = new PointF(0, calibrationSize.Height);
                
                w.WriteStartElement("CalibrationPlane");

                w.WriteElementString("Size", XmlHelper.WriteSizeF(calibrationSize));

                w.WriteStartElement("Quadrilateral");
                w.WriteElementString("A", XmlHelper.WritePointF(quad.A));
                w.WriteElementString("B", XmlHelper.WritePointF(quad.B));
                w.WriteElementString("C", XmlHelper.WritePointF(quad.C));
                w.WriteElementString("D", XmlHelper.WritePointF(quad.D));
                w.WriteEndElement();

                w.WriteElementString("Origin", XmlHelper.WritePointF(origin));

                w.WriteEndElement();
            }
            
            w.WriteStartElement("Unit");
            w.WriteAttributeString("Abbreviation", UnitHelper.LengthAbbreviation(lengthUnit));
            w.WriteString(lengthUnit.ToString());
            w.WriteEndElement();

            w.WriteEndElement();
        }

        private void WriteKeyframes(XmlTextWriter w)
        {
            int keyframeCount = random.Next(0, 50);
            if (keyframeCount == 0)
                return;

            w.WriteStartElement("Keyframes");
            
            for(int i = 0; i < keyframeCount; i++)
            {
                w.WriteStartElement("Keyframe");
                WriteKeyframe(w);
                w.WriteEndElement();
            }
             
            w.WriteEndElement();
        }

        private void WriteKeyframe(XmlTextWriter w)
        {
            string position = random.Next((int)durationTimestamps).ToString();
            //string userTime = metadata.TimeCodeBuilder(position - metadata.SelectionStart, TimeType.Time, TimecodeFormat.Unknown, false);
            string userTime = position;
            string title = random.NextString(20);
            
            w.WriteStartElement("Position");
            w.WriteAttributeString("UserTime", userTime);
            w.WriteString(position.ToString());
            w.WriteEndElement();

            w.WriteElementString("Title", title);
            
            int drawingsCount = random.Next(0, 10);
            if (drawingsCount == 0)
                return;
                        
            w.WriteStartElement("Drawings");
            
            for(int i = 0; i < drawingsCount; i++)
                WriteDrawing(w);

            w.WriteEndElement();
        }
        
        private void WriteDrawing(XmlTextWriter w)
        {
            Guid id = Guid.NewGuid();

            // TODO: select random drawing tool and do a switch.
            w.WriteStartElement("Label");
            w.WriteAttributeString("id", id.ToString());
            WriteDrawingText(w);
            w.WriteEndElement();
        }

        private void WriteDrawingText(XmlTextWriter w)
        {
            string text = random.NextString(random.Next(5, 30));
            PointF location = random.NextPointF(0, imageSize.Width, 0, imageSize.Height);
            Color textColor = random.NextColor(255);
            
            w.WriteElementString("Text", text);
            w.WriteElementString("Position", XmlHelper.WritePointF(location));

            w.WriteStartElement("DrawingStyle");
            WriteDrawingStyleColor(w, "back color");
            WriteDrawingStyleFont(w, "font size");
            w.WriteEndElement();

            w.WriteStartElement("InfosFading");
            WriteInfosFading(w);
            w.WriteEndElement();
        }

        private void WriteExtraDrawings(XmlTextWriter w, int max, string name, string itemName, Action<XmlTextWriter> itemWriter)
        {
            int count = random.Next(0, max);
            if (count == 0)
                return;

            w.WriteStartElement(name);

            for (int i = 0; i < count; i++)
            {
                w.WriteStartElement(itemName);
                itemWriter(w);
                w.WriteEndElement();
            }

            w.WriteEndElement();	
        }
        
        private void WriteChrono(XmlTextWriter w)
        {
            PointF location = random.NextPointF(0, imageSize.Width, 0, imageSize.Height);
            long visibleTimestamp = random.Next((int)durationTimestamps);
            long startCountingTimestamp = random.Next((int)visibleTimestamp, (int)durationTimestamps);
            long stopCountingTimestamp = random.Next((int)startCountingTimestamp, (int)durationTimestamps);
            long invisibleTimestamp = random.Next((int)stopCountingTimestamp, (int)durationTimestamps);
            bool countdown = random.NextBoolean();
            string label = random.NextString(20);
            bool showLabel = random.NextBoolean();

            w.WriteElementString("Position", XmlHelper.WritePointF(location));

            w.WriteStartElement("Values");
            w.WriteElementString("Visible", visibleTimestamp.ToString());
            w.WriteElementString("StartCounting", startCountingTimestamp.ToString());
            w.WriteElementString("StopCounting", stopCountingTimestamp.ToString());
            w.WriteElementString("Invisible", invisibleTimestamp.ToString());
            w.WriteElementString("Countdown", countdown.ToString().ToLower());
            w.WriteEndElement();

            w.WriteStartElement("Label");
            w.WriteElementString("Text", label);
            w.WriteElementString("Show", showLabel.ToString().ToLower());
            w.WriteEndElement();

            w.WriteStartElement("DrawingStyle");
            WriteDrawingStyleColor(w, "color");
            WriteDrawingStyleFont(w, "font size");
            w.WriteEndElement();
        }

        private void WriteTrack(XmlTextWriter w)
        {
            long beginTimestamp = random.Next((int)durationTimestamps);

            Array viewValues = Enum.GetValues(typeof(TrackView));
            TrackView view = (TrackView)viewValues.GetValue(random.Next(viewValues.Length));
            Array extraDataValues = Enum.GetValues(typeof(TrackExtraData));
            TrackExtraData extraData = (TrackExtraData)extraDataValues.GetValue(random.Next(extraDataValues.Length));
            Array markerValues = Enum.GetValues(typeof(TrackMarker));
            TrackMarker marker = (TrackMarker)markerValues.GetValue(random.Next(markerValues.Length));
            bool displayBestFitCircle = random.NextBoolean();
            string mainLabelText = random.NextString(20);

            w.WriteElementString("TimePosition", beginTimestamp.ToString());
            w.WriteElementString("Mode", view.ToString());
            w.WriteElementString("ExtraData", extraData.ToString());
            w.WriteElementString("Marker", marker.ToString());
            w.WriteElementString("DisplayBestFitCircle", displayBestFitCircle.ToString().ToLower());

            WriteTrackerParameters(w);
            WriteTrackPoints(w, beginTimestamp);

            w.WriteStartElement("DrawingStyle");
            WriteDrawingStyleColor(w, "color");
            WriteDrawingStyleLineSize(w, "line size");
            WriteDrawingStyleTrackShape(w, "track shape");
            w.WriteEndElement();

            w.WriteStartElement("MainLabel");
            w.WriteAttributeString("Text", mainLabelText);
            PointF location = random.NextPointF(0, imageSize.Width, 0, imageSize.Height);
            w.WriteElementString("SpacePosition", XmlHelper.WritePointF(location));
            w.WriteElementString("TimePosition", "0");
            w.WriteEndElement();

            
            /*if (keyframesLabels.Count > 0)
            {
                w.WriteStartElement("KeyframeLabelList");
                w.WriteAttributeString("Count", keyframesLabels.Count.ToString());

                foreach (KeyframeLabel kfl in keyframesLabels)
                {
                    w.WriteStartElement("KeyframeLabel");
                    kfl.WriteXml(w);
                    w.WriteEndElement();
                }

                w.WriteEndElement();
            }*/
        }

        private void WriteTrackPoints(XmlTextWriter w, long beginTimestamp)
        {
            int count = random.Next(0, 1000);
            List<TrackPointBlock> positions = new List<TrackPointBlock>();

            for (int i = 0; i < count; i++)
            {
                PointF location = random.NextPointF(0, imageSize.Width, 0, imageSize.Height);
                long t = beginTimestamp + (i * averageTimestampPerFrame);
                TrackPointBlock point = new TrackPointBlock(location.X, location.Y, t);
                positions.Add(point);
            }

            w.WriteStartElement("TrackPointList");
            w.WriteAttributeString("Count", count.ToString());
            //w.WriteAttributeString("UserUnitLength", parentMetadata.CalibrationHelper.GetLengthAbbreviation());

            if (positions.Count > 0)
            {
                foreach (AbstractTrackPoint tp in positions)
                {
                    w.WriteStartElement("TrackPoint");

                    /*PointF p = parentMetadata.CalibrationHelper.GetPoint(tp.Point);
                    string userT = parentMetadata.TimeCodeBuilder(tp.T, TimeType.Time, TimecodeFormat.Unknown, false);

                    w.WriteAttributeString("UserX", String.Format("{0:0.00}", p.X));
                    w.WriteAttributeString("UserXInvariant", String.Format(CultureInfo.InvariantCulture, "{0:0.00}", p.X));
                    w.WriteAttributeString("UserY", String.Format("{0:0.00}", p.Y));
                    w.WriteAttributeString("UserYInvariant", String.Format(CultureInfo.InvariantCulture, "{0:0.00}", p.Y));
                    w.WriteAttributeString("UserTime", userT);*/

                    tp.WriteXml(w);
                    w.WriteEndElement();
                }
            }

            w.WriteEndElement();
        }

        private void WriteSpotlights(XmlTextWriter w)
        {

        }

        private void WriteAutoNumbers(XmlTextWriter w)
        {

        }

        private void WriteCoordinateSystem(XmlTextWriter w)
        {
            Guid id = Guid.NewGuid();
            bool visible = random.NextBoolean();
            
            w.WriteStartElement("CoordinateSystem");
            w.WriteAttributeString("id", id.ToString());

            w.WriteElementString("Visible", visible.ToString().ToLower());
            
            w.WriteEndElement();
        }

        private void WriteTrackablePoints(XmlTextWriter w)
        {

        }

        #region Common utilities
        private QuadrilateralF GetRandomQuadrilateral()
        {
            // Convex and with corners in each image quadrants.
            PointF a = random.NextPointF(0, imageSize.Width / 2, 0, imageSize.Height / 2);
            PointF b = random.NextPointF(imageSize.Width / 2, imageSize.Width, 0, imageSize.Height / 2);
            PointF c = random.NextPointF(imageSize.Width / 2, imageSize.Width, imageSize.Height / 2, imageSize.Height);
            PointF d = random.NextPointF(0, imageSize.Width / 2, imageSize.Height / 2, imageSize.Height);

            return new QuadrilateralF(a, b, c, d);
        }

        private void WriteTrackerParameters(XmlTextWriter w)
        {
            double similarityThreshold = random.NextDouble(0.5, 1.0);
            double templateUpdateThreshold = random.NextDouble(0.4, similarityThreshold);
            int refinementNeighborhood = random.Next(1, 4);
            Size referenceSearchWindow = new Size(imageSize.Width / 20, imageSize.Height / 20);
            Size searchWindow = random.NextSize(referenceSearchWindow.Width - 10, referenceSearchWindow.Width + 10, referenceSearchWindow.Height - 10, referenceSearchWindow.Height + 10);
            Size blockWindow = random.NextSize(4, searchWindow.Width, 4, searchWindow.Height + 10);

            w.WriteStartElement("TrackerParameters");
            w.WriteElementString("SimilarityThreshold", String.Format(CultureInfo.InvariantCulture, "{0}", similarityThreshold));
            w.WriteElementString("TemplateUpdateThreshold", String.Format(CultureInfo.InvariantCulture, "{0}", templateUpdateThreshold));
            w.WriteElementString("RefinementNeighborhood", String.Format(CultureInfo.InvariantCulture, "{0}", refinementNeighborhood));
            w.WriteElementString("SearchWindow", XmlHelper.WriteSizeF(searchWindow));
            w.WriteElementString("BlockWindow", XmlHelper.WriteSizeF(blockWindow));
            w.WriteEndElement();
        }
        
        private void WriteDrawingStyleColor(XmlTextWriter w, string key)
        {
            Color color = random.NextColor(255);
            
            w.WriteStartElement("Color");
            w.WriteAttributeString("Key", key);
            w.WriteElementString("Value", XmlHelper.WriteColor(color, true));
            w.WriteEndElement();
        }

        private void WriteDrawingStyleLineSize(XmlTextWriter w, string key)
        {
            int lineSize = random.Next(1, 20);

            w.WriteStartElement("LineSize");
            w.WriteAttributeString("Key", key);
            w.WriteElementString("Value", lineSize.ToString());
            w.WriteEndElement();
        }

        private void WriteDrawingStyleTrackShape(XmlTextWriter w, string key)
        {
            Array values = Enum.GetValues(typeof(DashStyle));
            DashStyle value = (DashStyle)values.GetValue(random.Next(values.Length));
            bool showSteps = random.NextBoolean();
            string output = string.Format("{0};{1}", value.ToString(), showSteps.ToString().ToLower());

            w.WriteStartElement("TrackShape");
            w.WriteAttributeString("Key", key);
            w.WriteElementString("Value", output);
            w.WriteEndElement();
        }

        private void WriteDrawingStyleFont(XmlTextWriter w, string key)
        {
            int fontSize = random.Next(6, 32);

            w.WriteStartElement("FontSize");
            w.WriteAttributeString("Key", key);
            w.WriteElementString("Value", fontSize.ToString());
            w.WriteEndElement();
        }

        private void WriteInfosFading(XmlTextWriter w)
        {
            bool enabled = random.NextBoolean();
            int fadingFrames = random.Next(100);
            bool alwaysVisible = random.NextBoolean();
            bool useDefault = random.NextBoolean();

            w.WriteElementString("Enabled", enabled.ToString().ToLower());
            w.WriteElementString("Frames", fadingFrames.ToString());
            w.WriteElementString("AlwaysVisible", alwaysVisible.ToString().ToLower());
            w.WriteElementString("UseDefault", useDefault.ToString().ToLower());
        }
        #endregion
    }
}
