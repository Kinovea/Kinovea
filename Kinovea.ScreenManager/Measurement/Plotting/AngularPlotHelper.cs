using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// A helper class to group utilities useful for both angular kinematics and angle-angle plots.
    /// </summary>
    public static class AngularPlotHelper
    {
        private static AngularKinematics angularKinematics = new AngularKinematics();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        public static void ImportData(Metadata metadata, List<TimeSeriesPlotData> timeSeriesData)
        {
            ImportAngleDrawingsData(metadata, timeSeriesData);
            ImportCustomDrawingsData(metadata, timeSeriesData);
        }

        private static void ImportAngleDrawingsData(Metadata metadata, List<TimeSeriesPlotData> timeSeriesData)
        {
            // Create three filtered trajectories named o, a, b directly based on the trackable points.
            foreach (DrawingAngle drawingAngle in metadata.Angles())
            {
                if (!metadata.TrackabilityManager.IsObjectTrackingInitialized(drawingAngle.Id))
                    continue;

                Dictionary<string, DrawingTrack> tracks = metadata.TrackabilityManager.GetTrackingTracks(drawingAngle);
                Dictionary<string, FilteredTrajectory> trajs = new Dictionary<string, FilteredTrajectory>();
                foreach (var pair in tracks)
                {
                    trajs.Add(pair.Key, pair.Value.FilteredTrajectory);
                }

                TimeSeriesCollection tsc = angularKinematics.BuildKinematics(trajs, drawingAngle.AngleOptions, metadata.CalibrationHelper);
                TimeSeriesPlotData data = new TimeSeriesPlotData(drawingAngle.Name, drawingAngle.Color, tsc);
                timeSeriesData.Add(data);
            }
        }

        private static void ImportCustomDrawingsData(Metadata metadata, List<TimeSeriesPlotData> timeSeriesData)
        {
            // Collect angular trajectories for all the angles in all the custom tools.
            
            foreach (DrawingGenericPosture drawing in metadata.GenericPostures())
            {
                if (!metadata.TrackabilityManager.IsObjectTrackingInitialized(drawing.Id))
                    continue;

                // Get all the individual trajectories bound to points of this drawing.
                Dictionary<string, DrawingTrack> tracks = metadata.TrackabilityManager.GetTrackingTracks(drawing);
                Dictionary<string, FilteredTrajectory> trajs = new Dictionary<string, FilteredTrajectory>();
                foreach (var pair in tracks)
                {
                    trajs.Add(pair.Key, pair.Value.FilteredTrajectory);
                }

                // Loop over all angles in this drawing and find the trackable aliases of the points making up the particular angle.
                // The final collection of trajectories for each angle should have indices named o, a, b.
                foreach (GenericPostureAngle gpa in drawing.GenericPostureAngles)
                {
                    // From integer indices to tracking aliases.
                    string keyO = gpa.Origin.ToString();
                    string keyA = gpa.Leg1.ToString();
                    string keyB = gpa.Leg2.ToString();

                    // The angle we are interested in is not necessarily based on three tracked points.
                    // In the case of angle-to-horizontal or angle-to-vertical, one of the points is static.
                    // In the case of Goniometer, one of the points is moved by an alignment constraint.
                    List<string> keys = new List<string>() { keyO, keyA, keyB };
                    List<string> missingKeys = keys.Where(k => !trajs.ContainsKey(k)).ToList();

                    // At the moment we don't allow the O key to be missing as it's used as a reference
                    // for the time coordinates and length.
                    // We also only allow one missing key for now.
                    if (missingKeys.Contains(keyO) || missingKeys.Count > 1)
                    {
                        continue;
                    }

                    if (missingKeys.Count > 0)
                    {
                        FillMissingTrajectory(drawing, metadata, missingKeys, trajs, keyO, tracks);
                    }

                    Dictionary<string, FilteredTrajectory> angleTrajs = new Dictionary<string, FilteredTrajectory>();
                    angleTrajs["o"] = trajs[keyO];
                    angleTrajs["a"] = trajs[keyA];
                    angleTrajs["b"] = trajs[keyB];

                    AngleOptions options = new AngleOptions(gpa.Signed, gpa.CCW, gpa.Supplementary);
                    TimeSeriesCollection tsc = angularKinematics.BuildKinematics(angleTrajs, options, metadata.CalibrationHelper);

                    string name = drawing.Name;
                    if (!string.IsNullOrEmpty(gpa.Name))
                    {
                        name = name + " - " + gpa.Name;
                    }

                    Color color = gpa.Color == Color.Transparent ? drawing.Color : gpa.Color;
                    TimeSeriesPlotData data = new TimeSeriesPlotData(name, color, tsc);

                    timeSeriesData.Add(data);
                }
            }
        }

        private static void FillMissingTrajectory(DrawingGenericPosture drawing, Metadata metadata, List<string> missingKeys, Dictionary<string, FilteredTrajectory> trajs, string keyO, Dictionary<string, DrawingTrack> tracks)
        {
            string missingKey = missingKeys.FirstOrDefault();
            if (missingKey == null)
                return;

            int missingIndex = int.Parse(missingKeys.First());
            
            // Update the drawing according to the tracking data as if we were moving on the timeline.
            // Note that we need to move all the tracked points, or at least all the points
            // that are impacting the alignment constraint of our missing point.
            List<TimedPoint> positions = new List<TimedPoint>();
            foreach (var time in trajs[keyO].Times)
            {
                // Update all tracked points.
                // This will trigger the constraint engine.
                for (int i = 0; i < tracks.Count; i++)
                {
                    var track = tracks.ElementAt(i);
                    TimedPoint tp = track.Value.GetTimedPoint(time);
                    drawing.SetTrackablePointValue(track.Key, tp.Point, tp.T - time);
                }

                // Get the position of the non tracked point.
                PointF p = drawing.GenericPosture.PointList[missingIndex];
                TimedPoint tpMissing = new TimedPoint(p.X, p.Y, time);
                positions.Add(tpMissing);
            }

            // Rebuild the trajectory of the non-tracked point.
            FilteredTrajectory filteredTrajectory = new FilteredTrajectory();
            filteredTrajectory.Initialize(positions, metadata.CalibrationHelper);
            trajs[missingKey] = filteredTrajectory;
        }
    }
}
