#region License
/*
Copyright © Joan Charmant 2021.
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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using Kinovea.ScreenManager.Languages;
using Kinovea.Services;
using Kinovea.Video;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Interactive filter to create Kinograms.
    /// A Kinogram is a single image containing copies of the original frames of the video.
    /// 
    /// The parameters let the user change the subset of frames selected, the crop dimension,
    /// the number of columns and rows of the final composition, etc.
    /// </summary>
    public class VideoFilterKinogram : IVideoFilter
    {
        #region Properties
        public string FriendlyName
        {
            get { return "Kinogram"; }
        }
        public Bitmap Current
        {
            get { return bitmap; }
        }
        public bool HasContextMenu
        {
            get { return true; }
        }
        public bool RotatedCanvas 
        { 
            get { return rotatedCanvas; }
        }
        public bool DrawAttachedDrawings
        {
            get { return true; }
        }

        public bool DrawDetachedDrawings
        {
            get 
            {
                // Because the Kinogram is a summary picture is doesn't make sense to 
                // paint the trajectories and chronos over it, they won't match with anything.
                return false; 
            }
        }
        public bool CanExportVideo
        {
            get { return false; }
        }

        public bool CanExportImage
        {
            get { return true; }
        }
        public KinogramParameters Parameters
        {
            get { return parameters; }
        }
        public Metadata ParentMetadata
        {
            get { return parentMetadata; }
        }
        public int ContentHash
        {
            get { return parameters.GetContentHash(); }
        }
        #endregion

        #region members
        private Bitmap bitmap;
        private List<Bitmap> cache = new List<Bitmap>();    // cache of the original images we are using, at the right size for unscaled draw.
        private Size inputFrameSize;         // Size of input images.
        private Size canvasSize;        // Nominal size of output image, this is the same as frameSize unless the canvas is rotated.
        private float cacheScale = 1.0f;
        private bool isCacheDirty = true;
        private bool rotatedCanvas = false;
        private bool showDebug = false;
        private KinogramParameters parameters = new KinogramParameters();
        private IWorkingZoneFramesContainer framesContainer;
        private Metadata parentMetadata;
        private long timestamp;
        private Color BackgroundColor = Color.FromArgb(44, 44, 44);
        private int contextTile = -1;
        private int movingTile = -1;
        private int movingLabel = -1;

        // Labels
        private List<MiniLabel> frameLabels = new List<MiniLabel>();

        #region Menu
        private ToolStripMenuItem mnuConfigure = new ToolStripMenuItem();
        private ToolStripMenuItem mnuAction = new ToolStripMenuItem();
        private ToolStripMenuItem mnuInterpolate = new ToolStripMenuItem();
        private ToolStripMenuItem mnuResetLabelPositions = new ToolStripMenuItem();
        private ToolStripMenuItem mnuResetTile = new ToolStripMenuItem();
        private ToolStripMenuItem mnuResetAllTiles = new ToolStripMenuItem();
        private ToolStripMenuItem mnuOptions = new ToolStripMenuItem();
        private ToolStripMenuItem mnuRightToLeft = new ToolStripMenuItem();
        private ToolStripMenuItem mnuShowBorder = new ToolStripMenuItem();
        private ToolStripMenuItem mnuAutoInterpolate = new ToolStripMenuItem();

        private ToolStripMenuItem mnuMeasurement = new ToolStripMenuItem();
        private Dictionary<MeasureLabelType, ToolStripMenuItem> mnuMeasureLabelTypes = new Dictionary<MeasureLabelType, ToolStripMenuItem>();
        #endregion

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region ctor/dtor
        public VideoFilterKinogram(Metadata metadata)
        {
            this.parentMetadata = metadata;

            InitializeMenus();

            parameters = PreferencesManager.PlayerPreferences.Kinogram;
            ResetCropPositions();
            AfterTileCountChange();
        }

        private void InitializeMenus()
        {
            mnuConfigure.Image = Properties.Drawings.configure;
            mnuConfigure.Click += MnuConfigure_Click;

            mnuAction.Image = Properties.Resources.action;
            mnuInterpolate.Image = Properties.Resources.wand;
            mnuResetLabelPositions.Image = Properties.Drawings.label;
            mnuResetTile.Image = Properties.Resources.bin_empty;
            mnuResetAllTiles.Image = Properties.Resources.bin_empty;
            mnuInterpolate.Click += MnuInterpolate_Click;
            mnuResetLabelPositions.Click += MnuResetLabelPositions_Click;
            mnuResetTile.Click += MnuResetTile_Click;
            mnuResetAllTiles.Click += MnuResetAllTiles_Click;
            mnuAction.DropDownItems.AddRange(new ToolStripItem[] {
                mnuInterpolate,
                new ToolStripSeparator(),
                mnuResetLabelPositions,
                mnuResetTile,
                mnuResetAllTiles,
            });

            mnuOptions.Image = Properties.Resources.equalizer;
            mnuRightToLeft.Image = Properties.Resources.rtl;
            mnuShowBorder.Image = Properties.Resources.border_all;
            mnuAutoInterpolate.Image = Properties.Resources.wand;
            mnuRightToLeft.Click += MnuRightToLeft_Click;
            mnuShowBorder.Click += MnuShowBorder_Click;
            mnuAutoInterpolate.Click += MnuAutoInterpolate_Click;
            mnuOptions.DropDownItems.AddRange(new ToolStripItem[] {
                mnuRightToLeft,
                mnuShowBorder,
                mnuAutoInterpolate,
            });

            mnuMeasurement.Image = Properties.Drawings.label;
            mnuMeasurement.DropDownItems.AddRange(new ToolStripItem[] {
                CreateMeasureLabelTypeMenu(MeasureLabelType.None),
                new ToolStripSeparator(),
                CreateMeasureLabelTypeMenu(MeasureLabelType.Clock),
                CreateMeasureLabelTypeMenu(MeasureLabelType.Frame),
            });

        }

        ~VideoFilterKinogram()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (bitmap != null)
                    bitmap.Dispose();

                ClearCache();
            }
        }
        #endregion

        #region IVideoFilter methods
        
        public void SetFrames(IWorkingZoneFramesContainer framesContainer)
        {
            // Changing the number of frames in the source doesn't impact the grid arrangement.
            // If we don't have enough frames we just show black tiles.
            this.framesContainer = framesContainer;
            isCacheDirty = true;
            if (framesContainer != null && framesContainer.Frames != null && framesContainer.Frames.Count > 0)
            {
                inputFrameSize = framesContainer.Frames[0].Image.Size;
                UpdateSize(inputFrameSize);
            }
        }

        public void UpdateTime(long timestamp)
        {
            // At the moment the timestamp is only used to pass to the number sequence when generating or deleting the numbers.
            this.timestamp = timestamp;
        }

        public void StartMove(PointF p)
        {
            bool hitFrameLabel = false;
            if (parameters.MeasureLabelType != MeasureLabelType.None)
            {
                for (int i = 0; i < frameLabels.Count; i++)
                {
                    if (frameLabels[i].HitTest(p, parentMetadata.ImageTransform))
                    {
                        movingLabel = i;
                        hitFrameLabel = true;
                        break;
                    }
                }
            }
            
            if (!hitFrameLabel)
            {
                movingTile = GetTile(p);
            }

            if (movingTile != -1 || movingLabel != -1)
                CaptureMemento();
        }

        public void StopMove()
        {
            movingTile = -1;
            movingLabel = -1;

            if (parameters.AutoInterpolate)
            {
                InterpolatePositions();
                Update();
            }
        }

        public void Move(float dx, float dy, Keys modifiers)
        {
            if (movingLabel >= 0)
            {
                if ((modifiers & Keys.Shift) == Keys.Shift)
                {
                    foreach (var label in frameLabels)
                        label.MoveLabel(dx, dy);
                }
                else
                {
                    frameLabels[movingLabel].MoveLabel(dx, dy);
                }
            }
            else if (movingTile >= 0)
            {
                parameters.ManualPositions.Add(movingTile);

                if ((modifiers & Keys.Shift) == Keys.Shift)
                {
                    for (int i = 0; i < parameters.TileCount; i++)
                        MoveTile(dx, dy, i);
                
                    Update();
                }
                else
                {
                    MoveTile(dx, dy, movingTile);
                    Update(movingTile);
                }
            }
        }

        /// <summary>
        /// Draw extra content on top of the produced image.
        /// </summary>
        public void DrawExtra(Graphics canvas, DistortionHelper distorter, IImageToViewportTransformer transformer, long timestamp, bool export)
        {
            float step = (float)framesContainer.Frames.Count / parameters.TileCount;
            List<VideoFrame> frames = framesContainer.Frames.Where((frame, i) => i % step < 1).ToList();
            int cols = (int)Math.Ceiling((float)parameters.TileCount / parameters.Rows);
            Size cropSize = GetCropSize();
            Size fullSize = new Size(cropSize.Width * cols, cropSize.Height * parameters.Rows);
            Rectangle paintArea = UIHelper.RatioStretch(fullSize, bitmap.Size);
            Size tileSize = new Size(paintArea.Width / cols, paintArea.Height / parameters.Rows);

            if (!export)
                DrawHighlightBorder(canvas, transformer, timestamp, frames, cols, paintArea, tileSize);
            
            DrawLabels(canvas, transformer);
        }


        public void ExportVideo(IDrawingHostView host)
        {
            throw new NotImplementedException();
        }

        public void ExportImage(IDrawingHostView host)
        {
            // Launch dedicated dialog.
            FormExportKinogram fek = new FormExportKinogram(this, host.CurrentTimestamp);
            FormsHelper.Locate(fek);
            fek.ShowDialog();
            fek.Dispose();

            Update();
        }

        public void ResetData()
        {
            this.framesContainer = null;
            this.parameters = PreferencesManager.PlayerPreferences.Kinogram;
            ResetCropPositions();
            ClearCache();
            AfterTileCountChange();
        }

        public void WriteData(XmlWriter w)
        {
            parameters.WriteXml(w);
        }

        public void ReadData(XmlReader r)
        {
            parameters.ReadXml(r);
            AfterTileCountChange();
            Update();
        }
        #endregion

        #region Public methods, called from dialogs.
        /// <summary>
        /// Paint the composite + annotations on a new bitmap at the requested size and return it.
        /// Note: We do not paint the frame highlight for image export.
        /// </summary>
        public Bitmap Export(Size outputSize, long timestamp)
        {
            Bitmap bmpExport = new Bitmap(outputSize.Width, outputSize.Height);
            Graphics g = Graphics.FromImage(bmpExport);
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;

            Paint(g, outputSize);
            
            // Annotations are expressed in the original frames coordinate system.
            Rectangle fitArea = UIHelper.RatioStretch(outputSize, canvasSize);
            float scale = (float)outputSize.Width / fitArea.Width;
            Point location = new Point((int)(-fitArea.X * scale), (int)(-fitArea.Y * scale));

            MetadataRenderer metadataRenderer = new MetadataRenderer(parentMetadata, true);
            metadataRenderer.Render(g, location, scale, timestamp);
            
            // Local drawings (labels).
            ImageToViewportTransformer transformer = new ImageToViewportTransformer(location, scale);
            DrawExtra(g, null, transformer, timestamp, true);

            return bmpExport;
        }

        /// <summary>
        /// Returns the aspect ratio of the kinogram.
        /// </summary>
        public float GetAspectRatio()
        {
            int cols = (int)Math.Ceiling((float)parameters.TileCount / parameters.Rows);
            Size cropSize = GetCropSize();
            Size fullSize = new Size(cropSize.Width * cols, cropSize.Height * parameters.Rows);
            return (float)fullSize.Width / fullSize.Height;
        }

        /// <summary>
        /// Return the real tile count clamped to the number of available frames.
        /// </summary>
        public int GetTileCount(int tileCount)
        {
            return Math.Min(framesContainer.Frames.Count, tileCount);
        }

        /// <summary>
        /// Return the average interval in seconds between adjacent frames of the kinogram.
        /// </summary>
        public float GetFrameInterval(int tileCount)
        {
            int maxFrames = Math.Min(framesContainer.Frames.Count, tileCount);
            float intervalFrames = (float)framesContainer.Frames.Count / maxFrames;
            float intervalTimestamp = intervalFrames * parentMetadata.AverageTimeStampsPerFrame;
            float intervalSeconds = (float)(intervalTimestamp/ parentMetadata.AverageTimeStampsPerSecond);
            return intervalSeconds;
        }
        
        /// <summary>
        /// This is called from the configuration dialog to provide live update.
        /// </summary>
        public void ConfigurationChanged(bool tileCountChanged)
        {
            if (tileCountChanged)
            {
                AfterTileCountChange();
            }
            
            Update();
        }
        
        #endregion

        #region Context menu

        /// <summary>
        /// Get the context menu according to the mouse position, current time and locale.
        /// </summary>
        public List<ToolStripItem> GetContextMenu(PointF pivot, long timestamp)
        {
            List<ToolStripItem> contextMenu = new List<ToolStripItem>();
            ReloadMenusCulture();

            contextTile = GetTile(pivot);

            contextMenu.AddRange(new ToolStripItem[] {
                mnuConfigure,
                new ToolStripSeparator(),
                mnuAction,
                mnuOptions,
                mnuMeasurement,
            });

            mnuRightToLeft.Checked = !parameters.LeftToRight;
            mnuShowBorder.Checked = parameters.BorderVisible;
            mnuAutoInterpolate.Checked = parameters.AutoInterpolate;

            return contextMenu;
        }

        private void ReloadMenusCulture()
        {
            // Just in time localization.
            mnuConfigure.Text = ScreenManagerLang.Generic_ConfigurationElipsis;
            
            mnuAction.Text = "Action";
            mnuInterpolate.Text = "Interpolate tiles";
            mnuResetLabelPositions.Text = "Reset label positions";
            mnuResetTile.Text = "Reset this tile";
            mnuResetAllTiles.Text = "Reset all tiles";

            mnuOptions.Text = "Options";
            mnuRightToLeft.Text = "Right to left";
            mnuShowBorder.Text = "Show border";
            mnuAutoInterpolate.Text = "Auto interpolate";

            // Measurement
            mnuMeasurement.Text = "Labels";
            foreach (var pair in mnuMeasureLabelTypes)
            {
                ToolStripMenuItem tsmi = pair.Value;
                MeasureLabelType measureLabelType = pair.Key;
                tsmi.Text = GetMeasureLabelOptionText(measureLabelType);
                tsmi.Checked = parameters.MeasureLabelType == measureLabelType;
            }
        }

        public string GetMeasureLabelOptionText(MeasureLabelType data)
        {
            switch (data)
            {
                case MeasureLabelType.None: return ScreenManagerLang.dlgConfigureTrajectory_ExtraData_None;

                case MeasureLabelType.Clock: return "Clock";
                case MeasureLabelType.Frame: return "Frame";
            }

            return "";
        }

        private void MnuConfigure_Click(object sender, EventArgs e)
        {
            // The dialog is responsible for handling undo/redo.

            ToolStripMenuItem tsmi = sender as ToolStripMenuItem;
            if (tsmi == null)
                return;

            IDrawingHostView host = tsmi.Tag as IDrawingHostView;
            FormConfigureKinogram fck = new FormConfigureKinogram(this, host);
            FormsHelper.Locate(fck);
            fck.ShowDialog();

            if (fck.DialogResult == DialogResult.OK)
            {   
                AfterTileCountChange();
                SaveAsDefaultParameters();
            }

            fck.Dispose();
            Update();
            InvalidateFromMenu(sender);
        }

        private void MnuInterpolate_Click(object sender, EventArgs e)
        {
            CaptureMemento();
            InterpolatePositions();
            Update();
            InvalidateFromMenu(sender);
        }

        private void MnuResetTile_Click(object sender, EventArgs e)
        {
            if (contextTile < 0 || contextTile >= parameters.CropPositions.Count)
                return;

            CaptureMemento();

            if (parameters.ManualPositions.Contains(contextTile))
                parameters.ManualPositions.Remove(contextTile);
            
            if (parameters.AutoInterpolate)
                InterpolatePositions();
            else
                parameters.CropPositions[contextTile] = PointF.Empty;

            contextTile = -1;

            Update();
            InvalidateFromMenu(sender);
        }

        private void MnuResetAllTiles_Click(object sender, EventArgs e)
        {
            CaptureMemento();
            ResetCropPositions();
            Update();
            InvalidateFromMenu(sender);
        }

        private void MnuRightToLeft_Click(object sender, EventArgs e)
        {
            CaptureMemento();

            // Inverse to toggle.
            parameters.LeftToRight = mnuRightToLeft.Checked;

            Update();
            UpdateFrameLabels(true, false);
            InvalidateFromMenu(sender);
        }

        private void MnuShowBorder_Click(object sender, EventArgs e)
        {
            CaptureMemento();

            parameters.BorderVisible = !mnuShowBorder.Checked;

            Update();
            InvalidateFromMenu(sender);
        }

        private void MnuAutoInterpolate_Click(object sender, EventArgs e)
        {
            CaptureMemento();

            parameters.AutoInterpolate = !mnuAutoInterpolate.Checked;

            if (parameters.AutoInterpolate)
            {
                // We just turned auto-interpolate on, let's interpolate.
                InterpolatePositions();
            }

            Update();
            InvalidateFromMenu(sender);
        }

        private void MnuResetLabelPositions_Click(object sender, EventArgs e)
        {
            CaptureMemento();

            UpdateFrameLabels(true, true);

            Update();
            InvalidateFromMenu(sender);
        }

        /// <summary>
        /// Create a new measure label type menu and store it in the global dictionary.
        /// </summary>
        private ToolStripMenuItem CreateMeasureLabelTypeMenu(MeasureLabelType measureLabelType)
        {
            // Note: the tag is reserved for injecting the screen user interface to support invalidation.
            ToolStripMenuItem mnu = new ToolStripMenuItem();
            mnu.Click += mnuMeasureLabelType_Click;
            mnuMeasureLabelTypes.Add(measureLabelType, mnu);
            return mnu;
        }

        private void mnuMeasureLabelType_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem tsmi = sender as ToolStripMenuItem;
            if (tsmi == null)
                return;

            MeasureLabelType measureLabelType = MeasureLabelType.None;
            foreach (var pair in mnuMeasureLabelTypes)
            {
                if (pair.Value == tsmi)
                {
                    measureLabelType = pair.Key;
                    break;
                }
            }

            parameters.MeasureLabelType = measureLabelType;
            UpdateFrameLabels(false);
            InvalidateFromMenu(tsmi);
        }

        /// <summary>
        /// Update the main viewport from a menu event handler.
        /// </summary>
        private void InvalidateFromMenu(object sender)
        {
            // The screen hook was injected inside the menu.
            ToolStripMenuItem tsmi = sender as ToolStripMenuItem;
            if (tsmi == null)
                return;

            IDrawingHostView host = tsmi.Tag as IDrawingHostView;
            if (host == null)
                return;

            host.InvalidateFromMenu();
        }
        #endregion

        #region Rendering
        /// <summary>
        /// Paint the composite or paint one tile on the internal bitmap.
        /// This is used for viewport rendering.
        /// </summary>
        private void Update(int tile = -1)
        {
            if (bitmap == null || framesContainer == null || framesContainer.Frames == null || framesContainer.Frames.Count < 1)
                return;

            Graphics g = Graphics.FromImage(bitmap);
            g.PixelOffsetMode = PixelOffsetMode.HighSpeed;
            g.CompositingQuality = CompositingQuality.HighSpeed;
            g.InterpolationMode = InterpolationMode.Bilinear;
            g.SmoothingMode = SmoothingMode.HighSpeed;

            Paint(g, canvasSize, tile);

            if (frameLabels.Count == 0)
                UpdateFrameLabels(true, true);
        }

        /// <summary>
        /// Paint the composite or paint one tile of the composite.
        /// </summary>
        private void Paint(Graphics g, Size outputSize, int tile = -1)
        { 
            float step = (float)framesContainer.Frames.Count / parameters.TileCount;
            List<VideoFrame> frames = framesContainer.Frames.Where((frame, i) => i % step < 1).ToList();

            int cols = (int)Math.Ceiling((float)parameters.TileCount / parameters.Rows);
            Size cropSize = GetCropSize();
            Size fullSize = new Size(cropSize.Width * cols, cropSize.Height * parameters.Rows);

            Rectangle paintArea = UIHelper.RatioStretch(fullSize, outputSize);
            Size tileSize = new Size(paintArea.Width / cols, paintArea.Height / parameters.Rows);

            UpdateCache(frames, cropSize, tileSize);

            if (tile >= 0)
            {
                // Render a single tile.
                DrawTile(g, cache[tile], tile, cols, paintArea, tileSize);
            }
            else
            {
                // Render the whole composite.
                
                // Viewport background.
                using (SolidBrush backgroundBrush = new SolidBrush(BackgroundColor))
                    g.FillRectangle(backgroundBrush, 0, 0, outputSize.Width, outputSize.Height);
                
                for (int i = 0; i < cache.Count; i++)
                    DrawTile(g, cache[i], i, cols, paintArea, tileSize);
            }
        }

        /// <summary>
        /// Render one tile.
        /// </summary>
        private void DrawTile(Graphics g, Bitmap image, int index, int cols, Rectangle paintArea, Size tileSize)
        {
            if (index < 0 || index >= parameters.CropPositions.Count)
                return;

            Rectangle destRect = GetDestinationRectangle(index, cols, parameters.Rows, parameters.LeftToRight, paintArea, tileSize);

            // Tile background
            using (SolidBrush b = new SolidBrush(parameters.BorderColor))
                g.FillRectangle(b, destRect);

            // Tile image.
            int x = destRect.X + (int)(-parameters.CropPositions[index].X * cacheScale);
            int y = destRect.Y + (int)(-parameters.CropPositions[index].Y * cacheScale);
            g.SetClip(destRect);
            g.DrawImageUnscaled(image, x, y);
            g.ResetClip();

            if (showDebug)
            {
                using (Font f = new Font("Arial", 10))
                using (SolidBrush brush = new SolidBrush(Color.Red))
                {
                    // crop position.
                    string info = string.Format("{0}: {1}, {2}", index, x - destRect.X, y - destRect.Y);
                    g.DrawString(info, f, brush, destRect.X + 5, destRect.Y + 5);

                    // Manual position.
                    if (parameters.ManualPositions.Contains(index))
                    {
                        PointF corner = new PointF(destRect.X + 10, destRect.Y + 20);
                        g.FillEllipse(brush, corner.Box(3));
                    }
                }
            }

            DrawBorder(g, destRect);
        }

        /// <summary>
        /// Draw the default border around the tile.
        /// </summary>
        private void DrawBorder(Graphics g, Rectangle rect)
        {
            if (!parameters.BorderVisible)
                return;
            
            using (Pen p = new Pen(parameters.BorderColor))
                g.DrawRectangle(p, new Rectangle(rect.X, rect.Y, rect.Width - 1, rect.Height - 1));
        }

        /// <summary>
        /// Find the tile corresponding to the current time and draw a border around it.
        /// </summary>
        private void DrawHighlightBorder(Graphics canvas, IImageToViewportTransformer transformer, long timestamp, List<VideoFrame> frames, int cols, Rectangle paintArea, Size tileSize)
        {
            // Draw a border around the current tile.
            // More precisely, the tile immediately before the current time.            
            int index = 0;
            foreach (VideoFrame f in frames)
            {
                if (f.Timestamp < timestamp)
                {
                    index++;
                    continue;
                }

                Rectangle destRect = GetDestinationRectangle(index, cols, parameters.Rows, parameters.LeftToRight, paintArea, tileSize);
                destRect = transformer.Transform(destRect);
                destRect.Width = destRect.Width - 1;
                destRect.Height = destRect.Height - 1;
                using (Pen p = new Pen(Color.CornflowerBlue, 2.0f))
                    canvas.DrawRectangle(p, destRect);

                break;
            }
        }

        private void DrawLabels(Graphics canvas, IImageToViewportTransformer transformer)
        {
            if (parameters.MeasureLabelType == MeasureLabelType.None)
                return;

            foreach (var label in frameLabels)
                label.Draw(canvas, transformer, 1.0f);
        }

        /// <summary>
        /// Returns the part of the target image where the passed tile should be drawn.
        /// </summary>
        private Rectangle GetDestinationRectangle(int index, int cols, int rows, bool ltr, Rectangle paintArea, Size tileSize)
        {
            int row = index / cols;
            int col = index - (row * cols);
            if (!ltr)
                col = (cols - 1) - col;

            return new Rectangle(paintArea.Left + col * tileSize.Width, paintArea.Top + row * tileSize.Height, tileSize.Width, tileSize.Height);
        }
        #endregion

        #region Frame labels
        private void UpdateFrameLabels(bool resetPositions, bool hardReset = false)
        {
            if (canvasSize == Size.Empty || framesContainer == null || framesContainer.Frames == null || framesContainer.Frames.Count == 0)
                return;

            // Reset the auto-numbers to be into the tiles.
            float step = (float)framesContainer.Frames.Count / parameters.TileCount;
            List<VideoFrame> frames = framesContainer.Frames.Where((frame, i) => i % step < 1).ToList();

            int cols = (int)Math.Ceiling((float)parameters.TileCount / parameters.Rows);
            Size cropSize = GetCropSize();
            Size fullSize = new Size(cropSize.Width * cols, cropSize.Height * parameters.Rows);
            Rectangle paintArea = UIHelper.RatioStretch(fullSize, canvasSize);
            Size tileSize = new Size(paintArea.Width / cols, paintArea.Height / parameters.Rows);
            int tileCount = Math.Min(framesContainer.Frames.Count, parameters.TileCount);

            if (resetPositions)
            {
                // Reset vs hard reset:
                // If we have the same number of labels we might be in the case where we have only changed the tile size or sequence direction.
                // In this case we keep the offset of the labels, just change their attach point.
                // This is especially important if the user has set all the labels to be bottom center for example.
                hardReset |= (frameLabels.Count != frames.Count);
                if (hardReset)
                    frameLabels.Clear();

                int index = 0;
                foreach (VideoFrame f in frames)
                {
                    // Find the destination rectangle of this tile.
                    Rectangle destRect = GetDestinationRectangle(index, cols, parameters.Rows, parameters.LeftToRight, paintArea, tileSize);

                    // Anchor in the top-left by default. 
                    PointF location = new PointF(destRect.X + 10, destRect.Y + 10);

                    if (hardReset)
                    {
                        MiniLabel label = new MiniLabel();
                        label.AttachIndex = index;
                        label.SetAttach(location, false);
                        label.SetCenter(location);
                        label.Timestamp = f.Timestamp;
                        label.BackColor = Color.Black;
                        label.ShowConnector = false;
                        frameLabels.Add(label);
                    }
                    else
                    {
                        frameLabels[index].SetAttach(location, true);
                    }

                    index++;
                }
            }

            // Update the labels text.
            if (parameters.MeasureLabelType != MeasureLabelType.None)
            {
                for (int i = 0; i < frameLabels.Count; i++)
                {
                    frameLabels[i].SetText(GetMeasureLabelText(frameLabels[i]));
                }
            }

        }

        private string GetMeasureLabelText(MiniLabel label)
        {
            string displayText = "";
            switch (parameters.MeasureLabelType)
            {
                case MeasureLabelType.None:
                    displayText = "";
                    break;

                case MeasureLabelType.Clock:
                    displayText = parentMetadata.TimeCodeBuilder(label.Timestamp, TimeType.UserOrigin, TimecodeFormat.Unknown, true);
                    break;
                case MeasureLabelType.Frame:
                    displayText = string.Format("{0}", label.AttachIndex + 1);
                    break;
                default:
                    break;
            }

            return displayText;
        }

        #endregion

        /// <summary>
        /// The input frames size has changed.
        /// </summary>
        public void UpdateSize(Size inputFrameSize)
        {
            canvasSize = rotatedCanvas ? new Size(inputFrameSize.Height, inputFrameSize.Width) : inputFrameSize;

            if (bitmap == null || bitmap.Size != canvasSize)
            {
                if (bitmap != null)
                    bitmap.Dispose();

                bitmap = new Bitmap(canvasSize.Width, canvasSize.Height);
            }

            // Redraw the kinogram and update the cache if necessary.
            Update();
            UpdateFrameLabels(true, false);
        }

        /// <summary>
        /// Add or remove crop positions slots after a change in the number of tiles.
        /// parameters.TileCount has the new number of tiles, 
        /// parameters.CropPositions has the old list of positions.
        /// This will interpolate the new positions based on the old ones.
        /// </summary>
        private void AfterTileCountChange()
        {
            // Adapt the anchor tiles (manually placed tiles) to the new list.
            // Collect where we have anchor tiles.
            List<float> anchorCoords = new List<float>();
            foreach (int index in parameters.ManualPositions)
            {
                float coord = (float)index / parameters.CropPositions.Count;
                anchorCoords.Add(coord);
            }

            // Perform the interpolation using all old tiles as anchors.
            parameters.CropPositions = Interpolate(parameters.TileCount, parameters.CropPositions);

            // Restore anchor tiles based on the new tile count.
            parameters.ManualPositions.Clear();
            foreach (float anchorCoord in anchorCoords)
            {
                int anchorIndex = (int)Math.Round(parameters.CropPositions.Count * anchorCoord);
                anchorIndex = Math.Min(anchorIndex, parameters.CropPositions.Count - 1);
                parameters.ManualPositions.Add(anchorIndex);
            }

            UpdateFrameLabels(true);
        }

        /// <summary>
        /// Save the configuration as the new preferred configuration.
        /// </summary>
        private void SaveAsDefaultParameters()
        {
            PreferencesManager.PlayerPreferences.Kinogram = parameters.Clone();
            PreferencesManager.Save();
        }

        /// <summary>
        /// Reset all crop positions to zero.
        /// </summary>
        private void ResetCropPositions()
        {
            parameters.CropPositions.Clear();
            for (int i = 0; i < parameters.TileCount; i++)
                parameters.CropPositions.Add(PointF.Empty);

            parameters.ManualPositions.Clear();
        }

        /// <summary>
        /// Get the final crop size, clamped to the original frame size.
        /// </summary>
        private Size GetCropSize()
        {
            int cropWidth = parameters.CropSize.Width;
            int cropHeight = parameters.CropSize.Height;

            float aspect = (float)cropWidth / cropHeight;
            if (cropWidth > inputFrameSize.Width)
            {
                cropWidth = inputFrameSize.Width;
                cropHeight = (int)(cropWidth / aspect);
            }

            if (cropHeight > inputFrameSize.Height)
            {
                cropHeight = inputFrameSize.Height;
                cropWidth = (int)(cropHeight * aspect);
            }

            return new Size(cropWidth, cropHeight);
        }

        /// <summary>
        /// Find the tile under this point.
        /// The point is given in the space of the input images.
        /// </summary>
        private int GetTile(PointF p)
        {
            int cols = (int)Math.Ceiling((float)parameters.TileCount / parameters.Rows);
            Size fullSize = new Size(parameters.CropSize.Width * cols, parameters.CropSize.Height * parameters.Rows);
            Rectangle paintArea = UIHelper.RatioStretch(fullSize, canvasSize);
            Size tileSize = new Size(paintArea.Width / cols, paintArea.Height / parameters.Rows);

            // Express the coordinate in the paint area.
            p = new PointF(p.X - paintArea.X, p.Y - paintArea.Y);

            if (p.X < 0 || p.Y < 0 || p.X >= paintArea.Width || p.Y >= paintArea.Height)
                return -1;

            int col = (int)(p.X / tileSize.Width);
            if (!parameters.LeftToRight)
                col = (cols - 1) - col;

            int row = (int)(p.Y / tileSize.Height);
            int index = row * cols + col;

            if (index >= framesContainer.Frames.Count)
                return -1;

            return index;
        }

        /// <summary>
        /// Move the crop position of a specific tile.
        /// </summary>
        private void MoveTile(float dx, float dy, int index)
        {
            PointF oldPosition = parameters.CropPositions[index];

            // Scale the offset so the cached image sticks to the mouse.
            float x = oldPosition.X - (dx / cacheScale);
            float y = oldPosition.Y - (dy / cacheScale);
            
            PointF newPosition = new PointF(x, y);
            parameters.CropPositions[index] = newPosition;
        }

        /// <summary>
        /// Interpolate between already positionned tiles.
        /// </summary>
        private void InterpolatePositions()
        {
            // Interpolation approach.
            // 1. The original function was interpolating between the first and last tiles.
            // In practice a strategy that worked better was to reduce the number of tiles to a few, place these tiles
            // and then expand back to the total number. Essentially interpolating between a few manually placed tiles.
            // 2. The second approach was to use non-zero position to identify tiles placed manually.
            // The issue with that was that as soon as we do one pass of interpolation we lose the information.
            // 3. The third version of the interpolation explicitly keeps track of which tile were placed manually.
            // The information is stored in parameters.ManualPositions and saved to KVA.

            // Identify and collect the anchor points (tiles placed manually) and their 1D coordinate in the sequence.
            List<PointF> oldCrops = new List<PointF>();
            List<float> coords = new List<float>();
            for (int i = 0; i < parameters.CropPositions.Count; i++)
            {
                if (parameters.ManualPositions.Contains(i))
                {
                    oldCrops.Add(parameters.CropPositions[i]);
                    coords.Add((float)i / parameters.CropPositions.Count);
                }
            }

            parameters.CropPositions = Interpolate(parameters.TileCount, oldCrops, coords);
        }

        /// <summary>
        /// Generate new positions by interpolating the old list.
        /// oldCoords contains the 1D coordinate of the old crops along the sequence.
        /// </summary>
        private List<PointF> Interpolate(int newCount, List<PointF> oldCrops, List<float> oldCoords = null)
        {
            int oldCount = oldCrops.Count;
            if (newCount == oldCount)
                return oldCrops;

            if (oldCoords == null)
            {
                oldCoords = new List<float>();
                for (int i = 0; i < oldCrops.Count; i++)
                    oldCoords.Add((float)i / oldCrops.Count);
            }

            int goodTiles = newCount;
            if (framesContainer != null && framesContainer.Frames != null && framesContainer.Frames.Count < newCount)
                goodTiles = framesContainer.Frames.Count;

            List<PointF> newCrops = new List<PointF>();
            if (oldCount == 0)
            {
                for (int i = 0; i < goodTiles; i++)
                    newCrops.Add(PointF.Empty);

                PadTiles(newCrops, parameters.TileCount);
                return newCrops;
            }

            // Interpolate the new positions to match the existing motion of the tiles within the scene.
            for (int i = 0; i < goodTiles; i++)
            {
                // 1D coord in the new sequence.    
                float t = (float)i / goodTiles;

                // Find the two closest old values and where we sit between them.
                int a = -1;
                for (int j = oldCoords.Count - 1; j >= 0; j--)
                {
                    if (t > oldCoords[j])
                    {
                        a = j;
                        break;
                    }
                }

                if (a == -1)
                {
                    // All the existing known positions are after the tile being interpolated.
                    newCrops.Add(oldCrops[0]);
                    continue;
                }

                if (a == oldCount - 1)
                {
                    // All the existing known positions are before the tile being interpolated.
                    newCrops.Add(oldCrops[oldCount-1]);
                    continue;
                }

                int b = a + 1;
                float alpha = (t - oldCoords[a]) / (oldCoords[b] - oldCoords[a]) ;
                PointF lerped = GeometryHelper.Mix(oldCrops[a], oldCrops[b], alpha);
                newCrops.Add(lerped);
            }
            
            PadTiles(newCrops, parameters.TileCount);
            return newCrops;
        }

        /// <summary>
        /// Add extra crop positions for the tiles we don't have source frames for.
        /// This happens when the table config produces more cells than there are available frames.
        /// </summary>
        private void PadTiles(List<PointF> crops, int targetCount)
        {
            int filledCount = crops.Count;
            if (filledCount == targetCount)
                return;

            for (int i = 0; i < targetCount - filledCount; i++)
                crops.Add(PointF.Empty);
        }

        /// <summary>
        /// Update the cache of pre-sized source images.
        /// This should be called whenever the source or output size change.
        /// </summary>
        private void UpdateCache(List<VideoFrame> frames, Size cropSize, Size tileSize)
        {
            // Find the size of the images such that we can draw them unscaled to the output.
            // Crop size is the source rectangle size and tileSize is the destination rectangle size.
            // They should already have the same aspect ratio.
            // Cache scale is the factor we apply to the input images to get the cached ones.
            float newCacheScale = (float)tileSize.Width / cropSize.Width;
            if (!isCacheDirty && newCacheScale == cacheScale && frames.Count == cache.Count)
                return;
            
            Size cachedSize = new Size((int)(inputFrameSize.Width * newCacheScale), (int)(inputFrameSize.Height * newCacheScale));
            log.DebugFormat("Kinogram, updating cache. Scale: {0} -> {1}", cacheScale, newCacheScale);

            ClearCache();

            foreach (var frame in frames)
            {
                Bitmap cachedFrame = new Bitmap(frame.Image, cachedSize);
                cache.Add(cachedFrame);
            }

            cacheScale = newCacheScale;
            isCacheDirty = false;
        }

        /// <summary>
        /// Clear the cache.
        /// </summary>
        private void ClearCache()
        {
            foreach (var f in cache)
                f.Dispose();

            cache.Clear();
        }
    
        private void CaptureMemento()
        {
            var memento = new HistoryMementoModifyVideoFilter(parentMetadata, VideoFilterType.Kinogram, FriendlyName);
            parentMetadata.HistoryStack.PushNewCommand(memento);
        }
    }
}
