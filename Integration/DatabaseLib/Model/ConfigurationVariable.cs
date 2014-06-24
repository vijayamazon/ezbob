namespace EZBob.DatabaseLib.Model {
	using ConfigManager;
	using FluentNHibernate.Mapping;

	public class ConfigurationVariable {
		public ConfigurationVariable() {} // constructor

		public ConfigurationVariable(VariableValue v) {
			Id = v.ID;
			Name = v.Name.ToString();
			Value = v.Value;
			Description = v.Description;
		} // constructor

		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual string Value { get; set; }
		public virtual string Description { get; set; }
	} // class ConfigurationVariable

	public sealed class ConfigurationVariablesMap : ClassMap<ConfigurationVariable> {
		public ConfigurationVariablesMap() {
			Table("ConfigurationVariables");
			Id(x => x.Id);
			Map(x => x.Name).Length(255);
			Map(x => x.Value).CustomType("StringClob").LazyLoad();
			Map(x => x.Description).CustomType("StringClob").LazyLoad();
		} // constructor
	} // class ConfigurationVariablesMap
} // namespace
