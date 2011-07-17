/*
Copyright © Joan Charmant 2008.
joan.charmant@gmail.com 
 
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
using Microsoft.VisualBasic.FileIO;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// Thumbnail control.
	/// 
	/// </summary>
	public partial class ThumbListViewItem : UserControl
	{
		#region Events
		public delegate void LaunchVideoHandler(object sender, EventArgs e);
		public delegate void VideoSelectedHandler(object sender, EventArgs e);
		public delegate void FileNameEditingHandler(object sender, EditingEventArgs e);
		
		[Category("Action"), Browsable(true)]
		public event LaunchVideoHandler LaunchVideo;

		[Category("Action"), Browsable(true)]
		public event VideoSelectedHandler VideoSelected;
		
		[Category("Action"), Browsable(true)]
		public event FileNameEditingHandler FileNameEditing;
		#endregion
		
		#region Properties
		public string FileName
		{
			get { return m_FileName; }
			set 
			{ 
				m_FileName = value;
				lblFileName.Text = Path.GetFileNameWithoutExtension(m_FileName);
				if(m_ToolTipHandler != null)
				{
					m_ToolTipHandler.SetToolTip(picBox, Path.GetFileNameWithoutExtension(m_FileName));
				}
			}
		}
		public ToolTip ToolTipHandler
		{
			get { return m_ToolTipHandler; }
			set { m_ToolTipHandler = value; }
		}
		public bool ErrorImage
		{
			get { return m_bErrorImage; }
			set { m_bErrorImage = value; }
		}
		public List<Bitmap> Thumbnails
		{
			get { return m_Bitmaps; }	// unused.
			set
			{
				m_Bitmaps = value;
				if(m_Bitmaps != null)
				{
					if(m_Bitmaps.Count > 0)
					{
						m_iCurrentThumbnailIndex = 0;
						m_CurrentThumbnail = m_Bitmaps[m_iCurrentThumbnailIndex];
						
					}
				}
				
				SetSize(this.Width);
			}
		}
		public Bitmap Thumbnail
		{
			get { return m_CurrentThumbnail; }	// unused.
			set
			{
				m_CurrentThumbnail = value;
				SetSize(this.Width);
			}
		}
		public string Duration
		{
			get { return m_DurationText;}	// unused.
			set { m_DurationText = value;}
		}
		public Size ImageSize
		{
			set { m_ImageSize = String.Format("{0}×{1}", value.Width, value.Height);}
		}
		public bool IsImage
		{
			get { return m_bIsImage; }
			set { m_bIsImage = value; }
		}
		public bool HasKva
		{
			get { return m_bHasKva; }
			set { m_bHasKva = value; }
		}
		#endregion
		
		#region Members
		private String m_FileName;
		private ToolTip m_ToolTipHandler;
		private bool m_bIsSelected = false;
		private bool m_bErrorImage = false;
		private List<Bitmap> m_Bitmaps;
		private Bitmap m_CurrentThumbnail;
		private string m_DurationText = "0:00:00";
		private string m_ImageSize = "";
		private bool m_bIsImage;
		private bool m_bHasKva;
		private string m_ImageText;
		private int m_iCurrentThumbnailIndex;
		private bool m_Hovering;
		private Bitmap bmpKvaAnalysis = Resources.bullet_white;
		private System.Windows.Forms.Timer tmrThumbs = new System.Windows.Forms.Timer();
		private ResourceManager m_ResManager = new ResourceManager("Kinovea.ScreenManager.Languages.ScreenManagerLang", Assembly.GetExecutingAssembly());
		
		#region Context menu
		private ContextMenuStrip  popMenu = new ContextMenuStrip();
		private ToolStripMenuItem mnuLaunch = new ToolStripMenuItem();
		private ToolStripSeparator mnuSep = new ToolStripSeparator();
		private ToolStripMenuItem mnuRename = new ToolStripMenuItem();
		private ToolStripMenuItem mnuDelete = new ToolStripMenuItem();
		#endregion
		
		private bool m_bEditMode;
		
		private static readonly int m_iFilenameMaxCharacters = 18;
		private static readonly int m_iTimerInterval = 700;
		private static readonly Pen m_PenSelected = new Pen(Color.DodgerBlue, 2);
		private static readonly Pen m_PenUnselected = new Pen(Color.Silver, 2);
		private static readonly Pen m_PenShadow = new Pen(Color.Lavender, 2);
		private static readonly Font m_FontDuration = new Font("Arial", 8, FontStyle.Bold);
		private static readonly SolidBrush m_BrushQuickPreviewActive = new SolidBrush(Color.FromArgb(128, Color.SteelBlue));
		private static readonly SolidBrush m_BrushQuickPreviewInactive = new SolidBrush(Color.FromArgb(128, Color.LightSteelBlue));
		private static readonly SolidBrush m_BrushDuration = new SolidBrush(Color.FromArgb(150, Color.Black));
		private Pen m_PenDuration = new Pen(m_BrushDuration);
		#endregion
		
		#region Construction & initialization
		public ThumbListViewItem()
		{
			InitializeComponent();
			BackColor = Color.White;
			picBox.BackgroundImage = null;
			
			// Setup timer
			tmrThumbs.Interval = m_iTimerInterval;
			tmrThumbs.Tick += new EventHandler(tmrThumbs_Tick);
			m_iCurrentThumbnailIndex = 0;
			
			m_PenDuration.StartCap = LineCap.Round;
			m_PenDuration.Width = 14;
			
			// Make the editbox follow the same layout pattern than the label.
			// except that its minimal height is depending on font.
			tbFileName.Left = lblFileName.Left;
			tbFileName.Width = lblFileName.Width;
			tbFileName.Top = this.Height - tbFileName.Height;
			tbFileName.Anchor = lblFileName.Anchor;
			
			BuildContextMenus();
			RefreshUICulture();
		}
		private void BuildContextMenus()
		{
			mnuLaunch.Image = Properties.Resources.film_go;
			mnuLaunch.Click += new EventHandler(mnuLaunch_Click);
			mnuRename.Image = Properties.Resources.rename;
			mnuRename.Click += new EventHandler(mnuRename_Click);
			mnuDelete.Image = Properties.Resources.delete;
			mnuDelete.Click += new EventHandler(mnuDelete_Click);
			popMenu.Items.AddRange(new ToolStripItem[] { mnuLaunch, mnuSep, mnuRename, mnuDelete});
			this.ContextMenuStrip = popMenu;
		}
		#endregion
		
		#region Public interface
		public void SetSize(int iWidth)
		{
			// Called at init step and on resize..
			
			// Width changed due to screen resize or thumbview mode change.
			this.Width = iWidth;
			this.Height = ((this.Width * 3) / 4) + 15;
			
			// picBox is ratio strecthed.
			if(m_CurrentThumbnail != null)
			{
				int iDoubleMargin = 6;
				
				float fWidthRatio = (float)m_CurrentThumbnail.Width / (this.Width - iDoubleMargin);
				float fHeightRatio = (float)m_CurrentThumbnail.Height / (this.Height - 15 - iDoubleMargin);
				if (fWidthRatio > fHeightRatio)
				{
					picBox.Width = this.Width - iDoubleMargin;
					picBox.Height = (int)((float)m_CurrentThumbnail.Height / fWidthRatio);
				}
				else
				{
					picBox.Width = (int)((float)m_CurrentThumbnail.Width / fHeightRatio);
					picBox.Height = this.Height - 15 - iDoubleMargin;
				}
				
				// Center back.
				picBox.Left = 3 + ( this.Width - iDoubleMargin - picBox.Width ) / 2;
				picBox.Top = 3 + ( this.Height - iDoubleMargin - 15 - picBox.Height ) / 2;
			}
			else
			{
				picBox.Height = (picBox.Width * 3) / 4;
			}
			
			// File name may have to be hidden if not enough room.
			lblFileName.Visible = (this.Width >= 110);
				
			picBox.Invalidate();
		}
		public void DisplayAsError()
		{
			// Called only at init step.
			picBox.BackColor = Color.WhiteSmoke;
			lblFileName.ForeColor = Color.Silver;
			picBox.BackgroundImage = Properties.Resources.missing3;
			picBox.BackgroundImageLayout = ImageLayout.Center;
			picBox.Cursor = Cursors.No;
			m_bErrorImage = true;
			mnuLaunch.Visible = false;
			mnuSep.Visible = false;
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
			mnuLaunch.Text = ScreenManagerLang.mnuThumbnailPlay;
			mnuRename.Text = ScreenManagerLang.mnuThumbnailRename;
			mnuDelete.Text = ScreenManagerLang.mnuThumbnailDelete;
			
			// The # char is just a placeholder for a space,
		    // Because MeasureString doesn't support trailing spaces. 
		    // (see PicBoxPaint)
			m_ImageText = String.Format("{0}#", ScreenManagerLang.Generic_Image);	
			
			picBox.Invalidate();
		}
		#endregion
		
		#region UI Event Handlers
		private void ThumbListViewItem_DoubleClick(object sender, EventArgs e)
		{
			// this event handler is actually shared by all controls 
			if (LaunchVideo != null)
			{
				this.Cursor = Cursors.WaitCursor;
				LaunchVideo(this, EventArgs.Empty);
				this.Cursor = Cursors.Default;
			}
		}
		private void ThumbListViewItem_Click(object sender, EventArgs e)
		{
			// this event handler is actually shared by all controls.
			// (except for lblFilename)
			if(!m_bErrorImage)
			{
				SetSelected();
			}
		}
		private void LblFileNameClick(object sender, EventArgs e)
        {
			if(!m_bErrorImage)
			{
				if(!m_bIsSelected)
				{
					SetSelected();
				}
				else
				{
					StartRenaming();
				}
			}
        }
		private void lblFileName_TextChanged(object sender, EventArgs e)
		{
			// Re check if we need to elid it.
			if (lblFileName.Text.Length > m_iFilenameMaxCharacters)
			{
				lblFileName.Text = lblFileName.Text.Substring(0, m_iFilenameMaxCharacters) + "...";
			}
		}
		private void PicBoxPaint(object sender, PaintEventArgs e)
		{
			// Draw picture, border and duration.
			if(!m_bErrorImage && m_CurrentThumbnail != null)
			{
				// configure for speed. These are thumbnails anyway.
				e.Graphics.PixelOffsetMode = PixelOffsetMode.None; //PixelOffsetMode.HighSpeed;
				e.Graphics.CompositingQuality = CompositingQuality.HighSpeed;
				e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
				e.Graphics.SmoothingMode = SmoothingMode.HighSpeed;
				
				// Draw picture. We always draw to the whole container.
				// it is the picBox that is ratio stretched, see SetSize().
				e.Graphics.DrawImage(m_CurrentThumbnail, 0, 0, picBox.Width, picBox.Height);
				
				// Draw border.
				Pen p = m_bIsSelected?m_PenSelected:m_PenUnselected;
				e.Graphics.DrawRectangle(p, 1, 1, picBox.Width-2, picBox.Height-2);
				e.Graphics.DrawRectangle(Pens.White, 2, 2, picBox.Width-5, picBox.Height-5);
				
				// Draw quick preview rectangles.
				if(m_Hovering && m_Bitmaps != null && m_Bitmaps.Count > 1)
				{
					int rectWidth = picBox.Width / m_Bitmaps.Count;
					int rectHeight = 20;
					for(int i=0;i<m_Bitmaps.Count;i++)
					{
						if(i == m_iCurrentThumbnailIndex)
						{
							e.Graphics.FillRectangle(m_BrushQuickPreviewActive, rectWidth * i, picBox.Height - 20, rectWidth, rectHeight);	
						}
						else
						{
							e.Graphics.FillRectangle(m_BrushQuickPreviewInactive, rectWidth * i, picBox.Height - 20, rectWidth, rectHeight);	
						}						
					}
				}
				
				// Draw duration text in the corner + background.
				e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
				if(m_bIsImage)
				{
					// MeasureString doesn't support trailing spaces.
					// We used # as placeholders, remove them just before drawing.
					SizeF bgSize = e.Graphics.MeasureString(m_ImageText, m_FontDuration);
					e.Graphics.DrawLine(m_PenDuration, (float)picBox.Width - bgSize.Width - 1, 12, (float)picBox.Width - 4, 12);
					e.Graphics.DrawString(m_ImageText.Replace('#', ' '), m_FontDuration, Brushes.White, (float)picBox.Width - bgSize.Width - 3, 5);
				}
				else
				{
					SizeF bgSize = e.Graphics.MeasureString(m_DurationText, m_FontDuration);
					e.Graphics.DrawLine(m_PenDuration, (float)picBox.Width - bgSize.Width - 1, 12, (float)picBox.Width - 4, 12);
					e.Graphics.DrawString(m_DurationText, m_FontDuration, Brushes.White, (float)picBox.Width - bgSize.Width - 3, 5);
				}
				
				// Draw image size				
				SizeF bgSize2 = e.Graphics.MeasureString(m_ImageSize, m_FontDuration);
				int sizeTop = 29;
				e.Graphics.DrawLine(m_PenDuration, (float)picBox.Width - bgSize2.Width - 1, sizeTop, (float)picBox.Width - 4, sizeTop);
				e.Graphics.DrawString(m_ImageSize, m_FontDuration, Brushes.White, (float)picBox.Width - bgSize2.Width - 3, sizeTop - 7);
				
				// Draw KVA file indicator
				if(m_bHasKva)
				{
					e.Graphics.DrawLine(m_PenDuration, (float)picBox.Width - 20, 45, (float)picBox.Width - 4, 45);
					e.Graphics.DrawImage(bmpKvaAnalysis, picBox.Width - 25, 38);
				}
			}
		}
		private void PicBoxMouseMove(object sender, MouseEventArgs e)
        {
        	if(!m_bErrorImage && m_Bitmaps != null)
        	{
        		if(m_Bitmaps.Count > 0)
        		{
        			if(e.Y > picBox.Height - 20)
		        	{
		        		tmrThumbs.Stop();
		        		int index = e.X / (picBox.Width / m_Bitmaps.Count);
		        		m_iCurrentThumbnailIndex = Math.Max(Math.Min(index, m_Bitmaps.Count - 1), 0);
			  			m_CurrentThumbnail = m_Bitmaps[m_iCurrentThumbnailIndex];
			  			picBox.Invalidate();
		        	}
		        	else
		        	{
			  			tmrThumbs.Start();
		        	}
        		}
        	}
        	
        }
		private void ThumbListViewItemPaint(object sender, PaintEventArgs e)
		{
			// Draw the shadow
			e.Graphics.DrawLine(m_PenShadow, picBox.Left + picBox.Width + 1, picBox.Top + m_PenShadow.Width, picBox.Left + picBox.Width + 1, picBox.Top + picBox.Height + m_PenShadow.Width);
			e.Graphics.DrawLine(m_PenShadow, picBox.Left + m_PenShadow.Width, picBox.Top + picBox.Height + 1, picBox.Left + m_PenShadow.Width + picBox.Width, picBox.Top + picBox.Height + 1);
		}
		private void tmrThumbs_Tick(object sender, EventArgs e) 
		{
			// This event occur when the user has been staying for a while on the same thumbnail.
			// Loop between all stored images.
			if(!m_bErrorImage && m_Bitmaps != null)
			{
				if(m_Bitmaps.Count > 1)
				{
		  			// Change the thumbnail displayed.
		  			m_iCurrentThumbnailIndex++;
		  			if(m_iCurrentThumbnailIndex >= m_Bitmaps.Count)
		  			{
		  				m_iCurrentThumbnailIndex = 0;
		  			}
		  			
	  				m_CurrentThumbnail = m_Bitmaps[m_iCurrentThumbnailIndex];
	  				picBox.Invalidate();
	  			}
			}
		}
		private void PicBoxMouseEnter(object sender, EventArgs e)
        {
			m_Hovering = true;
		
			if(!m_bErrorImage && m_Bitmaps != null)
			{
				if(m_Bitmaps.Count > 1)
		  		{
					// Instantly change image
		  			m_iCurrentThumbnailIndex = 1;
		  			m_CurrentThumbnail = m_Bitmaps[m_iCurrentThumbnailIndex];
		  			picBox.Invalidate();

		  			// Then start timer to slideshow.
		  			tmrThumbs.Start();
		  		}
			}
				
        }
		private void PicBoxMouseLeave(object sender, EventArgs e)
        {
			m_Hovering = false;
			
        	if(!m_bErrorImage && m_Bitmaps != null)
        	{
				tmrThumbs.Stop();
				if(m_Bitmaps.Count > 0)
				{
					m_iCurrentThumbnailIndex = 0;
					m_CurrentThumbnail = m_Bitmaps[m_iCurrentThumbnailIndex];
	  				picBox.Invalidate();	
				}
        	}
        }
		private void TbFileNameKeyPress(object sender, KeyPressEventArgs e)
        {
			// editing a file name.
			
        	if (e.KeyChar == 13) // Carriage Return.
			{
        		string newFileName = Path.GetDirectoryName(m_FileName) + "\\" + tbFileName.Text;				
        		
        		// Prevent overwriting.
        		if(File.Exists(m_FileName) && !File.Exists(newFileName) && newFileName.Length > 5)
        		{
        			// Try to change the filename 
					try
        			{
						File.Move(m_FileName, newFileName);
						
		        		// If renaming went fine, consolidate the file name.
		        		if(!File.Exists(m_FileName))
		        		{
		        			FileName = newFileName;
		        		}   
		        		
		        		// Ask the Explorer tree to refresh itself...
						// But not the thumbnails pane.
			            DelegatesPool dp = DelegatesPool.Instance();
			            if (dp.RefreshFileExplorer != null)
			            {
			                dp.RefreshFileExplorer(false);
			            }
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
        	else if(e.KeyChar == 27) // Escape.
        	{
        		QuitEditMode();
        	}
        }
		#endregion
		
		#region Menu Event Handlers
		private void mnuRename_Click(object sender, EventArgs e)
		{
			StartRenaming();
		}
		private void mnuDelete_Click(object sender, EventArgs e)
		{
			// Use the built-in dialogs to confirm (or not).
			// Delete is done through moving to recycle bin.
			try
			{
				FileSystem.DeleteFile(m_FileName, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
			}
			catch(OperationCanceledException)
			{
				// User cancelled confirmation box.
			}

			// Other possible error case: the file couldn't be deleted because it's still in use.
			
			// If file was effectively moved to trash, reload the folder.
			if(!File.Exists(m_FileName))
			{
				// Ask the Explorer tree to refresh itself...
				// This will in turn refresh the thumbnails pane.
	            DelegatesPool dp = DelegatesPool.Instance();
	            if (dp.RefreshFileExplorer != null)
	            {
	                dp.RefreshFileExplorer(true);
	            }
			}
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
			{
    			FileNameEditing(this, new EditingEventArgs(false));
    		}
    		
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
        
	}
	
	#region EventArgs classe used here
    /// <summary>
    /// A (very) simple event args class to encapsulate the state of the editing.
    /// </summary>
    public class EditingEventArgs : EventArgs
	{
    	public bool Editing
		{
			get { return m_bEditing; }
		}
		
    	private readonly bool m_bEditing;

		public EditingEventArgs( bool _bEditing )
		{
			m_bEditing = _bEditing;
		}
	}
    #endregion
}
