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
using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// A simple wrapper around a dash style and the presence of time ticks.
    /// Used to describe line shape for tracks.
    /// </summary>
    [TypeConverter(typeof(TrackShapeConverter))]
    public struct TrackShape
    {
        public readonly DashStyle DashStyle;
        public readonly bool ShowSteps;

        #region Static Properties
        private static TrackShape solid = new TrackShape(DashStyle.Solid, false);
        public static TrackShape Solid
        {
            get { return solid; }
        }
        private static TrackShape dash = new TrackShape(DashStyle.Dash, false);
        public static TrackShape Dash
        {
            get { return dash; }
        }
        private static TrackShape solidSteps = new TrackShape(DashStyle.Solid, true);
        public static TrackShape SolidSteps
        {
            get { return solidSteps; }
        }
        private static TrackShape dashSteps = new TrackShape(DashStyle.Dash, true);
        public static TrackShape DashSteps
        {
            get { return dashSteps; }
        }
        #endregion

        public TrackShape(DashStyle style, bool steps)
        {
            DashStyle = style;
            ShowSteps = steps;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is TrackShape))
                return false;
            return Equals((TrackShape)obj);
        }
        public bool Equals(TrackShape other)
        {
            return DashStyle == other.DashStyle && ShowSteps == other.ShowSteps;
        }
        public static bool operator ==(TrackShape v1, TrackShape v2)
        {
            return v1.Equals(v2);
        }
        public static bool operator !=(TrackShape v1, TrackShape v2)
        {
            return !v1.Equals(v2);
        }
        public override int GetHashCode()
        {
            return DashStyle.GetHashCode() ^ ShowSteps.GetHashCode();
        }
    }
}