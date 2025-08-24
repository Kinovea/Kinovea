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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;

using Kinovea.ScreenManager.Languages;
using Kinovea.ScreenManager.Properties;
using Kinovea.Services;
using Kinovea.Video;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Thumbnail control.
    /// 
    /// </summary>
    public partial class ThumbnailFile : UserControl
    {
        #region Events
        public event EventHandler LaunchVideo;
        public event EventHandler VideoSelected;
        public event EventHandler<EventArgs<bool>> FileNameEditing;
        #endregion

        #region Properties

        /// <summary>
        /// Full path to the target file.
        /// </summary>
        public string FilePath 
        {
            get 
            { 
                return path; 
            }
        }

        /// <summary>
        /// True if the file couldn't be loaded.
        /// </summary>
        public bool IsError 
        {
            get { return isError;}
        }

        /// <summary>
        /// Last known write time of the target file.
        /// </summary>
        public DateTime LastWriteUTC
        {
            get { return lastWriteUTC; }
            set { lastWriteUTC = value; }
        }
        #endregion

        #region Members
        private string path;
        private bool loaded;
        private int paddingHorizontal = 6;
        private int paddingVertical = 21;
        private int minWidthForDetails = 150;

        private bool selected = false;
        private bool isError;
        private List<Bitmap> bitmaps;
        private Bitmap currentThumbnail;
        private FileDetails details = new FileDetails();
        private bool isImage;
        private int currentThumbnailIndex;
        private bool hoverInProgress;
        private Bitmap bmpKvaAnalysis = Resources.bullet_white;
        private DateTime lastWriteUTC = DateTime.MinValue;
        private System.Windows.Forms.Timer tmrThumbs = new System.Windows.Forms.Timer();
        private Dictionary<FileProperty, bool> visibilityOptions = new Dictionary<FileProperty, bool>();

        #region Context menu
        private ContextMenuStrip  popMenu = new ContextMenuStrip();
        private ToolStripMenuItem mnuLaunch = new ToolStripMenuItem();
        private ToolStripMenuItem mnuRename = new ToolStripMenuItem();
        private ToolStripMenuItem mnuDelete = new ToolStripMenuItem();
        private ToolStripMenuItem mnuOpenInExplorer = new ToolStripMenuItem();
        #endregion
        
        private bool editModeInProgress;
        
        private static readonly int timerInterval = 700;
        private static readonly Pen penSelected = new Pen(Color.DodgerBlue, 2);
        private static readonly Pen penUnselected = new Pen(Color.Silver, 2);
        private static readonly Pen penShadow = new Pen(Color.Lavender, 2);
        private static readonly Font fontFileDetails = new Font("Arial", 8, FontStyle.Regular);
        private static readonly SolidBrush brushQuickPreviewActive = new SolidBrush(Color.FromArgb(128, Color.SteelBlue));
        private static readonly SolidBrush brushQuickPreviewInactive = new SolidBrush(Color.FromArgb(128, Color.LightSteelBlue));
        private static readonly SolidBrush brushDuration = new SolidBrush(Color.FromArgb(150, Color.Black));
        private Pen penFileDetails = new Pen(brushDuration);
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        #region Construction & initialization
        public ThumbnailFile(string path)
        {
            InitializeComponent();
            
            lblFileName.BackColor = Color.Transparent;

            this.path = path;
            penFileDetails.StartCap = LineCap.Round;
            penFileDetails.Width = 14;

            SetupTimer();
            SetupTextbox();
            BuildContextMenus();

            visibilityOptions = PreferencesManager.FileExplorerPreferences.FilePropertyVisibility.Visible;
            RefreshUICulture(visibilityOptions);
        }
        private void SetupTimer()
        {
            tmrThumbs.Interval = timerInterval;
            tmrThumbs.Tick += tmrThumbs_Tick;
            currentThumbnailIndex = 0;
        }
        private void SetupTextbox()
        {
            // Make the editbox follow the same layout pattern than the label.
            // except that its minimal height is depending on font.
            tbFileName.Font = lblFileName.Font;
            tbFileName.Left = lblFileName.Left;
            tbFileName.Width = lblFileName.Width;
            tbFileName.Top = this.Height - tbFileName.Height;
            //tbFileName.Height = lblFileName.Height;
            tbFileName.Anchor = lblFileName.Anchor;
        }
        private void BuildContextMenus()
        {
            mnuLaunch.Image = Properties.Resources.film_go;
            mnuOpenInExplorer.Image = Properties.Resources.folder_new;
            mnuRename.Image = Properties.Resources.rename;
            mnuDelete.Image = Properties.Resources.delete;
            
            mnuLaunch.Click += mnuLaunch_Click;
            mnuOpenInExplorer.Click += mnuOpenInExplorer_Click;
            mnuRename.Click += mnuRename_Click;
            mnuDelete.Click += mnuDelete_Click;
            
            popMenu.Items.AddRange(new ToolStripItem[] { 
                mnuLaunch, 
                mnuOpenInExplorer,
                //mnuRename, 
                new ToolStripSeparator(),
                mnuDelete
            });

            this.ContextMenuStrip = popMenu;
        }
        
        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                    components.Dispose();

                mnuLaunch.Click -= mnuLaunch_Click;
                mnuOpenInExplorer.Click -= mnuOpenInExplorer_Click;
                mnuRename.Click -= mnuRename_Click;
                mnuDelete.Click -= mnuDelete_Click;
                
                mnuLaunch.Dispose();
                mnuOpenInExplorer.Dispose();
                mnuRename.Dispose();
                mnuDelete.Dispose();
                
                popMenu.Items.Clear();
                popMenu.Dispose();
                this.ContextMenuStrip = null;

                penFileDetails.Dispose();

                tmrThumbs.Tick -= tmrThumbs_Tick;
                tmrThumbs.Dispose();

                tbFileName.KeyPress -= TbFileName_KeyPress;
                tbFileName.Dispose();

                LaunchVideo = null;
                VideoSelected = null;
                FileNameEditing = null;
            }
            
            base.Dispose(disposing);
        }
        #endregion
        
        #region Public interface

        public void SetTargetFile(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                // We won't be using this thumbnail this time around.
                this.path = string.Empty;
                this.lastWriteUTC = DateTime.MinValue;
                loaded = false;
                return;
            }

            if (this.path == path)
                return;

            this.path = path;

            // Invalidate the data.
            // It will be filled back later in LoadSummary().
            lastWriteUTC = DateTime.MinValue;
            loaded = false;

            // Force invalidate to make sure we are not showing an old thumbnail.
            // We don't dispose the old bitmap until the last moment.
            this.Invalidate();
        }

        /// <summary>
        /// Fill the thumbnail control with the video summary.
        /// </summary>
        public void LoadSummary(VideoSummary summary)
        {
            DisposeImages();

            if (path != summary.Filename)
            {
                // This should never happen.
                log.ErrorFormat("Summary received for the wrong thumbnail control.");
                DisplayAsError();
                lastWriteUTC = DateTime.MinValue;
            }
            else if (summary == null || summary.Thumbs == null || summary.Thumbs.Count < 1)
            {
                log.ErrorFormat("No images extracted for {0}.", path);

                // This can happen when a file is copy/pasted externally.
                // We receive an event as soon as the file is created but we can't extract a summary yet.

                // Trigger the file name update anyway. Now that we recycle controls we need
                // to not show the wrong name. We don't do this on file name update for perf reasons.
                // Keep whatever size it currently has.
                TruncateFilename();

                DisplayAsError();
                lastWriteUTC = DateTime.MinValue;
            }
            else
            {
                bitmaps = summary.Thumbs;
                if (bitmaps != null && bitmaps.Count > 0)
                {
                    currentThumbnailIndex = 0;
                    currentThumbnail = bitmaps[currentThumbnailIndex];
                }

                if (summary.IsImage)
                {
                    isImage = true;
                    details.Details[FileProperty.Duration] = "0";
                }
                else
                {
                    details.Details[FileProperty.Duration] = TimeHelper.MillisecondsToTimecode((double)summary.DurationMilliseconds, 0);
                    details.Details[FileProperty.Framerate] = string.Format("{0:0.##} fps", summary.Framerate);
                }

                if (summary.ImageSize == Size.Empty)
                    details.Details[FileProperty.Size] = "";
                else
                    details.Details[FileProperty.Size] = string.Format("{0}×{1}", summary.ImageSize.Width, summary.ImageSize.Height);

                // Filesystem level properties.
                bool hasKva = false;
                DateTime creation = DateTime.Now;
                if (!string.IsNullOrEmpty(summary.Filename) && File.Exists(summary.Filename))
                {
                    hasKva = VideoSummary.HasCompanionKva(summary.Filename);
                    creation = File.GetCreationTime(summary.Filename);
                }

                if (hasKva)
                    details.Details[FileProperty.HasKva] = "kva";

                details.Details[FileProperty.CreationTime] = string.Format("{0:g}", creation);

                isError = false;
                mnuLaunch.Visible = true;

                // Keep track of the last write time to detect if a reload is really needed.
                lastWriteUTC = File.Exists(summary.Filename) ? File.GetLastWriteTimeUtc(summary.Filename) : DateTime.MinValue;
            }

            loaded = true;
        }

        /// <summary>
        /// Reset the size of the thumbnail control and recompute the image size and the
        /// filename truncation/visibility.
        /// </summary>
        public void SetSize(int width, int height)
        {
            // Called at init step and on resize.
            // Represent the size of the whole control, not just the image.
            
            // Width changed due to screen resize or thumbview mode change.
            this.Width = width;
            this.Height = height;
            
            // picBox is ratio stretched.
            if(currentThumbnail != null)
            {
                picBox.Size = ComputeImageSize(currentThumbnail.Size, this.Size);
                picBox.Left = (paddingHorizontal / 2) + (this.Width - paddingHorizontal - picBox.Width) / 2;
                picBox.Top = (paddingHorizontal / 2) + (this.Height - picBox.Height - paddingVertical);
            }
            else
            {
                picBox.Height = (int)(picBox.Width * 0.75f);
            }

            TruncateFilename();
            picBox.Invalidate();
        }

        /// <summary>
        /// Return the actual image size so it fits in the container retaining ratio.
        /// </summary>
        public Size ComputeImageSize(Size originalSize, Size containerSize)
        {
            float ratioWidth = (float)originalSize.Width / (containerSize.Width - paddingHorizontal);
            float ratioHeight = (float)originalSize.Height / (containerSize.Height - paddingVertical);
            float ratio = Math.Max(ratioWidth, ratioHeight);

            int width = (int)(originalSize.Width / ratio);
            int height = (int)(originalSize.Height / ratio);

            return new Size(width, height);
        }

        public Size MaxImageSize(Size containerSize)
        {
            return new Size(containerSize.Width - paddingHorizontal, containerSize.Height - paddingVertical);
        }

        public void DisplayAsError()
        {
            isError = true;
            mnuLaunch.Visible = false;
        }
        public void SetUnselected()
        {
            // This method does NOT trigger an event to notify the container.
            selected = false;
            picBox.Invalidate();
        }
        public void SetSelected()
        {
            // This method triggers an event to notify the container.
            if (!selected)
            {
                selected = true;
                picBox.Invalidate();
                
                // Report change in selection
                if (VideoSelected != null)
                {
                    VideoSelected(this, EventArgs.Empty);
                }
            }
        }
        public void CancelEditMode()
        {
            // Called from the container when we click nowhere.
            // Do not call QuitEditMode here, as we may be entering as a result of that.
            if(editModeInProgress)
            {
                editModeInProgress = false;
                ToggleEditMode();	
            }
        }
        public void RefreshUICulture(Dictionary<FileProperty, bool> visibilityOptions)
        {
            lblFileName.Text = Path.GetFileNameWithoutExtension(path);
            TruncateFilename();

            mnuLaunch.Text = ScreenManagerLang.Generic_Open;
            mnuRename.Text = ScreenManagerLang.mnuThumbnailRename;
            mnuDelete.Text = ScreenManagerLang.mnuThumbnailDelete;
            mnuOpenInExplorer.Text = ScreenManagerLang.mnuThumbnailLocate;

            this.visibilityOptions = visibilityOptions;

            picBox.Invalidate();
        }

        public void DisposeImages()
        {
            if(bitmaps == null)
                return;
            
            foreach(Bitmap bmp in bitmaps)
                bmp.Dispose();
            
            bitmaps.Clear();
        }
        #endregion
        
        #region UI Event Handlers
        private void AllControls_DoubleClick(object sender, EventArgs e)
        {
            if (LaunchVideo == null)
                return;
            
            this.Cursor = Cursors.WaitCursor;
            LaunchVideo(this, EventArgs.Empty);
            this.Cursor = Cursors.Default;
        }
        private void AllControls_Click(object sender, EventArgs e)
        {
            if(!isError)
                SetSelected();
        }
        private void LblFileNameClick(object sender, EventArgs e)
        {
            if(!isError)
            {
                if(!selected)
                    SetSelected();
                else
                    StartRenaming();
            }
        }


        #region Painting
        private void PicBoxPaint(object sender, PaintEventArgs e)
        {
            // Configure for speed. These are thumbnails anyway.
            e.Graphics.PixelOffsetMode = PixelOffsetMode.None; //PixelOffsetMode.HighSpeed;
            e.Graphics.CompositingQuality = CompositingQuality.HighSpeed;
            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            e.Graphics.SmoothingMode = SmoothingMode.HighSpeed;
            
            if(loaded)
            {
                if(isError)
                {
                   DrawPlaceHolder(e.Graphics);
                   DrawError(e.Graphics);			       
                }
                else
                {
                    DrawImage(e.Graphics);
                    DrawBorder(e.Graphics);
                    DrawPreviewRectangles(e.Graphics);

                    if (this.Width > minWidthForDetails)
                        DrawFileProperties(e.Graphics);
                }
            }
            else
            {
                DrawPlaceHolder(e.Graphics);
            }
        }
        private void DrawImage(Graphics canvas)
        {
            // We always draw to the whole container,
            // it is the picBox that is ratio stretched, see SetSize().
            if(currentThumbnail != null)
                canvas.DrawImage(currentThumbnail, 0, 0, picBox.Width, picBox.Height);
        }
        private void DrawBorder(Graphics canvas)
        {
            Pen p = selected ? penSelected : penUnselected;
            canvas.DrawRectangle(p, 1, 1, picBox.Width-2, picBox.Height-2);
            canvas.DrawRectangle(Pens.White, 2, 2, picBox.Width-5, picBox.Height-5);
        }
        private void DrawPreviewRectangles(Graphics canvas)
        {
            // Draw quick preview rectangles.
            if(!hoverInProgress || bitmaps == null || bitmaps.Count < 2)
                return;

            int rectWidth = picBox.Width / bitmaps.Count;
            int rectHeight = 20;
            for(int i=0;i<bitmaps.Count;i++)
            {
                SolidBrush b = i == currentThumbnailIndex ? brushQuickPreviewActive : brushQuickPreviewInactive;
                canvas.FillRectangle(b, rectWidth * i, picBox.Height - rectHeight, rectWidth, rectHeight);	
            }
        }

        #region Draw file details
        private void DrawFileProperties(Graphics canvas)
        {
            int top = 12;
            int verticalMargin = (int)penFileDetails.Width + 3;

            if (ShouldShowProperty(FileProperty.Size))
            {
                string size = details.Details[FileProperty.Size];
                DrawPropertyString(canvas, size, top);
                top += verticalMargin;
            }

            if (ShouldShowProperty(FileProperty.Framerate))
            {
                string framerate = details.Details[FileProperty.Framerate];
                DrawPropertyString(canvas, framerate, top);
                top += verticalMargin;
            }

            if (ShouldShowProperty(FileProperty.Duration))
            {
                string duration = isImage ? ScreenManagerLang.Generic_Image + " " : details.Details[FileProperty.Duration];
                DrawPropertyString(canvas, duration, top);
                top += verticalMargin;
            }

            if (ShouldShowProperty(FileProperty.CreationTime))
            {
                string creationTime = details.Details[FileProperty.CreationTime];
                DrawPropertyString(canvas, creationTime, top);
                top += verticalMargin;
            }

            if (ShouldShowProperty(FileProperty.HasKva))
            {
                DrawPropertyString(canvas, "kva", top);
                top += verticalMargin;
            }
        }

        private bool ShouldShowProperty(FileProperty prop)
        {
            return details.Details.ContainsKey(prop) && visibilityOptions.ContainsKey(prop) && visibilityOptions[prop];
        }

        private void DrawPropertyString(Graphics canvas, string text, int top)
        {
            if (string.IsNullOrEmpty(text))
                return;
            
            canvas.SmoothingMode = SmoothingMode.AntiAlias;
            
            SizeF bgSize = canvas.MeasureString(text, fontFileDetails);
            canvas.DrawLine(penFileDetails, (float)picBox.Width - bgSize.Width - 1, top, (float)picBox.Width - 4, top);
            canvas.DrawString(text, fontFileDetails, Brushes.White, (float)picBox.Width - bgSize.Width - 3, top - penFileDetails.Width / 2);
        }
        #endregion

        private void DrawPlaceHolder(Graphics canvas)
        {
            canvas.DrawRectangle(Pens.Gainsboro, 0, 0, picBox.Width-1, picBox.Height-1);
        }

        private void DrawError(Graphics canvas)
        {
            Bitmap bmp = Properties.Resources.film_error2;
            int left = (picBox.Width - bmp.Width) / 2;
            int top = (picBox.Height - bmp.Height) / 2;
            canvas.DrawImage(bmp, left, top);
        }
        #endregion

        private void PicBoxMouseMove(object sender, MouseEventArgs e)
        {
            if(isError || bitmaps == null || bitmaps.Count < 1)
                return;
            
            if(e.Y > picBox.Height - 20)
            {
                tmrThumbs.Stop();
                int index = e.X / (picBox.Width / bitmaps.Count);
                currentThumbnailIndex = Math.Max(Math.Min(index, bitmaps.Count - 1), 0);
                currentThumbnail = bitmaps[currentThumbnailIndex];
                picBox.Invalidate();
            }
            else
            {
                tmrThumbs.Start();
            }
        }

        private void ThumbnailFile_Paint(object sender, PaintEventArgs e)
        {
            // Draw the shadow
            if(loaded && !isError)
            {
                e.Graphics.DrawLine(penShadow, picBox.Left + picBox.Width + 1, picBox.Top + penShadow.Width, picBox.Left + picBox.Width + 1, picBox.Top + picBox.Height + penShadow.Width);
                e.Graphics.DrawLine(penShadow, picBox.Left + penShadow.Width, picBox.Top + picBox.Height + 1, picBox.Left + penShadow.Width + picBox.Width, picBox.Top + picBox.Height + 1);
            }
        }

        private void tmrThumbs_Tick(object sender, EventArgs e) 
        {
            // This event occur when the user has been staying for a while on the same thumbnail. Loop between all stored images.
            if(isError || bitmaps == null || bitmaps.Count < 2)
                return;
            
            currentThumbnailIndex++;
            if(currentThumbnailIndex >= bitmaps.Count)
                currentThumbnailIndex = 0;
            
            currentThumbnail = bitmaps[currentThumbnailIndex];
            picBox.Invalidate();
        }

        private void PicBox_MouseEnter(object sender, EventArgs e)
        {
            hoverInProgress = true;
        
            if(isError || bitmaps == null || bitmaps.Count < 2)
                return;

            // Instantly change image
            currentThumbnailIndex = 1;
            currentThumbnail = bitmaps[currentThumbnailIndex];
            picBox.Invalidate();

            // Then start timer to slideshow.
            tmrThumbs.Start();
        }

        private void PicBox_MouseLeave(object sender, EventArgs e)
        {
            hoverInProgress = false;

            if (isError || bitmaps == null)
                return;

            tmrThumbs.Stop();
            if(bitmaps.Count > 0)
            {
                currentThumbnailIndex = 0;
                currentThumbnail = bitmaps[currentThumbnailIndex];
                picBox.Invalidate();	
            }
        }

        private void TbFileName_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Editing a file name.
            if(e.KeyChar == 27)
            {
                QuitEditMode();
                return;
            }
            
            if (e.KeyChar != 13) // Carriage Return.
                return;
            
            
            string newFileName = System.IO.Path.GetDirectoryName(path) + "\\" + tbFileName.Text;				
            if(File.Exists(path) && !File.Exists(newFileName) && newFileName.Length > 5)
            {
                try
                {
                    File.Move(path, newFileName);
                    
                    if(!File.Exists(path))
                    {
                        path = newFileName;
                        lblFileName.Text = System.IO.Path.GetFileNameWithoutExtension(path);
                        TruncateFilename();
                    }

                    NotificationCenter.RaiseRefreshFileExplorer(this, false);
                }
                catch(ArgumentException)
                {
                    // contains only white space, or contains invalid characters as defined in InvalidPathChars.
                    // -> Silently fail.
                    // TODO:Display error dialog box.
                }
                catch(UnauthorizedAccessException)
                {
                    // The caller does not have the required permission.
                }
                catch(Exception)
                {
                    // Log error.
                }
            }
            QuitEditMode();
            
            // Set this thumb as selected.
            SetSelected();
        }
        #endregion
        
        #region Menu Event Handlers
        private void mnuRename_Click(object sender, EventArgs e)
        {
            StartRenaming();
        }
        private void mnuDelete_Click(object sender, EventArgs e)
        {
            Delete();
        }
        private void mnuLaunch_Click(object sender, EventArgs e)
        {
            if (LaunchVideo != null)
            {
                this.Cursor = Cursors.WaitCursor;
                LaunchVideo(this, EventArgs.Empty);
                this.Cursor = Cursors.Default;
            }
        }
        private void mnuOpenInExplorer_Click(object sender, EventArgs e)
        {
            FilesystemHelper.LocateFile(path);
        }
        #endregion
        
        #region Edit mode
        public void StartRenaming()
        {
            // Switch to edit mode.
            FileNameEditing?.Invoke(this, new EventArgs<bool>(true));
            editModeInProgress = true;
            ToggleEditMode();
        }
        private void QuitEditMode()
        {
            // Quit edit mode.
            FileNameEditing?.Invoke(this, new EventArgs<bool>(false));
            editModeInProgress = false;
            ToggleEditMode();
        }
        private void ToggleEditMode()
        {
            // the global variable m_bEditMode should already have been set
            // Now let's configure the display depending on its value.
            if(editModeInProgress)
            {
                // The layout is configured at construction time.
                tbFileName.Text = System.IO.Path.GetFileName(path);
                tbFileName.SelectAll();	// Only works for tab ?
                tbFileName.Visible = true;
                tbFileName.Focus();
            }
            else
            {
                tbFileName.Visible = false;
            }
        }
        #endregion
        
        /// <summary>
        /// Truncate or hide the filename under the thumbnail based on width.
        /// </summary>
        private void TruncateFilename()
        {
            lblFileName.Text = "";
            lblFileName.Visible = false;
            
            if (this.Width < 200)
                return;

            try
            {
                string text = Path.GetFileNameWithoutExtension(path);
                
                bool fits = true;
                float maxWidth = this.Width - paddingHorizontal;
                while (TextHelper.MeasureString(text + "#", lblFileName.Font).Width >= maxWidth)
                {
                    text = text.Substring(0, text.Length - 1);
                    fits = false;

                    if (text.Length == 0)
                        break;

                }

                lblFileName.Text = fits ? text : text + "…";
                lblFileName.Visible = true;
            }
            catch
            {
                // An exception is thrown here during closing of the application.
            }
        }

        public void Delete()
        {
            FilesystemHelper.DeleteFile(path);
            if (!File.Exists(path))
                NotificationCenter.RaiseRefreshFileExplorer(this, true);
        }

    }
}
