#region License
/*
Copyright © Joan Charmant 2012.
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
#endregion
using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Kinovea.Services;

namespace Kinovea.Camera
{
    /// <summary>
    /// A camera manager is a bridge with a technology (not with a single camera).
    /// For example, we have a camera manager for DirectShow, another for HTTP connected cameras.
    /// It is responsible for discovering cameras of its type and instanciating a corresponding frame grabber.
    /// </summary>
    public abstract class CameraManager
    {
        /// <summary>
        /// Event raised by Camera managers to report a new thumbnail.
        /// </summary>
        public event EventHandler<CameraThumbnailProducedEventArgs> CameraThumbnailProduced;
        protected virtual void OnCameraThumbnailProduced(CameraThumbnailProducedEventArgs e)
        {
            EventHandler<CameraThumbnailProducedEventArgs> invoker = CameraThumbnailProduced;
            if (invoker != null)
                invoker(this, e);
        }

        private Control dummy = new Control();
        public CameraManager()
        {
            // Show that the main UI thread (calling this ctor) "owns" the dummy control.
            // Then we can use it to switch from worker threads to the main thread.
            IntPtr forceHandleCreation = dummy.Handle; 
        }

        /// <summary>
        /// Invoke an action on the UI thread.
        /// Some events are normally raised inside the camera manager on a worker thread,
        /// we continue the call to the UI thread.
        /// </summary>
        public void Invoke(Action handler)
        {
            if (dummy == null || dummy.IsDisposed || !dummy.IsHandleCreated)
                return;

            dummy.BeginInvoke(handler);
        }

        public abstract bool Enabled { get; }

        public abstract string CameraType { get; }
        
        public abstract string CameraTypeFriendlyName { get; }
        
        /// <summary>
        /// Whether cameras of this type can be manually connected from the wizard.
        /// </summary>
        public abstract bool HasConnectionWizard { get; }

        /// <summary>
        /// Checks that the necessary components are available for the manager to work.
        /// This function will be called once, if it returns false the manager will be discarded.
        /// </summary>
        public abstract bool SanityCheck();
        
        /// <summary>
        /// Get the list of reachable cameras, try to connect to each of them to get a snapshot, and return a small summary of the device.
        /// Knowing about the camera is enough, the camera managers should cache the snapshots to avoid connecting to the camera each time.
        /// </summary>
        public abstract List<CameraSummary> DiscoverCameras(IEnumerable<CameraBlurb> blurbs);

        /// <summary>
        /// Invalidate the camera reference from any cache held in the module.
        /// It should be discovered again at the next DiscoverCameras step.
        /// </summary>
        public abstract void ForgetCamera(CameraSummary summary);

        /// <summary>
        /// Check if this manager knows this camera.
        /// Returns the CameraSummary from discovery or null.
        /// </summary>
        public abstract CameraSummary GetCameraSummary(string alias);

        /// <summary>
        /// Start a function that will retrieve a single image from a particular camera, for thumbnail purposes.
        /// The function should be asynchronous and raise CameraThumbnailProduced event when done.
        /// The event should always be raised, even if the thumbnail could not be retrieved.
        /// </summary>
        public abstract void StartThumbnail(CameraSummary summary);

        /// <summary>
        /// Cancel any thumbnail retrieval in progress.
        /// </summary>
        public abstract void StopAllThumbnails();

        /// <summary>
        /// Extract a camera blurb (used for KVA persistence) from a camera summary.
        /// </summary>
        public abstract CameraBlurb BlurbFromSummary(CameraSummary summary);
        
        /// <summary>
        /// Create a frame grabbing object that will let us connect to and grab the camera.
        /// </summary>
        public abstract ICaptureSource CreateCaptureSource(CameraSummary summary);
        
        /// <summary>
        /// Launch a dialog to configure the device. 
        /// Returns true if the camera needs to be disconnected/reconnected to take the new parameters into account.
        /// </summary>
        public abstract bool Configure(CameraSummary summary);

        /// <summary>
        /// Launch a dialog to configure the device. Returns true if the configuration has changed.
        /// This method can be overriden by managers that require disconnecting/reconnecting during the operation.
        /// Default behavior is to call Configure(summary).
        /// </summary>
        public virtual bool Configure(CameraSummary summary, Action disconnect, Action reconnect)
        {
            return Configure(summary);
        }

        /// <summary>
        /// Returns a small non-translatable text to be displayed in the header line above the image.
        /// </summary>
        public abstract string GetSummaryAsText(CameraSummary summary);
        
        /// <summary>
        /// Returns a piece of user interface that will be presented in the main connection wizard.
        /// </summary>
        public abstract Control GetConnectionWizard();
        
        public override string ToString()
        {
            return CameraTypeFriendlyName;
        }

        /// <summary>
        /// Called after the user personalizes a camera. Should persist the customized information.
        /// </summary>
        public void UpdatedCameraSummary(CameraSummary summary)
        {
            PreferencesManager.CapturePreferences.AddCamera(BlurbFromSummary(summary));
        }

    }
}
