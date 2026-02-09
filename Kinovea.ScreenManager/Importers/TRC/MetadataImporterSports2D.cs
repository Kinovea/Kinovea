using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Kinovea.ScreenManager.Languages;
using Kinovea.ScreenManager.Properties;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Imports TRC from Sports2D (this class is not a generic TRC parser).
    /// For now this is for qualitative analysis of trajectories and posture over time, not measurement.
    /// 
    /// Example command line used for testing:
    /// > sports2d --video_input "video.mp4" --filter False --display_angle_values_on None 
    /// 
    /// --filter False: is important to get the stick figure to match the video, the filtering will be done in Kinovea.
    /// --display_angle_values_on None: just to avoid cluttering the resulting video for comparison purposes.
    /// 
    /// - Do not use multiperson=False unless there is really only one person throughout the video, 
    /// otherwise it will jump from one person to another.
    /// </summary>
    public static class MetadataImporterSports2D
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static char[] delim = new char[] { '\t' };

        public static void Import(Metadata metadata, string source, bool isFile)
        {
            // TRC Format description:
            // https://opensimconfluence.atlassian.net/wiki/spaces/OpenSim/pages/53089972/Marker+.trc+Files
            // The .trc (Track Row Column) file format was created by Motion Analysis Corporation to specify
            // the positions of markers placed on a subject at different times during a motion capture trial. 
            // The first three lines of the .trc file is a header, followed by two rows of column labels,
            // followed by a blank row, followed by the rows of data.
            string text = isFile ? File.ReadAllText(source, Encoding.UTF8) : source;
            string[] allLines = text.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.None);

            
            // Headers.
            //string[] head1 = allLines[0].Split(delim);
            //string[] head2 = allLines[1].Split(delim);
            string[] head3 = allLines[2].Split(delim);
            string[] head4 = allLines[3].Split(delim, StringSplitOptions.RemoveEmptyEntries);
            //string[] head5 = allLines[4].Split(delim);

            // Only pick the relevant info and assume the layout from Sports2D, which is also the standard layout.
            float dataRate = float.Parse(head3[0]);
            float cameraRate = float.Parse(head3[1]);
            int numFrames = int.Parse(head3[2]);
            int numMarkers = int.Parse(head3[3]);

            // Get marker names.
            if (head4.Length != numMarkers + 2)
            {
                throw new InvalidDataException();
            }

            List<string> markers = new List<string>();
            for (int i = 0; i < numMarkers; i++)
            {
                markers.Add(head4[i+2]);
            }

            // Create a drawing.
            // Model: BodyWithFeet from HALPE_26 (full-body without hands, for RTMPose, AlphaPose, MMPose, etc.)
            // https://github.com/MVIG-SJTU/AlphaPose/blob/master/docs/MODEL_ZOO.md
            // https://github.com/open-mmlab/mmpose/tree/main/projects/rtmpose
            string toolName = "BodyWithFeet";
            DrawingToolGenericPosture tool = ToolManager.Tools[toolName] as DrawingToolGenericPosture;
            if (tool == null)
            {
                throw new InvalidProgramException();
            }

            DrawingGenericPosture drawing = null;

            // Prepare track data for each marker.
            // This should simulate DrawingTrack > ParseTrackPointList.
            //Dictionary<string, List<TimedPoint>> timelines = new Dictionary<string, List<TimedPoint>>();
            List<List<TimedPoint>> timelines = new List<List<TimedPoint>>();
            for (int i = 0; i < markers.Count; i++)
            {
                timelines.Add(new List<TimedPoint>());
            }

            // Parse the data into the drawing object.
            // Each row of data contains a frame number followed by a time value followed by the (x, y, z) coordinates of each marker.
            for (int i = 5; i < numFrames; i++)
            {
                int frameIndex = i - 5;
                long timestamp = (long)Math.Round(metadata.FirstTimeStamp + (frameIndex * metadata.AverageTimeStampsPerFrame));

                GenericPosture posture = GenericPostureManager.Instanciate(tool.ToolId, true);
                ParsePosture(posture, allLines[i], markers);
                
                if (frameIndex == 0)
                {
                    drawing = new DrawingGenericPosture(tool.ToolId, PointF.Empty, posture, timestamp, metadata.AverageTimeStampsPerFrame, ToolManager.GetDefaultStyleElements(toolName));
                    drawing.Name = "Human";

                    // Add key frame.
                    Guid id = Guid.NewGuid();
                    long position = timestamp;
                    string title = null;
                    Color color = Keyframe.DefaultColor;
                    string comments = "";
                    var drawingList = new List<AbstractDrawing>(){ drawing };
                    Keyframe keyframe = new Keyframe(id, position, title, color, comments, drawingList, metadata);
                    metadata.MergeInsertKeyframe(keyframe);

                    // At this point the drawing should be added to the trackability manager.
                }

                for (int j = 0; j < markers.Count; j++)
                {
                    PointF p = posture.GetValue(markers[j]);
                    timelines[j].Add(new TimedPoint(p.X, p.Y, timestamp));
                }
            }

            // Create the DrawingTrack objects from the collected timelines and add them to metadata.
            log.DebugFormat("Import: Creating {0} tracks for Sports2D.", timelines.Count);
            Dictionary<string, DrawingTrack> tracks = new Dictionary<string, DrawingTrack>();
            for (int i = 0; i < timelines.Count; i++)
            {
                Color color = drawing.GenericPosture.Points[i].Color;
                int lineSize = 1;
                var styleElements = GetStyleElements(color, lineSize);
                string trackName = string.Format("{0}.{1}", drawing.Name, markers[i]);
                
                DrawingTrack track = new DrawingTrack(
                    trackName, 
                    timelines[i], 
                    metadata.AverageTimeStampsPerFrame,
                    styleElements);

                // Trackable points inside the generic posture drawing are identified by their index,
                // not by the custom name of the point.
                tracks.Add(i.ToString(), track);
                metadata.AddDrawing(metadata.TrackManager.Id, track);
            }

            log.DebugFormat("Import: Creating DrawingTracker.");
            // Import the trackable points into a tracker and add the tracker to the manager.
            DrawingTracker drawingTracker = new DrawingTracker(drawing, tracks);
            metadata.TrackabilityManager.ImportTracker(drawingTracker, tracks.Values.ToList());
        }

        private static StyleElements GetStyleElements(Color color, int lineSize)
        {
            // Mostly defaults except for color and line size.
            var styleElements = new StyleElements();
            styleElements.Elements.Add("color", new StyleElementColor(color));
            styleElements.Elements.Add("track shape", new StyleElementTrackShape(TrackShape.Solid));
            styleElements.Elements.Add("line size", new StyleElementLineSize(lineSize));
            styleElements.Elements.Add("TrackPointSize", new StyleElementPenSize(3));
            styleElements.Elements.Add("label size", new StyleElementFontSize(8, ScreenManagerLang.StyleElement_FontSize_LabelSize));
            return styleElements;
        }

        private static void ParsePosture(GenericPosture posture, string line, List<string> markers)
        {
            string[] values = line.Split(delim);

            // Loop through columns by groups of 3 and assign the value to the posture object.
            int markerIndex = 0;
            for (int i = 2; i < values.Length; i+=3)
            {
                // Parse values.
                // Coordinate system of Sports2D:
                // - Y inverted, Values divided by 1000, Z always 0.
                float x = float.Parse(values[i + 0]) * 1000;
                float y = float.Parse(values[i + 1]) * (-1000);

                // Assign the value to the matching point.
                posture.AssignValue(markers[markerIndex], new PointF(x, y));

                markerIndex++;
            }
        }
    }
}
