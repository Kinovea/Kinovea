#region License
/*
Copyright © Joan Charmant 2013.
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
using System.Xml;

using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public class GenericPostureComputedPoint : IWeightedPoint
    {
        public float Weight { get; private set;}
        public string Name { get; private set;}
        public Color Color { get; private set; }
        public string Symbol { get; private set;}
        public string OptionGroup { get; private set;}
        public PointF LastPoint { get; private set;}
        public bool DisplayCoordinates { get; set;}
        
        private List<IWeightedPoint> weightedPoints = new List<IWeightedPoint>();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public GenericPostureComputedPoint(XmlReader r)
        {
            Weight = 0.0F;
            Symbol = "";
            Color = Color.Transparent;
            OptionGroup = "";
            DisplayCoordinates = false;
            
            bool isEmpty = r.IsEmptyElement;
            
            if(r.MoveToAttribute("name"))
                Name = r.ReadContentAsString();
            
            if(r.MoveToAttribute("weight"))
                Weight = r.ReadContentAsFloat();
                
            Weight = Math.Max(0.0F, Math.Min(1.0F, Weight));
            
            if(r.MoveToAttribute("symbol"))
                Symbol = r.ReadContentAsString();
            
            if(r.MoveToAttribute("color"))
                Color = XmlHelper.ParseColor(r.ReadContentAsString(), Color);
            
            if(r.MoveToAttribute("optionGroup"))
                OptionGroup = r.ReadContentAsString();
                
            if(r.MoveToAttribute("displayCoordinates"))
                DisplayCoordinates = XmlHelper.ParseBoolean(r.ReadContentAsString());
                
            r.ReadStartElement();
            
            if(isEmpty)
                return;
            
            while(r.NodeType == XmlNodeType.Element)
            {
                if(r.Name == "ComputedPoint")
                {
                    weightedPoints.Add(new GenericPostureComputedPoint(r));
                }
                else if(r.Name == "ReferencePoint")
                {
                    weightedPoints.Add(new GenericPostureReferencePoint(r));
                }
                else
                {
                    string outerXml = r.ReadOuterXml();
                    log.DebugFormat("Unparsed content: {0}", outerXml);
                }
            }
            
            r.ReadEndElement();
            
            CheckTotalWeight();
        }
    
        public PointF ComputeLocation(GenericPosture posture)
        {
            PointF result = PointF.Empty;

            if(weightedPoints.Count == 2)
            {
                // Special case, weight is optionnal if the other point is weighted.
                IWeightedPoint p0 = weightedPoints[0];
                IWeightedPoint p1 = weightedPoints[1];
                
                float w0 = p0.Weight;
                float w1 = p1.Weight;
                
                if(w0 == 0)
                    w0 = 1-w1;
                else if(w1 == 0)
                    w1 = 1-w0;
                
                PointF l0 = p0.ComputeLocation(posture);
                PointF l1 = p1.ComputeLocation(posture);
                
                float x = (l0.X * w0) + (l1.X * w1);
                float y = (l0.Y * w0) + (l1.Y * w1);
                result = new PointF(x,y);
            }
            else
            {
                foreach(IWeightedPoint weightedPoint in weightedPoints)
                {
                    PointF location = weightedPoint.ComputeLocation(posture);
                    PointF scaled = location.Scale(weightedPoint.Weight, weightedPoint.Weight);
                    result = result.Translate(scaled.X, scaled.Y);
                }
            }
            
            LastPoint = result;
            
            return result;
        }
        
        private void CheckTotalWeight()
        {
            if(weightedPoints.Count == 2)
            {
                if(weightedPoints[0].Weight + weightedPoints[1].Weight == 0)
                    log.ErrorFormat("Warning: total weight for \"{0}\" is 0.", Name);
                
                return;
            }
                
            float totalWeight = 0;
            
            foreach(IWeightedPoint weightedPoint in weightedPoints)
                totalWeight += weightedPoint.Weight;
            
            if(totalWeight != 1.0F)
                log.ErrorFormat("Warning: total weight for \"{0}\" is {1:0.000} instead of 1", Name, totalWeight);
        }
    }
}
