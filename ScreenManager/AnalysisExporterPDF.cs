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
using System.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using System.Drawing.Imaging;
using Videa.Services;
using System.IO;
using System.Windows.Forms;
using System.Resources;
using System.Reflection;
using System.Threading;


namespace Videa.ScreenManager
{
    public class AnalysisExporterPDF
    {
        #region Members
        private PdfDocument document;
        private int imageTop;
        private int imageLeft;
        private int imageHeight;
        private int imageWidth;
        private double fStretchFactor;
        private Metadata m_MetaData;
        #endregion

        #region Constructor
        public AnalysisExporterPDF()
        {
            //-----------------------------------------------------------------
            // Pourquoi on ne force pas le branding ?
            // 1. L'utilisateur doit pouvoir garder le contrôle du contenu.
            // 2. Cela s'apparente a un watermarking, -> perçut négativement.
            // 3. L'utilisateur doit pouvoir dire "C'est moi qui l'ai fait" et que le rôle exact de Kinovea reste flou... ;-)
            //  => Principe d'utilisabilité : Enable users. Catalyser leur ingéniosité.
            //
            // Voir NSIS, qui n'impose pas son branding dans les installeurs.
            // Même si non supprimé, cela doit rester TRES discret et sobre.
            // Pas sous la forme d'une url complète => spam reminiscent.
            //-----------------------------------------------------------------

            document = new PdfDocument();

            // Default position for image.
            imageTop    = 200;
            imageLeft   = 100;
            imageWidth  = 400;
            imageHeight = 300;
            fStretchFactor = 1.0;
        }
        #endregion

        #region Public Interface
        public void Export(String _filePath, Metadata _metaData)
        {
            m_MetaData = _metaData;

            //--------------------
            // Creates a PDF file.
            //--------------------
            _metaData.GlobalTitle = Path.GetFileNameWithoutExtension(_metaData.FullPath);

            // TODO: Make the export information configurable by the user.
            // => Page layout, Global Title, Author.
            // => Keywords, Subject, branding.

            document.Info.Title = _metaData.GlobalTitle;   // Defaults to the filename.
            document.Info.Subject = "Video Analysis, Exported from Kinovea";
            //document.Info.Author = "John Doe";
            //document.Info.Keywords = "Hauteur, Frances Elites, Indoor, Cianci, 2.10m";
            document.Info.Creator = "Kinovea " + PreferencesManager.ReleaseVersion + " (www.kinovea.org)";

            #region Other possible infos :
            //document.Info.CreationDate        => AUTO
            //document.Info.Producer            => PDFsharp 1.0.898 (www.pdfsharp.com) - READONLY    
            //document.Info.Elements            => ?
            //document.Info.ModificationDate    => ?
            //document.Info.Owner               => ?
            //document.Info.Stream              => ?
            #endregion

            // Create and fill pages.
            for (int iKeyf = 0; iKeyf < _metaData.Count; iKeyf++)
            {
                if(!_metaData[iKeyf].Disabled)
                {
                    PdfPage page = document.AddPage();
                    XGraphics gfx = XGraphics.FromPdfPage(page);

                    // Contents
                    DrawMainTitle(gfx, _metaData.GlobalTitle);
                    DrawImage(gfx, _metaData[iKeyf].FullFrame);
                    DrawAnnotations(gfx, _metaData[iKeyf]);
                    DrawImageSubtitle(gfx, iKeyf+1, _metaData[iKeyf].Title);
                    DrawTitle(gfx, _metaData[iKeyf].Title);
                    DrawComments(gfx, _metaData[iKeyf].Comments);
                    DrawFooter(page, gfx, "kinovea.org");

                    // Add the page to the bookmarks index.
                    document.Outlines.Add(_metaData[iKeyf].Title, page, true);
                }
            }

            // Save document.
            try
            {
                document.Save(_filePath);
            }
            catch(IOException)
            {
                // Most probable cause : the pdf is already openned.

                ResourceManager rm = new ResourceManager("Videa.ScreenManager.Languages.ScreenManagerLang", Assembly.GetExecutingAssembly());
                MessageBox.Show(rm.GetString("Error_SavePdf_IOException", Thread.CurrentThread.CurrentUICulture),
                                            rm.GetString("Error_SavePdf", Thread.CurrentThread.CurrentUICulture),
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Exclamation);
            }
        }
        #endregion

        #region High Level Helpers
        private void DrawMainTitle(XGraphics gfx, string title)
        {
            // Main title on top of the page.

            if (title != null)
            {
                XRect rect = new XRect(XPoint.Empty, gfx.PageSize);
                rect.Inflate(-10, -15);

                XFont font = new XFont("Verdana", 10, XFontStyle.Bold);
                gfx.DrawString(title, font, XBrushes.MidnightBlue, rect, XStringFormat.TopCenter);
            }
        }
        private void DrawImage(XGraphics gfx, Image img)
        {
            // Draw the picture.

            XRect rect = new XRect(XPoint.Empty, gfx.PageSize);
            rect.Inflate(-10, -15);

            XImage image = XImage.FromGdiPlusImage(img);

            // Scale the image.
            int iMaxWidth = 400;
            int iMaxHeight = 300;

            float fWidthRatio = (float)image.Width / iMaxWidth;
            float fHeightRatio = (float)image.Height / iMaxHeight;

            if (fWidthRatio > fHeightRatio)
            {
                imageWidth = iMaxWidth;
                imageHeight = (int)((float)image.Height / fWidthRatio);
                fStretchFactor = (1.0 / fWidthRatio);
            }
            else
            {
                imageWidth = (int)((float)image.Width / fHeightRatio);
                imageHeight = iMaxHeight;
                fStretchFactor = (1.0 / fHeightRatio);
            }

            imageLeft = (int)((rect.Width / 2) - (imageWidth / 2));
            imageTop = (int)((rect.Height / 3) - (imageHeight / 2));
            
            // Draw scaled image
            gfx.DrawImage(image, imageLeft, imageTop, imageWidth, imageHeight);
        }
        private void DrawAnnotations(XGraphics gfx, Keyframe _keyframe)
        {
            // Overlay the drawings onto the picture.
            foreach (AbstractDrawing ad in _keyframe.Drawings)
            {
                ad.DrawOnPDF(gfx, imageLeft, imageTop, imageWidth, imageHeight, fStretchFactor);
            }

            foreach (DrawingChrono dc in m_MetaData.Chronos)
            {
                dc.DrawOnPDF(gfx, imageLeft, imageTop, imageWidth, imageHeight, fStretchFactor, _keyframe.Position);
            }

        }
        private void DrawImageSubtitle(XGraphics gfx, int number, string title)
        {
            // This goes just under the image as a note.
            // May not be suitable for all page layouts...

            XRect rect = new XRect(XPoint.Empty, gfx.PageSize);
            rect.Inflate(-10, -15);
            rect.Offset(imageLeft + imageWidth/4, imageTop + imageHeight - 10);

            XFont font = new XFont("Verdana", 10);
            XStringFormat format = new XStringFormat();

            format.LineAlignment = XLineAlignment.Near;
            format.Alignment = XStringAlignment.Near;

            string txt = number.ToString();
            txt = txt + ". " + title;

            gfx.DrawString(txt, font, XBrushes.Black, rect, format);   
        }
        private void DrawTitle(XGraphics gfx, string title)
        {
            // this is the title of the image as defined in the comments of the KeyFrame.

            XRect rect = new XRect(XPoint.Empty, gfx.PageSize);
            rect.Inflate(-10, -15);
            rect.Offset(imageLeft, imageTop + imageHeight + 30);

            XFont font = new XFont("Verdana", 14, XFontStyle.Underline);
            XStringFormat format = new XStringFormat();

            format.LineAlignment = XLineAlignment.Near;
            format.Alignment = XStringAlignment.Near;

            gfx.DrawString(title, font, XBrushes.Black, rect, format);
        }
        private void DrawComments(XGraphics gfx, List<string> comments)
        {
            // The KeyImage Comments.

            XRect rect = new XRect(XPoint.Empty, gfx.PageSize);
            rect.Inflate(-10, -15);
            rect.Offset(imageLeft, imageTop + imageHeight + 60);

            XFont font = new XFont("Verdana", 10, XFontStyle.Regular);
            XStringFormat format = new XStringFormat();

            format.LineAlignment = XLineAlignment.Near;
            format.Alignment = XStringAlignment.Near;

            // Comments are stored as a list of lines. 
            int iInterline = 18;
            for (int i = 0; i < comments.Count; i++)
            {
                //gfx.DrawString("Lorem ipsum dolor sit amet, consectetuer adipiscing elit.", font, XBrushes.Black, rect, format);
                gfx.DrawString(comments[i], font, XBrushes.Black, rect, format);
                rect.Offset(0, iInterline);
            }
        }
        private void DrawFooter(PdfPage page, XGraphics gfx, string brand)
        {
            // Branding and page number for each page. 
  
            XRect rect = new XRect(XPoint.Empty, gfx.PageSize);
            rect.Inflate(-10, -15);
            rect.Offset(0, 5);

            XFont font = new XFont("Verdana", 8); // , XFontStyle.Italic
            XStringFormat format = new XStringFormat();

            // Align to : bottom.
            format.LineAlignment = XLineAlignment.Far;

            // Branding.
            format.Alignment = XStringAlignment.Far;
            gfx.DrawString(brand, font, XBrushes.Gray, rect, format);

            // Page Number
            format.Alignment = XStringAlignment.Center;
            gfx.DrawString(document.PageCount.ToString(), font, XBrushes.Gray, rect, format);
        }        
        #endregion
    }
}
