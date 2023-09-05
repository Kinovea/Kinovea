
namespace Kinovea.Services
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

        //----------------------------
        // Time measures
        //----------------------------

        /// <summary>
        /// Time relative to the time origin.
        /// </summary>
        Clock,

        /// <summary>
        /// Time relative to a reference point in the drawing.
        /// </summary>
        RelativeTime,

        /// <summary>
        /// Frame number within a sequence (Kinogram mode).
        /// </summary>
        Frame,


        //----------------------------
        // Position based measures
        //----------------------------

        /// <summary>
        /// 2D position in the coordinate system.
        /// </summary>
        Position,

        /// <summary>
        /// 2D position to the first point in the drawing.
        /// </summary>
        RelativePosition,

        /// <summary>
        /// Distance to the coordinate system origin.
        /// </summary>
        Distance,

        /// <summary>
        /// Travel distance between the reference point of the drawing and the current point.
        /// Sum of the length of each segment.
        /// </summary>
        TravelDistance,

        /// <summary>
        /// Cumulative horizontal travel.
        /// Sum of the absolute value of the horizontal component of each segment.
        /// </summary>
        HorizontalTravel,

        /// <summary>
        /// Cumulative vertical travel.
        /// Sum of the absolute value of the vertical component of each segment).
        /// </summary>
        VerticalTravel,

        /// <summary>
        /// Angle between a line and the horizontal axis of the coordinate system.
        /// </summary>
        AngleToHorizontal,

        /// <summary>
        /// Angle between a line and the vertical axis of the coordinate system.
        /// </summary>
        AngleToVertical,


        //-------------------------------
        // Speed and acceleration measures
        //-------------------------------

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

        //-------------------------------
        // Circle based measures
        //-------------------------------

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


        //-------------------------------------------------
        // Deviation from a model
        //-------------------------------------------------

        /// <summary>
        /// Distance between the current point and the average of the data points.
        /// </summary>
        PointDeviation,

        /// <summary>
        /// Distance between the current point and its projection on the best-fit line of the data.
        /// </summary>
        LineDeviation,

        /// <summary>
        /// Distance between the horizontal component of the point and the average horizontal component of the data.
        /// This is how much the point deviates from a vertical line fitted to the data.
        /// </summary>
        HorizontalDeviation,

        /// <summary>
        /// Distance between the vertical component of the point and the average vertical component of the data.
        /// This is how much the point deviates from a horizontal line fitted to the data.
        /// </summary>
        VerticalDeviation,

        /// <summary>
        /// Distance between the point and the closest point on a circle fitted to the data.
        /// </summary>
        CircularDeviation,

        /// <summary>
        /// Horizontal offset (signed).
        /// Horizontal component of the offset between the first point and the current point.
        /// </summary>
        TotalHorizontalDisplacement,

        /// <summary>
        /// Vertical offset (signed).
        /// Horizontal component of the offset between the first point and the current point.
        /// </summary>
        TotalVerticalDisplacement,
    }
}
