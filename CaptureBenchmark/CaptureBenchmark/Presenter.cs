using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinovea.Camera;
using Kinovea.Services;
using Kinovea.Pipeline;
using Kinovea.Pipeline.Consumers;
using System.Threading;

namespace CaptureBenchmark
{
    public class Presenter
    {
        private FramePipeline pipeline;
        private IFrameGrabber grabber;

        private List<IFrameConsumer> consumers = new List<IFrameConsumer>();
        private ConsumerNoop noop = new ConsumerNoop();
        private ConsumerOccasionallySlow occasionallySlow = new ConsumerOccasionallySlow();
        private ConsumerSlow slow = new ConsumerSlow();
        private ConsumerLZ4 lz4 = new ConsumerLZ4();
        private ConsumerFrameNumber frameNumbers = new ConsumerFrameNumber();
        private BenchmarkMode benchmarkMode = BenchmarkMode.None;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Presenter(View view)
        {
            view.Load += view_Load;
            view.StartBenchmark += view_StartBenchmark;
            view.StopBenchmark += view_StopBenchmark;

            // Start long lived thread.

            //Thread thread = new Thread(longLived.Run) { IsBackground = true };
            //thread.Start();
        }

        private void view_StartBenchmark(object sender, EventArgs<BenchmarkMode> e)
        {
            IFrameProducer frameProducer = grabber as IFrameProducer;
            //int width = grabber.Size.Width;
            //int height = grabber.Size.Height;
            //int depth = grabber.Depth;
            int width = 2048;
            int height = 1084;
            int depth = 1;

            benchmarkMode = e.Value;

            switch (benchmarkMode)
            {
                case BenchmarkMode.Heartbeat:
                case BenchmarkMode.Commitbeat:
                case BenchmarkMode.Bradycardia:
                case BenchmarkMode.FrameDrops:
                    break;

                case BenchmarkMode.Noop:
                    consumers.Add(noop);
                    break;
                case BenchmarkMode.OccasionallySlow:
                    consumers.Add(occasionallySlow);
                    break;
                case BenchmarkMode.Slow:
                    consumers.Add(slow);
                    break;
                case BenchmarkMode.LZ4:
                    consumers.Add(lz4);
                    break;
                case BenchmarkMode.FrameNumberToDisk:
                    consumers.Add(frameNumbers);
                    break;

                default:
                    break;
            }

            // Start all consumer threads.
            // In a real application some consumers would have been long started. 
            // for example the application main thread is also the display consumer.
            foreach (IFrameConsumer consumer in consumers)
            {
                Thread thread = new Thread(consumer.Run) { IsBackground = true };
                thread.Name = consumer.GetType().Name;
                thread.Start();
            }
            
            // Bind the ring buffer to consumers and producers.
            pipeline = new FramePipeline(frameProducer, consumers, width, height, depth);

            pipeline.SetBenchmarkMode(benchmarkMode);    
            grabber.Start();

            // Activate all consumers.
            // In a real application this may be done much later, in a "record" button event handler for example.
            foreach (IFrameConsumer consumer in consumers)
                consumer.Activate();
        }

        private void view_StopBenchmark(object sender, EventArgs e)
        {
            foreach (IFrameConsumer consumer in consumers)
            {
                consumer.Deactivate();
                while (consumer.Active)
                {
                    // Busy spin while the consumer thread reach a clean break.
                }
            }

            grabber.Stop();

            Dictionary<string, IBenchmarkCounter> counters = pipeline.StopBenchmark();
            foreach (IFrameConsumer consumer in consumers)
            {
                AbstractConsumer c = consumer as AbstractConsumer;
                if (c == null)
                    continue;

                var counter = c.BenchmarkCounter;
                if (counter != null)
                    counters.Add(consumer.GetType().Name, counter);
            }
            
            List<string> extraBefore = new List<string>();
            AddSessionInformation(extraBefore);

            List<string> extraAfter = new List<string>();
            AddMachineInformation(extraAfter);
            
            BenchmarkReport br = new BenchmarkReport(extraBefore, extraAfter, counters);
            br.ShowDialog();
            br.Dispose();
        }
        
        private void view_Load(object sender, EventArgs e)
        {
            CameraTypeManager.CamerasDiscovered += CameraTypeManager_CamerasDiscovered;
            //CameraTypeManager.CameraSummaryUpdated += CameraTypeManager_CameraSummaryUpdated;
            //CameraTypeManager.CameraImageReceived += CameraTypeManager_CameraImageReceived;
            CameraTypeManager.CameraLoadAsked += CameraTypeManager_CameraLoadAsked;

            CameraTypeManager.LoadCameraManagers();
            CameraTypeManager.DiscoverCameras();
        }

        private void CameraTypeManager_CamerasDiscovered(object sender, CamerasDiscoveredEventArgs e)
        {
            //if (currentContent != ThumbnailViewerContent.Cameras)
            //    return;

            //viewerCameras.CamerasDiscovered(e.Summaries);

            List<CameraSummary> summaries = e.Summaries;
            if (e.Summaries.Count == 1)
                LoadCamera(e.Summaries[0]);
        }

        private void CameraTypeManager_CameraSummaryUpdated(object sender, CameraSummaryUpdatedEventArgs e)
        {
            //if (currentContent != ThumbnailViewerContent.Cameras)
            //    return;

            //viewerCameras.CameraSummaryUpdated(e.Summary);
        }

        private void CameraTypeManager_CameraImageReceived(object sender, CameraImageReceivedEventArgs e)
        {
            //if (currentContent == ThumbnailViewerContent.Cameras)
            //    viewerCameras.CameraImageReceived(e.Summary, e.Image);
        }

        private void CameraTypeManager_CameraLoadAsked(object source, CameraLoadAskedEventArgs e)
        {
            //DoLoadCameraInScreen(e.Source, e.Target);
            LoadCamera(e.Source);
        }


        public void LoadCamera(CameraSummary summary)
        {
            CameraTypeManager.StopDiscoveringCameras();

            CameraManager manager = summary.Manager;
            grabber = manager.Connect(summary);

            
        }

        private void AddMachineInformation(List<string> extra)
        {
            extra.Add("Machine information:");

            // Processor
            List<string> cpuNames = WMI.Processor_Name();
            foreach (string s in cpuNames)
                extra.Add(string.Format("- Processor: {0}", s));

            List<string> clockspeed = WMI.Processor_MaxClockSpeed();
            foreach(string s in clockspeed)
                extra.Add(string.Format("- Processor clock speed: {0} MHz", s));

            extra.Add(string.Format("- Logical processors: {0}", Environment.ProcessorCount));
            extra.Add(string.Format("- Physical memory: {0:0.000} GB", WMI.PhysicalMemory_Capacity()));
            extra.Add(string.Format("- OS version: {0}", Environment.OSVersion));
            
            // Physical disks
            List<PhysicalDisk> disks = WMI.PhysicalDisks();
            foreach (PhysicalDisk disk in disks)
            {
                extra.Add(string.Format("- Disk: {0}", disk.Model));
                extra.Add(string.Format("    Type: {0}", disk.Type));
                extra.Add(string.Format("    Interface: {0}", disk.InterfaceType));
                extra.Add(string.Format("    Size: {0:0.000} GB", disk.Size));
            }

            List<LogicalDisk> logicalDisks = WMI.LogicalDisks();
            foreach (LogicalDisk disk in logicalDisks)
            {
                if (disk.Size == 0)
                    continue;

                extra.Add(string.Format("- Partition: {0}", disk.Caption));
                extra.Add(string.Format("    Type: {0}", disk.DriveType.ToString()));
                extra.Add(string.Format("    File System: {0}", disk.FileSystem));
                extra.Add(string.Format("    Size: {0:0.000} GB", disk.Size));
                extra.Add(string.Format("    Free space: {0:0.000} GB", disk.Free));
            }
        }

        private void AddSessionInformation(List<string> extra)
        {
            extra.Add("Session information:");

            extra.Add(string.Format("- Mode: {0}", benchmarkMode.ToString()));
            extra.Add(string.Format("- Capture speed: {0:0.000} FPS", grabber.Framerate));
            extra.Add(string.Format("- Frame interval: {0:0.000} ms", 1000f / grabber.Framerate));
            //extra.Add(string.Format("- Frame Size: {0}×{1}×{2} B", width, height, depth);
            extra.Add(string.Format("- Bandwidth: {0:0.000} MB/s", ((float)pipeline.FrameLength / (1024 * 1024)) * grabber.Framerate));
            extra.Add("");
        }
    }
}
