﻿using ApplicationMng.Repository;
using NHibernate;

namespace EZBob.DatabaseLib.Model
{
    public interface IConfigurationVariablesRepository : IRepository<ConfigurationVariable>
    {
        ConfigurationVariable GetByName(string name);
    }

    public class ConfigurationVariablesRepository : NHibernateRepositoryBase<ConfigurationVariable>, IConfigurationVariablesRepository
    {
        public ConfigurationVariablesRepository(ISession session)
            : base(session)
        {
        }

        public ConfigurationVariable GetByName(string name)
        {
            return _session.QueryOver<ConfigurationVariable>().Where(c => c.Name== name).SingleOrDefault<ConfigurationVariable>();
        }
    }
}
