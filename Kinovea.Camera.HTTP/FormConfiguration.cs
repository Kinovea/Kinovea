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
using System.Drawing;
using System.Windows.Forms;

using Kinovea.Camera;

namespace Kinovea.Camera.HTTP
{
    public partial class FormConfiguration : Form
    {
        public bool AliasChanged
        {
            get { return iconChanged || tbAlias.Text != summary.Alias;}
        }
        
        public string Alias 
        { 
            get { return tbAlias.Text; }
        }
        
        public Bitmap PickedIcon
        { 
            get { return (Bitmap)btnIcon.BackgroundImage; }
        }
        
        public bool SpecificChanged
        {
            get { return wizard.GetSpecific() != (summary.Specific as SpecificInfo);}
        }
        
        public SpecificInfo Specific
        {
            get { return wizard.GetSpecific();}
        }
        
        private bool iconChanged;
        private ConnectionWizard wizard;
        private CameraSummary summary;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public FormConfiguration(CameraSummary summary)
        {
            this.summary = summary;
            
            InitializeComponent();
            
            tbAlias.Text = summary.Alias;
            lblSystemName.Text = summary.Name;
            btnIcon.BackgroundImage = summary.Icon;
            
            wizard = (ConnectionWizard)summary.Manager.GetConnectionWizard();
            wizard.Location = new Point(8, 18);
            gpParameters.Controls.Add(wizard);
            wizard.Populate((summary.Specific as SpecificInfo).Clone());
        }
        
        private void BtnIconClick(object sender, EventArgs e)
        {
            FormIconPicker fip = new FormIconPicker(IconLibrary.Icons, 5, "Icons");
            LocateForm(fip);
            if(fip.ShowDialog() == DialogResult.OK)
            {
                btnIcon.BackgroundImage = fip.PickedIcon;
                iconChanged = true;
            }
            
            fip.Dispose();
        }
        
        private void LocateForm(Form form)
        {
            // Note: function duplicated from ScreenManager which we don't want to depend upon.
            // Maybe this method would be better in Kinovea.Service.
            if (Cursor.Position.X + (form.Width / 2) >= SystemInformation.PrimaryMonitorSize.Width || 
                Cursor.Position.Y + form.Height >= SystemInformation.PrimaryMonitorSize.Height)
                form.StartPosition = FormStartPosition.CenterScreen;
            else
                form.Location = new Point(Cursor.Position.X - (form.Width / 2), Cursor.Position.Y - 20);
        }
        
        private void BtnTest_Click(object sender, EventArgs e)
        {
            wizard.TestCamera();
        }
    }
}

