using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using Kinovea.ScreenManager.Properties;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Imports TRC from Sports2D (not a generic TRC parser).
    /// This is for qualitative analysis of trajectories and posture over time, not measurement.
    /// 
    /// Example command line used for testing:
    /// sports2d --video_input "video.mp4" --filter False --display_angle_values_on None 
    /// --filter False: is important to get the stick figure to match the video, the filtering will be done in Kinovea.
    /// --display_angle_values_on None: just to avoid cluttering the resulting video for comparison purposes.
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
            Dictionary<string, Timeline<TrackingTemplate>> timelines = new Dictionary<string, Timeline<TrackingTemplate>>();
            foreach (string marker in markers)
            {
                timelines.Add(marker, new Timeline<TrackingTemplate>());
            }

            // Parse the data into the drawing.
            // Each row of data contains a frame number followed by a time value followed by the (x, y, z) coordinates of each marker.
            for (int i = 5; i < numFrames; i++)
            {
                int frameIndex = i - 5;
                long timestamp = metadata.FirstTimeStamp + (frameIndex * metadata.AverageTimeStampsPerFrame);

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
                    Keyframe keyframe = new Keyframe(id, position, title, color, comments, new List<AbstractDrawing>() { drawing }, metadata);
                    metadata.MergeInsertKeyframe(keyframe);
                    
                    // At this point the drawing should be added to the trackability manager.
                }

                foreach (var marker in markers)
                {
                    PointF value = posture.GetValue(marker);
                    TrackingTemplate frame = new TrackingTemplate(timestamp, value, TrackingSource.Auto);
                    timelines[marker].Insert(timestamp, frame);
                }
            }

            // Create the trackable points from the timelines.
            TrackingParameters trackingParameters = PreferencesManager.PlayerPreferences.TrackingParameters.Clone();
            Dictionary<string, TrackablePoint> trackablePoints = new Dictionary<string, TrackablePoint>();
            int index = 0;
            foreach (var marker in markers)
            {
                PointF currentValue = timelines[marker].ClosestFrom(0).Location;
                TrackablePoint trackablePoint = new TrackablePoint(trackingParameters, currentValue, timelines[marker]);
                trackablePoints.Add(index.ToString(), trackablePoint);
                index++;
            }

            // Import the trackable points into a tracker and add the tracker to the manager.
            DrawingTracker drawingTracker = new DrawingTracker(drawing, trackablePoints);
            metadata.TrackabilityManager.ImportTracker(drawingTracker);
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
