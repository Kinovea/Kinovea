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
using System.Xml;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// Class to hold a set of decorations informations for the various tools.
	/// This is used to load/save/share a predefined set of decorations informations.
	/// The class doesn't fully use polymorphisms on DrawingTools to simplify understanding.
	/// The DrawingTools are grouped in two big categories, Texts and Lines.
	/// Both categories implements a set of common methods to update, load and save decoration data.
	/// </summary>
    public class ColorProfile
    {
        #region Properties
        // Folowing properties are used in formColorProfile to set up current values.
        public Color ColorAngle2D
        {
            get { return m_DecorationAngle2D.BackColor; }
        }
        public Color ColorChrono
        {
            get { return m_DecorationChrono.BackColor; }
        }
        public Color ColorCross2D
        {
            get { return m_DecorationCross2D.Color; }
        }
        public Color ColorLine2D
        {
            get { return m_DecorationLine2D.Color; }
        }
        public Color ColorPencil
        {
            get { return m_DecorationPencil.Color; }
        }
        public Color ColorText
        {
            get { return m_DecorationText.BackColor; }
        }
        public LineStyle StyleLine2D
        {
        	get { return m_DecorationLine2D; }
        }
        public LineStyle StylePencil
        {
            get { return m_DecorationPencil; }
        }
        public int FontSizeText
        {
        	get { return m_DecorationText.FontSize; }
        }
        public int FontSizeChrono
        {
        	get { return m_DecorationChrono.FontSize; }
        }
        #endregion

        #region Members
        private InfosTextDecoration m_DecorationAngle2D;
        private InfosTextDecoration m_DecorationChrono;
        private LineStyle m_DecorationCross2D;
        private LineStyle m_DecorationLine2D;
        private LineStyle m_DecorationPencil;
        private InfosTextDecoration m_DecorationText;
        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public ColorProfile()
        {
            // Default values
            m_DecorationAngle2D = new InfosTextDecoration(8, Color.DarkOliveGreen);
            m_DecorationChrono = new InfosTextDecoration(10, Color.Black);
            m_DecorationCross2D = new LineStyle(1, LineShape.Simple, Color.CornflowerBlue);
            m_DecorationLine2D = new LineStyle(3, LineShape.Simple, Color.LightGreen);
            m_DecorationPencil = new LineStyle(6, LineShape.Simple, Color.LightGreen);
            m_DecorationText = new InfosTextDecoration(10, Color.CornflowerBlue);
        }
		#endregion
        
        public void Save(string _filePath)
        {
        	log.Debug("Exporting color profile to xml file.");
            try
            {
                // Save to XML file
                XmlTextWriter PreferencesWriter = new XmlTextWriter(_filePath, null);
                PreferencesWriter.Formatting = Formatting.Indented;
                PreferencesWriter.WriteStartDocument();
                PreferencesWriter.WriteStartElement("KinoveaColorProfile");

                // Format version.
                PreferencesWriter.WriteStartElement("FormatVersion");
                PreferencesWriter.WriteString("2.0");
                PreferencesWriter.WriteEndElement();

                // Data.
                PreferencesWriter.WriteStartElement("Angle2D");
                m_DecorationAngle2D.ToXml(PreferencesWriter);
                PreferencesWriter.WriteEndElement();
                
                PreferencesWriter.WriteStartElement("Chrono");
                m_DecorationChrono.ToXml(PreferencesWriter);
                PreferencesWriter.WriteEndElement();
                
                PreferencesWriter.WriteStartElement("Cross2D");
                m_DecorationCross2D.ToXml(PreferencesWriter);
                PreferencesWriter.WriteEndElement();
                
                PreferencesWriter.WriteStartElement("Line2D");
                m_DecorationLine2D.ToXml(PreferencesWriter);
                PreferencesWriter.WriteEndElement();
                
                PreferencesWriter.WriteStartElement("Pencil");
                m_DecorationPencil.ToXml(PreferencesWriter);
                PreferencesWriter.WriteEndElement();
                
                PreferencesWriter.WriteStartElement("Text");
                m_DecorationText.ToXml(PreferencesWriter);
                PreferencesWriter.WriteEndElement();
                
                PreferencesWriter.WriteEndElement();// </KinoveaColorProfile>
                PreferencesWriter.WriteEndDocument();
                PreferencesWriter.Flush();
                PreferencesWriter.Close();
               
                #region Old 1.1 format
                /*
                // Colors
                PreferencesWriter.WriteStartElement("TextColorRGB");
                PreferencesWriter.WriteString(m_ColorText.R.ToString() + ";" + m_ColorText.G.ToString() + ";" + m_ColorText.B.ToString());
                PreferencesWriter.WriteEndElement();

                PreferencesWriter.WriteStartElement("PencilColorRGB");
                PreferencesWriter.WriteString(m_ColorPencil.R.ToString() + ";" + m_ColorPencil.G.ToString() + ";" + m_ColorPencil.B.ToString());
                PreferencesWriter.WriteEndElement();

                PreferencesWriter.WriteStartElement("PencilSize");
                PreferencesWriter.WriteString(m_StylePencil.Size.ToString());
                PreferencesWriter.WriteEndElement();

                PreferencesWriter.WriteStartElement("Cross2DColorRGB");
                PreferencesWriter.WriteString(m_ColorCross2D.R.ToString() + ";" + m_ColorCross2D.G.ToString() + ";" + m_ColorCross2D.B.ToString());
                PreferencesWriter.WriteEndElement();

                PreferencesWriter.WriteStartElement("Line2DColorRGB");
                PreferencesWriter.WriteString(m_ColorLine2D.R.ToString() + ";" + m_ColorLine2D.G.ToString() + ";" + m_ColorLine2D.B.ToString());
                PreferencesWriter.WriteEndElement();

                PreferencesWriter.WriteStartElement("Line2DSize");
                PreferencesWriter.WriteString(m_StyleLine2D.Size.ToString());
                PreferencesWriter.WriteEndElement();

                PreferencesWriter.WriteStartElement("Line2DStartArrow");
                PreferencesWriter.WriteString(m_StyleLine2D.StartArrow.ToString());
                PreferencesWriter.WriteEndElement();

                PreferencesWriter.WriteStartElement("Line2DEndArrow");
                PreferencesWriter.WriteString(m_StyleLine2D.EndArrow.ToString());
                PreferencesWriter.WriteEndElement();


                PreferencesWriter.WriteStartElement("Angle2DColorRGB");
                PreferencesWriter.WriteString(m_ColorAngle2D.R.ToString() + ";" + m_ColorAngle2D.G.ToString() + ";" + m_ColorAngle2D.B.ToString());
                PreferencesWriter.WriteEndElement();

                PreferencesWriter.WriteStartElement("ChronoColorRGB");
                PreferencesWriter.WriteString(m_ColorChrono.R.ToString() + ";" + m_ColorChrono.G.ToString() + ";" + m_ColorChrono.B.ToString());
                PreferencesWriter.WriteEndElement();
                */
               #endregion
            }
            catch
            {
                // Most probable cause : Folder not found.
                // Other cause: doesn't have rights to write.
                log.Error("Error while saving color profile file.");
                log.Error("Tried to write to : " + _filePath);
            }
        }
        public void Load(string _filePath)
        {
            log.Debug("Loading color profile from xml file.");
            
            XmlReader reader = new XmlTextReader(_filePath);

            if (reader != null)
            {
                try
                {
                    while (reader.Read())
                    {
                        if ((reader.IsStartElement()) && (reader.Name == "KinoveaColorProfile"))
                        {
                            while (reader.Read())
                            {
                                if (reader.IsStartElement())
                                {
                                	// TODO: Do not try to read from older format (1.1)
                                	
                                	switch (reader.Name)
                                    {
                                		case "Angle2D":
                                			m_DecorationAngle2D = ParseTextDecorationEntry(reader, reader.Name);
                                			break;
										case "Chrono":
                                			m_DecorationChrono = ParseTextDecorationEntry(reader, reader.Name);
                                			break;
                                		case "Cross2D":
                                			m_DecorationCross2D = ParseLineStyleEntry(reader, reader.Name);
                                			break;
										case "Line2D":
                                			m_DecorationLine2D = ParseLineStyleEntry(reader, reader.Name);
                                			break; 
                                		case "Pencil":
                                			m_DecorationPencil = ParseLineStyleEntry(reader, reader.Name);
                                			break;
										case "Text":
                                			m_DecorationText = ParseTextDecorationEntry(reader, reader.Name);
                                			break; 
                                		default:
                                            // DrawingTool from a newer file format...
                                            // We don't have a holder variable for it: ignore.
                                            break;
                                	}
                                	
                                	#region old format 1.1
                                	/*
                                    switch (reader.Name)
                                    {
                                        case "TextColorRGB":
                                            m_ColorText = XmlHelper.ColorParse(reader.ReadString(), ';');
                                            break;
                                        case "PencilColorRGB":
                                            m_ColorPencil = XmlHelper.ColorParse(reader.ReadString(), ';');
                                            break;
                                        case "PencilSize":
                                            m_StylePencil.Size = int.Parse(reader.ReadString());
                                            break;
                                        case "Cross2DColorRGB":
                                            m_ColorCross2D = XmlHelper.ColorParse(reader.ReadString(), ';');
                                            break;
                                        case "Line2DColorRGB":
                                            m_ColorLine2D = XmlHelper.ColorParse(reader.ReadString(), ';');
                                            break;
                                        case "Line2DSize":
                                            m_StyleLine2D.Size = int.Parse(reader.ReadString());
                                            break;
                                        case "Line2DStartArrow":
                                            m_StyleLine2D.StartArrow = bool.Parse(reader.ReadString());
                                            break;
                                        case "Line2DEndArrow":
                                            m_StyleLine2D.EndArrow = bool.Parse(reader.ReadString());
                                            break;
                                        case "Angle2DColorRGB":
                                            m_ColorAngle2D = XmlHelper.ColorParse(reader.ReadString(), ';');
                                            break;
                                        case "ChronoColorRGB":
                                            m_ColorChrono = XmlHelper.ColorParse(reader.ReadString(), ';');
                                            break;
                                        default:
                                            // Parameter from a newer file format...
                                            // We don't have a holder variable for it: ignore.
                                            break;
                                    }*/
                                	#endregion
                                	
                                }
                                else if (reader.Name == "KinoveaColorProfile")
                                {
                                    break;
                                }
                                else
                                {
                                    // Fermeture d'un tag interne.
                                }
                            }
                        }
                    }

                }
                catch (Exception)
                {
                    log.Error("Error happenned while parsing color profile. We'll keep the default values.");
					log.Error("File we tried to read was :" + _filePath);                    
                }
                finally
                {
                    reader.Close();
                }
            }
        }
        private LineStyle ParseLineStyleEntry(XmlReader _reader, string _element)
        {
        	LineStyle ls = new LineStyle();
        	
        	while (_reader.Read())
            {
                if (_reader.IsStartElement())
                {
                	if(_reader.Name == "LineStyle")
                	{
                		ls = LineStyle.FromXml(_reader);	
                	}
                }
                else if (_reader.Name == _element)
                {
                    break;
                }
                else
                {
                    // Fermeture d'un tag interne.
                }
        	}
        	
        	return ls;
        }
        private InfosTextDecoration ParseTextDecorationEntry(XmlReader _reader, string _element)
        {
        	InfosTextDecoration itd = new InfosTextDecoration();
        	
        	while (_reader.Read())
            {
                if (_reader.IsStartElement())
                {
                	if(_reader.Name == "TextDecoration")
                	{
                		itd = InfosTextDecoration.FromXml(_reader);	
                	}
                }
                else if (_reader.Name == _element)
                {
                    break;
                }
                else
                {
                    // Fermeture d'un tag interne.
                }
        	}
        	
        	return itd;
        }
        public void Load(ColorProfile _origin)
        {
            log.Debug("Loading color profile from object.");
            
            this.m_DecorationAngle2D = _origin.m_DecorationAngle2D.Clone();
            this.m_DecorationChrono = _origin.m_DecorationChrono.Clone();
            this.m_DecorationCross2D = _origin.m_DecorationCross2D.Clone();
            this.m_DecorationLine2D = _origin.m_DecorationLine2D.Clone();
            this.m_DecorationPencil = _origin.m_DecorationPencil.Clone();
            this.m_DecorationText = _origin.m_DecorationText.Clone();
        }
        public void UpdateData(DrawingToolType _tool, Color _color)
        {
        	// Update a ColorProfile entry from specified color.
        	// This method is only used to update the color.
        	
        	switch (_tool)
            {
        		case DrawingToolType.Angle2D:
        			m_DecorationAngle2D.Update(_color);
        			break;
				case DrawingToolType.Chrono:
        			m_DecorationChrono.Update(_color);
        			break;
        		case DrawingToolType.Cross2D:
        			m_DecorationCross2D.Update(_color);
        			break;
                case DrawingToolType.Line2D:
                    m_DecorationLine2D.Update(_color);
                    break;
                case DrawingToolType.Pencil:
					m_DecorationPencil.Update(_color);
                    break;
                case DrawingToolType.Text:
                    m_DecorationText.Update(_color);
                    break;
                default:
                    // These tools do not have any color info. (shouldn't happen) 
                    break;
            }
        }
        public void UpdateData(DrawingToolType _tool, LineStyle _style)
        {
        	// Update a ColorProfile entry from specified _style.
        	// This method is only used to update the line shape and line size.
        	// The _style doesn't need to be complete. (We won't use color info here)
        	
        	// Note: We could have used polymorphism here and ask the DrawingTools to 
        	// update the decoration entry themselves.
        	// To do this we would need a table of DrawingTools and a table of Decorations
        	// both indexed by DrawingToolsType.
        	// All in all it feels simpler to just get the tools as independant classes.
        	
			switch (_tool)
            {
                case DrawingToolType.Pencil:
					m_DecorationPencil.Update(_style, false, true, true);
                    break;
                case DrawingToolType.Line2D:
                    m_DecorationLine2D.Update(_style, false, true, true);
                    break;
                case DrawingToolType.Angle2D:
                case DrawingToolType.Chrono: 
                case DrawingToolType.Cross2D:
                case DrawingToolType.Text:
                default:
                    // These tools do not have any line shape / line size info. 
                    break;
            }
        }
        public void UpdateData(DrawingToolType _tool, int _iFontSize)
        {
        	// Update a ColorProfile entry from specified font size.
        	// This method is only used to update the font size.
        	switch (_tool)
            {
        		case DrawingToolType.Angle2D:
        			// Actually not used for now.
                    m_DecorationAngle2D.Update(_iFontSize);
                    break;
                case DrawingToolType.Chrono:
					m_DecorationChrono.Update(_iFontSize);
                    break;
                case DrawingToolType.Text:
                    m_DecorationText.Update(_iFontSize);
                    break;
                case DrawingToolType.Cross2D: 
                case DrawingToolType.Line2D:
                case DrawingToolType.Pencil:
                default:
                    // These tools do not have any font size info. 
                    break;
            }
        	
        }
        public void SetupDrawing(AbstractDrawing _drawing, DrawingToolType _tool)
        {	
        	// Modify a drawing instance according to the current value for its parent tool.
        	switch (_tool)
            {
        		case DrawingToolType.Angle2D:
        			_drawing.UpdateDecoration(m_DecorationAngle2D.BackColor);
        			_drawing.UpdateDecoration(m_DecorationAngle2D.FontSize);
                    break;
                case DrawingToolType.Chrono: 
                    _drawing.UpdateDecoration(m_DecorationChrono.BackColor);
        			_drawing.UpdateDecoration(m_DecorationChrono.FontSize);
                    break;
                case DrawingToolType.Cross2D: 
                    _drawing.UpdateDecoration(m_DecorationCross2D.Color);
        			_drawing.UpdateDecoration(m_DecorationCross2D);
                    break;
				case DrawingToolType.Line2D: 
                    _drawing.UpdateDecoration(m_DecorationLine2D.Color);
        			_drawing.UpdateDecoration(m_DecorationLine2D);
                    break;
                case DrawingToolType.Pencil: 
                    _drawing.UpdateDecoration(m_DecorationPencil.Color);
        			_drawing.UpdateDecoration(m_DecorationPencil);
                    break;
                case DrawingToolType.Text: 
                    _drawing.UpdateDecoration(m_DecorationText.BackColor);
        			_drawing.UpdateDecoration(m_DecorationText.FontSize);
                    break;
                default:
                    // Unsupported tool type.
                    break;
            }
        }
    }
}
