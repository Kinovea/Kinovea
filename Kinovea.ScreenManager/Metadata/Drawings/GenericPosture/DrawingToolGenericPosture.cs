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
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using Kinovea.ScreenManager.Languages;

namespace Kinovea.ScreenManager
{
    public class DrawingToolGenericPosture : AbstractDrawingTool
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
            get { return true; }
        }
        public override bool KeepTool
        {
            get { return false; }
        }
        public override bool KeepToolFrameChanged
        {
            get { return false; }
        }
        public override DrawingStyle StylePreset
        {
            get { return stylePreset;}
            set { stylePreset = value;}
        }
        public override DrawingStyle DefaultStylePreset
        {
            get { return defaultStylePreset;}
        }
        #endregion
        
        #region Members
        private DrawingStyle defaultStylePreset = new DrawingStyle();
        private DrawingStyle stylePreset;
        private Guid id;
        private string name = "GenericPosture";
        private string displayName;
        private Bitmap icon = Properties.Drawings.generic_posture;
        #endregion
        
        #region Constructor
        public DrawingToolGenericPosture()
        {
            defaultStylePreset.Elements.Add("line color", new StyleElementColor(Color.DarkOliveGreen));
            stylePreset = defaultStylePreset.Clone();
        }
        #endregion
        
        #region Public Methods
        public override AbstractDrawing GetNewDrawing(PointF origin, long timestamp, long averageTimeStampsPerFrame, IImageToViewportTransformer transformer)
        {
            if (ToolManager.Tools.ContainsKey(name))
                stylePreset = ToolManager.GetStylePreset(name);
            
            GenericPosture posture = GenericPostureManager.Instanciate(id, false);
            return new DrawingGenericPosture(origin, posture, timestamp, averageTimeStampsPerFrame, stylePreset);
        }
        public override Cursor GetCursor(double stretchFactor)
        {
            return Cursors.Cross;
        }
        public void SetInfo(GenericPosture posture)
        {
            this.id = posture.Id;

            if (!string.IsNullOrEmpty(posture.Name))
                name = posture.Name;

            if (!string.IsNullOrEmpty(posture.DisplayName))
                displayName = posture.DisplayName;
            
            if(posture.Icon != null && posture.Icon.Width == 16 && posture.Icon.Height == 16)
              icon = posture.Icon;
        }
        #endregion
    }
}




