namespace AutomationCalculator.ProcessHistory.ReApproval {
	public class RejectAfterLacr : ATrace {
		public RejectAfterLacr(int nCustomerID, DecisionStatus nDecisionStatus) : base(nCustomerID, nDecisionStatus) {
		} // constructor

		public int LacrID { get; private set; }
		public int RejectID { get; private set; }

		public void Init(int nRejectID, int nLacrID) {
			LacrID = nLacrID;
			RejectID = nRejectID;

			if (RejectID > 0) {
				Comment = string.Format(
					"customer {0} has rejected cash request ({1}) after the last manually approved cash request ({2})",
					CustomerID,
					RejectID,
					LacrID
				);
			}
			else {
				Comment = string.Format(
					"customer {0} has no rejected cash request after the last manually approved cash request ({1})",
					CustomerID,
					LacrID
				);
			} // if
		} // Init
	} // class RejectAfterLacr
} // namespace