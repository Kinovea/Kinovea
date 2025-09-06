using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.Services
{
    /// <summary>
    /// Validation mode used in the calibration validation dialog.
    /// This is used when we know the 3D location of certain points
    /// in the image and we have lens calibration and plane calibration.
    /// </summary>
    public enum CalibrationValidationMode
    {
        /// <summary>
        /// In this mode the user fixes all 3 components of the 3D point.
        /// Kinovea will calculate the ray going from the camera to 
        /// this point and then intersect that ray with the calibrated plane.
        /// We then move the marker object accordingly in the image.
        /// This helps determining if there is a calibration error or a 
        /// resolution limitation.
        /// </summary>
        Fix3D,

        /// <summary>
        /// In this mode the user fixes one component of the 3D point.
        /// Kinovea will calculate the other 2 coordinates according 
        /// to the location of the point on the z=0 plane. In this case
        /// we assume the ray going from the camera to the calibrated plane
        /// is known and correct. We intersect the camera ray with 
        /// the new plane specified by the user.
        /// </summary>
        Fix1D,

        /// <summary>
        /// In this mode the user must have calibrated the same plane in two
        /// different videos, and have lens calibration in both.
        /// We match markers by name from both videos and compute the 
        /// 3D location of the points by finding the point closest to 
        /// both rays.
        /// </summary>
        Compute3D,
    }
}
