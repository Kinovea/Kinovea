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
using System.Drawing;
using System.Windows.Forms;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public abstract class AbstractScreen
    {
        #region Events
        public event EventHandler Activated;
        protected virtual void OnActivated(EventArgs e)
        {
            RaiseEvent(Activated, e);
        }
        
        public event EventHandler CloseAsked;
        protected virtual void OnCloseAsked(EventArgs e)
        {
            RaiseEvent(CloseAsked, e);
        }

        public event EventHandler<EventArgs<HotkeyCommand>> DualCommandReceived;
        protected virtual void OnDualCommandReceived(EventArgs<HotkeyCommand> e)
        {
            RaiseEvent(DualCommandReceived, e);
        }

        private void RaiseEvent(EventHandler invoker, EventArgs e)
        {
            if(invoker != null)
                invoker(this, e);
        }

        private void RaiseEvent<T>(EventHandler<T> invoker, T e) where T : EventArgs
        {
            if (invoker != null)
                invoker(this, e);
        }
        #endregion

        #region Properties

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
        public abstract IScreenDescription GetScreenDescription();
        #endregion

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Trigger the save or save as dialog for the screen's metadata.
        /// </summary>
        public void SaveKVA()
        {
            if (this.Metadata == null)
            {
                log.Error("Screen with no metadata.");
                return;
            }

            MetadataSerializer serializer = new MetadataSerializer();
            serializer.UserSave(this.Metadata, this.FilePath);
        }

        /// <summary>
        /// Trigger the save as dialog for the screen's metadata.
        /// </summary>
        public void SaveKVAAs()
        {
            if (this.Metadata == null)
            {
                log.Error("Screen with no metadata.");
                return;
            }

            MetadataSerializer serializer = new MetadataSerializer();
            serializer.UserSaveAs(this.Metadata, this.FilePath);
        }

        public void UnloadAnnotations()
        {
            if (this.Metadata == null)
            {
                log.Error("Screen with no metadata.");
                return;
            }

            this.Metadata.Unload();
        }

    }   
}
