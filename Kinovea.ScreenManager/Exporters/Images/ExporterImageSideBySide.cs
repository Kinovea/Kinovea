using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using Kinovea.ScreenManager.Languages;
using System.IO;
using Kinovea.Services;
using System.Drawing.Imaging;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Exporter for side by side images. Either horizontal or vertical layout.
    /// </summary>
    public class ExporterImageSideBySide
    {
        public void Export(string filePath, bool horizontal, PlayerScreen leftPlayer, PlayerScreen rightPlayer)
        {
            bool isVideo = false;

            // Stop playing and disable custom decoding size.
            leftPlayer.view.BeforeExportVideo();
            rightPlayer.view.BeforeExportVideo();

            Size sizeLeft = leftPlayer.FrameServer.VideoReader.Info.ReferenceSize;
            Size sizeRight = rightPlayer.FrameServer.VideoReader.Info.ReferenceSize;
            Size sizeComp = ImageHelper.GetSideBySideCompositeSize(sizeLeft, sizeRight, isVideo, false, horizontal);

            var format = PixelFormat.Format24bppRgb;
            Bitmap bmpLeft = new Bitmap(sizeLeft.Width, sizeLeft.Height, format);
            Bitmap bmpRight = new Bitmap(sizeRight.Width, sizeRight.Height, format);
            Bitmap bmpComposite = new Bitmap(sizeComp.Width, sizeComp.Height, format);

            leftPlayer.PaintFlushedImage(bmpLeft);
            rightPlayer.PaintFlushedImage(bmpRight);

            ImageHelper.PaintSideBySideComposite(bmpComposite, bmpLeft, bmpRight, horizontal);
            ImageHelper.Save(filePath, bmpComposite);

            PreferencesManager.PlayerPreferences.ImageFormat = FilesystemHelper.GetImageFormat(filePath);

            bmpLeft.Dispose();
            bmpRight.Dispose();
            bmpComposite.Dispose();

            leftPlayer.view.AfterExportVideo();
            rightPlayer.view.AfterExportVideo();

            NotificationCenter.RaiseRefreshFileList(false);
        }

        public static string SuggestFilename(PlayerScreen player1, PlayerScreen player2)
        {
            if (player1 == null || player2 == null || !player1.Full || !player2.Full)
                return null;

            return String.Format("{0} - {1}", 
                Path.GetFileNameWithoutExtension(player1.FilePath), 
                Path.GetFileNameWithoutExtension(player2.FilePath));
        }
    }
}
