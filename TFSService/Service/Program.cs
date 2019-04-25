﻿using System.ServiceProcess;

namespace Service
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new TfsWatcher()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
