using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Imports an SRT file as a series of labels.
    /// Formatting options are ignored.
    /// Parts of the code are derived from: https://github.com/AlexPoint/SubtitlesParser
    /// (License: MIT).
    /// </summary>
    public static class MetadataImporterSRT
    {
        private static readonly string[] delimiters = { "-->", "- >", "->" };

        public static void Import(Metadata metadata, string source, bool isFile)
        {
            string text = isFile ? File.ReadAllText(source, Encoding.UTF8) : source;
            string[] allLines = text.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.None);
            
            // Process each subtitle block on the fly.
            foreach (var srtSubPart in GetSrtSubTitleParts(allLines))
            {
                var lines = srtSubPart.Split(new string[] { Environment.NewLine }, StringSplitOptions.None)
                                .Select(s => s.Trim())
                                .Where(l => !string.IsNullOrEmpty(l))
                                .ToList();

                int startTime = -1;
                int endTime = -1;
                StringBuilder subtitle = new StringBuilder();
                foreach (var line in lines)
                {
                    if (startTime == -1 && endTime == -1)
                    {
                        // Look for the timecodes first
                        int startTc;
                        int endTc;
                        var success = TryParseTimecodeLine(line, out startTc, out endTc);
                        if (success)
                        {
                            startTime = startTc;
                            endTime = endTc;
                        }
                    }
                    else
                    {
                        // Strip formatting tags from the text.
                        string strippedLine = Regex.Replace(line, @"<[^>]*>", String.Empty);
                        subtitle.AppendLine(strippedLine);
                    }
                }

                if ((startTime != -1 || endTime != -1) && subtitle.Length > 0)
                    AddSubtitle(metadata, startTime, endTime, subtitle.ToString());
            }
        }

        /// <summary>
        /// Enumerates the subtitle parts in a srt file based on the standard line break observed between them. 
        /// An srt subtitle part is in the form:
        /// 
        /// 1
        /// 00:00:20,000 --> 00:00:24,400
        /// Altocumulus clouds occur between six thousand
        /// 
        /// </summary>
        private static IEnumerable<string> GetSrtSubTitleParts(string[] lines)
        {
            var sb = new StringBuilder();

            foreach (var line in lines)
            {
                if (string.IsNullOrEmpty(line.Trim()))
                {
                    // return only if not empty
                    var res = sb.ToString().TrimEnd();
                    if (!string.IsNullOrEmpty(res))
                    {
                        yield return res;
                    }
                    sb = new StringBuilder();
                }
                else
                {
                    sb.AppendLine(line);
                }
            }

            if (sb.Length > 0)
            {
                yield return sb.ToString();
            }
        }

        private static bool TryParseTimecodeLine(string line, out int startTc, out int endTc)
        {
            var parts = line.Split(delimiters, StringSplitOptions.None);
            if (parts.Length != 2)
            {
                // this is not a timecode line
                startTc = -1;
                endTc = -1;
                return false;
            }
            else
            {
                startTc = ParseSrtTimecode(parts[0]);
                endTc = ParseSrtTimecode(parts[1]);
                return true;
            }
        }

        /// <summary>
        /// Takes an SRT timecode as a string and parses it into an int (in milliseconds). A SRT timecode reads as follows: 
        /// 00:00:20,000
        /// </summary>
        /// <param name="s">The timecode to parse</param>
        /// <returns>The parsed timecode as a TimeSpan instance. If the parsing was unsuccessful, -1 is returned (subtitles should never show)</returns>
        private static int ParseSrtTimecode(string s)
        {
            var match = Regex.Match(s, "[0-9]+:[0-9]+:[0-9]+([,\\.][0-9]+)?");
            if (match.Success)
            {
                s = match.Value;
                TimeSpan result;
                if (TimeSpan.TryParse(s.Replace(',', '.'), out result))
                {
                    var nbOfMs = (int)result.TotalMilliseconds;
                    return nbOfMs;
                }
            }
            return -1;
        }

        /// <summary>
        /// Create a keyframe and a label to host the subtitle.
        /// </summary>
        private static void AddSubtitle(Metadata metadata, int start, int end, string content)
        {
            long startTs = (long)(start * metadata.AverageTimeStampsPerSecond / 1000.0f);
            long endTs = (long)(end * metadata.AverageTimeStampsPerSecond / 1000.0f);

            // Make sure the keyframe timestamps corresponds to an actual video frame.
            long startFrame = (long)Math.Round((float)startTs / metadata.AverageTimeStampsPerFrame);
            startTs = startFrame * metadata.AverageTimeStampsPerFrame;
            long endFrame = (long)Math.Round((float)endTs / metadata.AverageTimeStampsPerFrame);
            endTs = endFrame * metadata.AverageTimeStampsPerFrame;

            Guid id = Guid.NewGuid();
            long position = startTs;
            string title = null;
            string timecode = start.ToString();
            string comments = "";
            List<AbstractDrawing> drawings = new List<AbstractDrawing>();
            
            // Create a label object.
            float top = metadata.ImageSize.Height * 0.75f;
            PointF location = new PointF(200, top);

            DrawingText drawing = new DrawingText(location, startTs, metadata.AverageTimeStampsPerFrame, content);
            drawing.Name = "Subtitle";

            drawing.InfosFading.UseDefault = false;
            drawing.InfosFading.ReferenceTimestamp = position;
            drawing.InfosFading.AverageTimeStampsPerFrame = metadata.AverageTimeStampsPerFrame;
            drawing.InfosFading.AlwaysVisible = false;
            drawing.InfosFading.OpaqueFrames = (int)((endTs - startTs) / (float)metadata.AverageTimeStampsPerFrame);
            drawing.InfosFading.FadingFrames = 10;

            // FIXME: there seems to be an issue with style element .Value modification.
            //StyleElementFontSize styleElement = label.DrawingStyle.Elements["font size"] as StyleElementFontSize;
            //if (styleElement != null)
            //{
            //    styleElement.Value = 24;
            //    label.DrawingStyle.RaiseValueChanged();
            //}

            drawings.Add(drawing);
            Keyframe keyframe = new Keyframe(id, position, title, timecode, comments, drawings, metadata);

            metadata.MergeInsertKeyframe(keyframe);
        }
    }
}
