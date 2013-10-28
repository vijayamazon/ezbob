using EZBob.DatabaseLib.Model.Database.Loans;

namespace EzBob.Web.Areas.Underwriter.Models {
	public class LoanSourceModel {
		public int Id { get; set; }
		public string Name { get; set; }
		public decimal MaxInterest { get; set; }
		public int DefaultRepaymentPeriod { get; set; }
		public bool IsCustomerRepaymentPeriodSelectionAllowed { get; set; }

		public static LoanSourceModel Create(LoanSource ls) {
			return new LoanSourceModel {
				Id = ls.ID,
				Name = ls.Name,
				MaxInterest = ls.MaxInterest ?? -1m,
				DefaultRepaymentPeriod = ls.DefaultRepaymentPeriod ?? -1,
				IsCustomerRepaymentPeriodSelectionAllowed = ls.IsCustomerRepaymentPeriodSelectionAllowed
			};
		} // Create
	} // class LoanSourceModel
} // namespace