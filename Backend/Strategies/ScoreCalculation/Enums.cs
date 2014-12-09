namespace Ezbob.Backend.Strategies.ScoreCalculation {
	public enum MedalMultiplier {
		NoMedal = 0,
		Silver = 6,
		Gold = 8,
		Platinum = 10,
		Diamond = 12,
	} // enum MedalMultiplier

	public enum Parameter {
		ExperianScore,
		MpSeniority,
		MaritalStatus,
		PositiveFeedback,
		Other,
		AnnualTurnover,
		NumOfStores,
		EzbobSeniority,
		EzbobNumOfLoans,
		EzbobNumOfLateRepayments,
		EzbobNumOfEarlyRepayments
	} // enum Parameter
} // namespace
