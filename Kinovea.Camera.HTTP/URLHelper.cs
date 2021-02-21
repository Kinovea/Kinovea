using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.Camera.HTTP
{
    public static class URLHelper
    {
        public static string BuildURL(SpecificInfo specific)
        {
            string url = "";
            if (string.IsNullOrEmpty(specific.User) && string.IsNullOrEmpty(specific.Password))
            {
                if (string.IsNullOrEmpty(specific.Port) || specific.Port == "80")
                    url = string.Format("http://{0}{1}", specific.Host, specific.Path);
                else
                    url = string.Format("http://{0}:{1}{2}", specific.Host, specific.Port, specific.Path);
            }
            else
            {
                if (string.IsNullOrEmpty(specific.Port) || specific.Port == "80")
                    url = string.Format("http://{0}:{1}@{2}{3}", specific.User, specific.Password, specific.Host, specific.Path);
                else
                    url = string.Format("http://{0}:{1}@{2}:{3}{4}", specific.User, specific.Password, specific.Host, specific.Port, specific.Path);
            }

            return url;
        }
    }
}
