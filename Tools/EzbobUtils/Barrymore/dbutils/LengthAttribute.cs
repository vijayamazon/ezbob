namespace Ezbob.Utils.dbutils {
	using System;
	using System.ComponentModel;
	using System.Globalization;
	using Ezbob.Utils.Extensions;

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

        public LengthAttribute(LengthType length) {
			Length = length.DescriptionAttr() ?? string.Empty;
		} // constructor

		public string Length { get; private set; }
	} // class LengthAttribute

    public enum LengthType {
        [Description("MAX")]
        MAX,
        [Description("22, 2")]
        D22E2,
        [Description("18, 6")]
        D18E6,
        [Description("MONEY")]
        Money
    }//enum LengthType
} // namespace
