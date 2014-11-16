using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinovea.Services;
using System.Threading;
using Kinovea.Pipeline.MemoryLayout;

namespace Kinovea.Pipeline
{
    /// <summary>
    /// The main data structure and facilitator between the producer and consumers.
    /// Inspired by the disruptor pattern.
    /// 
    /// Preallocated buffer to avoid allocation and GC, and improve spatial locality.
    /// Constrain capacity to powers of two to use bitwise "&" rather than modulo when computing actual slot number.
    /// </summary>
    public class RingBuffer
    {
        public int FrameLength
        {
            get { return slots[0].Bytes.Length; }
        }

        public long ProducerPosition
        {
            get { return producerPosition.Data; }
        }

        private Frame[] slots;
        private int capacity;
        private int remainderMask;
        private Random random = new Random();
        private List<IFrameConsumer> consumers;
        private CacheLineStorageLong producerPosition = new CacheLineStorageLong(-1); // Last position written to by the producer.
        private BenchmarkMode benchmarkMode;

        public RingBuffer(int capacity, int width, int height, int depth)
        {
            if ((capacity & (capacity - 1)) != 0)
                throw new ArgumentException("Capacity must be a power of two.");

            remainderMask = capacity - 1;

            this.capacity = capacity;
            slots = new Frame[capacity];

            for (int i = 0; i < slots.Count(); i++)
            {
                slots[i] = new Frame(width, height, depth);
            }
        }

        public void SetConsumers(List<IFrameConsumer> consumers)
        {
            this.consumers = consumers;
        }

        public void SetBenchmarkMode(BenchmarkMode benchmarkMode)
        {
            this.benchmarkMode = benchmarkMode;
        }

        public Frame GetEntry(long position)
        {
            return slots[(int)(position & remainderMask)];
        }

        #region Producer barrier
        /// <summary>
        /// Waits for all readers to be past the wrap point, then returns the entry to fill.
        /// The producer will then stuff the entry and commit.
        /// </summary>
        public void Claim(out Frame entry)
        {
            //-------------------------
            // Runs in producer thread.
            //-------------------------

            long nextPosition = producerPosition.Data + 1;

            WaitForReaders(nextPosition);

            entry = slots[(int)(nextPosition & remainderMask)];

            return;
        }

        /// <summary>
        /// Test whether all active readers are past the wrap point, and returns the entry to fill.
        /// Returns the writeability of the entry.
        /// This is not blocking and is the preferred way to manage frame drops.
        /// The entry is returned anyway for testing scenarios that want to overwrite readers.
        /// </summary>
        public bool TryClaim(out Frame entry)
        {
            //-------------------------
            // Runs in producer thread.
            //-------------------------

            long nextPosition = producerPosition.Data + 1;

            bool writeable = IsWriteable(nextPosition);

            entry = slots[(int)(nextPosition & remainderMask)];

            return writeable;
        }

        public void Commit()
        {
            //-------------------------
            // Runs in producer thread.
            //-------------------------

            // The producer has finished stuffing the bytes in the Frame.
            // Mark the position as available for reading.
            producerPosition.Data = producerPosition.Data + 1;
        }

        private void WaitForReaders(long position)
        {
            //-------------------------
            // Runs in producer thread.
            //-------------------------

            // Prefer the IsWriteable method for complete control over dropped frames.
            // With this method the camera wrapper thread is slowed down, and drops are left to its decision.

            if (benchmarkMode == BenchmarkMode.Bradycardia)
            {
                // Artificially slow down the producer thread to see how the hardware/driver reacts.
                Thread.Sleep(50);
                return;
            }

            // Compute wrap point : position - bufferSize.
            // Get minimum position from readers.

            // Yield thread until all readers are past the wrap point.
            //while (wrapPoint > (minPosition = GetMinimumPosition()))
        }

        private bool IsWriteable(long position)
        {
            //-------------------------
            // Runs in producer thread.
            //-------------------------

            if (benchmarkMode == BenchmarkMode.FrameDrops)
            {
                // Simulates readers that have occasional periods of trouble.
                double dropRate = 0.25;
                bool drop = random.NextDouble() < dropRate;
                return !drop;
            }

            // Test whether all active readers have read past the wrap point.
            long mustHaveRead = position - slots.Length;
            return consumers.All(c => !c.Active || c.ConsumerPosition >= mustHaveRead);
        }
        #endregion

        #region Consumer barrier
        public long WaitFor(long position)
        {
            //---------------------------
            // Runs in a consumer thread.
            //---------------------------

            // In the case of a fast consumer, this method will Yield the consumer thread until the asked position is written.
            // In the case of a slow consumer, this method will return instantly with the current producer position,
            // this way the consumer can consume all the frames up to the current position on its own, in a tight loop.
            // Thread context switch via Sleep() is not critical here, because it can only happen when the consumer is 
            // faster than the producer anyway, so it should have time to catch-up later on.
            while (position > producerPosition.Data)
            {
                //Thread.Yield(); // Only in .NET 4.0
                Thread.Sleep(0);
            }
            
            return producerPosition.Data;
        }

        #endregion
    }
}
