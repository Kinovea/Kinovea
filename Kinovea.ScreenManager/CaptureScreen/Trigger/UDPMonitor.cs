using Kinovea.Services;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Monitors UDP packets arriving on a defined port and triggers the capture.
    /// </summary>
    public class UDPMonitor : IDisposable
    {
        #region Events
        public event EventHandler Triggered;
        #endregion

        #region Properties
        public bool Enabled
        {
            get { return enabled; }
            set
            {
                enabled = value;
                if (!enabled && started)
                    Stop();
            }
        }

        public int Port
        {
            get { return port; }
            set { port = value; }
        }
        #endregion

        #region Members
        private bool enabled;
        private bool started;
        private bool changePortAsked;
        private bool stopAsked;
        private UdpClient udpServer;
        private int port = DEFAULT_PORT;
        private Control dummy = new Control();
        private static int DEFAULT_PORT = 8875;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public UDPMonitor()
        {
            // Needed to show that the main thread "owns" this Control.
            IntPtr forceHandleCreation = dummy.Handle;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~UDPMonitor()
        {
            Dispose(false);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (udpServer != null)
                {
                    udpServer.Close();
                    udpServer.Dispose();
                }

                dummy.Dispose();
            }
        }

        public void Start(int nextPort)
        {
#if DEBUG
            if (!Enabled)
                throw new InvalidProgramException();
#endif

            if (started && nextPort == port)
                return;

            if (started)
            {
                // Change of port asked.
                log.DebugFormat("UDP monitor already started on port {0}.", port);
                changePortAsked = true;
                Stop();
            }

            port = nextPort;
            udpServer = new UdpClient(port);
            stopAsked = false;
            changePortAsked = false;
            log.DebugFormat("Starting UDP monitor on port {0}.", port);
            udpServer.BeginReceive(new AsyncCallback(OnUDPData), udpServer);
            started = true;
        }

        private void OnUDPData(IAsyncResult result)
        {
            // This runs in the background thread.
            // The way to cancel an ongoing async receive is to call Close and then 
            // call EndReceive which will throw an exception.
            // https://stackoverflow.com/questions/18309974/how-do-you-cancel-a-udpclientbeginreceive
            try
            {
                IPEndPoint source = new IPEndPoint(0, 0);
                byte[] message = udpServer.EndReceive(result, ref source);
            }
            catch (Exception e)
            {
                return;
            }

            if (!Enabled || !started || stopAsked || changePortAsked)
                return;

            if (QuietPeriodHelper.IsQuiet())
            {
                log.DebugFormat("UDP trigger during quiet period: ignored.");
            }
            else
            {
                log.DebugFormat("UDP trigger.");

                if (Triggered != null)
                {
                    dummy.BeginInvoke((Action)delegate {
                        Triggered(this, EventArgs.Empty);
                    });
                }
            }

            // Start listening for the next packet.
            udpServer.BeginReceive(new AsyncCallback(OnUDPData), udpServer);
        }

        public void Stop()
        {
            if (!started)
                return;

            stopAsked = true;
            started = false;
            udpServer.Close();
            log.DebugFormat("UDP trigger monitor stopped.");
        }
    }
}
