#region License
/*
Copyright © Joan Charmant 2011.
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
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Kinovea.Services;
using Kinovea.ScreenManager.Languages;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Dialog to change the name of a specfiic drawing.
    /// Works on the original and revert to a copy in case of cancel.
    /// </summary>
    public partial class FormConfigureDrawingName : Form
    {
        #region Members
        private AbstractDrawing drawing;
        private bool manualClose;
        private string oldName;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        #region Constructor
        public FormConfigureDrawingName(AbstractDrawing drawing)
        {
            this.drawing = drawing;
            this.oldName = drawing.Name;

            InitializeComponent();
            LocalizeForm();
            SetupForm();
        }
        #endregion
        
        #region Private Methods
        private void LocalizeForm()
        {
            this.Text = "   " + ScreenManagerLang.dlgConfigureDrawing_Title;
            grpIdentifier.Text = ScreenManagerLang.drawingName;
            btnCancel.Text = ScreenManagerLang.Generic_Cancel;
            btnOK.Text = ScreenManagerLang.Generic_Apply;
        }
        private void SetupForm()
        {
            tbName.Text = drawing.Name;
        }
        
        #region Closing
        private void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(!manualClose)
            {
                Revert();
            }
        }
        
        private void Revert()
        {
            drawing.Name = oldName;
        }
        private void BtnCancel_Click(object sender, EventArgs e)
        {	
            Revert();
            manualClose = true;
        }
        private void BtnOK_Click(object sender, EventArgs e)
        {
            manualClose = true;	
        }
        #endregion

        private void tbName_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(tbName.Text))
                return;

            drawing.Name = tbName.Text;
        }
        
        #endregion
        
        
    }
}
