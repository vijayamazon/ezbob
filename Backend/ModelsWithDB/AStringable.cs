﻿namespace Ezbob.Backend.ModelsWithDB.NewLoan {
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
		public string propertyTab = "\n";
		public const string novalue = "--";

		public const string pad = " ";

		public static int ColumnTotalWidth = 26;

		public static ExcludeFromToStringAttribute NonPrintableInstance = new ExcludeFromToStringAttribute(false);
		public static CultureInfo NLCultureInfo = new CultureInfo("en-GB");

		/// <exception><paramref /> cannot be cast to the element type of the current <see cref="T:System.Array" />.</exception>
		/// <exception cref="InvalidCastException"><paramref /> cannot be cast to the element type of the current <see cref="T:System.Array" />.</exception>
		public string ToStringTable() {
			Type t = GetType();
			var props = FilterPrintable(t);
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

					var formatattr = prop.GetCustomAttribute(typeof(DecimalFormatAttribute)) as DecimalFormatAttribute;

					switch (prop.PropertyType.Name) {
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
								.Append(":C2} "); // other decimals
						break;
					case "DateTime":
						format.Append(",-")
							.Append(headerLen)
							.Append(":dd/MM/yy} "); //date
						break;
					default:
						format.Append(",-")
							.Append(headerLen)
							.Append("} "); // int/string
						break;
					}

					var enumattr = prop.GetCustomAttribute(typeof(EnumNameAttribute)) as EnumNameAttribute;
					// display enum name
					val = (enumattr != null) ? enumattr.GetName((int)val) : val;

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
			//Console.WriteLine(values);

			return string.Format(format.ToString(), values) + Environment.NewLine;
		}
		/// <summary>
		/// returns string of headers line-printable properties
		/// </summary>
		/// <returns></returns>
		//public string ToStringHeadersLine() {
		public static string GetHeadersLine(Type t) {
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

				bool toPrint = (CustomAttributeExtensions.GetCustomAttribute((MemberInfo)prop, typeof(ExcludeFromToStringAttribute)) == null || (CustomAttributeExtensions.GetCustomAttribute((MemberInfo)prop, typeof(ExcludeFromToStringAttribute)) != null && CustomAttributeExtensions.GetCustomAttribute((MemberInfo)prop, typeof(ExcludeFromToStringAttribute)).Equals(NonPrintableInstance)));
				object val = prop.GetValue(this);

				if (val != null && toPrint) {
					strVal = val.ToString();
					var formatattr = CustomAttributeExtensions.GetCustomAttribute((MemberInfo)prop, typeof(DecimalFormatAttribute)) as DecimalFormatAttribute;
					if (formatattr != null)
						strVal = formatattr.Formatted((decimal)val);
					var enumattr = CustomAttributeExtensions.GetCustomAttribute((MemberInfo)prop, typeof(EnumNameAttribute)) as EnumNameAttribute;
					if (enumattr != null)
						strVal = enumattr.GetName((int)val);
					sb.Append("\t").Append(prop.Name).Append(": ").Append(strVal).Append(Environment.NewLine);
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
