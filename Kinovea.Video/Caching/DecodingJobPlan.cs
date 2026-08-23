using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.Video
{
    /// <summary>
    /// How we reinitialize the decoder for the next job.
    /// </summary>
    public class DecodingJobPlan
    {
        /// <summary>
        /// The reference timestamp from the player request.
        /// This may be approximate and coming from pixel or clock
        /// mapping.
        /// This is informative only, use TargetTimestamp.
        /// </summary>
        public long RequestedTimestamp { get; }

        /// <summary>
        /// The actual media timestamp matching the requested timestamp.
        /// If TargetIsResolved is false, this is the same as RequestedTimestamp.
        /// </summary>
        public long TargetTimestamp { get; }

        /// <summary>
        /// Whether the target was acquired.
        /// </summary>
        public bool TargetIsResolved { get; }

        /// <summary>
        /// Whether we should Stay there, Advance or Seek.
        /// </summary>
        public DecoderInitAction DecoderInitAction{ get; }

        /// <summary>
        /// Whether we should resume decoding after init or not.
        /// </summary>
        public bool ResumeDecoding { get; }


        public DecodingJobPlan(long requestedTimestamp, long targetTimestamp, bool targetIsResolved, DecoderInitAction decoderInitAction, bool resumeDecoding)
        {
            RequestedTimestamp = requestedTimestamp;
            TargetTimestamp = targetTimestamp;
            TargetIsResolved = targetIsResolved;
            DecoderInitAction = decoderInitAction;
            ResumeDecoding = resumeDecoding;
        }

        public override string ToString()
        {
            return string.Format("DecodingJobPlan: RequestedTimestamp: [~{0}]. TargetTimestamp: [{1}]. TargetIsResolved:{2}. DecoderInitAction: {3}, ResumeDecoding: {4}.",
                RequestedTimestamp, TargetTimestamp, TargetIsResolved, DecoderInitAction, ResumeDecoding);
        }
    }
}
