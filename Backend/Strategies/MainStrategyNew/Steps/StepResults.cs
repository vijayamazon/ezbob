﻿namespace Ezbob.Backend.Strategies.MainStrategyNew.Steps {
	internal enum StepResults {
		StopMachine,
		Affirmative,
		Negative,
		Failed,
		Success,
		Requested,
		NotRequestedWithAutoRules,
		NotRequestedWithoutAutoRules,
		NotRequested,
		Applied,
		NotApplied,
		Found,
		NotFound,
		NotExecuted,
	} // enum StepResults
} // namespace
