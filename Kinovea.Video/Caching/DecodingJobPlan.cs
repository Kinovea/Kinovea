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
        /// Whether the target was acquired during the initial preparation.
        /// That is, the job is already fullfilled, we just have to relocate
        /// the decoder if needed.
        /// This means the player is already showing the frame.
        /// </summary>
        public bool TargetAcquired { get; set; }


        /// <summary>
        /// The target was acquired during the planning phase.
        /// For example we decoded and stored the frame 
        /// while the main thread was preparing the job.
        /// </summary>
        public bool TargetAcquiredInPlanning { get; set; }


        /// <summary>
        /// Whether we should Stay there, Advance or Seek.
        /// </summary>
        public DecoderRelocation DecoderRelocation { get; set; }

        /// <summary>
        /// True if the pending frame should be resubmitted to the decoder.
        /// </summary>
        public bool ResubmitPending { get; set; }

        /// <summary>
        /// Start storing frames as it approaches the target.
        /// </summary>
        public bool PreRoll { get; set; }


        public DecodingJobPlan()
        {
            RequestedTimestamp = -1;
            TargetTimestamp = -1;
            TargetAcquired = false;
            TargetAcquiredInPlanning = false;
            DecoderRelocation = DecoderRelocation.None;
            ResubmitPending = false;
            PreRoll = true;
        }

        public override string ToString()
        {
            return string.Format("DecodingJobPlan: Request: [~{0}]. Target: [{1}]. Acquired: {2}. AcquiredInPlanning: {3}. DecoderInitAction: {4}. Resubmit pending: {5}. PreRoll: {6}.",
                RequestedTimestamp, TargetTimestamp, TargetAcquired, TargetAcquiredInPlanning, DecoderRelocation, ResubmitPending, PreRoll);
        }
    }
}
