
namespace SalesForceLib {
	using System;
	using System.Reflection;
	using System.Text;

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
	}
}
