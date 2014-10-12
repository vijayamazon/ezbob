namespace EzBob.Backend.Strategies.VatReturn {
	using System.Globalization;

	public partial class CalculateVatReturnSummary : AStrategy {
		private class AnnualizedData {
			public decimal? Turnover { get; set; }
			public decimal? ValueAdded { get; set; }
			public decimal? FreeCashFlow { get; set; }

			public string ToString(string sPrefix) {
				sPrefix = string.IsNullOrEmpty(sPrefix) ? string.Empty : sPrefix;

				return string.Format(
					"{0}Turnover: {1}\n{0}Value added: {2}\n{0}Free cash flow: {3}",
					sPrefix,
					Turnover.HasValue ? Turnover.Value.ToString(CultureInfo.InvariantCulture) : "-- null --",
					ValueAdded.HasValue ? ValueAdded.Value.ToString(CultureInfo.InvariantCulture) : "-- null --",
					FreeCashFlow.HasValue ? FreeCashFlow.Value.ToString(CultureInfo.InvariantCulture) : "-- null --"
				);
			} // ToString
		} // class AnnualizedData
	} // class CalculateVatReturnSummary
} // namespace
