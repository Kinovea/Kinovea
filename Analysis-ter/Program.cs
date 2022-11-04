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
            Data dataCT = SyncR.ClickTargets(isStart); // ClickTargets() for syncing via autoclicker <-- basically just for testing atm
            Data dataR = SyncR.Record(isStart); // Record() for syncing via Kinovea.Record() and clicking SparkVue <-- the function we will likely actually use
            Data dataRT = SyncR.RecordThreads(isStart); // " <-- same as above but with threads; kinda seems unnecessary

            Console.WriteLine("CT: {0}us -- [ {1}, {2} ]", dataCT.delay, dataCT.targets[0].minVal, dataCT.targets[1].minVal);
            Console.WriteLine("R: {0}us -- [ {1} ]", dataR.delay, dataR.targets[1].minVal);
            Console.WriteLine("RT: {0}us -- [ {1} ]", dataRT.delay, dataRT.targets[1].minVal);
        }
    }
}
