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

namespace Kinovea.ScreenManager
{
    // Some general enums used in this namespace.
    // Some other enums might be in the file where the most related class is defined.
    // But avoid declaring enums as nested types inside these classes.
    
	/// <summary>
	/// Standards units for distance, restricted to sports range. (No microscopic or macroscopic).
	/// </summary>
	public enum LengthUnits
	{
		Centimeters,
		Meters,
		Inches,
		Feet,
		Yards,
		Pixels		  // Native unit.
	}

    public enum VideoFilterType
	{
		AutoLevels,
		AutoContrast,
		Sharpen,
		EdgesOnly,
		Mosaic,
		Reverse,
		Sandbox,
		NumberOfVideoFilters
	}
    
}
