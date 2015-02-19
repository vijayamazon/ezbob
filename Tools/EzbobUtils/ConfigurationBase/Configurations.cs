using Ezbob.Logger;
using Ezbob.Database;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ConfigurationBase {
	// How to use
	// 1. Add reference to C:\DEV\Dlls\ConfigurationBase.dll
	// 2. Create a class like:
	//  namespace EzSupportAgent {
	//      using ConfigurationBase;

	//      public class ConfigurationSample : Configurations {
	//          public ConfigurationSample(string storedProcName, ASafeLog logger = null) : base(storedProcName, logger)
	//          {
	//          }

	//          public virtual string PropertySample1 { get; protected set; }
	//          public virtual int PropertySample2 { get; protected set; }
	//      } // class
	//  }

	public class Configurations : SafeLog {
		private enum SupportedTypes {
			Int32,
			Int64,
			String,
			Boolean
		} // enum SupportedTypes

		private const string DefaultKeyFieldName = "CfgKey";
		private const string DefaultValueFieldName = "CfgValue";

		private readonly Dictionary<string, PropertyInfo> configPropertyInfos = new Dictionary<string, PropertyInfo>();
		protected string SpName { get; private set; }

		public string KeyFieldName;
		public string ValueFieldName;

		public Configurations(string spNameInput, ASafeLog oLog = null) : base(oLog) {
			SpName = spNameInput;
			KeyFieldName = DefaultKeyFieldName;
			ValueFieldName = DefaultValueFieldName;
		} // constructor

		public virtual void Init() {
			PropertyInfo[] propertyInfos = GetType().GetProperties();

			foreach (PropertyInfo pi in propertyInfos) {
				SupportedTypes ignored;

				bool bInclude =
					pi.GetGetMethod().IsVirtual &&
					pi.GetGetMethod().IsPublic &&
					(pi.GetSetMethod(true) != null) &&
					SupportedTypes.TryParse(pi.PropertyType.Name, out ignored);

				if (bInclude) {
					configPropertyInfos.Add(pi.Name, pi);
					Debug("Configuration property to read from DB: {0}", pi.Name);
				} // if
			} // for each property

			Info("Reading configurations");

			try {
				AConnection oDB = DbConnectionGenerator.Get(this);

				oDB.ForEachRow((row, bRowsetStart) => {
					AddSingleConfiguration(row[KeyFieldName].ToString(), row[ValueFieldName].ToString());
					return ActionResult.Continue;
				}, SpName);
			}
			catch (Exception e) {
				throw new ConfigurationBaseException("Error reading configurations.", e);
			} // try
		} // Init

		public virtual void Refresh() {
			Info("Refreshing configurations");

			try {
				AConnection oDB = DbConnectionGenerator.Get(this);

				oDB.ForEachRow((row, bRowsetStart) => {
					RefreshSingleConfiguration(row[KeyFieldName].ToString(), row[ValueFieldName].ToString());
					return ActionResult.Continue;
				}, SpName);
			}
			catch (Exception e) {
				Error("Error reading configurations: {0}", e);
			} // try
		} // Refresh

		protected virtual void RefreshSingleConfiguration(string key, string value) {
			if (configPropertyInfos.ContainsKey(key)) {
				if (configPropertyInfos[key].GetValue(null).ToString() != value) {
					SetProperty(key, value);
					Info("Refreshed configuration '{0}': '{1}'", key, value);
				} // if
			}
			else
				Error("Unimplemented configuration was read: '{0}' with value '{1}'.", key, value);
		} // RefreshSingleConfiguration

		protected virtual void AddSingleConfiguration(string key, string value) {
			if (configPropertyInfos.ContainsKey(key)) {
				SetProperty(key, value);
				Info("Added configuration '{0}': '{1}'", key, value);
			}
			else
				Error("Unimplemented configuration was read: '{0}' with value '{1}'.", key, value);
		} // AddSingleConfiguration

		protected virtual void SetProperty(string key, string value) {
			SupportedTypes st;

			if (!SupportedTypes.TryParse(configPropertyInfos[key].PropertyType.Name, out st))
				throw new ParseConfigurationBaseException("type is not implemented", key, value);

			switch (st) {
			case SupportedTypes.Int32:
				int valueAsInt;

				if (!int.TryParse(value, out valueAsInt))
					throw new ParseConfigurationBaseException("as int", key, value);

				configPropertyInfos[key].SetValue(this, valueAsInt);
				break;

			case SupportedTypes.Int64:
				long valueAsLong;

				if (!long.TryParse(value, out valueAsLong))
					throw new ParseConfigurationBaseException("as long", key, value);

				configPropertyInfos[key].SetValue(this, valueAsLong);
				break;

			case SupportedTypes.String:
				configPropertyInfos[key].SetValue(this, value);
				break;
			case SupportedTypes.Boolean:
				Boolean valueAsBool;

				if (!Boolean.TryParse(value, out valueAsBool))
					throw new ParseConfigurationBaseException("as bool", key, value);

				configPropertyInfos[key].SetValue(this, valueAsBool);
				break;
			default:
				throw new ParseConfigurationBaseException("type is not implemented", key, value);
			} // switch
		} // SetProperty
	} // class Configurations
} // namespace
