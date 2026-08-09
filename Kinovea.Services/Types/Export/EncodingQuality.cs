using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.Services.Types.Export
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
        Medium,

        /// <summary>
        /// Preview, thumbnail.
        /// </summary>
        Low,
    }
}
