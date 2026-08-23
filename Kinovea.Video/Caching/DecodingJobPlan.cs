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
        public long RequestedTimestamp { get; set; }

        /// <summary>
        /// The actual media timestamp matching the requested timestamp.
        /// If TargetIsResolved is false, this is the same as RequestedTimestamp.
        /// </summary>
        public long TargetTimestamp { get; set; }

        /// <summary>
        /// Whether the target was acquired.
        /// </summary>
        public bool TargetIsResolved { get; set; }

        /// <summary>
        /// Whether we should Stay there, Advance or Seek.
        /// </summary>
        public DecoderInitAction DecoderInitAction { get; set; }

        /// <summary>
        /// True if the pending frame should be resubmitted to the decoder.
        /// </summary>
        public bool ResubmitPending { get; set; }


        public DecodingJobPlan()
        {
            RequestedTimestamp = -1;
            TargetTimestamp = -1;
            TargetIsResolved = false;
            DecoderInitAction = DecoderInitAction.None;
            ResubmitPending = false;
        }

        public override string ToString()
        {
            return string.Format("DecodingJobPlan: RequestedTimestamp: [~{0}]. TargetTimestamp: [{1}]. TargetIsResolved:{2}. DecoderInitAction: {3}. Resubmit pending: {4}.",
                RequestedTimestamp, TargetTimestamp, TargetIsResolved, DecoderInitAction, ResubmitPending);
        }
    }
}
