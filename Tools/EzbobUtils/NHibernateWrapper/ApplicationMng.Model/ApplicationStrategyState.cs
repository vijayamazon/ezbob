using System;
namespace ApplicationMng.Model
{
	public enum ApplicationStrategyState
	{
		Halted = -1,
		NeedProcessBySE,
		NeedProcessByNode,
		StrategyFinishedWithoutErrors,
		StrategyFinishedWithErrors,
		Suspended,
		Error,
		SecurityViolation,
		SuspendedWithHandledException
	}
}
