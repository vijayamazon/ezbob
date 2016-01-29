namespace Ezbob.Backend.Strategies.MainStrategyNew.Steps {
	using System;
	using AutomationCalculator.AutoDecision.AutoRejection;
	using AutomationCalculator.Common;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;
	using RejectAgent = Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Reject.LogicalGlue.Agent;

	internal class Reject : ADecisionBaseStep {
		public Reject(
			string outerContextDescription,
			bool avoidAutomaticDecision,
			bool enabled,
			int customerID,
			long cashRequestID,
			long nlCashRequestID,
			string tag,
			int companyID,
			int monthlyPayment,
			bool customerIsAlibaba
		) : base(
			outerContextDescription,
			avoidAutomaticDecision,
			enabled,
			customerID,
			cashRequestID,
			nlCashRequestID,
			tag
		) {
			this.companyID = companyID;
			this.monthlyPayment = monthlyPayment;
			this.customerIsAlibaba = customerIsAlibaba;
		} // constructor

		[StepOutput]
		public AutoRejectionOutput AutoRejectionOutput {
			get { return this.rejectAgent == null ? null : this.rejectAgent.Output; }
		} // AutoRejectionOutput

		protected override string ProcessName { get { return "auto rejection"; } }

		protected override string DecisionName { get { return "rejected"; } }

		protected override IDecisionCheckAgent CreateDecisionCheckAgent() {
			this.rejectAgent = new RejectAgent(
				new AutoRejectionArguments(
					CustomerID,
					this.companyID,
					this.monthlyPayment,
					CashRequestID,
					NLCashRequestID,
					Tag,
					DateTime.UtcNow,
					DB,
					Log
				)
			);

			return this.rejectAgent;
		} // CreateDecisionCheckAgent

		protected override StepResults Run() {
			StepResults result = base.Run();

			if (this.customerIsAlibaba && (result == StepResults.Affirmative)) {
				this.outcome = string.Format("'not {0}'", DecisionName);
				result = StepResults.Negative;

				Log.Msg(
					"Process of {1} for {0} decision changed to {2} because this is an Alibaba customer.",
					OuterContextDescription,
					ProcessName,
					Outcome
				);
			} // if

			return result;
		} // Run

		private RejectAgent rejectAgent;
		private readonly int companyID;
		private readonly int monthlyPayment;
		private readonly bool customerIsAlibaba;
	} // class Reject
} // namespace
