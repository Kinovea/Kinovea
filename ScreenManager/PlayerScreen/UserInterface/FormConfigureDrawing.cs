#region License
/*
Copyright © Joan Charmant 2009.
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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Resources;
using System.Threading;
using System.Reflection;
using Videa.Services;

namespace Videa.ScreenManager
{
	/// <summary>
	/// Dialog that let the user change the color and style of a specific drawing
	/// _or_ change the color profile entry for the corresponding DrawingTool.
	/// 
	/// One of the differences is that updating a drawing is done in real time while
	/// updating the main color profile is done 'a posteriori' when we submit the form.
	/// We can't use 100% polymorphism here because the controls configuration and visibility
	/// depend on the tool or drawing we'll be updating. 
	/// 
	/// Chronos are a special case:
	/// Instance modification is done through the formConfigureChrono dialog, while
	/// profile entry preconfiguration is done here, with less options. (only color currently)
	/// </summary>
    public partial class formConfigureDrawing : Form
    {
    	#region members
    	// Common
        private ResourceManager m_ResourceManager;
    	private bool m_bPreConfigure;				// true if we are updating the ColorProfile.
    	private DrawingToolType m_ToolType;		// Needed for controls organisation and visibility.
    	private bool m_bManualClose = false;		// Needed to handle cancel button.
    	
    	// For color profile configuration only.
        private ColorProfile m_ColorProfile;        // Direct ref to the original color profile.
        private ColorProfile m_TempColorProfile;    // Temporary ColorProfile.
    	
    	// For individual drawing configuration only.
    	private AbstractDrawing m_Drawing;			// The drawing we will modify.
    	private PictureBox m_SurfaceScreen;         // Used to update the image while configuring.
        
        //private int m_MemoFontSize = -1;
        #endregion
        
        #region Construction & Initialization
        public formConfigureDrawing(DrawingToolType _ToolType, ColorProfile _colorProfile)
        {
            // This constructor is called when we will be updating 
            // the DrawingTool entry in the main ColorProfile.
            
            m_ResourceManager = new ResourceManager("Videa.ScreenManager.Languages.ScreenManagerLang", Assembly.GetExecutingAssembly());
            m_bPreConfigure = true;

            // We will make the updates on a copy of the color profile
            // and only commit them to the real one when user submit the form.
            m_ColorProfile = _colorProfile;
            m_TempColorProfile = new ColorProfile();
            m_TempColorProfile.Load(_colorProfile);
            
            m_ToolType = _ToolType;
            
            SetupForm();
        }
        public formConfigureDrawing(AbstractDrawing _drawing, PictureBox _SurfaceScreen)
        {
            // This constructor is called when we will be updating a specific drawing.
            m_ResourceManager = new ResourceManager("Videa.ScreenManager.Languages.ScreenManagerLang", Assembly.GetExecutingAssembly());
            m_bPreConfigure = false;
            m_SurfaceScreen = _SurfaceScreen;
            m_Drawing = _drawing;
            m_ToolType = m_Drawing.ToolType;
            m_Drawing.MemorizeDecoration();

            SetupForm();
        }
        private void SetupForm()
        {
        	InitializeComponent();
        	ConfigureForm(m_ToolType);
            LocalizeForm();
        }
        private void ConfigureForm(DrawingToolType _ToolType)
        {
        	// Configure the controls depending on the tool updated.
        	// Note: Color Picker is always visible.
        	// TODO: Height should be computed.
            switch (_ToolType)
            {
                case DrawingToolType.Pencil:
                    lblFontSize.Visible = false;
                    cmbFontSize.Visible = false;
                    stlPicker.ToolType = DrawingToolType.Pencil;
                    stlPicker.Top = 158;
                    stlPicker.Left = (grpConfig.Width - stlPicker.Width) / 2;
                    stlPicker.Visible = true;
                    Height = 348;
                    break;
                case DrawingToolType.Line2D:
                    lblFontSize.Visible = false;
                    cmbFontSize.Visible = false;
                    stlPicker.ToolType = DrawingToolType.Line2D;
                    stlPicker.Top = 158;
                    stlPicker.Left = (grpConfig.Width - stlPicker.Width) / 2;
                    stlPicker.Visible = true;
                    Height = 332;
                    break;
                case DrawingToolType.Text:
                    stlPicker.Visible = false;
                    lblFontSize.Visible = true;
                    cmbFontSize.Visible = true;
                    Height = 280;
                    
                    // For text, we need to display the current value.
                    if(m_bPreConfigure)
                    {
                    	cmbFontSize.Text = m_TempColorProfile.FontSizeText.ToString();
                    }
                    else
                    {
                    	cmbFontSize.Text = ((DrawingText)m_Drawing).FontSize.ToString();
                    }
                    break;
                default:
                    // For the rest, only show the color picker.
                    stlPicker.Visible = false;
                    lblFontSize.Visible = false;
                    cmbFontSize.Visible = false;
                    Height = 238;
                    break;
            }
        }
        private void LocalizeForm()
        {
            this.Text = "   " + m_ResourceManager.GetString("dlgConfigureDrawing_Title", Thread.CurrentThread.CurrentUICulture);
            btnCancel.Text = m_ResourceManager.GetString("Generic_Cancel", Thread.CurrentThread.CurrentUICulture);
            btnOK.Text = m_ResourceManager.GetString("Generic_Apply", Thread.CurrentThread.CurrentUICulture);
            grpConfig.Text = m_ResourceManager.GetString("Generic_Configuration", Thread.CurrentThread.CurrentUICulture);
        }
        #endregion

        #region User choices handlers
        private void colPicker_ColorPicked(object sender, EventArgs e)
        {
            if (m_bPreConfigure)
            {
                m_TempColorProfile.UpdateData(m_ToolType, colPicker.PickedColor); 
            }
            else
            {
            	m_Drawing.UpdateDecoration(colPicker.PickedColor);
            	m_SurfaceScreen.Invalidate();
            }
        }
        private void stlPicker_StylePicked(object sender, EventArgs e)
        {
            if (m_bPreConfigure)
            {
                m_TempColorProfile.UpdateData(m_ToolType, stlPicker.PickedStyle);
            }
            else
            {
            	m_Drawing.UpdateDecoration(stlPicker.PickedStyle);
            	m_SurfaceScreen.Invalidate();
            }
        }

        private void cmbFontSize_SelectedValueChanged(object sender, EventArgs e)
        {
            if (m_bPreConfigure)
            {
            	m_TempColorProfile.UpdateData(m_ToolType, int.Parse((string)cmbFontSize.Items[cmbFontSize.SelectedIndex]));
            }
            else
            {
            	m_Drawing.UpdateDecoration(int.Parse((string)cmbFontSize.Items[cmbFontSize.SelectedIndex]));
            	m_SurfaceScreen.Invalidate();
            }
        }
        #endregion
    
        #region OK/Cancel Handlers
        private void btnOK_Click(object sender, EventArgs e)
        {
            if (m_bPreConfigure)
            {
                m_ColorProfile.Load(m_TempColorProfile);
                
                string folder = PreferencesManager.SettingsFolder + PreferencesManager.ResourceManager.GetString("ColorProfilesFolder");
                m_ColorProfile.Save(folder + "\\current.xml");
            }
            else
            {
                // Nothing special to do, the drawing has already been updated.   
            }

            m_bManualClose = true;
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (m_bPreConfigure)
            {
                // Nothing special to do. Do not change the Color Profile.
            }
            else
            {
            	m_Drawing.RecallDecoration();
                m_SurfaceScreen.Invalidate();
            }

            m_bManualClose = true;
        }
        private void formConfigureDrawing_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!m_bManualClose && !m_bPreConfigure)
            {
            	m_Drawing.RecallDecoration();
                m_SurfaceScreen.Invalidate();
            }
        }
        #endregion
    }
}