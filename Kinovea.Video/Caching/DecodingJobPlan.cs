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
        /// The target was decoded during the planning phase.
        /// For example the frame could have been decoded and stored 
        /// in parallel to the main thread preparing the job.
        /// This means the request is fulfilled, the player still needs
        /// to be told about it and acquire it.
        /// </summary>
        public bool RequestFulfilledInPlanning { get; set; }


        /// <summary>
        /// Whether we should Stay there, Advance or Seek.
        /// </summary>
        public DecoderRelocation DecoderRelocation { get; set; }

        /// <summary>
        /// True if the pending frame should be resubmitted to the decoder.
        /// </summary>
        public bool ResubmitPending { get; set; }

        public DecodingJobPlan()
        {
            TargetTimestamp = -1;
            TargetAcquired = false;
            RequestFulfilledInPlanning = false;
            DecoderRelocation = DecoderRelocation.None;
            ResubmitPending = false;
        }

        public override string ToString()
        {
            return string.Format("DecodingJobPlan: Target: [~{0}]. Acquired: {1}. FulfilledInPlanning: {2}. DecoderInitAction: {3}. Resubmit pending: {4}.",
                TargetTimestamp, TargetAcquired, RequestFulfilledInPlanning, DecoderRelocation, ResubmitPending);
        }
    }
}
