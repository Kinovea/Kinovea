#region License
/*
Copyright © Joan Charmant 2012.
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
using System.Xml;

namespace Kinovea.Services
{
    public interface IScreenDescriptor
    {
        /// <summary>
        /// Type of screen: capture or playback.
        /// </summary>
        ScreenType ScreenType { get; }

        /// <summary>
        /// A string suitable for display in the UI.
        /// For playback the name of the video.
        /// For replay the name of the folder.
        /// For capture the alias of the camera.
        /// </summary>
        string FriendlyName { get; }

        /// <summary>
        /// Returns a deep copy of the descriptor.
        /// </summary>
        IScreenDescriptor Clone();

        /// <summary>
        /// Serialize to XML.
        /// </summary>
        void WriteXml(XmlWriter w);
    }
}
