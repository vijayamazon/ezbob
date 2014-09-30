namespace EzBob.Backend.Strategies.Misc 
{
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;
	using ScoreCalculation;

	public class CalculateNewMedals : AStrategy
	{
		private readonly NewMedalScoreCalculator calculator;
		public CalculateNewMedals(AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog)
		{
			calculator = new NewMedalScoreCalculator(oDb, oLog);
		}

		public override string Name {
			get { return "CalculateNewMedals"; }
		}

		public override void Execute()
		{
			int customerId = 0;

			DB.ForEachRowSafe((sr, bRowsetStart) => {
				try
				{
					customerId = sr["CustomerId"];
					ScoreResult result = calculator.CalculateMedalScore(customerId);
					DB.ExecuteNonQuery("StoreNewMedal", CommandSpecies.StoredProcedure,
									   new QueryParameter("CustomerId", customerId),
									   new QueryParameter("BusinessScore", result.BusinessScore),
									   new QueryParameter("BusinessScoreWeight", result.BusinessScoreWeight),
									   new QueryParameter("BusinessScoreGrade", result.BusinessScoreGrade),
									   new QueryParameter("BusinessScoreScore", result.BusinessScoreScore),
									   new QueryParameter("FreeCashFlow", result.FreeCashFlow),
									   new QueryParameter("FreeCashFlowWeight", result.FreeCashFlowWeight),
									   new QueryParameter("FreeCashFlowGrade", result.FreeCashFlowGrade),
									   new QueryParameter("FreeCashFlowScore", result.FreeCashFlowScore),
									   new QueryParameter("AnnualTurnover", result.AnnualTurnover),
									   new QueryParameter("AnnualTurnoverWeight", result.AnnualTurnoverWeight),
									   new QueryParameter("AnnualTurnoverGrade", result.AnnualTurnoverGrade),
									   new QueryParameter("AnnualTurnoverScore", result.AnnualTurnoverScore),
									   new QueryParameter("TangibleEquity", result.TangibleEquity),
									   new QueryParameter("TangibleEquityWeight", result.TangibleEquityWeight),
									   new QueryParameter("TangibleEquityGrade", result.TangibleEquityGrade),
									   new QueryParameter("TangibleEquityScore", result.TangibleEquityScore),
									   new QueryParameter("BusinessSeniority", result.BusinessSeniority.HasValue && result.BusinessSeniority.Value.Year > 1800 ? result.BusinessSeniority : null),
									   new QueryParameter("BusinessSeniorityWeight", result.BusinessSeniorityWeight),
									   new QueryParameter("BusinessSeniorityGrade", result.BusinessSeniorityGrade),
									   new QueryParameter("BusinessSeniorityScore", result.BusinessSeniorityScore),
									   new QueryParameter("ConsumerScore", result.ConsumerScore),
									   new QueryParameter("ConsumerScoreWeight", result.ConsumerScoreWeight),
									   new QueryParameter("ConsumerScoreGrade", result.ConsumerScoreGrade),
									   new QueryParameter("ConsumerScoreScore", result.ConsumerScoreScore),
									   new QueryParameter("NetWorth", result.NetWorth),
									   new QueryParameter("NetWorthWeight", result.NetWorthWeight),
									   new QueryParameter("NetWorthGrade", result.NetWorthGrade),
									   new QueryParameter("NetWorthScore", result.NetWorthScore),
									   new QueryParameter("MaritalStatus", result.MaritalStatus.ToString()),
									   new QueryParameter("MaritalStatusWeight", result.MaritalStatusWeight),
									   new QueryParameter("MaritalStatusGrade", result.MaritalStatusGrade),
									   new QueryParameter("MaritalStatusScore", result.MaritalStatusScore),
									   new QueryParameter("EzbobSeniority", result.EzbobSeniority),
									   new QueryParameter("EzbobSeniorityWeight", result.EzbobSeniorityWeight),
									   new QueryParameter("EzbobSeniorityGrade", result.EzbobSeniorityGrade),
									   new QueryParameter("EzbobSeniorityScore", result.EzbobSeniorityScore),
									   new QueryParameter("NumOfLoans", result.NumOfLoans),
									   new QueryParameter("NumOfLoansWeight", result.NumOfLoansWeight),
									   new QueryParameter("NumOfLoansGrade", result.NumOfLoansGrade),
									   new QueryParameter("NumOfLoansScore", result.NumOfLoansScore),
									   new QueryParameter("NumOfLateRepayments", result.NumOfLateRepayments),
									   new QueryParameter("NumOfLateRepaymentsWeight", result.NumOfLateRepaymentsWeight),
									   new QueryParameter("NumOfLateRepaymentsGrade", result.NumOfLateRepaymentsGrade),
									   new QueryParameter("NumOfLateRepaymentsScore", result.NumOfLateRepaymentsScore),
									   new QueryParameter("NumOfEarlyRepayments", result.NumOfEarlyRepayments),
									   new QueryParameter("NumOfEarlyRepaymentsWeight", result.NumOfEarlyRepaymentsWeight),
									   new QueryParameter("NumOfEarlyRepaymentsGrade", result.NumOfEarlyRepaymentsGrade),
									   new QueryParameter("NumOfEarlyRepaymentsScore", result.NumOfEarlyRepaymentsScore),
									   new QueryParameter("TotalScore", result.TotalScore),
									   new QueryParameter("TotalScoreNormalized", result.TotalScoreNormalized),
									   new QueryParameter("Medal", result.Medal.ToString()));
				}
				catch (Exception ex)
				{
					Log.Error("Medal calculation for customer:{0} failed with exception:{1}", customerId, ex);
				}

				return ActionResult.Continue;
			}, 
				"GetAllConsumersForNewMedals",
				CommandSpecies.StoredProcedure
			);
		}
	}
}
