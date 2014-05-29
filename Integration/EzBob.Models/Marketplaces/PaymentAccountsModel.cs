namespace EzBob.Models.Marketplaces {
	using EzBob.Models;

	public class PaymentAccountsModel : SimpleMarketPlaceModel {
		public int id { get; set; }
		public double TransactionsNumber { get; set; }
		public double TotalNetInPayments { get; set; }
		public double MonthInPayments { get; set; }
		public double MonthInPaymentsAnnualized { get; set; }
		public double TotalNetOutPayments { get; set; }
		public string Status { get; set; }
		public bool IsNew { get; set; }
	} // class PaymentAccountsModel
} // namespace
