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
