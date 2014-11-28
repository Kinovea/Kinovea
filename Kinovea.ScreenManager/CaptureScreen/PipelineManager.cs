using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinovea.Pipeline;
using Kinovea.Services;
using Kinovea.Pipeline.Consumers;

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

        private bool connected;
        private FramePipeline pipeline;
        private IFrameProducer producer;
        private ConsumerDisplay consumerDisplay;
        private ConsumerMJPEGRecorder consumerRecord;
        private List<IFrameConsumer> consumers = new List<IFrameConsumer>();

        public void Connect(ImageDescriptor imageDescriptor, IFrameProducer producer, ConsumerDisplay consumerDisplay, ConsumerMJPEGRecorder consumerRecord)
        {
            // At that point the consumer threads are already started.
            // But only the display thread (actually the UI main thread) should be "active".
            // The producer thread is not started yet, it will be started outside the pipeline manager.
            this.producer = producer;
            this.consumerDisplay = consumerDisplay;
            this.consumerRecord = consumerRecord;

            consumerDisplay.SetImageDescriptor(imageDescriptor);
            consumerRecord.SetImageDescriptor(imageDescriptor);

            consumers.Clear();
            consumers.Add(consumerDisplay as IFrameConsumer);
            consumers.Add(consumerRecord as IFrameConsumer);

            int buffers = 8;

            pipeline = new FramePipeline(producer, consumers, buffers, imageDescriptor.BufferSize);
            pipeline.SetBenchmarkMode(BenchmarkMode.None);

            producer.FrameProduced += producer_FrameProduced;
            connected = true;
        }

        public void Disconnect()
        {
            if (!connected)
                return;

            producer.FrameProduced -= producer_FrameProduced;
            pipeline.Teardown();

            connected = false;
        }

        public void StartRecord()
        {
            consumerRecord.Activate();
            pipeline.ResetDrops();
        }

        public void StopRecord()
        {
            consumerRecord.Deactivate();
        }

        private void producer_FrameProduced(object sender, FrameProducedEventArgs e)
        {
            if (FrameSignaled != null)
                FrameSignaled(this, EventArgs.Empty);
        }
    }
}
