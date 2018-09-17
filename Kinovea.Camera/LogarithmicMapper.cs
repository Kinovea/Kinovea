#region License
/*
Copyright © Joan Charmant 2013.
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

namespace Kinovea.Camera
{
    /// <summary>
    /// Maps a linear range to a log range. This is useful to create sliders working on log scale.
    /// "Data" variables are the original full range data. (ex: 1 to 10⁶)
    /// "Proxy" variables are the values used in the slider. (ex: 1 to 100).
    /// Input data are inclusive. Both min and max are authorized values.
    /// </summary>
    public class LogarithmicMapper
    {
        private double minData;
        private double maxData;
        private double minProxy;
        private double maxProxy;
        private double dataRange;
        private double proxyRange;
        private double logBase;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public LogarithmicMapper(int minData, int maxData, int minProxy, int maxProxy)
        {
            this.minData = (double)minData;
            this.maxData = (double)maxData;
            this.minProxy = (double)minProxy;
            this.maxProxy = (double)maxProxy;

            dataRange = maxData - minData;
            proxyRange = maxProxy - minProxy;

            if (dataRange == proxyRange)
                throw new ArgumentOutOfRangeException();

            logBase = Math.Pow(dataRange, (1.0 / proxyRange));
        }

        /// <summary>
        /// Maps a data value to proxy value.
        /// </summary>
        public int Map(int dataValue)
        {
            if (dataValue < minData || dataValue > maxData)
                throw new ArgumentOutOfRangeException();

            double shifted = (double)dataValue - minData;

            if (shifted == 0)
                return (int)minProxy;

            double mapped = Math.Log(shifted, logBase);
            int proxyValue = (int)Math.Round(mapped + minProxy);
            return proxyValue;
        }

        /// <summary>
        /// Maps proxy value to data value.
        /// </summary>
        public int Unmap(int proxyValue)
        {
            if (proxyValue < minProxy || proxyValue > maxProxy)
                throw new ArgumentOutOfRangeException();

            double shifted = (double)proxyValue - minProxy;

            if (shifted == 0)
                return (int)minData;

            double mapped = Math.Pow(logBase, shifted);
            int dataValue = (int)Math.Round(mapped + minData);
            return dataValue;
        }

        #region Unit test
        public static void Test()
        {
            Test(1, 1000, 1, 10);
            Test(0, 1000, 0, 10);
            Test(-500, 500, 1, 10);
            Test(-1, 1, 0, 10);
            Test(0, 10, 0, 1000);
            //Test(0, 1, 0, 1);
        }
        private static void Test(int minData, int maxData, int minProxy, int maxProxy)
        {
            LogarithmicMapper mapper = new LogarithmicMapper(minData, maxData, minProxy, maxProxy);

            int m1 = mapper.Map(minData);
            int m2 = mapper.Map(maxData);
            int u1 = mapper.Unmap(minProxy);
            int u2 = mapper.Unmap(maxProxy);

            log.Debug("----------------------------------------");
            log.DebugFormat("data:[{0}..{1}], proxy:[{2}..{3}]", minData, maxData, minProxy, maxProxy);
            log.DebugFormat("data:{0}, proxy:{1}", minData, m1);
            log.DebugFormat("data:{0}, proxy:{1}", maxData, m2);
            log.DebugFormat("proxy:{0}, data:{1}", minProxy, u1);
            log.DebugFormat("proxy:{0}, data:{1}", maxProxy, u2);

            /*
            //int m3 = mapper.Map((maxData - minData) / 2);
            //int u3 = mapper.Unmap((maxProxy - minProxy) / 2);
            
            int m4 = mapper.Map(0);
            int m5 = mapper.Map(1001);
            int u4 = mapper.Unmap(0);
            int u5 = mapper.Unmap(11);*/
        }
        #endregion
    }
}
