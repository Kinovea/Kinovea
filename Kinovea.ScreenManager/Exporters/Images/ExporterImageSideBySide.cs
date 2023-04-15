using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using Kinovea.ScreenManager.Languages;
using System.IO;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public class ExporterImageSideBySide
    {
    
        public void Export(string filePath, bool horizontal, PlayerScreen leftPlayer, PlayerScreen rightPlayer)
        {
            Bitmap leftImage = leftPlayer.GetFlushedImage();
            Bitmap rightImage = rightPlayer.GetFlushedImage();

            bool isVideo = false;
            Bitmap composite = ImageHelper.GetSideBySideComposite(leftImage, rightImage, isVideo, horizontal);

            leftImage.Dispose();
            rightImage.Dispose();
            
            ImageHelper.Save(filePath, composite);

            composite.Dispose();
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
