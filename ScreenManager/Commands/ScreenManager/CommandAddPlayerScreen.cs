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
using System.Reflection;
using System.Resources;
using System.Threading;

using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    //--------------------------------------------
    // CommandAddPlayerScreen -> devrait être réversible ?
    // Charge le fichier spécifier dans un écran, en créé un si besoin.
    // Si ok, réorganise les écrans pour montrer le nouveau ou décharger un ancien si besoin
    // Affiche le nouvel écran avec la vidéo dedans, prête.
    //--------------------------------------------
    public class CommandAddPlayerScreen : IUndoableCommand
    {

        public string FriendlyName
        {
            get 
            {
                ResourceManager rm = new ResourceManager("Kinovea.ScreenManager.Languages.ScreenManagerLang", Assembly.GetExecutingAssembly());
                return rm.GetString("CommandAddPlayerScreen_FriendlyName", Thread.CurrentThread.CurrentUICulture);
            }
        }
        
        ScreenManagerKernel screenManagerKernel;

        #region constructor
        public CommandAddPlayerScreen(ScreenManagerKernel _smk, bool _bStoreState)
        {
            screenManagerKernel = _smk;
            if (_bStoreState) { screenManagerKernel.StoreCurrentState(); }
        }
        #endregion

        /// <summary>
        /// Add a PlayerScreen to the screen list and initialize it.
        /// </summary>
        public void Execute()
        {
            PlayerScreen screen = new PlayerScreen();
            
            // Delegates
            screen.CloseMe += new AbstractScreen.DelegateCloseMe(screenManagerKernel.Screen_CloseAsked);
            screen.SetMeAsActiveScreen += new AbstractScreen.DelegateSetMeAsActiveScreen(screenManagerKernel.Screen_SetActiveScreen);
            screen.m_PlayerIsReady += new PlayerScreen.PlayerIsReady(screenManagerKernel.Player_IsReady);
            screen.m_PlayerSelectionChanged  += new PlayerScreen.PlayerSelectionChanged(screenManagerKernel.Player_SelectionChanged);

            screen.refreshUICulture(); 

            screenManagerKernel.screenList.Add(screen);
        }
        public void Unexecute()
        {
            screenManagerKernel.RecallState();
        }
    }
}
