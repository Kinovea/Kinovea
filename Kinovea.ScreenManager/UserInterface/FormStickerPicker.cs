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
        private string pickedStickerRef;
        private int buttonSize = 48;
        private int margin = 4;
        #endregion

        #region Construction and Initialization
        public FormStickerPicker()
        {
            this.SuspendLayout();
            InitializeComponent();
            GenerateStickerPalette();
            this.ResumeLayout();
        }
        #endregion
        
        private void GenerateStickerPalette()
        {
            // Curated list of Emojis.
            // https://github.com/twitter/twemoji
            // https://twemoji.godi.se/
            // https://twemoji-cheatsheet.vercel.app/
            // We want the stickers to appear in a specific order and not in alphanumeric order of their code point.

            List<List<string>> stickers = new List<List<string>>();

            // TODO: add one row for the recently used stickers.

            // Happy faces.
            stickers.Add(new List<string>());
            stickers[stickers.Count - 1].AddRange(new List<string>() {
                "_1f600",
                "_1f601",
                "_1f602",
                "_1f604",
                "_1f605",
                "_1f606",
                "_1f609",
                "_1f642",
                "_1f643",
                "_1f923",
                "_1f929",
                "_1f60e",
            });

            // Sad faces.
            stickers.Add(new List<string>());
            stickers[stickers.Count - 1].AddRange(new List<string>() {
                "_1f61f",
                "_1f62b",
                "_1f62d",
                "_1f613",
                "_1f615",
                "_1f620",
                "_1f621",
                "_1f622",
                "_1f623",
                "_1f626",
                "_1f629",
                "_1f641",
            });

            // Neutral, Thinking, Surprised.
            stickers.Add(new List<string>());
            stickers[stickers.Count - 1].AddRange(new List<string>() {
                "_1f610",
                "_1f611",
                "_1f928",
                "_1fae4",
                "_1f62c",
                "_1f62f",
                "_1f914",
                "_1f9d0",
                "_1f92f",
                "_1f631",
                "_1f632",
                "_1fae3",
            });

            // Angry, others
            stickers.Add(new List<string>());
            stickers[stickers.Count - 1].AddRange(new List<string>() {
                "_1f92c",
                "_1f92e",
                "_1f616",
                "_1f624",
                "_1f625",
                "_1f634",
                "_1f635",
                "_1f915",
                "_1f971",
                "_1f974",
                "_1f975",
                "_1f4a9",
            });

            // Misc faces
            stickers.Add(new List<string>());
            stickers[stickers.Count - 1].AddRange(new List<string>() {
                "_1f61b",
                "_1f60d",
                "_1f60b",
                "_1f913",
                "_1f973",
                "_1f979",
                "_1f921",
                "_1f64f",
                "_1f680",
                "_1f40c",
                "_1faab",
                "_1f4a4",
            });

            // Hand gestures
            stickers.Add(new List<string>());
            stickers[stickers.Count - 1].AddRange(new List<string>() {
                "_1f44b",
                "_1f44c",
                "_1f44d",
                "_1f44e",
                "_1f44f",
                "_1f91a",
                "_1f91d",
                "_1f446",
                "_1f447",
                "_1f448",
                "_1f449",
                "_270b",
            });

            // Commentary
            stickers.Add(new List<string>());
            stickers[stickers.Count - 1].AddRange(new List<string>() {
                "_2764",
                "_1f496",
                "_1f3af",
                "_1f3c5",
                "_1f64c",
                "_1f4af",
                "_1f389",
                "_1f525",
                "_2b50",
                "_26a1",
                "_2728",
                "_1f4a5",
            });

            // Body parts
            stickers.Add(new List<string>());
            stickers[stickers.Count - 1].AddRange(new List<string>() {
                "_1f9e0",
                "_1fac0",
                "_1fac1",
                "_1f440",
                "_1f445",
                "_1f463",
                "_1f480",
                "_1f9b4",
                "_1f4aa",
                "_1f9b5",
                "_1f9b6",
            });

            // Signs
            stickers.Add(new List<string>());
            stickers[stickers.Count - 1].AddRange(new List<string>() {
                "_2705",
                "_274c",
                "_2757",
                "_203c",
                "_2049",
                "_2753",
                "_26a0",
                "_1f514",
                "_1f6a6",
                "_1f6d1",
                "_1f6ab",
                "_26d4",
            });

            // Arrows
            stickers.Add(new List<string>());
            stickers[stickers.Count - 1].AddRange(new List<string>() {
                "_1f500",
                "_1f503",
                "_1f504",
                "_21a9",
                "_21aa",
                "_2934",
                "_2935",
            });

            int left = 0;
            int top = 0;

            List<Button> buttons = new List<Button>();
            for (int row = 0; row < stickers.Count; row++)
            {
                left = 0;
                for (int col = 0; col < stickers[row].Count; col++)
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

            b.Click += stickerButton_Click;
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
        #endregion
    }
}
