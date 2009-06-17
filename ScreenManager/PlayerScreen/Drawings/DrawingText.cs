/*
Copyright © Joan Charmant 2008.
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

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Xml;
using System.Windows.Forms;
using Videa.Services;
using System.Resources;
using System.Reflection;
using System.Threading;

namespace Videa.ScreenManager
{
    public class DrawingText : AbstractDrawing
    {
        #region Properties
        public override DrawingToolType ToolType
        {
        	get { return DrawingToolType.Text; }
        }
        public override InfosFading infosFading
        {
            get { return m_InfosFading; }
            set { m_InfosFading = value; }
        }
        public TextBox EditBox
        {
            get { return m_TextBox; }
            set { m_TextBox = value;}
        }
        public PictureBox ContainerScreen
        {
            get { return m_ContainerScreen; }
            set { m_ContainerScreen = value;}
        }
        public bool EditMode
        {
            get { return m_bEditMode; }
            set 
            { 
                m_bEditMode = value;

                //-----------------------------------------------------------
                // Activate or deactivate the ScreenManager Keyboard Handler, 
                // so we can use <space>, <return>, etc.
                //-----------------------------------------------------------
                DelegatesPool dp = DelegatesPool.Instance();
                if (m_bEditMode)
                {    
                    if (dp.DeactivateKeyboardHandler != null)
                    {
                        dp.DeactivateKeyboardHandler();
                    }

                    m_TextBox.Text = m_Text;
                    m_TextBox.Location = new Point(m_ContainerScreen.Left + m_RescaledBackground.X + 6, m_ContainerScreen.Top + m_RescaledBackground.Y + 3);
                    AutoSizeTextbox();
                    m_TextBox.Visible = true;
                    m_TextBox.Focus();
                    m_TextBox.Select(m_TextBox.Text.Length, 0);
                }
                else
                {
                    m_TextBox.Visible = false;

                    if (dp.ActivateKeyboardHandler != null)
                    {
                        dp.ActivateKeyboardHandler();
                    }
                } 
            }
        }
        
        // The following property is used by formConfigureDrawing to show current value.
        // the update is done through the UpdateDecoration methods. 
        public int FontSize
        {
            get { return m_TextStyle.FontSize; }
        }
        #endregion

        #region Members
        private string m_Text;							// Actual text displayed.
        private Color m_TextboxColor;					// Color of the textbox (?)
        private Rectangle m_BackgroundRectangle;		// Sizes of the background. Computed from text length.
        
      	private InfosTextDecoration m_TextStyle;		// Style infos (font, font size, colors)
        private InfosTextDecoration m_MemoTextStyle;	// Used when configuring.
      	private InfosFading m_InfosFading;
        
      	private bool m_bEditMode;
      	
      	private static readonly int m_iDefaultBackgroundAlpha = 128;
      	private static readonly int m_iDefaultFontSize = 16;    		// will also be used for the text box.
        
        private double m_fStretchFactor;
        private Point m_DirectZoomTopLeft;
        private Rectangle m_RescaledBackground;
        private TextBox m_TextBox;
        private PictureBox m_ContainerScreen;
        #endregion

        #region Constructors
        public DrawingText(int x, int y, int width, int height, long _iTimestamp, long _iAverageTimeStampsPerFrame)
        {
            m_Text = " ";
            m_TextboxColor = Color.White; // Color.LightSteelBlue;
            m_BackgroundRectangle = new Rectangle(x, y, width, height);            
            m_TextStyle = new InfosTextDecoration("Arial", m_iDefaultFontSize, FontStyle.Bold, Color.White, Color.CornflowerBlue);

            m_InfosFading = new InfosFading(_iTimestamp, _iAverageTimeStampsPerFrame);
            m_bEditMode = false;

            // Textbox initialization.
            m_TextBox = new TextBox();
            m_TextBox.Visible = false;
            m_TextBox.BackColor = m_TextboxColor;
            m_TextBox.BorderStyle = BorderStyle.None;
            m_TextBox.Text = m_Text;
            m_TextBox.Font = m_TextStyle.GetInternalFont();
            m_TextBox.Multiline = true;
            m_TextBox.TextChanged += new EventHandler(TextBox_TextChanged);
            
            
            m_fStretchFactor = 1.0;
            m_DirectZoomTopLeft = new Point(0, 0);
            RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);
        }
        #endregion

        #region AbstractDrawing Implementation
        public override void Draw(Graphics _canvas, double _fStretchFactor, bool _bSelected, long _iCurrentTimestamp, Point _DirectZoomTopLeft)
        {
            double fOpacityFactor = m_InfosFading.GetOpacityFactor(_iCurrentTimestamp);
            
            if (fOpacityFactor > 0)
            {
                // Rescale the points.
                m_fStretchFactor = _fStretchFactor;
                m_DirectZoomTopLeft = new Point(_DirectZoomTopLeft.X, _DirectZoomTopLeft.Y);
                RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);

                // This method may be used from worker threads and shouldn't touch UI controls.
                // -> The textbox will not be modified here.
                if (!m_bEditMode)
                {
                    DrawBackground(_canvas, fOpacityFactor);
                    SolidBrush fontBrush = new SolidBrush(m_TextStyle.GetFadingForeColor(fOpacityFactor));
                    Font fontText = m_TextStyle.GetInternalFont((float)m_fStretchFactor);
                    _canvas.DrawString(m_Text, fontText, fontBrush, new Point(m_RescaledBackground.X + 5, m_RescaledBackground.Y + 3));
                }
            }
        }
        public override int HitTest(Point _point, long _iCurrentTimestamp)
        {
            // _point is mouse coordinates already descaled.
            // Hit Result: -1: miss, 0: on object.

            int iHitResult = -1;
            double fOpacityFactor = m_InfosFading.GetOpacityFactor(_iCurrentTimestamp);
            if (fOpacityFactor > 0)
            {
                if (IsPointInObject(_point))
                {
                    iHitResult = 0;
                }
            }

            return iHitResult;
        }
        public override void MoveHandleTo(Point point, int handleNumber)
        {
            // Not implemented (No handlers)
        }
        public override void MoveDrawing(int _deltaX, int _deltaY)
        {
            // TODO : image width & height
            if (m_BackgroundRectangle.X + _deltaX >= 0 && m_BackgroundRectangle.Y + _deltaY >= 0  )
            {
               // _delatX and _delatY are mouse delta descaled.
                m_BackgroundRectangle = new Rectangle(m_BackgroundRectangle.X + _deltaX, m_BackgroundRectangle.Y + _deltaY, m_BackgroundRectangle.Width, m_BackgroundRectangle.Height);

                // move TextBox
                if (m_bEditMode && m_TextBox != null)
                {
                    m_TextBox.Top += _deltaY;
                    m_TextBox.Left += _deltaX;
                }

                // Update scaled coordinates accordingly.
                RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);
            }
        }
        public override void ToXmlString(XmlTextWriter _xmlWriter)
        {
            _xmlWriter.WriteStartElement("Drawing");
            _xmlWriter.WriteAttributeString("Type", "DrawingText");

            // Text
            _xmlWriter.WriteStartElement("Text");
            _xmlWriter.WriteString(m_Text);
            _xmlWriter.WriteEndElement();

            // Background StartPoint
            _xmlWriter.WriteStartElement("Position");
            _xmlWriter.WriteString(m_BackgroundRectangle.Left.ToString() + ";" + m_BackgroundRectangle.Top.ToString());
            _xmlWriter.WriteEndElement();

            // Textbox Color
            _xmlWriter.WriteStartElement("TextboxColor");
            _xmlWriter.WriteStartElement("ColorRGB");
            _xmlWriter.WriteString(m_TextboxColor.R.ToString() + ";" + m_TextboxColor.G.ToString() + ";" + m_TextboxColor.B.ToString());
            _xmlWriter.WriteEndElement();
            _xmlWriter.WriteEndElement();

            m_TextStyle.ToXml(_xmlWriter);
            m_InfosFading.ToXml(_xmlWriter, false);

            // </Drawing>
            _xmlWriter.WriteEndElement();
        }
        
        public override void UpdateDecoration(Color _color)
        {
        	m_TextStyle.Update(_color);
        }
        public override void UpdateDecoration(LineStyle _style)
        {
        	throw new Exception(String.Format("{0}, The method or operation is not implemented.", this.ToString()));	
        }
        public override void UpdateDecoration(int _iFontSize)
        {
        	m_TextStyle.Update(_iFontSize);
        	AutoSizeTextbox();
        }
        public override void MemorizeDecoration()
        {
        	m_MemoTextStyle = m_TextStyle.Clone();
        }
        public override void RecallDecoration()
        {
        	m_TextStyle = m_MemoTextStyle.Clone();
        	AutoSizeTextbox();
        }
        
        public override string ToString()
        {
            // Return the name of the tool used to draw this drawing.
            ResourceManager rm = new ResourceManager("Videa.ScreenManager.Languages.ScreenManagerLang", Assembly.GetExecutingAssembly());
            return rm.GetString("ToolTip_DrawingToolText", Thread.CurrentThread.CurrentUICulture);
        }
        public override int GetHashCode()
        {
            // Combine all relevant fields with XOR to get the Hash.
            int iHash = m_Text.GetHashCode();
            iHash ^= m_TextboxColor.GetHashCode();
            iHash ^= m_BackgroundRectangle.GetHashCode();
            iHash ^= m_iDefaultBackgroundAlpha.GetHashCode();
            iHash ^= m_TextStyle.GetHashCode();
            return iHash;
        }
        #endregion

        public static AbstractDrawing FromXml(XmlTextReader _xmlReader, PointF _scale)
        {
            DrawingText dt = new DrawingText(0,0,0,0,0,0);

            while (_xmlReader.Read())
            {
                if (_xmlReader.IsStartElement())
                {
                    if (_xmlReader.Name == "Text")
                    {
                        dt.m_Text = _xmlReader.ReadString();
                    }
                    else if (_xmlReader.Name == "Position")
                    {
                        Point p = XmlHelper.PointParse(_xmlReader.ReadString(), ';');
                        //dt.Background.Location = new Point(plotGrid.X, plotGrid.Y);

                        // Adapt to new Image size.
                        dt.m_BackgroundRectangle.Location = new Point((int)((float)p.X * _scale.X), (int)((float)p.Y * _scale.Y));
                    }
                    else if (_xmlReader.Name == "TextDecoration")
                    {
                    	dt.m_TextStyle = InfosTextDecoration.FromXml(_xmlReader);
                    }
                    else if (_xmlReader.Name == "InfosFading")
                    {
                        dt.m_InfosFading.FromXml(_xmlReader);
                    }
                    else
                    {
                        // forward compatibility : ignore new fields. 
                    }
                }
                else if (_xmlReader.Name == "Drawing")
                {
                    break;
                }
                else
                {
                    // Fermeture d'un tag interne.
                }
            }
            
            dt.RescaleCoordinates(dt.m_fStretchFactor, dt.m_DirectZoomTopLeft);
            dt.AutoSizeTextbox();

            return dt;
        }
        public void RelocateEditbox(double _fStretchFactor, Point _DirectZoomTopLeft)
        {
            m_fStretchFactor = _fStretchFactor;
            m_DirectZoomTopLeft = new Point(_DirectZoomTopLeft.X, _DirectZoomTopLeft.Y);

            RescaleCoordinates(_fStretchFactor, _DirectZoomTopLeft);
            m_TextBox.Location = new Point(m_ContainerScreen.Left + m_RescaledBackground.X + 6, m_ContainerScreen.Top + m_RescaledBackground.Y + 3);
        }

        #region Lower level helpers
        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            m_Text = m_TextBox.Text;
            AutoSizeTextbox();
        }
        private void RescaleCoordinates(double _fStretchFactor, Point _DirectZoomTopLeft)
        {
            m_RescaledBackground = new Rectangle();
            m_RescaledBackground.Location = new Point((int)((double)(m_BackgroundRectangle.X-_DirectZoomTopLeft.X) * _fStretchFactor), (int)((double)(m_BackgroundRectangle.Y-_DirectZoomTopLeft.Y) * _fStretchFactor));
            m_RescaledBackground.Size = new Size((int)((double)m_BackgroundRectangle.Width * _fStretchFactor), (int)((double)m_BackgroundRectangle.Height * _fStretchFactor));
        }
        private bool IsPointInObject(Point _point)
        {
            // Point coordinates are descaled.
            
            // Create path which contains wide line for easy mouse selection
            GraphicsPath areaPath = new GraphicsPath();
            areaPath.AddRectangle(m_BackgroundRectangle);           

            // Create region from the path
            Region areaRegion = new Region(areaPath);

            bool hit =  areaRegion.IsVisible(_point);
            return hit;
        }
        private void AutoSizeTextbox()
        {
            // We add a space at the end for when there is a new line with no characters in it.

            Button but = new Button();
            Graphics g = but.CreateGraphics();
            
            //Size of background
            SizeF bgSize = g.MeasureString(m_Text + " ", m_TextStyle.GetInternalFont());
            m_BackgroundRectangle.Width = (int)bgSize.Width + 8;
            m_BackgroundRectangle.Height = (int)bgSize.Height + 4;
            RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);

            // Size of textbox, we don't use the actual font size (far too big)
            Font f = m_TextStyle.GetInternalFont();
            SizeF edSize = g.MeasureString(m_Text + " ", new Font(f.Name, m_iDefaultFontSize, f.Style));
            m_TextBox.Size = new Size((int)edSize.Width + 8, (int)edSize.Height);

            g.Dispose();
        }
        private void DrawBackground(Graphics _canvas, double _fOpacityFactor)
        {
            // Draw background (Rounded rectangle)
            // The radius for rounding is based on font size.
            double fFontSize = (double)m_TextStyle.FontSize * m_fStretchFactor;
            int radius = (int)(fFontSize / 2);
            int diameter = radius * 2;

            GraphicsPath gp = new GraphicsPath();
            gp.StartFigure();

            gp.AddArc(m_RescaledBackground.X, m_RescaledBackground.Y, diameter, diameter, 180, 90);
            gp.AddLine(m_RescaledBackground.X + radius, m_RescaledBackground.Y, m_RescaledBackground.X + m_RescaledBackground.Width - diameter, m_RescaledBackground.Y);

            gp.AddArc(m_RescaledBackground.X + m_RescaledBackground.Width - diameter, m_RescaledBackground.Y, diameter, diameter, 270, 90);
            gp.AddLine(m_RescaledBackground.X + m_RescaledBackground.Width, m_RescaledBackground.Y + radius, m_RescaledBackground.X + m_RescaledBackground.Width, m_RescaledBackground.Y + m_RescaledBackground.Height - diameter);

            gp.AddArc(m_RescaledBackground.X + m_RescaledBackground.Width - diameter, m_RescaledBackground.Y + m_RescaledBackground.Height - diameter, diameter, diameter, 0, 90);
            gp.AddLine(m_RescaledBackground.X + m_RescaledBackground.Width - radius, m_RescaledBackground.Y + m_RescaledBackground.Height, m_RescaledBackground.X + radius, m_RescaledBackground.Y + m_RescaledBackground.Height);

            gp.AddArc(m_RescaledBackground.X, m_RescaledBackground.Y + m_RescaledBackground.Height - diameter, diameter, diameter, 90, 90);
            gp.AddLine(m_RescaledBackground.X, m_RescaledBackground.Y + m_RescaledBackground.Height - radius, m_RescaledBackground.X, m_RescaledBackground.Y + radius);

            gp.CloseFigure();

            _canvas.FillPath(new SolidBrush(Color.FromArgb((int)((double)m_iDefaultBackgroundAlpha * _fOpacityFactor), m_TextStyle.BackColor)), gp);
        }
        private void ShiftCoordinates(int _iShiftHorz, int _iShiftVert, double _fStretchFactor)
        {
            m_RescaledBackground = new Rectangle();
            m_RescaledBackground.Location = new Point((int)((double)m_BackgroundRectangle.X * _fStretchFactor) + _iShiftHorz, (int)((double)m_BackgroundRectangle.Y * _fStretchFactor) + _iShiftVert);
            m_RescaledBackground.Size = new Size(m_BackgroundRectangle.Width, m_BackgroundRectangle.Height);
        }
        #endregion

    }
}
