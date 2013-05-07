using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using log4net;
using log4net.Config;

namespace ServiceStarter
{
    class ScortoServiceStarter : AgentWindowsService.Service
    {
        public void Stop()
        {
            base.OnStop();
        }

        public void Start(string[] args)
        {
            base.OnStart(args);
        }
    }
    class Program
    {
        static readonly ILog log = LogManager.GetLogger("ServiceStarter");
        static void Main(string[] args)
        {
            log.Debug("Test Debug Message");
            log.Info("Test Info Message");
            log.Warn("Test Warning Message");
            log.Error("Test Error Message");
            log.Fatal("Test Fatal Message");

            ScortoServiceStarter srv = new ScortoServiceStarter();
            srv.Start(args);
            Console.ReadKey();
            srv.Stop();
        }
    }
}
