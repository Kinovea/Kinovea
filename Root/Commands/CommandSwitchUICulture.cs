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
        // Le nom de la commande pourrait apparaitre dans la langue dans laquelle on était avant...

        public string FriendlyName
        {
            get
            {
                ResourceManager rm = new ResourceManager("Kinovea.Root.Languages.RootLang", Assembly.GetExecutingAssembly());
                return rm.GetString("CommandSwitchUICulture_FriendlyName", Thread.CurrentThread.CurrentUICulture);
            }
        }

        #region Members
        private CultureInfo ci;
        private CultureInfo oldCi;
        private Thread thread;
        private RootKernel kernel;
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
            thread.CurrentUICulture = ci;
            kernel.RefreshUICulture();
            PreferencesManager pm = PreferencesManager.Instance();
            pm.UILanguage = ci.TwoLetterISOLanguageName;
            pm.Export();

            kernel.CheckLanguageMenu();
            
        }

        public void Unexecute()
        {
            thread.CurrentUICulture = oldCi;
            kernel.RefreshUICulture();

            PreferencesManager pm = PreferencesManager.Instance();
            pm.UILanguage = oldCi.TwoLetterISOLanguageName;
            pm.Export();

            kernel.CheckLanguageMenu();
        }

    }
}
