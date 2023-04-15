using Kinovea.ScreenManager.Languages;
using Kinovea.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Exporter for a single image.
    /// </summary>
    public class ExporterImage
    {
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void Export(PlayerScreen player)
        {
            try
            {
                SaveFileDialog dlgSave = new SaveFileDialog();
                dlgSave.Title = ScreenManagerLang.Generic_SaveImage;
                dlgSave.RestoreDirectory = true;
                dlgSave.Filter = FilesystemHelper.SaveImageFilter();
                dlgSave.FilterIndex = FilesystemHelper.GetFilterIndex(dlgSave.Filter, PreferencesManager.PlayerPreferences.ImageFormat);
                dlgSave.FileName = player.FrameServer.GetImageFilename(player.FrameServer.VideoReader.FilePath, player.view.CurrentTimestamp, PreferencesManager.PlayerPreferences.TimecodeFormat);

                if (dlgSave.ShowDialog() != DialogResult.OK)
                    return;

                Bitmap outputImage = player.view.GetFlushedImage();
                ImageHelper.Save(dlgSave.FileName, outputImage);
                outputImage.Dispose();

                PreferencesManager.PlayerPreferences.ImageFormat = FilesystemHelper.GetImageFormat(dlgSave.FileName);
                PreferencesManager.Save();

                player.FrameServer.AfterSave();
            }
            catch (Exception exp)
            {
                log.Error(exp.StackTrace);
            }
        }

    }
}
