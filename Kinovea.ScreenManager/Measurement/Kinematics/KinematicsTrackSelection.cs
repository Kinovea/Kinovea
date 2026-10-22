using System;
using System.Collections.Generic;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Rules deciding which trajectory the kinematics panel shows.
    ///
    /// This class has no dependency on UI so it can be unit tested in isolation.
    /// </summary>
    public static class KinematicsTrackSelection
    {
        /// <summary>
        /// Returns the index of the trajectory to show.
        /// The current selection is kept if it is still in the list, even without data.
        /// Otherwise the first trajectory with data is used, then the first trajectory, then -1 if the list is empty.
        /// Guid.Empty as current means nothing is selected.
        /// </summary>
        public static int Pick(IList<Guid> trackIds, IList<bool> hasData, Guid current)
        {
            if (trackIds == null)
                throw new ArgumentNullException("trackIds");

            if (hasData == null)
                throw new ArgumentNullException("hasData");

            if (trackIds.Count != hasData.Count)
                throw new ArgumentException("The lists must have the same length.");

            if (trackIds.Count == 0)
                return -1;

            if (current != Guid.Empty)
            {
                for (int i = 0; i < trackIds.Count; i++)
                {
                    if (trackIds[i] == current)
                        return i;
                }
            }

            for (int i = 0; i < hasData.Count; i++)
            {
                if (hasData[i])
                    return i;
            }

            return 0;
        }
    }
}
