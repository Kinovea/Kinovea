using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.ScreenManager
{
    public enum TrackExtraData
    {
        None,
        Name,
        Position,
        
        TotalDistance,
        TotalHorizontalDisplacement,
        TotalVerticalDisplacement,
        
        Speed,
        HorizontalVelocity,
        VerticalVelocity,

        Acceleration,
        HorizontalAcceleration,
        VerticalAcceleration,

        // Circle specific
        Center,
        Radius,
        Diameter,
        Circumference,
    }
}
