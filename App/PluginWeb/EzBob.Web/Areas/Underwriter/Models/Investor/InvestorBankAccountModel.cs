namespace EzBob.Web.Areas.Underwriter.Models.Investor {
	public class InvestorBankAccountModel {
		public int InvestorBankAccountID { get; set; }
		public string AccountType { get; set; }
		public string BankSortCode { get; set; }
		public string BankAccountNumber { get; set; }
		public string BankAccountName { get; set; }
	}
}