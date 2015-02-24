using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.Camera.DirectShow
{
    public class PropertyValueMapperDefault : IPropertyValueMapper
    {
        public virtual Func<int, string> GetMapper(string property)
        {
            return Mapper;
        }

        private string Mapper(int value)
        {
            return value.ToString();
        }
    }
}
