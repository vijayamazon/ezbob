namespace AutomationCalculator
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
		Widowed,
		LivingTogether,
		Separated,
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

	public enum Decision
	{
		Approve,
		Reject,
		Manual
	}

	public enum DecisionType
	{
		AutoApprove,
		AutoReApprove,
		AutoReject,
		AutoReReject,
		IsOffline
	}

	public enum TimePeriodEnum
	{
		Month = 1,
		Month3 = 2,
		Month6 = 3,
		Year = 4,
		Month15 = 5,
		Month18 = 6,
		Year2 = 7,
		Lifetime = 8,
		Zero = 9
	}
}
