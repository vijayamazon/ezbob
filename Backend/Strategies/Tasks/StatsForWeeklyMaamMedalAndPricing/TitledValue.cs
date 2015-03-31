namespace Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing {
	using System;

	internal class TitledValue {
		public static TitledValue Default {
			get { return defaultValue; }
		} // Default

		public TitledValue(object title = null, object val = null) {
			Title = title;
			Value = val ?? string.Empty;
		} // constructor

		public TitledValue(object title, decimal numerator, decimal denominator) {
			Title = title;
			Value = Math.Abs(denominator) < 0.000001m ? 0m : Math.Round(100m * numerator / denominator, 2);
		} // constructor

		public TitledValue(decimal num0, decimal den0, decimal num1, decimal den1) {
			Title = Math.Abs(den0) < 0.000001m ? 0m : Math.Round(100m * num0 / den0, 2);
			Value = Math.Abs(den1) < 0.000001m ? 0m : Math.Round(100m * num1 / den1, 2);
		} // constructor

		public object Title {
			get { return this.title; }
			private set { this.title = value ?? string.Empty; } // set
		} // Title

		public object Value { get; private set; }

		private static readonly TitledValue defaultValue = new TitledValue();

		private object title;
	} // class TitledValue
} // namespace
