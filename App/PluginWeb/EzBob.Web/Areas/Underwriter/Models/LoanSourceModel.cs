using EZBob.DatabaseLib.Model.Database.Loans;

namespace EzBob.Web.Areas.Underwriter.Models {
	public class LoanSourceModel {
		public int Id { get; set; }
		public string Name { get; set; }
		public decimal MaxInterest { get; set; }
		public int DefaultRepaymentPeriod { get; set; }
		public bool IsCustomerRepaymentPeriodSelectionAllowed { get; set; }
		public int MaxEmployeeCount { get; set; }
		public decimal MaxAnnualTurnover { get; set; }
		public bool IsDefault { get; set; }
		public int AlertOnCustomerReasonType { get; set; }

		public LoanSourceModel() {
			Id = -1;
			Name = "";
			MaxInterest = -1m;
			DefaultRepaymentPeriod = -1;
			IsCustomerRepaymentPeriodSelectionAllowed = true;
			MaxEmployeeCount = -1;
			MaxAnnualTurnover = -1;
			IsDefault = false;
			AlertOnCustomerReasonType = -1;
		} // default constructor

		public LoanSourceModel(LoanSource ls) : this() {
			if (ls == null)
				return;

			Id = ls.ID;
			Name = ls.Name;
			MaxInterest = ls.MaxInterest ?? -1m;
			DefaultRepaymentPeriod = ls.DefaultRepaymentPeriod ?? -1;
			IsCustomerRepaymentPeriodSelectionAllowed = ls.IsCustomerRepaymentPeriodSelectionAllowed;
			MaxEmployeeCount = ls.MaxEmployeeCount ?? -1;
			MaxAnnualTurnover = ls.MaxAnnualTurnover ?? -1;
			IsDefault = ls.IsDefault;
			AlertOnCustomerReasonType = ls.AlertOnCustomerReasonType ?? -1;
		} // constructor
	} // class LoanSourceModel
} // namespace