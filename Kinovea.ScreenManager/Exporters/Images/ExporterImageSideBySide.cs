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
    public static class ExporterImageSideBySide
    {
        public static void Save(PlayerScreen leftPlayer, PlayerScreen rightPlayer, bool merging)
        {
            string filename = GetFilename(leftPlayer, rightPlayer);
            if (string.IsNullOrEmpty(filename))
                return;

            Bitmap composite;

            Bitmap leftImage = leftPlayer.GetFlushedImage();

            if (!merging)
            {
                Bitmap rightImage = rightPlayer.GetFlushedImage();
                composite = ImageHelper.GetSideBySideComposite(leftImage, rightImage, false, true);
                rightImage.Dispose();
            }
            else
            {
                composite = leftImage;
            }
            
            ImageHelper.Save(filename, composite);

            composite.Dispose();
            
            NotificationCenter.RaiseRefreshFileExplorer(null, false);
        }

        private static string GetFilename(PlayerScreen leftPlayer, PlayerScreen rightPlayer)
        {
            SaveFileDialog dlgSave = new SaveFileDialog();
            dlgSave.Title = ScreenManagerLang.Generic_SaveImage;
            dlgSave.RestoreDirectory = true;
            dlgSave.Filter = FilesystemHelper.SaveImageFilter();
            dlgSave.FilterIndex = FilesystemHelper.GetFilterIndex(dlgSave.Filter, PreferencesManager.PlayerPreferences.ImageFormat);
            dlgSave.FileName = SuggestFilename(leftPlayer, rightPlayer);

            if (dlgSave.ShowDialog() != DialogResult.OK)
                return null;

            return dlgSave.FileName;
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
