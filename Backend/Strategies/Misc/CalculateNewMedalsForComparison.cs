namespace EzBob.Backend.Strategies.Misc 
{
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;
	using LimitedMedalCalculation;
	using ScoreCalculation;

	public class CalculateNewMedalsForComparison : AStrategy
	{
		private readonly NewMedalScoreCalculator1 calculator1;
		private readonly NewMedalScoreCalculator2 calculator2;

		public CalculateNewMedalsForComparison(AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog)
		{
			calculator1 = new NewMedalScoreCalculator1(oDb, oLog);
			calculator2 = new NewMedalScoreCalculator2(oDb, oLog);
		}

		public override string Name {
			get { return "CalculateNewMedalsForComparison"; }
		}

		public override void Execute()
		{
			int customerId = 0;

			DB.ForEachRowSafe((sr, bRowsetStart) => {
				try
				{
					customerId = sr["CustomerId"];
					ScoreResult result1 = calculator1.CalculateMedalScore(customerId);
					ScoreResult result2 = calculator2.CalculateMedalScore(customerId);

					DB.ExecuteNonQuery("StoreNewMedalForComparison1", CommandSpecies.StoredProcedure,
									   new QueryParameter("CustomerId", customerId),
									   new QueryParameter("BusinessScore", result1.BusinessScore),
									   new QueryParameter("BusinessScoreWeight", result1.BusinessScoreWeight),
									   new QueryParameter("BusinessScoreGrade", result1.BusinessScoreGrade),
									   new QueryParameter("BusinessScoreScore", result1.BusinessScoreScore),
									   new QueryParameter("FreeCashFlow", result1.FreeCashFlow),
									   new QueryParameter("FreeCashFlowWeight", result1.FreeCashFlowWeight),
									   new QueryParameter("FreeCashFlowGrade", result1.FreeCashFlowGrade),
									   new QueryParameter("FreeCashFlowScore", result1.FreeCashFlowScore),
									   new QueryParameter("AnnualTurnover", result1.AnnualTurnover),
									   new QueryParameter("AnnualTurnoverWeight", result1.AnnualTurnoverWeight),
									   new QueryParameter("AnnualTurnoverGrade", result1.AnnualTurnoverGrade),
									   new QueryParameter("AnnualTurnoverScore", result1.AnnualTurnoverScore),
									   new QueryParameter("TangibleEquity", result1.TangibleEquity),
									   new QueryParameter("TangibleEquityWeight", result1.TangibleEquityWeight),
									   new QueryParameter("TangibleEquityGrade", result1.TangibleEquityGrade),
									   new QueryParameter("TangibleEquityScore", result1.TangibleEquityScore),
									   new QueryParameter("BusinessSeniority", result1.BusinessSeniority.HasValue && result1.BusinessSeniority.Value.Year > 1800 ? result1.BusinessSeniority : null),
									   new QueryParameter("BusinessSeniorityWeight", result1.BusinessSeniorityWeight),
									   new QueryParameter("BusinessSeniorityGrade", result1.BusinessSeniorityGrade),
									   new QueryParameter("BusinessSeniorityScore", result1.BusinessSeniorityScore),
									   new QueryParameter("ConsumerScore", result1.ConsumerScore),
									   new QueryParameter("ConsumerScoreWeight", result1.ConsumerScoreWeight),
									   new QueryParameter("ConsumerScoreGrade", result1.ConsumerScoreGrade),
									   new QueryParameter("ConsumerScoreScore", result1.ConsumerScoreScore),
									   new QueryParameter("NetWorth", result1.NetWorth),
									   new QueryParameter("NetWorthWeight", result1.NetWorthWeight),
									   new QueryParameter("NetWorthGrade", result1.NetWorthGrade),
									   new QueryParameter("NetWorthScore", result1.NetWorthScore),
									   new QueryParameter("MaritalStatus", result1.MaritalStatus.ToString()),
									   new QueryParameter("MaritalStatusWeight", result1.MaritalStatusWeight),
									   new QueryParameter("MaritalStatusGrade", result1.MaritalStatusGrade),
									   new QueryParameter("MaritalStatusScore", result1.MaritalStatusScore),
									   new QueryParameter("EzbobSeniority", result1.EzbobSeniority.HasValue && result1.EzbobSeniority.Value.Year > 1800 ? result1.EzbobSeniority : null),
									   new QueryParameter("EzbobSeniorityWeight", result1.EzbobSeniorityWeight),
									   new QueryParameter("EzbobSeniorityGrade", result1.EzbobSeniorityGrade),
									   new QueryParameter("EzbobSeniorityScore", result1.EzbobSeniorityScore),
									   new QueryParameter("NumOfLoans", result1.NumOfLoans),
									   new QueryParameter("NumOfLoansWeight", result1.NumOfLoansWeight),
									   new QueryParameter("NumOfLoansGrade", result1.NumOfLoansGrade),
									   new QueryParameter("NumOfLoansScore", result1.NumOfLoansScore),
									   new QueryParameter("NumOfLateRepayments", result1.NumOfLateRepayments),
									   new QueryParameter("NumOfLateRepaymentsWeight", result1.NumOfLateRepaymentsWeight),
									   new QueryParameter("NumOfLateRepaymentsGrade", result1.NumOfLateRepaymentsGrade),
									   new QueryParameter("NumOfLateRepaymentsScore", result1.NumOfLateRepaymentsScore),
									   new QueryParameter("NumOfEarlyRepayments", result1.NumOfEarlyRepayments),
									   new QueryParameter("NumOfEarlyRepaymentsWeight", result1.NumOfEarlyRepaymentsWeight),
									   new QueryParameter("NumOfEarlyRepaymentsGrade", result1.NumOfEarlyRepaymentsGrade),
									   new QueryParameter("NumOfEarlyRepaymentsScore", result1.NumOfEarlyRepaymentsScore),
									   new QueryParameter("TotalScore", result1.TotalScore),
									   new QueryParameter("TotalScoreNormalized", result1.TotalScoreNormalized),
									   new QueryParameter("Medal", result1.Medal.ToString()),
									   new QueryParameter("Error", result1.Error));

					DB.ExecuteNonQuery("StoreNewMedalForComparison2", CommandSpecies.StoredProcedure,
									   new QueryParameter("CustomerId", customerId),
									   new QueryParameter("BusinessScore", result2.BusinessScore),
									   new QueryParameter("BusinessScoreWeight", result2.BusinessScoreWeight),
									   new QueryParameter("BusinessScoreGrade", result2.BusinessScoreGrade),
									   new QueryParameter("BusinessScoreScore", result2.BusinessScoreScore),
									   new QueryParameter("FreeCashFlow", result2.FreeCashFlow),
									   new QueryParameter("FreeCashFlowWeight", result2.FreeCashFlowWeight),
									   new QueryParameter("FreeCashFlowGrade", result2.FreeCashFlowGrade),
									   new QueryParameter("FreeCashFlowScore", result2.FreeCashFlowScore),
									   new QueryParameter("AnnualTurnover", result2.AnnualTurnover),
									   new QueryParameter("AnnualTurnoverWeight", result2.AnnualTurnoverWeight),
									   new QueryParameter("AnnualTurnoverGrade", result2.AnnualTurnoverGrade),
									   new QueryParameter("AnnualTurnoverScore", result2.AnnualTurnoverScore),
									   new QueryParameter("TangibleEquity", result2.TangibleEquity),
									   new QueryParameter("TangibleEquityWeight", result2.TangibleEquityWeight),
									   new QueryParameter("TangibleEquityGrade", result2.TangibleEquityGrade),
									   new QueryParameter("TangibleEquityScore", result2.TangibleEquityScore),
									   new QueryParameter("BusinessSeniority", result2.BusinessSeniority.HasValue && result2.BusinessSeniority.Value.Year > 1800 ? result2.BusinessSeniority : null),
									   new QueryParameter("BusinessSeniorityWeight", result2.BusinessSeniorityWeight),
									   new QueryParameter("BusinessSeniorityGrade", result2.BusinessSeniorityGrade),
									   new QueryParameter("BusinessSeniorityScore", result2.BusinessSeniorityScore),
									   new QueryParameter("ConsumerScore", result2.ConsumerScore),
									   new QueryParameter("ConsumerScoreWeight", result2.ConsumerScoreWeight),
									   new QueryParameter("ConsumerScoreGrade", result2.ConsumerScoreGrade),
									   new QueryParameter("ConsumerScoreScore", result2.ConsumerScoreScore),
									   new QueryParameter("NetWorth", result2.NetWorth),
									   new QueryParameter("NetWorthWeight", result2.NetWorthWeight),
									   new QueryParameter("NetWorthGrade", result2.NetWorthGrade),
									   new QueryParameter("NetWorthScore", result2.NetWorthScore),
									   new QueryParameter("MaritalStatus", result2.MaritalStatus.ToString()),
									   new QueryParameter("MaritalStatusWeight", result2.MaritalStatusWeight),
									   new QueryParameter("MaritalStatusGrade", result2.MaritalStatusGrade),
									   new QueryParameter("MaritalStatusScore", result2.MaritalStatusScore),
									   new QueryParameter("EzbobSeniority", result2.EzbobSeniority.HasValue && result2.EzbobSeniority.Value.Year > 1800 ? result2.EzbobSeniority : null),
									   new QueryParameter("EzbobSeniorityWeight", result2.EzbobSeniorityWeight),
									   new QueryParameter("EzbobSeniorityGrade", result2.EzbobSeniorityGrade),
									   new QueryParameter("EzbobSeniorityScore", result2.EzbobSeniorityScore),
									   new QueryParameter("NumOfLoans", result2.NumOfLoans),
									   new QueryParameter("NumOfLoansWeight", result2.NumOfLoansWeight),
									   new QueryParameter("NumOfLoansGrade", result2.NumOfLoansGrade),
									   new QueryParameter("NumOfLoansScore", result2.NumOfLoansScore),
									   new QueryParameter("NumOfLateRepayments", result2.NumOfLateRepayments),
									   new QueryParameter("NumOfLateRepaymentsWeight", result2.NumOfLateRepaymentsWeight),
									   new QueryParameter("NumOfLateRepaymentsGrade", result2.NumOfLateRepaymentsGrade),
									   new QueryParameter("NumOfLateRepaymentsScore", result2.NumOfLateRepaymentsScore),
									   new QueryParameter("NumOfEarlyRepayments", result2.NumOfEarlyRepayments),
									   new QueryParameter("NumOfEarlyRepaymentsWeight", result2.NumOfEarlyRepaymentsWeight),
									   new QueryParameter("NumOfEarlyRepaymentsGrade", result2.NumOfEarlyRepaymentsGrade),
									   new QueryParameter("NumOfEarlyRepaymentsScore", result2.NumOfEarlyRepaymentsScore),
									   new QueryParameter("TotalScore", result2.TotalScore),
									   new QueryParameter("TotalScoreNormalized", result2.TotalScoreNormalized),
									   new QueryParameter("Medal", result2.Medal.ToString()),
									   new QueryParameter("Error", result2.Error));
				}
				catch (Exception ex)
				{
					Log.Error("Medal calculation for customer:{0} failed with exception:{1}", customerId, ex);
				}

				return ActionResult.Continue;
			}, 
				"GetCustomersForNewMedalsComparison",
				CommandSpecies.StoredProcedure
				);

		}
	}
}
