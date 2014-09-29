using Ezbob.Logger;
using Ezbob.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace ConfigurationBase {
	// How to use
	// 1. Add reference to C:\DEV\Dlls\ConfigurationBase.dll
	// 2. Create a class like:
	//  namespace EzSupportAgent
	//  {
	//      using ConfigurationBase;

	//      public class ConfigurationSample : ConfigurationsBase
	//      {
	//          static ConfigurationSample()
	//          {
	//              Init(typeof (ConfigurationSample));
	//          }

	//          public static string PropertySample1 { get; private set; }
	//          public static int PropertySample2 { get; private set; }
	//      }
	//  }
	//
	// 3. (Optional) Use this line at the beginning of your code to make it initialized soon
	//  var touchToInit = ConfigurationSample.PropertySample1;
	//
	// 4. Can be accessed anywhere: 
	//  ConfigurationSample.PropertySample1
	//  ConfigurationSample.PropertySample2

	public class ConfigurationsBase {
		private const string TypeNameInt = "Int32";
		private const string TypeNameString = "String";
		private const string TypeNameLong = "Int64";

		private static int readConfigurationsCounter;
		private static readonly Dictionary<string, PropertyInfo> configPropertyInfos = new Dictionary<string, PropertyInfo>();
		private static PropertyInfo[] propertyInfos;
		private static string spName;

		private static ASafeLog Logger { get; set; }

		protected static void Init(Type derivedType, string spNameInput, ASafeLog oLog = null) {
			Logger = new SafeLog(oLog);

			spName = spNameInput;
			propertyInfos = derivedType.GetProperties(BindingFlags.Public | BindingFlags.Static);

			foreach (PropertyInfo pi in propertyInfos)
				configPropertyInfos.Add(pi.Name, pi);

			Read();
		} // Init

		private static void Read() {
			Logger.Info("Reading configurations");

			try {
				var oDB = new SqlConnection();

				oDB.ForEachRow((row, bRowsetStart) => {
					AddSingleConfiguration(row["CfgKey"].ToString(), row["CfgValue"].ToString());
					return ActionResult.Continue;
				}, spName);

				if (readConfigurationsCounter != propertyInfos.Length) {
					Logger.Error("Couldn't fill all properties");
					Environment.Exit(-1);
				} // if
			}
			catch (Exception e) {
				Logger.Error("Error reading configurations:{0}", e);
				Environment.Exit(-1);
			} // try
		} // Read

		public static void Refresh() {
			Logger.Info("Refreshing configurations");

			try {
				var oDB = new SqlConnection();

				oDB.ForEachRow((row, bRowsetStart) => {
					RefreshSingleConfiguration(row["CfgKey"].ToString(), row["CfgValue"].ToString());
					return ActionResult.Continue;
				}, spName);
			}
			catch (Exception e) {
				Logger.Error("Error reading configurations:{0}", e);
			} // try
		} // Refresh

		private static void RefreshSingleConfiguration(string key, string value) {
			if (configPropertyInfos.ContainsKey(key)) {
				if (configPropertyInfos[key].GetValue(null).ToString() != value) {
					SetProperty(key, value);
					Logger.Info("Refreshed configuration {0}: {1}", key, value);
				} // if
			}
			else
				Logger.Error("Unimplemented configuration was read:{0} with value:{1}.", key, value);
		} // RefreshSingleConfiguration

		private static void AddSingleConfiguration(string key, string value) {
			if (configPropertyInfos.ContainsKey(key)) {
				SetProperty(key, value);
				readConfigurationsCounter++;
				Logger.Info("Added configuration {0}: {1}", key, value);
			}
			else
				Logger.Error("Unimplemented configuration was read:{0} with value:{1}.", key, value);
		} // AddSingleConfiguration

		private static void SetProperty(string key, string value) {
			switch (configPropertyInfos[key].PropertyType.Name) {
			case TypeNameInt:
				int valueAsInt;

				if (!int.TryParse(value, out valueAsInt)) {
					Logger.Error("Error parsing configuration:{0} with value:{1} as int", key, value);
					Environment.Exit(-1);
				} // if

				configPropertyInfos[key].SetValue(null, valueAsInt);
				break;

			case TypeNameLong:
				long valueAsLong;

				if (!long.TryParse(value, out valueAsLong)) {
					Logger.Error("Error parsing configuration:{0} with value:{1} as long", key, value);
					Environment.Exit(-1);
				} // if

				configPropertyInfos[key].SetValue(null, valueAsLong);
				break;

			case TypeNameString:
				configPropertyInfos[key].SetValue(null, value);
				break;

			default:
				Logger.Error("Error parsing configuration:{0} with value:{1}. Type is not implemented", key, value);
				Environment.Exit(-1);
				break;
			} // switch
		} // SetProperty
	} // class ConfigurationBase
} // namespace
