namespace Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions {
	internal interface IDecisionCheckFlow {
		bool WasMismatch { get; }
		bool WasException { get; }
		bool AffirmativeDecisionMade { get; }
		void MakeAndVerifyDecision();
	} // interface IDecisionCheckFlow
} // namespace
