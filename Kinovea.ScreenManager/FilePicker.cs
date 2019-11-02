using Kinovea.ScreenManager.Languages;
using Kinovea.Services;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    /// <summary>
    ///  Helper function to launch open file dialogs for video, replay watchers, annotations.
    /// </summary>
    public static class FilePicker
    {
        public static string OpenVideo()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = ScreenManagerLang.mnuOpenVideo;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Filter = ScreenManagerLang.dlgOpenFile_Filter;
            openFileDialog.FilterIndex = 1;
            DialogResult result = openFileDialog.ShowDialog();
            if (result != DialogResult.OK || string.IsNullOrEmpty(openFileDialog.FileName) || !File.Exists(openFileDialog.FileName))
                return null;

            return openFileDialog.FileName;
        }

        public static string OpenReplayWatcher()
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            string lastReplayFolder = PreferencesManager.FileExplorerPreferences.LastReplayFolder;
            if (!string.IsNullOrEmpty(lastReplayFolder))
            {
                lastReplayFolder = Path.GetDirectoryName(lastReplayFolder);
                if (Directory.Exists(lastReplayFolder))
                    dialog.InitialDirectory = lastReplayFolder;
            }
            else
            {
                dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }

            dialog.IsFolderPicker = true;
            string path = null;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                path = dialog.FileName;

            if (path == null || !Directory.Exists(path))
                return null;

            path = Path.Combine(path, "*");
            return path;
        }

        public static string OpenAnnotations()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = ScreenManagerLang.dlgLoadAnalysis_Title;
            openFileDialog.RestoreDirectory = true;
            //openFileDialog.Filter = ScreenManagerLang.FileFilter_KVA;
            string filterAllAnalysis = "All supported formats (*.kva, *.srt, *.json, *.xml)|*.kva;*.srt;*.json;*.xml";
            string filterKVA = "Kinovea Video Analysis (*.kva)|*.kva";
            string filterSRT = "Subtitles (*.srt)|*.srt";
            string filterOpenPose = "OpenPose (*.json)|*.json";
            string totalFilter = string.Join("|", new string[] { filterAllAnalysis, filterKVA, filterSRT, filterOpenPose });
            openFileDialog.Filter = totalFilter;
            openFileDialog.FilterIndex = 1;

            DialogResult result = openFileDialog.ShowDialog();
            if (result != DialogResult.OK || string.IsNullOrEmpty(openFileDialog.FileName) || !File.Exists(openFileDialog.FileName))
                return null;

            return openFileDialog.FileName;
        }
    }
}
