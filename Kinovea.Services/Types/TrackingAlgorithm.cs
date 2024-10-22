
namespace Kinovea.Services
{
    /// <summary>
    /// Algorithm used to track a given point or object.
    /// </summary>
    public enum TrackingAlgorithm
    {
        /// <summary>
        /// Template matching with cross-correlation.
        /// Compute a correlation score at each possible location in the search window.
        /// </summary>
        Correlation,

        /// <summary>
        /// Finds blobs within a range of HSV values.
        /// </summary>
        Blob,

        /// <summary>
        /// Finds circles and match by size.
        /// </summary>
        Circle,

        /// <summary>
        /// Finds the central corner of a 2x2 checkerboard marker.
        /// </summary>
        QuadrantMarker,
    }
}
