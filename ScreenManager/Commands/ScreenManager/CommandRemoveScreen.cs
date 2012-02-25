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

using Kinovea.ScreenManager.Languages;
using System;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    //---------------------------------------------------------------
    // CommandRemoveScreen
    // entrée : _screenToRemove
    // 0, 1, 2 : index du screen dans la collection du manager.
    // -1 : fermer un écran vide si possible. ( sinon alerte ?)
    // Attention ne travaille que sur la liste d'AbstractScreen, pas sur les UI.
    //---------------------------------------------------------------
    public class CommandRemoveScreen : IUndoableCommand
    {
        public string FriendlyName
        {
        	get { return ScreenManagerLang.CommandRemoveScreen_FriendlyName; }
        }
        private ScreenManagerKernel screenManagerKernel;
        private int iScreenToRemove;
        private bool m_bStoreState;

        #region constructor
        public CommandRemoveScreen(ScreenManagerKernel _smk, int _iScreenToRemove, bool _bStoreState)
        {
            screenManagerKernel = _smk;
            iScreenToRemove = _iScreenToRemove;
            m_bStoreState = _bStoreState;
        }
        #endregion

        /// <summary>
        /// Execution de la commande
        /// </summary>
        public void Execute()
        {

			// When there is only one screen, we don't want it to have the "active" screen look.
            // Since we have 2 screens at most, we first clean up all of them, 
            // and we'll display the active screen look afterwards, only if needed.
            foreach (AbstractScreen screen in screenManagerKernel.screenList)
            {
                screen.DisplayAsActiveScreen(false);
            }

        	// There are two types of closing demands: explicit and implicit.
        	// explicit-close ask for a specific screen to be closed.
        	// implicit-close just ask for a close, we choose which one here.
        	
            if (iScreenToRemove >= 0 && iScreenToRemove < screenManagerKernel.screenList.Count)
            {
            	// Explicit. Make the other one the "active" screen if necessary.
                // For now, we do different actions based on screen type. (fixme?)
            	
            	if (screenManagerKernel.screenList[iScreenToRemove] is PlayerScreen)
                {
            		// PlayerScreen, check if dirty and ask for saving if so.
            		
                    PlayerScreen ps = (PlayerScreen)screenManagerKernel.screenList[iScreenToRemove];
                    bool bRemove = true;
                    if (ps.FrameServer.Metadata.IsDirty)
                    {
                        DialogResult dr = MessageBox.Show(ScreenManagerLang.InfoBox_MetadataIsDirty_Text.Replace("\\n", "\n"),
                                                          ScreenManagerLang.InfoBox_MetadataIsDirty_Title,
                                                          MessageBoxButtons.YesNoCancel,
                                                          MessageBoxIcon.Question);

                        if (dr == DialogResult.Yes)
                        {
                            // Launch the save dialog.
                            // Note: if user cancels this one, we will not save anything...
                            screenManagerKernel.mnuSaveOnClick(null, EventArgs.Empty);
                        }
                        else if (dr == DialogResult.Cancel)
                        {
                            // Cancel the close.
                            bRemove = false;
                            screenManagerKernel.CancelLastCommand = true;
                        }
                    }

                    if (bRemove)
                    {
                        // We store the current state now.
                        // (We don't store it at construction time to handle the redo case better)
                        if (m_bStoreState) 
                        	screenManagerKernel.StoreCurrentState(); 
                        
                        ps.AfterClose();
                        screenManagerKernel.screenList.RemoveAt(iScreenToRemove);

                        // TODO: Remove all commands that were executed during this screen life from the command history.
                        // But to do that we'll need a way to tell apart commands based on their parent screen...
                    }
                }
            	else if(screenManagerKernel.screenList[iScreenToRemove] is CaptureScreen)
            	{
            		// Capture.
            		// We store the current state now.
                    // (We don't store it at construction time to handle the redo case better)
                    if (m_bStoreState) 
                    { 
                    	screenManagerKernel.StoreCurrentState(); 
                    }
                    
                    screenManagerKernel.screenList[iScreenToRemove].BeforeClose();
                    screenManagerKernel.screenList.RemoveAt(iScreenToRemove);

	                // TODO: Remove all commands that were executed during this screen life from the command history.
	                // But to do that we'll need a way to tell apart commands based on their parent screen...
            	}
                
            	
            }
        	else
            {
                // Implicit close. Find the empty screen.
                // (We actually know for sure that there is indeed an empty screen).
                for(int i=0;i<screenManagerKernel.screenList.Count;i++)
                {
                	if(!screenManagerKernel.screenList[i].Full)
                	{
                		// We store the current state now.
                    	// (We don't store it at construction time to handle the redo case better)
                    	if (m_bStoreState) { screenManagerKernel.StoreCurrentState(); }
                    	screenManagerKernel.screenList.RemoveAt(i);
                    	// TODO: Remove all commands that were executed during this screen life from the command history.
	               		// But to do that we'll need a way to tell apart commands based on their parent screen...
                		break;
                	}
                }
            }

        	// Handle the remaining screen.
            if (screenManagerKernel.screenList.Count > 0)
            {
                screenManagerKernel.Screen_SetActiveScreen(screenManagerKernel.screenList[0]);
            }
            else
            {
                // Show back the Thumbnails...
            }
        }
        public void Unexecute()
        {
            screenManagerKernel.RecallState();
        }
    }
}

