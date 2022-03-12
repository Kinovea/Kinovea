using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Kinovea.Services;
using System.Text;
using System.Drawing;

namespace Kinovea.ScreenManager
{
    public class ExporterJSON
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void Export(string path, MeasuredData md)
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                writer.Formatting = Formatting.Indented;

                writer.WriteStartObject();

                WriteMetadata(writer, md);
                WriteData(writer, md);
                
                writer.WriteEndObject();

                if (sb.Length >= 0)
                    File.WriteAllText(path, sb.ToString());
            }
        }

        private void WriteMetadata(JsonWriter w, MeasuredData md)
        {
            w.WritePropertyName("metadata");
            w.WriteStartObject();

            w.WritePropertyName("producer");
            w.WriteValue(md.Producer);
            w.WritePropertyName("fullPath");
            w.WriteValue(md.FullPath);
            w.WritePropertyName("originalFilename");
            w.WriteValue(md.OriginalFilename);
            w.WritePropertyName("imageSize");
            WriteSize(w, md.ImageSize);
            w.WritePropertyName("captureFramerate");
            w.WriteValue(md.CaptureFramerate);
            w.WritePropertyName("userFramerate");
            w.WriteValue(md.UserFramerate);
            w.WriteEndObject();
        }

        private void WriteSize(JsonWriter w, Size size)
        {
            w.WriteStartObject();
            w.WritePropertyName("width");
            w.WriteValue(size.Width);
            w.WritePropertyName("height");
            w.WriteValue(size.Height);
            w.WriteEndObject();
        }

        private void WriteData(JsonWriter w, MeasuredData md)
        {
            w.WritePropertyName("data");
            w.WriteStartObject();

            WriteKeyframes(w, md);
            WritePositions(w, md);
            WriteDistances(w, md);
            WriteAngles(w, md);
            WriteTimes(w, md);
            WriteTimeseries(w, md);

            w.WriteEndObject();
        }

        private void WriteKeyframes(JsonWriter w, MeasuredData md)
        {
            w.WritePropertyName("keyframes");
            w.WriteStartArray();

            foreach (var kf in md.Keyframes)
            {
                w.WriteStartObject();
                
                w.WritePropertyName("name");
                w.WriteValue(kf.Name);
                w.WritePropertyName("timeUnit");
                w.WriteValue(md.Units.TimeSymbol);
                w.WritePropertyName("time");
                w.WriteValue(kf.Time);
                
                
                w.WriteEndObject();
            }

            w.WriteEndArray();
        }

        private void WritePositions(JsonWriter w, MeasuredData md)
        {
            w.WritePropertyName("positions");
            w.WriteStartArray();

            foreach (var o in md.Positions)
            {
                w.WriteStartObject();

                w.WritePropertyName("name");
                w.WriteValue(o.Name);
                w.WritePropertyName("timeUnit");
                w.WriteValue(md.Units.TimeSymbol);
                w.WritePropertyName("dataUnit");
                w.WriteValue(md.Units.LengthSymbol);
                w.WritePropertyName("time");
                w.WriteValue(o.Time);
                w.WritePropertyName("value");
                w.WriteStartArray();
                w.WriteValue(o.X);
                w.WriteValue(o.Y);
                w.WriteEndArray();

                w.WriteEndObject();
            }

            w.WriteEndArray();
        }

        private void WriteDistances(JsonWriter w, MeasuredData md)
        {
            w.WritePropertyName("distances");
            w.WriteStartArray();

            foreach (var o in md.Distances)
            {
                w.WriteStartObject();

                w.WritePropertyName("name");
                w.WriteValue(o.Name);
                w.WritePropertyName("timeUnit");
                w.WriteValue(md.Units.TimeSymbol);
                w.WritePropertyName("dataUnit");
                w.WriteValue(md.Units.LengthSymbol);
                w.WritePropertyName("time");
                w.WriteValue(o.Time);
                w.WritePropertyName("value");
                w.WriteValue(o.Value);
                
                w.WriteEndObject();
            }

            w.WriteEndArray();
        }

        private void WriteAngles(JsonWriter w, MeasuredData md)
        {
            w.WritePropertyName("angles");
            w.WriteStartArray();

            foreach (var o in md.Angles)
            {
                w.WriteStartObject();

                w.WritePropertyName("name");
                w.WriteValue(o.Name);
                w.WritePropertyName("timeUnit");
                w.WriteValue(md.Units.TimeSymbol);
                w.WritePropertyName("dataUnit");
                w.WriteValue(md.Units.AngleSymbol);
                w.WritePropertyName("time");
                w.WriteValue(o.Time);
                w.WritePropertyName("value");
                w.WriteValue(o.Value);

                w.WriteEndObject();
            }

            w.WriteEndArray();
        }

        private void WriteTimes(JsonWriter w, MeasuredData md)
        {
            w.WritePropertyName("times");
            w.WriteStartArray();

            foreach (var o in md.Times)
            {
                w.WriteStartObject();

                w.WritePropertyName("name");
                w.WriteValue(o.Name);
                w.WritePropertyName("timeUnit");
                w.WriteValue(md.Units.TimeSymbol);
                w.WritePropertyName("duration");
                w.WriteValue(o.Duration);
                w.WritePropertyName("start");
                w.WriteValue(o.Start);
                w.WritePropertyName("stop");
                w.WriteValue(o.Stop);

                w.WriteEndObject();
            }

            w.WriteEndArray();
        }

        private void WriteTimeseries(JsonWriter w, MeasuredData md)
        {
            // For time series we follow the Kinetics Toolkit model: 
            // First output an array of times, then an array of matching samples.
            // Each sample might itself be a multi-dimensional array, in our case a 2D array of spatial coordinates.
            w.WritePropertyName("timeseries");
            w.WriteStartArray();

            foreach (var o in md.Timeseries)
            {
                w.WriteStartObject();

                w.WritePropertyName("name");
                w.WriteValue(o.Name);
                w.WritePropertyName("timeUnit");
                w.WriteValue(md.Units.TimeSymbol);
                w.WritePropertyName("dataUnit");
                w.WriteValue(md.Units.LengthSymbol);

                WriteTimelineTimes(w, o);
                WriteTimelineData(w, o);

                w.WriteEndObject();
            }

            w.WriteEndArray();
        }

        private void WriteTimelineTimes(JsonWriter w, MeasuredDataTimeseries tl)
        {
            w.WritePropertyName("time");
            w.WriteStartArray();
            foreach (var t in tl.Times)
                w.WriteValue(t);
            w.WriteEndArray();
        }

        private void WriteTimelineData(JsonWriter w, MeasuredDataTimeseries tl)
        {
            w.WritePropertyName("data");
            w.WriteStartObject();

            // Export all tracked points.
            foreach (var pair in tl.Data)
            {
                w.WritePropertyName(pair.Key);

                // Export all coordinates of this tracked point.
                w.WriteStartArray();
                foreach (var c in pair.Value)
                {
                    w.WriteStartArray();
                    w.WriteValue(c.X);
                    w.WriteValue(c.Y);
                    w.WriteEndArray();
                }
                w.WriteEndArray();
            }
            w.WriteEndObject();
        }
    }
}
