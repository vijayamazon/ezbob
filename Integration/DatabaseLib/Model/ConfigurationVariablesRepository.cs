namespace EZBob.DatabaseLib.Model {
	using System;
	using System.Globalization;
	using ApplicationMng.Repository;
	using ConfigManager;
	using NHibernate;

	#region interface IConfigurationVariablesRepository

	public interface IConfigurationVariablesRepository : IRepository<ConfigurationVariable> {
		ConfigurationVariable GetByName(Variables name);
		decimal GetByNameAsDecimal(Variables name);
		void SetByName(Variables name, string value);
	} // IConfigurationVariablesRepository

	#endregion interface IConfigurationVariablesRepository

	#region class ConfigurationVariablesRepository

	public class ConfigurationVariablesRepository : NHibernateRepositoryBase<ConfigurationVariable>, IConfigurationVariablesRepository {
		public ConfigurationVariablesRepository(ISession session) : base(session) {}

		public ConfigurationVariable GetByName(Variables name) {
			string sName = name.ToString();
			return _session.QueryOver<ConfigurationVariable>().Where(c => c.Name == sName).SingleOrDefault<ConfigurationVariable>();
		} // GetByName

		public decimal GetByNameAsDecimal(Variables name) {
			return Convert.ToDecimal(GetByName(name).Value, CultureInfo.InvariantCulture);
		} // GetByNameAsDecimal

		public void SetByName(Variables name, string value) {
			string sName = name.ToString();

			var property = _session
				.QueryOver<ConfigurationVariable>()
				.Where(c => c.Name == sName)
				.SingleOrDefault<ConfigurationVariable>();

			property.Value = value;
			_session.Update(property);
			_session.Flush();
		} // SetByName
	} // ConfigurationVariablesRepository

	#endregion class ConfigurationVariablesRepository
} // namespace
