namespace AutomationCalculator.ProcessHistory.AutoApproval {
	using System.Collections.Generic;
	using Newtonsoft.Json;

	public class AmlCheck : ATrace {
		public AmlCheck(int nCustomerID, DecisionStatus nDecisionStatus) : base(nCustomerID, nDecisionStatus) {
		} // constructor

		public string AmlResult { get; private set; }

		public void Init(string sAmlResult) {
			AmlResult = sAmlResult;

			Comment = string.Format("customer {0} AML result is '{1}'", CustomerID, AmlResult);
		} // Init

		public override string GetInitArgs() {
			return JsonConvert.SerializeObject(new List<string> { AmlResult });
		} // GetInitArgs
	} // class AmlCheck
} // namespace