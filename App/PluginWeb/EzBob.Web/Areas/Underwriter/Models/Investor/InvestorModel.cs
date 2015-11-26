namespace EzBob.Web.Areas.Underwriter.Models.Investor {
	using System.Collections.Generic;

	public class InvestorModel {
		public int InvestorID { get; set; }
		public int InvestorType { get; set; }
		public string CompanyName { get; set; }
		public bool IsActive { get; set; }

		public IEnumerable<InvestorContactModel> Contacts { get; set; }
		public IEnumerable<InvestorBankAccountModel> Banks { get; set; } 
	}
}