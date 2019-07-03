using TfsAPI.Interfaces;
using TfsAPI.TFS;

namespace Tests.Tfs_Api_Tests
{
    public class BaseTest
    {
        protected ITfsApi GetConn()
        {
            return new TfsApi("https://msk-tfs1.securitycode.ru/tfs/Endpoint%20Security");
        }
    }
}