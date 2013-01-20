#region License
/*
Copyright © Joan Charmant 2013.
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
#endregion
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// A thumbnail viewer for files.
    /// Used for explorer and shortcuts content.
    /// </summary>
    public partial class ThumbnailViewerFiles : UserControl
    {
        public event EventHandler<LoadAskedEventArgs> LoadAsked;
        public event ProgressChangedEventHandler ProgressChanged;
        public event EventHandler BeforeLoad;
        public event EventHandler AfterLoad;
        
        #region Members
        private static readonly int leftMargin = 30;
		private static readonly int rightMargin = 20;  	// Allow for potential scrollbar. This value doesn't include the last pic spacing.
		private static readonly int topMargin = 5;
		private int columns = (int)ExplorerThumbSize.Large;
    	private object locker = new object();
		private List<SummaryLoader> loaders = new List<SummaryLoader>();
		private List<ThumbListViewItem> thumbnails = new List<ThumbListViewItem>();
		private ThumbListViewItem selectedThumbnail;
		private bool editMode;
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        public ThumbnailViewerFiles()
        {
            log.Debug("Constructing ThumbListView");
			
			InitializeComponent();
			RefreshUICulture();

			columns = (int)PreferencesManager.FileExplorerPreferences.ExplorerThumbsSize;
			
            this.Dock = DockStyle.Fill;
        }
        
        #region Public methods
        public void CurrentDirectoryChanged(List<string> files)
        {
            CleanupLoaders();
            Clear();
            
            if (files.Count > 0)
            {
                CreateThumbs(files);
                UpdateView();
            
                SummaryLoader sl = new SummaryLoader(files);
                sl.SummaryLoaded += SummaryLoader_SummaryLoaded;
                loaders.Add(sl);
                if(BeforeLoad != null)
                    BeforeLoad(this, EventArgs.Empty);
                    
                sl.Run();
            }
        }
        public void CancelLoading()
        {
            if(AfterLoad != null)
                AfterLoad(this, EventArgs.Empty);

            CleanupLoaders();
        }
        public void Clear()
        {
            selectedThumbnail = null;
            foreach(ThumbListViewItem tlvi in thumbnails)
                tlvi.DisposeImages();
            
            thumbnails.Clear();
            this.Controls.Clear();
        }
        public bool OnKeyPress(Keys _keycode)
        {
            return false;
            // Method called from the Screen Manager's PreFilterMessage.
            /*bool bWasHandled = false;
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
            ((ThumbListViewItem)splitResizeBar.Panel2.Controls[0]).SetSelected();
            }
            else
            {
            int index = (int)selectedThumbnail.Tag;
            int iRow = index / columns;
            int iCol = index - (iRow * columns);
            
            if (iCol > 0)
            ((ThumbListViewItem)splitResizeBar.Panel2.Controls[index - 1]).SetSelected();
            }
            break;
            }
            case Keys.Right:
            {
            if (selectedThumbnail == null)
            {
            ((ThumbListViewItem)splitResizeBar.Panel2.Controls[0]).SetSelected();
            }
            else
            {
            int index = (int)selectedThumbnail.Tag;
            int iRow = index / columns;
            int iCol = index - (iRow * columns);
            
            if (iCol < columns - 1 && index + 1 < splitResizeBar.Panel2.Controls.Count)
            ((ThumbListViewItem)splitResizeBar.Panel2.Controls[index + 1]).SetSelected();
            }
            break;
            }
            case Keys.Up:
            {
            if (selectedThumbnail == null)
            {
            ((ThumbListViewItem)splitResizeBar.Panel2.Controls[0]).SetSelected();
            }
            else
            {
            int index = (int)selectedThumbnail.Tag;
            int iRow = index / columns;
            int iCol = index - (iRow * columns);
            
            if (iRow > 0)
            {
            ((ThumbListViewItem)splitResizeBar.Panel2.Controls[index - columns]).SetSelected();
            }
            }
            splitResizeBar.Panel2.ScrollControlIntoView(selectedThumbnail);
            break;
            }
            case Keys.Down:
            {
            if (selectedThumbnail == null)
            {
            ((ThumbListViewItem)splitResizeBar.Panel2.Controls[0]).SetSelected();
            }
            else
            {
            int index = (int)selectedThumbnail.Tag;
            int iRow = index / columns;
            int iCol = index - (iRow * columns);
            
            if ((iRow < splitResizeBar.Panel2.Controls.Count / columns) && index + columns  < splitResizeBar.Panel2.Controls.Count)
            {
            ((ThumbListViewItem)splitResizeBar.Panel2.Controls[index + columns]).SetSelected();
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
            return bWasHandled;*/
        }
        public void RefreshUICulture()
		{
			foreach(ThumbListViewItem tlvi in thumbnails)
			    tlvi.RefreshUICulture();
		}
        #endregion
        
        #region Private methods
        
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
			    ThumbListViewItem tlvi = new ThumbListViewItem(file);
				tlvi.LaunchVideo += ThumbListViewItem_LaunchVideo;
				tlvi.VideoSelected += ThumbListViewItem_VideoSelected;
				tlvi.FileNameEditing += ThumbListViewItem_FileNameEditing;
				tlvi.Tag = index;
				thumbnails.Add(tlvi);
				this.Controls.Add(tlvi);
				index++;
		    }
		}
		private void SummaryLoader_SummaryLoaded(object sender, SummaryLoadedEventArgs e)
		{
            // One of the summaries was loaded, push it into a thumbnail.
            
		    if(e.Summary == null)
		        return;
		 
            // TODO: keep the controls in a dictionary indexed by the filename instead of a raw list.
		    foreach(ThumbListViewItem tlvi in thumbnails)
		    {
		        if(tlvi.FileName == e.Summary.Filename)
		        {
		            tlvi.Populate(e.Summary);
		            tlvi.Invalidate();
		        }
		    }
		    
		    int done = e.Progress+1;
		    int percentage = (int)(((float)done / thumbnails.Count) * 100);
		    
		    if(ProgressChanged != null)
		        ProgressChanged(this, new ProgressChangedEventArgs(percentage, null));
		    
		    if(done == thumbnails.Count && AfterLoad != null)
		        AfterLoad(this, EventArgs.Empty);
		}
		
		/*private void UpSizeThumbs()
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
		}*/
		/*private void DownSizeThumbs()
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
		}*/
		/*private void splitResizeBar_Panel2_Resize(object sender, EventArgs e)
		{
			if(this.Visible)
				UpdateView();
		}*/
		private void UpdateView()
		{
		    // Set location and size of each thumbnails depending on available width and options.
		    int colWidth = (this.Width - leftMargin - rightMargin) / columns;
            int spacing = colWidth / 20;
		    
            int thumbWidth = colWidth - spacing;
            int thumbHeight = (thumbWidth * 3 / 4) + 15;
				
            int current = 0;
            this.SuspendLayout();
		    foreach(ThumbListViewItem tlvi in SortedAndFilteredThumbs())
		    {
		        tlvi.SetSize(thumbWidth, thumbHeight);
		        
		        int row = current / columns;
                int col = current - (row * columns);
                int left = col * colWidth + leftMargin;
				int top = topMargin + (row * (thumbHeight + spacing));
				tlvi.Location = new Point(left, top);
				current++;
		    }
		    this.ResumeLayout();
		}
		private IEnumerable<ThumbListViewItem> SortedAndFilteredThumbs()
		{
		    foreach(ThumbListViewItem tlvi in thumbnails)
		        yield return tlvi;
		}
		#endregion

		#region Thumbnails items events handlers
		private void ThumbListViewItem_LaunchVideo(object sender, EventArgs e)
		{
			CancelEditMode();
			ThumbListViewItem tlvi = sender as ThumbListViewItem;
			
			if (tlvi != null && !tlvi.IsError && LoadAsked != null)
                LoadAsked(this, new LoadAskedEventArgs(tlvi.FileName, -1));
		}
		private void ThumbListViewItem_VideoSelected(object sender, EventArgs e)
		{
			CancelEditMode();
			ThumbListViewItem tlvi = sender as ThumbListViewItem;
			
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
        	for (int i = 0; i < this.Controls.Count; i++)
			{
				ThumbListViewItem tlvi = this.Controls[i] as ThumbListViewItem;
				if(tlvi != null)
					tlvi.CancelEditMode();
			}	
        }
        private void Panel2MouseEnter(object sender, EventArgs e)
        {
        	// Give focus to enbale mouse scroll
        	//if(!editMode)
        	//	this.Focus();
        }
        #endregion
        
    }
}
