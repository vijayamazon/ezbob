namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Reflection;
	using System.Runtime.Serialization;
	using System.Text;
	using Ezbob.Utils;
	using Ezbob.Utils.Attributes;

	[DataContract(IsReference = true)]
	public abstract class AStringable {

		public const string lineSeparatorChar =  "-";
		public const string propertyDelimiter = "|";
		public const string propertyTab = "\t";

		public static ExcludeFromToStringAttribute NonPrintableInstance = new ExcludeFromToStringAttribute(false);
		public static CultureInfo NLCultureInfo = new CultureInfo("en-GB");

		public override string ToString() {

			StringBuilder sb = new StringBuilder(GetType().Name + ": ");
			string strVal = "";
			this.Traverse((instance, prop) => {

				bool toPrint = (prop.GetCustomAttribute(typeof(ExcludeFromToStringAttribute)) == null || (prop.GetCustomAttribute(typeof(ExcludeFromToStringAttribute)) != null && prop.GetCustomAttribute(typeof(ExcludeFromToStringAttribute)).Equals(NonPrintableInstance)));
				object val = prop.GetValue(this);

				if (val != null && toPrint) {

					strVal = val.ToString();

					var formatattr = prop.GetCustomAttribute(typeof(DecimalFormatAttribute)) as DecimalFormatAttribute;
					if (formatattr != null)
						strVal = formatattr.Formatted((decimal)val);

					var enumattr = prop.GetCustomAttribute(typeof(EnumNameAttribute)) as EnumNameAttribute;
					if (enumattr != null)
						strVal = enumattr.GetName((int)val);

					sb.Append(prop.Name).Append(": ").Append(strVal).Append(Environment.NewLine);
				}
			});

			return sb.ToString();
		} // ToString


		public static List<PropertyInfo> FilterPrintable(Type t) {
			return t.GetProperties().Where(p => p.GetCustomAttribute(typeof(ExcludeFromToStringAttribute)) == null
				|| (p.GetCustomAttribute(typeof(ExcludeFromToStringAttribute)) != null && p.GetCustomAttribute(typeof(ExcludeFromToStringAttribute)).Equals(NonPrintableInstance))).ToList();
		}

		public static string HeadersLine(Type t, int totalWidth) {
			var props = FilterPrintable(t); 
			string lineSeparator =  Environment.NewLine + lineSeparatorChar.PadRight(totalWidth * props.Count, '-') + Environment.NewLine;
			StringBuilder sb = new StringBuilder(t.Name).Append(lineSeparator);
			props.ForEach(c => sb.Append(propertyDelimiter).Append(c.Name.PadRight(totalWidth)));
			sb.Append(lineSeparator);
			return sb.ToString();
		}

	} // class AStringable
} // namespace
