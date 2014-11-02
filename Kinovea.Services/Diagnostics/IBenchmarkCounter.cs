using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.Services
{
    public interface IBenchmarkCounter
    {
        Dictionary<string, float> GetMetrics();
    }
}
