using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Kinovea.ScreenManager
{
    public class TrackingProfileManager
    {
        public TrackingProfile Current
        {
            get { return current; }
        }

        public Dictionary<string, TrackingProfile> Profiles
        {
            get { return profiles; }
        }

        private Dictionary<string, TrackingProfile> profiles = new Dictionary<string, TrackingProfile>();
        private TrackingProfile current;

        public TrackingProfileManager()
        {
            TrackingProfile classic = new TrackingProfile();
            TrackingProfile football = new TrackingProfile("Football", 0.5, 0.8, new Size(75, 60), new Size(25, 50), TrackerParameterUnit.Pixels, TrackerParameterUnit.Pixels, false);

            profiles.Add(classic.Name, classic);
            profiles.Add(football.Name, football);

            current = classic;
        }
    }
}
