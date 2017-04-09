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
        LinearHorizontalDisplacement,   // Displacement with regards to starting point of trajectory.
        LinearVerticalDisplacement,     // Displacement with regards to starting point of trajectory.
        LinearSpeed,
        LinearHorizontalVelocity,
        LinearVerticalVelocity,
        LinearAcceleration,             // Instantaneous acceleration in the direction of the velocity vector.
        LinearHorizontalAcceleration,
        LinearVerticalAcceleration,

        AngularPosition,
        AngularDistance,
        AngularDisplacement,
        AngularVelocity,
        AngularAcceleration,
        TangentialVelocity,
        TangentialAcceleration,
        CentripetalAcceleration,
        ResultantLinearAcceleration
    }
}
