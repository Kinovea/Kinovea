using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.Services
{
    public enum EncodingQuality
    {
        /// <summary>
        /// Archival, master copy.
        /// </summary>
        PerceptuallyLossless,

        /// <summary>
        /// Web, sharing.
        /// </summary>
        High,

        /// <summary>
        /// Social media
        /// </summary>
        Good,

        /// <summary>
        /// Preview, thumbnail.
        /// </summary>
        Medium,
    }
}
