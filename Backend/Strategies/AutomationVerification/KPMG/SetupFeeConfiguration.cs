namespace Ezbob.Backend.Strategies.AutomationVerification.KPMG {
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using Ezbob.Database;
	using PaymentServices.Calculators;

	[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
	public class SetupFeeConfiguration {
		public int UseSetupFee { get; set; }
		public bool UseBrokerSetupFee { get; set; }

		[FieldName("ManualSetupFeePercent")]
		public decimal? Percent { get; set; }

		[FieldName("ManualSetupFeeAmount")]
		public decimal? Amount { get; set; }

		public void Calculate(AMedalAndPricing map) {
			if (map == null)
				return;

			map.SetupFee = new SetupFeeCalculator(
				UseSetupFee == 1,
				UseBrokerSetupFee,
				(int)(Amount ?? 0),
				Percent
			).Calculate(map.Amount, true);
		} // Calculate

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString() {
			return string.Format(
				"( use fee: {0}, broker fee: {1}; manual pct: {2}, amount {3} )",
				UseSetupFee,
				UseBrokerSetupFee,
				Percent.HasValue ? Percent.Value.ToString("P2", CultureInfo.InvariantCulture) : "--",
				Amount.HasValue ? Amount.Value.ToString("N2", CultureInfo.InvariantCulture) : "--"
			);
		} // ToString
	} // class SetupFeeConfiguration
} // namespace
