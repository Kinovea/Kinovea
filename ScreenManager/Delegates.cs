﻿#region License
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
using System.Collections.Generic;
using System.Drawing;

using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    //----------------------------------------------------------------------------------------------------------
    // Delegates signatures. (For events, use EventHandler and EventHandler<TEventArgs>)
    //
    // We don't use the Action<T1, T2, ...> shortcuts for delegate types, as it makes the usage of the delegate
    // obscure for the caller. Since the caller doesn't know about the implementer, 
    // the prototype of the delegate is the only place where he can guess the purpose of the parameters.
    // Except for the simple Action delegate (nothing in, nothing out).
    //----------------------------------------------------------------------------------------------------------
    
    public delegate void PropertyPagePrompter(IntPtr _windowHandle);
    public delegate long TimeStampMapper(long _iInputTimestamp, bool bRelative);
    public delegate string TimeCodeBuilder(long _iTimestamp, TimecodeFormat _timeCodeFormat, bool _bSynched);
    public delegate void ClosestFrameAction(Point _mouse, List<AbstractTrackPoint> _positions, int _iPixelTotalDistance, bool _b2DOnly);
    public delegate object BindReader(string _sourceProperty, Type _targetType);
    public delegate void BindWriter(string _targetProperty, object _value);
    public delegate void ImageProcessor(Bitmap _src);
    public delegate void DelegateUpdateTrackerFrame(long _iFrame);
}
