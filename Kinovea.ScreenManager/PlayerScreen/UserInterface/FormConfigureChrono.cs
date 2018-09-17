#region License
/*
Copyright © Joan Charmant 2008.
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
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;

using Kinovea.ScreenManager.Languages;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// The dialog lets the user configure a chronometer instance.
    /// Some of the logic is the same as for formConfigureDrawing.
    /// Specifically, we work and update the actual instance in real time. 
    /// If the user finally decide to cancel there's a "fallback to memo" mechanism. 
    /// </summary>
    public partial class formConfigureChrono : Form
    {
        #region Members
        private bool m_bManualClose = false;
        private Action m_Invalidate;
        private DrawingChrono m_Chrono;
        private string m_MemoLabel;
        private bool m_bMemoShowLabel;
        private AbstractStyleElement m_firstElement;
        private AbstractStyleElement m_secondElement;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        #region Construction
        public formConfigureChrono(DrawingChrono _chrono, Action _invalidate)
        {
            InitializeComponent();
            m_Invalidate = _invalidate;
            m_Chrono = _chrono;
            m_Chrono.DrawingStyle.ReadValue();
            m_Chrono.DrawingStyle.Memorize();
            m_MemoLabel = m_Chrono.Label;
            m_bMemoShowLabel = m_Chrono.ShowLabel;
            
            SetupForm();
            LocalizeForm();
            
            tbLabel.Text = m_Chrono.Label;
            chkShowLabel.Checked = m_Chrono.ShowLabel;
        }
        #endregion

        #region Private Methods
        private void SetupForm()
        {
            foreach(KeyValuePair<string, AbstractStyleElement> styleElement in m_Chrono.DrawingStyle.Elements)
            {
                if(m_firstElement == null)
                {
                    m_firstElement = styleElement.Value;
                }
                else if(m_secondElement == null)
                {
                    m_secondElement = styleElement.Value;
                }
                else
                {
                    log.DebugFormat("Discarding style element: \"{0}\". (Only 2 style elements supported).", styleElement.Key);
                }
            }
            
            // Configure editor line for each element.
            // The style element is responsible for updating the internal value and the editor appearance.
            // The element internal value might also be bound to a style helper property so that the underlying drawing will get updated.
            if(m_firstElement != null)
            {
                lblFirstElement.Text = m_firstElement.DisplayName;
                m_firstElement.ValueChanged += element_ValueChanged;
                
                int editorsLeft = 150; // works in High DPI ?
                
                Control firstEditor = m_firstElement.GetEditor();
                firstEditor.Size = new Size(50, 20);
                firstEditor.Location = new Point(editorsLeft, lblFirstElement.Top - 3);
                grpConfig.Controls.Add(firstEditor);
                
                if(m_secondElement != null)
                {
                    lblSecondElement.Text = m_secondElement.DisplayName;
                    m_secondElement.ValueChanged += element_ValueChanged;
                    
                    Control secondEditor = m_secondElement.GetEditor();
                    secondEditor.Size = new Size(50, 20);
                    secondEditor.Location = new Point(editorsLeft, lblSecondElement.Top  - 3);
                    grpConfig.Controls.Add(secondEditor);
                }
                else
                {
                    lblSecondElement.Visible = false;
                }
            }
            else
            {
                lblFirstElement.Visible = false;
            }
        }
        private void LocalizeForm()
        {
            this.Text = "   " + ScreenManagerLang.dlgConfigureChrono_Title;
            btnCancel.Text = ScreenManagerLang.Generic_Cancel;
            btnOK.Text = ScreenManagerLang.Generic_Apply;
            grpConfig.Text = ScreenManagerLang.Generic_Configuration;
            
            lblLabel.Text = ScreenManagerLang.dlgConfigureChrono_Label;
            chkShowLabel.Text = ScreenManagerLang.dlgConfigureChrono_chkShowLabel;
        }
        private void element_ValueChanged(object sender, EventArgs e)
        {
            if(m_Invalidate != null) m_Invalidate();
        }
        private void tbLabel_TextChanged(object sender, EventArgs e)
        {
            m_Chrono.Label = tbLabel.Text;
            if(m_Invalidate != null) m_Invalidate();
        }
        private void chkShowLabel_CheckedChanged(object sender, EventArgs e)
        {
            m_Chrono.ShowLabel = chkShowLabel.Checked;
            if(m_Invalidate != null) m_Invalidate();
        }
        #endregion
        
        #region Closing
        private void UnhookEvents()
        {
            // Unhook event handlers
            if(m_firstElement != null)
            {
                m_firstElement.ValueChanged -= element_ValueChanged;
            }
            
            if(m_secondElement != null)
            {
                m_secondElement.ValueChanged -= element_ValueChanged;
            }
        }
        private void Revert()
        {
            // Revert to memo and re-update data.
            m_Chrono.DrawingStyle.Revert();
            m_Chrono.DrawingStyle.RaiseValueChanged();
            m_Chrono.Label = m_MemoLabel;
            m_Chrono.ShowLabel = m_bMemoShowLabel;
            
            if(m_Invalidate != null) m_Invalidate();
        }
        private void btnOK_Click(object sender, EventArgs e)
        {
            UnhookEvents();
            m_bManualClose = true;	
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            UnhookEvents();
            Revert();
            m_bManualClose = true;
        }
        private void formConfigureChrono_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(!m_bManualClose)
            {
                UnhookEvents();
                Revert();
            }
        }
        #endregion 
    }
}