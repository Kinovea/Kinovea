using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.Services
{

    /// <summary>
    /// Dimensions along which we can sort a list of files.
    /// </summary>
    public enum FileSortAxis
    {
        /// <summary>
        /// Sort by filename.
        /// </summary>
        Name,

        /// <summary>
        /// Sort by last write date time.
        /// </summary>
        Date,

        /// <summary>
        /// Sort by file length.
        /// </summary>
        Size
    }
}
