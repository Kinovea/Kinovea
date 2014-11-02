using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Kinovea.Services;

namespace CaptureBenchmark
{
    public partial class View : Form
    {
        public event EventHandler<EventArgs<BenchmarkMode>> StartBenchmark;
        public event EventHandler StopBenchmark;
        private Timer timer = new Timer();
        private DateTime start;
        private double testDuration;

        public View()
        {
            InitializeComponent();
            pb.Maximum = 101;
            
            timer.Tick += timer_Tick;
            timer.Interval = 100;
            testDuration = 10 * 1000;
            //testDuration = 2 * 1000;

        }

        private void timer_Tick(object sender, EventArgs e)
        {
            double ratio = ((DateTime.Now - start).TotalMilliseconds) / testDuration;
            int value = (int)(ratio * 100);
            pb.Value = Math.Min(pb.Maximum, value + 1);
            pb.Value = pb.Value - 1;

            if (ratio >= 1.0)
            {
                timer.Stop();

                if (StopBenchmark != null)
                    StopBenchmark(this, EventArgs.Empty);
            }
        }

        private void btnHeartbeat_Click(object sender, EventArgs e)
        {
            if (StartBenchmark != null)
                StartBenchmark(this, new EventArgs<BenchmarkMode>(BenchmarkMode.Heartbeat));
            
            start = DateTime.Now;
            timer.Start();
        }

        private void btnCommitbeat_Click(object sender, EventArgs e)
        {
            if (StartBenchmark != null)
                StartBenchmark(this, new EventArgs<BenchmarkMode>(BenchmarkMode.Commitbeat));
            
            start = DateTime.Now;
            timer.Start();
        }

        private void btnBrady_Click(object sender, EventArgs e)
        {
            if (StartBenchmark != null)
                StartBenchmark(this, new EventArgs<BenchmarkMode>(BenchmarkMode.Bradycardia));
            
            start = DateTime.Now;
            timer.Start();
        }

        private void btnDrops_Click(object sender, EventArgs e)
        {
            if (StartBenchmark != null)
                StartBenchmark(this, new EventArgs<BenchmarkMode>(BenchmarkMode.FrameDrops));

            start = DateTime.Now;
            timer.Start();
        }

        private void btnNoop_Click(object sender, EventArgs e)
        {
            if (StartBenchmark != null)
                StartBenchmark(this, new EventArgs<BenchmarkMode>(BenchmarkMode.Noop));

            start = DateTime.Now;
            timer.Start();
        }

        private void btnOccslow_Click(object sender, EventArgs e)
        {
            if (StartBenchmark != null)
                StartBenchmark(this, new EventArgs<BenchmarkMode>(BenchmarkMode.OccasionallySlow));

            start = DateTime.Now;
            timer.Start();
        }

        private void btnSlow_Click(object sender, EventArgs e)
        {
            if (StartBenchmark != null)
                StartBenchmark(this, new EventArgs<BenchmarkMode>(BenchmarkMode.Slow));

            start = DateTime.Now;
            timer.Start();
        }

        private void btnLZ4_Click(object sender, EventArgs e)
        {
            if (StartBenchmark != null)
                StartBenchmark(this, new EventArgs<BenchmarkMode>(BenchmarkMode.LZ4));

            start = DateTime.Now;
            timer.Start();
        }

        private void btnFrameNumber_Click(object sender, EventArgs e)
        {
            if (StartBenchmark != null)
                StartBenchmark(this, new EventArgs<BenchmarkMode>(BenchmarkMode.FrameNumber));

            start = DateTime.Now;
            timer.Start();
        }

        private void btnJPEG1_Click(object sender, EventArgs e)
        {
            if (StartBenchmark != null)
                StartBenchmark(this, new EventArgs<BenchmarkMode>(BenchmarkMode.JPEG1));

            start = DateTime.Now;
            timer.Start();
        }
    }
}
