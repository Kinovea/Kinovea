#region License
/*
Copyright © Joan Charmant 2011.
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
using System.Xml;
using System.Drawing;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Objects that can serialize themselves to .kva. 
    /// Generally should have a static ReadXml method to deserialize too.
    /// 
    /// Additional non-inheritable contracts:
    /// - A .NET XmlType attribute with the serialization name of the drawing: [XmlType ("Angle")].
    /// - A constructor with the following prototype: void ctor(XmlReader, PointF, TimestampMapper, Metadata).
    /// </summary>
    public interface IKvaSerializable
    {
        Guid Id { get; }
        string Name { get; }
        
        void WriteXml(XmlWriter xmlWriter, SerializationFilter filter);
        void ReadXml(XmlReader xmlReader, PointF scale, TimestampMapper timestampMapper);
    }
}
