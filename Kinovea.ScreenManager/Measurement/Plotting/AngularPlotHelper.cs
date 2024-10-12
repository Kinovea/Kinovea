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
                Dictionary<string, FilteredTrajectory> trajs = new Dictionary<string, FilteredTrajectory>();
                Dictionary<string, TrackablePoint> trackablePoints = metadata.TrackabilityManager.GetTrackablePoints(drawingAngle);

                bool tracked = true;

                foreach (string key in trackablePoints.Keys)
                {
                    Timeline<TrackingTemplate> timeline = trackablePoints[key].Timeline;
                    if (timeline.Count == 0)
                    {
                        tracked = false;
                        break;
                    }

                    List<TimedPoint> samples = timeline.Enumerate().Select(p => new TimedPoint(p.Location.X, p.Location.Y, p.Time)).ToList();
                    FilteredTrajectory traj = new FilteredTrajectory();
                    traj.Initialize(samples, metadata.CalibrationHelper);

                    trajs.Add(key, traj);
                }

                if (!tracked)
                    continue;

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
                Dictionary<string, TrackablePoint> trackablePoints = metadata.TrackabilityManager.GetTrackablePoints(drawing);

                // First create trajectories for all the trackable points in the drawing.
                // This avoids duplicating the filtering operation for points shared by more than one angle.
                // Here the trajectories are indexed by the original alias in the custom tool, based on the index.
                Dictionary<string, FilteredTrajectory> trajs = new Dictionary<string, FilteredTrajectory>();
                bool tracked = true;

                foreach (string key in trackablePoints.Keys)
                {
                    Timeline<TrackingTemplate> timeline = trackablePoints[key].Timeline;

                    if (timeline.Count == 0)
                    {
                        // The point is trackable but doesn't have any timeline data.
                        // This happens if the user is not tracking that drawing, so we don't need to go further.
                        tracked = false;
                        break;
                    }

                    List<TimedPoint> samples = timeline.Enumerate().Select(p => new TimedPoint(p.Location.X, p.Location.Y, p.Time)).ToList();
                    FilteredTrajectory traj = new FilteredTrajectory();
                    traj.Initialize(samples, metadata.CalibrationHelper);

                    trajs.Add(key, traj);
                }

                if (!tracked)
                    continue;

                // Loop over all angles in this drawing and find the trackable aliases of the points making up the particular angle.
                // The final collection of trajectories for each angle should have indices named o, a, b.
                foreach (GenericPostureAngle gpa in drawing.GenericPostureAngles)
                {
                    // From integer indices to tracking aliases.
                    string keyO = gpa.Origin.ToString();
                    string keyA = gpa.Leg1.ToString();
                    string keyB = gpa.Leg2.ToString();

                    // All points in an angle must be trackable as there is currently no way to get the static point coordinate.
                    if (!trajs.ContainsKey(keyO) || !trajs.ContainsKey(keyA) || !trajs.ContainsKey(keyB))
                        continue;

                    // Remap to oab.
                    Dictionary<string, FilteredTrajectory> angleTrajs = new Dictionary<string, FilteredTrajectory>();
                    angleTrajs.Add("o", trajs[keyO]);
                    angleTrajs.Add("a", trajs[keyA]);
                    angleTrajs.Add("b", trajs[keyB]);

                    AngleOptions options = new AngleOptions(gpa.Signed, gpa.CCW, gpa.Supplementary);
                    TimeSeriesCollection tsc = angularKinematics.BuildKinematics(angleTrajs, options, metadata.CalibrationHelper);

                    string name = drawing.Name;
                    if (!string.IsNullOrEmpty(gpa.Name))
                        name = name + " - " + gpa.Name;

                    Color color = gpa.Color == Color.Transparent ? drawing.Color : gpa.Color;
                    TimeSeriesPlotData data = new TimeSeriesPlotData(name, color, tsc);

                    timeSeriesData.Add(data);
                }
            }
        }
    }
}
