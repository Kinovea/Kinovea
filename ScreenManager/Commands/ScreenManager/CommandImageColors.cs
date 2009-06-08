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
using System.Resources;
using System.Reflection;
using System.Threading;
using Videa.Services;
using VideaPlayerServer;

namespace Videa.ScreenManager
{

    // This is the command used for all 4 image menus.
    public class CommandImageColors : IUndoableCommand
    {
        public string FriendlyName
        {
            get
            {
                ResourceManager rm = new ResourceManager("Videa.ScreenManager.Languages.ScreenManagerLang", Assembly.GetExecutingAssembly());
                return rm.GetString("CommandImageColors_FriendlyName", Thread.CurrentThread.CurrentUICulture);
            }
        }

        PlayerScreen                    m_PlayerScreen;
        PlayerScreen.ImageFilterType    m_ImageFilterType;
        bool                            m_Dirty = false;
        MemoPlayerScreen m_MemoPlayerScreen;
        FilterParams m_FilterParams;

        #region constructor
        public CommandImageColors(PlayerScreen _PlayerScreen, PlayerScreen.ImageFilterType _ImageFilterType)
        {
            m_PlayerScreen = _PlayerScreen;
            m_ImageFilterType = _ImageFilterType;
            m_MemoPlayerScreen = null;
        }
        #endregion

        public void Execute()
        {
            m_PlayerScreen.StopPlaying();

            if (m_Dirty)
            {
                // This the redo. Re-apply the parameters.
                GetParams(m_FilterParams);
                m_PlayerScreen.FilterImage(m_ImageFilterType);

                // Ré-enregistrement de l'état courant du screen pour le undo.
                m_MemoPlayerScreen = m_PlayerScreen.GetMemo();
            }
            else
            {
                // Launch adjustment dialog box.
                formFilterTuner.ReportFilterParams report = new formFilterTuner.ReportFilterParams(GetParams);
                formFilterTuner ffc = new formFilterTuner(m_PlayerScreen.m_ResourceManager, m_PlayerScreen.m_PlayerScreenUI.m_PlayerServer, report, m_ImageFilterType);
                DialogResult res = ffc.ShowDialog();
                ffc.Dispose();

                if (res == DialogResult.OK && m_Dirty)
                {
                    // Les params choisis ont été positionnés, application du filtre
                    m_PlayerScreen.FilterImage(m_ImageFilterType);

                    // Enregistrement de l'état courant du screen pour le undo.
                    m_MemoPlayerScreen = m_PlayerScreen.GetMemo();
                }
                else
                {
                    // Désactivation du Undo si cancel ou clean.
                    CommandManager cm = CommandManager.Instance();
                    cm.UnstackLastCommand();
                }
            }

            m_PlayerScreen.RefreshImage();
        }

        public void Unexecute()
        {
            // On ne fait pas un vrai undo: on reset la sélection et on recharge les data.
            // Si il y avait plusieurs ajustements, on les perd tous.
            m_PlayerScreen.StopPlaying();
            m_PlayerScreen.ResetSelectionImages(m_MemoPlayerScreen);
            m_PlayerScreen.RefreshImage();
        }

        public void GetParams(FilterParams _params)
        {
            // Function used by the actual dialog to transmit the choosen parameters.

            m_FilterParams = _params;

            if ((_params.iValue != 0) || (m_ImageFilterType == PlayerScreen.ImageFilterType.Edges))
            {
                m_Dirty = true;
            }

            switch (m_ImageFilterType)
            {
                case PlayerScreen.ImageFilterType.Colors:
                    m_PlayerScreen.ColorsFilter = _params;
                    break;
                case PlayerScreen.ImageFilterType.Brightness:
                    m_PlayerScreen.BrightnessFilter = _params;
                    break;

                case PlayerScreen.ImageFilterType.Contrast:
                    m_PlayerScreen.ContrastFilter = _params;
                    break;

                case PlayerScreen.ImageFilterType.Sharpen:
                    m_PlayerScreen.SharpenFilter = _params;
                    break;

                case PlayerScreen.ImageFilterType.Edges:
                    m_PlayerScreen.EdgesFilter = _params;
                    break;

                default:
                    break;
            }
        }
    }
}

