using Kinovea.ScreenManager.Languages;
using Kinovea.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
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
            player.view.BeforeExportVideo();
            Size size = player.FrameServer.VideoReader.Info.ReferenceSize;
            Bitmap bmp = new Bitmap(size.Width, size.Height, PixelFormat.Format24bppRgb);

            player.view.PaintFlushedImage(bmp);
            ImageHelper.Save(file, bmp);

            bmp.Dispose();

            PreferencesManager.PlayerPreferences.ImageFormat = FilesystemHelper.GetImageFormat(file);

            player.view.AfterExportVideo();

            NotificationCenter.RaiseRefreshFileExplorer(this, false);
        }
    }
}
