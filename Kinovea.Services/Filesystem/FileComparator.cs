using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Kinovea.Services
{
    /// <summary>
    /// Comparator for files.
    /// An instance of this can be used as the argument to a List<string>.Sort() call.
    /// </summary>
    public class FileComparator : IComparer<string>
    {
        private AlphanumComparator alphaNumComparator = new AlphanumComparator();
        private FileSortAxis axis;
        private bool ascending;

        public FileComparator(FileSortAxis axis, bool ascending)
        {
            this.axis = axis;
            this.ascending = ascending;
        }

        public int Compare(string x, string y)
        {
            // Meaning of result:
            // -1: x is less than y.
            // 0: x == y.
            // 1: x is greater than y.

            if (string.IsNullOrEmpty(x) || string.IsNullOrEmpty(y))
                return 0;

            FileInfo xInfo = new FileInfo(x);
            FileInfo yInfo = new FileInfo(y);
            int result = 0;

            if (axis == FileSortAxis.Name)
            {
                result = alphaNumComparator.Compare(x, y);
            }
            if (axis == FileSortAxis.Date)
            {
                if (xInfo.LastWriteTimeUtc < yInfo.LastWriteTimeUtc)
                    result = -1;
                else if (xInfo.LastWriteTimeUtc > yInfo.LastWriteTimeUtc)
                    result = 1;
            }
            else if (axis == FileSortAxis.Size)
            {
                if (xInfo.Length < yInfo.Length)
                    result = -1;
                else if (xInfo.Length > yInfo.Length)
                    result = 1;
            }

            if (!ascending)
                result = -result;

            return result;
        }
    }
}
