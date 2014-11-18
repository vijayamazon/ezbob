﻿namespace AutomationCalculator.ProcessHistory.AutoRejection
{
	public class ConsumerScoreException : AThresholdTrace {
		public ConsumerScoreException(int nCustomerID, DecisionStatus nDecisionStatus) : base(nCustomerID, nDecisionStatus) {} // constructor

		protected override string ValueName {
			get { return "consumer score exception"; }
		} // ValueName
	}  // class
} // namespace
