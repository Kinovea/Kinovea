using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Drawing;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Import a folder of files created by OpenPose.
    /// https://github.com/CMU-Perceptual-Computing-Lab/openpose
    /// File format: https://github.com/CMU-Perceptual-Computing-Lab/openpose/blob/master/doc/output.md
    ///
    /// This importer converts OpenPose persons into instances of the custom tool "openpose_body25.xml".
    /// Each person on each frame is converted to a drawing (no tracking from frame to frame).
    /// </summary>
    public static class MetadataImporterOpenPose
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static Dictionary<int, string> options = new Dictionary<int, string>();
        private static readonly float confidenceThreshold = 0.25f;

        static MetadataImporterOpenPose()
        {
            // List of option keys, indexed by the joint index.
            // Options are used to hide points and segments when the confidence from OpenPose is low.
            options.Add(0, "showNose");
            options.Add(1, "showNeck");
            options.Add(2, "showRShoulder");
            options.Add(3, "showRElbow");
            options.Add(4, "showRWrist");
            options.Add(5, "showLShoulder");
            options.Add(6, "showLElbow");
            options.Add(7, "showLWrist");
            options.Add(8, "showMidHip");
            options.Add(9, "showRHip");
            options.Add(10, "showRKnee");
            options.Add(11, "showRAnkle");
            options.Add(12, "showLHip");
            options.Add(13, "showLKnee");
            options.Add(14, "showLAnkle");
            options.Add(15, "showREye");
            options.Add(16, "showLEye");
            options.Add(17, "showREar");
            options.Add(18, "showLEar");
            options.Add(19, "showLBigToe");
            options.Add(20, "showLSmallToe");
            options.Add(21, "showLHeel");
            options.Add(22, "showRBigToe");
            options.Add(23, "showRSmallToe");
            options.Add(24, "showRHeel");
        }

        public static void Import(Metadata metadata, string source, bool isFile)
        {
            if (!isFile)
                return;

            // The file is assumed to be part of a folder full of keypoints files for other frames, 
            // but these may be mixed with keypoints files from other videos.
            // filename pattern: prefix_000000000000_keypoints.json, 
            // where prefix is free (name of the original file) and the chunk of digits is the frame number, 
            string prefix = GetFilePrefix(source);

            string[] fileList = Directory.GetFiles(Path.GetDirectoryName(source), prefix + "*", SearchOption.TopDirectoryOnly);
            Array.Sort(fileList, new AlphanumComparator());

            foreach (string f in fileList)
            {
                // send to parser.
                string[] tokens = f.Split(new char[] { '_' });
                int frameNumber;
                bool parsedFrameNumber = int.TryParse(tokens[tokens.Length - 2], out frameNumber);
                if (!parsedFrameNumber)
                    break;

                ParseFile(f, metadata, frameNumber);
            }
        }

        private static void ParseFile(string source, Metadata metadata, int frameNumber)
        { 
            string json = File.ReadAllText(source);
            OpenPoseFrame frame;
            try
            {
                frame = JsonConvert.DeserializeObject<OpenPoseFrame>(json);
            }
            catch(Exception e)
            {
                log.ErrorFormat(e.Message);
                return;
            }

            long timestamp = metadata.FirstTimeStamp + (frameNumber * metadata.AverageTimeStampsPerFrame);
            
            List<AbstractDrawing> drawings = new List<AbstractDrawing>();
            foreach (OpenPosePerson person in frame.people)
            {
                AbstractDrawing drawing = CreateDrawing(metadata, timestamp, person);
                if (drawing != null)
                    drawings.Add(drawing);
            }

            // Create a keyframe and add the drawings to it.
            Guid id = Guid.NewGuid();
            long position = timestamp;
            string title = null;
            Color color = Keyframe.DefaultColor;
            string timecode = null;
            string comments = "";
            Keyframe keyframe = new Keyframe(id, position, title, color, timecode, comments, drawings, metadata);

            metadata.MergeInsertKeyframe(keyframe);
        }

        private static AbstractDrawing CreateDrawing(Metadata metadata, long timestamp, OpenPosePerson person)
        {
            // We only support files created using the BODY_25 model, not COCO or MPI.
            if (person.pose_keypoints_2d != null && person.pose_keypoints_2d.Count != 75)
                return null;

            string toolName = "OpenPoseBody25";
            DrawingToolGenericPosture tool = ToolManager.Tools[toolName] as DrawingToolGenericPosture;
            if (tool == null)
                return null;

            GenericPosture posture = GenericPostureManager.Instanciate(tool.ToolId, true);
            ParsePosture(posture, person);

            DrawingGenericPosture drawing = new DrawingGenericPosture(tool.ToolId, PointF.Empty, posture, timestamp, metadata.AverageTimeStampsPerFrame, ToolManager.GetStylePreset(toolName));
            drawing.Name = "OpenPose";

            // Disable onion skinning.
            drawing.InfosFading.UseDefault = false;
            drawing.InfosFading.ReferenceTimestamp = timestamp;
            drawing.InfosFading.AverageTimeStampsPerFrame = metadata.AverageTimeStampsPerFrame;
            drawing.InfosFading.AlwaysVisible = false;
            drawing.InfosFading.OpaqueFrames = 1;
            drawing.InfosFading.FadingFrames = 0;

            return drawing;
        }

        private static void ParsePosture(GenericPosture posture, OpenPosePerson person)
        {
            // We assume pixel values for x and y. (See flag keypoint_scale)
            // The order of entries in the custom tool is the same.
            for (int i = 0; i < person.pose_keypoints_2d.Count; i += 3)
            {
                float x = person.pose_keypoints_2d[i + 0];
                float y = person.pose_keypoints_2d[i + 1];
                float c = person.pose_keypoints_2d[i + 2];

                int index = i / 3;
                posture.PointList[index] = new PointF(x, y);

                // Visibility of the point and incoming segments depends on confidence.
                if (options.ContainsKey(index) && posture.Options.ContainsKey(options[index]))
                    posture.Options[options[index]].Value = c >= confidenceThreshold;
            }
        }

        private static string GetFilePrefix(string source)
        {
            string filename = Path.GetFileName(source);
            string[] tokens = filename.Split(new char[] { '_' });
            if (tokens.Length < 3)
                return null;

            if (tokens[tokens.Length - 1] != "keypoints.json")
                return null;

            int suffixCharacters = tokens[tokens.Length - 1].Length + tokens[tokens.Length - 2].Length + 2;

            return filename.Substring(0, filename.Length - suffixCharacters);
        }
    }
}
