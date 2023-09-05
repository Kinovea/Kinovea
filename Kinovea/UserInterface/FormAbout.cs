/*
Copyright © Joan Charmant 2008.
jcharmant@gmail.com 
 
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

using Kinovea.Root.Languages;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using Kinovea.Services;
using System.Text;

namespace Kinovea.Root
{
    public partial class FormAbout : Form
    {
        private string year = "2023";
        private Font fontHeader = new Font("Microsoft Sans Serif", 10, FontStyle.Bold);
        private Font fontText = new Font("Microsoft Sans Serif", 9, FontStyle.Regular);

        public FormAbout()
        {
            InitializeComponent();
            Populate();
        }

        private void Populate()
        {
            this.Text = "   " + RootLang.mnuAbout;
            labelCopyright.Text = string.Format("Copyright © 2006-{0} - Joan Charmant and contributors.", year);
            lblKinovea.Text = string.Format("{0} - {1}", Software.ApplicationName, Software.Version);
            lnkKinovea.Links.Clear();
            lnkKinovea.Links.Add(0, lnkKinovea.Text.Length, "https://www.kinovea.org");

            PopulateTranslators();
            PopulateLicense();
            PopulateBuildingBlocks();
            PopulateCitation();
        }

        private void PopulateTranslators()
        {
            pageTranslation.Text = RootLang.dlgAbout_Translation;
            
            rtbTranslators.SelectionFont = fontText;
            rtbTranslators.AppendText(Properties.Resources.translators);
        }

        private void PopulateLicense()
        {
            pageLicense.Text = RootLang.dlgAbout_License;
        }

        private void PopulateBuildingBlocks()
        {
            pageBuildingBlocks.Text = RootLang.dlgAbout_BuildingBlocks;
         
            rtbBuildingBlocks.AppendText(" FFmpeg - https://www.ffmpeg.org/\n");
            rtbBuildingBlocks.AppendText(" OpenCV - http://opencv.org/.\n");
            rtbBuildingBlocks.AppendText(" AForge - http://www.aforgenet.com/\n");
            rtbBuildingBlocks.AppendText(" EmguCV - http://www.emgu.com/\n");
            rtbBuildingBlocks.AppendText(" OxyPlot - https://oxyplot.github.io/\n");
            rtbBuildingBlocks.AppendText(" Sharp Vector Graphics - http://sourceforge.net/projects/svgdomcsharp/\n");
            
            rtbBuildingBlocks.AppendText(" NAudio - https://github.com/naudio/NAudio\n");
            rtbBuildingBlocks.AppendText(" Math.Net Numerics- https://numerics.mathdotnet.com/\n");
            rtbBuildingBlocks.AppendText(" SharpZipLib - https://github.com/icsharpcode/SharpZipLib\n");
            rtbBuildingBlocks.AppendText(" Json.NET - https://www.newtonsoft.com/json\n");
            rtbBuildingBlocks.AppendText(" SpreadsheetLight - https://spreadsheetlight.com/\n");

            rtbBuildingBlocks.AppendText(" ExpTree - http://www.codeproject.com/Articles/8546/\n");
            rtbBuildingBlocks.AppendText(" log4Net - https://logging.apache.org/log4net/\n");
            rtbBuildingBlocks.AppendText(" Silk Icon set - http://www.famfamfam.com/lab/icons/silk/\n");
            rtbBuildingBlocks.AppendText(" Fugue Icon set - http://p.yusukekamiyamane.com/\n");
        }

        private void PopulateCitation()
        {
            pageCitation.Text = RootLang.dlgAbout_Citation;

            //fontHeader = new Font("Microsoft Sans Serif", 10, FontStyle.Bold);
            //fontText = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular);

            rtbCitation.DetectUrls = false;

            //------------------------------------------
            rtbCitation.SelectionFont = fontHeader;
            rtbCitation.AppendText("BibTeX\n\n");
            rtbCitation.SelectionFont = fontText;
            StringBuilder b = new StringBuilder();
            b.AppendLine("@Misc{,");
            b.AppendLine(string.Format("\ttitle = {{Kinovea ({0})}},", Software.Version));
            b.AppendLine("\tauthor = {Charmant, Joan and contributors},");
            b.AppendLine("\turl = {https://www.kinovea.org},");
            b.AppendLine(string.Format("\tyear = {{{0}}},", year));
            b.AppendLine("}");
            rtbCitation.AppendText(b.ToString());

            rtbCitation.AppendText("\n\n\n");
            //------------------------------------------
            rtbCitation.SelectionFont = fontHeader;
            rtbCitation.AppendText("APA\n\n");
            rtbCitation.SelectionFont = fontText;
            rtbCitation.AppendText(string.Format("Charmant, J., & contributors. ({0}) Kinovea (Version {1}) [Computer software]. https://www.kinovea.org\n", year, Software.Version));
            rtbCitation.AppendText("\n");
        }

        private void lnkKinovea_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(e.Link.LinkData.ToString());
        }

        private void rtbBuildingBlocks_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            Process.Start(e.LinkText);
        }
    }
}