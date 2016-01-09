namespace Ezbob.Backend.Strategies.AutoDecisionAutomation {
	using AutomationCalculator.ProcessHistory.Trails;

	interface ICreateOfferInputData {
		ApprovalTrail Trail { get; }
		bool LogicalGlueFlowFollowed { get; }
	} // interface ICreateOfferInputData
} // namespace
