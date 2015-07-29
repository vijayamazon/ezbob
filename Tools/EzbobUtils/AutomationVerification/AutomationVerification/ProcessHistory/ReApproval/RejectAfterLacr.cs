namespace AutomationCalculator.ProcessHistory.ReApproval {
	public class RejectAfterLacr : ATrace {
		public RejectAfterLacr(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		public long LacrID { get; private set; }
		public int RejectID { get; private set; }

		public void Init(int nRejectID, long nLacrID) {
			LacrID = nLacrID;
			RejectID = nRejectID;

			if (RejectID > 0) {
				Comment = string.Format(
					"customer has a rejected cash request ({0}) after the last manually approved cash request ({1})",
					RejectID,
					LacrID
				);
			}
			else {
				Comment = string.Format(
					"customer has no rejected cash request after the last manually approved cash request ({0})",
					LacrID
				);
			} // if
		} // Init
	} // class RejectAfterLacr
} // namespace