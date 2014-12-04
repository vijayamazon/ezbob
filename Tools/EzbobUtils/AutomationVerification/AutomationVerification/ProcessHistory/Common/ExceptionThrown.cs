namespace AutomationCalculator.ProcessHistory.Common {
	using System;

	public class ExceptionThrown : ATrace {
		public delegate void OnAfterInit(ExceptionThrown obj);

		public event OnAfterInit OnAfterInitEvent;

		public ExceptionThrown(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		public void Init(Exception oException) {
			Thrown = oException;

			Comment = string.Format(
				"exception of type {0} thrown with message '{1}'",
				Thrown.GetType(), Thrown.Message
			);

			if (OnAfterInitEvent != null)
				OnAfterInitEvent(this);
		} // Init

		public Exception Thrown { get; private set; }
	}  // class ExceptionThrown
} // namespace
