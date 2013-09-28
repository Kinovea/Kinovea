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

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Parameters passed to tracking module.
    /// </summary>
    public class TrackerParameters
    {
        public double SimilarityThreshold
        {
            get { return similarityThreshold; }
        }

        public double TemplateUpdateThreshold
        {
            get { return templateUpdateThreshold; }
        }

        public Size SearchWindow
        {
            get { return searchWindow; }
        }

        public Size BlockWindow
        {
            get { return blockWindow; }
        }

        public bool ResetOnMove
        {
            get { return resetOnMove; }
        }


        private double similarityThreshold = 0.50f;
        
        // If simi is better than this, we keep the same template, to avoid the template update drift.
        // When using CCORR : 0.90 or 0.95.
        // When using CCOEFF : 0.80
        private double templateUpdateThreshold = 0.80f;
        private Size searchWindow;
        private Size blockWindow;
        private bool resetOnMove = true;

        public TrackerParameters(double similarityThreshold, double templateUpdateThreshold, Size searchWindow, Size blockWindow, bool resetOnMove)
        {
            this.similarityThreshold = similarityThreshold;
            this.templateUpdateThreshold = templateUpdateThreshold;
            this.searchWindow = searchWindow;
            this.blockWindow = blockWindow;
            this.resetOnMove = resetOnMove;
        }

        public TrackerParameters(TrackingProfile profile, Size imageSize)
        {
            this.similarityThreshold = profile.SimilarityThreshold;
            this.templateUpdateThreshold = profile.TemplateUpdateThreshold;

            this.searchWindow = profile.SearchWindow;
            if (profile.SearchWindowUnit == TrackerParameterUnit.Percentage)
                this.searchWindow = new Size((int)(imageSize.Width * (profile.SearchWindow.Width / 100.0)), (int)(imageSize.Height * (profile.SearchWindow.Height / 100.0)));

            this.blockWindow = profile.BlockWindow;
            if (profile.BlockWindowUnit == TrackerParameterUnit.Percentage)
                this.blockWindow = new Size((int)(imageSize.Width * (profile.BlockWindow.Width / 100.0)), (int)(imageSize.Height * (profile.BlockWindow.Height / 100.0)));

            this.resetOnMove = profile.ResetOnMove;
        }
    }

}
