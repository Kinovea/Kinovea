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
using Kinovea.VideoFiles;

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
	/// through the help of the ThumbListLoader who is responsible for this.
	/// </summary>
	public partial class ThumbListView : UserControl
	{
		#region EventDelegates
		// Types
		public delegate void DelegateClosing(object sender);

		// Events
		[Category("Action"), Browsable(true)]
		public event DelegateClosing Closing;
		#endregion
		
		#region Properties
		public IScreenManagerUIContainer ScreenManagerUIContainer
		{
			set { m_ScreenManagerUIContainer = value; }
		}
		#endregion

		#region Members
		private VideoFile m_VideoFile = new VideoFile();

		private int m_iLeftMargin = 30;
		private int m_iRightMargin = 20;  	// Allow for potential scrollbar. This value doesn't include the last pic spacing.
		private int m_iTopMargin = 5;
		private int m_iHorzSpacing = 20;   	// Right placed and respected even for the last column.
		private int m_iVertSpacing = 20;
		private int m_iCurrentSize = (int)ExplorerThumbSizes.Large;
		private static readonly Brush m_GradientBrush = new LinearGradientBrush(new Point(33, 0), new Point(350, 0), Color.LightSteelBlue, Color.White);
    	private static readonly Pen m_GradientPen = new Pen(m_GradientBrush);

		private List<ThumbListLoader> m_Loaders = new List<ThumbListLoader>();
		private ThumbListViewItem m_SelectedVideo;
				
		private bool m_bEditMode;
		private IScreenManagerUIContainer m_ScreenManagerUIContainer;
		private PreferencesManager m_PreferencesManager = PreferencesManager.Instance();
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		#endregion

		#region Construction & initialization
		public ThumbListView()
		{
			log.Debug("Constructing ThumbListView");
			
			InitializeComponent();

			RefreshUICulture();

			m_iCurrentSize = (int)m_PreferencesManager.ExplorerThumbsSize;
			DeselectAllSizingButtons();
			SelectSizingButton();
		}
		#endregion
		
		public void RefreshUICulture()
		{
			btnHideThumbView.Text = ScreenManagerLang.btnHideThumbView;
			
			// Refresh all thumbnails.
			for (int i = 0; i < splitResizeBar.Panel2.Controls.Count; i++)
			{
				ThumbListViewItem tlvi = splitResizeBar.Panel2.Controls[i] as ThumbListViewItem;
				if(tlvi != null)
				{
					tlvi.RefreshUICulture();
				}
			}
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
			// Remove loaders that completed loading or cancellation.
			CleanupLoaders();
			log.Debug(String.Format("New set of files asked, currently having {0} loaders", m_Loaders.Count));

			// Cancel remaining ones.
			foreach (ThumbListLoader loader in m_Loaders)
			{
				loader.Cancel();
			}

			if (_fileNames.Count > 0)
			{
				// Reset display for new files.
				SetupPlaceHolders(_fileNames);

				// Create the new loader and launch it.
				ThumbListLoader tll = new ThumbListLoader(_fileNames, splitResizeBar.Panel2, m_VideoFile);
				m_Loaders.Add(tll);
				tll.Launch();
			}
		}
		public void CleanupThumbnails()
		{
			// Remove the controls and deallocate any ressources used.
			for (int iCtrl = splitResizeBar.Panel2.Controls.Count - 1; iCtrl >= 0; iCtrl--)
			{
				ThumbListViewItem tlvi = splitResizeBar.Panel2.Controls[iCtrl] as ThumbListViewItem;
				if(tlvi != null)
				{
					Image bmp = tlvi.picBox.BackgroundImage;
					splitResizeBar.Panel2.Controls.RemoveAt(iCtrl);
					
					if (!tlvi.ErrorImage)
					{
						if(bmp != null)
							bmp.Dispose();
					}
				}
			}

			m_SelectedVideo = null;
		}
		
		private void CleanupLoaders()
		{
			// Remove loaders that completed loading or cancellation.
			for(int i= m_Loaders.Count-1;i>=0;i--)
			{
				if (m_Loaders[i].Unused)
				{
					m_Loaders.RemoveAt(i);
				}
			}
		}
		private void SetupPlaceHolders(List<String> _fileNames)
		{
			//-----------------------------------------------------------
			// Creates a list of thumb boxes to hold this folder's thumbs
			// They will be turned visible only when
			// they are loaded with their respective thumbnail.
			//-----------------------------------------------------------
			
			log.Debug("Organizing placeholders.");
			
			CleanupThumbnails();

			if (_fileNames.Count > 0)
			{
				ToggleButtonsVisibility(true);

				int iColumnWidth = (splitResizeBar.Panel2.Width - m_iLeftMargin - m_iRightMargin) / m_iCurrentSize;

				m_iHorzSpacing = iColumnWidth / 20;
				m_iVertSpacing = m_iHorzSpacing;
			
				for (int i = 0; i < _fileNames.Count; i++)
				{
					ThumbListViewItem tlvi = new ThumbListViewItem();

					tlvi.FileName = _fileNames[i];
					tlvi.Tag = i;
					tlvi.ToolTipHandler = toolTip1;
					tlvi.SetSize(iColumnWidth - m_iHorzSpacing);
					tlvi.Location = new Point(0, 0);
					tlvi.LaunchVideo += new ThumbListViewItem.LaunchVideoHandler(ThumbListViewItem_LaunchVideo);
					tlvi.VideoSelected += new ThumbListViewItem.VideoSelectedHandler(ThumbListViewItem_VideoSelected);
					tlvi.FileNameEditing += new ThumbListViewItem.FileNameEditingHandler(ThumbListViewItem_FileNameEditing);
					
					// Organize
					int iRow = i / m_iCurrentSize;
					int iCol = i - (iRow * m_iCurrentSize);
					tlvi.Location = new Point(m_iLeftMargin + (iCol * (tlvi.Size.Width + m_iHorzSpacing)), m_iTopMargin + (iRow * (tlvi.Size.Height + m_iVertSpacing)));
					
					tlvi.Visible = false;
					splitResizeBar.Panel2.Controls.Add(tlvi);
				}
			}
			else
			{
				ToggleButtonsVisibility(false);
			}
			
			log.Debug("Placeholders organized.");
		}
		private void ToggleButtonsVisibility(bool bVisible)
		{
			btnHideThumbView.Visible = bVisible;
			btnExtraSmall.Visible = bVisible;
			btnSmall.Visible = bVisible;
			btnMedium.Visible = bVisible;
			btnLarge.Visible = bVisible;
			btnExtraLarge.Visible = bVisible;
		}
		private void OrganizeThumbnailsByColumns(int iTotalCols)
		{
			// Resize and Organize thumbs to match a given number of columns
			if (splitResizeBar.Panel2.Controls.Count > 0 && !IsLoading())
			{
				log.Debug("Reorganizing thumbnails.");
			
				int iColumnWidth = (splitResizeBar.Panel2.Width - m_iLeftMargin - m_iRightMargin) / iTotalCols;
				m_iHorzSpacing = iColumnWidth / 20;
				m_iVertSpacing = m_iHorzSpacing;
				
				// Scroll up before relocating controls.
				splitResizeBar.Panel2.ScrollControlIntoView(splitResizeBar.Panel2.Controls[0]);

				splitResizeBar.Panel2.SuspendLayout();

				for (int i = 0; i < splitResizeBar.Panel2.Controls.Count; i++)
				{
					ThumbListViewItem tlvi = splitResizeBar.Panel2.Controls[i] as ThumbListViewItem;
					if(tlvi != null)
					{
						int iRow = i / iTotalCols;
						int iCol = i - (iRow * iTotalCols);
	
						tlvi.SetSize(iColumnWidth - m_iHorzSpacing);
	
						Point loc = new Point();
						loc.X = m_iLeftMargin + (iCol * (tlvi.Size.Width + m_iHorzSpacing));
						loc.Y = m_iTopMargin + (iRow * (tlvi.Size.Height + m_iVertSpacing));
						tlvi.Location = loc;
					}
				}

				splitResizeBar.Panel2.ResumeLayout();
				
				log.Debug("Thumbnails reorganized.");
			}
			
		}
		private void UpSizeThumbs()
		{
			DeselectAllSizingButtons();

			switch (m_iCurrentSize)
			{
				case (int)ExplorerThumbSizes.ExtraSmall:
					m_iCurrentSize = (int)ExplorerThumbSizes.Small;
					btnSmall.BackColor = Color.LightSteelBlue;
					break;
				case (int)ExplorerThumbSizes.Small:
					m_iCurrentSize = (int)ExplorerThumbSizes.Medium;
					btnMedium.BackColor = Color.LightSteelBlue;
					break;
				case (int)ExplorerThumbSizes.Medium:
					m_iCurrentSize = (int)ExplorerThumbSizes.Large;
					btnLarge.BackColor = Color.LightSteelBlue;
					break;
				case (int)ExplorerThumbSizes.Large:
				default:
					m_iCurrentSize = (int)ExplorerThumbSizes.ExtraLarge;
					btnExtraLarge.BackColor = Color.LightSteelBlue;
					break;
			}

			OrganizeThumbnailsByColumns(m_iCurrentSize);
			splitResizeBar.Panel2.Invalidate();
		}
		private void DownSizeThumbs()
		{
			DeselectAllSizingButtons();

			switch (m_iCurrentSize)
			{
				case (int)ExplorerThumbSizes.Small:
					m_iCurrentSize = (int)ExplorerThumbSizes.ExtraSmall;
					btnExtraSmall.BackColor = Color.LightSteelBlue;
					break;
				case (int)ExplorerThumbSizes.Medium:
					m_iCurrentSize = (int)ExplorerThumbSizes.Small;
					btnSmall.BackColor = Color.LightSteelBlue;
					break;
				case (int)ExplorerThumbSizes.Large:
					m_iCurrentSize = (int)ExplorerThumbSizes.Medium;
					btnMedium.BackColor = Color.LightSteelBlue;
					break;
				case (int)ExplorerThumbSizes.ExtraLarge:
					m_iCurrentSize = (int)ExplorerThumbSizes.Large;
					btnLarge.BackColor = Color.LightSteelBlue;
					break;
				case (int)ExplorerThumbSizes.ExtraSmall:
				default:
					m_iCurrentSize = (int)ExplorerThumbSizes.ExtraSmall;
					btnExtraSmall.BackColor = Color.LightSteelBlue;
					break;
			}

			OrganizeThumbnailsByColumns(m_iCurrentSize);
			splitResizeBar.Panel2.Invalidate();
		}
		private void splitResizeBar_Panel2_Resize(object sender, EventArgs e)
		{
			if(this.Visible)
			{
				OrganizeThumbnailsByColumns(m_iCurrentSize);
			}
		}
		private bool IsLoading()
		{
			bool bLoading = false;
			foreach (ThumbListLoader loader in m_Loaders)
			{
				if(!loader.Unused)
				{
					bLoading = true;
					break;
				}
			}
			return bLoading;
		}
		#endregion

		#region Thumbnails items events handlers
		private void ThumbListViewItem_LaunchVideo(object sender, EventArgs e)
		{
			CancelEditMode();
			ThumbListViewItem tlvi = sender as ThumbListViewItem;
			
			if (tlvi != null && !tlvi.ErrorImage)
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
				if (m_SelectedVideo != null && m_SelectedVideo != tlvi )
				{
					m_SelectedVideo.SetUnselected();
				}
			
				m_SelectedVideo = tlvi;
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
			switch (m_iCurrentSize)
			{
				case (int)ExplorerThumbSizes.Small:
					btnSmall.BackColor = Color.LightSteelBlue;
					break;
				case (int)ExplorerThumbSizes.Medium:
					btnMedium.BackColor = Color.LightSteelBlue;
					break;
				case (int)ExplorerThumbSizes.Large:
					btnLarge.BackColor = Color.LightSteelBlue;
					break;
				case (int)ExplorerThumbSizes.ExtraLarge:
					btnExtraLarge.BackColor = Color.LightSteelBlue;
					break;
				case (int)ExplorerThumbSizes.ExtraSmall:
					btnExtraSmall.BackColor = Color.LightSteelBlue;
					break;
				default:
					break;
			}
	
		}
		private void btnExtraSmall_Click(object sender, EventArgs e)
		{
			m_iCurrentSize = 14;
			OrganizeThumbnailsByColumns(m_iCurrentSize);
			DeselectAllSizingButtons();
			btnExtraSmall.BackColor = Color.LightSteelBlue;
			splitResizeBar.Panel2.Invalidate();
			SavePrefs();
		}
		private void btnSmall_Click(object sender, EventArgs e)
		{
			m_iCurrentSize = 10;
			OrganizeThumbnailsByColumns(m_iCurrentSize);
			DeselectAllSizingButtons();
			btnSmall.BackColor = Color.LightSteelBlue;
			splitResizeBar.Panel2.Invalidate();
			SavePrefs();
		}
		private void btnMedium_Click(object sender, EventArgs e)
		{
			m_iCurrentSize = 7;
			OrganizeThumbnailsByColumns(m_iCurrentSize);
			DeselectAllSizingButtons();
			btnMedium.BackColor = Color.LightSteelBlue;
			splitResizeBar.Panel2.Invalidate();
			SavePrefs();
		}
		private void btnLarge_Click(object sender, EventArgs e)
		{
			m_iCurrentSize = 5;
			OrganizeThumbnailsByColumns(m_iCurrentSize);
			DeselectAllSizingButtons();
			btnLarge.BackColor = Color.LightSteelBlue;
			splitResizeBar.Panel2.Invalidate();
			SavePrefs();
		}
		private void btnExtraLarge_Click(object sender, EventArgs e)
		{
			m_iCurrentSize = 4;
			OrganizeThumbnailsByColumns(m_iCurrentSize);
			DeselectAllSizingButtons();
			btnExtraLarge.BackColor = Color.LightSteelBlue;
			splitResizeBar.Panel2.Invalidate();
			SavePrefs();
		}
		#endregion

		#region Closing
		private void btnClose_Click(object sender, EventArgs e)
		{
			if (Closing != null) Closing(this);
		}
		private void btnShowThumbView_Click(object sender, EventArgs e)
		{
			CleanupThumbnails();
			if (Closing != null) Closing(this);
		}
		private void SavePrefs()
		{
			m_PreferencesManager.ExplorerThumbsSize = (ExplorerThumbSizes)m_iCurrentSize;
			m_PreferencesManager.Export();
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
							if (m_SelectedVideo == null)
							{
								((ThumbListViewItem)splitResizeBar.Panel2.Controls[0]).SetSelected();
							}
							else
							{
								int index = (int)m_SelectedVideo.Tag;
								int iRow = index / m_iCurrentSize;
								int iCol = index - (iRow * m_iCurrentSize);

								if (iCol > 0)
								{
									((ThumbListViewItem)splitResizeBar.Panel2.Controls[index - 1]).SetSelected();
								}
							}
							break;
						}
					case Keys.Right:
						{
							if (m_SelectedVideo == null)
							{
								((ThumbListViewItem)splitResizeBar.Panel2.Controls[0]).SetSelected();
							}
							else
							{
								int index = (int)m_SelectedVideo.Tag;
								int iRow = index / m_iCurrentSize;
								int iCol = index - (iRow * m_iCurrentSize);

								if (iCol < m_iCurrentSize - 1 && index + 1 < splitResizeBar.Panel2.Controls.Count)
								{
									((ThumbListViewItem)splitResizeBar.Panel2.Controls[index + 1]).SetSelected();
								}
							}
							break;
						}
					case Keys.Up:
						{
							if (m_SelectedVideo == null)
							{
								((ThumbListViewItem)splitResizeBar.Panel2.Controls[0]).SetSelected();
							}
							else
							{
								int index = (int)m_SelectedVideo.Tag;
								int iRow = index / m_iCurrentSize;
								int iCol = index - (iRow * m_iCurrentSize);

								if (iRow > 0)
								{
									((ThumbListViewItem)splitResizeBar.Panel2.Controls[index - m_iCurrentSize]).SetSelected();
								}
							}
							splitResizeBar.Panel2.ScrollControlIntoView(m_SelectedVideo);
							break;
						}
					case Keys.Down:
						{
							if (m_SelectedVideo == null)
							{
								((ThumbListViewItem)splitResizeBar.Panel2.Controls[0]).SetSelected();
							}
							else
							{
								int index = (int)m_SelectedVideo.Tag;
								int iRow = index / m_iCurrentSize;
								int iCol = index - (iRow * m_iCurrentSize);

								if ((iRow < splitResizeBar.Panel2.Controls.Count / m_iCurrentSize) && index + m_iCurrentSize  < splitResizeBar.Panel2.Controls.Count)
								{
									((ThumbListViewItem)splitResizeBar.Panel2.Controls[index + m_iCurrentSize]).SetSelected();
								}
							}
							splitResizeBar.Panel2.ScrollControlIntoView(m_SelectedVideo);
							break;
						}
					case Keys.Return:
						{
							if (m_SelectedVideo != null)
							{
								if (!m_SelectedVideo.ErrorImage)
								{
									m_ScreenManagerUIContainer.DropLoadMovie(m_SelectedVideo.FileName, -1);
								}
							}
							break;
						}
					case Keys.Add:
						{
							if ((ModifierKeys & Keys.Control) == Keys.Control)
							{
								UpSizeThumbs();
							}
							break;
						}
					case Keys.Subtract:
						{
							if ((ModifierKeys & Keys.Control) == Keys.Control)
							{
								DownSizeThumbs();
							}
							break;
						}
					case Keys.F2:
						{
							if(m_SelectedVideo != null)
							{
								if(!m_SelectedVideo.ErrorImage)
									m_SelectedVideo.StartRenaming();
							}
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
        	// Clicked nowhere.
        	
        	// 1. Deselect all videos.
        	if(m_SelectedVideo != null)
        	{
        		m_SelectedVideo.SetUnselected();
        		m_SelectedVideo = null;
        	}
        	
        	// 2. Toggle off edit mode.
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
				{
					tlvi.CancelEditMode();
				}
			}	
        }
        private void Panel2MouseEnter(object sender, EventArgs e)
        {
        	// Give focus to enbale mouse scroll
        	if(!m_bEditMode)
        	{
        		splitResizeBar.Panel2.Focus();
        	}
        }
        
        private void Panel2Paint(object sender, PaintEventArgs e)
        {
    		e.Graphics.DrawLine(m_GradientPen, 33, 0, 350, 0);
        }
	}
}
