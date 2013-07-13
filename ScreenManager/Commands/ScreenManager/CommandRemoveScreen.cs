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
        private int screenToRemoveIndex;
        private bool storeState;

        #region constructor
        public CommandRemoveScreen(ScreenManagerKernel screenManagerKernel, int screenToRemoveIndex, bool storeState)
        {
            this.screenManagerKernel = screenManagerKernel;
            this.screenToRemoveIndex = screenToRemoveIndex;
            this.storeState = storeState;
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
            
            if(screenToRemoveIndex == -1)
            {
                screenManagerKernel.RemoveFirstEmpty(storeState);
            }
            else
            {
                AbstractScreen screenToRemove = screenManagerKernel.GetScreenAt(screenToRemoveIndex);
                
                // Explicit. Make the other one the "active" screen if necessary.
                // For now, we do different actions based on screen type. (fixme?)

                if (screenToRemove is PlayerScreen)
                {
                    // check if dirty and ask for saving if so.
                    PlayerScreen ps = (PlayerScreen)screenManagerKernel.screenList[screenToRemoveIndex];

                    bool shouldRemove = true;
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
                            screenManagerKernel.SaveData();
                        }
                        else if (dr == DialogResult.Cancel)
                        {
                            // Cancel the close.
                            shouldRemove = false;
                            screenManagerKernel.CancelLastCommand = true;
                        }
                    }

                    if (shouldRemove)
                    {
                        // We store the current state now.
                        // (We don't store it at construction time to handle the redo case better)
                        if (storeState)
                            screenManagerKernel.StoreCurrentState(); 
                        
                        screenManagerKernel.RemoveScreen(screenToRemove);
                    }
                }
                else if(screenToRemove is CaptureScreen)
                {
                    // We store the current state now.
                    // (We don't store it at construction time to handle the redo case better)
                    if (storeState) 
                        screenManagerKernel.StoreCurrentState(); 
                    
                    screenManagerKernel.RemoveScreen(screenToRemove);
                }
            }
        }
        public void Unexecute()
        {
            screenManagerKernel.RecallState();
        }
    }
}

