using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Kinovea.ScreenManager
{
    public class TrackingProfile
    {
        public readonly string Name;
        public readonly double SimilarityThreshold;
        public readonly double TemplateUpdateThreshold;
        public readonly Size SearchWindow;
        public readonly Size BlockWindow;
        public readonly TrackerParameterUnit SearchWindowUnit;
        public readonly TrackerParameterUnit BlockWindowUnit;
        public readonly bool ResetOnMove;

        public TrackingProfile() :
            this("default", 0.5, 0.8, new Size(20, 20), new Size(5, 5), TrackerParameterUnit.Percentage, TrackerParameterUnit.Percentage, true)
        {            
        }

        public TrackingProfile(string name, double similarityThreshold, double templateUpdateThreshold, Size searchWindow, Size blockWindow, TrackerParameterUnit searchWindowUnit, TrackerParameterUnit blockWindowUnit, bool resetOnMove)
        {
            this.Name = name;
            this.SimilarityThreshold = similarityThreshold;
            this.TemplateUpdateThreshold = templateUpdateThreshold;
            this.SearchWindow = searchWindow;
            this.BlockWindow = blockWindow;
            this.SearchWindowUnit = searchWindowUnit;
            this.BlockWindowUnit = blockWindowUnit;
            this.ResetOnMove = resetOnMove;
        }
    }
}
