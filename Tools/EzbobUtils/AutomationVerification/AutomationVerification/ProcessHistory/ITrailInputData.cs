namespace AutomationCalculator.ProcessHistory {
	using System;

	public interface ITrailInputData {
		DateTime DataAsOf { get; }
		string Serialize();
	} // interface ITrailInputData
} // namespace AutomationCalculator.ProcessHistory
