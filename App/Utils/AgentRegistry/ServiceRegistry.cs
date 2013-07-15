using EZBob.DatabaseLib.Model.Database;
using EzBob.Configuration;
using MailApi;
using Scorto.Configuration;
using StructureMap.Configuration.DSL;

namespace AgentRegistry
{
    public class ServiceRegistry : Registry
    {
        public ServiceRegistry()
        {
            Scorto.NHibernate.NHibernateManager.HbmAssemblies.Add(typeof(PerformencePerUnderwriterDataRow).Assembly);
            var bobconfig = EnvironmentConfiguration.Configuration.GetCurrentConfiguration<ConfigurationRootBob>();
            For<IMandrillConfig>().Use(bobconfig.MandrillConfig);
            For<IMail>().Use<Mail>();
        }
    }
}
