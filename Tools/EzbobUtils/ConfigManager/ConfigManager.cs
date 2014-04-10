namespace ConfigManager
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Globalization;
	using Ezbob.Database;
	using Ezbob.Logger;
	using log4net;

	public static class ConfigManager
	{
		private static readonly AConnection db;
		private static readonly SafeILog log;
		private static readonly Dictionary<string, string> configs;

		static ConfigManager()
		{
			log = new SafeILog(LogManager.GetLogger(typeof(ConfigManager)));
			db = new SqlConnection(new Ezbob.Context.Environment(), log);

			configs = new Dictionary<string, string>();
			DataTable dt = db.ExecuteReader("GetAllConfigurationVariables", CommandSpecies.StoredProcedure);
			foreach (DataRow row in dt.Rows)
			{
				var sr = new SafeReader(row);
				string name = sr["Name"];
				string value = sr["Value"];

				if (!configs.ContainsKey(name))
				{
					configs.Add(name, value);
				}
			}
		}

		public static string GetByName(string name)
		{
			if (configs.ContainsKey(name))
			{
				return configs[name];
			}

			return string.Empty;
		}

		public static decimal GetByNameAsDecimal(string name)
		{
			return Convert.ToDecimal(GetByName(name), CultureInfo.InvariantCulture);
		}

		public static int GetByNameAsInt(string name)
		{
			return Convert.ToInt32(GetByName(name), CultureInfo.InvariantCulture);
		}

		public static bool GetByNameAsBool(string name)
		{
			string value = GetByName(name);
			return value.ToLower() == "true" || value == "1";
		}
	}
}
