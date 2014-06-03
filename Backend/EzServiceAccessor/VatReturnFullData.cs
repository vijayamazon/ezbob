namespace EzServiceAccessor {
	using Ezbob.Backend.Models;

	public class VatReturnFullData {
		public VatReturnRawData[] VatReturnRawData { get; set; }
		public RtiTaxMonthRawData[] RtiTaxMonthRawData { get; set; }
		public VatReturnSummary[] Summary { get; set; }
	} // class VatReturnFullData
} // namespace EzServiceAccessor
