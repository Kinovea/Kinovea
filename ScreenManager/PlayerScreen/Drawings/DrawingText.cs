#region License
/*
Copyright © Joan Charmant 2008-2011.
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
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public class DrawingText : AbstractDrawing, IXMLSerializable, IDecorable
    {
        #region Properties
        public DrawingType DrawingType
        {
        	get { return DrawingType.Label; }
        }
        public override InfosFading infosFading
        {
            get { return m_InfosFading; }
            set { m_InfosFading = value; }
        }
        public override Capabilities Caps
		{
			get { return Capabilities.ConfigureColorSize | Capabilities.Fading; }
		}
        public override List<ToolStripMenuItem> ContextMenu
		{
			get { return null; }
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
                    m_TextBox.Location = new Point(m_ContainerScreen.Left + m_LabelBackground.Location.X + 6, m_ContainerScreen.Top + m_LabelBackground.Location.Y + 3);
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
        
      	private InfosTextDecoration m_TextStyle;		// Style infos (font, font size, colors)
        private InfosTextDecoration m_MemoTextStyle;	// Used when configuring.
      	private InfosFading m_InfosFading;
        
      	private bool m_bEditMode;
      	private static readonly int m_iDefaultFontSize = 16;    		// will also be used for the text box.
		
      	private Point m_TopLeft;                         	// position (in image coords).
        private double m_fStretchFactor;
        private Point m_DirectZoomTopLeft;
        
        private LabelBackground m_LabelBackground = new LabelBackground();
        private SizeF m_BackgroundSize;				  	// Size of the background rectangle (scaled).
        
        private TextBox m_TextBox;
        private PictureBox m_ContainerScreen;
        #endregion

        #region Constructors
        public DrawingText(int x, int y, int width, int height, long _iTimestamp, long _iAverageTimeStampsPerFrame)
        {
            m_Text = " ";
            m_TextboxColor = Color.White; // Color.LightSteelBlue;
            m_TopLeft = new Point(x, y);
            m_BackgroundSize = new SizeF(100, 20);
            
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
                   _canvas.DrawString(m_Text, fontText, fontBrush, m_LabelBackground.TextLocation);
                   fontBrush.Dispose();
                   fontText.Dispose();
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
            	if(GetHandleRectangle().Contains(_point))
            	{
            		iHitResult = 1;	
            	}
                else if (IsPointInObject(_point))
                {
                    iHitResult = 0;
                }
            }

            return iHitResult;
        }
        public override void MoveHandle(Point point, int handleNumber)
        {	
        	// Invisible handler to change font size.
            // Compare wanted mouse position with current bottom right.
            int wantedHeight = point.Y - m_TopLeft.Y;
            int newFontSize = m_TextStyle.ReverseFontSize(wantedHeight, m_Text);
            UpdateDecoration(newFontSize);
        }
        public override void MoveDrawing(int _deltaX, int _deltaY, Keys _ModifierKeys)
        {
            // Note: _delatX and _delatY are mouse delta already descaled.            
            m_TopLeft.X += _deltaX;
            m_TopLeft.Y += _deltaY;

            if (m_bEditMode && m_TextBox != null)
            {
                m_TextBox.Top += _deltaY;
                m_TextBox.Left += _deltaX;
            }
            
            // Update scaled coordinates accordingly.
            RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);
        }
        #endregion

		#region IXMLSerializable implementation
        public void ToXmlString(XmlTextWriter _xmlWriter)
        {
            _xmlWriter.WriteStartElement("Drawing");
            _xmlWriter.WriteAttributeString("Type", "DrawingText");

            // Text
            _xmlWriter.WriteStartElement("Text");
            _xmlWriter.WriteString(m_Text);
            _xmlWriter.WriteEndElement();

            // Background StartPoint
            _xmlWriter.WriteStartElement("Position");
            _xmlWriter.WriteString(m_TopLeft.X.ToString() + ";" + m_TopLeft.Y.ToString());
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
        #endregion
        
        #region IDecorable implementation
        public void UpdateDecoration(Color _color)
        {
        	m_TextStyle.Update(_color);
        }
        public void UpdateDecoration(LineStyle _style)
        {
        	throw new Exception(String.Format("{0}, The method or operation is not implemented.", this.ToString()));	
        }
        public void UpdateDecoration(int _iFontSize)
        {
        	m_TextStyle.Update(_iFontSize);
        	AutoSizeTextbox();
        }
        public void MemorizeDecoration()
        {
        	m_MemoTextStyle = m_TextStyle.Clone();
        }
        public void RecallDecoration()
        {
        	m_TextStyle = m_MemoTextStyle.Clone();
        	AutoSizeTextbox();
        }
        #endregion
        
        public override string ToString()
        {
            // Return the name of the tool used to draw this drawing.
            ResourceManager rm = new ResourceManager("Kinovea.ScreenManager.Languages.ScreenManagerLang", Assembly.GetExecutingAssembly());
            return rm.GetString("ToolTip_DrawingToolText", Thread.CurrentThread.CurrentUICulture);
        }
        public override int GetHashCode()
        {
            // Combine all relevant fields with XOR to get the Hash.
            int iHash = m_Text.GetHashCode();
            iHash ^= m_TextboxColor.GetHashCode();
            iHash ^= m_TopLeft.GetHashCode();
            iHash ^= m_TextStyle.GetHashCode();
            return iHash;
        }

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
                        dt.m_TopLeft = new Point((int)((float)p.X * _scale.X), (int)((float)p.Y * _scale.Y));
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
            m_TextBox.Location = new Point(m_ContainerScreen.Left + m_LabelBackground.Location.X + 6, m_ContainerScreen.Top + m_LabelBackground.Location.Y + 3);
        }

        #region Lower level helpers
        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            m_Text = m_TextBox.Text;
            AutoSizeTextbox();
        }
        private void RescaleCoordinates(double _fStretchFactor, Point _DirectZoomTopLeft)
        {
        	m_LabelBackground.Location = new Point((int)((double)(m_TopLeft.X-_DirectZoomTopLeft.X) * _fStretchFactor), (int)((double)(m_TopLeft.Y-_DirectZoomTopLeft.Y) * _fStretchFactor));
        }
        private void ShiftCoordinates(int _iShiftHorz, int _iShiftVert, double _fStretchFactor)
        {
            m_LabelBackground.Location = new Point((int)((double)m_TopLeft.X * _fStretchFactor) + _iShiftHorz, (int)((double)m_TopLeft.Y * _fStretchFactor) + _iShiftVert);
        }
        private bool IsPointInObject(Point _point)
        {
            // Point coordinates are descaled.
            // We need to descale the hit area size for coherence.
            Size descaledSize = new Size((int)((m_BackgroundSize.Width + m_LabelBackground.MarginWidth) / m_fStretchFactor), (int)((m_BackgroundSize.Height + m_LabelBackground.MarginHeight) / m_fStretchFactor));

            GraphicsPath areaPath = new GraphicsPath();
            areaPath.AddRectangle(new Rectangle(m_TopLeft.X, m_TopLeft.Y, descaledSize.Width, descaledSize.Height));

            // Create region from the path
            Region areaRegion = new Region(areaPath);

            bool hit = areaRegion.IsVisible(_point);
            return hit;
        }
        private void AutoSizeTextbox()
        {
            // We add a space at the end for when there is a new line with no characters in it.
            Button but = new Button();
            Graphics g = but.CreateGraphics();
           
            // Size of textbox, we don't use the actual font size (far too big)
            Font f = m_TextStyle.GetInternalFontDefault(m_iDefaultFontSize);
            SizeF edSize = g.MeasureString(m_Text + " ", f);
            m_TextBox.Size = new Size((int)edSize.Width + 8, (int)edSize.Height);
            f.Dispose();
            g.Dispose();
        }
        private void DrawBackground(Graphics _canvas, double _fOpacityFactor)
        {
            // Draw background (Rounded rectangle)
            // The radius for rounding is based on font size.
            Font f = m_TextStyle.GetInternalFont((float)m_fStretchFactor);
            m_BackgroundSize = _canvas.MeasureString(m_Text + " ", f);
            int radius = (int)(f.Size / 2);
            f.Dispose();
            
            m_LabelBackground.Draw(_canvas, _fOpacityFactor, radius, (int)m_BackgroundSize.Width, (int)m_BackgroundSize.Height, m_TextStyle.BackColor);
        }
        
        private Rectangle GetHandleRectangle()
        {
            // This function is only used for Hit Testing.
            Size descaledSize = new Size((int)((m_BackgroundSize.Width + m_LabelBackground.MarginWidth) / m_fStretchFactor), (int)((m_BackgroundSize.Height + m_LabelBackground.MarginHeight) / m_fStretchFactor));

            return new Rectangle(m_TopLeft.X + descaledSize.Width - 10, m_TopLeft.Y + descaledSize.Height - 10, 20, 20);
        }
        #endregion
    }
}
