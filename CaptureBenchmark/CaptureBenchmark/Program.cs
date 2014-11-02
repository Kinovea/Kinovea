using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace CaptureBenchmark
{
    static class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("");
        
        [STAThread]
        static void Main()
        {
            
            Thread.CurrentThread.Name = "Main";
            log.Info("--------------------------");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            View view = new View();
            Presenter presenter = new Presenter(view);
            Application.Run(view);
        }
    }
}
