﻿namespace Ezbob.Backend.Strategies.MainStrategy.Steps {
	internal enum StepResults {
		NormalShutdown,
		AbnormalShutdown,
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
