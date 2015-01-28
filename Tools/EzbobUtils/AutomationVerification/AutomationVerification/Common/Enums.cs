namespace AutomationCalculator.Common {
	using System.ComponentModel;

	public enum Decision {
		Approve,
		Reject,
		Manual
	}

	public enum DecisionType {
		AutoApprove,
		AutoReApprove,
		AutoReject,
		AutoReReject,
		IsOffline
	}

	public enum Gender {
		M,
		F
	} // enum Gender

	public enum LoanSource {
		Standard,
		EU
	}

	public enum LoanType {
		[Description("Standard")]
		StandardLoanType,

		[Description("Half Way")]
		HalfWayLoanType,

		[Description("Alibaba")]
		AlibabaLoanType
	}

	public enum MaritalStatus {
		Married,
		Single,
		Divorced,
		Widowed,
		LivingTogether,
		Separated,
		Other
	} // enum MaritalStatus

	public enum Medal {
		NoClassification,
		Silver,
		Gold,
		Platinum,
		Diamond,
	} // enum Medal

	public static class MedalExt {
		public static string Stringify(this Medal medal, int padLength = -1) {
			string output = medal == Medal.NoClassification ? "NoClass" : medal.ToString();
			return padLength > 0 ? output.PadRight(padLength) : output;
		} // Stringify
	} // MedalExt

	public enum MedalType {
		NoMedal,
		Limited,
		NonLimited,
		OnlineLimited,
		OnlineNonLimitedNoBusinessScore,
		OnlineNonLimitedWithBusinessScore,
		SoleTrader,
	}

	public enum TurnoverType {
		HMRC,
		Bank,
		Online,
	}

	public enum OfferCalculationType {
		Seek,
		Boundaries
	}

	public enum Parameter {
		BusinessScore,
		TangibleEquity,
		BusinessSeniority,
		ConsumerScore,
		EzbobSeniority,
		MaritalStatus,
		NumOfOnTimeLoans,
		NumOfLatePayments,
		NumOfEarlyPayments,
		AnnualTurnover,
		FreeCashFlow,
		NetWorth,

		/*online only*/
		NumOfStores,
		PositiveFeedbacks,
	}

	public enum TimePeriodEnum {
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
