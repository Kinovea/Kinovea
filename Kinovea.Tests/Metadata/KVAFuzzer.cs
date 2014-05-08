using System;
using System.Drawing;
using System.Text;
using System.Xml;
using System.Reflection;
using System.IO;
using System.Globalization;
using Kinovea.ScreenManager;
using Kinovea.Services;

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
            WriteKeyframes(w);
            WriteChronos(w);
            /*WriteTracks(w);
            WriteSpotlights(w);
            WriteAutoNumbers(w);
            WriteCoordinateSystem(w);
            WriteTrackablePoints(w);*/
            
            w.WriteEndElement();
            w.WriteEndDocument();

            w.Flush();
            w.Close();
        }

        private void Initialize()
        {
            //durationTimestamps = random.Next(10000);
            durationTimestamps = 100000;
            imageSize = random.NextSize(50, 4096);
        }

        private void WriteGeneralInformation(XmlTextWriter w)
        {
            //long averageTimestampPerFrame = random.Next(1, 5000);
            long averageTimestampPerFrame = 1;
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
        
        private void WriteChronos(XmlTextWriter w)
        {
            int count = random.Next(0, 50);
            if (count == 0)
                return;

            w.WriteStartElement("Chronos");
            
            for(int i=0; i < count; i++)
            {
                w.WriteStartElement("Chrono");
                WriteChrono(w);
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
        
        private void WriteDrawingStyleColor(XmlTextWriter w, string key)
        {
            Color color = random.NextColor(255);
            
            w.WriteStartElement("Color");
            w.WriteAttributeString("Key", key);
            w.WriteElementString("Value", XmlHelper.WriteColor(color, true));
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
