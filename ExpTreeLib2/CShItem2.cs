using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.ExpTreeLib2
{
    /// <summary>
    /// This class is currently an empty stub matching the API of the existing CShItem class.
    /// </summary>
    public class CShItem2
    {
        #region Properties
        public string Path { get; set; }
        public bool IsFileSystem { get; set; }
        public bool IsDisk { get; set; }
        public CShItem2 Parent { get; set; }
        public string DisplayName { get; set; }
        #endregion

        #region Construction/Destruction
        public CShItem2(string pathFolder)
        {
        }
        #endregion

        #region Public methods
        public ArrayList GetFiles()
        {
            return new ArrayList();
        }
        #endregion

    }
}
