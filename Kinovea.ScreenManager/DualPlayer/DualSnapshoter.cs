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
    public static class DualSnapshoter
    {
        public static void Save(PlayerScreen leftPlayer, PlayerScreen rightPlayer)
        {
            Bitmap leftImage = leftPlayer.GetFlushedImage();
            Bitmap rightImage = rightPlayer.GetFlushedImage();
            Bitmap composite = ImageHelper.GetSideBySideComposite(leftImage, rightImage, false, true);

            SaveFileDialog dlgSave = new SaveFileDialog();
            dlgSave.Title = ScreenManagerLang.Generic_SaveImage;
            dlgSave.RestoreDirectory = true;
            dlgSave.Filter = ScreenManagerLang.dlgSaveFilter;
            dlgSave.FilterIndex = 1;
            dlgSave.FileName = String.Format("{0} - {1}", Path.GetFileNameWithoutExtension(leftPlayer.FilePath), Path.GetFileNameWithoutExtension(rightPlayer.FilePath));

            if (dlgSave.ShowDialog() == DialogResult.OK)
                ImageHelper.Save(dlgSave.FileName, composite);

            composite.Dispose();
            leftImage.Dispose();
            rightImage.Dispose();

            NotificationCenter.RaiseRefreshFileExplorer(null, false);
        }
    }
}
