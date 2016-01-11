namespace Ezbob.Backend.ModelsWithDB {
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
		public string propertyTab = Environment.NewLine;
		public const string novalue = "--";

		public const string pad = " ";

		public static int ColumnTotalWidth = 26;

		public static ExcludeFromToStringAttribute NonPrintableInstance = new ExcludeFromToStringAttribute(false);
		public static CultureInfo NLCultureInfo = new CultureInfo("en-GB");

		/// <exception><paramref /> cannot be cast to the element type of the current <see cref="T:System.Array" />.</exception>
		/// <exception cref="InvalidCastException"><paramref /> cannot be cast to the element type of the current <see cref="T:System.Array" />.</exception>
		public string ToStringAsTable() {
			Type t = GetType();
			var props = FilterPrintable(t);

			props.RemoveAll(p => p.PropertyType.IsClass);

			int propsCount = props.Count;

			StringBuilder format = new StringBuilder();
			object[] values = new object[propsCount];

			foreach (var x in ForeachExt.WithIndex(props)) {
				PropertyInfo prop = x.Value;
				var val = prop.GetValue(this);
				var headerLen = prop.Name.Length;

				// append value placeholder "{index"
				format.Append("{").Append(x.Index);

				if (val != null) {
					var enumattr = prop.GetCustomAttribute(typeof(EnumNameAttribute)) as EnumNameAttribute;
					// display enum name
					val = (enumattr != null) ? enumattr.GetName((int)val) : val;

					var formatattr = prop.GetCustomAttribute(typeof(DecimalFormatAttribute)) as DecimalFormatAttribute;
					//Console.WriteLine(prop.PropertyType.Name + "=" + prop.PropertyType.FullName);

					string propType = prop.PropertyType.Name;

					if (propType.Equals("Nullable`1")) {
						if (prop.PropertyType.FullName.Contains("System.Decimal")) {
							propType = "Decimal";
						}
						if (prop.PropertyType.FullName.Contains("System.DateTime")) {
							propType = "DateTime";
						}
					}
					switch (propType) {
					case "Decimal":
						if (formatattr != null)
							format.Append(",-")
								.Append(headerLen)
								.Append(":")
								.Append(formatattr.format)
								.Append("} "); // percent or other decimal formatting
						else
							format.Append(",-")
								.Append(headerLen)
								.Append(":F} "); // other decimals
						break;
					case "DateTime":
						format.Append(",-")
							.Append(headerLen)
							.Append(":d} "); //date
						break;
					default:
						format.Append(",-")
							.Append(headerLen)
							.Append("} "); // int/string
						break;
					}
				} else {
					val = novalue;
					format.Append(",-")
							.Append(headerLen)
							.Append("} "); // null value
				}
				values.SetValue(val, x.Index);
			}
			if (!values.Any()) 
				return "No " + t.Name + " found.";

			//Console.WriteLine(format);
			//values.ToList().ForEach(i => Console.WriteLine(","+i.ToString()));

			return string.Format(format.ToString(), values) + Environment.NewLine;
		}

		/// <summary>
		/// returns string of headers line-printable properties
		/// </summary>
		/// <returns></returns>
		/// <exception cref="InvalidCastException"><paramref /> cannot be cast to the element type of the current <see cref="T:System.Array" />.</exception>
		public static string PrintHeadersLine(Type t) {
			var props = FilterPrintable(t);
			int propsCount = props.Count;
			//	headers line
			StringBuilder header = new StringBuilder();
			object[] headers = new object[propsCount];
			bool fillHeadersline = true;
			//	build headers line
			foreach (var x in ForeachExt.WithIndex(props)) {
				if (!fillHeadersline)
					continue;
				if (x.Index < propsCount && fillHeadersline) {
					// append header placeholder "{0, -4}"
					header.Append("{").Append(x.Index).Append(",-4} ");
					// set property name
					headers.SetValue(x.Value.Name, x.Index);
					if (x.IsLast)
						fillHeadersline = false;
				}
			}
			return string.Format(header.ToString(), headers) + Environment.NewLine;
		}

		public override string ToString() {
			StringBuilder sb = new StringBuilder(GetType().Name + ": ");
			string strVal = "";
			this.Traverse((instance, prop) => {
				bool toPrint = (prop.GetCustomAttribute(typeof(ExcludeFromToStringAttribute)) == null || (prop.GetCustomAttribute(typeof(ExcludeFromToStringAttribute)) != null && prop.GetCustomAttribute(typeof(ExcludeFromToStringAttribute)).Equals(NonPrintableInstance)));
				object val = prop.GetValue(this);
				// display enum name
				if (val != null && toPrint) {
					strVal = val.ToString();
					var formatattr = prop.GetCustomAttribute(typeof(DecimalFormatAttribute)) as DecimalFormatAttribute;
					if (formatattr != null)
						strVal = formatattr.Formatted((decimal)val);

					var enumattr = prop.GetCustomAttribute(typeof(EnumNameAttribute)) as EnumNameAttribute;
					strVal = (enumattr != null) ? enumattr.GetName((int)val) : strVal;

					sb.Append(prop.Name).Append(": ").Append(strVal).Append(this.propertyTab); 

					if(this.propertyTab != Environment.NewLine) sb.Append(Environment.NewLine);
				}
			});
			return sb.ToString();
		} // ToString


		public static List<PropertyInfo> FilterPrintable(Type t) {
			return t.GetProperties().Where(p => p.GetCustomAttribute(typeof(ExcludeFromToStringAttribute)) == null
				|| (p.GetCustomAttribute(typeof(ExcludeFromToStringAttribute)) != null && p.GetCustomAttribute(typeof(ExcludeFromToStringAttribute)).Equals(NonPrintableInstance))).ToList();
		}

		public string BaseString() {
			return this.ToString();
		}
	} // class AStringable
} // namespace
