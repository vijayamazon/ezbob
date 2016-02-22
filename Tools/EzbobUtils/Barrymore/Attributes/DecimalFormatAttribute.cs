namespace Ezbob.Utils.Attributes {
	using System;
	using System.Globalization;

	[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
	public class DecimalFormatAttribute : Attribute {

		public string FormattedString { get; set; }
		public string format;
		public CultureInfo culture = new CultureInfo("en-GB");

		public DecimalFormatAttribute(string format) {
			//if (format == "percent")
			//	format = "F4";
			this.format = format;
		}

		public string Formatted(decimal propertyValue = 0m) {
			FormattedString = propertyValue.ToString(this.format, this.culture);
			return FormattedString;
		}


	}
}
