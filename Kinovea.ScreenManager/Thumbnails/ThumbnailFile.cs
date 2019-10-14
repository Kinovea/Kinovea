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
        public event EventHandler<EditingEventArgs> FileNameEditing;
        #endregion
        
        #region Properties
        
        public string FileName {
            get { return m_FileName; }
        }
        public bool IsError {
            get { return m_IsError;}
        }
        #endregion
        
        #region Members
        private String m_FileName;
        private bool m_Loaded;
        private int paddingHorizontal = 6;
        private int paddingVertical = 21;
        private int minWidthForDetails = 150;

        private bool m_bIsSelected = false;
        private bool m_IsError;
        private List<Bitmap> m_Bitmaps;
        private Bitmap currentThumbnail;
        private FileDetails details = new FileDetails();
        private bool m_bIsImage;
        private bool hasKva;
        private int currentThumbnailIndex;
        private bool m_Hovering;
        private Bitmap bmpKvaAnalysis = Resources.bullet_white;
        private System.Windows.Forms.Timer tmrThumbs = new System.Windows.Forms.Timer();
        
        #region Context menu
        private ContextMenuStrip  popMenu = new ContextMenuStrip();
        private ToolStripMenuItem mnuLaunch = new ToolStripMenuItem();
        private ToolStripMenuItem mnuRename = new ToolStripMenuItem();
        private ToolStripMenuItem mnuDelete = new ToolStripMenuItem();
        private ToolStripMenuItem mnuOpenInExplorer = new ToolStripMenuItem();
        #endregion
        
        private bool m_bEditMode;
        
        private static readonly int m_iTimerInterval = 700;
        private static readonly Pen m_PenSelected = new Pen(Color.DodgerBlue, 2);
        private static readonly Pen m_PenUnselected = new Pen(Color.Silver, 2);
        private static readonly Pen m_PenShadow = new Pen(Color.Lavender, 2);
        private static readonly Font fontFileDetails = new Font("Arial", 8, FontStyle.Regular);
        private static readonly SolidBrush m_BrushQuickPreviewActive = new SolidBrush(Color.FromArgb(128, Color.SteelBlue));
        private static readonly SolidBrush m_BrushQuickPreviewInactive = new SolidBrush(Color.FromArgb(128, Color.LightSteelBlue));
        private static readonly SolidBrush m_BrushDuration = new SolidBrush(Color.FromArgb(150, Color.Black));
        private Pen penFileDetails = new Pen(m_BrushDuration);
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        #region Construction & initialization
        public ThumbnailFile(string _fileName)
        {
            InitializeComponent();
            
            m_FileName = _fileName;
            penFileDetails.StartCap = LineCap.Round;
            penFileDetails.Width = 14;
            
            SetupTimer();
            SetupTextbox();
            BuildContextMenus();
            RefreshUICulture();
        }
        private void SetupTimer()
        {
            tmrThumbs.Interval = m_iTimerInterval;
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

                tbFileName.KeyPress -= TbFileNameKeyPress;
                tbFileName.Dispose();

                LaunchVideo = null;
                VideoSelected = null;
                FileNameEditing = null;
            }
            
            base.Dispose(disposing);
        }
        #endregion
        
        #region Public interface
        public void Populate(VideoSummary summary)
        {
            m_Loaded = true;
            
            if (summary == null || summary.Thumbs == null || summary.Thumbs.Count < 1)
            {
                DisplayAsError();
            }
            else
            {
                m_Bitmaps = summary.Thumbs;
                if(m_Bitmaps != null && m_Bitmaps.Count > 0)
                {
                    currentThumbnailIndex = 0;
                    currentThumbnail = m_Bitmaps[currentThumbnailIndex];
                }
                
                if(summary.IsImage)
                {
                    m_bIsImage = true;
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
                
                hasKva = summary.HasKva;
                if (hasKva)
                    details.Details[FileProperty.HasKva] = "kva";

                details.Details[FileProperty.CreationTime] = string.Format("{0:g}", summary.Creation);

                SetSize(this.Width, this.Height);
            }
        }
        
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
            m_IsError = true;
            mnuLaunch.Visible = false;
        }
        public void SetUnselected()
        {
            // This method does NOT trigger an event to notify the container.
            m_bIsSelected = false;
            picBox.Invalidate();
        }
        public void SetSelected()
        {
            // This method triggers an event to notify the container.
            if (!m_bIsSelected)
            {
                m_bIsSelected = true;
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
            if(m_bEditMode)
            {
                m_bEditMode = false;
                ToggleEditMode();	
            }
        }
        public void RefreshUICulture()
        {
            lblFileName.Text = Path.GetFileNameWithoutExtension(m_FileName);
            TruncateFilename();

            mnuLaunch.Text = ScreenManagerLang.Generic_Open;
            mnuRename.Text = ScreenManagerLang.mnuThumbnailRename;
            mnuDelete.Text = ScreenManagerLang.mnuThumbnailDelete;
            mnuOpenInExplorer.Text = ScreenManagerLang.mnuThumbnailLocate;
            
            picBox.Invalidate();
        }
        public void DisposeImages()
        {
            if(m_IsError || m_Bitmaps == null)
                return;
            
            foreach(Bitmap bmp in m_Bitmaps)
                bmp.Dispose();
            
            m_Bitmaps.Clear();
        }
        public void Animate(bool _animate)
        {
            if(_animate)
                tmrThumbs.Start();
            else
                tmrThumbs.Stop();
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
            if(!m_IsError)
                SetSelected();
        }
        private void LblFileNameClick(object sender, EventArgs e)
        {
            if(!m_IsError)
            {
                if(!m_bIsSelected)
                    SetSelected();
                else
                    StartRenaming();
            }
        }

        private void PicBoxPaint(object sender, PaintEventArgs e)
        {
            // Configure for speed. These are thumbnails anyway.
            e.Graphics.PixelOffsetMode = PixelOffsetMode.None; //PixelOffsetMode.HighSpeed;
            e.Graphics.CompositingQuality = CompositingQuality.HighSpeed;
            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            e.Graphics.SmoothingMode = SmoothingMode.HighSpeed;
            
            if(m_Loaded)
            {
                if(m_IsError)
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
        private void DrawImage(Graphics _canvas)
        {
            // We always draw to the whole container,
            // it is the picBox that is ratio stretched, see SetSize().
            if(currentThumbnail != null)
                _canvas.DrawImage(currentThumbnail, 0, 0, picBox.Width, picBox.Height);
        }
        private void DrawBorder(Graphics _canvas)
        {
            Pen p = m_bIsSelected ? m_PenSelected : m_PenUnselected;
            _canvas.DrawRectangle(p, 1, 1, picBox.Width-2, picBox.Height-2);
            _canvas.DrawRectangle(Pens.White, 2, 2, picBox.Width-5, picBox.Height-5);
        }
        private void DrawPreviewRectangles(Graphics _canvas)
        {
            // Draw quick preview rectangles.
            if(!m_Hovering || m_Bitmaps == null || m_Bitmaps.Count < 2)
                return;

            int rectWidth = picBox.Width / m_Bitmaps.Count;
            int rectHeight = 20;
            for(int i=0;i<m_Bitmaps.Count;i++)
            {
                SolidBrush b = i == currentThumbnailIndex ? m_BrushQuickPreviewActive : m_BrushQuickPreviewInactive;
                _canvas.FillRectangle(b, rectWidth * i, picBox.Height - rectHeight, rectWidth, rectHeight);	
            }
        }

        #region Draw file details
        private void DrawFileProperties(Graphics canvas)
        {
            Dictionary<FileProperty, bool> visibility = PreferencesManager.FileExplorerPreferences.FilePropertyVisibility.Visible;

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
                string duration = m_bIsImage ? ScreenManagerLang.Generic_Image + " " : details.Details[FileProperty.Duration];
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
            return details.Details.ContainsKey(prop) &&
                   PreferencesManager.FileExplorerPreferences.FilePropertyVisibility.Visible.ContainsKey(prop) &&
                   PreferencesManager.FileExplorerPreferences.FilePropertyVisibility.Visible[prop];
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

        private void DrawPlaceHolder(Graphics _canvas)
        {
            _canvas.DrawRectangle(Pens.Gainsboro, 0, 0, picBox.Width-1, picBox.Height-1);
        }

        private void DrawError(Graphics _canvas)
        {
            Bitmap bmp = Properties.Resources.film_error2;
            int left = (picBox.Width - bmp.Width) / 2;
            int top = (picBox.Height - bmp.Height) / 2;
            _canvas.DrawImage(bmp, left, top);
        }

        private void PicBoxMouseMove(object sender, MouseEventArgs e)
        {
            if(m_IsError || m_Bitmaps == null || m_Bitmaps.Count < 1)
                return;
            
            if(e.Y > picBox.Height - 20)
            {
                tmrThumbs.Stop();
                int index = e.X / (picBox.Width / m_Bitmaps.Count);
                currentThumbnailIndex = Math.Max(Math.Min(index, m_Bitmaps.Count - 1), 0);
                currentThumbnail = m_Bitmaps[currentThumbnailIndex];
                picBox.Invalidate();
            }
            else
            {
                tmrThumbs.Start();
            }
        }

        private void ThumbListViewItemPaint(object sender, PaintEventArgs e)
        {
            // Draw the shadow
            if(m_Loaded && !m_IsError)
            {
                e.Graphics.DrawLine(m_PenShadow, picBox.Left + picBox.Width + 1, picBox.Top + m_PenShadow.Width, picBox.Left + picBox.Width + 1, picBox.Top + picBox.Height + m_PenShadow.Width);
                e.Graphics.DrawLine(m_PenShadow, picBox.Left + m_PenShadow.Width, picBox.Top + picBox.Height + 1, picBox.Left + m_PenShadow.Width + picBox.Width, picBox.Top + picBox.Height + 1);
            }
        }

        private void tmrThumbs_Tick(object sender, EventArgs e) 
        {
            // This event occur when the user has been staying for a while on the same thumbnail. Loop between all stored images.
            if(m_IsError || m_Bitmaps == null || m_Bitmaps.Count < 2)
                return;
            
            currentThumbnailIndex++;
            if(currentThumbnailIndex >= m_Bitmaps.Count)
                currentThumbnailIndex = 0;
            
            currentThumbnail = m_Bitmaps[currentThumbnailIndex];
            picBox.Invalidate();
        }

        private void PicBoxMouseEnter(object sender, EventArgs e)
        {
            m_Hovering = true;
        
            if(m_IsError || m_Bitmaps == null || m_Bitmaps.Count < 2)
                return;
            
            // Instantly change image
            currentThumbnailIndex = 1;
            currentThumbnail = m_Bitmaps[currentThumbnailIndex];
            picBox.Invalidate();

            // Then start timer to slideshow.
            tmrThumbs.Start();
        }

        private void PicBoxMouseLeave(object sender, EventArgs e)
        {
            m_Hovering = false;
            
            if(!m_IsError && m_Bitmaps != null)
            {
                tmrThumbs.Stop();
                if(m_Bitmaps.Count > 0)
                {
                    currentThumbnailIndex = 0;
                    currentThumbnail = m_Bitmaps[currentThumbnailIndex];
                    picBox.Invalidate();	
                }
            }
        }

        private void TbFileNameKeyPress(object sender, KeyPressEventArgs e)
        {
            // Editing a file name.
            if(e.KeyChar == 27)
            {
                QuitEditMode();
                return;
            }
            
            if (e.KeyChar != 13) // Carriage Return.
                return;
            
            
            string newFileName = Path.GetDirectoryName(m_FileName) + "\\" + tbFileName.Text;				
            if(File.Exists(m_FileName) && !File.Exists(newFileName) && newFileName.Length > 5)
            {
                try
                {
                    File.Move(m_FileName, newFileName);
                    
                    if(!File.Exists(m_FileName))
                    {
                        m_FileName = newFileName;
                        lblFileName.Text = Path.GetFileNameWithoutExtension(m_FileName);
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
            FilesystemHelper.LocateFile(m_FileName);
        }
        #endregion
        
        #region Edit mode
        public void StartRenaming()
        {
            // Switch to edit mode.
            if (FileNameEditing != null)
            {
                FileNameEditing(this, new EditingEventArgs(true));
                m_bEditMode = true;
                ToggleEditMode();
            }
        }
        private void QuitEditMode()
        {
            // Quit edit mode.
            if (FileNameEditing != null)
                FileNameEditing(this, new EditingEventArgs(false));
            
            m_bEditMode = false;
            ToggleEditMode();
        }
        private void ToggleEditMode()
        {
            // the global variable m_bEditMode should already have been set
            // Now let's configure the display depending on its value.
            if(m_bEditMode)
            {
                // The layout is configured at construction time.
                tbFileName.Text = Path.GetFileName(m_FileName);
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
        
        private void TruncateFilename()
        {
            try
            {
                string text = Path.GetFileNameWithoutExtension(m_FileName);
                
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
            }
            catch
            {
                // An exception is thrown here during closing of the application.
            }
            
        }

        public void Delete()
        {
            FilesystemHelper.DeleteFile(m_FileName);
            if (!File.Exists(m_FileName))
                NotificationCenter.RaiseRefreshFileExplorer(this, true);
        }

    }
    
    #region EventArgs classe used here
    /// <summary>
    /// A (very) simple event args class to encapsulate the state of the editing.
    /// </summary>
    public class EditingEventArgs : EventArgs
    {
        public readonly bool Editing;

        public EditingEventArgs( bool _bEditing )
        {
            Editing = _bEditing;
        }
    }
    #endregion
}
