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

using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    //--------------------------------------------
    // CommandLoadMovieInScreen.
    //
    // Gestion des �crans.
    // Charge le fichier sp�cifi� dans un �cran, en cr�� un si besoin.
    // Ou demande ce qu'il faut faire en fonction des options de config.
    // Affiche le nouvel �cran avec la vid�o dedans, pr�te.
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

            if (ForceScreen != -1)
            {
                // Position d'�cran forc�e: V�rifier s'il y a des choses � enregistrer.

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
                    // Utiliser l'�cran, qu'il soit vide ou plein.
                    clm = new CommandLoadMovie(ps, filePath);
                    CommandManager.LaunchCommand(clm);

                    //Si on a pu charger la vid�o, sauver dans l'historique
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
	                            if (ps.FrameServer.VideoFile.Loaded)
	                            {
	                                SaveFileToHistory(filePath);
	                            }

                            	//Afficher l'�cran qu'on vient de le cr�er.
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
	
	                                //Si on a pu charger la vid�o, sauver dans l'historique
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
                            //Chercher un �cran vide. 
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
                                // On a trouv� un �cran vide, charger la vid�o dedans.
                                clm = new CommandLoadMovie((PlayerScreen)screenManagerKernel.screenList[iEmptyScreen], filePath);
                                CommandManager.LaunchCommand(clm);

                                //Si on a pu charger la vid�o, sauver dans l'historique
                                if (((PlayerScreen)screenManagerKernel.screenList[iEmptyScreen]).FrameServer.VideoFile.Loaded)
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
	
	                                    //Si on a pu charger la vid�o, sauver dans l'historique
	                                    if (ps.FrameServer.VideoFile.Loaded)
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
	                                            screenManagerKernel.Screen_SetActiveScreen(screenManagerKernel.screenList[0]);
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
