﻿#region License
/*
Copyright © Joan Charmant 2011.
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
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Xml;
using System.Linq;

using Kinovea.ScreenManager.Languages;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Spotlights.
    /// This is the proxy object dispatching all spotlights requests. (draw, hit testing, etc.)
    /// </summary>
    public class SpotlightManager : AbstractMultiDrawing, IInitializable
    {
        #region Events
        public event EventHandler<TrackableDrawingEventArgs> TrackableDrawingAdded;
        public event EventHandler<TrackableDrawingEventArgs> TrackableDrawingDeleted;
        #endregion
        
        #region Properties
        public override string DisplayName
        {
            get {  return ScreenManagerLang.ToolTip_DrawingToolSpotlight; }
        }
        public override int ContentHash
        {
            get 
            {
                return spotlights.Aggregate(0, (a, s) => a ^ s.GetHashCode());  
            }
        } 
        public override AbstractMultiDrawingItem SelectedItem 
        {
            get 
            {
                if(selected >= 0 && selected < spotlights.Count)
                    return spotlights[selected];
                else
                    return null;
            }
        }
        public override int Count 
        {
            get { return spotlights.Count; }
        }
        
        // Fading is not currently modifiable from outside.
        public override InfosFading  InfosFading
        {
            get { return null; }
            set { }
        }
        public override DrawingCapabilities Caps
        {
            get { return DrawingCapabilities.Track; }
        }
        public override List<ToolStripItem> ContextMenu
        {
            get { return null; }
        }
        #endregion
        
        #region Members
        private List<Spotlight> spotlights = new List<Spotlight>();
        private int selected = -1;
        private static readonly int defaultBackgroundAlpha = 150; // <-- opacity of the dim layer. Higher value => darker.
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        #region AbstractDrawing Implementation
        public override void Draw(Graphics canvas, DistortionHelper distorter, IImageToViewportTransformer transformer, bool selected, long currentTimestamp)
        {
            // We draw a single translucent black rectangle to cover the whole image.
            // (Opacity varies between 0% and 50%, depending on the opacity factor of the closest spotlight in time)
            if(spotlights.Count < 1)
                return;
            
            // Create a mask rectangle and obliterate spotlights from it.
            // FIXME: spots subtract from each other which is not desirable.
            // TODO: might be better to first get the opacity, then only ask for the path. In case opacity is 0.
            GraphicsPath globalPath = new GraphicsPath();
            globalPath.AddRectangle(canvas.ClipBounds);
            
            // Combine all spots into a single GraphicsPath.
            // Get their opacity in the process to compute the global opacity of the covering rectangle.
            double maxOpacity = 0.0;
            GraphicsPath spotsPath = new GraphicsPath();
            foreach(Spotlight spot in spotlights)
            {
                double opacity = spot.AddSpot(currentTimestamp, spotsPath, transformer);
                maxOpacity = Math.Max(maxOpacity, opacity);
            }
            
            if(maxOpacity <= 0)
                return;
            
            // Obliterate the spots from the mask.
            globalPath.AddPath(spotsPath, false);
            
            // Draw the mask with the spot holes on top of the frame.
            int backgroundAlpha = (int)((double)defaultBackgroundAlpha * maxOpacity);
            using(SolidBrush brushBackground = new SolidBrush(Color.FromArgb(backgroundAlpha, Color.Black)))
            {
                canvas.FillPath(brushBackground, globalPath);
            }
            
            // Draw each spot border or any visuals.
            foreach(Spotlight spot in spotlights)
                spot.Draw(canvas, currentTimestamp);
            
            globalPath.Dispose();
            spotsPath.Dispose();
        }
        public override void MoveDrawing(float dx, float dy, Keys modifiers, bool zooming)
        {
            if(selected >= 0 && selected < spotlights.Count)
                spotlights[selected].MouseMove(dx, dy);
        }
        public override void MoveHandle(PointF point, int handleNumber, Keys modifiers)
        {
            if(selected >= 0 && selected < spotlights.Count)
                spotlights[selected].MoveHandleTo(point);
        }
        public override int HitTest(Point point, long currentTimestamp, DistortionHelper distorter, IImageToViewportTransformer transformer, bool zooming)
        {
            int currentSpot = 0;
            int handle = -1;
            foreach(Spotlight spot in spotlights)
            {
                handle = spot.HitTest(point, currentTimestamp, transformer);
                if(handle >= 0)
                {
                    selected = currentSpot;
                    break;
                }
                currentSpot++;
            }
            
            return handle;
        }
        #endregion
        
        #region AbstractMultiDrawing Implementation
        public override AbstractMultiDrawingItem GetNewItem(PointF point, long position, long averageTimeStampsPerFrame)
        {
            return new Spotlight(position, averageTimeStampsPerFrame, point);
        }
        public override AbstractMultiDrawingItem GetItem(Guid id)
        {
            return spotlights.FirstOrDefault(n => n.Id == id);
        }
        public override void Add(AbstractMultiDrawingItem item)
        {
            Spotlight spotlight = item as Spotlight;
            if(spotlight == null)
                return;
            
            spotlights.Add(spotlight);
            selected = spotlights.Count - 1;
            
            if(TrackableDrawingAdded != null)
                TrackableDrawingAdded(this, new TrackableDrawingEventArgs(spotlight));
        }
        public override void Remove(Guid id)
        {
            spotlights.RemoveAll(s => s.Id == id);
            selected = -1;
        }
        public override void Clear()
        {
            if(TrackableDrawingDeleted != null)
            {
                foreach(Spotlight spotlight in spotlights)
                    TrackableDrawingDeleted(this, new TrackableDrawingEventArgs(spotlight));
            }
            
            spotlights.Clear();
            selected = -1;
        }
        #endregion
        
        #region IInitializable implementation
        public void ContinueSetup(PointF point, Keys modifiers)
        {
            MoveHandle(point, -1, modifiers);
        }
        #endregion
        
        #region Public methods
        public void ReadXml(XmlReader r, PointF scale, TimestampMapper timestampMapper, Metadata metadata)
        {
            Clear();

            r.ReadStartElement();

            while (r.NodeType == XmlNodeType.Element)
            {
                if (r.Name == "Spotlight")
                {
                    AbstractMultiDrawingItem item = MultiDrawingItemSerializer.Deserialize(r, scale, timestampMapper, metadata);
                    Spotlight spotlight = item as Spotlight;
                    if (spotlight != null)
                        metadata.AddMultidrawingItem(this, spotlight);
                }
                else
                {
                    string unparsed = r.ReadOuterXml();
                    log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                }
            }

            r.ReadEndElement();
        }
        public void WriteXml(XmlWriter w, SerializationFilter filter)
        {
            foreach (Spotlight spot in spotlights)
                DrawingSerializer.Serialize(w, spot as IKvaSerializable, filter);
        }
        #endregion
    }
}

