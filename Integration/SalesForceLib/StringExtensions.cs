
namespace SalesForceLib {
	using System;
	using System.Reflection;
	using System.Text;
	using Newtonsoft.Json;

	public static class StringExtensions {
		public static string ToStringExtension(this object obj) {
			var sb = new StringBuilder();
			foreach (PropertyInfo property in obj.GetType().GetProperties()) {
				sb.Append(property.Name);
				sb.Append(": ");
				if (property.GetIndexParameters().Length > 0) {
					sb.Append("Indexed Property cannot be used");
				} else {
					sb.Append(property.GetValue(obj, null));
				}

				sb.Append(Environment.NewLine);
			}

			return sb.ToString();
		}

		public static string ToJsonExtension(this object obj) {

			JsonSerializerSettings dateFormatSettings = new JsonSerializerSettings
		    {
				DateFormatString = "yyyy-MM-dd"
			};

			string jsonInput = JsonConvert.SerializeObject(obj, Formatting.Indented, dateFormatSettings);
			return "[" + jsonInput + "]";
		}

		public static T JsonStringToObject<T>(this string strObj) {
			T obj = JsonConvert.DeserializeObject<T>(strObj);
			return obj;
		}
	}
}
