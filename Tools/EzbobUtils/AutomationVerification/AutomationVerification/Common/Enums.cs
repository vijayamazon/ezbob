namespace AutomationCalculator.Common {
	using System.ComponentModel;
	using System.Runtime.Serialization;

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

	public static class MedalTypeExtension {
		public static bool IsOnline(this MedalType variable) {
			return variable.In(
				MedalType.OnlineLimited,
				MedalType.OnlineNonLimitedNoBusinessScore,
				MedalType.OnlineNonLimitedWithBusinessScore
			);
		} // class MedalTypeExtension

		public static bool In(this MedalType variable, params MedalType[] args) {
			foreach (MedalType mt in args)
				if (variable == mt)
					return true;

			return false;
		} // class MedalTypeExtension
	} // class MedalTypeExtension

	public enum TurnoverType {
		HMRC,
		Bank,
		Online,
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

	public enum StringDifference {
		NotEqual = -1,
		SoundNotEqual = 0,
		SoundNotSimilar = 1,
		SoundSimilar = 2,
		SoundVerySimilar = 3,
		SoundEqual = 4,
		Equal = 5,
	} // enum StringDifference

	public enum LGEtlCode {
		Success = 1,
		HardReject = 2,
	} // enum LGEtlCode

	public enum LGTimeoutSources {
		Equifax = 1,
		LogicalGlueInferenceApi = 2,
	} // enum LGTimeoutSources
} // namespace
