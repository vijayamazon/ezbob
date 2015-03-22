namespace Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing {
	using System;

	internal class TitledValue {
		public static TitledValue Default {
			get { return defaultValue; }
		} // Default

		public TitledValue(string title = null, object val = null) {
			Title = title;
			Value = val ?? string.Empty;
		} // constructor

		public TitledValue(string title, decimal numerator, decimal denominator) {
			Title = title;
			Value = Math.Abs(denominator) < 0.000001m ? 0m : Math.Round(100m * numerator / denominator, 2);
		} // constructor

		public string Title {
			get { return this.title; }
			private set { this.title = value ?? string.Empty; } // set
		} // Title

		public object Value { get; private set; }

		private static readonly TitledValue defaultValue = new TitledValue();

		private string title;
	} // class TitledValue
} // namespace
