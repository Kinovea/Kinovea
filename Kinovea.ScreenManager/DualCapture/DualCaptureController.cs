using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public class DualCaptureController
    {
        #region Properties
        public CommonControlsCapture View
        {
            get { return view; }
        }
        public bool Active
        {
            get { return active; }
        }
        #endregion

        #region Members
        bool active;
        private CommonControlsCapture view = new CommonControlsCapture();
        private List<CaptureScreen> screens = new List<CaptureScreen>();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        #endregion

        #region Constructor
        public DualCaptureController()
        {
            view.Dock = DockStyle.Fill;

            view.GrabbingChanged += CCtrl_GrabbingChanged;
            view.SnapshotAsked += CCtrl_SnapshotAsked;
            view.RecordingChanged += CCtrl_RecordingChanged;
        }
        #endregion

        #region Public methods
        public void ScreenListChanged(List<AbstractScreen> screenList)
        {
            if (screenList.Count == 2 && screenList[0] is CaptureScreen && screenList[1] is CaptureScreen)
                Enter(screenList);
            else
                Exit();
        }
        public void RefreshUICulture()
        {
            view.RefreshUICulture();
        }
        public void ExecuteDualCommand(HotkeyCommand cmd)
        {
            view.ExecuteDualCommand(cmd.CommandCode);
        }
        #endregion

        #region Screens event handlers
        private void Screen_RecordingStarted(object sender, EventArgs e)
        {
            view.UpdateRecordingStatus(true);
        }
        private void Screen_RecordingStopped(object sender, EventArgs e)
        {
            if (screens.All(s => !s.Recording))
                view.UpdateRecordingStatus(false);
        }
        #endregion

        #region Entering/Exiting dual capture management
        private void Enter(List<AbstractScreen> screenList)
        {
            Exit();

            screens.Clear();
            screens.Add((CaptureScreen)screenList[0]);
            screens.Add((CaptureScreen)screenList[1]);

            foreach (CaptureScreen screen in screens)
            {
                AddEventHandlers(screen);
                screen.Synched = true;
            }

            active = true;
        }
        private void Exit()
        {
            if (active)
            {
                foreach (CaptureScreen screen in screens)
                {
                    RemoveEventHandlers(screen);
                    screen.Synched = false;
                }

                screens.Clear();
            }

            active = false;
        }
        private void AddEventHandlers(CaptureScreen screen)
        {
            screen.RecordingStarted += Screen_RecordingStarted;
            screen.RecordingStopped += Screen_RecordingStopped;
        }
        private void RemoveEventHandlers(CaptureScreen screen)
        {
            screen.RecordingStarted -= Screen_RecordingStarted;
            screen.RecordingStopped -= Screen_RecordingStopped;
        }
        #endregion

        #region View event handlers
        private void CCtrl_GrabbingChanged(object sender, EventArgs<bool> e)
        {
            foreach (CaptureScreen screen in screens)
                screen.ForceGrabbingStatus(e.Value);
        }
        private void CCtrl_SnapshotAsked(object sender, EventArgs e)
        {
            foreach (CaptureScreen screen in screens)
                screen.PerformSnapshot();
        }
        private void CCtrl_RecordingChanged(object sender, EventArgs<bool> e)
        {
            foreach (CaptureScreen screen in screens)
                screen.ForceRecordingStatus(e.Value);
        }
        #endregion
        
        #region Private methods

        #endregion
    }
}
