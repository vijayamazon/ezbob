namespace AutomationCalculator.ProcessHistory.Common {
	using System;
	using System.Collections.Generic;
	using Newtonsoft.Json;

	public class ExceptionThrown : ATrace {
		public ExceptionThrown(int nCustomerID, DecisionStatus nDecisionStatus) : base(nCustomerID, nDecisionStatus) {
		} // constructor

		public void Init(Exception oException) {
			Thrown = oException;

			Comment = string.Format(
				"customer {0}: exception of type {1} thrown with message '{2}'",
				CustomerID, Thrown.GetType(), Thrown.Message
			);
		} // Init

		public Exception Thrown { get; private set; }

		public override string GetInitArgs() {
			return JsonConvert.SerializeObject(new List<string> { Thrown.Message, Thrown.GetType().FullName });
		} // GetInitArgs
	}  // class ExceptionThrown
} // namespace
