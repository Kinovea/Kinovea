using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using SyncRecording;

namespace Analysistem
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            /*Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());*/

            bool isStart = true; // you set this depending on if you want to start/stop recording
            SyncR.ClickTargets(isStart); // ClickTargets() for syncing via autoclicker <-- basically just for testing atm
            SyncR.Record(isStart); // Record() for syncing via Kinovea.Record() and clicking SparkVue <-- the function we will likely actually use
        }
    }
}
