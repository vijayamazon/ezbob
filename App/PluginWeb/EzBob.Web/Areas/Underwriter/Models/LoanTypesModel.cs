using EZBob.DatabaseLib.Model.Loans;

namespace EzBob.Web.Areas.Underwriter.Models {
	public class LoanTypesModel {
		public int Id { get; set; }
		public string Name { get; set; }

		public int value { get; set; }
		public string text { get; set; }
		public int RepaymentPeriod { get; set; }

		public static LoanTypesModel Create(LoanType lt) {
			return new LoanTypesModel {
				Id = lt.Id,
				Name = lt.Name,
				value = lt.Id,
				text = lt.Name,
				RepaymentPeriod = lt.RepaymentPeriod
			};
		} // Create
	} // class LoanTypesModel
} // namespace
