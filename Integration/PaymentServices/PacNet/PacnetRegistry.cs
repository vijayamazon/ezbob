using System;
using EzBob.Configuration;
using Scorto.Configuration;
using StructureMap.Configuration.DSL;

namespace PaymentServices.PacNet
{
	using EZBob.DatabaseLib.Model.Database.Loans;
	using NHibernate;
	using NHibernateWrapper.NHibernate;
	using StructureMap.Pipeline;

	public class PacnetRegistry : Registry
    {
        public PacnetRegistry()
        {
            try
            {
                ConfigurationRoot configuration = ConfigurationRoot.GetConfiguration();
                if (configuration != null &&
                    ConfigurationRootBob.GetConfiguration().PacNet.GetValue<string>("SERVICE_TYPE") == "Testing")
                {
                    For<IPacnetService>().Use<LogPacnet<FakePacnetService>>();
                }
                else
                {
                    For<IPacnetService>().Use<LogPacnet<PacnetService>>();
                }
            }
            catch (Exception)
            {
                For<IPacnetService>().Use<PacnetService>();
            }
        }
    }
}
