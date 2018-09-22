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
    /// Add a summary as an xML comment to get stats on what was generated.
    /// </summary>
    public class KVAFuzzer20
    {
        private const string KVA_VERSION = "2.0";
        private Random random = new Random();
        private string producer;
        
        private long durationTimestamps;
        private Size imageSize;
        private long averageTimestampPerFrame;
        private List<TrackableDrawing> trackableDrawings = new List<TrackableDrawing>(); 

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
            WriteKeyframes(w, 20);
            //WriteExtraDrawings(w, 50, "Chronos", "Chrono", WriteChrono);
            //WriteExtraDrawings(w, 50, "Tracks", "Track", WriteTrack);
            //WriteExtraDrawings(w, 1000, "Spotlights", "Spotlight", WriteSpotlight);
            //WriteAutoNumbers(w, 100);
            WriteCoordinateSystem(w);
            WriteTrackablePoints(w);
            
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

        private void WriteKeyframes(XmlTextWriter w, int max)
        {
            int keyframeCount = random.Next(0, max);
            if (keyframeCount == 0)
                return;

            w.WriteStartElement("Keyframes");

            List<int> times = new List<int>();
            for (int i = 0; i < keyframeCount; i++)
                times.Add(random.Next((int)durationTimestamps));

            times.Sort();
            
            for(int i = 0; i < keyframeCount; i++)
            {
                w.WriteStartElement("Keyframe");
                WriteKeyframe(w, times[i]);
                w.WriteEndElement();
            }
             
            w.WriteEndElement();
        }

        private void WriteKeyframe(XmlTextWriter w, int time)
        {
            string position = time.ToString();
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
                WriteDrawing(w, time);

            w.WriteEndElement();
        }
        
        private void WriteDrawing(XmlTextWriter w, long time)
        {
            Guid id = Guid.NewGuid();

            int drawing = random.Next(6, 7);

            switch(drawing)
            {
                case 0:
                    w.WriteStartElement("Label");
                    w.WriteAttributeString("id", id.ToString());
                    WriteDrawingText(w);
                    w.WriteEndElement();
                    break;
                case 1:
                    w.WriteStartElement("Angle");
                    w.WriteAttributeString("id", id.ToString());
                    WriteDrawingAngle(w, id, time);
                    w.WriteEndElement();
                    break;
                case 2:
                    w.WriteStartElement("CrossMark");
                    w.WriteAttributeString("id", id.ToString());
                    WriteDrawingCrossMark(w, id, time);
                    w.WriteEndElement();
                    break;
                case 3:
                    w.WriteStartElement("Line");
                    w.WriteAttributeString("id", id.ToString());
                    WriteDrawingLine(w, id, time);
                    w.WriteEndElement();
                    break;
                case 4:
                    w.WriteStartElement("Circle");
                    w.WriteAttributeString("id", id.ToString());
                    WriteDrawingCircle(w);
                    w.WriteEndElement();
                    break;
                case 5:
                    w.WriteStartElement("Pencil");
                    w.WriteAttributeString("id", id.ToString());
                    WriteDrawingPencil(w);
                    w.WriteEndElement();
                    break;
                case 6:
                    w.WriteStartElement("Plane");
                    w.WriteAttributeString("id", id.ToString());
                    WriteDrawingPlane(w, id, time);
                    w.WriteEndElement();
                    break;
            }
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

            WriteInfosFading(w);
        }

        private void WriteDrawingAngle(XmlTextWriter w, Guid id, long time)
        {
            PointF o = random.NextPointF(0, imageSize.Width, 0, imageSize.Height);
            PointF a = random.NextPointF(0, imageSize.Width, 0, imageSize.Height);
            PointF b = random.NextPointF(0, imageSize.Width, 0, imageSize.Height);

            w.WriteElementString("PointO", XmlHelper.WritePointF(o));
            w.WriteElementString("PointA", XmlHelper.WritePointF(a));
            w.WriteElementString("PointB", XmlHelper.WritePointF(b));

            w.WriteStartElement("DrawingStyle");
            WriteDrawingStyleColor(w, "line color");
            w.WriteEndElement();

            WriteInfosFading(w);

            bool tracked = random.NextBoolean();
            if (!tracked)
                return;

            List<string> pointKeys = new List<string>() { "o", "a", "b" };
            trackableDrawings.Add(new TrackableDrawing(id, time, pointKeys));
        }

        private void WriteDrawingCrossMark(XmlTextWriter w, Guid id, long time)
        {
            PointF center = random.NextPointF(0, imageSize.Width, 0, imageSize.Height);
            bool measurableInfoVisible = random.NextBoolean();

            w.WriteElementString("CenterPoint", XmlHelper.WritePointF(center));
            w.WriteElementString("CoordinatesVisible", measurableInfoVisible.ToString().ToLower());

            w.WriteStartElement("DrawingStyle");
            WriteDrawingStyleColor(w, "back color");
            w.WriteEndElement();

            WriteInfosFading(w);

            bool tracked = random.NextBoolean();
            if (!tracked)
                return;

            List<string> pointKeys = new List<string>() { "0" };
            trackableDrawings.Add(new TrackableDrawing(id, time, pointKeys));
        }

        private void WriteDrawingLine(XmlTextWriter w, Guid id, long time)
        {
            PointF a = random.NextPointF(0, imageSize.Width, 0, imageSize.Height);
            PointF b = random.NextPointF(0, imageSize.Width, 0, imageSize.Height);
            bool measurableInfoVisible = random.NextBoolean();

            w.WriteElementString("Start", XmlHelper.WritePointF(a));
            w.WriteElementString("End", XmlHelper.WritePointF(b));
            w.WriteElementString("MeasureVisible", measurableInfoVisible.ToString().ToLower());

            w.WriteStartElement("DrawingStyle");
            WriteDrawingStyleColor(w, "color");
            WriteDrawingStyleLineSize(w, "line size");
            WriteDrawingStyleArrows(w, "arrows");
            w.WriteEndElement();

            WriteInfosFading(w);

            bool tracked = random.NextBoolean();
            if (!tracked)
                return;

            List<string> pointKeys = new List<string>() { "a", "b" };
            trackableDrawings.Add(new TrackableDrawing(id, time, pointKeys));
        }

        private void WriteDrawingCircle(XmlTextWriter w)
        {
            PointF center = random.NextPointF(0, imageSize.Width, 0, imageSize.Height);
            int radius = random.Next(200);

            w.WriteElementString("Origin", XmlHelper.WritePointF(center));
            w.WriteElementString("Radius", radius.ToString());

            w.WriteStartElement("DrawingStyle");
            WriteDrawingStyleColor(w, "color");
            WriteDrawingStylePenSize(w, "pen size");
            w.WriteEndElement();
            
            WriteInfosFading(w);
        }

        private void WriteDrawingPencil(XmlTextWriter w)
        {
            int count = random.Next(1, 200);

            w.WriteStartElement("PointList");
            w.WriteAttributeString("Count", count.ToString());
            List<PointF> points = GetRandomTrajectory(count);
            for (int i = 0; i < count; i++)
                w.WriteElementString("Point", XmlHelper.WritePointF(points[i]));

            w.WriteEndElement();

            w.WriteStartElement("DrawingStyle");
            WriteDrawingStyleColor(w, "color");
            WriteDrawingStylePenSize(w, "pen size");
            w.WriteEndElement();

            WriteInfosFading(w);
        }

        private void WriteDrawingPlane(XmlTextWriter w, Guid id, long time)
        {
            QuadrilateralF quadImage = GetRandomQuadrilateral();
            bool inPerspective = random.NextBoolean();
            if (!inPerspective)
            {
                quadImage.C = new PointF(quadImage.B.X, quadImage.D.Y);
                quadImage.MakeRectangle(0);
            }

            w.WriteElementString("PointUpperLeft", XmlHelper.WritePointF(quadImage.A));
            w.WriteElementString("PointUpperRight", XmlHelper.WritePointF(quadImage.B));
            w.WriteElementString("PointLowerRight", XmlHelper.WritePointF(quadImage.C));
            w.WriteElementString("PointLowerLeft", XmlHelper.WritePointF(quadImage.D));

            w.WriteElementString("Perspective", inPerspective.ToString().ToLower());

            w.WriteStartElement("DrawingStyle");
            WriteDrawingStyleColor(w, "color");
            WriteDrawingStyleGridDivisions(w, "divisions");
            w.WriteEndElement();

            WriteInfosFading(w);

            bool tracked = random.NextBoolean();
            if (!tracked)
                return;

            List<string> pointKeys = new List<string>() { "0", "1", "2", "3" };
            trackableDrawings.Add(new TrackableDrawing(id, time, pointKeys));
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

                foreach (MiniLabel kfl in keyframesLabels)
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
            List<PointF> points = GetRandomTrajectory(count);
            for (int i = 0; i < count; i++)
            {
                long t = beginTimestamp + (i * averageTimestampPerFrame);
                TrackPointBlock point = new TrackPointBlock(points[i].X, points[i].Y, t);
                positions.Add(point);
            }

            w.WriteStartElement("TrackPointList");
            w.WriteAttributeString("Count", count.ToString());

            if (positions.Count > 0)
            {
                foreach (AbstractTrackPoint tp in positions)
                {
                    w.WriteStartElement("TrackPoint");

                    tp.WriteXml(w);
                    w.WriteEndElement();
                }
            }

            w.WriteEndElement();
        }

        private void WriteSpotlight(XmlTextWriter w)
        {
            Guid id = Guid.NewGuid();
            w.WriteAttributeString("id", id.ToString());

            long time = random.Next((int)durationTimestamps);
            //long time = random.Next((int)1000);

            PointF location = random.NextPointF(0, imageSize.Width, 0, imageSize.Height);
            int radius = random.Next(5, 100);

            w.WriteElementString("Time", time.ToString());
            w.WriteElementString("Center", XmlHelper.WritePointF(location));
            w.WriteElementString("Radius", radius.ToString());
        }

        private void WriteAutoNumbers(XmlTextWriter w, int max)
        {
            string name = "AutoNumbers";
            string itemName = "AutoNumber";

            w.WriteStartElement(name);

            w.WriteStartElement("DrawingStyle");
            WriteDrawingStyleColor(w, "back color");
            WriteDrawingStyleFont(w, "font size");
            w.WriteEndElement();

            int count = random.Next(0, max);
            if (count == 0)
                return;

            for (int i = 0; i < count; i++)
            {
                w.WriteStartElement(itemName);
                WriteAutoNumber(w, i);
                w.WriteEndElement();
            }

            w.WriteEndElement();
        }
        
        private void WriteAutoNumber(XmlTextWriter w, int value)
        {
            //long time = random.Next((int)durationTimestamps);
            long time = random.Next((int)1000);
            PointF location = random.NextPointF(0, imageSize.Width, 0, imageSize.Height);

            w.WriteElementString("Time", time.ToString());
            w.WriteElementString("Location", XmlHelper.WritePointF(location));
            w.WriteElementString("Value", value.ToString());
        }

        private void WriteCoordinateSystem(XmlTextWriter w)
        {
            Guid id = Guid.NewGuid();
            bool visible = random.NextBoolean();
            
            w.WriteStartElement("CoordinateSystem");
            w.WriteAttributeString("id", id.ToString());

            w.WriteElementString("Visible", visible.ToString().ToLower());
            
            w.WriteStartElement("DrawingStyle");
            WriteDrawingStyleColor(w, "line color");
            w.WriteEndElement();
            
            w.WriteEndElement();
        }

        private void WriteTrackablePoints(XmlTextWriter w)
        {
            if (trackableDrawings.Count == 0)
                return;

            w.WriteStartElement("Trackability");

            foreach (TrackableDrawing drawing in trackableDrawings)
            {
                bool tracking = random.NextBoolean();
                w.WriteStartElement("TrackableDrawing");
                w.WriteAttributeString("id", drawing.DrawingId.ToString());
                w.WriteAttributeString("tracking", tracking.ToString().ToLower());

                int frameCount = random.Next(200);
                foreach (string key in drawing.PointKeys)
                {
                    w.WriteStartElement("TrackablePoint");
                    w.WriteAttributeString("key", key);
                    WriteTrackablePoint(w, frameCount, drawing.Time);
                    w.WriteEndElement();
                }
                w.WriteEndElement();
            }

            w.WriteEndElement();
        }

        private void WriteTrackablePoint(XmlTextWriter w, int frameCount, long time)
        {
            PointF nonTrackingValue = random.NextPointF(0, imageSize.Width, 0, imageSize.Height);
            PointF currentValue = random.NextPointF(0, imageSize.Width, 0, imageSize.Height);
            
            WriteTrackerParameters(w);
            w.WriteElementString("NonTrackingValue", XmlHelper.WritePointF(nonTrackingValue));
            w.WriteElementString("CurrentValue", XmlHelper.WritePointF(currentValue));

            w.WriteStartElement("Timeline");
            List<PointF> points = GetRandomTrajectory(frameCount);

            for (int i = 0; i < frameCount; i++)
            {
                Array values = Enum.GetValues(typeof(PositionningSource));
                PositionningSource source = (PositionningSource)values.GetValue(random.Next(values.Length));
                long currentTime = time + (i * averageTimestampPerFrame);
                w.WriteStartElement("Frame");
                w.WriteAttributeString("time", currentTime.ToString());
                w.WriteAttributeString("location", XmlHelper.WritePointF(points[i]));
                w.WriteAttributeString("source", source.ToString());
                w.WriteEndElement();
            }
                
            w.WriteEndElement();
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

        private List<PointF> GetRandomTrajectory(int count)
        {
            PointF current = random.NextPointF(0, imageSize.Width, 0, imageSize.Height);
            List<PointF> trajectory = new List<PointF>();
            
            for (int i = 0; i < count; i++)
            {
                double x = random.NextDouble(-10, 11);
                double y = random.NextDouble(-10, 11);
                current = new PointF((float)(current.X + x), (float)(current.Y + y));
                
                trajectory.Add(current);
            }

            return trajectory;
        }

        private void WriteTrackerParameters(XmlTextWriter w)
        {
            double templateUpdateThreshold = random.NextDouble(0.5, 1.0);
            double similarityThreshold = random.NextDouble(0.4, templateUpdateThreshold);
            int refinementNeighborhood = random.Next(1, 4);
            Size referenceSearchWindow = new Size(imageSize.Width / 20, imageSize.Height / 20);
            Size searchWindow = random.NextSize(referenceSearchWindow.Width - 10, referenceSearchWindow.Width + 10, referenceSearchWindow.Height - 10, referenceSearchWindow.Height + 10);
            Size blockWindow = random.NextSize(4, Math.Max(searchWindow.Width, 5), 4, Math.Max(searchWindow.Height, 5));

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
            int lineSize = StyleElementLineSize.options[random.Next(StyleElementLineSize.options.Count)];

            w.WriteStartElement("LineSize");
            w.WriteAttributeString("Key", key);
            w.WriteElementString("Value", lineSize.ToString());
            w.WriteEndElement();
        }

        private void WriteDrawingStyleTrackShape(XmlTextWriter w, string key)
        {
            TrackShape value = StyleElementTrackShape.options[random.Next(StyleElementTrackShape.options.Count)];
            StyleElementTrackShape style = new StyleElementTrackShape(value);
            
            w.WriteStartElement("TrackShape");
            w.WriteAttributeString("Key", key);
            style.WriteXml(w);
            w.WriteEndElement();
        }

        private void WriteDrawingStyleFont(XmlTextWriter w, string key)
        {
            //string fontSize = StyleElementFontSize.options[random.Next(StyleElementFontSize.options.Count)];

            //w.WriteStartElement("FontSize");
            //w.WriteAttributeString("Key", key);
            //w.WriteElementString("Value", fontSize);
            //w.WriteEndElement();
        }

        private void WriteDrawingStyleGridDivisions(XmlTextWriter w, string key)
        {
            string size = StyleElementGridDivisions.options[random.Next(StyleElementGridDivisions.options.Count)].ToString();

            w.WriteStartElement("GridDivisions");
            w.WriteAttributeString("Key", key);
            w.WriteElementString("Value", size);
            w.WriteEndElement();
        }

        private void WriteDrawingStylePenSize(XmlTextWriter w, string key)
        {
            int size = StyleElementPenSize.options[random.Next(StyleElementPenSize.options.Count)];

            w.WriteStartElement("PenSize");
            w.WriteAttributeString("Key", key);
            w.WriteElementString("Value", size.ToString());
            w.WriteEndElement();
        }

        private void WriteDrawingStyleArrows(XmlTextWriter w, string key)
        {
            LineEnding value = StyleElementLineEnding.options[random.Next(StyleElementLineEnding.options.Count)];
            StyleElementLineEnding style = new StyleElementLineEnding(value);

            w.WriteStartElement("Arrows");
            w.WriteAttributeString("Key", key);
            style.WriteXml(w);
            w.WriteEndElement();
        }

        private void WriteInfosFading(XmlTextWriter w)
        {

            bool enabled = random.NextBoolean();
            int fadingFrames = random.Next(1, 100);
            bool alwaysVisible = random.NextBoolean();
            bool useDefault = random.NextBoolean();

            w.WriteStartElement("InfosFading");
            w.WriteElementString("Enabled", enabled.ToString().ToLower());
            w.WriteElementString("Frames", fadingFrames.ToString());
            w.WriteElementString("AlwaysVisible", alwaysVisible.ToString().ToLower());
            w.WriteElementString("UseDefault", useDefault.ToString().ToLower());
            w.WriteEndElement();
        }
        #endregion
    }
}
