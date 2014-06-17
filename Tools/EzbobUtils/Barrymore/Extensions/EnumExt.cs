namespace Ezbob.Utils.Extensions {
	using System.ComponentModel;

	public static class EnumDescription {
		public static string DescriptionAttr<T>(this T source) {
			try {
				System.Reflection.FieldInfo fi = source.GetType().GetField(source.ToString());

				var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(
					typeof(DescriptionAttribute), false);

				if (attributes.Length > 0)
					return attributes[0].Description;

				return source.ToString();
			}
			catch {
				return "-";
			} // try
		} // DescriptionAttr
	} // class EnumDescription
} // namespace
