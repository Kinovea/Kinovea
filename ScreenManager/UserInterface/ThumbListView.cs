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
		#region Members
		private int m_iLeftMargin = 30;
		private int m_iRightMargin = 20;  	// Allow for potential scrollbar. This value doesn't include the last pic spacing.
		private int m_iTopMargin = 5;
		private int m_iVertSpacing = 20;
		private int m_Columns = (int)ExplorerThumbSize.Large;
		private static readonly Brush m_GradientBrush = new LinearGradientBrush(new Point(33, 0), new Point(350, 0), Color.LightSteelBlue, Color.White);
    	private static readonly Pen m_GradientPen = new Pen(m_GradientBrush);
    	private object m_Locker = new object();

		private List<SummaryLoader> m_Loaders = new List<SummaryLoader>();
		private List<ThumbListViewItem> m_Thumbnails = new List<ThumbListViewItem>();
		private ThumbListViewItem m_SelectedThumbnail;
				
		private bool m_bEditMode;
		private IScreenManagerUIContainer m_ScreenManagerUIContainer;
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		#endregion

		#region Construction & initialization
		public ThumbListView()
		{
			log.Debug("Constructing ThumbListView");
			
			InitializeComponent();
			InitSizingButtons();
			RefreshUICulture();

			m_Columns = (int)PreferencesManager.FileExplorerPreferences.ExplorerThumbsSize;
			DeselectAllSizingButtons();
			SelectSizingButton();
		}
		#endregion
		
		public void SetScreenManagerUIContainer(IScreenManagerUIContainer _value)
		{
			m_ScreenManagerUIContainer = _value;
		}
		public void RefreshUICulture()
		{
			//btnHideThumbView.Text = ScreenManagerLang.btnHideThumbView;
			
			foreach(ThumbListViewItem tlvi in m_Thumbnails)
			    tlvi.RefreshUICulture();
		}
		
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
				m_Loaders.Add(sl);
				pbFiles.Visible = true;
				sl.Run();
			}
		}
		public void StopLoading()
		{
		    CleanupLoaders();
		}
		private void CleanupLoaders()
		{
			for(int i=m_Loaders.Count-1;i>=0;i--)
			{
			    m_Loaders[i].SummaryLoaded -= SummaryLoader_SummaryLoaded;
			    
				if (m_Loaders[i].IsAlive)
					m_Loaders[i].Cancel();
				else
				    m_Loaders.RemoveAt(i);
			}
		}
		public void CleanupThumbnails()
		{
		    m_SelectedThumbnail = null;
		    foreach(ThumbListViewItem tlvi in m_Thumbnails)
		        tlvi.DisposeImages();
		    m_Thumbnails.Clear();
		    splitResizeBar.Panel2.Controls.Clear();
		}
		
		
		private void CreateThumbs(List<String> _fileNames)
		{
		    foreach(string file in _fileNames)
			{
			    ThumbListViewItem tlvi = new ThumbListViewItem(file);
				tlvi.LaunchVideo += ThumbListViewItem_LaunchVideo;
				tlvi.VideoSelected += ThumbListViewItem_VideoSelected;
				tlvi.FileNameEditing += ThumbListViewItem_FileNameEditing;
				m_Thumbnails.Add(tlvi);
				splitResizeBar.Panel2.Controls.Add(tlvi);
		    }
		}
		private void SummaryLoader_SummaryLoaded(object sender, SummaryLoadedEventArgs e)
		{
		    if(e.Summary == null)
		        return;
		 
		    foreach(ThumbListViewItem tlvi in m_Thumbnails)
		    {
		        if(tlvi.FileName == e.Summary.Filename)
		        {
		            tlvi.Populate(e.Summary);
		            tlvi.Invalidate();
		        }
		    }
		    
		    int done = e.Progress+1;
		    
		    log.DebugFormat("progress: {0}/{1}", done, m_Thumbnails.Count);
		    pbFiles.Value = (int)(((float)done / m_Thumbnails.Count) * 100);
		    
		    if(done == m_Thumbnails.Count)
		        pbFiles.Visible = false;
		}
		private void UpSizeThumbs()
		{
			DeselectAllSizingButtons();

			switch (m_Columns)
			{
				case (int)ExplorerThumbSize.ExtraSmall:
					m_Columns = (int)ExplorerThumbSize.Small;
					btnSmall.BackColor = Color.LightSteelBlue;
					break;
				case (int)ExplorerThumbSize.Small:
					m_Columns = (int)ExplorerThumbSize.Medium;
					btnMedium.BackColor = Color.LightSteelBlue;
					break;
				case (int)ExplorerThumbSize.Medium:
					m_Columns = (int)ExplorerThumbSize.Large;
					btnLarge.BackColor = Color.LightSteelBlue;
					break;
				case (int)ExplorerThumbSize.Large:
				default:
					m_Columns = (int)ExplorerThumbSize.ExtraLarge;
					btnExtraLarge.BackColor = Color.LightSteelBlue;
					break;
			}

			splitResizeBar.Panel2.Invalidate();
		}
		private void DownSizeThumbs()
		{
			DeselectAllSizingButtons();

			switch (m_Columns)
			{
				case (int)ExplorerThumbSize.Small:
					m_Columns = (int)ExplorerThumbSize.ExtraSmall;
					btnExtraSmall.BackColor = Color.LightSteelBlue;
					break;
				case (int)ExplorerThumbSize.Medium:
					m_Columns = (int)ExplorerThumbSize.Small;
					btnSmall.BackColor = Color.LightSteelBlue;
					break;
				case (int)ExplorerThumbSize.Large:
					m_Columns = (int)ExplorerThumbSize.Medium;
					btnMedium.BackColor = Color.LightSteelBlue;
					break;
				case (int)ExplorerThumbSize.ExtraLarge:
					m_Columns = (int)ExplorerThumbSize.Large;
					btnLarge.BackColor = Color.LightSteelBlue;
					break;
				case (int)ExplorerThumbSize.ExtraSmall:
				default:
					m_Columns = (int)ExplorerThumbSize.ExtraSmall;
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
		    int columns = m_Columns;
		    int colWidth = (splitResizeBar.Panel2.Width - m_iLeftMargin - m_iRightMargin) / columns;
            int horzSpacing = colWidth / 20;
		    int vertSpacing = horzSpacing;
            
            int thumbWidth = colWidth - horzSpacing;
            int thumbHeight = (thumbWidth * 3 / 4) + 15;
				
            int current = 0;
            splitResizeBar.Panel2.SuspendLayout();
		    foreach(ThumbListViewItem tlvi in SortedAndFilteredThumbs())
		    {
		        tlvi.SetSize(thumbWidth, thumbHeight);
		        
		        int row = current / columns;
                int col = current - (row * columns);
                int left = col * colWidth + m_iLeftMargin;
				int top = m_iTopMargin + (row * (thumbHeight + m_iVertSpacing));
				tlvi.Location = new Point(left, top);
				current++;
		    }
		    splitResizeBar.Panel2.ResumeLayout();
		}
		private IEnumerable<ThumbListViewItem> SortedAndFilteredThumbs()
		{
		    foreach(ThumbListViewItem tlvi in m_Thumbnails)
		        yield return tlvi;
		}
		#endregion

		#region Thumbnails items events handlers
		private void ThumbListViewItem_LaunchVideo(object sender, EventArgs e)
		{
			CancelEditMode();
			ThumbListViewItem tlvi = sender as ThumbListViewItem;
			
			if (tlvi != null && !tlvi.IsError)
			{
				m_ScreenManagerUIContainer.DropLoadMovie(tlvi.FileName, -1);
			}
		}
		private void ThumbListViewItem_VideoSelected(object sender, EventArgs e)
		{
			CancelEditMode();
			ThumbListViewItem tlvi = sender as ThumbListViewItem;
			
			if(tlvi != null)
			{
				if (m_SelectedThumbnail != null && m_SelectedThumbnail != tlvi )
				{
					m_SelectedThumbnail.SetUnselected();
				}
			
				m_SelectedThumbnail = tlvi;
			}
		}
		private void ThumbListViewItem_FileNameEditing(object sender, EditingEventArgs e)
        {
			// Make sure the keyboard handling doesn't interfere 
			// if one thumbnail is in edit mode.
        	// There should only be one thumbnail in edit mode at a time.
			m_bEditMode = e.Editing;
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
			switch (m_Columns)
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
		        m_Columns = (int)btn.Tag;
		        //OrganizeThumbnailsByColumns(m_iThumbWidth);
		        UpdateView();
    			DeselectAllSizingButtons();
    			SelectSizingButton();
    			splitResizeBar.Panel2.Invalidate();
    			SavePrefs();
		    }
		}
		#endregion

		#region Closing
		private void SavePrefs()
		{
			PreferencesManager.FileExplorerPreferences.ExplorerThumbsSize = (ExplorerThumbSize)m_Columns;
			PreferencesManager.Save();
		}
		#endregion

		#region Keyboard Handling
		public bool OnKeyPress(Keys _keycode)
		{
			// Method called from the Screen Manager's PreFilterMessage.
			bool bWasHandled = false;
			if(splitResizeBar.Panel2.Controls.Count > 0 && !m_bEditMode)
			{
				// Note that ESC key to cancel editing is handled directly in
				// each thumbnail item.
				switch (_keycode)
				{
					case Keys.Left:
						{
							if (m_SelectedThumbnail == null)
							{
								((ThumbListViewItem)splitResizeBar.Panel2.Controls[0]).SetSelected();
							}
							else
							{
								int index = (int)m_SelectedThumbnail.Tag;
								int iRow = index / m_Columns;
								int iCol = index - (iRow * m_Columns);

								if (iCol > 0)
								{
									((ThumbListViewItem)splitResizeBar.Panel2.Controls[index - 1]).SetSelected();
								}
							}
							break;
						}
					case Keys.Right:
						{
							if (m_SelectedThumbnail == null)
							{
								((ThumbListViewItem)splitResizeBar.Panel2.Controls[0]).SetSelected();
							}
							else
							{
								int index = (int)m_SelectedThumbnail.Tag;
								int iRow = index / m_Columns;
								int iCol = index - (iRow * m_Columns);

								if (iCol < m_Columns - 1 && index + 1 < splitResizeBar.Panel2.Controls.Count)
								{
									((ThumbListViewItem)splitResizeBar.Panel2.Controls[index + 1]).SetSelected();
								}
							}
							break;
						}
					case Keys.Up:
						{
							if (m_SelectedThumbnail == null)
							{
								((ThumbListViewItem)splitResizeBar.Panel2.Controls[0]).SetSelected();
							}
							else
							{
								int index = (int)m_SelectedThumbnail.Tag;
								int iRow = index / m_Columns;
								int iCol = index - (iRow * m_Columns);

								if (iRow > 0)
								{
									((ThumbListViewItem)splitResizeBar.Panel2.Controls[index - m_Columns]).SetSelected();
								}
							}
							splitResizeBar.Panel2.ScrollControlIntoView(m_SelectedThumbnail);
							break;
						}
					case Keys.Down:
						{
							if (m_SelectedThumbnail == null)
							{
								((ThumbListViewItem)splitResizeBar.Panel2.Controls[0]).SetSelected();
							}
							else
							{
								int index = (int)m_SelectedThumbnail.Tag;
								int iRow = index / m_Columns;
								int iCol = index - (iRow * m_Columns);

								if ((iRow < splitResizeBar.Panel2.Controls.Count / m_Columns) && index + m_Columns  < splitResizeBar.Panel2.Controls.Count)
								{
									((ThumbListViewItem)splitResizeBar.Panel2.Controls[index + m_Columns]).SetSelected();
								}
							}
							splitResizeBar.Panel2.ScrollControlIntoView(m_SelectedThumbnail);
							break;
						}
					case Keys.Return:
						{
							if (m_SelectedThumbnail != null && !m_SelectedThumbnail.IsError)
								m_ScreenManagerUIContainer.DropLoadMovie(m_SelectedThumbnail.FileName, -1);
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
							if(m_SelectedThumbnail != null && !m_SelectedThumbnail.IsError)
								m_SelectedThumbnail.StartRenaming();
							break;
						}
					default:
						break;
				}
			}
			return bWasHandled;
		}
		#endregion
		
        private void Panel2MouseDown(object sender, MouseEventArgs e)
        {
        	// Clicked off nowhere.
        	if(m_SelectedThumbnail != null)
        	{
        		m_SelectedThumbnail.SetUnselected();
        		m_SelectedThumbnail = null;
        	}
        	
        	CancelEditMode();
        }
        private void CancelEditMode()
        {
        	m_bEditMode = false;
        	
        	// Browse all thumbs and make sure they are all in normal mode.
        	for (int i = 0; i < splitResizeBar.Panel2.Controls.Count; i++)
			{
				ThumbListViewItem tlvi = splitResizeBar.Panel2.Controls[i] as ThumbListViewItem;
				if(tlvi != null)
					tlvi.CancelEditMode();
			}	
        }
        private void Panel2MouseEnter(object sender, EventArgs e)
        {
        	// Give focus to enbale mouse scroll
        	if(!m_bEditMode)
        		splitResizeBar.Panel2.Focus();
        }
        
        private void Panel2Paint(object sender, PaintEventArgs e)
        {
    		e.Graphics.DrawLine(m_GradientPen, 33, 0, 350, 0);
        }
	}
}
