using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using Kinovea.Video;
using Kinovea.Services;
using Kinovea.ScreenManager.Languages;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// This is the router to specialized exporters for the different image export formats.
    /// The "formats" at this level are the different types of image export we support (single, sequence, etc.),
    /// not the final file format. All exporters should be able to export to all the supported file formats,
    /// which will be chosen in the save file dialog.
    /// </summary>
    public class ImageExporter
    {
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // Get a filename from the user and export an image or image sequence.
        public void Export(ImageExportFormat format, PlayerScreen player1, PlayerScreen player2)
        {
            if (player1 == null)
                return;

            // Special case for video filter with custom save mechanics.
            if (format == ImageExportFormat.Image && 
                player1.ActiveVideoFilterType != VideoFilterType.None &&
                player1.FrameServer.Metadata.ActiveVideoFilter.CanExportImage)
            {
                player1.FrameServer.Metadata.ActiveVideoFilter.ExportImage(player1.view);
                return;
            }

            // Immediately get a file name to save to.
            // Any configuration of the save happens later.
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = ScreenManagerLang.Generic_SaveImage;
            sfd.RestoreDirectory = true;
            sfd.Filter = FilesystemHelper.SaveImageFilter();
            sfd.FilterIndex = FilesystemHelper.GetFilterIndex(sfd.Filter, PreferencesManager.PlayerPreferences.ImageFormat);
            sfd.FileName = SuggestFilename(format, player1, player2);
            
            if (sfd.ShowDialog() != DialogResult.OK || string.IsNullOrEmpty(sfd.FileName))
                return;

            // Save the new preferred image format.
            PreferencesManager.PlayerPreferences.ImageFormat = FilesystemHelper.GetImageFormat(sfd.FileName);
            
            SavingSettings s = new SavingSettings();

            try
            {
                switch (format)
                {
                    case ImageExportFormat.Image:
                        ExporterImage exporterImage = new ExporterImage();
                        exporterImage.Export(sfd.FileName, player1);
                        player1.FrameServer.AfterSave();
                        break;

                    case ImageExportFormat.SideBySide:

                        // Show a configuration dialog to get the layout.
                        FormConfigureExportImageSideBySide fceisbs = new FormConfigureExportImageSideBySide();
                        fceisbs.StartPosition = FormStartPosition.CenterScreen;
                        if (fceisbs.ShowDialog() != DialogResult.OK)
                        {
                            fceisbs.Dispose();
                            return;
                        }

                        bool horizontal = fceisbs.Horizontal;
                        fceisbs.Dispose();

                        // Save this as the new preferred layout.
                        PreferencesManager.PlayerPreferences.SideBySideHorizontal = horizontal;

                        // Export
                        ExporterImageSideBySide exporterSidebySide = new ExporterImageSideBySide();
                        exporterSidebySide.Export(sfd.FileName, horizontal, player1, player2);
                        
                        player1.FrameServer.AfterSave();
                        player2.FrameServer.AfterSave();
                        break;

                    case ImageExportFormat.ImageSequence:
                        
                        // Show a configuration dialog to get the interval.
                        FormConfigureExportImageSequence fceis = new FormConfigureExportImageSequence(player1);
                        fceis.StartPosition = FormStartPosition.CenterScreen;
                        if (fceis.ShowDialog() != DialogResult.OK)
                        {
                            fceis.Dispose();
                            return;
                        }

                        s.Section = new VideoSection(player1.FrameServer.Metadata.SelectionStart, player1.FrameServer.Metadata.SelectionEnd);
                        s.KeyframesOnly = false;
                        s.File = sfd.FileName;
                        s.ImageRetriever = player1.view.GetFlushedImage;
                        s.InputIntervalTimestamps = fceis.IntervalTimestamps;
                        s.TotalFrameCount = fceis.RemainingFrames;

                        fceis.Dispose();

                        ExporterImageSequence exporterImageSequence = new ExporterImageSequence();
                        exporterImageSequence.Export(s, player1);
                        break;

                    case ImageExportFormat.KeyImages:
                        
                        // No dialog needed.
                        
                        s.Section = new VideoSection(player1.FrameServer.Metadata.SelectionStart, player1.FrameServer.Metadata.SelectionEnd);
                        s.KeyframesOnly = true;
                        s.File = sfd.FileName;
                        s.ImageRetriever = player1.view.GetFlushedImage;
                        s.TotalFrameCount = player1.FrameServer.Metadata.Keyframes.Count;

                        ExporterImageSequence exporterKeyImages = new ExporterImageSequence();
                        exporterKeyImages.Export(s, player1);
                        break;
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("Exception encountered while exporting images.", e);
            }
        }

        /// <summary>
        /// Returns a suggested filename (without directory nor extension), to be used in the save file dialog.
        /// </summary>
        private string SuggestFilename(ImageExportFormat format, PlayerScreen player1, PlayerScreen player2)
        {
            string filename = "";
            string videoFilePath = player1.FrameServer.VideoReader.FilePath;
            switch (format)
            {
                case ImageExportFormat.Image:
                    // Video name + time code.
                    filename = player1.FrameServer.GetImageFilename(videoFilePath, player1.view.CurrentTimestamp, PreferencesManager.PlayerPreferences.TimecodeFormat);
                    break;
                case ImageExportFormat.ImageSequence:
                case ImageExportFormat.KeyImages:
                    // Video name alone.
                    filename = Path.GetFileNameWithoutExtension(videoFilePath);
                    break;
                case ImageExportFormat.SideBySide:
                    // Double video name.
                    filename = ExporterImageSideBySide.SuggestFilename(player1, player2);
                    break;
            }

            return filename;
        }
    }
}
