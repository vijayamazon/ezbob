namespace EzBob.Web.Areas.Broker.Models {
	public class GetInstantOfferModel {
		public string CompanyNameNumber { get; set; }
		public decimal AnnualTurnover { get; set; }
		public decimal AnnualProfit { get; set; }
		public int NumOfEmployees { get; set; }
		public bool IsHomeOwner { get; set; }
		public string MainApplicantCreditScore { get; set; }
	}
}