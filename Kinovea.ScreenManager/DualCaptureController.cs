using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    class DualCaptureController
    {
        #region Properties
        public CommonControlsCapture View
        {
            get { return view; }
        }
        #endregion

        #region Members
        private CommonControlsCapture view = new CommonControlsCapture();
        #endregion

        #region Constructor
        public DualCaptureController()
        {
            view.Dock = DockStyle.Fill;
        }
        #endregion

        #region Public methods
        public void ScreenListChanged(List<AbstractScreen> screenList)
        {
            // TODO.
        }
        public void RefreshUICulture()
        {
            view.RefreshUICulture();
        }
        #endregion

        #region Private methods

        #endregion
    }
}
