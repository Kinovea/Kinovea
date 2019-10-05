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
            get { return outputHeight; }
        }

        public float ResultingFramerate
        {
            get { return resultingFramerate; }
        }

        public byte[] BufferOutput
        {
            get { return bufferOutput; }
        }
        #endregion

        private bool enabled = false;
        private float resultingFramerate;
        private ImageDescriptor imageDescriptor;
        private int thresholdHeight;              // If the user configures the camera with an height below this, we automatically switch to finishline mode.
        private int consolidationHeight;          // Number of rows we grab from the incoming frames, independently of the user configured height. (Some camera have a min height).
        private int outputHeight;                 // Size of the frames we will output. aka, how many consolidated rows make a frame.
        private int waterfallFlushHeight;         // Size of waterfall sections before we output that section into the frame and flush.
        private bool waterfallEnabled;            // If false, each frame will have unique rows. If true we overlap the rows and flush more often.
        private byte[] bufferCurrent;             // Current frame being consolidated.
        private byte[] bufferOld;                 // Last completely consolidated frame. Used during waterfall for overlap.
        private byte[] bufferOutput;              // The finished frame. May contain rows from both current and previous.
        private int row;                          // The rows we have consolidated so far in the current frame.
        private int section;                      // The section of rows we are in, for waterfall overlap.

        /// <summary>
        /// Compute the new image size/framerate and prepare the buffers.
        /// </summary>
        public void Prepare(int width, int height, ImageFormat format, float inputFramerate)
        {
            thresholdHeight = PreferencesManager.CapturePreferences.PhotofinishConfiguration.ThresholdHeight;
            enabled = height <= thresholdHeight;

            if (!enabled)
                return;

            // Constraints:
            // -The number of consolidated rows has to be lower than the height threshold, otherwise we won't have enough source material to copy.
            // -The output height has to be a multiple of the number of consolidated rows, otherwise there will be a hole at the bottom of the output. 
            consolidationHeight = PreferencesManager.CapturePreferences.PhotofinishConfiguration.ConsolidationHeight;
            consolidationHeight = Math.Min(consolidationHeight, thresholdHeight);

            outputHeight = PreferencesManager.CapturePreferences.PhotofinishConfiguration.OutputHeight;
            outputHeight = outputHeight - (outputHeight % consolidationHeight);

            waterfallEnabled = PreferencesManager.CapturePreferences.PhotofinishConfiguration.Waterfall;
            if (waterfallEnabled)
            {
                waterfallFlushHeight = PreferencesManager.CapturePreferences.PhotofinishConfiguration.WaterfallFlushHeight;

                bool isWaterfallFlushHeightValid =
                    waterfallFlushHeight >= consolidationHeight &&
                    waterfallFlushHeight <= outputHeight &&
                    (outputHeight % waterfallFlushHeight == 0) &&
                    (waterfallFlushHeight % consolidationHeight == 0);

                if (!isWaterfallFlushHeightValid)
                    waterfallFlushHeight = outputHeight;
            }

            // Prepare buffer for output images.
            float rowsPerSecond = inputFramerate * consolidationHeight;
            if (waterfallEnabled)
                this.resultingFramerate = rowsPerSecond / waterfallFlushHeight;
            else
                this.resultingFramerate = rowsPerSecond / outputHeight;

            int pfBufferSize = ImageFormatHelper.ComputeBufferSize(width, outputHeight, format);
            imageDescriptor = new ImageDescriptor(format, width, outputHeight, true, pfBufferSize);

            bufferCurrent = new byte[pfBufferSize];
            bufferOld = new byte[pfBufferSize];
            bufferOutput = new byte[pfBufferSize];
            row = 0;
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
            int bytesPerPixel = ImageFormatHelper.BytesPerPixel(imageDescriptor.Format);
            int stride = imageDescriptor.Width * bytesPerPixel;

            // Consolidate the current sub-frame.
            Buffer.BlockCopy(buffer, 0, bufferCurrent, row * imageDescriptor.Width * bytesPerPixel, imageDescriptor.Width * bytesPerPixel * consolidationHeight);
            row += consolidationHeight;

            if (waterfallEnabled && (row % waterfallFlushHeight == 0))
            {
                // Flush the current and old image to output in sliding window mode.
                // "section" is at 0 after we grabbed the first section of rows.
                int totalSections = outputHeight / waterfallFlushHeight;

                // Rows are always added to the bottom, so here we continue this pattern.
                // The current image is copied at the bottom, and the old image is copied at the top.
                // We just need to figure out which portion of each image to copy.
                int bufferCurrentSource = 0;
                int bufferCurrentLength = (section + 1) * waterfallFlushHeight * stride;
                int bufferCurrentDestination = (totalSections - (section + 1)) * waterfallFlushHeight * stride;
                int bufferOldSource = bufferCurrentLength;
                int bufferOldLength = bufferCurrentDestination;
                int bufferOldDestination = 0;

                Buffer.BlockCopy(bufferCurrent, bufferCurrentSource, bufferOutput, bufferCurrentDestination, bufferCurrentLength);
                Buffer.BlockCopy(bufferOld, bufferOldSource, bufferOutput, bufferOldDestination, bufferOldLength);

                section = (section + 1) % totalSections;

                flush = true;
            }

            if (row >= outputHeight)
            {
                row = 0;
                section = 0;

                if (!waterfallEnabled)
                    flush = true;

                // Swap buffers.
                byte[] pfBufferTemp = bufferOld;
                bufferOld = bufferCurrent;
                bufferCurrent = bufferOld;
            }

            return flush;
        }
    }
}
