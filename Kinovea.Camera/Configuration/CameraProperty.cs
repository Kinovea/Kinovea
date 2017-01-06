using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.Camera
{
    /// <summary>
    /// Represents a camera property like gain or exposure.
    /// </summary>
    public class CameraProperty
    {
        /// <summary>
        /// The name of the property in the underlying API. Used for read/write calls.
        /// </summary>
        public string Identifier { get; set; }

        /// <summary>
        /// Whether the property is supported by the device.
        /// </summary>
        public bool Supported { get; set; }

        /// <summary>
        /// Whether the configuration of the property is supported by the device.
        /// </summary>
        public bool ReadOnly { get; set; }

        /// <summary>
        /// The fundamental type of the property.
        /// </summary>
        public CameraPropertyType Type { get; set; }

        public string Minimum { get; set; }
        public string Maximum { get; set; }
        public string Step { get; set; }
        public bool CanBeAutomatic { get; set; }

        /// <summary>
        /// The preferred user interface representation for the property.
        /// </summary>
        public CameraPropertyRepresentation Representation { get; set; }

        /// <summary>
        /// The current value of the property.
        /// </summary>
        public string CurrentValue { get; set; }

        /// <summary>
        /// The current Automatic state of the property.
        /// </summary>
        public bool Automatic { get; set; }

        /// <summary>
        /// The name of the property for the Auto flag in the underlying API if the flag is handled by a separate property.
        /// </summary>
        public string AutomaticIdentifier { get; set; }

        /// <summary>
        /// An opaque metadata that may be used by camera plugins.
        /// </summary>
        public string Specific { get; set; }

        public CameraProperty()
        {
            Identifier = "";
            Supported = false;
            ReadOnly = true;
            Type = CameraPropertyType.Undefined;
            
            Minimum = "";
            Maximum = "";
            Step = "";
            CanBeAutomatic = false;

            Representation = CameraPropertyRepresentation.Undefined;
            
            CurrentValue = "";
            Automatic = false;
            AutomaticIdentifier = "";
            Specific = "";
        }
    }
}
