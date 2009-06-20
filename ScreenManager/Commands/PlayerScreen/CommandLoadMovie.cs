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
using System.IO; 
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Resources;
using System.Reflection;
using Kinovea.Services;
//using Kinovea.VideoFiles;


namespace Kinovea.ScreenManager
{
    //-------------------------------------------------
    // CommandLoadMovie
    //
    // Objet : Rendre le PlayerScreen opérationnel.
    // - Charger un fichier vidéo dans le PlayerServer
    //--------------------------------------------------

    public class CommandLoadMovie : ICommand
    {
        public string FriendlyName
        {
            get
            {
                ResourceManager rm = new ResourceManager("Kinovea.ScreenManager.Languages.ScreenManagerLang", Assembly.GetExecutingAssembly());
                return rm.GetString("CommandLoadMovie_FriendlyName", Thread.CurrentThread.CurrentUICulture);
            }
        }

        String              m_FilePath;
        PlayerScreen        m_PlayerScreen;

        #region constructor
        public CommandLoadMovie( PlayerScreen _PlayerScreen, String _FilePath)
        {
            m_PlayerScreen = _PlayerScreen;    
            m_FilePath = _FilePath;
        }
        #endregion

        /// <summary>
        /// Execution de la commande
        /// </summary>
        
        public void Execute()
        {
            DelegatesPool dp = DelegatesPool.Instance();
            if (dp.StopPlaying != null)
            {
                dp.StopPlaying();
            }
            
			DirectLoad();
        }

        private void DirectLoad()
        {
            
            int iRes = m_PlayerScreen.m_PlayerScreenUI.m_PlayerServer.LoadMovie(m_FilePath);

            //------------------------------------------------------------------
            // Eviter d'utiliser MessageBoxIcon.Error.
            // (Fait vraiment peur, et devrait être réservé aux crash imminents)
            //------------------------------------------------------------------

            switch (iRes)
            {
                case 0:
                    {
                        // Chargement a priori OK. 

                        m_PlayerScreen.m_bIsMovieLoaded = true;
                        m_PlayerScreen.m_sFileName = Path.GetFileName(m_FilePath);
                        m_PlayerScreen.FilePath = m_FilePath;

                        // Essayer de charger la première frame et autres initialisations.
                        int iPostLoadProcess = m_PlayerScreen.m_PlayerScreenUI.PostLoadProcess(0, m_FilePath);

                        switch (iPostLoadProcess)
                        {
                            case 0:
                                // Chargment OK.
                                // On est déjà passé en mode analyse si c'était possible.
                                break;
                            case -1:
                                {
                                    // Le chargement de la première frame à complètement échoué.
                                    // Cause la plus probable, taille image non standard.
                                    Unload();
                                    MessageBox.Show(m_PlayerScreen.m_ResourceManager.GetString("LoadMovie_ImageFormatError", Thread.CurrentThread.CurrentUICulture),
                                            m_PlayerScreen.m_ResourceManager.GetString("LoadMovie_Error", Thread.CurrentThread.CurrentUICulture),
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Exclamation);
                                    break;
                                }
                            case -2:
                                {
                                    // Chargement de la première frame à montré que le fichier était problématique.
                                    Unload();
                                    MessageBox.Show(m_PlayerScreen.m_ResourceManager.GetString("LoadMovie_InconsistantMovieError", Thread.CurrentThread.CurrentUICulture),
                                                    m_PlayerScreen.m_ResourceManager.GetString("LoadMovie_Error", Thread.CurrentThread.CurrentUICulture),
                                                    MessageBoxButtons.OK,
                                                    MessageBoxIcon.Exclamation);
                                    break;
                                }
                            default:
                                break;
                        }
                        break;
                    }
                case 1:
                    {
                        //--------------------------------------------
                        // FFMPEG_ERROR_FILE_NOT_OPENED
                        // Exemple : tentative d'ouverture d'une image, fichier introuvable.
                        //--------------------------------------------
                        Unload();
                        MessageBox.Show(m_PlayerScreen.m_ResourceManager.GetString("LoadMovie_FileNotOpened", Thread.CurrentThread.CurrentUICulture),
                                        m_PlayerScreen.m_ResourceManager.GetString("LoadMovie_Error", Thread.CurrentThread.CurrentUICulture),
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Exclamation);
                        break;
                    }
                case 2:
                    {
                        //--------------------------------------------
                        // FFMPEG_ERROR_STREAM_INFO_NOT_FOUND
                        //--------------------------------------------
                        Unload();
                        MessageBox.Show(m_PlayerScreen.m_ResourceManager.GetString("LoadMovie_StreamInfoNotFound", Thread.CurrentThread.CurrentUICulture),
                                        m_PlayerScreen.m_ResourceManager.GetString("LoadMovie_Error", Thread.CurrentThread.CurrentUICulture),
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Exclamation);
                        break;
                    }
                case 3:
                    {
                        //--------------------------------------------
                        // FFMPEG_ERROR_VIDEO_STREAM_NOT_FOUND
                        // Exemple : tentative d'ouverture d'un mp3
                        //--------------------------------------------
                        Unload();
                        MessageBox.Show(m_PlayerScreen.m_ResourceManager.GetString("LoadMovie_VideoStreamNotFound", Thread.CurrentThread.CurrentUICulture),
                                        m_PlayerScreen.m_ResourceManager.GetString("LoadMovie_Error", Thread.CurrentThread.CurrentUICulture),
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Exclamation);
                        break;
                    }
                case 4:
                    {
                        //--------------------------------------------------
                        // FFMPEG_ERROR_CODEC_NOT_FOUND
                        // Exemple : tentative d'ouverture d'un fichier wmv3
                        //--------------------------------------------------
                        Unload();
                        MessageBox.Show(m_PlayerScreen.m_ResourceManager.GetString("LoadMovie_CodecNotFound", Thread.CurrentThread.CurrentUICulture),
                                        m_PlayerScreen.m_ResourceManager.GetString("LoadMovie_Error", Thread.CurrentThread.CurrentUICulture),
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Exclamation);
                        break;
                    }
                case 5:
                    {
                        //--------------------------------------------
                        // FFMPEG_ERROR_CODEC_NOT_OPENED
                        //--------------------------------------------
                        Unload();
                        MessageBox.Show(m_PlayerScreen.m_ResourceManager.GetString("LoadMovie_CodecNotOpened", Thread.CurrentThread.CurrentUICulture),
                                        m_PlayerScreen.m_ResourceManager.GetString("LoadMovie_Error", Thread.CurrentThread.CurrentUICulture),
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Exclamation);
                        break;
                    }
                case 6:
                    {
                        //--------------------------------------------
                        // FFMPEG_ERROR_CODEC_NOT_SUPPORTED
                        // Exemple : tentative d'ouverture d'un .dpa
                        //--------------------------------------------
                        Unload();
                        MessageBox.Show(m_PlayerScreen.m_ResourceManager.GetString("LoadMovie_CodecNotSupported", Thread.CurrentThread.CurrentUICulture),
                                        m_PlayerScreen.m_ResourceManager.GetString("LoadMovie_Error", Thread.CurrentThread.CurrentUICulture),
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Exclamation);
                        break;
                    }
                case 8:
                    {
                        //-----------------------------------------------------------------------------------
                        // FFMPEG_ERROR_LOAD_CANCELLED
                        // Ne devrait jamais passer par là. 
                        // Ne pas lire la valeur de e.Result si e.Cancelled est a true.
                        //-----------------------------------------------------------------------------------
                        Unload();
                        break;
                    }
                case 9:
                    {
                        //----------------------------------------------------------------------------
                        // FFMPEG_ERROR_FRAMECOUNT_ERROR
                        // On utilise le même message d'erreur que si le codec n'avait pas été trouvé.
                        //----------------------------------------------------------------------------
                        Unload();
                        MessageBox.Show(m_PlayerScreen.m_ResourceManager.GetString("LoadMovie_CodecNotFound", Thread.CurrentThread.CurrentUICulture),
                                        m_PlayerScreen.m_ResourceManager.GetString("LoadMovie_Error", Thread.CurrentThread.CurrentUICulture),
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Exclamation);
                        break;
                    }

                default:
                    {
                        //--------------------------------------------
                        // Exemple : crash dans le PlayerServer
                        //--------------------------------------------
                        Unload();
                        MessageBox.Show(m_PlayerScreen.m_ResourceManager.GetString("LoadMovie_UnkownError", Thread.CurrentThread.CurrentUICulture),
                                        m_PlayerScreen.m_ResourceManager.GetString("LoadMovie_Error", Thread.CurrentThread.CurrentUICulture),
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Exclamation);
                        break;
                    }
            }

            m_PlayerScreen.UniqueId = System.Guid.NewGuid();
          
        }

        private void Unload()
        {
            m_PlayerScreen.m_bIsMovieLoaded = false;
            m_PlayerScreen.m_PlayerScreenUI.DisableControls();
            m_PlayerScreen.m_PlayerScreenUI.UnloadMovie();
        }
    }
}
