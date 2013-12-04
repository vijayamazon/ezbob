namespace EzBob.Backend.Strategies.ScoreCalculation
{
	public enum Gender
	{
		M,
		F
	} // enum Gender

	public enum MaritalStatus
	{
		Married,
		Single,
		Divorced,
		Widower,
		Other
	} // enum MaritalStatus

	public enum Medal
	{
		NoMedal = 0,
		Silver = 6,
		Gold = 8,
		Platinum = 10,
		Diamond = 12,
	} // enum Medal

	public enum Parameter
	{
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
	}
}
