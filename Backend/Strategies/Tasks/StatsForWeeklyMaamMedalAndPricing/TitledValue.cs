namespace Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing {
	using System;

	internal class TitledValue {
		public static TitledValue Default {
			get { return defaultValue; }
		} // Default

		public TitledValue(object title = null, object val = null) {
			Title = title;
			Value = val ?? string.Empty;

			TitleFormat = Format.Detect(title);
			ValueFormat = Format.Detect(val);
		} // constructor

		public TitledValue(object title, decimal numerator, decimal denominator) {
			Title = title;
			Value = Math.Abs(denominator) < 0.000001m ? 0m : numerator / denominator;

			TitleFormat = Format.Detect(title);
			ValueFormat = Format.Percent;
		} // constructor

		public TitledValue(decimal num0, decimal den0, decimal num1, decimal den1) {
			Title = Math.Abs(den0) < 0.000001m ? 0m : num0 / den0;
			Value = Math.Abs(den1) < 0.000001m ? 0m : num1 / den1;

			TitleFormat = Format.Percent;
			ValueFormat = Format.Percent;
		} // constructor

		public object Title {
			get { return this.title; }
			private set { this.title = value ?? string.Empty; } // set
		} // Title

		public object Value { get; private set; }

		public string TitleFormat { get; private set; }
		public string ValueFormat { get; private set; }

		private static readonly TitledValue defaultValue = new TitledValue { TitleFormat = null, ValueFormat = null, };

		public static class Format {
			public const string Default = null;
			public const string Int = "#,##0";
			public const string Money = "£ #,##0.00";
			public const string Percent = "0.00%";

			public static string Detect(object val) {
				if (val == null)
					return Default;

				if (val is int)
					return Int;

				if (val is decimal)
					return Money;

				return Default;
			} // Detect
		} // class Format

		private object title;
	} // class TitledValue
} // namespace
