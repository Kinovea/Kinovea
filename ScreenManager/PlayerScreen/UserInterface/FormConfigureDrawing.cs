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
        private bool m_bPreConfigure;				// true if we are updating the ColorProfile.
    	//private DrawingType m_DrawingType;			// Needed for controls organisation and visibility.
    	private bool m_bManualClose = false;		// Needed to handle cancel button.
    	private ColorPicker m_ColorPicker = new ColorPicker();
    	
    	// For color profile configuration only.
        //private ColorProfile m_ColorProfile;        // Direct ref to the original color profile.
        //private ColorProfile m_TempColorProfile;    // Temporary ColorProfile.
    	
    	// For individual drawing configuration only.
    	private IDecorable m_Drawing;               // The drawing we will modify.
    	private PictureBox m_SurfaceScreen;         // Used to update the image while configuring.
        
        //private int m_MemoFontSize = -1;
        #endregion
        
        #region Construction & Initialization
        /*public formConfigureDrawing(DrawingType _DrawingType, ColorProfile _colorProfile)
        {
            // This constructor is called when we will be updating 
            // the DrawingTool entry in the main ColorProfile.
            
            m_bPreConfigure = true;

            // We will make the updates on a copy of the color profile
            // and only commit them to the real one when user submit the form.
            m_ColorProfile = _colorProfile;
            m_TempColorProfile = new ColorProfile();
            m_TempColorProfile.Load(_colorProfile);
            
            m_DrawingType = _DrawingType;
            
            SetupForm();
        }*/
        public formConfigureDrawing(IDecorable _drawing, PictureBox _SurfaceScreen)
        {
            // This constructor is called when we will be updating a specific drawing.
            m_bPreConfigure = false;
            m_SurfaceScreen = _SurfaceScreen;
            m_Drawing = _drawing;
            //m_DrawingType = m_Drawing.DrawingType;
	        //m_Drawing.MemorizeDecoration();            
	        SetupForm();
        }
        private void SetupForm()
        {
        	InitializeComponent();
        	//ConfigureForm(m_DrawingType);
            LocalizeForm();
        }
        private void ConfigureForm(/*DrawingType _DrawingType*/)
        {
        	// Configure the controls depending on the tool updated.
        	
        	// Color Picker (always visible).
        	/*m_ColorPicker.Top = 18;
			m_ColorPicker.Left = 9;
			
			m_ColorPicker.ColorPicked += new ColorPickedHandler(colorPicker_ColorPicked);
			grpConfig.Controls.Add(m_ColorPicker);
        	
			// TODO: Height should be computed.
            switch (_DrawingType)
            {
            	case DrawingType.Circle:
                case DrawingType.Pencil:
                    lblFontSize.Visible = false;
                    cmbFontSize.Visible = false;
                    stlPicker.DrawingType = DrawingType.Pencil;
                    stlPicker.Top = m_ColorPicker.Bottom + 10;
                    stlPicker.Left = (grpConfig.Width - stlPicker.Width) / 2;
                    stlPicker.Visible = true;
                    grpConfig.Height = stlPicker.Bottom + 20;
                    break;
                case DrawingType.Line:
                    lblFontSize.Visible = false;
                    cmbFontSize.Visible = false;
                    stlPicker.DrawingType = DrawingType.Line;
                    stlPicker.Top = m_ColorPicker.Bottom + 10;
                    stlPicker.Left = (grpConfig.Width - stlPicker.Width) / 2;
                    stlPicker.Visible = true;
                    grpConfig.Height = stlPicker.Bottom + 20;
                    break;
                case DrawingType.Label:
                    stlPicker.Visible = false;
                    lblFontSize.Visible = true;
                    lblFontSize.Top = m_ColorPicker.Bottom + 10;
                    cmbFontSize.Top = m_ColorPicker.Bottom + 10;
                    cmbFontSize.Visible = true;
                    
                    // Initialize font size combo.
                    cmbFontSize.Items.Clear();
                    foreach(int size in InfosTextDecoration.AllowedFontSizes)
                    {
                    	cmbFontSize.Items.Add(size.ToString());
                    }
                    
                    // For text, we need to display the current value.
                    // This value should be one of the AllowedFontSizes.
                    if(m_bPreConfigure)
                    {
                    	cmbFontSize.Text = m_TempColorProfile.FontSizeText.ToString();
                    }
                    else
                    {
                    	//cmbFontSize.Text = ((DrawingText)m_Drawing).FontSize.ToString();
                    	cmbFontSize.Text = "8";
                    }
                    grpConfig.Height = cmbFontSize.Bottom + 20;
                    break;
                default:
                    // For the rest, only show the color picker.
                    stlPicker.Visible = false;
                    lblFontSize.Visible = false;
                    cmbFontSize.Visible = false;
                    grpConfig.Height = m_ColorPicker.Bottom + 20;
                    break;
            }
            
            btnCancel.Top = grpConfig.Bottom + 10;
            btnOK.Top = grpConfig.Bottom + 10;
            this.Height = btnOK.Bottom + 10;*/
        }
        private void LocalizeForm()
        {
            this.Text = "   " + ScreenManagerLang.dlgConfigureDrawing_Title;
            btnCancel.Text = ScreenManagerLang.Generic_Cancel;
            btnOK.Text = ScreenManagerLang.Generic_Apply;
            grpConfig.Text = ScreenManagerLang.Generic_Configuration;
        }
        #endregion

        #region User choices handlers
        private void colorPicker_ColorPicked(object sender, EventArgs e)
        {
            /*if (m_bPreConfigure)
            {
                m_TempColorProfile.UpdateData(m_DrawingType, m_ColorPicker.PickedColor); 
            }
            else
            {
            	//m_Drawing.UpdateDecoration(m_ColorPicker.PickedColor);
            	m_SurfaceScreen.Invalidate();
            }
            
            PreferencesManager.Instance().AddRecentColor(m_ColorPicker.PickedColor);*/
        }
        private void stlPicker_StylePicked(object sender, EventArgs e)
        {
            /*if (m_bPreConfigure)
            {
                m_TempColorProfile.UpdateData(m_DrawingType, stlPicker.PickedStyle);
            }
            else
            {
            	//m_Drawing.UpdateDecoration(stlPicker.PickedStyle);
            	m_SurfaceScreen.Invalidate();
            }*/
        }

        private void cmbFontSize_SelectedValueChanged(object sender, EventArgs e)
        {
            /*if (m_bPreConfigure)
            {
            	m_TempColorProfile.UpdateData(m_DrawingType, int.Parse((string)cmbFontSize.Items[cmbFontSize.SelectedIndex]));
            }
            else
            {
            	//m_Drawing.UpdateDecoration(int.Parse((string)cmbFontSize.Items[cmbFontSize.SelectedIndex]));
            	m_SurfaceScreen.Invalidate();
            }*/
        }
        #endregion
    
        #region OK/Cancel Handlers
        private void btnOK_Click(object sender, EventArgs e)
        {
            /*if (m_bPreConfigure)
            {
                m_ColorProfile.Load(m_TempColorProfile);
                
                string folder = PreferencesManager.SettingsFolder + PreferencesManager.ResourceManager.GetString("ColorProfilesFolder");
                m_ColorProfile.Save(folder + "\\current.xml");
            }
            else
            {
                // Nothing special to do, the drawing has already been updated.   
            }

            m_bManualClose = true;*/
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (m_bPreConfigure)
            {
                // Nothing special to do. Do not change the Color Profile.
            }
            else
            {
            	//m_Drawing.RecallDecoration();
                m_SurfaceScreen.Invalidate();
            }

            m_bManualClose = true;
        }
        private void formConfigureDrawing_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!m_bManualClose && !m_bPreConfigure)
            {
            	//m_Drawing.RecallDecoration();
                m_SurfaceScreen.Invalidate();
            }
        }
        #endregion
    }
}