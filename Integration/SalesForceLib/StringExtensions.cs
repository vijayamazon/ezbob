﻿
namespace SalesForceLib {
	using System;
	using System.Reflection;
	using System.Text;
	using Newtonsoft.Json;

	public static class StringExtensions {
		public static string ToStringExtension(this object obj) {
			var sb = new StringBuilder();
			foreach (PropertyInfo property in obj.GetType().GetProperties()) {
				try {
					var val = property.GetValue(obj, null);
					string valStr = val == null ? null : val.ToString();
					if (!string.IsNullOrEmpty(valStr)) {
						sb.Append(property.Name);
						sb.Append(": ");
						if (property.GetIndexParameters()
							.Length > 0) {
							sb.Append("Indexed Property cannot be used");
						} else {
							sb.Append(valStr);
						}
						sb.Append(Environment.NewLine);
					}
				} catch { 
					//doesn't care
				}
			}

			return sb.ToString();
		}

		public static string ToJsonExtension(this object obj, bool usedate = false) {
			
			JsonSerializerSettings dateFormatSettings = usedate ? 
				new JsonSerializerSettings { DateFormatString = "yyyy-MM-dd" } :
				new JsonSerializerSettings { DateFormatString = "yyyy-MM-ddTHH:mm:ss" };
			
			string jsonInput = JsonConvert.SerializeObject(obj, Formatting.Indented, dateFormatSettings);
			return "[" + jsonInput + "]";
		}

		public static T JsonStringToObject<T>(this string strObj, bool keepBrackets = false)
		{
			if(!keepBrackets) strObj = strObj.Replace("[", "").Replace("]", "");

			T obj = JsonConvert.DeserializeObject<T>(strObj);
			return obj;
		}
	}
}
