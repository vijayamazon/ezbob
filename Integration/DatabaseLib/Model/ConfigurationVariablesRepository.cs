namespace EZBob.DatabaseLib.Model
{
	using System;
	using System.Globalization;
	using ApplicationMng.Repository;
	using NHibernate;

    public interface IConfigurationVariablesRepository : IRepository<ConfigurationVariable>
    {
		ConfigurationVariable GetByName(string name);
		decimal GetByNameAsDecimal(string name);
		int GetByNameAsInt(string name);
		bool GetByNameAsBool(string name);
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

		public int GetByNameAsInt(string name)
		{
			return Convert.ToInt32(GetByName(name).Value, CultureInfo.InvariantCulture);
		}

		public bool GetByNameAsBool(string name)
		{
			string value = GetByName(name).Value;
			return value.ToLower() == "true" || value == "1";
		}

        public void SetByName(string name, string value)
        {
            var property = _session.QueryOver<ConfigurationVariable>().Where(c => c.Name == name).SingleOrDefault<ConfigurationVariable>();
            property.Value = value;
            _session.Update(property);
        }
    }
}
