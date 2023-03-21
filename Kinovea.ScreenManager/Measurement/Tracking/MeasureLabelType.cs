using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// The various type of measurement that "mesurable" drawings can display in their attached label.
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
        /// Time relative to the configured time origin.
        /// </summary>
        Clock,

        /// <summary>
        /// Time relative to a reference point in the drawing.
        /// </summary>
        RelativeTime,

        /// <summary>
        /// 2D position to the coordinate system origin.
        /// </summary>
        Position,

        /// <summary>
        /// 2D position to a reference point in the drawing.
        /// </summary>
        RelativePosition,

        /// <summary>
        /// Heading angle from the origin to the current point.
        /// </summary>
        Direction,

        /// <summary>
        /// Distance to the coordinate system origin.
        /// </summary>
        Distance,

        /// <summary>
        /// Distance to a reference point in the drawing.
        /// </summary>
        RelativeDistance,

        /// <summary>
        /// Travel distance between the reference point of the drawing and the current point. 
        /// </summary>
        TravelDistance,

        /// <summary>
        /// Cumulative horizontal travel (absolute value of horizontal displacement).
        /// </summary>
        HorizontalTravel,

        /// <summary>
        /// Cumulative vertical travel (absolute value of horizontal displacement).
        /// </summary>
        VerticalTravel,

        /// <summary>
        /// Instantaneous speed at the point, based on the position at the previous and next points.
        /// </summary>
        Speed,

        /// <summary>
        /// The horizontal component of the velocity vector.
        /// </summary>
        HorizontalVelocity,

        /// <summary>
        /// The vertical component of the velocity vector.
        /// </summary>
        VerticalVelocity,

        /// <summary>
        /// Instantaneous acceleration, based on the speed at the previous and next points. 
        /// </summary>
        Acceleration,

        /// <summary>
        /// The horizontal component of the acceleration vector.
        /// </summary>
        HorizontalAcceleration,

        /// <summary>
        /// The vertical component of the acceleration vector.
        /// </summary>
        VerticalAcceleration,

        /// <summary>
        /// Circle center.
        /// </summary>
        Center,

        /// <summary>
        /// Circle radius.
        /// </summary>
        Radius,

        /// <summary>
        /// Circle diameter.
        /// </summary>
        Diameter,

        /// <summary>
        /// Circle circumference.
        /// </summary>
        Circumference,


        /// <summary>
        /// Horizontal offset: horizontal-component of the segment going from the reference point to the current point.
        /// </summary>
        TotalHorizontalDisplacement,

        /// <summary>
        /// Vertical offset: vertical-component of the segment going from the reference point to the current point.
        /// </summary>
        TotalVerticalDisplacement,
    }
}
