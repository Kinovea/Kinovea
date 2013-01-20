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
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;

using Kinovea.ScreenManager.Languages;
using Kinovea.Services;
using Kinovea.Video;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// A control that let the user explore a folder.
	/// 
	/// A folder is loaded asynchronically through a background worker.
	/// We hold a list of several bgWorkers. (List ThumbListLoader)
	///	When we start loading thumbs, we try the first bgWorker,
	/// if it's currently used, we cancel it and spawn a new one.
	/// we use this new one to handle the new thumbs.
	/// This allows us to effectively cancel the display of a folder
	/// without preventing the load of the new folder.
	/// (Fixes bugs for when we change directories fastly)
	/// 
	/// Each thumbnail will be presented in a ThumbListViewItem.
	/// We first position and initialize the ThumbListViewItem,
	/// and then load them with the video data. (thumbnail from ffmpeg)
	/// through the help of the SummaryLoader who is responsible for this.
	/// </summary>
	public partial class ThumbListView : UserControl
	{
	    public event EventHandler<LoadAskedEventArgs> LoadAsked;
	    
		#region Members
		private static readonly int leftMargin = 30;
		private static readonly int rightMargin = 20;  	// Allow for potential scrollbar. This value doesn't include the last pic spacing.
		private static readonly int topMargin = 5;
		private static readonly Brush gradientBrush = new LinearGradientBrush(new Point(33, 0), new Point(350, 0), Color.LightSteelBlue, Color.White);
    	private static readonly Pen gradientPen = new Pen(gradientBrush);
    	private int columns = (int)ExplorerThumbSize.Large;
    	private object locker = new object();
		private List<SummaryLoader> loaders = new List<SummaryLoader>();
		private List<ThumbnailFile> thumbnails = new List<ThumbnailFile>();
		private ThumbnailFile selectedThumbnail;
		private bool editMode;
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		#endregion

		#region Construction 
		public ThumbListView()
		{
			log.Debug("Constructing ThumbListView");
			
			InitializeComponent();
			InitSizingButtons();
			RefreshUICulture();

			columns = (int)PreferencesManager.FileExplorerPreferences.ExplorerThumbsSize;
			DeselectAllSizingButtons();
			SelectSizingButton();
		}
		#endregion
		
		#region Public methods
		public void RefreshUICulture()
		{
			//btnHideThumbView.Text = ScreenManagerLang.btnHideThumbView;
			
			foreach(ThumbnailFile tlvi in thumbnails)
			    tlvi.RefreshUICulture();
		}
		public void DisplayThumbnails(List<String> _fileNames)
		{
		    pbFiles.Visible = false;
		    pbFiles.Value = 0;
		    
			CleanupLoaders();
			CleanupThumbnails();
			
			if (_fileNames.Count > 0)
			{
				CreateThumbs(_fileNames);
				UpdateView();
				
				SummaryLoader sl = new SummaryLoader(_fileNames);
				sl.SummaryLoaded += SummaryLoader_SummaryLoaded;
				loaders.Add(sl);
				pbFiles.Visible = true;
				sl.Run();
			}
		}
		public void StopLoading()
		{
		    CleanupLoaders();
		}
		public void CleanupThumbnails()
		{
		    selectedThumbnail = null;
		    foreach(ThumbnailFile tlvi in thumbnails)
		        tlvi.DisposeImages();
		    thumbnails.Clear();
		    splitResizeBar.Panel2.Controls.Clear();
		}
		public bool OnKeyPress(Keys _keycode)
		{
			// Method called from the Screen Manager's PreFilterMessage.
			bool bWasHandled = false;
			if(splitResizeBar.Panel2.Controls.Count > 0 && !editMode)
			{
				// Note that ESC key to cancel editing is handled directly in
				// each thumbnail item.
				switch (_keycode)
				{
					case Keys.Left:
						{
							if (selectedThumbnail == null )
							{
								((ThumbnailFile)splitResizeBar.Panel2.Controls[0]).SetSelected();
							}
							else
							{
								int index = (int)selectedThumbnail.Tag;
								int iRow = index / columns;
								int iCol = index - (iRow * columns);

								if (iCol > 0)
									((ThumbnailFile)splitResizeBar.Panel2.Controls[index - 1]).SetSelected();
							}
							break;
						}
					case Keys.Right:
						{
							if (selectedThumbnail == null)
							{
								((ThumbnailFile)splitResizeBar.Panel2.Controls[0]).SetSelected();
							}
							else
							{
								int index = (int)selectedThumbnail.Tag;
								int iRow = index / columns;
								int iCol = index - (iRow * columns);

								if (iCol < columns - 1 && index + 1 < splitResizeBar.Panel2.Controls.Count)
									((ThumbnailFile)splitResizeBar.Panel2.Controls[index + 1]).SetSelected();
							}
							break;
						}
					case Keys.Up:
						{
							if (selectedThumbnail == null)
							{
								((ThumbnailFile)splitResizeBar.Panel2.Controls[0]).SetSelected();
							}
							else
							{
								int index = (int)selectedThumbnail.Tag;
								int iRow = index / columns;
								int iCol = index - (iRow * columns);

								if (iRow > 0)
								{
									((ThumbnailFile)splitResizeBar.Panel2.Controls[index - columns]).SetSelected();
								}
							}
							splitResizeBar.Panel2.ScrollControlIntoView(selectedThumbnail);
							break;
						}
					case Keys.Down:
						{
							if (selectedThumbnail == null)
							{
								((ThumbnailFile)splitResizeBar.Panel2.Controls[0]).SetSelected();
							}
							else
							{
								int index = (int)selectedThumbnail.Tag;
								int iRow = index / columns;
								int iCol = index - (iRow * columns);

								if ((iRow < splitResizeBar.Panel2.Controls.Count / columns) && index + columns  < splitResizeBar.Panel2.Controls.Count)
								{
									((ThumbnailFile)splitResizeBar.Panel2.Controls[index + columns]).SetSelected();
								}
							}
							splitResizeBar.Panel2.ScrollControlIntoView(selectedThumbnail);
							break;
						}
					case Keys.Return:
						{
							if (selectedThumbnail != null && !selectedThumbnail.IsError && LoadAsked != null)
							    LoadAsked(this, new LoadAskedEventArgs(selectedThumbnail.FileName, -1));
							break;
						}
					case Keys.Add:
						{
							if ((ModifierKeys & Keys.Control) == Keys.Control)
								UpSizeThumbs();
							break;
						}
					case Keys.Subtract:
						{
							if ((ModifierKeys & Keys.Control) == Keys.Control)
								DownSizeThumbs();
							break;
						}
					case Keys.F2:
						{
							if(selectedThumbnail != null && !selectedThumbnail.IsError)
								selectedThumbnail.StartRenaming();
							break;
						}
					default:
						break;
				}
			}
			return bWasHandled;
		}
		#endregion
		
		#region RAM Monitoring
		/*private void TraceRamUsage(int id)
        {
            float iCurrentRam = m_RamCounter.NextValue();
            if (id >= 0)
            {
                Console.WriteLine("id:{0}, RAM: {1}", id.ToString(), m_fLastRamValue - iCurrentRam);
            }
            m_fLastRamValue = iCurrentRam;
        }
        private void InitRamCounter()
        {
            m_RamCounter = new PerformanceCounter("Memory", "Available KBytes");
            m_fLastRamValue = m_RamCounter.NextValue();
            Console.WriteLine("Initial state, Available RAM: {0}", m_fLastRamValue);
        }*/
		#endregion

		#region Organize and Display
		private void CleanupLoaders()
		{
			for(int i=loaders.Count-1;i>=0;i--)
			{
			    loaders[i].SummaryLoaded -= SummaryLoader_SummaryLoaded;
			    
				if (loaders[i].IsAlive)
					loaders[i].Cancel();
				else
				    loaders.RemoveAt(i);
			}
		}
		private void CreateThumbs(List<String> _fileNames)
		{
		    int index = 0;
		    foreach(string file in _fileNames)
			{
			    ThumbnailFile tlvi = new ThumbnailFile(file);
				tlvi.LaunchVideo += ThumbListViewItem_LaunchVideo;
				tlvi.VideoSelected += ThumbListViewItem_VideoSelected;
				tlvi.FileNameEditing += ThumbListViewItem_FileNameEditing;
				tlvi.Tag = index;
				thumbnails.Add(tlvi);
				splitResizeBar.Panel2.Controls.Add(tlvi);
				index++;
		    }
		}
		private void SummaryLoader_SummaryLoaded(object sender, SummaryLoadedEventArgs e)
		{
		    if(e.Summary == null)
		        return;
		 
		    foreach(ThumbnailFile tlvi in thumbnails)
		    {
		        if(tlvi.FileName == e.Summary.Filename)
		        {
		            tlvi.Populate(e.Summary);
		            tlvi.Invalidate();
		        }
		    }
		    
		    int done = e.Progress+1;
		    
		    log.DebugFormat("progress: {0}/{1}", done, thumbnails.Count);
		    pbFiles.Value = (int)(((float)done / thumbnails.Count) * 100);
		    
		    if(done == thumbnails.Count)
		        pbFiles.Visible = false;
		}
		private void UpSizeThumbs()
		{
			DeselectAllSizingButtons();

			switch (columns)
			{
				case (int)ExplorerThumbSize.ExtraSmall:
					columns = (int)ExplorerThumbSize.Small;
					btnSmall.BackColor = Color.LightSteelBlue;
					break;
				case (int)ExplorerThumbSize.Small:
					columns = (int)ExplorerThumbSize.Medium;
					btnMedium.BackColor = Color.LightSteelBlue;
					break;
				case (int)ExplorerThumbSize.Medium:
					columns = (int)ExplorerThumbSize.Large;
					btnLarge.BackColor = Color.LightSteelBlue;
					break;
				case (int)ExplorerThumbSize.Large:
				default:
					columns = (int)ExplorerThumbSize.ExtraLarge;
					btnExtraLarge.BackColor = Color.LightSteelBlue;
					break;
			}

			splitResizeBar.Panel2.Invalidate();
		}
		private void DownSizeThumbs()
		{
			DeselectAllSizingButtons();

			switch (columns)
			{
				case (int)ExplorerThumbSize.Small:
					columns = (int)ExplorerThumbSize.ExtraSmall;
					btnExtraSmall.BackColor = Color.LightSteelBlue;
					break;
				case (int)ExplorerThumbSize.Medium:
					columns = (int)ExplorerThumbSize.Small;
					btnSmall.BackColor = Color.LightSteelBlue;
					break;
				case (int)ExplorerThumbSize.Large:
					columns = (int)ExplorerThumbSize.Medium;
					btnMedium.BackColor = Color.LightSteelBlue;
					break;
				case (int)ExplorerThumbSize.ExtraLarge:
					columns = (int)ExplorerThumbSize.Large;
					btnLarge.BackColor = Color.LightSteelBlue;
					break;
				case (int)ExplorerThumbSize.ExtraSmall:
				default:
					columns = (int)ExplorerThumbSize.ExtraSmall;
					btnExtraSmall.BackColor = Color.LightSteelBlue;
					break;
			}

			//OrganizeThumbnailsByColumns(m_Columns);
			splitResizeBar.Panel2.Invalidate();
		}
		private void splitResizeBar_Panel2_Resize(object sender, EventArgs e)
		{
			if(this.Visible)
				UpdateView();
		}
		private void UpdateView()
		{
		    // Set location and size of each thumbnails depending on available width and options.
		    int colWidth = (splitResizeBar.Panel2.Width - leftMargin - rightMargin) / columns;
            int spacing = colWidth / 20;
		    
            int thumbWidth = colWidth - spacing;
            int thumbHeight = (thumbWidth * 3 / 4) + 15;
				
            int current = 0;
            splitResizeBar.Panel2.SuspendLayout();
		    foreach(ThumbnailFile tlvi in SortedAndFilteredThumbs())
		    {
		        tlvi.SetSize(thumbWidth, thumbHeight);
		        
		        int row = current / columns;
                int col = current - (row * columns);
                int left = col * colWidth + leftMargin;
				int top = topMargin + (row * (thumbHeight + spacing));
				tlvi.Location = new Point(left, top);
				current++;
		    }
		    splitResizeBar.Panel2.ResumeLayout();
		}
		private IEnumerable<ThumbnailFile> SortedAndFilteredThumbs()
		{
		    foreach(ThumbnailFile tlvi in thumbnails)
		        yield return tlvi;
		}
		#endregion

		#region Thumbnails items events handlers
		private void ThumbListViewItem_LaunchVideo(object sender, EventArgs e)
		{
			CancelEditMode();
			ThumbnailFile tlvi = sender as ThumbnailFile;
			
			if (tlvi != null && !tlvi.IsError && LoadAsked != null)
                LoadAsked(this, new LoadAskedEventArgs(tlvi.FileName, -1));
		}
		private void ThumbListViewItem_VideoSelected(object sender, EventArgs e)
		{
			CancelEditMode();
			ThumbnailFile tlvi = sender as ThumbnailFile;
			
			if(tlvi != null)
			{
				if (selectedThumbnail != null && selectedThumbnail != tlvi )
					selectedThumbnail.SetUnselected();
			
				selectedThumbnail = tlvi;
			}
		}
		private void ThumbListViewItem_FileNameEditing(object sender, EditingEventArgs e)
        {
			// Make sure the keyboard handling doesn't interfere 
			// if one thumbnail is in edit mode.
        	// There should only be one thumbnail in edit mode at a time.
			editMode = e.Editing;
        }
		#endregion
		
		#region Sizing Buttons
		private void InitSizingButtons()
		{
		    btnExtraSmall.Tag = 14;
			btnSmall.Tag = 10;
			btnMedium.Tag = 7;
			btnLarge.Tag = 5;
			btnExtraLarge.Tag = 4;
		}
		private void DeselectAllSizingButtons()
		{
			btnExtraSmall.BackColor = Color.SteelBlue;
			btnSmall.BackColor = Color.SteelBlue;
			btnMedium.BackColor = Color.SteelBlue;
			btnLarge.BackColor = Color.SteelBlue;
			btnExtraLarge.BackColor = Color.SteelBlue;
		}
		private void SelectSizingButton()
		{
			switch (columns)
			{
				case (int)ExplorerThumbSize.Small:
					btnSmall.BackColor = Color.LightSteelBlue;
					break;
				case (int)ExplorerThumbSize.Medium:
					btnMedium.BackColor = Color.LightSteelBlue;
					break;
				case (int)ExplorerThumbSize.Large:
					btnLarge.BackColor = Color.LightSteelBlue;
					break;
				case (int)ExplorerThumbSize.ExtraLarge:
					btnExtraLarge.BackColor = Color.LightSteelBlue;
					break;
				case (int)ExplorerThumbSize.ExtraSmall:
					btnExtraSmall.BackColor = Color.LightSteelBlue;
					break;
				default:
					break;
			}
	
		}
		private void btnSize_Click(object sender, EventArgs e)
		{
		    Button btn = sender as Button;
		    if(btn != null)
		    {
		        columns = (int)btn.Tag;
		        //OrganizeThumbnailsByColumns(m_iThumbWidth);
		        UpdateView();
    			DeselectAllSizingButtons();
    			SelectSizingButton();
    			splitResizeBar.Panel2.Invalidate();
    			SavePrefs();
		    }
		}
		#endregion

		private void SavePrefs()
		{
			PreferencesManager.FileExplorerPreferences.ExplorerThumbsSize = (ExplorerThumbSize)columns;
			PreferencesManager.Save();
		}
		
        private void Panel2MouseDown(object sender, MouseEventArgs e)
        {
        	// Clicked off nowhere.
        	if(selectedThumbnail != null)
        	{
        		selectedThumbnail.SetUnselected();
        		selectedThumbnail = null;
        	}
        	
        	CancelEditMode();
        }
        private void CancelEditMode()
        {
        	editMode = false;
        	
        	// Browse all thumbs and make sure they are all in normal mode.
        	for (int i = 0; i < splitResizeBar.Panel2.Controls.Count; i++)
			{
				ThumbnailFile tlvi = splitResizeBar.Panel2.Controls[i] as ThumbnailFile;
				if(tlvi != null)
					tlvi.CancelEditMode();
			}	
        }
        private void Panel2MouseEnter(object sender, EventArgs e)
        {
        	// Give focus to enbale mouse scroll
        	if(!editMode)
        		splitResizeBar.Panel2.Focus();
        }
        
        private void Panel2Paint(object sender, PaintEventArgs e)
        {
    		e.Graphics.DrawLine(gradientPen, 33, 0, 350, 0);
        }
	}
}
