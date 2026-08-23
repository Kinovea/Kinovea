using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kinovea.Services;

namespace Kinovea.Video
{
    /// <summary>
    /// The actual image geometry decided by the reader.
    /// The reader consumes the VideoGeometryRequest and produces a VideoGeometry,
    /// based on the original video, its capabilities and its internal state.
    /// </summary>
    public sealed class VideoGeometry
    {
        /// <summary>
        /// The canonical size of images produced by the reader, before any scaling is applied.
        /// This is the size that should be used for drawing coordinates and other calculations.
        /// </summary>
        public Size ReferenceSize { get; }

        /// <summary>
        /// Actual bitmap size produced by the reader.
        /// </summary>
        public Size OutputSize { get; }

        /// <summary>
        /// This should be true whenever the OutputSize matches the request PresentationSize.
        /// If the presentation size is unknown, for example when loading the file for the first 
        /// time, this should be false.
        /// </summary>
        public bool IsPreScaled { get; }

        /// <summary>
        /// The scale factor applied to the reference size to produce the output size.
        /// </summary>
        public float Scale { get; }

        /// <summary>
        /// Aspect ratio option used by the reader.
        /// </summary>
        public ImageAspectRatio ImageAspectRatio { get; }

        /// <summary>
        /// Image rotation applied by the reader.
        /// </summary>
        public ImageRotation ImageRotation { get; }

        /// <summary>
        /// Demosaicing option applied by the reader.
        /// </summary>
        public Demosaicing Demosaicing { get; }

        /// <summary>
        /// Deinterlacing option applied by the reader.
        /// </summary>
        public bool Deinterlacing { get; set; }

        /// <summary>
        /// Whether the requested stabilization is applied.
        /// </summary>
        public bool StabilizationApplied { get; }

        public int Generation { get; }

        public VideoGeometry(
            Size referenceSize,
            Size outputSize,
            bool isPreScaled,
            float scale,
            ImageAspectRatio imageAspectRatio,
            ImageRotation imageRotation,
            Demosaicing demosaicing,
            bool deinterlacing,
            bool stabilizationApplied,
            int generation)
        {
            ReferenceSize = referenceSize;
            OutputSize = outputSize;
            IsPreScaled = isPreScaled;
            Scale = scale;
            ImageAspectRatio = imageAspectRatio;
            ImageRotation = imageRotation;
            Demosaicing = demosaicing;
            Deinterlacing = deinterlacing;
            StabilizationApplied = stabilizationApplied;
            Generation = generation;
        }

        public VideoGeometry()
        {
            ReferenceSize = Size.Empty;
            OutputSize = Size.Empty;
            IsPreScaled = false;
            Scale = 1.0f;
            ImageAspectRatio = ImageAspectRatio.Auto;
            ImageRotation = ImageRotation.Rotate0;
            Demosaicing = Demosaicing.None;
            Deinterlacing = false;
            StabilizationApplied = false;
            Generation = 0;
        }
    }
}
