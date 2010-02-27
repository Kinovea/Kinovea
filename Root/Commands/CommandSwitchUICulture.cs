using Kinovea.Root.Languages;
using System;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Threading;
using Kinovea.Services;

namespace Kinovea.Root
{
    class CommandSwitchUICulture : IUndoableCommand
    {
		public string FriendlyName
        {
        	get { return RootLang.CommandSwitchUICulture_FriendlyName; }
        }

        #region Members
        private CultureInfo ci;
        private CultureInfo oldCi;
        private Thread thread;
        private RootKernel kernel;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public CommandSwitchUICulture(RootKernel kernel, Thread thread, CultureInfo ci, CultureInfo oldCi)
        {
            this.oldCi = oldCi; 
            this.ci = ci;
            this.thread = thread;
            this.kernel = kernel;
        }
        #endregion

        public void Execute()
        {
        	log.Debug(String.Format("Changing culture from [{0}] to [{1}].", oldCi.Name, ci.Name));
        	ChangeToCulture(ci);
        }

        public void Unexecute()
        {
        	log.Debug(String.Format("Changing back culture from [{0}] to [{1}].", ci.Name, oldCi.Name));
        	ChangeToCulture(oldCi);
        }
        private void ChangeToCulture(CultureInfo _newCulture)
        {
            PreferencesManager pm = PreferencesManager.Instance();
            pm.UICultureName = _newCulture.Name;
            thread.CurrentUICulture = pm.GetSupportedCulture();
            kernel.RefreshUICulture();
            pm.Export();
        }

    }
}
