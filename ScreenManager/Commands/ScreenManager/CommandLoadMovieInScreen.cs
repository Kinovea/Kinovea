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
using System.Windows.Forms;

using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    //--------------------------------------------
    // CommandLoadMovieInScreen.
    //
    // Gestion des écrans.
    // Charge le fichier spécifié dans un écran, en créé un si besoin.
    // Ou demande ce qu'il faut faire en fonction des options de config.
    // Affiche le nouvel écran avec la vidéo dedans, prête.
    // Utilise la commande LoadMovie.
    //--------------------------------------------
    public class CommandLoadMovieInScreen : IUndoableCommand
    {
        public string FriendlyName
        {
            get { return Languages.ScreenManagerLang.CommandLoadMovieInScreen_FriendlyName;}
        }

        private ResourceManager m_ResManager  = new ResourceManager("Kinovea.ScreenManager.Languages.ScreenManagerLang", Assembly.GetExecutingAssembly());
        private String filePath;
        private ScreenManagerKernel screenManagerKernel;
        private int ForceScreen;

        #region constructor
        public CommandLoadMovieInScreen(ScreenManagerKernel _smk, String _filePath, int _iForceScreen, bool _bStoreState)
        {
            screenManagerKernel = _smk;
            filePath = _filePath;
            ForceScreen = _iForceScreen;
            if (_bStoreState) { screenManagerKernel.StoreCurrentState(); }
        }
        #endregion

        /// <summary>
        /// Execution de la commande
        /// Si ok, enregistrement du path dans l'historique.
        /// </summary>
        public void Execute()
        {
            //-----------------------------------------------------------------------------------------------
            // Principes d'ouverture.
            //
            // 1. Si il n'y a qu'un seul écran, on ouvre sur place.
            //      On part du principe que l'utilisateur peut se placer en mode DualScreen s'il le souhaite.
            //      Sinon on doit demander si il veut charger sur place ou pas...
            //      Si c'est un écran Capture -> idem.
            //      On offre de plus la possibilité d'annuler l'action au cas où.
            //
            // 2. Si il y a deux players, dont au moins un vide, on ouvre dans le premier vide trouvé.
            //
            // 3. Si il y a deux players plein, on pose à droite.
            //
            // 4+ Variations à définir...
            // 4. Si il y a 1 player plein et un capture vide, on ouvre dans le player. 
            //-----------------------------------------------------------------------------------------------
            ICommand clm;
            CommandManager cm = CommandManager.Instance();
            ICommand css = new CommandShowScreens(screenManagerKernel);

            if (ForceScreen != -1)
            {
                // Position d'écran forcée: Vérifier s'il y a des choses à enregistrer.

                PlayerScreen ps = (PlayerScreen)screenManagerKernel.screenList[ForceScreen-1];
                bool bLoad = true;
                if (ps.m_PlayerScreenUI.Metadata.Dirty)
                {
                    DialogResult dr = MessageBox.Show(m_ResManager.GetString("InfoBox_MetadataIsDirty_Text", Thread.CurrentThread.CurrentUICulture).Replace("\\n", "\n"),
                                                      m_ResManager.GetString("InfoBox_MetadataIsDirty_Title", Thread.CurrentThread.CurrentUICulture),
                                                      MessageBoxButtons.YesNoCancel,
                                                      MessageBoxIcon.Question);

                    if (dr == DialogResult.Yes)
                    {
                        // Launch the save dialog.
                        // Note: if we cancel this one, we will go on without saving...
                        screenManagerKernel.mnuSaveOnClick(null, EventArgs.Empty);
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
                    // Utiliser l'écran, qu'il soit vide ou plein.
                    clm = new CommandLoadMovie(ps, filePath);
                    CommandManager.LaunchCommand(clm);

                    //Si on a pu charger la vidéo, sauver dans l'historique
                    if (ps.FrameServer.VideoFile.Loaded)
                    {
                        SaveFileToHistory(filePath);
                    }
                }
            }
            else
            {
                switch (screenManagerKernel.screenList.Count)
                {
                    case 0:
                        {
                            // Ajouter le premier écran
                            ICommand caps = new CommandAddPlayerScreen(screenManagerKernel, false);
                            CommandManager.LaunchCommand(caps);

                            // Charger la vidéo dedans
                            PlayerScreen ps = screenManagerKernel.screenList[0] as PlayerScreen;
                            if(ps != null)
                            {
	                            clm = new CommandLoadMovie(ps, filePath);
	                            CommandManager.LaunchCommand(clm);
	
	                            //Si on a pu charger la vidéo, sauver dans l'historique
	                            if (ps.FrameServer.VideoFile.Loaded)
	                            {
	                                SaveFileToHistory(filePath);
	                            }

                            	//Afficher l'écran qu'on vient de le créer.
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
	                            if (ps.m_PlayerScreenUI.Metadata.Dirty)
	                            {
	                                DialogResult dr = MessageBox.Show(m_ResManager.GetString("InfoBox_MetadataIsDirty_Text", Thread.CurrentThread.CurrentUICulture).Replace("\\n", "\n"),
	                                                                  m_ResManager.GetString("InfoBox_MetadataIsDirty_Title", Thread.CurrentThread.CurrentUICulture),
	                                                                  MessageBoxButtons.YesNoCancel,
	                                                                  MessageBoxIcon.Question);
	
	                                if (dr == DialogResult.Yes)
	                                {
	                                    // Launch the save dialog.
	                                    // Note: if we cancel this one, we will go on without saving...
	                                    screenManagerKernel.mnuSaveOnClick(null, EventArgs.Empty);
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
	
	                                //Si on a pu charger la vidéo, sauver dans l'historique
	                                if (ps.FrameServer.VideoFile.Loaded)
	                                {
	                                    SaveFileToHistory(filePath);
	                                }
	                            }
                			}
                            
                            break;
                        }
                    case 2:
                        {
                            //Chercher un écran vide. 
                            int iEmptyScreen = -1;

                            PlayerScreen ps0 = screenManagerKernel.screenList[0] as PlayerScreen;
                            PlayerScreen ps1 = screenManagerKernel.screenList[1] as PlayerScreen;
                            
                            if (ps0 != null && ps0.FrameServer.VideoFile.Loaded == false)
                            {
                                iEmptyScreen = 0;
                            }
                            else if (ps1 != null && ps1.FrameServer.VideoFile.Loaded == false)
                            {
                                iEmptyScreen = 1;
                            }


                            if (iEmptyScreen >= 0)
                            {
                                // On a trouvé un écran vide, charger la vidéo dedans.
                                clm = new CommandLoadMovie((PlayerScreen)screenManagerKernel.screenList[iEmptyScreen], filePath);
                                CommandManager.LaunchCommand(clm);

                                //Si on a pu charger la vidéo, sauver dans l'historique
                                if (((PlayerScreen)screenManagerKernel.screenList[iEmptyScreen]).FrameServer.VideoFile.Loaded)
                                {
                                    SaveFileToHistory(filePath);
                                }

                                //--------------------------------------------
                                // Sur échec, on ne modifie pas l'écran actif.
                                // normalement c'est toujours l'autre écran.
                                //--------------------------------------------
                            }
                            else
                            {
                                // On a pas trouvé d'écran vide...
                                // Par défaut : toujours à droite.
                                // (étant donné que l'utilisateur à la possibilité d'annuler l'opération
                                // et de revenir à l'ancienne vidéo facilement, autant éviter une boîte de dialogue.)

                                PlayerScreen ps = screenManagerKernel.screenList[1] as PlayerScreen;
                                if(ps != null)
                                {
	                                bool bLoad = true;
	                                if (ps.m_PlayerScreenUI.Metadata.Dirty)
	                                {
	                                    DialogResult dr = MessageBox.Show(m_ResManager.GetString("InfoBox_MetadataIsDirty_Text", Thread.CurrentThread.CurrentUICulture).Replace("\\n", "\n"),
	                                                                      m_ResManager.GetString("InfoBox_MetadataIsDirty_Title", Thread.CurrentThread.CurrentUICulture),
	                                                                      MessageBoxButtons.YesNoCancel,
	                                                                      MessageBoxIcon.Question);
	
	                                    if (dr == DialogResult.Yes)
	                                    {
	                                        // Launch the save dialog.
	                                        // Note: if we cancel this one, we will go on without saving...
	                                        screenManagerKernel.mnuSaveOnClick(null, EventArgs.Empty);
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
	
	                                    //Si on a pu charger la vidéo, sauver dans l'historique
	                                    if (ps.FrameServer.VideoFile.Loaded)
	                                    {
	                                        SaveFileToHistory(filePath);
	                                    }
	                                    else
	                                    {
	                                        //----------------------------------------------------------------------------
	                                        // Echec de chargement, vérifier si on ne vient pas d'invalider l'écran actif.
	                                        //----------------------------------------------------------------------------
	                                        if (screenManagerKernel.m_ActiveScreen == ps)
	                                        {
	                                            screenManagerKernel.Screen_SetActiveScreen(screenManagerKernel.screenList[0]);
	                                        }
	                                    }
	                                }
	                            }
                            }

                            // Vérifier qu'on a un écran actif.
                            // sinon, positionner le premier comme actif.
                            break;
                        }
                    default:
                        break;
                }
            }

            screenManagerKernel.OrganizeMenus();
            screenManagerKernel.UpdateStatusBar();
        }

        private void SaveFileToHistory(string _FilePath)
        {
            // Enregistrer le nom du fichier dans l'historique.
            PreferencesManager pm = PreferencesManager.Instance();
            pm.HistoryAdd(_FilePath);
            pm.OrganizeHistoryMenu();
        }

        public void Unexecute()
        {
            screenManagerKernel.RecallState();
        }
    }
}
