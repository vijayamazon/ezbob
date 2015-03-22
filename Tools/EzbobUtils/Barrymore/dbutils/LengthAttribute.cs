namespace Ezbob.Utils.dbutils {
	using System;
	using System.Globalization;

	[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	public class LengthAttribute : Attribute {
		public LengthAttribute(int length) {
			if (length <= 0)
				Length = "255";
			else if (length > 4000)
				Length = "MAX";
			else
				Length = length.ToString(CultureInfo.InvariantCulture);
		} // constructor

		public LengthAttribute(string length) {
			Length = length ?? string.Empty;
		} // constructor

		public string Length { get; private set; }
	} // class LengthAttribute
} // namespace
