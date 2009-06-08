/*
Copyright © Joan Charmant 2008.
joan.charmant@gmail.com 
 
This file is part of Kinovea.

Kinovea is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License version 2 
as published by the Free Software Foundation.

Kinovea is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Kinovea. If not, see http://www.gnu.org/licenses/.

*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Videa.Services
{
    //------------------------------------------------------------------------------
    // The delegates pool is an area to share services between distant modules.
    // When a module exposes functionnality that will be accessed from an lower level
    // or from a sibling module, it should be done through the delegates pool
    // (instead of dependency injection or delegates tunnels).
    // 
    // The variable is filled by the server module, and called by the consumer.
    //------------------------------------------------------------------------------

    // Types.
    public delegate void DelegateOpenVideoFile();
    public delegate void DelegateLoadMovieInScreen(string _filePath, int _iForceScreen, bool _bStoreState);
    public delegate void DelegateUpdateStatusBar(string _status);
    //public delegate void DelegateCompilePlayerScreen();
    public delegate void DelegateStopPlaying();
    public delegate void DelegateMakeTopMost(Form _form);
    public delegate void DelegateDeactivateKeyboardHandler();
    public delegate void DelegateActivateKeyboardHandler();
    public delegate void DelegateDisplayThumbnails(List<String> _fileNames, bool _bRefreshNow);
    public delegate void DelegateRefreshFileExplorer(bool _bRefreshThumbnails);

    public class DelegatesPool
    {
        #region Exposed Delegates
        public DelegateOpenVideoFile OpenVideoFile;
        public DelegateLoadMovieInScreen LoadMovieInScreen;
        public DelegateUpdateStatusBar UpdateStatusBar;
        //public DelegateCompilePlayerScreen CompilePlayerScreen;
        public DelegateStopPlaying StopPlaying;
        public DelegateMakeTopMost MakeTopMost;
        public DelegateDeactivateKeyboardHandler DeactivateKeyboardHandler;
        public DelegateActivateKeyboardHandler ActivateKeyboardHandler;
        public DelegateDisplayThumbnails DisplayThumbnails;
        public DelegateRefreshFileExplorer RefreshFileExplorer;
        #endregion
  
        #region Instance & Constructor
        private static DelegatesPool _instance = null;
        public static DelegatesPool Instance()
        {
            if (_instance == null)
            {
                _instance = new DelegatesPool();
            }
            return _instance;
        }
        // Private Ctor
        private DelegatesPool()
        {

        }
        #endregion
    }
}
