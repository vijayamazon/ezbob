﻿using System;
using System.Globalization;
using ApplicationMng.Repository;
using NHibernate;

namespace EZBob.DatabaseLib.Model
{
    public interface IConfigurationVariablesRepository : IRepository<ConfigurationVariable>
    {
        ConfigurationVariable GetByName(string name);
        decimal GetByNameAsDecimal(string name);
        void SetByName(string name, string value);
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

        public decimal GetByNameAsDecimal(string name)
        {
            return Convert.ToDecimal(GetByName(name).Value, CultureInfo.InvariantCulture);
        }

        public void SetByName(string name, string value)
        {
            var property = _session.QueryOver<ConfigurationVariable>().Where(c => c.Name == name).SingleOrDefault<ConfigurationVariable>();
            property.Value = value;
            _session.Update(property);
        }
    }
}
