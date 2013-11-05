using System;
using System.Collections.Generic;

namespace EzbobScoringModelCalculator
{
	using System.ComponentModel;

	public class EzbobScoringModel
	{
		public Dictionary<AcParameters, double /*value*/> AcParametersDict { get; set; }
		public List<AcParameters> AcDescriptors { get; set; }
		public Dictionary<AcParameters, double /*value*/> ResultWeights { get; set; }
		public Dictionary<AcParameters, double /*value*/> ResultMaxPossiblePoints { get; set; }
		public string Medal { get; set; }
		public double ScorePoints { get; set; }
		public double ScoreResult { get; set; }
		public DateTime ScoreDate { get; set; }
	}

	public enum AcParameters
	{
		[Description("Experian Score")]
		ExperianScore,
		[Description("Marketplace Seniority")]
		MarketplaceSeniority,
		[Description("Marital Status")]
		MaritalStatus,
		[Description("Positive Feedback Count")]
		PositiveFeedbackCount,
		[Description("Gender / Other ?")]
		Gender,
		[Description("Annual Turnover")]
		AnnualTurnover,
		[Description("Number Of Stores")]
		NumberOfStores,
		[Description("Ezbob Seniority")]
		EzbobSeniority,
		[Description("Ezbob Number Of Loans")]
		EzbobNumberOfLoans,
		[Description("Ezbob Previous Late Payments")]
		EzbobPreviousLatePayments,
		[Description("Ezbob Previous Early Payments")]
		EzbobPreviousEarlyPayments
	}
}
