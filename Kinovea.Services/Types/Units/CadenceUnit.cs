using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.Services
{
    /// <summary>
    /// Units of frequency. 
    /// These correspond to all the user-friendly units. 
    /// These are activity based so there might be duplicated concepts.
    /// </summary>
    public enum CadenceUnit
    {
        // It's not too clear if this should deduplicate similar units or not.
        // There is basically only two time base, seconds and minutes.
        // But each activity uses slightly different approaches and meaning.
        // Depending on the activity a "cycle" might be two or three beats.

        // Universal units.
        // Avoid using beats/timestamp.
        Hertz,              
        CyclesPerSecond,    
        CyclesPerMinute,    

        // Walking/Running.
        // Typically measured from each ground contact (step).
        // Normally one stride = two steps.
        // Some people use the terms "stride" and "step" interchangeably.
        StepsPerSecond, // Might be used for sprint.
        StepsPerMinute, // Most common unit.


        // Swimming, Rowing, Canoe, etc.
        StrokesPerSecond, // Swimming.
        StrokesPerMinute, // Swimming, Rowing, Canoe, etc.

        // Cycling
        // Unlike running this is measured in full cycles.
        RevolutionsPerMinute,


        // Orders of magnitude:
        // - Running middle/long distance: 180 steps/min.
        // - Sprint: 4 to 5 Hz.
        // - Cycling: 90 rpm.
        // - Swimming: 1 Hz (full cycle).
        // - Rowing: 30 strokes/min.
    }
}
