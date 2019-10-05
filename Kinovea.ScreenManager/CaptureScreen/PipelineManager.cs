using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinovea.Pipeline;
using Kinovea.Services;
using Kinovea.Pipeline.Consumers;
using Kinovea.Video;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// The pipeline manager for capture pipelines.
    /// Handle connection between the camera (frame producer), display (consumer) and record (consumer) threads.
    /// </summary>
    public class PipelineManager
    {
        public event EventHandler FrameSignaled;

        public long Drops
        {
            get { return pipeline == null ? 0 : pipeline.Drops; }
        }

        public double Frequency
        {
            get { return pipeline == null ? 0 : pipeline.Frequency; }
        }

        private bool connected;
        private FramePipeline pipeline;
        private IFrameProducer producer;
        private ConsumerMJPEGRecorder consumerRecord;
        private ConsumerDelayer consumerDelayer;
        private List<IFrameConsumer> consumers = new List<IFrameConsumer>();

        public void Connect(ImageDescriptor imageDescriptor, IFrameProducer producer, ConsumerDisplay consumerDisplay, ConsumerMJPEGRecorder consumerRecord)
        {
            // At that point the consumer threads are already started.
            // But only the display thread (actually the UI main thread) should be "active".
            // The producer thread is not started yet, it will be started outside the pipeline manager.
            this.producer = producer;
            this.consumerRecord = consumerRecord;
            this.consumerDelayer = null;

            consumerDisplay.SetImageDescriptor(imageDescriptor);
            consumerRecord.SetImageDescriptor(imageDescriptor);

            consumers.Clear();
            consumers.Add(consumerDisplay as IFrameConsumer);
            consumers.Add(consumerRecord as IFrameConsumer);

            CreatePipeline(imageDescriptor);
        }

        public void Connect(ImageDescriptor imageDescriptor, IFrameProducer producer, ConsumerDisplay consumerDisplay, ConsumerDelayer consumerDelayer)
        {
            // Same as above but for the recording mode "delay" case.
            this.producer = producer;
            this.consumerRecord = null;
            this.consumerDelayer = consumerDelayer;

            consumerDisplay.SetImageDescriptor(imageDescriptor);
            consumerDelayer.SetImageDescriptor(imageDescriptor);

            consumers.Clear();
            consumers.Add(consumerDisplay as IFrameConsumer);
            consumers.Add(consumerDelayer as IFrameConsumer);

            CreatePipeline(imageDescriptor);
        }

        private void CreatePipeline(ImageDescriptor imageDescriptor)
        {
            int buffers = 8;

            pipeline = new FramePipeline(producer, consumers, buffers, imageDescriptor.BufferSize);
            pipeline.SetBenchmarkMode(BenchmarkMode.None);

            if (pipeline.Allocated)
            {
                producer.FrameProduced += producer_FrameProduced;
                connected = true;
            }
        }

        public void Disconnect()
        {
            if (!connected)
                return;

            producer.FrameProduced -= producer_FrameProduced;
            pipeline.Teardown();

            connected = false;
        }

        public SaveResult StartRecord(string filepath, double interval, int age)
        {
            if (consumerRecord == null && consumerDelayer == null)
                throw new InvalidProgramException();

            pipeline.ResetDrops();
            SaveResult result;
            if (consumerRecord != null)
            {
                result = consumerRecord.StartRecord(filepath, interval);
                if (result == SaveResult.Success)
                    consumerRecord.Activate();
            }
            else
            {
                result = consumerDelayer.StartRecord(filepath, interval, age);
            }

            return result;
        }

        public void StopRecord()
        {
            if (consumerRecord == null && consumerDelayer == null)
                throw new InvalidProgramException();

            if (consumerRecord != null)
            {
                consumerRecord.Deactivate();
            }
            else
            {
                consumerDelayer.StopRecord();
            }
        }

        private void producer_FrameProduced(object sender, FrameProducedEventArgs e)
        {
            if (FrameSignaled != null)
                FrameSignaled(this, EventArgs.Empty);
        }
    }
}
