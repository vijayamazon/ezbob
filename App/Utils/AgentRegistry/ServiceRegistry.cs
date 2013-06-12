using EZBob.DatabaseLib.Model.Database;
using StructureMap.Configuration.DSL;

namespace AgentRegistry
{
    public class ServiceRegistry : Registry
    {
        public ServiceRegistry()
        {
            Scorto.NHibernate.NHibernateManager.HbmAssemblies.Add(typeof(PerformencePerUnderwriterDataRow).Assembly);
        }
    }
}
