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

        public void Export(string file, PlayerScreen player)
        {
            Bitmap outputImage = player.view.GetFlushedImage();
            ImageHelper.Save(file, outputImage);
            outputImage.Dispose();

            PreferencesManager.PlayerPreferences.ImageFormat = FilesystemHelper.GetImageFormat(file);
            PreferencesManager.Save();

            player.FrameServer.AfterSave();
        }
    }
}
