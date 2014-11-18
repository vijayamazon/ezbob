namespace AutomationCalculator.ProcessHistory.Common {
	using System;

	public class ExceptionThrown : ATrace {
		public ExceptionThrown(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		public void Init(Exception oException) {
			Thrown = oException;

			Comment = string.Format(
				"exception of type {0} thrown with message '{1}'",
				Thrown.GetType(), Thrown.Message
			);
		} // Init

		public Exception Thrown { get; private set; }
	}  // class ExceptionThrown
} // namespace
