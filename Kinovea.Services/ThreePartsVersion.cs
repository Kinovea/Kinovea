/*
Copyright © Joan Charmant 2008.
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

using System;

namespace Kinovea.Services
{
    public class ThreePartsVersion
    {
        public int Major;
        public int Minor;
        public int Revision;

        public ThreePartsVersion() { }
        public ThreePartsVersion(string version)
        {
            // version takes the form : "0.6.1"
            if (version != null && version.Length > 0)
            {
                string[] split = version.Split(new Char[] { '.' });
                Major = int.Parse(split[0]);
                Minor = int.Parse(split[1]);
                Revision = int.Parse(split[2]);
            }
        }

        public static bool operator <(ThreePartsVersion a, ThreePartsVersion b)
        {
            if (a == null && b == null)
                return false;
            
            if (a.Major < b.Major)
            {
                return true;
            }
            else if (a.Major > b.Major)
            {
                return false;
            }
            else
            {
                if (a.Minor < b.Minor)
                    return true;
                else if (a.Minor > b.Minor)
                    return false;
                else
                    return a.Revision < b.Revision;
            }
        }
        public static bool operator >(ThreePartsVersion a, ThreePartsVersion b)
        {
            if (a == null && b == null)
                return false;
            
            if (a.Major > b.Major)
            {
                return true;
            }
            else if (a.Major < b.Major)
            {
                return false;
            }
            else
            {
                if (a.Minor > b.Minor)
                    return true;
                else if (a.Minor < b.Minor)
                    return false;
                else
                    return a.Revision > b.Revision;
            }
        }
        public override string ToString()
        {
            return String.Format("{0}.{1}.{2}", Major, Minor, Revision);
        }
    }
}
