using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Kinovea.ScreenManager
{
    public class TrackerBlockParameters
    {
        public float SimilarityThreshold
        {
            get { return similarityThreshold; }
        }

        public float TemplateUpdateThreshold
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


        private float similarityThreshold = 0.50f;
        
        // If simi is better than this, we keep the same template, to avoid the template update drift.
        // When using CCORR : 0.90 or 0.95.
        // When using CCOEFF : 0.80
        private float templateUpdateThreshold = 0.80f;

        private Size searchWindow;
        private Size blockWindow;

        public TrackerBlockParameters(float similarityThreshold, float templateUpdateThreshold, Size searchWindow, Size blockWindow)
        {
            this.similarityThreshold = similarityThreshold;
            this.templateUpdateThreshold = templateUpdateThreshold;
            this.searchWindow = searchWindow;
            this.blockWindow = blockWindow;
        }
    }
}
