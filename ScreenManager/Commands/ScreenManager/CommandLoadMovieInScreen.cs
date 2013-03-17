/*
Copyright � Joan Charmant 2008.
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
using System.Windows.Forms;

using Kinovea.ScreenManager.Languages;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Loads a file in a screen, create the screen if needed.
    /// </summary>
    public class CommandLoadMovieInScreen : IUndoableCommand
    {
        public string FriendlyName
        {
            get { return ScreenManagerLang.CommandLoadMovieInScreen_FriendlyName;}
        }

        private String filePath;
        private ScreenManagerKernel screenManagerKernel;
        private int targetScreen;

        #region constructor
        public CommandLoadMovieInScreen(ScreenManagerKernel screenManagerKernel, String filePath, int targetScreen, bool storeState)
        {
            this.screenManagerKernel = screenManagerKernel;
            this.filePath = filePath;
            this.targetScreen = targetScreen;
            if (storeState) 
                screenManagerKernel.StoreCurrentState();
        }
        #endregion

        public void Execute()
        {
            //-----------------------------------------------------------------------------------------------
            // Principes d'ouverture.
            //
            // 1. Si il n'y a qu'un seul �cran, on ouvre sur place.
            //      On part du principe que l'utilisateur peut se placer en mode DualScreen s'il le souhaite.
            //      Sinon on doit demander si il veut charger sur place ou pas...
            //      Si c'est un �cran Capture -> idem.
            //      On offre de plus la possibilit� d'annuler l'action au cas o�.
            //
            // 2. Si il y a deux players, dont au moins un vide, on ouvre dans le premier vide trouv�.
            //
            // 3. Si il y a deux players plein, on pose � droite.
            //
            // 4+ Variations � d�finir...
            // 4. Si il y a 1 player plein et un capture vide, on ouvre dans le player. 
            //-----------------------------------------------------------------------------------------------
            ICommand clm;
            CommandManager cm = CommandManager.Instance();
            ICommand css = new CommandShowScreens(screenManagerKernel);

            if (targetScreen != -1)
            {
                // Position d'�cran forc�e: V�rifier s'il y a des choses � enregistrer.

                PlayerScreen ps = (PlayerScreen)screenManagerKernel.screenList[targetScreen];
                bool bLoad = true;
                if (ps.FrameServer.Metadata.IsDirty)
                {
                    DialogResult dr = ConfirmDirty();
                    if (dr == DialogResult.Yes)
                    {
                        // Launch the save dialog.
                        // Note: if we cancel this one, we will go on without saving...
                        screenManagerKernel.SaveData();
                    }
                    else if (dr == DialogResult.Cancel)
                    {
                        // Cancel the load.
                        bLoad = false;
                        screenManagerKernel.CancelLastCommand = true;
                    }
                    // else (DialogResult.No) => Do nothing.
                }

                if (bLoad)
                {
                    // Utiliser l'�cran, qu'il soit vide ou plein.
                    clm = new CommandLoadMovie(ps, filePath);
                    CommandManager.LaunchCommand(clm);

                    //Si on a pu charger la vid�o, sauver dans l'historique
                    if (ps.FrameServer.Loaded)
                        SaveFileToHistory(filePath);
                }
            }
            else
            {
                switch (screenManagerKernel.screenList.Count)
                {
                    case 0:
                        {
                            // Ajouter le premier �cran
                            ICommand caps = new CommandAddPlayerScreen(screenManagerKernel, false);
                            CommandManager.LaunchCommand(caps);

                            // Charger la vid�o dedans
                            PlayerScreen ps = screenManagerKernel.screenList[0] as PlayerScreen;
                            if(ps != null)
                            {
	                            clm = new CommandLoadMovie(ps, filePath);
	                            CommandManager.LaunchCommand(clm);
	
	                            //Si on a pu charger la vid�o, sauver dans l'historique
	                            if (ps.FrameServer.Loaded)
	                                SaveFileToHistory(filePath);

                            	//Afficher l'�cran qu'on vient de cr�er.
                            	CommandManager.LaunchCommand(css);
                            }
                            break;
                        }
                    case 1:
                        {
                			PlayerScreen ps = screenManagerKernel.screenList[0] as PlayerScreen;
                			if(ps!=null)
                			{
	                            bool bLoad = true;
	                            if (ps.FrameServer.Metadata.IsDirty)
	                            {
	                                DialogResult dr = ConfirmDirty();
	                                if (dr == DialogResult.Yes)
	                                {
	                                    // Launch the save dialog.
	                                    // Note: if we cancel this one, we will go on without saving...
	                                    screenManagerKernel.SaveData();
	                                }
	                                else if (dr == DialogResult.Cancel)
	                                {
	                                    // Cancel the load.
	                                    bLoad = false;
	                                    screenManagerKernel.CancelLastCommand = true;
	                                }
	                                // else (DialogResult.No) => Do nothing.
	                            }
	
	                            if (bLoad)
	                            {
	                                clm = new CommandLoadMovie(ps, filePath);
	                                CommandManager.LaunchCommand(clm);
	
	                                //Si on a pu charger la vid�o, sauver dans l'historique
	                                if (ps.FrameServer.VideoReader.Loaded)
	                                {
	                                    SaveFileToHistory(filePath);
	                                }
	                            }
                			}
                			else
                			{
                				// Only screen is a capture screen and we try to play a video.
                				// In that case we create a new player screen and load the video in it.
                				
                            	ICommand caps = new CommandAddPlayerScreen(screenManagerKernel, false);
                            	CommandManager.LaunchCommand(caps);
                            	
                            	// Reset the buffer before the video is loaded.
                            	screenManagerKernel.UpdateCaptureBuffers();

                            	// load video.
                            	PlayerScreen newScreen = (screenManagerKernel.screenList.Count > 0) ? (screenManagerKernel.screenList[1] as PlayerScreen) : null;
                            	if(newScreen != null)
                            	{
	                            	clm = new CommandLoadMovie(newScreen, filePath);
	                            	CommandManager.LaunchCommand(clm);
	
	                            	//video loaded finely, save in history.
	                            	if (newScreen.FrameServer.Loaded)
	                                	SaveFileToHistory(filePath);

                            		// Display screens.
                            		CommandManager.LaunchCommand(css);
                            	}
                			}
                            
                            break;
                        }
                    case 2:
                        {
                            //Chercher un �cran vide. 
                            int iEmptyScreen = -1;

                            PlayerScreen ps0 = screenManagerKernel.screenList[0] as PlayerScreen;
                            PlayerScreen ps1 = screenManagerKernel.screenList[1] as PlayerScreen;
                            
                            if (ps0 != null && !ps0.FrameServer.Loaded)
                            {
                                iEmptyScreen = 0;
                            }
                            else if (ps1 != null && !ps1.FrameServer.Loaded)
                            {
                                iEmptyScreen = 1;
                            }


                            if (iEmptyScreen >= 0)
                            {
                                // On a trouv� un �cran vide, charger la vid�o dedans.
                                clm = new CommandLoadMovie((PlayerScreen)screenManagerKernel.screenList[iEmptyScreen], filePath);
                                CommandManager.LaunchCommand(clm);

                                //Si on a pu charger la vid�o, sauver dans l'historique
                                if (((PlayerScreen)screenManagerKernel.screenList[iEmptyScreen]).FrameServer.Loaded)
                                {
                                    SaveFileToHistory(filePath);
                                }

                                //--------------------------------------------
                                // Sur �chec, on ne modifie pas l'�cran actif.
                                // normalement c'est toujours l'autre �cran.
                                //--------------------------------------------
                            }
                            else
                            {
                                // On a pas trouv� d'�cran vide...
                                // Par d�faut : toujours � droite.
                                // (�tant donn� que l'utilisateur � la possibilit� d'annuler l'op�ration
                                // et de revenir � l'ancienne vid�o facilement, autant �viter une bo�te de dialogue.)

                                PlayerScreen ps = screenManagerKernel.screenList[1] as PlayerScreen;
                                if(ps != null)
                                {
	                                bool bLoad = true;
	                                if (ps.FrameServer.Metadata.IsDirty)
	                                {
	                                    DialogResult dr = ConfirmDirty();
	                                    if (dr == DialogResult.Yes)
	                                    {
	                                        // Launch the save dialog.
	                                        // Note: if we cancel this one, we will go on without saving...
	                                        screenManagerKernel.SaveData();
	                                    }
	                                    else if (dr == DialogResult.Cancel)
	                                    {
	                                        // Cancel the load.
	                                        bLoad = false;
	                                        screenManagerKernel.CancelLastCommand = true;
	                                    }
	                                    // else (DialogResult.No) => Do nothing.
	                                }
	
	                                if (bLoad)
	                                {
	
	                                    clm = new CommandLoadMovie(ps, filePath);
	                                    CommandManager.LaunchCommand(clm);
	
	                                    //Si on a pu charger la vid�o, sauver dans l'historique
	                                    if (ps.FrameServer.Loaded)
	                                    {
	                                        SaveFileToHistory(filePath);
	                                    }
	                                    else
	                                    {
	                                        //----------------------------------------------------------------------------
	                                        // Echec de chargement, v�rifier si on ne vient pas d'invalider l'�cran actif.
	                                        //----------------------------------------------------------------------------
	                                        if (screenManagerKernel.m_ActiveScreen == ps)
	                                        {
	                                            screenManagerKernel.SetActiveScreen(screenManagerKernel.screenList[0]);
	                                        }
	                                    }
	                                }
	                            }
                            }

                            // V�rifier qu'on a un �cran actif.
                            // sinon, positionner le premier comme actif.
                            break;
                        }
                    default:
                        break;
                }
            }

            screenManagerKernel.OrganizeCommonControls();
            screenManagerKernel.OrganizeMenus();
            screenManagerKernel.UpdateStatusBar();
        }

        private void SaveFileToHistory(string _FilePath)
        {
            PreferencesManager.FileExplorerPreferences.AddRecentFile(_FilePath);
            PreferencesManager.Save();
        }

        public void Unexecute()
        {
            screenManagerKernel.RecallState();
        }
        
        private DialogResult ConfirmDirty()
        {
            return MessageBox.Show(ScreenManagerLang.InfoBox_MetadataIsDirty_Text.Replace("\\n", "\n"),
                                   ScreenManagerLang.InfoBox_MetadataIsDirty_Title,
                                   MessageBoxButtons.YesNoCancel,
                                   MessageBoxIcon.Question);
            
            
            
        }
    }
}
