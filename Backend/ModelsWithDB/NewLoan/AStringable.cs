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
	using Ezbob.Utils.Extensions;

	[DataContract(IsReference = true)]
	public abstract class AStringable {

		public const string lineSeparatorChar = "";//  "-";
		public const string propertyDelimiter = ""; //"|";
		public const string propertyTab = "\n";

		public const string pad = " ";

		public static int ColumnTotalWidth = 26;

		public static ExcludeFromToStringAttribute NonPrintableInstance = new ExcludeFromToStringAttribute(false);
		public static CultureInfo NLCultureInfo = new CultureInfo("en-GB");

		/// <exception><paramref /> cannot be cast to the element type of the current <see cref="T:System.Array" />.</exception>
		/// <exception cref="InvalidCastException"><paramref /> cannot be cast to the element type of the current <see cref="T:System.Array" />.</exception>
		public string ToStringTable() {

			Type t = GetType();
			var props = FilterPrintable(t);

			StringBuilder format = new StringBuilder();
			StringBuilder header = new StringBuilder();

			object[] values = new object[props.Count];
			object[] headers = new object[props.Count];

			foreach (var x in ForeachExt.WithIndex(props)) {

				PropertyInfo prop = x.Value;
				var val = prop.GetValue(this);
				var headerLen = prop.Name.Length;
				var enumattr = prop.GetCustomAttribute(typeof(EnumNameAttribute)) as EnumNameAttribute;

				if (val != null) {

					var formatattr = prop.GetCustomAttribute(typeof(DecimalFormatAttribute)) as DecimalFormatAttribute;

					// append header placeholder "{0, -2}"
					header.Append("{").Append(x.Index).Append(",-4} ");

					// append value placeholder "{index"
					format.Append("{").Append(x.Index);

					//var formattedLen = 0;
					if (prop.PropertyType.Name == "Decimal") {

						if (formatattr != null) {

							format.Append(",-")
								.Append(headerLen)
								.Append(":")
								.Append(formatattr.format)
								.Append("} ");// percent

						} else {

							format.Append(",-")
							.Append(headerLen)
							.Append(":C2} ");	// other decimals
						}
					} else if (prop.PropertyType.Name == "DateTime") {


						format.Append(",-")
							.Append(headerLen)
							.Append(":MM/dd/yy} ");//date
					} else
						format.Append(",-")
							.Append(headerLen)
							.Append("} "); // int/string
				} else
					val = "";

				// enum name
				val = (enumattr != null) ? enumattr.GetName((int)val) : val;

				headers.SetValue(prop.Name, x.Index);
				values.SetValue(val, x.Index);
			}



			return Environment.NewLine + string.Format(header.ToString(), headers) + Environment.NewLine + string.Format(format.ToString(), values);
		}

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
