
namespace Kinovea.Services
{
    /// <summary>
    /// Algorithm used to track a given point or object.
    /// </summary>
    public enum TrackingAlgorithm
    {
        /// <summary>
        /// Pattern matching with cross-correlation over the whole pattern window.
        /// Compute a correlation score at each possible location in the search window.
        /// </summary>
        Correlation,

        /// <summary>
        /// Finds circles in the pattern window and match reference by size.
        /// </summary>
        Circle,

        /// <summary>
        /// Finds the central corner of a "quadrant" marker and use it as the point.
        /// </summary>
        QuadrantMarker,
    }
}
