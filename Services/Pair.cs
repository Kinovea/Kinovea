using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.Services
{
    public class Pair<T1, T2>
    {
        public T1 First { get; set; }
        public T2 Second { get; set; }
        
        public Pair()
        {
        }

        public Pair(T1 first, T2 second)
        {
            this.First = first;
            this.Second = second;
        }
    };
}
