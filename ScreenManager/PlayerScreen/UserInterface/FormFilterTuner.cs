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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Resources;
using System.Threading;
    
using VideaPlayerServer;


namespace Videa.ScreenManager
{
    public partial class formFilterTuner : Form
    {
        //const int WM_KEYDOWN = 0x100;

        public delegate void ReportFilterParams(FilterParams _params);
        public ReportFilterParams DelegateReportFilterParams;

        private ResourceManager m_ResourceManager = null;
        private PlayerServer m_PlayerServer = null;
        private Bitmap m_bmpOriginal = null;
        private Bitmap m_bmpPreview = null;

        private FilterParams m_FilterParams = null;

        private PlayerScreen.ImageFilterType m_ImageFilterType;

        public formFilterTuner(ResourceManager _ResourceManager, PlayerServer _PlayerServer, ReportFilterParams _DelegateReportFilterParams, PlayerScreen.ImageFilterType _ImageFilterType)
        {
            InitializeComponent();

            m_ResourceManager = _ResourceManager;
            m_PlayerServer = _PlayerServer;
            DelegateReportFilterParams = _DelegateReportFilterParams;
            m_ImageFilterType = _ImageFilterType;

            // defaults
            this.Height = 427;

            // Labels
            switch(m_ImageFilterType)
            {
                // Cas général : [-100 à 100]

                case PlayerScreen.ImageFilterType.Colors:
                    this.Text = "   " + m_ResourceManager.GetString("mnuColors", Thread.CurrentThread.CurrentUICulture);
                    lblValue.Text = m_ResourceManager.GetString("FormFilterColors_lblValue_Text", Thread.CurrentThread.CurrentUICulture);
                    break;
                case PlayerScreen.ImageFilterType.Brightness:
                    this.Text = "   " + m_ResourceManager.GetString("mnuBrightness", Thread.CurrentThread.CurrentUICulture);
                    lblValue.Text = m_ResourceManager.GetString("FormFilterBrightness_lblValue_Text", Thread.CurrentThread.CurrentUICulture);
                    break;
                case PlayerScreen.ImageFilterType.Contrast:
                    this.Text = "   " + m_ResourceManager.GetString("mnuContrast", Thread.CurrentThread.CurrentUICulture);
                    lblValue.Text = m_ResourceManager.GetString("FormFilterContrast_lblValue_Text", Thread.CurrentThread.CurrentUICulture);
                    break;
                case PlayerScreen.ImageFilterType.Sharpen:
                    this.Text = "   " + m_ResourceManager.GetString("mnuSharpen", Thread.CurrentThread.CurrentUICulture);
                    lblValue.Text = m_ResourceManager.GetString("FormFilterSharpen_lblValue_Text", Thread.CurrentThread.CurrentUICulture);
                    // Cas particulier : [0 à 100]
                    trkValue.Minimum = 0;
                    trkValue.Maximum = 100;
                    trkValue.Value = 0;
                    break;
                case PlayerScreen.ImageFilterType.Edges:
                    this.Text = "   " + m_ResourceManager.GetString("mnuEdges", Thread.CurrentThread.CurrentUICulture);
                    lblValue.Text = m_ResourceManager.GetString("FormFilterEdges_lblValue_Text", Thread.CurrentThread.CurrentUICulture);
                    
                    // Cas particulier : Pas de paramètre.
                    trkValue.Enabled    = false;
                    txtbxValue.Enabled  = false;
                    lblValue.Visible    = false;
                    txtbxValue.Visible  = false;
                    trkValue.Visible    = false;
                    this.Height = 358;
                    
                    break;
                default:
                    break;
            }

            btnOK.Text = m_ResourceManager.GetString("Generic_Apply", Thread.CurrentThread.CurrentUICulture);
            btnCancel.Text = m_ResourceManager.GetString("Generic_Cancel", Thread.CurrentThread.CurrentUICulture);


            // Valeurs par défaut
            m_FilterParams = new FilterParams();

            txtbxValue.Text = "0";
            trkValue.Value = 0;

            // copie de l'image en cours.
            m_bmpOriginal = new Bitmap(_PlayerServer.m_BmpImage);
        }

        private void formFilterTuner_Load(object sender, EventArgs e)
        {
            RatioStretch();
            UpdatePreview();
        }

        private void picPreview_Paint(object sender, PaintEventArgs e)
        {
            // Afficher l'image.
            if (m_bmpPreview != null)
            {
                e.Graphics.DrawImage(m_bmpPreview, 0, 0, picPreview.Width, picPreview.Height);
            }     
        }

        private void RatioStretch()
        {
            // Agrandi la picturebox pour maximisation dans le panel.
            if (m_bmpOriginal != null)
            {
                float WidthRatio = (float)m_bmpOriginal.Width / pnlPreview.Width;
                float HeightRatio = (float)m_bmpOriginal.Height / pnlPreview.Height;

                //Redimensionner l'image selon la dimension la plus proche de la taille du panel.
                if (WidthRatio > HeightRatio)
                {
                    picPreview.Width = pnlPreview.Width;
                    picPreview.Height = (int)((float)m_bmpOriginal.Height / WidthRatio);
                }
                else
                {
                    picPreview.Width = (int)((float)m_bmpOriginal.Width / HeightRatio);
                    picPreview.Height = pnlPreview.Height;
                }

                //recentrer
                picPreview.Left = (pnlPreview.Width / 2) - (picPreview.Width / 2);
                picPreview.Top = (pnlPreview.Height / 2) - (picPreview.Height / 2);
            }
        }
        private void UpdatePreview()
        {
            if (m_bmpPreview != null)
            {
                m_bmpPreview.Dispose();
                m_bmpPreview = null;
            }
            m_bmpPreview = new Bitmap(m_bmpOriginal);
            
            // Passe la copie de l'image dans le filtre.
            switch (m_ImageFilterType)
            {
                case PlayerScreen.ImageFilterType.Colors:
                    m_bmpPreview = m_PlayerServer.m_ImageFilter.DoFilterColors(m_bmpPreview, m_FilterParams);
                    break;
                case PlayerScreen.ImageFilterType.Brightness:
                    m_bmpPreview = m_PlayerServer.m_ImageFilter.DoFilterBrightness(m_bmpPreview, m_FilterParams);
                    break;
                case PlayerScreen.ImageFilterType.Contrast:
                    m_bmpPreview = m_PlayerServer.m_ImageFilter.DoFilterContrast(m_bmpPreview, m_FilterParams);
                    break;
                case PlayerScreen.ImageFilterType.Sharpen:
                    m_bmpPreview = m_PlayerServer.m_ImageFilter.DoFilterSharpen(m_bmpPreview, m_FilterParams);
                    break;
                case PlayerScreen.ImageFilterType.Edges:
                    m_bmpPreview = m_PlayerServer.m_ImageFilter.DoFilterEdges(m_bmpPreview, m_FilterParams);
                    break;
                default:
                    break;
            }

            // Refresh l'image
            picPreview.Invalidate();
        }


        private void trkValue_ValueChanged(object sender, EventArgs e)
        {
            m_FilterParams.iValue = trkValue.Value;
            UpdatePreview();
            
            txtbxValue.Text = trkValue.Value.ToString();
        }
        private void txtbxValue_TextChanged(object sender, EventArgs e)
        {
                try
                {
                    trkValue.Value = int.Parse(txtbxValue.Text);
                }
                catch (Exception)
                {
                }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            //-------------------------------------------------------------
            // renvoyer l'état final du filtre à la commande.
            // elle se chargera d'appliquer le filtre sur les images.
            // ¤ La propriété DialogResult du bouton est a DialogResult.OK
            //-------------------------------------------------------------
            if (DelegateReportFilterParams != null)
            {
                DelegateReportFilterParams(m_FilterParams);
            }
            if (m_bmpPreview != null)
            {
                m_bmpPreview.Dispose();
                m_bmpPreview = null;
            }
            if (m_bmpOriginal != null)
            {
                m_bmpOriginal.Dispose();
                m_bmpOriginal = null;
            }

            Hide();
            Close();
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (m_bmpPreview != null)
            {
                m_bmpPreview.Dispose();
                m_bmpPreview = null;
            }
            if (m_bmpOriginal != null)
            {
                m_bmpOriginal.Dispose();
                m_bmpOriginal = null;
            }

            Hide();
            Close();
        }
    }
}