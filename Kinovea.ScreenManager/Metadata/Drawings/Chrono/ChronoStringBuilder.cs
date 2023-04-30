using Kinovea.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.ScreenManager
{
    public class ChronoStringBuilder
    {
        private List<ChronoSection> sections;
        private Metadata parentMetadata;
        private StringBuilder sb = new StringBuilder();
        private Dictionary<ChronoColumns, int> length = new Dictionary<ChronoColumns, int>();
        private Dictionary<ChronoColumns, string> padding = new Dictionary<ChronoColumns, string>();

        public ChronoStringBuilder(List<ChronoSection> sections, Metadata parentMetadata)
        {
            this.sections = sections;
            this.parentMetadata = parentMetadata;
        }

        /// <summary>
        /// Build a single piece of text with all the section columns up to the passed timestamp.
        /// Sections that are not yet started are not returned (exception when before first).
        /// </summary>
        public string Build(long timestamp, HashSet<ChronoColumns> visible)
        {
            int sectionIndex = DrawingChronoMulti.GetSectionIndex(sections, timestamp);

            if (sectionIndex == -1)
                return parentMetadata.TimeCodeBuilder(0, TimeType.Absolute, TimecodeFormat.Unknown, true);

            UpdateTimecodes(timestamp);
            
            // If we are on the first section we only show the time.
            bool onFirst = sections.Count == 1 || (sections.Count > 1 && timestamp < sections[1].Section.Start);
            if (onFirst)
                return sections[0].Duration;

            UpdateColumnLengths();
            bool hasTags = length[ChronoColumns.Tag] > 0;

            // Compute padding for column alignment.
            padding.Clear();
            padding.Add(ChronoColumns.Name, "{0," + length[ChronoColumns.Name] + "}");
            padding.Add(ChronoColumns.Duration, "{0," + length[ChronoColumns.Duration] + "}");
            padding.Add(ChronoColumns.Cumul, "{0," + length[ChronoColumns.Cumul] + "}");
            padding.Add(ChronoColumns.Tag, "{0," + length[ChronoColumns.Tag] + "}");

            // TODO: Sanity check, if everything is invisible force show the duration.
            // Otherwise it becomes impossible to interact with the drawing.

            sb.Clear();
            List<string> cells = new List<string>();
            for (int i = 0; i < sections.Count; i++)
            {
                if (timestamp < sections[i].Section.Start)
                    break;

                sb.Append(sections[i].IsCurrent ? "▶ " : "  ");

                string name = string.Format(padding[ChronoColumns.Name], sections[i].NameOrIndex);
                string duration = string.Format(padding[ChronoColumns.Duration], sections[i].Duration);
                string cumul = string.Format(padding[ChronoColumns.Cumul], sections[i].Cumul);
                string tag = string.Format(padding[ChronoColumns.Tag], sections[i].Tag);

                if (visible.Contains(ChronoColumns.Name))
                    sb.Append(string.Format("{0}: ", name));
                
                cells.Clear();
                if (visible.Contains(ChronoColumns.Duration))
                    cells.Add(duration);

                if (visible.Contains(ChronoColumns.Cumul))
                    cells.Add(cumul);

                if (visible.Contains(ChronoColumns.Tag) && hasTags)
                    cells.Add(tag);
                
                string joined = string.Join("|", cells);

                sb.Append(joined);
                sb.Append(Environment.NewLine);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Update the textual part of the section objects, based on the current timestamp.
        /// - Duration is the time since the start of the section or the complete section time if we are after it.
        /// - Cumul is the total active time since the start of the first section.
        /// 
        /// The produced text is always contextualized to the current time.
        /// No special treatment for open-ended sections.
        /// </summary>
        private void UpdateTimecodes(long timestamp)
        {
            // Update the textual part of the ChronoSection entries.
            long cumulTimestamps = 0;
            for (int i = 0; i < sections.Count; i++)
            {
                if (timestamp < sections[i].Section.Start)
                {
                    sections[i].IsCurrent = false;
                    sections[i].NameOrIndex = "";
                    sections[i].Duration = "";
                    sections[i].Cumul = "";
                    continue;
                }

                sections[i].IsCurrent = sections[i].Section.Contains(timestamp);
                sections[i].NameOrIndex = string.IsNullOrEmpty(sections[i].Name) ? (i + 1).ToString() : sections[i].Name;
                
                long elapsedTimestamps = 0;
                if (timestamp <= sections[i].Section.End)
                    elapsedTimestamps = timestamp - sections[i].Section.Start;
                else
                    elapsedTimestamps = sections[i].Section.End - sections[i].Section.Start;

                cumulTimestamps += elapsedTimestamps;

                sections[i].Duration = parentMetadata.TimeCodeBuilder(elapsedTimestamps, TimeType.Absolute, TimecodeFormat.Unknown, true);
                sections[i].Cumul = parentMetadata.TimeCodeBuilder(cumulTimestamps, TimeType.Absolute, TimecodeFormat.Unknown, true);
            }
        }

        /// <summary>
        /// Find the longest cell of each column, by number of characters.
        /// This is used to align the column text.
        /// </summary>
        private void UpdateColumnLengths()
        {
            length.Clear();

            // Find longest cell of each row.
            length[ChronoColumns.Name] = 0;
            length[ChronoColumns.Duration] = 0;
            length[ChronoColumns.Cumul] = 0;
            length[ChronoColumns.Tag] = 0;

            foreach (var section in sections)
            {
                length[ChronoColumns.Name] = Math.Max(length[ChronoColumns.Name], section.NameOrIndex.Length);
                length[ChronoColumns.Duration] = Math.Max(length[ChronoColumns.Duration], section.Duration.Length);
                length[ChronoColumns.Cumul] = Math.Max(length[ChronoColumns.Cumul], section.Cumul.Length);
                length[ChronoColumns.Tag] = Math.Max(length[ChronoColumns.Tag], section.Tag.Length);
            }
        }
    }
}
