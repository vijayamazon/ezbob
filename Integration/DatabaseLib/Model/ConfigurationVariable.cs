namespace EZBob.DatabaseLib.Model {
	using System;
	using System.Globalization;
	using FluentNHibernate.Mapping;

	public class ConfigurationVariable {
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual string Value { get; set; }
		public virtual string Description { get; set; }

		public static implicit operator string(ConfigurationVariable oVariable) {
			return oVariable.Value;
		} // operator to string

		public static implicit operator bool(ConfigurationVariable oVariable) {
			switch (oVariable.Name.ToLower()) {
			case "true":
			case "1":
			case "yes":
				return true;
			} // switch

			return false;
		} // operator to bool

		public static implicit operator int(ConfigurationVariable oVariable) {
			return Convert.ToInt32(oVariable.Value, CultureInfo.InvariantCulture);
		} // operator to int

		public static implicit operator double(ConfigurationVariable oVariable) {
			return Convert.ToDouble(oVariable.Value, CultureInfo.InvariantCulture);
		} // operator to double

		public static implicit operator decimal(ConfigurationVariable oVariable) {
			return Convert.ToDecimal(oVariable.Value, CultureInfo.InvariantCulture);
		} // operator to decimal
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
