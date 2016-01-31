namespace EzBob.Web.Areas.Underwriter.Models.Investor {
    using System;

    public class InvestorBankAccountModel {
		public int InvestorBankAccountID { get; set; }
		public int AccountType { get; set; }
		public string AccountTypeStr { get; set; }
		public string BankSortCode { get; set; }
		public string BankAccountNumber { get; set; }
		public string BankAccountName { get; set; }
        public DateTime TimeStamp { get; set; }
		public bool IsActive { get; set; }
	}
}