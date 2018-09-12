﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Kinovea.Services;
using System.Drawing;
using System.Reflection;
using Kinovea.ScreenManager.Languages;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// A generic drawing tool (drawing factory) for tools defined in XML.
    /// This is mostly used to have several tools generating the same underlying drawing but providing different predefined style presets.
    /// As an example, the arrow and line tools are both generating DrawingLine, which has both capabilities.
    /// The fact that the endpoints display arrows or not is just a style attribute of the drawing.
    /// This is not the same as the GenericPosture tool.
    /// 
    /// This framework also allows for the removal of the DrawingTool*** classes that are now generic and described purely in XML.
    /// In order to be compatible, the constructor for generated drawings must conform to a predefined prototype.
    /// </summary>
    public class DrawingTool : AbstractDrawingTool
    {
        #region Properties
        public override string Name
        {
            get { return name; }
        }
        public override string DisplayName
        {
            get 
            {
                if (string.IsNullOrEmpty(displayName))
                    return name;

                string localized = ScreenManagerLang.ResourceManager.GetString(displayName);

                if (string.IsNullOrEmpty(localized))
                    return name;

                return localized;
            }
        }
        public override Bitmap Icon
        {
            get { return icon; }
        }
        public override bool Attached
        {
            get { return attached; }
        }
        public override bool KeepTool
        {
            get { return keepToolAfterDrawingCreation; }
        }
        public override bool KeepToolFrameChanged
        {
            get { return keepToolAfterFrameChange; }
        }
        public override DrawingStyle StylePreset
        {
            get { return currentStyle; }
            set { currentStyle = value; }
        }
        public override DrawingStyle DefaultStylePreset
        {
            get { return defaultStyle; }
        }
        public Type DrawingType
        {
            get { return drawingType; }
        }
        #endregion

        #region Members
        private Guid id;
        private string name;
        private string displayName;
        private Bitmap icon;
        private Type drawingType;
        private bool attached;
        private bool keepToolAfterDrawingCreation;
        private bool keepToolAfterFrameChange;
        private DrawingStyle currentStyle;
        private DrawingStyle defaultStyle;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public DrawingTool(Guid id, string name, string displayName, Bitmap icon, Type drawingType, bool attached, bool keepToolAfterDrawingCreation, bool keepToolAfterFrameChange, DrawingStyle defaultStyle)
        {
            this.id = id;
            this.name = name;
            this.displayName = displayName;
            this.icon = icon;
            this.drawingType = drawingType;
            this.attached = attached;
            this.keepToolAfterDrawingCreation = keepToolAfterDrawingCreation;
            this.keepToolAfterFrameChange = keepToolAfterFrameChange;
            this.defaultStyle = defaultStyle;
            
            this.currentStyle = defaultStyle.Clone();
        }

        public override AbstractDrawing GetNewDrawing(PointF origin, long timestamp, long averageTimeStampsPerFrame, IImageToViewportTransformer transformer)
        {
            // Drawings constructors must conforms to one of two predefined prototypes:
            // (PointF origin, long timestamp, long averageTimeStampsPerFrame, DrawingStyle preset = null, IImageToViewportTransformer transformer = null).
            // (PointF origin, long timestamp, long averageTimeStampsPerFrame, DrawingStyle stylePreset).
            // Exemple for 1: DrawingAngle.
            // Exemple for 2: DrawingText.

            ConstructorInfo ci = drawingType.GetConstructor(new[] { typeof(PointF), typeof(long), typeof(long), typeof(DrawingStyle), typeof(IImageToViewportTransformer) });
            if (ci != null)
            {
                object[] parameters = new object[] { origin, timestamp, averageTimeStampsPerFrame, currentStyle, transformer };
                AbstractDrawing drawing = (AbstractDrawing)Activator.CreateInstance(drawingType, parameters);
                return drawing;
            }
            
            ci = drawingType.GetConstructor(new[] { typeof(PointF), typeof(long), typeof(long), typeof(DrawingStyle)});
            if (ci != null)
            {
                object[] parameters = new object[] { origin, timestamp, averageTimeStampsPerFrame, currentStyle};
                AbstractDrawing drawing = (AbstractDrawing)Activator.CreateInstance(drawingType, parameters);
                return drawing;
            }

            return null;
        }

        #region Deserialization
        public static DrawingTool CreateFromFile(string file)
        {
            DrawingTool result = null;

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            settings.IgnoreProcessingInstructions = true;
            settings.IgnoreWhitespace = true;
            settings.CloseInput = true;

            using (XmlReader r = XmlReader.Create(file, settings))
            {
                try
                {
                    result = ParseTool(r);
                }
                catch (Exception)
                {
                    log.Error("An error happened during the parsing of the tool presets file");
                }
            }

            return result;
        }

        private static DrawingTool ParseTool(XmlReader r)
        {
            r.MoveToContent();
            r.ReadStartElement();

            string version = r.ReadElementContentAsString("FormatVersion", "");
            if (version != "1.0")
            {
                log.ErrorFormat("Unsupported format version ({0}) for tool description.", version);
                return null;
            }

            Guid id = Guid.Empty;
            string name = "";
            string displayName = "";
            Bitmap icon = null;
            Type drawingType = null;
            bool attached = true;
            bool keepToolAfterDrawingCreation = false;
            bool keepToolAfterFrameChange = false;
            DrawingStyle style = null;
            
            while (r.NodeType == XmlNodeType.Element)
            {
                switch (r.Name)
                {
                    case "Id":
                        id = new Guid(r.ReadElementContentAsString());
                        break;
                    case "Name":
                        name = r.ReadElementContentAsString();
                        break;
                    case "DisplayName":
                        displayName = r.ReadElementContentAsString();
                        break;
                    case "Icon":
                        string iconReference = r.ReadElementContentAsString();
                        if (!string.IsNullOrEmpty(iconReference))
                            icon = Properties.Drawings.ResourceManager.GetObject(iconReference) as Bitmap;
                        break;
                    case "DrawingClass":
                        string drawingClass = r.ReadElementContentAsString();
                        drawingType = FindType(drawingClass);
                        break;
                    case "Attached":
                        attached = XmlHelper.ParseBoolean(r.ReadElementContentAsString());
                        break;
                    case "KeepToolAfterDrawingCreation":
                        keepToolAfterDrawingCreation = XmlHelper.ParseBoolean(r.ReadElementContentAsString());
                        break;
                    case "KeepToolAfterFrameChange":
                        keepToolAfterFrameChange = XmlHelper.ParseBoolean(r.ReadElementContentAsString());
                        break;
                    case "DefaultStyle":
                        style = new DrawingStyle(r);
                        break;
                    default:
                        string unparsed = r.ReadOuterXml();
                        log.DebugFormat("Unparsed content in Drawing Tool XML: {0}", unparsed);
                        break;
                }

            }

            r.ReadEndElement();

            if (icon == null)
                icon = Properties.Drawings.generic_posture;

            DrawingTool tool = new DrawingTool(id, name, displayName, icon, drawingType, attached, keepToolAfterDrawingCreation, keepToolAfterFrameChange, style);
            return tool;
        }

        private static Type FindType(string drawingClass)
        {
            // The actual drawing class should be in the current assembly.
            // If drawings can come from plugins we'll have to modify this.
            Type type = Type.GetType(string.Format("Kinovea.ScreenManager.{0}", drawingClass));
            return type;
        }
        #endregion
    }
}
