using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.ScreenManager
{
    public enum Kinematics
    {
        X,
        Y,
        XRaw,
        YRaw,

        LinearDistance,                 // Total distance covered since starting point of trajectory.
        LinearHorizontalDisplacement,   // Total displacement with regards to starting point of trajectory.
        LinearVerticalDisplacement,     // Total displacement with regards to starting point of trajectory.
        LinearSpeed,
        LinearHorizontalVelocity,
        LinearVerticalVelocity,
        LinearAcceleration,             // Instantaneous acceleration in the direction of the velocity vector.
        LinearHorizontalAcceleration,
        LinearVerticalAcceleration,

        AngularPosition,                // Absolute or relative value of the angle at this time.
        AngularDisplacement,            // Displacement with regards to previous point of measure.
        TotalAngularDisplacement,       // Total displacement with regards to first point of measure.
        
        AngularVelocity,                
        TangentialVelocity,

        AngularAcceleration,
        TangentialAcceleration,
        CentripetalAcceleration,
        ResultantLinearAcceleration
    }
}
