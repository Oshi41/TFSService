using System.Diagnostics;
using System.ServiceProcess;

namespace Service
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Trace.Write("On starting app");
        }

        protected override void OnStop()
        {
            Trace.Write("On shutdown");
        }
    }
}
