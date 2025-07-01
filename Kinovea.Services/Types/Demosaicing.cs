using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.Services
{
    /// <summary>
    /// 原始图像处理中的去马赛克选项
    /// </summary>
    public enum Demosaicing
    {
        None,
        RGGB,
        BGGR,
        GRBG,
        GBRG
    }
}
