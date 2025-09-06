using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Kinovea.ScreenManager.Languages;
using System.Drawing.Drawing2D;
using Kinovea.Services;
using BrightIdeasSoftware;
using Kinovea.Services.Types;
using System.Globalization;

namespace Kinovea.ScreenManager
{
    public partial class FormPostRecordingCommand : Form
    {
        #region Members
        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public FormPostRecordingCommand()
        {
            InitializeComponent();
            
        }

        
    }
}
