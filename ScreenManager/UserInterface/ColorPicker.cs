#region License
/*
Copyright © Joan Charmant 2010.
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
using Kinovea.ScreenManager.Languages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// ColorPicker. Let the user choose a color.
	/// 
	/// This color picker is heavily inspired by the Color Picker from Greenshot.
	/// The code for generating the palette is taken from Greenshot with almost no modifications.
	/// http://greenshot.sourceforge.net/
	/// </summary>
	public partial class ColorPicker : UserControl
	{
		#region Events
        [Category("Action"), Browsable(true)]
        public event ColorPickedHandler ColorPicked;
		#endregion
        
		#region Properties
		public Color PickedColor
        {
            get { return m_PickedColor; }
        }
		#endregion
		
		#region Members
		private List<Button> m_ColorButtons = new List<Button>();
		private Color m_PickedColor;
		private static readonly int m_iButtonSize = 15;
		#endregion
		
		#region Construction and Initialization
		public ColorPicker()
		{
			this.SuspendLayout();
			InitializeComponent();
			GeneratePalette(0, 0, m_iButtonSize, m_iButtonSize);
			this.ResumeLayout();
		}
		private void GeneratePalette(int _left, int _top, int _buttonWidth, int _buttonHeight) 
		{
			int shades = 11;
			
			createColorButtonColumn(255,0,0, _left, _top, _buttonWidth, _buttonHeight, shades);
			
			_left += _buttonWidth;
			createColorButtonColumn(255,255/2,0, _left, _top, _buttonWidth, _buttonHeight, shades);
			
			_left += _buttonWidth;
			createColorButtonColumn(255,255,0, _left, _top, _buttonWidth, _buttonHeight, shades);
			
			_left += _buttonWidth;
			createColorButtonColumn(255/2,255,0, _left, _top, _buttonWidth, _buttonHeight, shades);
			
			_left += _buttonWidth;
			createColorButtonColumn(0,255,0, _left, _top, _buttonWidth, _buttonHeight, shades);
			
			_left += _buttonWidth;
			createColorButtonColumn(0,255,255/2, _left, _top, _buttonWidth, _buttonHeight, shades);
			
			_left += _buttonWidth;
			createColorButtonColumn(0,255,255, _left, _top, _buttonWidth, _buttonHeight, shades);
			
			_left += _buttonWidth;
			createColorButtonColumn(0,255/2,255, _left, _top, _buttonWidth, _buttonHeight, shades);
			
			_left += _buttonWidth;
			createColorButtonColumn(0,0,255, _left, _top, _buttonWidth, _buttonHeight, shades);
			
			_left += _buttonWidth;
			createColorButtonColumn(255/2,0,255, _left, _top, _buttonWidth, _buttonHeight, shades);
			
			_left += _buttonWidth;
			createColorButtonColumn(255,0,255, _left, _top, _buttonWidth, _buttonHeight, shades);
			
			_left += _buttonWidth;
			createColorButtonColumn(255,0,255/2, _left, _top, _buttonWidth, _buttonHeight, shades);
			
			// Grayscale column.
			_left += _buttonWidth + 5;
			createColorButtonColumn(255/2,255/2,255/2, _left, _top, _buttonWidth, _buttonHeight, shades);
			
			Controls.AddRange(m_ColorButtons.ToArray());
		}
		private void createColorButtonColumn(int red, int green, int blue, int x, int y, int w, int h, int shades) 
		{
			int shadedColorsNum = (shades - 1) / 2;
			
			for(int i=0; i<=shadedColorsNum; i++)
			{
				m_ColorButtons.Add(createColorButton(red * i / shadedColorsNum, green * i /shadedColorsNum, blue * i / shadedColorsNum, x, y + i * h, w, h));
				
				if (i>0) 
				{
					m_ColorButtons.Add(createColorButton(red + (255 - red) * i / shadedColorsNum, green + (255 - green)* i /shadedColorsNum, blue+ (255 - blue) * i / shadedColorsNum, x, y + (i+shadedColorsNum) * h, w,h));
				}
			}
		}
		private Button createColorButton(int red, int green, int blue, int x, int y, int w, int h) 
		{
			return createColorButton(Color.FromArgb(255, red, green, blue), x, y, w, h);
		}
		private Button createColorButton(Color color,  int x, int y, int w, int h) 
		{
			Button b = new Button();
			
			b.BackColor = color;
			b.FlatAppearance.BorderSize = 0;
			b.FlatAppearance.MouseOverBackColor = color;
			b.FlatAppearance.BorderColor = Color.FromArgb(color.A, 255 - color.R, 255 - color.G, 255 - color.B);
			b.FlatStyle = FlatStyle.Flat;
			b.Location = new Point(x,y);
			b.Size = new Size(w,h);
			b.TabStop = false;
			        
			b.Click += new System.EventHandler(colorButton_Click);
			b.MouseEnter += new EventHandler(colorButton_MouseEnter);
			b.MouseLeave += new EventHandler(colorButton_MouseLeave);
			
			return b;
		}
		#endregion
		
		public void DisplayRecentColors(List<Color> _recentColors)
		{
			if(_recentColors.Count > 0)
			{
				Label lblRecent = new Label();
				lblRecent.AutoSize = true;
				
				lblRecent.Text = ScreenManagerLang.RecentlyUsedColors;
				lblRecent.Top = this.Margin.Top + (11 * m_iButtonSize) + 30;
				Controls.Add(lblRecent);
				
				List<Button> recentButtons = new List<Button>();
				int x = 0;
				int y = lblRecent.Bottom + 5;
				for(int i=0; i<_recentColors.Count;i++)
				{
					Button b = createColorButton(_recentColors[i], x, y, m_iButtonSize, m_iButtonSize);
					recentButtons.Add(b);
					x += m_iButtonSize;
				}
				
				Controls.AddRange(recentButtons.ToArray());
			}
			
		}
		
		#region event handlers
		private void colorButton_Click(object sender, System.EventArgs e) 
		{
			Button b = (Button) sender;
			m_PickedColor = b.BackColor;
			
			// Raise event.
			if(ColorPicked != null)
			{
				ColorPicked(this, e);
			}
		}
		private void colorButton_MouseEnter(object sender, System.EventArgs e) 
		{
			Button b = (Button) sender;
			b.FlatAppearance.BorderSize = 1;
		}
		private void colorButton_MouseLeave(object sender, System.EventArgs e) 
		{
			Button b = (Button) sender;
			b.FlatAppearance.BorderSize = 0;
		}
		#endregion
	}
	
	public delegate void ColorPickedHandler(object sender, EventArgs e);
}
