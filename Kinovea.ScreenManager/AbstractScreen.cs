/*
Copyright © Joan Charmant 2008.
jcharmant@gmail.com 
 
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
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Kinovea.ScreenManager.Languages;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public abstract class AbstractScreen
    {
        #region Events
        public event EventHandler Activated;
        public event EventHandler LoadAnnotationsAsked;
        public event EventHandler<EventArgs<HotkeyCommand>> DualCommandReceived;
        public event EventHandler CloseAsked;

        protected virtual void RaiseActivated(EventArgs e)
        {
            Activated?.Invoke(this, e);
        }

        protected virtual void RaiseLoadAnnotationsAsked(EventArgs e)
        {
            LoadAnnotationsAsked?.Invoke(this, e);
        }

        protected virtual void RaiseDualCommandReceived(EventArgs<HotkeyCommand> e)
        {
            DualCommandReceived?.Invoke(this, e);
        }
        protected virtual void RaiseCloseAsked(EventArgs e)
        {
            CloseAsked?.Invoke(this, e);
        }
        #endregion

        #region Abstract properties

        /// <summary>
        /// Screen ID.
        /// </summary>
        public abstract Guid Id
        {
            get;
            set;
        }

        /// <summary>
        /// Returns the screen view user control.
        /// </summary>
        public abstract UserControl UI
        {
            get;
        }

        /// <summary>
        /// Returns whether the screen is loaded with a video or camera.
        /// </summary>
        public abstract bool Full
        {
        	get;
        }
        

        /// <summary>
        /// Returns the metadata associated with the screen.
        /// </summary>
        public abstract Metadata Metadata
        {
            get;
        }

        /// <summary>
        /// Returns the file name component of the video file for loaded players 
        /// or an empty string for capture screens and empty players. 
        /// </summary>
        public abstract string FileName
        {
        	get;
        }

        /// <summary>
        /// Returns the full path to the video file for loaded players
        /// or an empty string for capture screens and empty players.
        /// For replay watchers, returns the current file, not the directory.
        /// </summary>
        public abstract string FilePath
        {
        	get;
        }

        /// <summary>
        /// Returns a string suitable for display in the status bar.
        /// </summary>
        public abstract string Status
        {
        	get;
        }


        public abstract bool CapabilityDrawings
        {
        	get;
        }
        public abstract ImageAspectRatio AspectRatio
        {
        	get;
        	set;
        }
        public abstract ImageRotation ImageRotation
        {
            get;
            set;
        }
        public abstract Demosaicing Demosaicing
        {
            get;
            set;
        }
        public abstract bool Mirrored
        {
            get;
            set;
        }

        public abstract bool CoordinateSystemVisible
        {
            get;
            set;
        }

        public abstract bool TestGridVisible
        {
            get;
            set;
        }
        
        /// <summary>
        /// Get the undo/redo history stack for the screen.
        /// </summary>
        public abstract HistoryStack HistoryStack
        {
            get;
        }
        #endregion

        #region Abstract methods
        public abstract void DisplayAsActiveScreen(bool active);
        public abstract void RefreshUICulture();
        public abstract void PreferencesUpdated();
        public abstract void BeforeClose();
        public abstract void AfterClose();
        public abstract void RefreshImage();
        public abstract void AddImageDrawing(string filename, bool isSvg);
        public abstract void AddImageDrawing(Bitmap bmp);
        public abstract void FullScreen(bool fullScreen);
        public abstract void Identify(int index);
        public abstract void ExecuteScreenCommand(int cmd);
        public abstract void LoadKVA(string path);


        /// <summary>
        /// Return a screen descriptor to be used for startup mechanics.
        /// </summary>
        public abstract IScreenDescriptor GetScreenDescriptor();
        #endregion

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Concrete properties

        /// <summary>
        /// Profile containing custom variables and their values.
        /// Set by the screen manager when the screen is created.
        /// </summary>
        public VariablesRepository ProfileManager
        {
            get;
        }
        #endregion

        public AbstractScreen(VariablesRepository profileManager)
        {
            this.ProfileManager = profileManager;
        }

        /// <summary>
        /// Trigger the save or save as dialog for the screen's metadata.
        /// </summary>
        public bool SaveAnnotations()
        {
            if (this.Metadata == null)
                return false;

            MetadataSerializer serializer = new MetadataSerializer();
            string forcedPath = "";
            string defaultPath = this.FilePath;
            bool saved = serializer.UserSave(this.Metadata, forcedPath, defaultPath);
            if (saved)
                AfterLastKVAPathChanged();
            
            return saved;
        }

        /// <summary>
        /// Trigger the save as dialog for the screen's metadata.
        /// </summary>
        public void SaveAnnotationsAs()
        {
            if (this.Metadata == null)
                return;

            MetadataSerializer serializer = new MetadataSerializer();
            serializer.UserSaveAs(this.Metadata, this.FilePath);

            AfterLastKVAPathChanged();
        }

        /// <summary>
        /// Save the current annotations to the path specified in the preferences or to the standard path.
        /// </summary>
        public void SaveDefaultAnnotations(bool forPlayer)
        {
            if (this.Metadata == null)
                return;

            // If there is no default yet, save to the standard location.
            // If there is a default, interpolate the path variables and save to that target.
            // This way the user can save to a custom location corresponding to the active profile.
            // That path is where the default is going to be read from by the screens.
            
            string filename = forPlayer ? "player.kva" : "capture.kva";
            string standardPath = Path.Combine(Software.SettingsDirectory, filename);
            string currentPath = forPlayer ? PreferencesManager.PlayerPreferences.PlaybackKVA : PreferencesManager.CapturePreferences.CaptureKVA;
            
            string targetPath;
            if (string.IsNullOrEmpty(currentPath))
            {
                // The file doesn't exist yet. Save to standard path.
                targetPath = standardPath;
                
                // In that case this becomes the new default path.
                // But we only save the unrooted filename for cleanliness.
                // All loaders must test if the path is rooted.
                if (forPlayer)
                    PreferencesManager.PlayerPreferences.PlaybackKVA = filename;
                else
                    PreferencesManager.CapturePreferences.CaptureKVA = filename;

            }
            else if (currentPath == filename)
            {
                // The path already exists but it's just the standard path in the settings.
                targetPath = standardPath;
            }
            else
            {
                targetPath = Filenamer.ResolveDefaultKVAPath(currentPath, ProfileManager);

                // If the resulting path is not rooted it is a mistake or a variable issue.
                // This might happen if the user puts a random filename in there instead of a full path.
                if (!Path.IsPathRooted(targetPath))
                {
                    log.ErrorFormat("Unrecognized path to default annotations. The path must resolve to an existing directory and start at the root of a drive. {0}", targetPath);
                    return;
                }

                // TODO: if the full path including the file already exists, ask the user for confirmation?
                // Bail out if cancelled.
                
                
                // If path is correctly rooted but parts of it don't exist create them.
                // The file itself doesn't have to exist yet but the directory must exist.
                var directory = Path.GetDirectoryName(targetPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Bail out on failure.
                if (!Directory.Exists(directory))
                {
                    log.ErrorFormat("Could not create directory for default annotations. {0}", directory);
                    return;
                }
            }

            // At this point we always have a target path to save to.
            MetadataSerializer serializer = new MetadataSerializer();
            serializer.UserSave(this.Metadata, targetPath);
            log.DebugFormat("Saved default KVA: {0}", targetPath);

            // Don't let the default kva become the working kva. aka: disable "save".
            // The user needs to either save to default again or save elsewhere.
            // This is to avoid mistakenly overwriting the default file.
            this.Metadata.ResetKVAPath();

            AfterLastKVAPathChanged();
        }

        /// <summary>
        /// Reset the modifiable data but not the data related to the video or camera.
        /// This is used when unloading the metadata to start afresh, in the same video/camera.
        /// </summary>
        public void UnloadAnnotations()
        {
            if (this.Metadata == null)
                return;

            // Since there is no undo of this, ask for confirmation.
            // Even if the user is unloading it's possible they want to keep the data for later.
            // Also a safety against misclicking the unload menu.
            bool confirmed = BeforeUnloadingAnnotations();
            if (!confirmed)
                return;

            this.Metadata.Unload();
            AfterLastKVAPathChanged();
        }

        public void ReloadDefaultAnnotations(bool forPlayer)
        {
            if (this.Metadata == null)
                return;

            string path = "";
            bool found = Filenamer.GetDefaultKVAPath(ref path, ProfileManager, forPlayer);

            if (!found)
                return;

            bool confirmed = BeforeUnloadingAnnotations();
            if (!confirmed)
                return;

            this.Metadata.Unload();
            LoadKVA(path);

            // Never let the default file become the working file.
            this.Metadata.ResetKVAPath();
            AfterLastKVAPathChanged();
        }

        /// <summary>
        /// Check if we can unload the metadata, ask the user to save it if they want.
        /// This happens when replacing or closing a screen, or for unloading annotations.
        /// </summary>
        /// <returns>true if the operation can carry on, false if the operation is cancelled</returns>
        public bool BeforeUnloadingAnnotations()
        {
            if (Metadata == null || !Metadata.IsDirty)
            {
                // No metadata or metadata already saved, we can safely carry on.
                return true;
            }

            DialogResult save = ShowConfirmDirtyDialog();
            if (save == DialogResult.No)
            {
                // No: no need to save, can carry on.
                return true;
            }
            else if (save == DialogResult.Cancel)
            {
                // Cancel: do not save, do not carry on.
                return false;
            }
            else
            {
                // Yes: save first, then we can carry on.
                // The save function may trigger the save as dialog,
                // and that in turn might be cancelled by the user.
                // In that case we cancel everything.
                return SaveAnnotations();
            }
        }
        private DialogResult ShowConfirmDirtyDialog()
        {
            string caption = ScreenManagerLang.InfoBox_MetadataIsDirty_Title;
            string text = ScreenManagerLang.InfoBox_MetadataIsDirty_Text.Replace("\\n", "\n");
            return MessageBox.Show(text, caption, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
        }

        private void AfterLastKVAPathChanged()
        {
            // Make sure the main File menu is up to date.
            RaiseActivated(EventArgs.Empty);
        }

    }   
}
