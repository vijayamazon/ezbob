namespace Ezbob.Backend.Strategies.MainStrategyNew.Steps {
	internal enum StepResults {
		StopMachine,

		Completed,

		Yes,
		No,

		Affirmative,
		Negative,
		Failed,

		Requested,
		NotRequestedWithAutoRules,
		NotRequestedWithoutAutoRules,
		NotRequested,

		Applied,
		NotApplied,
	} // enum StepResults
} // namespace
