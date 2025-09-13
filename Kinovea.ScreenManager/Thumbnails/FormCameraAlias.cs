#region License
/*
Copyright © Joan Charmant 2013.
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
using System.Drawing;
using System.Windows.Forms;

using Kinovea.Camera;
using Kinovea.Services;
using Kinovea.ScreenManager.Languages;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Description of FormCameraAlias.
    /// </summary>
    public partial class FormCameraAlias : Form
    {
        public string Alias 
        { 
            get { return tbAlias.Text; }
        }
        
        public Bitmap PickedIcon 
        { 
            get { return (Bitmap)btnIcon.BackgroundImage; }
        }
    
        private CameraSummary summary;
        public FormCameraAlias(CameraSummary summary)
        {
            this.summary = summary;
            InitializeComponent();
            Initialize();
            tbAlias.SelectAll();
            tbAlias.Select();
        }
        
        private void Initialize()
        {
            this.Text = ScreenManagerLang.FormCameraAlias_Title;
            lblAlias.Text = ScreenManagerLang.FormCameraAlias_Alias;
            lblIcon.Text = ScreenManagerLang.FormCameraAlias_Icon;
            tbAlias.Text = summary.Alias;
            btnIcon.BackgroundImage = summary.Icon;
            btnOK.Text = ScreenManagerLang.Generic_Apply;
            btnCancel.Text = ScreenManagerLang.Generic_Cancel;
        }
        
        private void BtnReset_Click(object sender, EventArgs e)
        {
            tbAlias.Text = summary.Name;
        }
        
        private void BtnIconClick(object sender, EventArgs e)
        {
            FormIconPicker fip = new FormIconPicker(IconLibrary.Icons, 5);
            FormsHelper.Locate(fip);
            if(fip.ShowDialog() == DialogResult.OK)
                btnIcon.BackgroundImage = fip.PickedIcon;
            
            fip.Dispose();
        }

        private void tbAlias_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                this.DialogResult = DialogResult.OK;
                Close();
            }
        }
    }
}
