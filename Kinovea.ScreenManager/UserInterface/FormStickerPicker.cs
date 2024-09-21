#region License
/*
Copyright © Joan Charmant 2010.
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
using Kinovea.ScreenManager.Languages;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Kinovea.Services;
using Kinovea.ScreenManager.Properties;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Sticker picker.
    /// </summary>
    public partial class FormStickerPicker : Form
    {
        #region Properties
        public string PickedStickerRef
        {
            get { return pickedStickerRef; }
        }
        #endregion

        #region Members
        private string currentStickerRef;
        private string pickedStickerRef;
        private List<string> recentStickerRefs;
        private int buttonSize = 48;
        private int margin = 4;
        #endregion

        #region Construction and Initialization
        public FormStickerPicker(string currentStickerRef)
        {
            // TODO.

            //this.currentColor = Color.Black;
            this.SuspendLayout();
            InitializeComponent();
            GenerateStickerPalette();
            //colorPicker = new ColorPicker(currentColor);
            //colorPicker.Top = 5;
            //colorPicker.Left = 5;
            //colorPicker.ColorPicked += colorPicker_ColorPicked;
            
            //Controls.Add(colorPicker);
            this.ResumeLayout();
            
            // Recent colors.
            //recentColors = PreferencesManager.PlayerPreferences.RecentColors;
            
            //colorPicker.DisplayRecentColors(recentColors);
            //this.Height = colorPicker.Bottom + 20;
        }
        #endregion
        
        private void GenerateStickerPalette()
        {
            // We want the stickers to appear in a specific order and not in alphanumeric order of their code point.

            List<List<string>> stickers = new List<List<string>>();

            var row1 = new List<string>();
            stickers.Add(row1);
            row1.Add("_1f600");
            row1.Add("_1f601");
            row1.Add("_1f602");
            row1.Add("_1f604");
            row1.Add("_1f605");
            row1.Add("_1f606");
            row1.Add("_1f609");
            row1.Add("_1f610");
            row1.Add("_1f613");
            row1.Add("_1f615");
            row1.Add("_1f616");
            row1.Add("_1f620");

            var row2 = new List<string>();
            stickers.Add(row2);
            row2.Add("_1f621");
            row2.Add("_1f622");
            row2.Add("_1f623");
            row2.Add("_1f624");
            row2.Add("_1f625");
            row2.Add("_1f626");
            row2.Add("_1f629");
            row2.Add("_1f631");
            row2.Add("_1f632");
            row2.Add("_1f634");
            row2.Add("_1f635");
            row2.Add("_1f641");

            var row3 = new List<string>();
            stickers.Add(row3);
            row3.Add("_1f642");
            row3.Add("_1f643");
            row3.Add("_1f60b");
            row3.Add("_1f60d");
            row3.Add("_1f60e");
            row3.Add("_1f61b");
            row3.Add("_1f61f");
            row3.Add("_1f62b");
            row3.Add("_1f62c");
            row3.Add("_1f62d");
            row3.Add("_1f62e");
            row3.Add("_1f62f");

            var row4 = new List<string>();
            stickers.Add(row4);
            row4.Add("_1f913");
            row4.Add("_1f914");
            row4.Add("_1f915");
            row4.Add("_1f921");
            row4.Add("_1f923");
            row4.Add("_1f928");
            row4.Add("_1f929");
            row4.Add("_1f971");
            row4.Add("_1f973");
            row4.Add("_1f974");
            row4.Add("_1f975");
            row4.Add("_1f979");

            var row5 = new List<string>();
            stickers.Add(row5);
            row5.Add("_1fae3");
            row5.Add("_1fae4");
            row5.Add("_1f9d0");
            row5.Add("_1f3af"); // non-faces.
            row5.Add("_1f3c5");
            row5.Add("_1f4a4");
            row5.Add("_1f4a5");
            row5.Add("_1f4a9");
            row5.Add("_1f4aa");
            row5.Add("_1f4af");
            row5.Add("_1f4cf");
            row5.Add("_1f6a6");

            var row6 = new List<string>();
            stickers.Add(row6);
            row6.Add("_1f6ab");
            row6.Add("_1f6d1");
            row6.Add("_1f9b4");
            row6.Add("_1f9b5");
            row6.Add("_1f9b6");
            row6.Add("_1f9e0");
            row6.Add("_1f40c");
            row6.Add("_1f44b");
            row6.Add("_1f44c");
            row6.Add("_1f44d");
            row6.Add("_1f44e");
            row6.Add("_1f44f");

            var row7 = new List<string>();
            stickers.Add(row7);
            row7.Add("_1f64c");
            row7.Add("_1f64f");
            row7.Add("_1f91a");
            row7.Add("_1f91d");
            row7.Add("_1f389");
            row7.Add("_1f440");
            row7.Add("_1f445");
            row7.Add("_1f446");
            row7.Add("_1f447");
            row7.Add("_1f448");
            row7.Add("_1f449");
            row7.Add("_1f463");

            var row8 = new List<string>();
            stickers.Add(row8);
            row8.Add("_1f480");
            row8.Add("_1f496");
            row8.Add("_1f500");
            row8.Add("_1f503");
            row8.Add("_1f504");
            row8.Add("_1f514");
            row8.Add("_1f525");
            row8.Add("_1f680");
            row8.Add("_1faab");
            row8.Add("_1fac1");
            row8.Add("_2b50");
            row8.Add("_21a9");

            var row9 = new List<string>();
            stickers.Add(row9);
            row9.Add("_21aa");
            row9.Add("_26a0");
            row9.Add("_26a1");
            row9.Add("_26d4");
            row9.Add("_203c");
            row9.Add("_270b");
            row9.Add("_274c");
            row9.Add("_2049");
            row9.Add("_2705");
            row9.Add("_2728");
            row9.Add("_2753");
            row9.Add("_2757");


            int left = 0;
            int top = 0;

            List<Button> buttons = new List<Button>();
            for (int row = 0; row < stickers.Count; row++)
            {
                left = 0;
                for (int col = 0; col < row1.Count; col++)
                {
                    buttons.Add(CreateStickerButton(stickers[row][col], left, top));
                    left += buttonSize + margin;
                }

                top += buttonSize + margin;
            }

            Controls.AddRange(buttons.ToArray());
        }

        private Button CreateStickerButton(string stickerRef, int x, int y)
        {
            Button b = new Button();

            b.BackColor = Color.White;
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = Color.White;
            b.FlatAppearance.BorderColor = Color.Blue;
            b.FlatStyle = FlatStyle.Flat;
            b.Location = new Point(x, y);
            b.Size = new Size(buttonSize, buttonSize);
            b.TabStop = false;
            b.Tag = stickerRef;
            b.BackgroundImageLayout = ImageLayout.Stretch;

            object resource = Stickers.ResourceManager.GetObject(stickerRef);
            if (resource != null && resource is Bitmap)
            {
                b.BackgroundImage = resource as Bitmap;
            }

            //if (currentColorButton == null && currentColor == color)
            //{
            //    b.FlatAppearance.BorderSize = 1;
            //    currentColorButton = b;
            //}

            b.Click += stickerButton_Click;
            //b.MouseEnter += stickerButton_MouseEnter;
            //b.MouseLeave += stickerButton_MouseLeave;

            return b;
        }

        #region event handlers
        private void stickerButton_Click(object sender, EventArgs e)
        {
            Button b = sender as Button;
            if (b == null)
                return;

            pickedStickerRef = (string)b.Tag;
            DialogResult = DialogResult.OK;
            Close();
        }

        //private void colorPicker_ColorPicked(object sender, System.EventArgs e)
        //{
        //    pickedColor = colorPicker.PickedColor;
        //    PreferencesManager.PlayerPreferences.AddRecentColor(colorPicker.PickedColor);
        //    PreferencesManager.Save();
        //    DialogResult = DialogResult.OK;
        //    Close();
        //}
        #endregion
    }
}
