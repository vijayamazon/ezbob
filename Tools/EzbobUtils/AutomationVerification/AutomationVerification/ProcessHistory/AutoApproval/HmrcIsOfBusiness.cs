namespace AutomationCalculator.ProcessHistory.AutoApproval {
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	/// Checks weather the uploaded or linked HMRC business name same as experian company name
	/// </summary>
	public class HmrcIsOfBusiness : ATrace {
		public HmrcIsOfBusiness(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {} // constructor
		
		/// <summary>
		/// Init if has HMRC.
		/// </summary>
		public void Init(List<NameForComparison> hmrcBusinessName, NameForComparison experianCompanyName) {
			Comment = string.Format(
				"HMRC business name: '{0}', experian business name: '{1}'", 
				hmrcBusinessName.Select(n => n.RawName).Aggregate((a, b) => string.Format("{0},{1}", a, b)),
				experianCompanyName.RawName
			);
		} // Init

		/// <summary>
		/// Init if has no HMRC.
		/// </summary>
		public void Init() {
			Comment = "Customer has no HMRC.";
		} // Init
	} // class
} // namespace