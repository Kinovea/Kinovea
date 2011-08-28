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

namespace Kinovea.Services
{
    public enum TimeCodeFormat
    {
        ClassicTime,
        Frames,
        Milliseconds,
        TenThousandthOfHours,
        HundredthOfMinutes,
        TimeAndFrames,
        Timestamps,
        Unknown,
        NumberOfTimeCodeFormats
    }
    
    public enum SpeedUnits
	{
		MetersPerSecond,
		KilometersPerHour,
		FeetPerSecond,
		MilesPerHour,
		Knots,
		PixelsPerFrame,			// Native unit. 
	}
	
    public enum TimeCodeType
    {
    	Number,
    	String,
    	Time
    }
	
	public enum ActiveFileBrowserTab
    {
    	Explorer = 0,
    	Shortcuts
    }
	
	/// <summary>
	/// Size of the thumbnails in the explorer.
	/// Sizes are expressed in number of thumbnails that should fit in the width of the explorer.
	/// the actual size of any given thumbnail will change depending on the available space.
	/// </summary>
	public enum ExplorerThumbSizes
	{
		ExtraLarge = 4,
		Large = 5,
		Medium = 7,
		Small = 10,
		ExtraSmall = 14
	};
	
	// Named with Kinovea to avoid conflict with System.Drawing.Imaging.
	public enum KinoveaImageFormat
	{
		JPG,
		PNG,
		BMP
	};
    public enum KinoveaVideoFormat
	{
		MKV,
		MP4,
		AVI
	};
    public enum NetworkCameraFormat
    {
    	JPEG,
    	MJPEG
    };
}
