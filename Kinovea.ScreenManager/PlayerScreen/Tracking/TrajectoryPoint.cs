using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Kinovea.ScreenManager
{
    // Represent all kinematic data associated with a time point.
    // All values are expressed in user calibrated coordinates.
    public class TrajectoryPoint
    {
        public PointF Coordinates { get; set; }
        public float TotalDistance { get; set; }
        public float Speed { get; set; }
        public float VerticalVelocity { get; set; }
        public float HorizontalVelocity { get; set; }
        public float Acceleration { get; set; }
        public float VerticalAcceleration { get; set; }
        public float HorizontalAcceleration { get; set; }
        
        public PointF RotationCenter { get; set; }
        public float RotationRadius { get; set; }
        
        public float AbsoluteAngle { get; set; }
        public float DisplacementAngle { get; set; }
        public float TotalDisplacementAngle { get; set; }
        public float AngularVelocity { get; set; }
        public float TangentialVelocity { get; set; }
        public float CentripetalAcceleration { get; set; }
        public float AngularAcceleration { get; set; }
    }
}
