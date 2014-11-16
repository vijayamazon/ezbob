﻿namespace AutomationCalculator.ProcessHistory.ReApproval {
	public class Charges : ANumericTrace {
		#region constructor

		public Charges(int nCustomerID, DecisionStatus nDecisionStatus) : base(nCustomerID, nDecisionStatus) {
		} // constructor

		#endregion constructor

		protected override string ValueStr {
			get {
				return DecisionStatus == DecisionStatus.Affirmative
					? "no charges after the last manually approved cash request"
					: string.Format("charges of {0} after the last manually approved cash request", Value.ToString("N2"));
			} // get
		} // ValueStr
	} // class Charges
} // namespace
