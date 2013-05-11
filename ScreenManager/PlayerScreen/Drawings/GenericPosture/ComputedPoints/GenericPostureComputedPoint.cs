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

namespace Kinovea.ScreenManager
{
    public class GenericPostureComputedPoint : IWeightedPoint
    {
        public float Weight { get; private set;}
        public string Name { get; private set;}
        
        private List<IWeightedPoint> weightedPoints = new List<IWeightedPoint>();
        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public GenericPostureComputedPoint(XmlReader r)
        {
            Weight = 1.0F;
            
            bool isEmpty = r.IsEmptyElement;
            
            if(r.MoveToAttribute("name"))
                Name = r.ReadContentAsString();
            
            if(r.MoveToAttribute("weight"))
                Weight = r.ReadContentAsFloat();
            
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
            
            log.DebugFormat("Parsed computed point \"{0}\" successfully.", Name);
            
            r.ReadEndElement();
            
            CheckTotalWeight();
        }
    
        public PointF ComputeLocation(GenericPosture posture)
        {
            // TODO: if there are only two weighted points, allow for the specification of only one weight.
            
            PointF result = PointF.Empty;

            foreach(IWeightedPoint weightedPoint in weightedPoints)
            {
                PointF location = weightedPoint.ComputeLocation(posture);
                PointF scaled = location.Scale(weightedPoint.Weight, weightedPoint.Weight);
                result = result.Translate(scaled.X, scaled.Y);
            }
            
            return result;
        }
        
        private void CheckTotalWeight()
        {
            float totalWeight = 0;
            
            foreach(IWeightedPoint weightedPoint in weightedPoints)
                totalWeight += weightedPoint.Weight;
            
            if(totalWeight != 1.0F)
                log.ErrorFormat("Warning: total weight for \"{0}\" is {1:0.000} instead of 1", Name, totalWeight);
        }
    }
}
