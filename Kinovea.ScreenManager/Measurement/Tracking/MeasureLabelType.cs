using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// The various type of measurement that "mesurable" drawings can display in their attached label.
    /// Depending on the tool the name and exact meaning of the measurement may vary.
    /// </summary>
    public enum MeasureLabelType
    {
        /// <summary>
        /// The label should be hidden.
        /// </summary>
        None,

        /// <summary>
        /// The label should be the name of the object.
        /// </summary>
        Name,

        /// <summary>
        /// 2D position wrt the coordinate system origin.
        /// </summary>
        Position,
        
        /// <summary>
        /// Distance to a reference point or to the coordinate system origin.
        /// </summary>
        TotalDistance,
        TotalHorizontalDisplacement,
        TotalVerticalDisplacement,

        //-------------------------------------------------
        // Speed and acceleration.
        //-------------------------------------------------
        Speed,
        HorizontalVelocity,
        VerticalVelocity,
        Acceleration,
        HorizontalAcceleration,
        VerticalAcceleration,

        //-------------------------------------------------
        // Circle-based.
        //-------------------------------------------------
        Center,
        Radius,
        Diameter,
        Circumference,
    }
}
