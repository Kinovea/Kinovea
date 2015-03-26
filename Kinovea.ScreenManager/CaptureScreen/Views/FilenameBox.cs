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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    public partial class FilenameBox : UserControl
    {
        #region Events
        [Category("Action")] 
        public event EventHandler ImageClick;
        
        [Category("Property Changed")] 
        public event EventHandler FilenameChanged;
        #endregion
        
        #region Properties
        [Category("Appearance")]
        public Image Image
        {
            get { return btnDirectoryLocation.Image;}
            set { btnDirectoryLocation.Image = value;}
        }
        
        [Category("Appearance")]
        public string InfoText
        {
            get { return lblFilename.Text;}
            set 
            { 
                lblFilename.Text = value;
                tbFilename.Left = lblFilename.Right + 5;
            }
        }
        
        [Category("Appearance")]
        public string Filename
        {
            get { return tbFilename.Text;}
            set { tbFilename.Text = value; }
        }
        
        [Category("Appearance")]
        public bool Editable
        {
            get { return tbFilename.Enabled;}
            set { tbFilename.Enabled = value; }
        }
        
        [Browsable(false)]
        public new bool Focused
        {
            get { return tbFilename.Focused;}
        }

        #endregion

        public FilenameBox()
        {
            InitializeComponent();
        }

        private void BtnDirectoryLocationClick(object sender, EventArgs e)
        {
            if(ImageClick != null)
                ImageClick(this, EventArgs.Empty);
        }
        
        private void TbFilenameTextChanged(object sender, EventArgs e)
        {
            if(FilenameChanged != null)
                FilenameChanged(this, EventArgs.Empty);
        }
    }
}
