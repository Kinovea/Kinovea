using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Kinovea.Video;
using Kinovea.Pipeline;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Threading;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Circular buffer storing delayed frames.
    /// This buffer uses the infinite array abstraction.
    /// </summary>
    public class Delayer
    {
        #region Properties
        public int SafeCapacity
        {
            get { return Math.Max(fullCapacity - reserveCapacity, 0); }
        }
        public int FullCapacity
        {
            get { return fullCapacity; }
        }
        public int CurrentPosition
        {
            get { return currentPosition; }
        }
        #endregion

        #region Members
        private List<Frame> frames = new List<Frame>();
        private Rectangle rect;
        private int minCapacity = 12;
        private int reserveCapacity = 8;    // Number of frames kept unreachable to clients.
        private int fullCapacity = 12;      // Total number of frames kept.
        private int currentPosition = -1;   // Freshest absolute position written to and available to consumers.
        private int triggerPosition = -1;
        private bool allocated;
        private long availableMemory;
        private ImageDescriptor imageDescriptor;
        int pitch;
        byte[] tempJpeg;
        private Stopwatch stopwatch = new Stopwatch();
        private object lockerFrame = new object();
        private object lockerPosition = new object();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Public methods
        /// <summary>
        /// Attempt to preallocate the circular buffer for as many images as possible that fits in available memory.
        /// </summary>
        public bool AllocateBuffers(ImageDescriptor imageDescriptor, long availableMemory)
        {
            if (!NeedsReallocation(imageDescriptor, availableMemory))
                return true;

            int targetCapacity = (int)(availableMemory / imageDescriptor.BufferSize);

            bool memoryPressure = minCapacity * imageDescriptor.BufferSize > availableMemory;
            if (memoryPressure)
            {
                // The user explicitly asked to not use enough memory. We try to honor the request by lowering the min levels.
                // This may result in thread cross talks.
                reserveCapacity = Math.Max(targetCapacity, 2);
                minCapacity = Math.Max(targetCapacity + 1, 3);
                targetCapacity = Math.Max(targetCapacity, minCapacity);
            }
            else
            {
                reserveCapacity = 8;
                minCapacity = 12;
                targetCapacity = Math.Max(targetCapacity, minCapacity);
            }
            
            bool compatible = ImageDescriptor.Compatible(this.imageDescriptor, imageDescriptor);
            if (compatible && targetCapacity <= fullCapacity)
            {
                FreeSome(targetCapacity);
                this.fullCapacity = frames.Count;
                this.availableMemory = availableMemory;
                return true;
            }
            
            if (!compatible)
                FreeAll();
            
            stopwatch.Restart();
            frames.Capacity = targetCapacity;
            log.DebugFormat("Allocating {0} frames.", targetCapacity - fullCapacity);

            int bufferSize = ImageFormatHelper.ComputeBufferSize(imageDescriptor.Width, imageDescriptor.Height, imageDescriptor.Format);

            try
            {
                for (int i = fullCapacity; i < targetCapacity; i++)
                {
                    Frame slot = new Frame(bufferSize);
                    frames.Add(slot);
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("Error while allocating delay buffer.");
                log.Error(e);
            }

            if (frames.Count > 0)
            {
                // The following variables are used during frame -> bitmap conversion.
                this.rect = new Rectangle(0, 0, imageDescriptor.Width, imageDescriptor.Height);
                this.pitch = imageDescriptor.Width * 3;
                this.tempJpeg = new byte[bufferSize];

                this.allocated = true;
                this.fullCapacity = frames.Count;
                this.availableMemory = availableMemory;
                this.imageDescriptor = imageDescriptor;

                // Better do the GC now to push everything to gen2 and LOH rather than taking a hit later during normal streaming operations.
                GC.Collect(2);
            }

            log.DebugFormat("Allocated delay buffer: {0} ms. Total: {1} frames.", stopwatch.ElapsedMilliseconds, fullCapacity);
            return allocated;
        }

        /// <summary>
        /// Returns true if the delayer needs to allocate or reallocate memory.
        /// </summary>
        public bool NeedsReallocation(ImageDescriptor imageDescriptor, long availableMemory)
        {
            return !allocated || !ImageDescriptor.Compatible(this.imageDescriptor, imageDescriptor) || this.availableMemory != availableMemory;
        }

        /// <summary>
        /// Push a single frame to the buffer.
        /// Copies the content into a pre-allocated slot.
        /// </summary>
        public bool Push(Frame src)
        {
            //-----------------------------------------
            // Runs in UI thread in mode Camera.
            // Runs in consumer thread in mode Delayed.
            //-----------------------------------------
            if (!allocated)
                return false;

            int nextPosition = currentPosition + 1;
            int index = nextPosition % fullCapacity;
            bool pushed = false;

            try
            {
                frames[index].Import(src);
                pushed = true;
            }
            catch
            {
                log.ErrorFormat("Failed to push frame to delay buffer.");
            }

            // Lock on write just to avoid a torn read in Get().
            lock (lockerPosition)
                currentPosition = nextPosition;

            return pushed;
        }

        /// <summary>
        /// Get the frame from `age` frames ago, wait for it if necessary, copy it into the passed buffer.
        /// </summary>
        public bool GetStrong(int age, Frame dst)
        {
            //-----------------------------------------------
            // Runs in the consumer thread, during recording.
            //-----------------------------------------------
            Frame frame = Get(age, out _);
            if (frame == null)
                return false;

            // The UI thread and the recording thread can ask the same image at the same time.
            // Here we have a strong need to get the image out, so in the event the UI has 
            // taken the lock on the image, we wait for it.
            lock (lockerFrame)
            {
                dst.Import(frame);
            }

            return true;
        }

        /// <summary>
        /// Get the frame from `age` frames ago as an RGB24 Bitmap, correctly oriented. Do not wait for it and returns null if it's not available. 
        /// The out target parameter provides the actual frame position we got, or a negative number if we are not ready yet. This can be used
        /// to implement a waiting image.
        /// </summary>
        public Bitmap GetWeak(int age, ImageRotation rotation, bool mirror, out int target)
        {
            //----------------------------------------------------
            // Runs in the UI thread, to get the image to display.
            //----------------------------------------------------

            Bitmap copy = null;
            target = 0;

            // The UI thread and the recording thread can ask the same image at the same time.
            // We yield priority to the recording, so if the lock is taken, we return immediately.
            if (Monitor.TryEnter(lockerFrame, 0))
            {
                try
                {
                    Frame frame = Get(age, out target);
                    if (frame == null)
                        return null;

                    // Returns a newly allocated RGB24 bitmap.
                    // TODO: maybe get a pre-allocated bitmap from caller.
                    copy = new Bitmap(imageDescriptor.Width, imageDescriptor.Height, PixelFormat.Format24bppRgb);

                    switch (imageDescriptor.Format)
                    {
                        case Kinovea.Services.ImageFormat.RGB24:
                            BitmapHelper.FillFromRGB24(copy, rect, imageDescriptor.TopDown, frame.Buffer);
                            break;
                        case Kinovea.Services.ImageFormat.RGB32:
                            BitmapHelper.FillFromRGB32(copy, rect, imageDescriptor.TopDown, frame.Buffer);
                            break;
                        case Kinovea.Services.ImageFormat.Y800:
                            BitmapHelper.FillFromY800(copy, rect, imageDescriptor.TopDown, frame.Buffer);
                            break;
                        case Kinovea.Services.ImageFormat.JPEG:
                            BitmapHelper.FillFromJPEG(copy, rect, tempJpeg, frame.Buffer, frame.PayloadLength, pitch);
                            break;
                    }

                    switch (rotation)
                    {
                        case ImageRotation.Rotate90:
                            if (mirror)
                                copy.RotateFlip(RotateFlipType.Rotate90FlipX);
                            else
                                copy.RotateFlip(RotateFlipType.Rotate90FlipNone);
                            break;
                        case ImageRotation.Rotate180:
                            if (mirror)
                                copy.RotateFlip(RotateFlipType.Rotate180FlipX);
                            else
                                copy.RotateFlip(RotateFlipType.Rotate180FlipNone);
                            break;
                        case ImageRotation.Rotate270:
                            if (mirror)
                                copy.RotateFlip(RotateFlipType.Rotate270FlipX);
                            else
                                copy.RotateFlip(RotateFlipType.Rotate270FlipNone);
                            break;
                        default:
                            if (mirror)
                                copy.RotateFlip(RotateFlipType.RotateNoneFlipX);
                            break;
                    }
                }
                catch
                {
                    log.Error("Error while copying frame into bitmap for display.");
                }
                finally
                {
                    Monitor.Exit(lockerFrame);
                }
            }

            return copy;
        }

        /// <summary>
        /// Retrieve a frame from "age" frames ago. Returns the original image or null.
        /// </summary>
        private Frame Get(int age, out int target)
        {
            //----------------------------------------------------------
            // Runs in UI thread in mode Camera for display (through compositor).
            // Runs in UI thread in mode Delayed for display (through compositor).
            // Runs in consumer thread in mode Delayed for recording.
            //----------------------------------------------------------
            target = 0;
            if (!allocated || frames.Count == 0)
                return null;

            int newestAvailablePosition = 0;

            // We only lock on reading to avoid a torn read if the other thread is writing to this variable.
            // The mechanism to avoid actually reading the frame we want while the other thread is writing to it 
            // is the reserve capacity.
            lock (lockerPosition)
                newestAvailablePosition = currentPosition;

            if (newestAvailablePosition < 0)
                return null;

            target = newestAvailablePosition - age;
            if (target <= 0)
            {
                // This happens if delay is set and we haven't captured these images yet.
                return null;
            }

            // The producer may currently be writing the slot after the newest available position, which may wrap around the ring buffer.
            // we use the reserve capacity to give the writer some room.
            // Both are only doing copies so there should be very little chance that the writer had time to 
            // overwrite more than reserve capacity while the reader is still making one copy.
            int requestedPosition = newestAvailablePosition - age;
            int oldestAvailablePosition = newestAvailablePosition - (fullCapacity - 1) + reserveCapacity;
            int finalPosition = Math.Max(requestedPosition, oldestAvailablePosition);

            // We return the actual image, not a copy. The caller is responsible for doing its own copy as fast as possible.
            // If not fast enough, the writer could catch up the reserve capacity and start writing this slot.
            return frames[finalPosition % fullCapacity];
        }

        /// <summary>
        /// Mark the frame at `age` ago as the trigger.
        /// </summary>
        public void MarkTrigger(int age)
        {
            triggerPosition = currentPosition - age;
        }

        /// <summary>
        /// Returns the age of the trigger frame.
        /// The frame is not guaranteed to still be in the buffer.
        /// </summary>
        public int GetTriggerAge()
        {
            return currentPosition - triggerPosition;
        }

        public void LogPosition(int age)
        {
            if (!allocated || frames.Count == 0)
                return;

            int newestAvailablePosition = 0;

            // We only lock on reading to avoid a torn read if the other thread is writing to this variable.
            // The mechanism to avoid actually reading the frame we want while the other thread is writing to it 
            // is the reserve capacity.
            lock (lockerPosition)
                newestAvailablePosition = currentPosition;

            if (newestAvailablePosition < 0)
                return;

            int target = newestAvailablePosition - age;
            log.DebugFormat("Current position: {0}, position at age: {1}", newestAvailablePosition, target);
        }

        /// <summary>
        /// Free the circular buffer and reset state.
        /// </summary>
        public void FreeAll()
        {
            stopwatch.Restart();

            if (fullCapacity == 0 && !allocated)
                return;

            log.DebugFormat("Freeing {0} frames.", fullCapacity);

            frames.Clear();
            GC.Collect(2);

            ResetData();

            log.DebugFormat("Freed delay buffer: {0} ms. Total: {1} frames.", stopwatch.ElapsedMilliseconds, frames.Count);
        }
        
        private void ResetData()
        {
            allocated = false;
            fullCapacity = 0;
            rect = Rectangle.Empty;
            imageDescriptor = ImageDescriptor.Invalid;
            availableMemory = 0;
            currentPosition = -1;
        }

        private void FreeSome(int targetCapacity)
        {
            stopwatch.Restart();

            log.DebugFormat("Freeing {0} frames.", fullCapacity - targetCapacity);

            for (int i = frames.Count - 1; i >= targetCapacity; i--)
            {
                frames.RemoveAt(i);
            }

            GC.Collect(2);

            log.DebugFormat("Freed delay buffer: {0} ms. Total: {1} frames.", stopwatch.ElapsedMilliseconds, frames.Count);
        }
        #endregion

    }
}
