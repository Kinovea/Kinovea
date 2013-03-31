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
using System.Drawing;

namespace Kinovea.Camera.HTTP
{
    /// <summary>
    /// Information about a camera that is specific to the HTTP plugin.
    /// This info is opaquely transported inside the CameraSummary.
    /// </summary>
    public class SpecificInfo
    {
        public string User { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public string Port { get; set; }
        public string Path { get; set; }
        public string Format { get; set; }
        
        public SpecificInfo Clone()
        {
            SpecificInfo specific = new SpecificInfo();
            specific.User = this.User;
            specific.Password = this.Password;
            specific.Host = this.Host;
            specific.Port = this.Port;
            specific.Path = this.Path;
            specific.Format = this.Format;
            return specific;
        }
        
        #region Equals and GetHashCode implementation
        public override bool Equals(object obj)
        {
            SpecificInfo other = obj as SpecificInfo;
            if (other == null)
                return false;
            return this.User == other.User && this.Password == other.Password && this.Host == other.Host && this.Port == other.Port && this.Path == other.Path && this.Format == other.Format;
        }
        
        public override int GetHashCode()
        {
            int hashCode = 0;
            unchecked
            {
                if (User != null)
                    hashCode += 1000000007 * User.GetHashCode();
                if (Password != null)
                    hashCode += 1000000009 * Password.GetHashCode();
                if (Host != null)
                    hashCode += 1000000021 * Host.GetHashCode();
                if (Port != null)
                    hashCode += 1000000033 * Port.GetHashCode();
                if (Path != null)
                    hashCode += 1000000087 * Path.GetHashCode();
                if (Format != null)
                    hashCode += 1000000093 * Format.GetHashCode();
            }
            return hashCode;
        }
        
        public static bool operator ==(SpecificInfo lhs, SpecificInfo rhs)
        {
            if (ReferenceEquals(lhs, rhs))
                return true;
            if (ReferenceEquals(lhs, null) || ReferenceEquals(rhs, null))
                return false;
            return lhs.Equals(rhs);
        }
        
        public static bool operator !=(SpecificInfo lhs, SpecificInfo rhs)
        {
            return !(lhs == rhs);
        }
        #endregion

    }
}
