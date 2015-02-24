using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.Camera.DirectShow
{
    public interface IPropertyValueMapper
    {
        Func<int, string> GetMapper(string property);
    }
}
