#region License
/*
Copyright © Joan Charmant 2012.
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
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

using Kinovea.ScreenManager.Languages;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    [XmlType ("CoordinateSystem")]
    public class DrawingCoordinateSystem : AbstractDrawing, IScalable, ITrackable, IMeasurable, IDecorable
    {
        #region Events
        public event EventHandler<TrackablePointMovedEventArgs> TrackablePointMoved; 
        public event EventHandler ShowMeasurableInfoChanged = delegate {}; // not used.
        #endregion
        
        #region Properties
        public DrawingStyle DrawingStyle
        {
            get { return style;}
        }
        public override InfosFading InfosFading
        {
            get { return null; }
            set { }
        }
        public override DrawingCapabilities Caps
        {
            get { return DrawingCapabilities.ConfigureColor | DrawingCapabilities.Track; }
        }
        public override List<ToolStripItem> ContextMenu
        {
            get 
            { 
                // Rebuild the menu to get the localized text.
                List<ToolStripItem> contextMenu = new List<ToolStripItem>();
                
                menuShowAxis.Text = ScreenManagerLang.mnuCoordinateSystemShowAxis;
                menuShowGrid.Text = ScreenManagerLang.mnuCoordinateSystemShowGrid;
                menuShowGraduations.Text = ScreenManagerLang.mnuCoordinateSystemShowTickMarks;
                menuHide.Text = ScreenManagerLang.mnuCoordinateSystemHide;
                
                menuShowAxis.Checked = showAxis;
                menuShowGrid.Checked = showGrid;
                menuShowGraduations.Checked = showGraduations;
                
                contextMenu.Add(menuShowAxis);
                contextMenu.Add(menuShowGrid);
                contextMenu.Add(menuShowGraduations);
                contextMenu.Add(new ToolStripSeparator());
                contextMenu.Add(menuHide);
                
                return contextMenu; 
            }
        }
        
        public bool Visible { get; set; }
        public CalibrationHelper CalibrationHelper { get; set; }
        public bool ShowMeasurableInfo { get; set; }
        #endregion

        #region Members
        private Guid id = Guid.NewGuid();
        private Dictionary<string, Point> points = new Dictionary<string, Point>();
        private bool showAxis = true;
        private bool showGrid;
        private bool showGraduations;
        private Size imageSize;

        // Decoration
        private StyleHelper styleHelper = new StyleHelper();
        private DrawingStyle style;

        // Context menu
        private ToolStripMenuItem menuShowAxis = new ToolStripMenuItem();
        private ToolStripMenuItem menuShowGrid = new ToolStripMenuItem();
        private ToolStripMenuItem menuShowGraduations = new ToolStripMenuItem();
        private ToolStripMenuItem menuHide = new ToolStripMenuItem();
        
        private const int defaultBackgroundAlpha = 92;
        private const int gridAlpha = 92;
        private const int textMargin = 8;
        #endregion

        #region Constructors
        public DrawingCoordinateSystem(Point origin, DrawingStyle stylePreset)
        {
            points["0"] = origin;
            
            // Decoration & binding with editors
            styleHelper.Bicolor = new Bicolor(Color.Empty);
            styleHelper.Font = new Font("Arial", 10, FontStyle.Bold);
            if(stylePreset != null)
            {
                style = stylePreset.Clone();
                BindStyle();
            }
            
            // Context menu
            menuShowAxis.Click += menuShowAxis_Click;
            menuShowGrid.Click += menuShowGrid_Click;
            menuShowGraduations.Click += menuShowGraduations_Click;
            menuHide.Click += menuHide_Click;
            
            menuShowAxis.Image = Properties.Drawings.coordinates_axis;
            menuShowGrid.Image = Properties.Drawings.coordinates_grid;
            menuShowGraduations.Image = Properties.Drawings.coordinates_graduations;
            menuHide.Image = Properties.Drawings.hide;
        }
        #endregion

        #region AbstractDrawing Implementation
        public override void Draw(Graphics canvas, CoordinateSystem transformer, bool selected, long currentTimestamp)
        {
            if(!Visible)
                return;
            
            float widthUserUnit = (float)CalibrationHelper.GetLengthInUserUnit((double)imageSize.Width);
            float stepSizeUserUnit = CalibrationHelper.RulerStepSize(widthUserUnit, 10);
            int stepSizePixels = (int)CalibrationHelper.GetLengthInPixels(stepSizeUserUnit);
            
            Point origin = transformer.Transform(points["0"]);
            Size size = transformer.Transform(imageSize);
            int stepSize = transformer.Transform(stepSizePixels);
            
            using(Pen penLine = styleHelper.GetBackgroundPen(255))
            {
                if(!showGrid && !showGraduations && !showAxis)
                {
                    DrawMarker(canvas, penLine, origin);
                }
                else
                {
                    if(showGrid)
                        DrawGrid(canvas, penLine, origin, size, stepSize);
                    
                    if(showGraduations)
                        DrawGraduations(canvas, penLine, origin, size, stepSize, stepSizeUserUnit);
                    
                    DrawAxis(canvas, penLine, origin, size);
                }
            }
        }
        public override int HitTest(Point point, long currentTimestamp)
        {
            // Convention: miss = -1, object = 0, handle = n.
            if(!Visible)
                return -1;
            
            int result = -1;
            
            if (points["0"].Box(10).Contains(point))
                return 1;
            
            if(showGrid || showGraduations || showAxis)
            {
                if(IsPointOnHorizontalAxis(point))
                    result = 2;
                else if(IsPointOnVerticalAxis(point))
                    result = 3;
            }
            
            return result;
        }
        public override void MoveHandle(Point point, int handleNumber, Keys modifiers)
        {
            if(handleNumber == 1)
                points["0"] = point;
            else if(handleNumber == 2)
                points["0"] = new Point(points["0"].X, point.Y);
            else if(handleNumber == 3)
                points["0"] = new Point(point.X, points["0"].Y);
            
            CalibrationHelper.CoordinatesOrigin = point;
            SignalTrackablePointMoved();
        }
        public override void MoveDrawing(int _deltaX, int _deltaY, Keys _ModifierKeys)
        {
        }
        
        #endregion
        
        public override string ToString()
        {
            return "Coordinate System";
        }
        public override int GetHashCode()
        {
            // The coordinate system position is not yet saved in preferences.
            int iHash = styleHelper.GetHashCode();
            return iHash;
        }
        
        #region ITrackable implementation and support.
        public Guid ID
        {
            get { return id; }
        }
        public Dictionary<string, Point> GetTrackablePoints()
        {
            return points;
        }
        public void SetTracking(bool tracking)
        {
        }
        public void SetTrackablePointValue(string name, Point value)
        {
            if(!points.ContainsKey(name))
                throw new ArgumentException("This point is not bound.");
            
            points[name] = value;
            CalibrationHelper.CoordinatesOrigin = value;
        }
        private void SignalTrackablePointMoved()
        {
            if(TrackablePointMoved == null)
                return;
            
            TrackablePointMoved(this, new TrackablePointMovedEventArgs("0", points["0"]));
        }
        #endregion

        #region IScalable implementation
		public void Scale(Size imageSize)
		{
		    this.imageSize = imageSize;
		    points["0"] = new Point(imageSize.Width / 2, imageSize.Height / 2);
		}
		#endregion
		
        #region Context menu
        private void menuShowAxis_Click(object sender, EventArgs e)
        {
            showAxis = !showAxis;
            CallInvalidateFromMenu(sender);
        }
        private void menuShowGrid_Click(object sender, EventArgs e)
        {
            showGrid = !showGrid;
            CallInvalidateFromMenu(sender);
        }
        private void menuShowGraduations_Click(object sender, EventArgs e)
        {
            showGraduations = !showGraduations;
            CallInvalidateFromMenu(sender);
        }
        private void menuHide_Click(object sender, EventArgs e)
        {
            Visible = false;
            CallInvalidateFromMenu(sender);
        }
        #endregion
        
        #region Lower level helpers
        private void BindStyle()
        {
            style.Bind(styleHelper, "Bicolor", "line color");
        }
        private void DrawAxis(Graphics canvas, Pen pen, Point origin, Size size)
        {
            canvas.DrawLine(pen, 0, origin.Y, size.Width, origin.Y);
            canvas.DrawLine(pen, origin.X, 0, origin.X, size.Height);
        }
        private void DrawMarker(Graphics canvas, Pen pen, Point origin)
        {
            int radius = 5;
            canvas.DrawLine(pen, origin.X - radius, origin.Y, origin.X + radius, origin.Y);
            canvas.DrawLine(pen, origin.X, origin.Y - radius, origin.X, origin.Y + radius);
        }
        private void DrawGrid(Graphics canvas, Pen pen, Point origin, Size size, int stepSize)
        {
            Pen p = new Pen(Color.FromArgb(gridAlpha, pen.Color), pen.Width);
            p.DashStyle = DashStyle.Dash;
            
            DrawLinesAtTicks(canvas, p, origin, size, stepSize, size.Height, size.Width, false);
            
            p.Dispose();
        }
        private void DrawGraduations(Graphics canvas, Pen pen, Point origin, Size size, int stepSize, float stepSizeUserUnit)
        {
            DrawLinesAtTicks(canvas, pen, origin, size, stepSize, 10, 10, true, true, stepSizeUserUnit);
        }
        private void DrawLinesAtTicks(Graphics canvas, Pen pen, Point origin, Size size, int stepSize, int linesHeight, int linesWidth, bool relative)
        {
            DrawLinesAtTicks(canvas, pen, origin, size, stepSize, linesHeight, linesWidth, relative, false, 0);
        }
        private void DrawLinesAtTicks(Graphics canvas, Pen pen, Point origin, Size size, int stepSize, int linesHeight, int linesWidth, bool relative, bool graduations, float stepSizeUserUnit)
        {
            SolidBrush brushFill = styleHelper.GetBackgroundBrush(defaultBackgroundAlpha);
            SolidBrush fontBrush = styleHelper.GetForegroundBrush(255);
            Font font = styleHelper.GetFont(1.0F);
            
            int top = relative ? origin.Y - linesHeight / 2 : 0;
            int bottom = relative ? origin.Y + linesHeight / 2 : linesHeight;
            int left = relative ? origin.X - linesWidth / 2 : 0;
            int right = relative ? origin.X + linesWidth / 2 : linesWidth;
         
            // Vertical lines.
            int x = origin.X;
            int tick = 0;             
            while(x < size.Width)
            {
                canvas.DrawLine(pen, x, top, x, bottom);    
                if(graduations)
                {
                    string grad = String.Format("{0}", (tick * stepSizeUserUnit));
                    PointF tickPosition = new PointF(x, origin.Y);
                    if(tick == 0)
                        DrawStepText(canvas, tickPosition, TextAlignment.BottomRight, grad, brushFill, fontBrush, font);
                    else
                        DrawStepText(canvas, tickPosition, TextAlignment.Bottom, grad, brushFill, fontBrush, font);
                    tick++;
                }
                
                x += stepSize;
            }
            
            x = origin.X - stepSize;
            tick = -1;
            while(x >= 0)
            {
                canvas.DrawLine(pen, x, top, x, bottom);
                
                if(graduations)
                {
                    string grad = String.Format("{0}", tick * stepSizeUserUnit);
                    PointF tickPosition = new PointF(x, origin.Y);
                    DrawStepText(canvas, tickPosition, TextAlignment.Bottom, grad, brushFill, fontBrush, font);
                    tick--;
                }
                
                x -= stepSize;
            }
            
            // Horizontal lines.
            int y = origin.Y;            
            tick = 0;
            while(y < size.Height)
            {
                canvas.DrawLine(pen, left, y, right, y);    
                if(graduations)
                {
                    if(tick != 0)
                    {
                        string grad = String.Format("{0}", -tick * stepSizeUserUnit);
                        PointF tickPosition = new PointF(origin.X, y);
                        DrawStepText(canvas, tickPosition, TextAlignment.Left, grad, brushFill, fontBrush, font);
                    }
                    
                    tick++;
                }
                
                y += stepSize;
            }
            
            y = origin.Y - stepSize;
            tick = -1;
            while(y >= 0)
            {
                canvas.DrawLine(pen, left, y, right, y);
                if(graduations)
                {
                    string grad = String.Format("{0}", -tick * stepSizeUserUnit);
                    PointF tickPosition = new PointF(origin.X, y);
                    DrawStepText(canvas, tickPosition, TextAlignment.Left, grad, brushFill, fontBrush, font);
                    tick--;
                }
                
                y -= stepSize;
            }
            
            brushFill.Dispose();
            fontBrush.Dispose();
            font.Dispose();
        }
        private void DrawStepText(Graphics canvas, PointF tickPosition, TextAlignment textAlignment, string label, SolidBrush brushFill, SolidBrush fontBrush, Font font)
        {
            SizeF labelSize = canvas.MeasureString(label, font);
            PointF textPosition = GetTextPosition(tickPosition, textAlignment, labelSize);
            RectangleF backRectangle = new RectangleF(textPosition, labelSize);
            
            RoundedRectangle.Draw(canvas, backRectangle, brushFill, font.Height/4, false, false, null);
            canvas.DrawString(label, font, fontBrush, backRectangle.Location);
        }
        private PointF GetTextPosition(PointF tickPosition, TextAlignment textAlignment, SizeF textSize)
        {
            PointF textPosition = tickPosition;
            
            switch (textAlignment)
            {
                case TextAlignment.Top: 
                    textPosition = new PointF(tickPosition.X - textSize.Width / 2, tickPosition.Y - textSize.Height - textMargin);
                    break;
                case TextAlignment.Left:
                    textPosition = new PointF(tickPosition.X - textSize.Width - textMargin, tickPosition.Y - textSize.Height / 2);
                    break;
                case TextAlignment.Right:
                    textPosition = new PointF(tickPosition.X + textMargin, tickPosition.Y - textSize.Height / 2);
                    break;
                case TextAlignment.Bottom:
                    textPosition = new PointF(tickPosition.X - textSize.Width / 2, tickPosition.Y + textMargin);
                    break;
                case TextAlignment.BottomRight:
                    textPosition = new PointF(tickPosition.X + textMargin, tickPosition.Y + textMargin);
                    break;
            }
            
            return textPosition;
        }
        private bool IsPointOnHorizontalAxis(Point p)
        {
            int widenRadius = 5;
            Rectangle axis = new Rectangle(0, points["0"].Y - widenRadius, imageSize.Width, widenRadius * 2);
            return axis.Contains(p);
        }
        private bool IsPointOnVerticalAxis(Point p)
        {
            int widenRadius = 5;
            Rectangle axis = new Rectangle(points["0"].X - widenRadius, 0, widenRadius * 2, imageSize.Height);
            return axis.Contains(p);
        }
        #endregion
        
        private enum TextAlignment
        {
            Top,
            Left,
            Right,
            Bottom,
            BottomRight
        }

    }
}

