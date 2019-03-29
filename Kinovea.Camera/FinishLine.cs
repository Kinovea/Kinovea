using Kinovea.Pipeline;
using Kinovea.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinovea.Video;

namespace Kinovea.Camera
{
    /// <summary>
    /// Helper class to manage finishline mode.
    /// This class host the buffer and builds the current frame.
    /// </summary>
    public class Finishline
    {
        #region Properties
        public bool Enabled
        {
            get { return enabled; }
        }

        public int Height
        {
            get { return pfOutputHeight; }
        }

        public float ResultingFramerate
        {
            get { return resultingFramerate; }
        }

        public byte[] BufferOutput
        {
            get { return pfBufferOutput; }
        }
        #endregion

        private bool enabled = false;
        private float resultingFramerate;
        private ImageDescriptor pfImageDescriptor;
        private int pfThresholdHeight;              // If the user configures the camera with an height below this, we automatically switch to finishline mode.
        private int pfConsolidationHeight;          // Number of rows we grab from the incoming frames, independently of the user configured height. (Some camera have a min height).
        private int pfOutputHeight;                 // Size of the frames we will output. aka, how many consolidated rows make a frame.
        private int pfWaterfallFlushHeight;         // Size of waterfall sections before we output that section into the frame and flush.
        private bool pfWaterfallEnabled;            // If false, each frame will have unique rows. If true we overlap the rows and flush more often.
        private byte[] pfBufferCurrent;             // Current frame being consolidated.
        private byte[] pfBufferOld;                 // Last completely consolidated frame. Used during waterfall for overlap.
        private byte[] pfBufferOutput;              // The finished frame. May contain rows from both current and previous.
        private int pfRow;                          // The rows we have consolidated so far in the current frame.
        private int pfSection;                      // The section we are in, for waterfall overlap.

        /// <summary>
        /// Compute the new image size/framerate and prepare the buffers.
        /// </summary>
        public void Prepare(int width, int height, ImageFormat format, float inputFramerate)
        {
            pfThresholdHeight = PreferencesManager.CapturePreferences.PhotofinishConfiguration.ThresholdHeight;
            enabled = height <= pfThresholdHeight;

            if (!enabled)
                return;

            // Constraints:
            // -The number of consolidated rows has to be lower than the height threshold, otherwise we won't have enough source material to copy.
            // -The output height has to be a multiple of the number of consolidated rows, otherwise there will be a hole at the bottom of the output. 
            pfConsolidationHeight = PreferencesManager.CapturePreferences.PhotofinishConfiguration.ConsolidationHeight;
            pfConsolidationHeight = Math.Min(pfConsolidationHeight, pfThresholdHeight);

            pfOutputHeight = PreferencesManager.CapturePreferences.PhotofinishConfiguration.OutputHeight;
            pfOutputHeight = pfOutputHeight - (pfOutputHeight % pfConsolidationHeight);

            pfWaterfallEnabled = PreferencesManager.CapturePreferences.PhotofinishConfiguration.Waterfall;
            if (pfWaterfallEnabled)
            {
                pfWaterfallFlushHeight = PreferencesManager.CapturePreferences.PhotofinishConfiguration.WaterfallFlushHeight;

                bool isWaterfallFlushHeightValid =
                    pfWaterfallFlushHeight >= pfConsolidationHeight &&
                    pfWaterfallFlushHeight <= pfOutputHeight &&
                    (pfOutputHeight % pfWaterfallFlushHeight == 0) &&
                    (pfWaterfallFlushHeight % pfConsolidationHeight == 0);

                if (!isWaterfallFlushHeightValid)
                    pfWaterfallFlushHeight = pfOutputHeight;
            }

            // Prepare buffer for output images.
            float rowsPerSecond = inputFramerate * pfConsolidationHeight;
            if (pfWaterfallEnabled)
                this.resultingFramerate = rowsPerSecond / pfWaterfallFlushHeight;
            else
                this.resultingFramerate = rowsPerSecond / pfOutputHeight;

            int pfBufferSize = ImageFormatHelper.ComputeBufferSize(width, pfOutputHeight, format);
            pfImageDescriptor = new ImageDescriptor(format, width, pfOutputHeight, true, pfBufferSize);

            pfBufferCurrent = new byte[pfBufferSize];
            pfBufferOld = new byte[pfBufferSize];
            pfBufferOutput = new byte[pfBufferSize];
            pfRow = 0;
        }

        /// <summary>
        /// Consolidate the incoming rows.
        /// Returns true if the buffer has to be flushed, in which case the properties BufferOutput will be valid
        /// and can be used by the caller to raise the FrameProducedEvent.
        /// </summary>
        public bool Consolidate(byte[] buffer)
        {
            if (!enabled)
                throw new InvalidOperationException();

            bool flush = false;
            int bytesPerPixel = pfImageDescriptor.Format == ImageFormat.Y800 ? 1 : 4;
            int stride = pfImageDescriptor.Width * bytesPerPixel;

            // Consolidate the current sub-frame.
            Buffer.BlockCopy(buffer, 0, pfBufferCurrent, pfRow * pfImageDescriptor.Width * bytesPerPixel, pfImageDescriptor.Width * bytesPerPixel * pfConsolidationHeight);
            pfRow += pfConsolidationHeight;

            if (pfWaterfallEnabled && (pfRow % pfWaterfallFlushHeight == 0))
            {
                // Flush the current and old image to output in sliding window mode.
                // "section" is at 0 after we grabbed the first section of rows.
                int totalSections = pfOutputHeight / pfWaterfallFlushHeight;

                // Rows are always added to the bottom, so here we continue this pattern.
                // The current image is copied at the bottom, and the old image is copied at the top.
                // We just need to figure out which portion of each image to copy.
                int bufferCurrentSource = 0;
                int bufferCurrentLength = (pfSection + 1) * pfWaterfallFlushHeight * stride;
                int bufferCurrentDestination = (totalSections - (pfSection + 1)) * pfWaterfallFlushHeight * stride;
                int bufferOldSource = bufferCurrentLength;
                int bufferOldLength = bufferCurrentDestination;
                int bufferOldDestination = 0;

                Buffer.BlockCopy(pfBufferCurrent, bufferCurrentSource, pfBufferOutput, bufferCurrentDestination, bufferCurrentLength);
                Buffer.BlockCopy(pfBufferOld, bufferOldSource, pfBufferOutput, bufferOldDestination, bufferOldLength);

                pfSection = (pfSection + 1) % totalSections;

                flush = true;
            }

            if (pfRow >= pfOutputHeight)
            {
                pfRow = 0;
                pfSection = 0;

                if (!pfWaterfallEnabled)
                    flush = true;

                // Swap buffers.
                byte[] pfBufferTemp = pfBufferOld;
                pfBufferOld = pfBufferCurrent;
                pfBufferCurrent = pfBufferOld;
            }

            return flush;
        }
    }
}
