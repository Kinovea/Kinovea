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
using System.Drawing;

// This file is part of a future assembly where core type declarations will go. 
// The assembly will be referencable by every other assembly, and will have no dependency outside .NET.
// This was the goal of Service namespace but ultimately Service should be about providing transversal services, so may have
// dependencies on other part of the project.

namespace Kinovea.Base
{
    public delegate long ImageRetriever(Graphics _canvas, Bitmap _src, long _timestamp, bool _flushDrawings, bool _keyframesOnly);
}
