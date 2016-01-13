namespace Ezbob.Utils.dbutils {
	using System;
	using System.ComponentModel;
	using System.Globalization;
	using Ezbob.Utils.Extensions;

	[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	public class LengthAttribute : Attribute {
		public LengthAttribute(int length) {
			if (length <= 0) {
				Quantity = 255;
				Length = "255";
			} else if (length > 4000) {
				Quantity = Int32.MaxValue;
				Length = "MAX";
			} else {
				Quantity = length;
				Length = length.ToString(CultureInfo.InvariantCulture);
			} // if
		} // constructor

		public LengthAttribute(LengthType length) {
			Quantity = (int)length;
			Length = length.DescriptionAttr() ?? string.Empty;
		} // constructor

		public string Length { get; private set; }

		public int Quantity { get; private set; }
	} // class LengthAttribute

	public enum LengthType {
		[Description("22, 2")]
		D22E2 = 22,

		[Description("18, 6")]
		D18E6 = 18,

		[Description("MONEY")]
		Money = 19,

		[Description("MAX")]
		MAX = Int32.MaxValue,
	}//enum LengthType
} // namespace
