namespace Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions {
	internal interface IDecisionCheckAgent {
		bool WasMismatch { get; }
		bool WasException { get; }
		bool AffirmativeDecisionMade { get; }
		void MakeAndVerifyDecision();
	} // interface IDecisionCheckAgent
} // namespace
