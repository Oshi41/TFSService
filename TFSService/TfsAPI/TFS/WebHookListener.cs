using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace TfsAPI.TFS
{
    public class WebHookListener
    {
        public void Register(HttpConfiguration config)
        {
            var server = new HttpServer(config);
        }
    }
}
